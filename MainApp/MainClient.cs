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
		/// 업로드 최대 용량(디폴트 32bit)
		/// </summary>
		private const long MAX_UPLOAD_FILE_BUFFER_SIZE = 2147483647;

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
				if (ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.ConnectionRefused)
				{
					MessageBox.Show("서버에 접속 할 수 없습니다.\n서버가 오프라인이거나 연결을 거부 했습니다");
					Invoke(new Action(() =>
					{
						btnConnect.Enabled = true;
					}));
					UpdateLog("서버 접속에 실패했습니다");
				}
				else if (ex.SocketErrorCode == SocketError.AddressNotAvailable)
				{
					MessageBox.Show("그 아이피 주소로는 연결 할 수 없습니다.\n내부망에서 연결 시도를 하는 경우에는 \"내부망연결\"에 체크 해주십시오.");
					Invoke(new Action(() =>
					{
						btnConnect.Enabled = true;
					}));
					UpdateLog("서버 접속에 실패했습니다");
				}
			}
			catch (Exception)
			{
				MessageBox.Show("서버 접속에 실패했습니다");
				Invoke(new Action(() =>
				{
					btnConnect.Enabled = true;
				}));
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

				// 버튼 입력 제한 활성
				_ = setMessageSendInterVal(2000);
			}
			catch (Exception)
			{
				UpdateLog("메시지 전송에 실패했습니다");
			}
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
				if (UploadFileBuffer.Length > ((1024 * 1024) * 100))
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
			catch (OutOfMemoryException)
			{
				MessageBox.Show("컴퓨터의 메모리가 부족합니다. 파일을 전송하지 못했습니다");
				// 메모리 정리
				UploadFilePath = string.Empty;
				DisposeGlobalUploadBuffer();
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
				MessageBox.Show("파일 전송중 에러가 발생 했습니다");
				UpdateLog($"파일을 전송하지 못했습니다");
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
					// 메모리 정리
					UploadFilePath = string.Empty;
					DisposeGlobalUploadBuffer();

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
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
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
			if (cbConnectPrivate.Checked)
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
