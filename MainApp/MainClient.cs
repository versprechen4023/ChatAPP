using AppCommon;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace MainApp
{
	public partial class MainClient : Form
	{
		/// <summary>
		/// 클라이언트
		/// </summary>
		private TcpClientExtension Client { get; set; }

		/// <summary>
		/// 전송할 데이터의 타입 구분
		/// </summary>
		private enum DataType { TEXT = 1, File, CallBackFileAccept, CallBackFileSended, Ping};

		/// <summary>
		/// 서버에 업로드할 파일 경로
		/// </summary>
		private string UploadFilePath = string.Empty;

		/// <summary>
		/// 서버에 업로드할 파일 버퍼
		/// </summary>
		private byte[] UploadFileBuffer;

		/// <summary>
		/// 서버에서 받은 파일 버퍼
		/// </summary>
		private byte[] DownloadFileBuffer;

		/// <summary>
		/// 서버에서 받은 파일 이름
		/// </summary>
		private string DownloadFileName = string.Empty;

		/// <summary>
		/// 서버에서 받은 파일 용량
		/// </summary>
		private int DownloadFileSizeLen = 0;

		/// <summary>
		/// 내부망(사설 IP)에 연결 여부
		/// </summary>
		private bool ConnectPrivate = false;

		public MainClient()
		{
			InitializeComponent();
		}

		private void MainClient_Load(object sender, EventArgs e)
		{
			textNickName.Text = GetPid();
			SetConnectionStatus(false);
		}

		/// <summary>
		/// 프로그램 종료 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainClient_FormClosed(object sender, FormClosedEventArgs e)
		{
			// 클라이언트가 서버에 접속중인 경우 종료
			if (Client != null)
			{
				Client.Client.Close();
				Client.Client.Dispose();
				Client = null;
			}
		}

		/// <summary>
		/// 서버 접속 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void btnConnect_Click(object sender, EventArgs e)
		{
			// 연결할서버의 포트
			var connectPort = textConnectPort.Text.Trim();
			// 연결할서버의 아이피
			var connectIpAddress = $"{textConnectIp1.Text.Trim()}.{textConnectIp2.Text.Trim()}.{textConnectIp3.Text.Trim()}.{textConnectIp4.Text.Trim()}".Trim();
			try
			{
				// 아아피 포트 정규식 체크
				if (!CheckPortAndIpNumber(connectPort, connectIpAddress)) return;

				UpdateLog("서버에 접속중입니다...");

				Invoke(new Action(() =>
				{
					btnConnect.Enabled = false;
				}));

				// 접속할 서버의 엔드포인트 작성
				//var connectEndPoint = new IPEndPoint(IPAddress.Parse(connectIpAddress), int.Parse(connectPort));

				// IP 설정
				var localIP = ConnectPrivate ? GetLocalIP() : GetPublicIP();

				// 클라이언트의 엔드포인트 작성(포트는 자동할당) 내부 환경일경우 127.0.0.1 or GetLocalIP() 함수 활용
				var localEndPoint = new IPEndPoint(IPAddress.Parse(localIP), 0);

				// 클라이언트 생성
				Client = new TcpClientExtension(localEndPoint);

				// 비동기로 서버에 접속시도
				await Client.Client.ConnectAsync(IPAddress.Parse(connectIpAddress), int.Parse(connectPort));

				// 연결 테스트용 소켓 전송
				byte[] test = new byte[1];
				Client?.Socket.Send(test);

				// 서버 접속 상태를 접속중으로 변경
				SetConnectionStatus(true);

				// 접속 로그 출력
				UpdateLog("서버에 접속 했습니다");

				// 서버로부터 데이터 수신 대기
				var data = new CommonData(Client);
				Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);

				Invoke(new Action(() =>
				{
					btnDisconnect.Enabled = true;
					btnSendMessage.Enabled = true;
					btnFileUpload.Enabled = true;
				}));

			}
			catch (SocketException ex)
			{
				if(ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.ConnectionRefused)
				{
					MessageBox.Show("서버에 접속 할 수 없습니다.\n서버가 오프라인이거나 연결을 거부 했습니다");
					Invoke(new Action(() =>
					{
						btnConnect.Enabled = true;
					}));
				}
				else if(ex.SocketErrorCode == SocketError.AddressNotAvailable)
				{
					MessageBox.Show("그 아이피 주소로는 연결 할 수 없습니다.\n내부망에서 연결 시도를 하는 경우에는 \"내부망연결\"에 체크 해주십시오.");
					Invoke(new Action(() =>
					{
						btnConnect.Enabled = true;
					}));
				}
			}
			catch(Exception)
			{
				MessageBox.Show("서버 접속에 실패했습니다");
				Invoke(new Action(() =>
				{
					btnConnect.Enabled = true;
				}));
			}
		}

		/// <summary>
		/// 서버 접속 종료 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			// 서버와의 접속 종료
			Client.Client.Close();
			Client.Client.Dispose();
			Client = null;

			// 로그 출력
			UpdateLog("서버와 연결이 종료되었습니다");

			// 메모리 및 설정 초기화
			ResetClient();
		}

		/// <summary>
		/// 메시지 전송 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSendMessage_Click(object sender, EventArgs e)
		{
			try
			{
				// 전송할 데이터 타입 설정
				var dataType = BitConverter.GetBytes((int)DataType.TEXT);
				// 전송할 메시지 데이터 설정
				var message = Encoding.UTF8.GetBytes(textNickName.Text + " : " + textMessage.Text);

				// 데이터 조립 type(TEXT) 4 byte + message
				byte[] sendData = new byte[dataType.Length + message.Length];

				dataType.CopyTo(sendData, 0);
				message.CopyTo(sendData, 4);

				// 메시지 송신
				Client?.Socket.Send(sendData);

				// 로그 출력
				UpdateLog($"메시지 송신>>{Encoding.UTF8.GetString(message)}");

				// 메시지 창 초기화
				textMessage.Text = "";
				textMessage.Focus();
			}
			catch (Exception)
			{
				UpdateLog("메시지 전송에 실패했습니다");
			}
		}

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
					UpdateLog($"서버로 부터 파일 수신 : 파일명 : {fileName} 파일 크기 : {CalculateFileSize(fileSizeLen)}");
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
				else if(dataType == (int)DataType.Ping)
				{
					// 핑 메타데이터 생성
					var ping = BitConverter.GetBytes((int)DataType.Ping);

					// 핑 전송
					Client?.Socket.Send(ping);
				}

				if (dataType == (int)DataType.File)
				{
					UpdateLog("파일 데이터 처리중...");

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


						// 나머지 파일 데이터 가져오기
						while (receivedBytes < fileSizeLen)
						{
							if (ns.DataAvailable)
							{
								Thread.Sleep(2000);
								receivedBytes += data.Client.Socket.Receive(fileBuffer, receivedBytes, (fileSizeLen - receivedBytes), SocketFlags.None);
							}
						}

						// 전역 변수에 파일 데이터 저장
						DownloadFileBuffer = fileBuffer;
						DownloadFileName = fileName;
						DownloadFileSizeLen = fileSizeLen;

						UpdateLog("서버로부터 파일을 수신완료 했습니다");

						Invoke(new Action(() =>
						{
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
				// 메모리 및 설정 초기화
				ResetClient();
			}
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
			int max_log_length = 55;
			int index = 0;
			while (index < log.Length)
			{
				if (log.Length - index > max_log_length)
				{
					ShowLog(log.Substring(index, max_log_length));
				}
				else
				{
					ShowLog(log.Substring(index));
				}
				index += max_log_length;
			}
		}

		/// <summary>
		/// 로그 업데이트
		/// </summary>
		/// <param name="log"></param>
		private void ShowLog(string log)
		{
			Invoke(new Action(() =>
			{
				lbLog.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")} : {log}");

				// 항상 최신 로그가 선택되게끔 함
				lbLog.SelectedIndex = lbLog.Items.Count != -1 ? lbLog.Items.Count - 1 : -1;
			}));
		}

		/// <summary>
		/// 현재 서버 접속 상태를 표시
		/// </summary>
		/// <param name="connectionStatus"></param>
		private void SetConnectionStatus(bool connectionStatus)
		{
			Invoke(new Action(() => ssServerStatusLabel.Text = $"서버 접속 상태 : {(connectionStatus ? "접속중" : "미접속")}"));
		}

		/// <summary>
		/// 메모리 및 설정 초기화
		/// </summary>
		private void ResetClient()
		{
			SetConnectionStatus(false);

			Invoke(new Action(() =>
			{
				btnConnect.Enabled = true;
				btnDisconnect.Enabled = false;
				btnSendMessage.Enabled = false;
				btnFileUpload.Enabled = false;
				btnFileSend.Enabled = false;
				btnFileDownload.Enabled = false;
				lbFileName.Text = "없음";
			}));

			UploadFilePath = string.Empty;
			UploadFileBuffer = null;
			DownloadFileBuffer = null;
			DownloadFileName = string.Empty;
			DownloadFileSizeLen = 0;

			if (Client != null)
			{
				Client.Client.Close();
				Client.Client.Dispose();
				Client = null;
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
		/// 사설 IP 처리
		/// </summary>
		/// <returns></returns>
		private string GetLocalIP()
		{
			string localIP = string.Empty;

			var host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIP = ip.ToString();
					break;
				}
			}
			return localIP;
		}

		/// <summary>
		/// 공인 IP 처리
		/// </summary>
		/// <returns></returns>
		private string GetPublicIP()
		{
			string publicIP = new WebClient().DownloadString("http://ipinfo.io/ip").Trim();

			if (string.IsNullOrWhiteSpace(publicIP))
			{
				publicIP = GetLocalIP();
			}
			return publicIP;
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
				if (DownloadFileBuffer != null && DownloadFileBuffer.Length > 0 && status == true)
				{
					btnFileDownload.Enabled = status;
				}
			}));
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
						btnFileUpload.Enabled = false;
					}));

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
			catch (IOException)
			{
				MessageBox.Show("파일의 용량이 너무 큽니다 파일은 2GB 이하여야 합니다");
				UpdateLog("파일 업로드에 실패했습니다");
				Invoke(new Action(() =>
				{
					btnFileUpload.Enabled = true;
				}));
			}
			catch(Exception ex)
			{
				UpdateLog(ex.Message);
				UpdateLog("파일 업로드에 실패했습니다");
				Invoke(new Action(() =>
				{
					btnFileUpload.Enabled = true;
				}));
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

					// 데이터 조립 type(File) 4 byte, 파일이름데이터, 파일이름(4 byte), 파일용량(4 byte), 파일데이터)
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
						// 라벨에 파일명 표시
						lbFileName.Text = "없음";
					}));
				});
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("파일을 찾을 수 없습니다");
				Invoke(new Action(() =>
				{
					btnFileUpload.Enabled = true;
				}));
			}
			catch (Exception)
			{
				MessageBox.Show("파일 전송중 에러 발생");
				Invoke(new Action(() =>
				{
					btnFileUpload.Enabled = true;
					btnFileSend.Enabled = true;
				}));
			}
		}

		private string TryLabelSubString(string data, int length)
		{
			if(data.Length > length)
			{
				data = data.Substring(0, length) + "...";
			}
			return data;
		}
		/// <summary>
		/// 파일 전송 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFileSend_Click(object sender, EventArgs e)
		{
			try
			{
				// 파일 용량 경고
				if(UploadFileBuffer.Length > ((1024 * 1024) * 100)) 
				{
					if (MessageBox.Show("전송할 파일의 용량이 100MB 이상 입니다.\n고용량의 파일을 전송시 서버에서 처리할동안 프로그램이 멈추거나\n메모리 부족시 서버와의 연결이 끊어 질 수 있습니다.\n전송하시겠습니까?", " 경고", MessageBoxButtons.YesNo) == DialogResult.No)
					{
						return;
					}
				}

				UpdateLog($"서버에 파일을 송신하고 있습니다...");

				// 파일 전송
				_ = SendFileToServerAsync();
			}
			catch (Exception)
			{
				MessageBox.Show("파일 전송중 에러 발생");
			}
		}

		/// <summary>
		/// 파일 업로드시 파일 위치 저장
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFileUpload_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					// 업로드 파일 처리 비동기 실행
					_ = UploadFileAsync(openFileDialog.FileName);
				}
			}
		}

		/// <summary>
		/// 버퍼에 저장되어있는 파일 저장 함수
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFileDownload_Click(object sender, EventArgs e)
		{
			using(SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				// 다이얼로그 로드시 최초로 보여주는 저장위치(C드라이브)
				saveFileDialog.InitialDirectory = @"C:\";
				// 초기 저장이름
				saveFileDialog.FileName = DownloadFileName;
				// 파일 필터
				saveFileDialog.Filter = "모든 파일 (*.*)|*.*";

				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					if (File.Exists(saveFileDialog.FileName))
					{
						File.Delete(saveFileDialog.FileName);
					}
					// 이진데이터로 파일 작성
					BinaryWriter bw = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Append));
					bw.Write(DownloadFileBuffer);
					bw.Close();
				}
			}
		}

		/// <summary>
		/// 내부망(사설 IP) 접속 설정
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cbConnectPrivate_CheckedChanged(object sender, EventArgs e)
		{
			if(cbConnectPrivate.Checked)
			{
				ConnectPrivate = true;
			}
			else
			{
				ConnectPrivate = false;
			}
		}
	}
}
