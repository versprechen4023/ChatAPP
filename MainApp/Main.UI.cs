using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainApp
{
	public partial class MainClient
	{
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
				pbFileProgress.Value = 0;
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

			// 전세대 가비지컬렉터 수행
			GC.Collect();
		}

		/// <summary>
		/// 메시지 전송 버튼 초 제한 설정
		/// </summary>
		/// <returns></returns>
		private async Task setMessageSendInterVal(int interVal)
		{

			Invoke(new Action(() =>
			{
				btnSendMessage.Enabled = false;
			}));

			// 메시지 전송 활성화 대기
			await Task.Delay(interVal);

			Invoke(new Action(() =>
			{
				btnSendMessage.Enabled = true;
			}));

		}

		/// <summary>
		/// 파일 업로드시 에러처리
		/// </summary>
		/// <param name="msg"></param>
		private void ShowFileUploadError(string msg)
		{
			MessageBox.Show(msg);
			UpdateLog("파일 업로드에 실패했습니다");
			Invoke(new Action(() =>
			{
				btnFileUpload.Enabled = true;
			}));
		}

		/// <summary>
		/// 아이피 포트 키 입력 핸들 함수
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool KeyHandler(KeyPressEventArgs e)
		{
			//숫자와 백스페이스만 허용
			if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
			{
				return true;
			}
			return false;
		}

		private void textConnectPort_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = KeyHandler(e);
		}

		private void textConnectIp4_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = KeyHandler(e);
		}

		private void textConnectIp3_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = KeyHandler(e);
		}

		private void textConnectIp2_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = KeyHandler(e);
		}

		private void textConnectIp1_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = KeyHandler(e);
		}
	}
}
