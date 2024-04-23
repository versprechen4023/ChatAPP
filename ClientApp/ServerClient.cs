﻿using AppCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

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

				// 서버 설정 및 실행 시작
				var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse(port));
				Server = new TcpListenerExtension(localEndPoint);
				Server.Start();

				// 비동기로 연결 수신 대기 루프 실행(Task 반환값을 별도로 반환받지 않으므로 무시변수로 할당)
				_ = AsyncAcceptWaitFor();

				// 서버 상태를 실행중으로 변경
				SetConnectionStatus(true);

				btnServerStart.Enabled = false;
				btnServerEnd.Enabled = true;
			}
			catch(SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) 
			{
				MessageBox.Show("설정하신 포트는 다른 시스템에서 이미 사용중입니다");
			}
			catch(Exception ex)
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

			// 접속중인 클라이언트 연결을 모두 끊음 쓰레드 충돌 방지를 위한 LOCK 키워드 사용
			lock (ClientList.SyncRoot)
			{
				foreach(var client in ClientList)
				{
					client.Socket.Close();
				}
				ClientList.Clear();
			}

			btnServerStart.Enabled = true;
			btnServerEnd.Enabled = false;
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
			if(!Regex.IsMatch(port, "^[0-9]+$"))
			{
				MessageBox.Show("포트 번호가 올바르지 않습니다");
				return false;
			}

			var portNum = int.Parse(port);

			// 포트 번호의 범위가 유효한지 확인
			if(portNum < IPEndPoint.MinPort || IPEndPoint.MaxPort < portNum)
			{
				MessageBox.Show("포트 범위가 유효하지 않습니다 포트범위는 0~65335 사이여야 합니다");
				return false;
			}

			return true;
			
		}

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
		private void SendDataToAllClient(CommonData data, string text)
		{
			// 비동기 실행중 클라이언트 리스트에 접근하므로 쓰레드 세이프 처리를 실행
			lock (ClientList.SyncRoot)
			{
				// 전송자를 제외한 모든 클라이언트에게 데이터 전송
				foreach(var client in ClientList.Where(e => !e.Equals(data.Client)))
				{
					// 데이터 전송
					client.Socket.Send(Encoding.UTF8.GetBytes(text));

					// 로그 업데이트
					UpdateLog($"{client.RemoteEndPoint}에 데이터 전송>>{text}");
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
				var length = data.Client.Socket.EndReceive(result);

				// 클라이언트로 부터 받은 데이터가 없다면 접속 절단으로 판단
				if(length == 0)
				{
					// 로그 표시
					UpdateLog($"{data.Client.RemoteEndPoint}에서 접속이 종료되었습니다");

					// 클라이언트 목록에서 절단 클라이언트를 삭제
					SetConnectionClient(data.Client.Client, false);
					ClientList.Remove(data.Client);

					// 접속중인 클라이언트에게 메시지 송신(절단 클라이언트 제외)
					SendDataToAllClient(data, $"{data.Client.RemoteEndPoint}에서 접속을 종료했습니다");

					return;
				}

				// 전송 로그 업데이트
				UpdateLog($"{data.Client.RemoteEndPoint}:{data}");

				// 전송자를 제외한 모든 클라이언트에게 데이터 전송
				SendDataToAllClient(data, data.ToString());

				// 서버가 실행중인경우 계속해서 접속중인 클라이언트로 부터 데이터 수신 대기
				if (Server != null && Server.Active)
				{
					data.Client.Socket.BeginReceive(data.Data, 0, data.Data.Length, SocketFlags.None, ReceiveCallback, data);
				}
			}

			catch (Exception) { }
		}
		/// <summary>
		/// 현재 서버 상태를 표시
		/// </summary>
		/// <param name="connectionStatus"></param>
		private void SetConnectionStatus(bool connectionStatus)
		{
			Invoke(new Action(() => ssServerStatusLabel.Text = $"서버 상태 : {(connectionStatus ? "실행중" : "정지중")}"));
		}

		/// <summary>
		/// 클라이언트 리스트에 목록 업데이트
		/// </summary>
		/// <param name="client"></param>
		/// <param name="status"></param>
		private void SetConnectionClient(TcpClient client, bool status)
		{
			Invoke(new Action(() =>
			{
				if (status)
				{
					lbConnectingList.Items.Add(client.Client.RemoteEndPoint);
				}
				else
				{
					lbConnectingList.Items.Remove(client.Client.RemoteEndPoint);
				}
				// 항상 최신 리스트가 선택되게끔 함
				lbConnectingList.SelectedIndex = lbConnectingList.Items.Count != -1 ? lbConnectingList.Items.Count - 1 : -1;
			}));
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
	}
}