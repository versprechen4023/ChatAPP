﻿using AppCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainApp
{
	public partial class MainClient : Form
	{
		/// <summary>
		/// 클라이언트
		/// </summary>
		private TcpClientExtension Client { get; set; }

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
			if(Client != null)
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
				// 전송할 메시지 데이터 설정
				var message = Encoding.UTF8.GetBytes(textNickName.Text + " : " + textMessage.Text);

				// 서버에 데이터 송신
				Client?.Socket.Send(message);

				// 로그 출력
				UpdateLog($"송신>>{Encoding.UTF8.GetString(message)}");

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

				// 서버로 부터 데이터 수신 처리
				data.Client.Socket?.EndReceive(result);

				// 수신받은 데이터 로그에 출력
				UpdateLog($"서버로 부터 데이터 수신<<{data}");

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
			if(!IPAddress.TryParse(ip, out IPAddress _))
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
	}
}