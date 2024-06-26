﻿namespace MainApp
{
	partial class MainClient
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainClient));
			this.gbMain = new System.Windows.Forms.GroupBox();
			this.textConnectIp3 = new System.Windows.Forms.TextBox();
			this.lbIP3 = new System.Windows.Forms.Label();
			this.lbIP2 = new System.Windows.Forms.Label();
			this.textConnectIp2 = new System.Windows.Forms.TextBox();
			this.textConnectIp4 = new System.Windows.Forms.TextBox();
			this.lbIP1 = new System.Windows.Forms.Label();
			this.cbConnectPrivate = new System.Windows.Forms.CheckBox();
			this.btnDisconnect = new System.Windows.Forms.Button();
			this.btnConnect = new System.Windows.Forms.Button();
			this.textConnectPort = new System.Windows.Forms.TextBox();
			this.textConnectIp1 = new System.Windows.Forms.TextBox();
			this.labelPort = new System.Windows.Forms.Label();
			this.labelIp = new System.Windows.Forms.Label();
			this.gbInfo = new System.Windows.Forms.GroupBox();
			this.textNickName = new System.Windows.Forms.TextBox();
			this.lbNickName = new System.Windows.Forms.Label();
			this.btnSendMessage = new System.Windows.Forms.Button();
			this.textMessage = new System.Windows.Forms.TextBox();
			this.lbMessage = new System.Windows.Forms.Label();
			this.btnFileSend = new System.Windows.Forms.Button();
			this.gbLog = new System.Windows.Forms.GroupBox();
			this.lbLog = new System.Windows.Forms.ListBox();
			this.ssServerStatus = new System.Windows.Forms.StatusStrip();
			this.ssServerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.gbFileUpload = new System.Windows.Forms.GroupBox();
			this.lbFileName = new System.Windows.Forms.Label();
			this.btnFileUpload = new System.Windows.Forms.Button();
			this.lbFilelabel = new System.Windows.Forms.Label();
			this.btnFileDownload = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lbFileProgress = new System.Windows.Forms.Label();
			this.pbFileProgress = new System.Windows.Forms.ProgressBar();
			this.gbMain.SuspendLayout();
			this.gbInfo.SuspendLayout();
			this.gbLog.SuspendLayout();
			this.ssServerStatus.SuspendLayout();
			this.gbFileUpload.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbMain
			// 
			this.gbMain.Controls.Add(this.textConnectIp3);
			this.gbMain.Controls.Add(this.lbIP3);
			this.gbMain.Controls.Add(this.lbIP2);
			this.gbMain.Controls.Add(this.textConnectIp2);
			this.gbMain.Controls.Add(this.textConnectIp4);
			this.gbMain.Controls.Add(this.lbIP1);
			this.gbMain.Controls.Add(this.cbConnectPrivate);
			this.gbMain.Controls.Add(this.btnDisconnect);
			this.gbMain.Controls.Add(this.btnConnect);
			this.gbMain.Controls.Add(this.textConnectPort);
			this.gbMain.Controls.Add(this.textConnectIp1);
			this.gbMain.Controls.Add(this.labelPort);
			this.gbMain.Controls.Add(this.labelIp);
			this.gbMain.Location = new System.Drawing.Point(10, 10);
			this.gbMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gbMain.Name = "gbMain";
			this.gbMain.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gbMain.Size = new System.Drawing.Size(679, 69);
			this.gbMain.TabIndex = 0;
			this.gbMain.TabStop = false;
			this.gbMain.Text = "접속설정";
			// 
			// textConnectIp3
			// 
			this.textConnectIp3.Location = new System.Drawing.Point(194, 30);
			this.textConnectIp3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.textConnectIp3.MaxLength = 3;
			this.textConnectIp3.Name = "textConnectIp3";
			this.textConnectIp3.Size = new System.Drawing.Size(36, 21);
			this.textConnectIp3.TabIndex = 3;
			this.textConnectIp3.Text = "0";
			this.textConnectIp3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textConnectIp3_KeyPress);
			// 
			// lbIP3
			// 
			this.lbIP3.AutoSize = true;
			this.lbIP3.Font = new System.Drawing.Font("굴림", 9F);
			this.lbIP3.Location = new System.Drawing.Point(236, 39);
			this.lbIP3.Name = "lbIP3";
			this.lbIP3.Size = new System.Drawing.Size(9, 12);
			this.lbIP3.TabIndex = 0;
			this.lbIP3.Text = ".";
			// 
			// lbIP2
			// 
			this.lbIP2.AutoSize = true;
			this.lbIP2.Font = new System.Drawing.Font("굴림", 9F);
			this.lbIP2.Location = new System.Drawing.Point(179, 39);
			this.lbIP2.Name = "lbIP2";
			this.lbIP2.Size = new System.Drawing.Size(9, 12);
			this.lbIP2.TabIndex = 0;
			this.lbIP2.Text = ".";
			// 
			// textConnectIp2
			// 
			this.textConnectIp2.Location = new System.Drawing.Point(137, 30);
			this.textConnectIp2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.textConnectIp2.MaxLength = 3;
			this.textConnectIp2.Name = "textConnectIp2";
			this.textConnectIp2.Size = new System.Drawing.Size(36, 21);
			this.textConnectIp2.TabIndex = 2;
			this.textConnectIp2.Text = "0";
			this.textConnectIp2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textConnectIp2_KeyPress);
			// 
			// textConnectIp4
			// 
			this.textConnectIp4.Location = new System.Drawing.Point(251, 31);
			this.textConnectIp4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.textConnectIp4.MaxLength = 3;
			this.textConnectIp4.Name = "textConnectIp4";
			this.textConnectIp4.Size = new System.Drawing.Size(36, 21);
			this.textConnectIp4.TabIndex = 4;
			this.textConnectIp4.Text = "1";
			this.textConnectIp4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textConnectIp4_KeyPress);
			// 
			// lbIP1
			// 
			this.lbIP1.AutoSize = true;
			this.lbIP1.Font = new System.Drawing.Font("굴림", 9F);
			this.lbIP1.Location = new System.Drawing.Point(122, 39);
			this.lbIP1.Name = "lbIP1";
			this.lbIP1.Size = new System.Drawing.Size(9, 12);
			this.lbIP1.TabIndex = 0;
			this.lbIP1.Text = ".";
			// 
			// cbConnectPrivate
			// 
			this.cbConnectPrivate.AutoSize = true;
			this.cbConnectPrivate.Location = new System.Drawing.Point(435, 19);
			this.cbConnectPrivate.Name = "cbConnectPrivate";
			this.cbConnectPrivate.Size = new System.Drawing.Size(88, 16);
			this.cbConnectPrivate.TabIndex = 6;
			this.cbConnectPrivate.TabStop = false;
			this.cbConnectPrivate.Text = "내부망 연결";
			this.cbConnectPrivate.UseVisualStyleBackColor = true;
			this.cbConnectPrivate.CheckedChanged += new System.EventHandler(this.cbConnectPrivate_CheckedChanged);
			// 
			// btnDisconnect
			// 
			this.btnDisconnect.Enabled = false;
			this.btnDisconnect.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnDisconnect.Location = new System.Drawing.Point(599, 19);
			this.btnDisconnect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btnDisconnect.Name = "btnDisconnect";
			this.btnDisconnect.Size = new System.Drawing.Size(66, 37);
			this.btnDisconnect.TabIndex = 8;
			this.btnDisconnect.TabStop = false;
			this.btnDisconnect.Text = "종료";
			this.btnDisconnect.UseVisualStyleBackColor = true;
			this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(528, 18);
			this.btnConnect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(66, 37);
			this.btnConnect.TabIndex = 7;
			this.btnConnect.TabStop = false;
			this.btnConnect.Text = "연결";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// textConnectPort
			// 
			this.textConnectPort.Location = new System.Drawing.Point(353, 31);
			this.textConnectPort.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.textConnectPort.MaxLength = 5;
			this.textConnectPort.Name = "textConnectPort";
			this.textConnectPort.Size = new System.Drawing.Size(46, 21);
			this.textConnectPort.TabIndex = 5;
			this.textConnectPort.Text = "33306";
			this.textConnectPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textConnectPort_KeyPress);
			// 
			// textConnectIp1
			// 
			this.textConnectIp1.Location = new System.Drawing.Point(80, 29);
			this.textConnectIp1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.textConnectIp1.MaxLength = 3;
			this.textConnectIp1.Name = "textConnectIp1";
			this.textConnectIp1.Size = new System.Drawing.Size(36, 21);
			this.textConnectIp1.TabIndex = 1;
			this.textConnectIp1.Text = "127";
			this.textConnectIp1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textConnectIp1_KeyPress);
			// 
			// labelPort
			// 
			this.labelPort.AutoSize = true;
			this.labelPort.Location = new System.Drawing.Point(294, 34);
			this.labelPort.Name = "labelPort";
			this.labelPort.Size = new System.Drawing.Size(53, 12);
			this.labelPort.TabIndex = 1;
			this.labelPort.Text = "접속포트";
			// 
			// labelIp
			// 
			this.labelIp.AutoSize = true;
			this.labelIp.Location = new System.Drawing.Point(10, 34);
			this.labelIp.Name = "labelIp";
			this.labelIp.Size = new System.Drawing.Size(64, 12);
			this.labelIp.TabIndex = 0;
			this.labelIp.Text = "접속서버IP";
			// 
			// gbInfo
			// 
			this.gbInfo.Controls.Add(this.textNickName);
			this.gbInfo.Controls.Add(this.lbNickName);
			this.gbInfo.Controls.Add(this.btnSendMessage);
			this.gbInfo.Controls.Add(this.textMessage);
			this.gbInfo.Controls.Add(this.lbMessage);
			this.gbInfo.Location = new System.Drawing.Point(10, 83);
			this.gbInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gbInfo.Name = "gbInfo";
			this.gbInfo.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gbInfo.Size = new System.Drawing.Size(462, 80);
			this.gbInfo.TabIndex = 0;
			this.gbInfo.TabStop = false;
			this.gbInfo.Text = "인포";
			// 
			// textNickName
			// 
			this.textNickName.Location = new System.Drawing.Point(87, 17);
			this.textNickName.MaxLength = 20;
			this.textNickName.Name = "textNickName";
			this.textNickName.Size = new System.Drawing.Size(100, 21);
			this.textNickName.TabIndex = 9;
			this.textNickName.TabStop = false;
			this.textNickName.Text = "익명";
			// 
			// lbNickName
			// 
			this.lbNickName.AutoSize = true;
			this.lbNickName.Location = new System.Drawing.Point(6, 20);
			this.lbNickName.Name = "lbNickName";
			this.lbNickName.Size = new System.Drawing.Size(41, 12);
			this.lbNickName.TabIndex = 3;
			this.lbNickName.Text = "닉네임";
			// 
			// btnSendMessage
			// 
			this.btnSendMessage.Enabled = false;
			this.btnSendMessage.Location = new System.Drawing.Point(335, 43);
			this.btnSendMessage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.btnSendMessage.Name = "btnSendMessage";
			this.btnSendMessage.Size = new System.Drawing.Size(117, 23);
			this.btnSendMessage.TabIndex = 11;
			this.btnSendMessage.TabStop = false;
			this.btnSendMessage.Text = "메세지 송신";
			this.btnSendMessage.UseVisualStyleBackColor = true;
			this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
			// 
			// textMessage
			// 
			this.textMessage.Location = new System.Drawing.Point(87, 45);
			this.textMessage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.textMessage.MaxLength = 300;
			this.textMessage.Name = "textMessage";
			this.textMessage.Size = new System.Drawing.Size(242, 21);
			this.textMessage.TabIndex = 10;
			this.textMessage.TabStop = false;
			// 
			// lbMessage
			// 
			this.lbMessage.AutoSize = true;
			this.lbMessage.Location = new System.Drawing.Point(5, 47);
			this.lbMessage.Name = "lbMessage";
			this.lbMessage.Size = new System.Drawing.Size(69, 12);
			this.lbMessage.TabIndex = 0;
			this.lbMessage.Text = "메세지 내용";
			// 
			// btnFileSend
			// 
			this.btnFileSend.Enabled = false;
			this.btnFileSend.Location = new System.Drawing.Point(113, 46);
			this.btnFileSend.Name = "btnFileSend";
			this.btnFileSend.Size = new System.Drawing.Size(84, 23);
			this.btnFileSend.TabIndex = 13;
			this.btnFileSend.TabStop = false;
			this.btnFileSend.Text = "파일 전송";
			this.btnFileSend.UseVisualStyleBackColor = true;
			this.btnFileSend.Click += new System.EventHandler(this.btnFileSend_Click);
			// 
			// gbLog
			// 
			this.gbLog.Controls.Add(this.lbLog);
			this.gbLog.Location = new System.Drawing.Point(10, 168);
			this.gbLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gbLog.Name = "gbLog";
			this.gbLog.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gbLog.Size = new System.Drawing.Size(583, 170);
			this.gbLog.TabIndex = 0;
			this.gbLog.TabStop = false;
			this.gbLog.Text = "로그";
			// 
			// lbLog
			// 
			this.lbLog.FormattingEnabled = true;
			this.lbLog.ItemHeight = 12;
			this.lbLog.Location = new System.Drawing.Point(5, 19);
			this.lbLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.lbLog.Name = "lbLog";
			this.lbLog.Size = new System.Drawing.Size(570, 148);
			this.lbLog.TabIndex = 0;
			this.lbLog.TabStop = false;
			// 
			// ssServerStatus
			// 
			this.ssServerStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ssServerStatusLabel});
			this.ssServerStatus.Location = new System.Drawing.Point(0, 338);
			this.ssServerStatus.Name = "ssServerStatus";
			this.ssServerStatus.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
			this.ssServerStatus.Size = new System.Drawing.Size(700, 22);
			this.ssServerStatus.TabIndex = 0;
			this.ssServerStatus.Text = "statusStrip1";
			// 
			// ssServerStatusLabel
			// 
			this.ssServerStatusLabel.Name = "ssServerStatusLabel";
			this.ssServerStatusLabel.Size = new System.Drawing.Size(59, 17);
			this.ssServerStatusLabel.Text = "서버 상태";
			// 
			// gbFileUpload
			// 
			this.gbFileUpload.Controls.Add(this.lbFileName);
			this.gbFileUpload.Controls.Add(this.btnFileUpload);
			this.gbFileUpload.Controls.Add(this.lbFilelabel);
			this.gbFileUpload.Controls.Add(this.btnFileSend);
			this.gbFileUpload.Location = new System.Drawing.Point(478, 84);
			this.gbFileUpload.Name = "gbFileUpload";
			this.gbFileUpload.Size = new System.Drawing.Size(212, 79);
			this.gbFileUpload.TabIndex = 0;
			this.gbFileUpload.TabStop = false;
			this.gbFileUpload.Text = "파일 업로드";
			// 
			// lbFileName
			// 
			this.lbFileName.AutoSize = true;
			this.lbFileName.Location = new System.Drawing.Point(58, 25);
			this.lbFileName.Name = "lbFileName";
			this.lbFileName.Size = new System.Drawing.Size(29, 12);
			this.lbFileName.TabIndex = 0;
			this.lbFileName.Text = "없음";
			// 
			// btnFileUpload
			// 
			this.btnFileUpload.Enabled = false;
			this.btnFileUpload.Location = new System.Drawing.Point(8, 46);
			this.btnFileUpload.Name = "btnFileUpload";
			this.btnFileUpload.Size = new System.Drawing.Size(99, 23);
			this.btnFileUpload.TabIndex = 12;
			this.btnFileUpload.TabStop = false;
			this.btnFileUpload.Text = "파일 업로드";
			this.btnFileUpload.UseVisualStyleBackColor = true;
			this.btnFileUpload.Click += new System.EventHandler(this.btnFileUpload_Click);
			// 
			// lbFilelabel
			// 
			this.lbFilelabel.AutoSize = true;
			this.lbFilelabel.Location = new System.Drawing.Point(6, 25);
			this.lbFilelabel.Name = "lbFilelabel";
			this.lbFilelabel.Size = new System.Drawing.Size(49, 12);
			this.lbFilelabel.TabIndex = 0;
			this.lbFilelabel.Text = "파일명 :";
			// 
			// btnFileDownload
			// 
			this.btnFileDownload.Enabled = false;
			this.btnFileDownload.Location = new System.Drawing.Point(6, 20);
			this.btnFileDownload.Name = "btnFileDownload";
			this.btnFileDownload.Size = new System.Drawing.Size(77, 28);
			this.btnFileDownload.TabIndex = 14;
			this.btnFileDownload.TabStop = false;
			this.btnFileDownload.Text = "파일 저장";
			this.btnFileDownload.UseVisualStyleBackColor = true;
			this.btnFileDownload.Click += new System.EventHandler(this.btnFileDownload_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lbFileProgress);
			this.groupBox1.Controls.Add(this.pbFileProgress);
			this.groupBox1.Controls.Add(this.btnFileDownload);
			this.groupBox1.Location = new System.Drawing.Point(599, 169);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(89, 166);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "파일 처리";
			// 
			// lbFileProgress
			// 
			this.lbFileProgress.AutoSize = true;
			this.lbFileProgress.Location = new System.Drawing.Point(4, 122);
			this.lbFileProgress.Name = "lbFileProgress";
			this.lbFileProgress.Size = new System.Drawing.Size(81, 12);
			this.lbFileProgress.TabIndex = 0;
			this.lbFileProgress.Text = "파일 처리상태";
			// 
			// pbFileProgress
			// 
			this.pbFileProgress.Location = new System.Drawing.Point(6, 137);
			this.pbFileProgress.Name = "pbFileProgress";
			this.pbFileProgress.Size = new System.Drawing.Size(77, 23);
			this.pbFileProgress.TabIndex = 0;
			// 
			// MainClient
			// 
			this.AcceptButton = this.btnSendMessage;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(700, 360);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.gbFileUpload);
			this.Controls.Add(this.ssServerStatus);
			this.Controls.Add(this.gbLog);
			this.Controls.Add(this.gbInfo);
			this.Controls.Add(this.gbMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MaximizeBox = false;
			this.Name = "MainClient";
			this.Text = "MainClient";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainClient_FormClosed);
			this.Load += new System.EventHandler(this.MainClient_Load);
			this.gbMain.ResumeLayout(false);
			this.gbMain.PerformLayout();
			this.gbInfo.ResumeLayout(false);
			this.gbInfo.PerformLayout();
			this.gbLog.ResumeLayout(false);
			this.ssServerStatus.ResumeLayout(false);
			this.ssServerStatus.PerformLayout();
			this.gbFileUpload.ResumeLayout(false);
			this.gbFileUpload.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox gbMain;
		private System.Windows.Forms.GroupBox gbInfo;
		private System.Windows.Forms.GroupBox gbLog;
		private System.Windows.Forms.StatusStrip ssServerStatus;
		private System.Windows.Forms.ToolStripStatusLabel ssServerStatusLabel;
		private System.Windows.Forms.Label labelIp;
		private System.Windows.Forms.Label labelPort;
		private System.Windows.Forms.TextBox textConnectIp1;
		private System.Windows.Forms.TextBox textConnectPort;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnDisconnect;
		private System.Windows.Forms.ListBox lbLog;
		private System.Windows.Forms.TextBox textMessage;
		private System.Windows.Forms.Label lbMessage;
		private System.Windows.Forms.Button btnSendMessage;
		private System.Windows.Forms.TextBox textNickName;
		private System.Windows.Forms.Label lbNickName;
		private System.Windows.Forms.Button btnFileSend;
		private System.Windows.Forms.GroupBox gbFileUpload;
		private System.Windows.Forms.Label lbFileName;
		private System.Windows.Forms.Button btnFileUpload;
		private System.Windows.Forms.Label lbFilelabel;
		private System.Windows.Forms.Button btnFileDownload;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox cbConnectPrivate;
		private System.Windows.Forms.Label lbIP1;
		private System.Windows.Forms.Label lbIP2;
		private System.Windows.Forms.TextBox textConnectIp2;
		private System.Windows.Forms.TextBox textConnectIp4;
		private System.Windows.Forms.TextBox textConnectIp3;
		private System.Windows.Forms.Label lbIP3;
		private System.Windows.Forms.Label lbFileProgress;
		private System.Windows.Forms.ProgressBar pbFileProgress;
	}
}

