using AppCommon;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using File = System.IO.File;
namespace MainApp
{
	public partial class MainClient
	{
		/// <summary>
		/// 서버로 부터 받는 데이터를 처리하는 함수
		/// </summary>
		/// <param name="result"></param>
		private void ReceiveCallback(IAsyncResult result)
		{
			try
			{
				//  서버로(BeginReceive)부터 데이터 수신
				var data = result.AsyncState as CommonData;

				// 연결이 끊어진경우 데이터 수신 함수 종료
				if (data.Client.Socket == null) return;

				// 서버로 부터 데이터 수신 완료 처리
				var length = data.Client.Socket.EndReceive(result);

				var fileNameLen = 0;

				var fileName = string.Empty;

				var fileSizeLen = 0;

				// 메타데이터 타입 가져오기
				var dataType = BitConverter.ToInt32(data.Data, 0);

				if (dataType == (int)DataType.TEXT)
				{
					// 메타데이터 빼고 텍스트 추출
					var message = Encoding.UTF8.GetString(data.Data, 4, length - 4);

					// 전송 로그 업데이트
					UpdateLog($"수신<<{message}");
				}
				else if (dataType == (int)DataType.File)
				{
					// 파일이름 데이터 가져오기(type(File) 4 byte, 파일이름데이터(4 byte), 파일이름, 파일데이터, 파일용량)
					fileNameLen = BitConverter.ToInt32(data.Data, 4);
					// 파일 명 추출
					fileName = Encoding.UTF8.GetString(data.Data, 8, fileNameLen);
					// 파일 용량 추출
					fileSizeLen = BitConverter.ToInt32(data.Data, 8 + fileNameLen);

					// 전송 로그 업데이트
					UpdateLog($"서버로 부터 파일을 수신했습니다");
					UpdateLog($"파일명 : {fileName} 파일 크기 :{CalculateFileSize(fileSizeLen)}");
				}
				else if (dataType == (int)DataType.CallBackFileAccept)
				{
					// 메타데이터 빼고 텍스트 추출
					var message = Encoding.UTF8.GetString(data.Data, 4, length - 4);

					SetFileSendAction(message, false);
				}
				else if (dataType == (int)DataType.CallBackFileSended)
				{
					// 메타데이터 빼고 텍스트 추출
					var message = Encoding.UTF8.GetString(data.Data, 4, length - 4);

					SetFileSendAction(message, true);
				}
				else if (dataType == (int)DataType.Ping)
				{
					// 핑 메타데이터 생성
					var ping = BitConverter.GetBytes((int)DataType.Ping);

					// 핑 전송
					Client?.Socket.Send(ping);
				}

				if (dataType == (int)DataType.File)
				{
					// 시스템 메모리 체킹(메모리 체킹이 불가능 한 경우 체킹하지 않고 진행)
					if (!CheckSystemMemory(fileSizeLen, out string errorMsg))
					{
						UpdateLog($"{errorMsg} 파일 수신이 취소되었습니다");
						// 다시 서버로 부터 데이터 수신 대기
						data.Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);
						return;
					}

					UpdateLog("파일 데이터 처리중입니다. 데이터 처리중에 프로그램을 닫지 말아 주십시오.");

					Invoke(new Action(() =>
					{
						btnDisconnect.Enabled = false;
					}));

					// 데이터 손실을 막기위해 네트워크 스트림 이용
					using (NetworkStream ns = new NetworkStream(data.Client.Socket))
					{
						// 파일 버퍼 작성
						byte[] fileBuffer = new byte[fileSizeLen];

						// 데이터 손실이 없는지 기록
						var receivedBytes = 0;

						// 첫 소켓통신시 받은 파일 데이터 버퍼에 복사(파일종류 + 파일이름 + 파일용량[12byte] + 파일이름바이너리데이터 제외)
						Array.Copy(data.Data, 12 + fileNameLen, fileBuffer, 0, length - (12 + fileNameLen));
						receivedBytes += length - (12 + fileNameLen);

						// 진행률을 확인하기위해 프로그레스바에 파일 크기 할당
						pbFileProgress.Maximum = fileSizeLen;
						// 나머지 파일 데이터 가져오기
						while (receivedBytes < fileSizeLen)
						{
							if (ns.DataAvailable)
							{
								receivedBytes += data.Client.Socket.Receive(fileBuffer, receivedBytes, (fileSizeLen - receivedBytes), SocketFlags.None);
								Invoke(new Action(() =>
								{
									pbFileProgress.Value = receivedBytes;
								}));
							}
							Thread.Sleep(1);
						}

						// 다운로드 버퍼 이미 있으면 비움
						DisposeGlobalDownloadBuffer();

						// 전역 변수에 파일 데이터 저장
						DownloadFileBuffer = fileBuffer;
						DownloadFileName = fileName;
						DownloadFileSizeLen = fileSizeLen;

						UpdateLog("서버로부터 파일을 수신완료 했습니다");

						Invoke(new Action(() =>
						{
							btnDisconnect.Enabled = true;
							pbFileProgress.Value = 0;
							btnFileDownload.Enabled = true;
						}));
					}
				}

				// 다시 서버로 부터 데이터 수신 대기
				data.Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);
			}
			catch (Exception)
			{
				UpdateLog("서버로 부터 접속이 끊어졌습니다");
				Invoke(new Action(() =>
				{
					btnDisconnect.Enabled = true;
				}));
				// 메모리 및 설정 초기화
				ResetClient();
			}
		}

		/// <summary>
		/// 파일 송수신시 서버로부터 오는 콜백 처리 함수
		/// </summary>
		/// <param name="message"></param>
		/// <param name="status"></param>
		private void SetFileSendAction(string message, bool status)
		{
			// 전송 로그 업데이트
			UpdateLog($"서버로 부터 메시지 수신<<{message}");

			Invoke(new Action(() =>
			{
				btnFileUpload.Enabled = status;
				btnFileSend.Enabled = false;
				btnSendMessage.Enabled = status;
				btnDisconnect.Enabled = status;
				if (DownloadFileBuffer != null && DownloadFileBuffer.Length > 0 && status == true)
				{
					btnFileDownload.Enabled = true;
				}
				else
				{
					btnFileDownload.Enabled = false;
				}
			}));
		}

		/// <summary>
		/// 파일 업로드 비동기 처리
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private async Task UploadFileAsync(string filePath)
		{
			try
			{
				await Task.Run(() =>
				{
					UpdateLog("파일 업로드 처리 중입니다...");

					Invoke(new Action(() =>
					{
						// 라벨명 초기화
						lbFileName.Text = "없음";
						btnFileUpload.Enabled = false;
					}));

					// 파일 사이즈 얻기
					long fileSize = GetFileLength(filePath);

					// 시스템 메모리 체킹(메모리 체킹이 불가능 한 경우 체킹하지 않고 진행)
					if (!CheckSystemMemory(fileSize, out string errorMsg)) { ShowFileUploadError($"{errorMsg} 업로드가 취소되었습니다"); return; }

					if (fileSize > MAX_UPLOAD_FILE_BUFFER_SIZE) 
					{
						ShowFileUploadError("파일이 시스템에서 제한한 용량보다 큽니다");
						return;
					}
					else if(fileSize == 0)
					{
						ShowFileUploadError("파일이 존재하지 않습니다");
						return;
					}

					// 파일의 바이너리 데이터 읽기 시도
					UploadFileBuffer = File.ReadAllBytes(filePath);

					// 파일 위치 저장
					UploadFilePath = filePath;

					Invoke(new Action(() =>
					{
						// 라벨에 파일명 표시
						lbFileName.Text = TryLabelSubString(Path.GetFileName(UploadFilePath), 15);
						btnFileSend.Enabled = true;
						btnFileUpload.Enabled = true;
					}));

					UpdateLog("파일을 서버에 송신할 준비가 되었습니다.");
				});
			}
			catch (IOException ex)
			{
				ShowFileUploadError(ex.Message);
			}
			catch (OutOfMemoryException)
			{
				ShowFileUploadError("컴퓨터의 메모리가 부족합니다. 파일을 업로드 하지 못했습니다");
			}
			catch (Exception ex)
			{
				ShowFileUploadError(ex.Message);
			}
		}

		/// <summary>
		/// 파일 전송 비동기 처리
		/// </summary>
		/// <returns></returns>
		private async Task SendFileToServerAsync()
		{
			try
			{
				await Task.Run(() =>
				{
					// 송신중 파일 비활성화
					Invoke(new Action(() =>
					{
						btnFileUpload.Enabled = false;
						btnFileSend.Enabled = false;
					}));

					// 파일 경로
					var filePath = UploadFilePath;
					// 파일명
					var fileName = Path.GetFileName(filePath);
					// 파일이름 인코딩
					var fileNameData = Encoding.UTF8.GetBytes(fileName);

					// 전송할 데이터 타입 설정
					var dataType = BitConverter.GetBytes((int)DataType.File);
					// 파일이름 바이너리 데이터
					var fileNameLen = BitConverter.GetBytes(fileNameData.Length);
					// 파일의 바이너리 데이터
					var fileData = UploadFileBuffer;
					// 파일 용량 데이터
					var fileSize = BitConverter.GetBytes(fileData.Length);

					// 데이터 조립 type(File) 4 byte, 파일이름(4 byte), 파일이름데이터, 파일용량(4 byte), 파일데이터)
					var sendData = new byte[dataType.Length + fileNameLen.Length + fileNameData.Length + fileSize.Length + fileData.Length];

					// 배열 앞부분에 메타데이터 삽입
					dataType.CopyTo(sendData, 0);
					// 메타데이터 뒷부분에 파일이름 데이터 삽입
					fileNameLen.CopyTo(sendData, 4);
					// 파일이름 바이너리 데이터 뒷부분에 파일이름 삽입
					fileNameData.CopyTo(sendData, 8);
					// 파일이름 뒷부분에 파일 용량 삽입
					fileSize.CopyTo(sendData, 8 + fileNameData.Length);
					// 파일 용량 뒷부분에 파일 데이터 삽입
					fileData.CopyTo(sendData, 8 + fileNameData.Length + fileSize.Length);

					// 서버에 데이터 송신
					Client?.Socket.Send(sendData);

					// 로그 출력
					UpdateLog($"서버에 파일을 송신했습니다. 파일명 : {fileName}");

					// 메모리 정리
					UploadFileBuffer = null;
					UploadFilePath = string.Empty;
					Invoke(new Action(() =>
					{
						// 라벨명 초기화
						lbFileName.Text = "없음";
					}));
				});
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("파일을 찾을 수 없습니다");
				// 메모리 정리
				UploadFileBuffer = null;
				UploadFilePath = string.Empty;
				Invoke(new Action(() =>
				{
					// 라벨명 초기화
					lbFileName.Text = "없음";
					btnFileUpload.Enabled = true;
				}));
				UpdateLog($"파일을 전송하지 못했습니다");
			}
			catch (OutOfMemoryException)
			{
				MessageBox.Show("컴퓨터의 메모리가 부족합니다. 파일을 전송하지 못했습니다");
				// 메모리 정리
				UploadFileBuffer = null;
				UploadFilePath = string.Empty;
				Invoke(new Action(() =>
				{
					// 라벨명 초기화
					lbFileName.Text = "없음";
					btnFileUpload.Enabled = true;
				}));
				UpdateLog($"파일을 전송하지 못했습니다");
			}
			catch (Exception)
			{
				MessageBox.Show("파일 전송중 에러 발생");
				Invoke(new Action(() =>
				{
					btnFileUpload.Enabled = true;
					btnFileSend.Enabled = true;
				}));
				UpdateLog($"파일을 전송하지 못했습니다");
			}
		}

		/// <summary>
		/// 파일명 최대 길이 자르기
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private string TryLabelSubString(string data, int length)
		{
			if (data.Length > length)
			{
				data = data.Substring(0, length) + "...";
			}
			return data;
		}

		/// <summary>
		/// 포트 번호 및 아이피 정규식 검증
		/// </summary>
		/// <param name="port"></param>
		/// <param name="ip"></param>
		/// <returns></returns>
		private bool CheckPortAndIpNumber(string port, string ip)
		{
			// 공백 체크
			if (string.IsNullOrEmpty(port))
			{
				MessageBox.Show("포트 번호를 입력해주십시오");
				return false;
			}

			// 정규식 체크
			if (!Regex.IsMatch(port, "^[0-9]+$"))
			{
				MessageBox.Show("포트 번호가 올바르지 않습니다");
				return false;
			}

			var portNum = int.Parse(port);

			// 포트 번호의 범위가 유효한지 확인
			if (portNum < IPEndPoint.MinPort || IPEndPoint.MaxPort < portNum)
			{
				MessageBox.Show("포트 범위가 유효하지 않습니다 포트범위는 0~65335 사이여야 합니다");
				return false;
			}

			// 아이피 주소가 유효한지 확인
			if (!IPAddress.TryParse(ip, out IPAddress _))
			{
				MessageBox.Show("유효한 아이피 주소가 아닙니다");
				return false;
			}
			return true;

		}

		/// <summary>
		/// 로그 글자 수 계산
		/// </summary>
		/// <param name="log"></param>
		private void UpdateLog(string log)
		{
			int max_log_length = 35; // 글자 최대 길이 한글 기준 35자
			int max_byte_length = max_log_length * 3; // 한글 기준 3byte

			int byteCount = 0;
			int startIndex = 0;

			for (int i = 0; i < log.Length; i++)
			{
				char c = log[i];
				byteCount += c > 0x7F ? 3 : 1; // 아스키(영문)일경우 1byte 아니면 3byte

				if (byteCount > max_byte_length)
				{
					ShowLog(log.Substring(startIndex, i - startIndex));
					byteCount = 0; // 바이트 계산 초기화
					startIndex = i; // 인덱스 재시작값 지정
				}
			}

			// 나머지 전체 문자열 출력
			if (startIndex < log.Length)
			{
				ShowLog(log.Substring(startIndex));
			}
		}

		/// <summary>
		/// 닉네임에 랜덤번호 부여(프로세스아이디)
		/// </summary>
		/// <returns></returns>
		private string GetPid()
		{
			Process currentProcess = Process.GetCurrentProcess();

			return $"익명{currentProcess.Id.ToString()}";
		}

		/// <summary>
		/// 파일 사이즈 계산
		/// </summary>
		/// <param name="fileLength"></param>
		/// <returns></returns>
		private string CalculateFileSize(int fileLength)
		{
			// 소수점 계산을위해 타입 변환
			double size = fileLength;
			// 용량타입 byte부터 GB까지 표현
			string[] units = { "bytes", "KB", "MB", "GB" };
			// 추출할 배열의 인덱스
			int arraytIndex = 0;

			// 계산실행
			while (size >= 1024 && arraytIndex < units.Length - 1)
			{
				size /= 1024;
				++arraytIndex;
			}
			// 0.## + 용량타입으로 반환
			return string.Format("{0:0.##} {1}", size, units[arraytIndex]);
		}

		/// <summary>
		/// 업로드 버퍼 초기화를 위한 가비지 컬렉터 수행
		/// </summary>
		private void DisposeGlobalUploadBuffer()
		{
			if(this.UploadFileBuffer != null)
			{
				this.UploadFileBuffer = null;
				GC.Collect(0); // 최근에 생성된 객체 소멸
			}
		}

		/// <summary>
		/// 다운로드 버퍼 초기화를 위한 가비지 컬렉터 수행
		/// </summary>
		private void DisposeGlobalDownloadBuffer()
		{
			if (this.DownloadFileBuffer != null)
			{
				this.DownloadFileBuffer = null;
				GC.Collect(0); // 최근에 생성된 객체 소멸
			}
		}
	}
}
