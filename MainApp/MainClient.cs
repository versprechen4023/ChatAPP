using AppCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
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
		private enum DataType { TEXT = 1, File };

		/// <summary>
		/// 전송 받은 파일을 저장할 폴더
		/// </summary>
		private string SaveFilePath = string.Empty;

		public MainClient()
		{
			InitializeComponent();
		}

		private void MainClient_Load(object sender, EventArgs e)
		{
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
		private void btnConnect_Click(object sender, EventArgs e)
		{
			// 연결할서버의 포트
			var connectPort = textConnectPort.Text.Trim();
			// 연결할서버의 아이피
			var connectIpAddress = textConnectIp.Text.Trim();

			try
			{
				// 아아피 포트 정규식 체크
				if (!CheckPortAndIpNumber(connectPort, connectIpAddress)) return;

				// 접속할 서버의 엔드포인트 작성
				var connectEndPoint = new IPEndPoint(IPAddress.Parse(connectIpAddress), int.Parse(connectPort));

				// 클라이언트의 엔드포인트 작성(포트는 자동할당)
				var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

				// 클라이언트 생성
				Client = new TcpClientExtension(localEndPoint);

				// 서버에 접속
				Client.Client.Connect(connectEndPoint);

				// 서버 접속 상태를 접속중으로 변경
				SetConnectionStatus(true);

				// 접속 로그 출력
				UpdateLog("서버에 접속 했습니다");

				// 서버로부터 데이터 수신 대기
				var data = new CommonData(Client);
				Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);

				btnConnect.Enabled = false;
				btnDisconnect.Enabled = true;
				btnSendMessage.Enabled = true;
				btnFileUpload.Enabled = true;
			}
			catch (Exception ex)
			{
				UpdateLog(ex.Message);

				UpdateLog("서버 접속에 실패했습니다");
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

			// 서버 접속 상태를 미접속으로 변경
			SetConnectionStatus(false);

			// 로그 출력
			UpdateLog("서버와 연결이 종료되었습니다");


			btnConnect.Enabled = true;
			btnDisconnect.Enabled = false;
			btnSendMessage.Enabled = false;
			btnFileUpload.Enabled = false;
			btnFileSend.Enabled = false;
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
			catch (Exception ex)
			{
				UpdateLog(ex.Message);
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
					UpdateLog($"서버로 부터 메시지 수신<<{message}");
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
					UpdateLog($"서버로 부터 파일 수신 :{fileName}");

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
								receivedBytes += data.Client.Socket.Receive(fileBuffer, receivedBytes, (fileSizeLen - receivedBytes), SocketFlags.None);
							}
						}

						UpdateLog($"수신된 {fileName}을 {SaveFilePath}에 저장했습니다");
					}
				}

				// 다시 서버로 부터 데이터 수신 대기
				data.Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);
			}

			catch (Exception)
			{
				UpdateLog("서버로 부터 접속이 끊어졌습니다");

				// 서버 접속 상태를 미접속으로 변경
				SetConnectionStatus(false);

				// ui 쓰레드는 별도 처리
				Invoke(new Action(() =>
				{
					btnConnect.Enabled = true;
					btnDisconnect.Enabled = false;
					btnSendMessage.Enabled = false;
					btnFileUpload.Enabled = false;
					btnFileSend.Enabled = false;
				}));
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
		/// 로그 업데이트
		/// </summary>
		/// <param name="log"></param>
		private void UpdateLog(string log)
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
		/// 파일 전송 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFileSend_Click(object sender, EventArgs e)
		{
			try
			{
				// 파일 경로
				var filePath = lbFileName.Text;
				// 파일명
				var fileName = Path.GetFileName(filePath);
				// 파일이름 인코딩
				var fileNameData = Encoding.UTF8.GetBytes(fileName);

				// 전송할 데이터 타입 설정
				var dataType = BitConverter.GetBytes((int)DataType.File);
				// 파일이름 바이너리 데이터
				var fileNameLen = BitConverter.GetBytes(fileNameData.Length);
				// 파일의 바이너리 데이터
				var fileData = File.ReadAllBytes(filePath);
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
				UpdateLog($"서버에 파일 송신>>{fileName}");
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("파일을 찾을 수 없습니다");
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
					Invoke(new Action(() =>
					{
						// 라벨에 파일 위치 저장
						lbFileName.Text = openFileDialog.FileName;
						btnFileSend.Enabled = true;
					}));
				}
			}
		}
	}
}
