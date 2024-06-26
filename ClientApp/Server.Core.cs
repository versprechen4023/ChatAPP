﻿using AppCommon;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ServerApp
{
	public partial class ServerClient
	{
		/// <summary>
		/// 접속 대기를 받는 비동기 함수
		/// </summary>
		/// <returns></returns>
		private async Task AsyncAcceptWaitFor()
		{
			UpdateLog("서버를 실행합니다");

			// 쓰레드 생성
			await Task.Run(() =>
			{
				while (Server != null && Server.Active)
				{
					try
					{
						// 비동기로 접속 대기를 계속 받음
						// (WaitOne)접속 요청 대기 시간은 무제한(-1) 접속요청 대기 처리를 하지 않으면 연결 시도가 계속 발생해 메모리 릭 발생)
						Server.BeginAcceptTcpClient(AcceptCallback, null).AsyncWaitHandle.WaitOne(-1);
					}
					catch (Exception)
					{
						UpdateLog("서버 실행중에 에러가 발생했습니다");
						break;
					}
				}
			});

			UpdateLog("서버가 종료되었습니다");
		}

		/// <summary>
		/// 클라이언트의 접속을 처리하는 함수
		/// </summary>
		/// <param name="result"></param>
		private void AcceptCallback(IAsyncResult result)
		{
			try
			{
				// 연결을 수락
				var client = Server.EndAcceptTcpClient(result);

				// 접속 로그 업데이트
				UpdateLog($"{client.Client.RemoteEndPoint}에서 접속 했습니다");

				// 클라이언트 리스트에 접속중인 클라이언트 추가
				var clientInfo = new TcpClientExtension(client);

				IPEndPoint ip = client.Client.RemoteEndPoint as IPEndPoint;

				// 해외 아이피 차단설정 시 해외 아이피 확인
				if (BlockForeignIP && !AcceptLocalOnly)
				{
					var errorMsg = "해외 아이피 접속 차단";

					var ipResult = CheckIp(clientInfo, ip, errorMsg, IsKRIP);
					if (!ipResult)
					{
						return;
					}
				}
				// 사설 아이피 체킹
				else if (AcceptLocalOnly)
				{
					var errorMsg = "외부 아이피 접속 차단";

					var ipResult = CheckIp(clientInfo, ip, errorMsg, IsPrivateIP);

					if (!ipResult)
					{
						return;
					}
				}

				SetConnectionClient(client, true);
				ClientList.Add(clientInfo);

				// 접속중인 클라이언트로부터 데이터 수신 대기
				var data = new CommonData(clientInfo);
				// 메서드 인자는 데이터의버퍼, 버퍼배열의 읽기 시작점, 버퍼사이즈, 송수신시의 동작, 수신시 콜백 대리자, 콜백에 전달한 사용자 정의객체가 된다)
				client.Client.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);
			}

			catch (Exception) { }
		}

		/// <summary>
		/// 접속중인 모든 클라이언트한테 메시지 전송 함수
		/// </summary>
		/// <param name="data"></param>
		/// <param name="text"></param>
		private void SendMessageToAllClient(CommonData data, string text)
		{
			// 비동기 실행중 클라이언트 리스트에 접근하므로 쓰레드 세이프 처리를 실행
			lock (ClientList.SyncRoot)
			{
				// 전송자를 제외한 모든 클라이언트에게 데이터 전송
				foreach (var client in ClientList.Where(e => !e.Equals(data.Client)))
				{
					// 데이터 재조립 후 클라이언트에 발송
					var dataType = BitConverter.GetBytes((int)DataType.TEXT);
					var message = Encoding.UTF8.GetBytes(text);
					byte[] sendData = new byte[dataType.Length + message.Length];
					dataType.CopyTo(sendData, 0);
					message.CopyTo(sendData, 4);

					// 메시지 전송
					client.Socket.Send(sendData);

					// 로그 업데이트
					UpdateLog($"{client.RemoteEndPoint}에 메시지 전송>>{text}");
				}
			}
		}

		/// <summary>
		/// 접속중인 모든 클라이언트한테 파일 전송
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fileName"></param>
		/// <param name="filePath"></param>
		private void SendFileDataToAllClient(CommonData data, string fileName, string savefilePath)
		{
			// 비동기 실행중 클라이언트 리스트에 접근하므로 쓰레드 세이프 처리를 실행
			lock (ClientList.SyncRoot)
			{
				// 전송자를 제외한 모든 클라이언트에게 데이터 전송
				foreach (var client in ClientList.Where(e => !e.Equals(data.Client)))
				{
					// 파일이름 인코딩
					var fileNameData = Encoding.UTF8.GetBytes(fileName);

					// 전송할 데이터 타입 설정
					var dataType = BitConverter.GetBytes((int)DataType.File);
					// 파일이름 바이너리 데이터
					var fileNameLen = BitConverter.GetBytes(fileNameData.Length);
					// 파일의 바이너리 데이터
					var fileData = File.ReadAllBytes(savefilePath);
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

					// 클라이언트에 데이터 송신
					client.Socket.Send(sendData);

					// 로그 업데이트
					UpdateLog($"{client.RemoteEndPoint}에 파일 전송>>{fileName}");
				}

				// 파일 전송 완료후 파일 삭제
				if (File.Exists(savefilePath))
				{
					File.Delete(savefilePath);
				}
			}
		}

		/// <summary>
		/// 클라이언트로 파일을 송신 받을 경우 송신 확인 콜백 함수
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fileName"></param>
		private void FileReceiveCallbackToClient(CommonData data, string text)
		{
			// 비동기 실행중 클라이언트 리스트에 접근하므로 쓰레드 세이프 처리를 실행
			lock (ClientList.SyncRoot)
			{
				// 전송자에게 콜백 메시지 전송
				foreach (var client in ClientList.Where(e => e.Equals(data.Client)))
				{
					// 데이터 재조립 후 클라이언트에 발송
					var dataType = BitConverter.GetBytes((int)DataType.CallBackFileAccept);
					var message = Encoding.UTF8.GetBytes(text);
					byte[] sendData = new byte[dataType.Length + message.Length];
					dataType.CopyTo(sendData, 0);
					message.CopyTo(sendData, 4);

					// 메시지 전송
					client.Socket.Send(sendData);
				}
			}
		}

		/// <summary>
		/// 서버에서 파일을 다른 클라이언트에 전송완료 한 경우 콜백 함수
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fileName"></param>
		private void FileSendedCallbackToClient(CommonData data, string fileName)
		{
			// 비동기 실행중 클라이언트 리스트에 접근하므로 쓰레드 세이프 처리를 실행
			lock (ClientList.SyncRoot)
			{
				// 전송자에게 콜백 메시지 전송
				foreach (var client in ClientList.Where(e => e.Equals(data.Client)))
				{
					// 데이터 재조립 후 클라이언트에 발송
					var dataType = BitConverter.GetBytes((int)DataType.CallBackFileSended);
					var message = Encoding.UTF8.GetBytes($"서버에서 {fileName}을 다른 클라이언트에게 송신했습니다");
					byte[] sendData = new byte[dataType.Length + message.Length];
					dataType.CopyTo(sendData, 0);
					message.CopyTo(sendData, 4);

					// 메시지 전송
					client.Socket.Send(sendData);
				}
			}
		}

		/// <summary>
		/// 접속중인 클라이언트로부터 데이터를 처리하는 함수
		/// </summary>
		/// <param name="result"></param>
		private void ReceiveCallback(IAsyncResult result)
		{
			try
			{
				// 클라이언트로(BeginReceive)부터 데이터 수신
				var data = result.AsyncState as CommonData;

				// 데이터 수신 완료 처리
				var length = data.Client.Socket.EndReceive(result);

				var fileNameLen = 0;

				var fileName = string.Empty;

				var fileSizeLen = 0;

				// 클라이언트로 부터 받은 데이터가 없다면 접속 절단으로 판단
				if (length == 0)
				{
					// 로그 표시
					UpdateLog($"{data.Client.RemoteEndPoint}에서 접속이 종료되었습니다");

					// 클라이언트 목록에서 절단 클라이언트를 삭제
					SetConnectionClient(data.Client.Client, false);
					ClientList.Remove(data.Client);

					// 접속중인 클라이언트에게 메시지 송신(절단 클라이언트 제외)
					SendMessageToAllClient(data, $"{data.Client.RemoteEndPoint}에서 접속을 종료했습니다");
					return;
				}

				// 메타데이터 타입 가져오기
				var dataType = BitConverter.ToInt32(data.Data, 0);

				if (dataType == (int)DataType.TEXT)
				{
					// 메타데이터 빼고 텍스트 추출
					var message = Encoding.UTF8.GetString(data.Data, 4, length - 4);

					// 전송 로그 업데이트
					UpdateLog($"{data.Client.RemoteEndPoint}에서 메시지 수신:{message}");

					// 클라이언트들에게 메시지 전달 준비
					SendMessageToAllClient(data, message);
				}
				else if (dataType == (int)DataType.File)
				{
					// 파일이름 데이터 가져오기(type(File) 4 byte, 파일이름데이터(4 byte), 파일이름, 파일데이터, 파일 용량)
					fileNameLen = BitConverter.ToInt32(data.Data, 4);
					// 파일 명 추출
					fileName = Encoding.UTF8.GetString(data.Data, 8, fileNameLen);
					// 파일 용량 추출
					fileSizeLen = BitConverter.ToInt32(data.Data, 8 + fileNameLen);

					// 전송 로그 업데이트
					UpdateLog($"{data.Client.RemoteEndPoint}에서 파일 수신 : 파일명 : {fileName} 파일 크기 : {CalculateFileSize(fileSizeLen)}");

					// 계정의 다운로드 폴더 위치 가져오기
					var pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					var pathDownload = Path.Combine(pathUser, "Downloads");

					// 파일 임시 저장 폴더 저장
					SaveFilePath = Path.Combine(pathDownload, fileName);

					// 파일 중복 방지 있으면 지움
					if (File.Exists(SaveFilePath))
					{
						File.Delete(SaveFilePath);
					}
				}
				else if (dataType == (int)DataType.Ping)
				{
					ConnectClientList.Add(data.Client);
				}

				if (dataType == (int)DataType.File)
				{
					UpdateLog("파일 데이터 처리중...");

					// 파일을 보낸 클라이언트한테 파일 수신 확인 메시지 발송
					FileReceiveCallbackToClient(data, $"서버에서 {fileName}을 수신하여 처리 중 입니다. 파일 처리 동안은 서버 안정을 위해 메시지를 보낼 수 없습니다.");

					// 데이터 손실을 막기위해 네트워크 스트림 이용
					using (NetworkStream ns = new NetworkStream(data.Client.Socket))
					{
						// 파일 버퍼 작성
						byte[] fileBuffer = new byte[fileSizeLen];

						// 데이터 손실이 없는지 기록
						var receivedBytes = 0;

						// 마지막 퍼센트 처리 기록
						var lastPercentageReported = 0;

						// 첫 소켓통신시 받은 파일 데이터 버퍼에 복사(파일종류 + 파일이름 + 파일용량[12byte] + 파일이름바이너리데이터 제외)
						Array.Copy(data.Data, 12 + fileNameLen, fileBuffer, 0, length - (12 + fileNameLen));
						receivedBytes += length - (12 + fileNameLen);

						// 진행률을 확인하기위해 프로그레스바에 파일 크기 할당
						pbFileProgress.Maximum = fileSizeLen;

						while (receivedBytes < fileSizeLen)
						{
							if (ns.DataAvailable)
							{
								receivedBytes += data.Client.Socket.Receive(fileBuffer, receivedBytes, (fileSizeLen - receivedBytes), SocketFlags.None);
								Invoke(new Action(() =>
								{
									pbFileProgress.Value = receivedBytes;
								}));

								// 퍼센트기록 파일이 100mb 이상일 경우 파일 처리 알림 전송
								if (fileSizeLen >= (1024 * 1024) * 100)
								{
									var percentage = (int)((double)receivedBytes / fileSizeLen * 100);
									if (percentage >= lastPercentageReported + 10)
									{
										FileReceiveCallbackToClient(data, $"파일 처리가 {percentage}% 완료되었습니다.");
										lastPercentageReported = percentage;
									}
								}
							}
							Thread.Sleep(1);
						}

						// 이진데이터로 파일 작성
						BinaryWriter bw = new BinaryWriter(File.Open(SaveFilePath, FileMode.Append));
						bw.Write(fileBuffer);
						bw.Close();

						UpdateLog($"수신된 {fileName} 임시저장 완료");

						// 접속중인 클라이언트에게 파일 전송 메시지 송신
						SendMessageToAllClient(data, $"{data.Client.RemoteEndPoint}에서 {fileName}을 보냈습니다");
						// 여기서 함수 호출 필요(고용량 데이터 수신 송신시 분할할 필요도 있을 것임)
						SendFileDataToAllClient(data, fileName, SaveFilePath);
						// 파일을 보낸 클라이언트에게 다른 클라이언트에게 파일을 송신했다는 확인 메시지 발송
						FileSendedCallbackToClient(data, fileName);

						Invoke(new Action(() =>
						{
							pbFileProgress.Value = 0;
						}));
					}

				}

				// 서버가 실행중인경우 계속해서 접속중인 클라이언트로 부터 데이터 수신 대기
				if (Server != null && Server.Active)
				{
					data.Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);
				}
			}

			catch (Exception) { }
		}

		/// <summary>
		/// 비동기로 클라이언트 체크
		/// </summary>
		/// <returns></returns>
		private async Task CheckConnect()
		{
			await Task.Run(async () =>
			{
				if (Server != null && Server.Active)
				{
					try
					{
						// 통신 10분 대기
						await Task.Delay((1000 * 60) * 10);

						lock (ClientList.SyncRoot)
						{
							foreach (var client in ClientList.ToList())
							{
								if (!ConnectClientList.Contains(client))
								{
									CommonData data = new CommonData(client);

									// 로그 표시
									UpdateLog($"{data.Client.RemoteEndPoint}에서 접속이 종료되었습니다");

									// 클라이언트 목록에서 절단 클라이언트를 삭제
									SetConnectionClient(data.Client.Client, false);
									ClientList.Remove(data.Client);

									// 접속중인 클라이언트에게 메시지 송신(절단 클라이언트 제외)
									SendMessageToAllClient(data, $"{data.Client.RemoteEndPoint}에서 접속을 종료했습니다");
								}
							}
							ConnectClientList = null;
						}
					}
					catch (Exception) { }
				}
			});
		}

		/// <summary>
		/// 포트번호 정규식 검증
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		private bool CheckPortNumber(string port)
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

			return true;
		}

		/// <summary>
		/// 로그 글자 수 계산
		/// </summary>
		/// <param name="log"></param>
		private void UpdateLog(string log)
		{
			int max_log_length = 45; // 글자 최대 길이 한글 기준 45자
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
	}
}
