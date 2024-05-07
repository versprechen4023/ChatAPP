using AppCommon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;


namespace ServerApp
{
	public partial class ServerClient : Form
	{
		/// <summary>
		/// TCP 서버
		/// </summary>
		private TcpListenerExtension Server { get; set; }

		/// <summary>
		/// 접속중인 클라이언트 리스트
		/// </summary>
		private SynchronizedCollection<TcpClientExtension> ClientList { get; set; }

		/// <summary>
		/// 핑에 응답한 클라이언트 리스트
		/// </summary>
		private List<TcpClientExtension> ConnectClientList;

		/// <summary>
		/// 전송할 데이터의 타입 구분
		/// </summary>
		private enum DataType { TEXT = 1, File, CallBackFileAccept, CallBackFileSended, Ping };

		/// <summary>
		/// 전송 받은 파일을 저장할 폴더
		/// </summary>
		private string SaveFilePath = string.Empty;

		/// <summary>
		/// 해외 아이피 차단 유무 디폴트 = true
		/// </summary>
		private bool BlockForeignIP = true;

		/// <summary>
		/// 내부망(사설IP)만 허용 할지 여부
		/// </summary>
		private bool AcceptLocalOnly = false;

		public ServerClient()
		{
			InitializeComponent();

			// 클라이언트 리스트 초기 객체 생성
			ClientList = new SynchronizedCollection<TcpClientExtension>();
		}

		/// <summary>
		/// 서버 실행 버튼 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnServerStart_Click(object sender, EventArgs e)
		{
			// 포트 번호 변수 공백제거
			var port = textPort.Text.Trim();

			try
			{
				// 포트번호가 유효한지 체크
				if (!CheckPortNumber(port)) return;

				// IP 설정
				var localIP = GetLocalIP();

				// 서버 설정 및 실행 시작(공유기 환경일 경우 localIP에 대한 포트포워딩 필요) 내부 환경일경우 127.0.0.1 활용
				var localEndPoint = new IPEndPoint(IPAddress.Parse(localIP), int.Parse(port));
				Server = new TcpListenerExtension(localEndPoint);
				Server.Start();

				// 비동기로 연결 수신 대기 루프 실행(Task 반환값을 별도로 반환받지 않으므로 무시변수로 할당)
				_ = AsyncAcceptWaitFor();

				// 서버 상태를 실행중으로 변경
				SetConnectionStatus(true);

				btnServerStart.Enabled = false;
				btnServerEnd.Enabled = true;
				cbIPCheck.Enabled = false;
				cbOnlyLocalIP.Enabled = false;

				// 핑 체크 타이머 실행
				CheckConnectList.Interval = (1000 * 60) * 30;
				CheckConnectList.Enabled = true;
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
			{
				MessageBox.Show("설정하신 포트는 다른 시스템에서 이미 사용중입니다");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

		}

		/// <summary>
		/// 서버 종료 버튼 액션
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnServerEnd_Click(object sender, EventArgs e)
		{
			// 서버 종료
			Server?.Stop();
			Server = null;

			// 서버 상태 설정
			SetConnectionStatus(false);

			// 클라이언트 리스트 초기화
			lbConnectingList.Items.Clear();

			CheckConnectList.Stop();
			// 접속중인 클라이언트 연결을 모두 끊음 쓰레드 충돌 방지를 위한 LOCK 키워드 사용
			lock (ClientList.SyncRoot)
			{
				foreach (var client in ClientList)
				{
					client.Socket.Close();
				}
				ClientList.Clear();
			}

			btnServerStart.Enabled = true;
			btnServerEnd.Enabled = false;
			cbOnlyLocalIP.Enabled = true;
			if (AcceptLocalOnly) { cbIPCheck.Enabled = false; } else { cbIPCheck.Enabled = true; }

			CheckConnectList.Enabled = false;
		}

		/// <summary>
		/// 폼 로드시 서버 상태 설정
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ServerClient_Load(object sender, EventArgs e)
		{
			// 서버 상태 초기 설정
			SetConnectionStatus(false);
		}

		/// <summary>
		/// 30분에 한번씩 접속 목록 확인
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckConnectList_Tick(object sender, EventArgs e)
		{
			lock (ClientList.SyncRoot)
			{
				foreach (var client in ClientList)
				{
					// 핑 메타데이터 생성
					var ping = BitConverter.GetBytes((int)DataType.Ping);

					// 핑 전송
					client.Socket.Send(ping);
				}
			}

			// 핑 처리 시작
			ConnectClientList = new List<TcpClientExtension>();
			_ = CheckConnect();
		}

		/// <summary>
		/// 해외 아이피 차단 설정
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cbIPCheck_CheckedChanged(object sender, EventArgs e)
		{
			if (cbIPCheck.Checked)
			{
				BlockForeignIP = true;
			}
			else
			{
				BlockForeignIP = false;
			}
		}

		/// <summary>
		/// 내부망(사설 IP)전용 설정
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cbOnlyLocalIP_CheckedChanged(object sender, EventArgs e)
		{
			if (cbOnlyLocalIP.Checked)
			{
				if (MessageBox.Show("내부망 설정을 하게 되면 외부에서 이 서버에 연결 할 수 없습니다.\n정말 설정 하시겠습니까?", " 경고", MessageBoxButtons.YesNo) == DialogResult.No)
				{
					cbOnlyLocalIP.Checked = false;
				}
				else
				{
					AcceptLocalOnly = true;
					cbIPCheck.Checked = false;
					cbIPCheck.Enabled = false;
				}
			}
			else
			{
				AcceptLocalOnly = false;
				cbIPCheck.Enabled = true;
			}
		}
	}
}
