using System;
using System.Net.Sockets;

namespace ServerApp
{
	public partial class ServerClient
	{
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
		private void ShowLog(string log)
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
