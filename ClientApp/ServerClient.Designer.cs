namespace ServerApp
{
	partial class ServerClient
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
			this.components = new System.ComponentModel.Container();
			this.gbServer = new System.Windows.Forms.GroupBox();
			this.labelPort = new System.Windows.Forms.Label();
			this.textPort = new System.Windows.Forms.TextBox();
			this.btnServerEnd = new System.Windows.Forms.Button();
			this.btnServerStart = new System.Windows.Forms.Button();
			this.gbConnectingList = new System.Windows.Forms.GroupBox();
			this.lbConnectingList = new System.Windows.Forms.ListBox();
			this.gbLog = new System.Windows.Forms.GroupBox();
			this.lbLog = new System.Windows.Forms.ListBox();
			this.ssServerStatus = new System.Windows.Forms.StatusStrip();
			this.ssServerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.CheckConnectList = new System.Windows.Forms.Timer(this.components);
			this.cbIPCheck = new System.Windows.Forms.CheckBox();
			this.cbOnlyLocalIP = new System.Windows.Forms.CheckBox();
			this.gbServer.SuspendLayout();
			this.gbConnectingList.SuspendLayout();
			this.gbLog.SuspendLayout();
			this.ssServerStatus.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbServer
			// 
			this.gbServer.Controls.Add(this.labelPort);
			this.gbServer.Controls.Add(this.textPort);
			this.gbServer.Controls.Add(this.btnServerEnd);
			this.gbServer.Controls.Add(this.btnServerStart);
			this.gbServer.Location = new System.Drawing.Point(12, 21);
			this.gbServer.Name = "gbServer";
			this.gbServer.Size = new System.Drawing.Size(617, 74);
			this.gbServer.TabIndex = 1;
			this.gbServer.TabStop = false;
			this.gbServer.Text = "서버설정";
			// 
			// labelPort
			// 
			this.labelPort.AutoSize = true;
			this.labelPort.Location = new System.Drawing.Point(6, 30);
			this.labelPort.Name = "labelPort";
			this.labelPort.Size = new System.Drawing.Size(136, 12);
			this.labelPort.TabIndex = 3;
			this.labelPort.Text = "서버 포트번호(0~65335)";
			// 
			// textPort
			// 
			this.textPort.Location = new System.Drawing.Point(163, 28);
			this.textPort.Name = "textPort";
			this.textPort.Size = new System.Drawing.Size(180, 21);
			this.textPort.TabIndex = 2;
			this.textPort.Text = "33306";
			// 
			// btnServerEnd
			// 
			this.btnServerEnd.Enabled = false;
			this.btnServerEnd.Location = new System.Drawing.Point(536, 13);
			this.btnServerEnd.Name = "btnServerEnd";
			this.btnServerEnd.Size = new System.Drawing.Size(75, 46);
			this.btnServerEnd.TabIndex = 1;
			this.btnServerEnd.Text = "서버 종료";
			this.btnServerEnd.UseVisualStyleBackColor = true;
			this.btnServerEnd.Click += new System.EventHandler(this.btnServerEnd_Click);
			// 
			// btnServerStart
			// 
			this.btnServerStart.Location = new System.Drawing.Point(454, 13);
			this.btnServerStart.Name = "btnServerStart";
			this.btnServerStart.Size = new System.Drawing.Size(76, 46);
			this.btnServerStart.TabIndex = 0;
			this.btnServerStart.Text = "서버 시작";
			this.btnServerStart.UseVisualStyleBackColor = true;
			this.btnServerStart.Click += new System.EventHandler(this.btnServerStart_Click);
			// 
			// gbConnectingList
			// 
			this.gbConnectingList.Controls.Add(this.lbConnectingList);
			this.gbConnectingList.Location = new System.Drawing.Point(12, 101);
			this.gbConnectingList.Name = "gbConnectingList";
			this.gbConnectingList.Size = new System.Drawing.Size(617, 108);
			this.gbConnectingList.TabIndex = 0;
			this.gbConnectingList.TabStop = false;
			this.gbConnectingList.Text = "접속리스트";
			// 
			// lbConnectingList
			// 
			this.lbConnectingList.FormattingEnabled = true;
			this.lbConnectingList.ItemHeight = 12;
			this.lbConnectingList.Location = new System.Drawing.Point(8, 14);
			this.lbConnectingList.Name = "lbConnectingList";
			this.lbConnectingList.Size = new System.Drawing.Size(603, 88);
			this.lbConnectingList.TabIndex = 0;
			// 
			// gbLog
			// 
			this.gbLog.Controls.Add(this.lbLog);
			this.gbLog.Location = new System.Drawing.Point(12, 215);
			this.gbLog.Name = "gbLog";
			this.gbLog.Size = new System.Drawing.Size(617, 166);
			this.gbLog.TabIndex = 2;
			this.gbLog.TabStop = false;
			this.gbLog.Text = "로그";
			// 
			// lbLog
			// 
			this.lbLog.FormattingEnabled = true;
			this.lbLog.ItemHeight = 12;
			this.lbLog.Location = new System.Drawing.Point(6, 20);
			this.lbLog.Name = "lbLog";
			this.lbLog.Size = new System.Drawing.Size(605, 136);
			this.lbLog.TabIndex = 0;
			// 
			// ssServerStatus
			// 
			this.ssServerStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ssServerStatusLabel});
			this.ssServerStatus.Location = new System.Drawing.Point(0, 401);
			this.ssServerStatus.Name = "ssServerStatus";
			this.ssServerStatus.Size = new System.Drawing.Size(641, 22);
			this.ssServerStatus.TabIndex = 3;
			// 
			// ssServerStatusLabel
			// 
			this.ssServerStatusLabel.Name = "ssServerStatusLabel";
			this.ssServerStatusLabel.Size = new System.Drawing.Size(59, 17);
			this.ssServerStatusLabel.Text = "서버 상태";
			// 
			// CheckConnectList
			// 
			this.CheckConnectList.Interval = 5000;
			this.CheckConnectList.Tick += new System.EventHandler(this.CheckConnectList_Tick);
			// 
			// cbIPCheck
			// 
			this.cbIPCheck.AutoSize = true;
			this.cbIPCheck.Checked = true;
			this.cbIPCheck.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbIPCheck.Location = new System.Drawing.Point(519, 5);
			this.cbIPCheck.Name = "cbIPCheck";
			this.cbIPCheck.Size = new System.Drawing.Size(112, 16);
			this.cbIPCheck.TabIndex = 5;
			this.cbIPCheck.Text = "해외아이피 차단";
			this.cbIPCheck.UseVisualStyleBackColor = true;
			this.cbIPCheck.CheckedChanged += new System.EventHandler(this.cbIPCheck_CheckedChanged);
			// 
			// cbOnlyLocalIP
			// 
			this.cbOnlyLocalIP.AutoSize = true;
			this.cbOnlyLocalIP.Location = new System.Drawing.Point(383, 5);
			this.cbOnlyLocalIP.Name = "cbOnlyLocalIP";
			this.cbOnlyLocalIP.Size = new System.Drawing.Size(130, 16);
			this.cbOnlyLocalIP.TabIndex = 6;
			this.cbOnlyLocalIP.Text = "내부망(비공개)설정";
			this.cbOnlyLocalIP.UseVisualStyleBackColor = true;
			this.cbOnlyLocalIP.CheckedChanged += new System.EventHandler(this.cbOnlyLocalIP_CheckedChanged);
			// 
			// ServerClient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(641, 423);
			this.Controls.Add(this.cbOnlyLocalIP);
			this.Controls.Add(this.cbIPCheck);
			this.Controls.Add(this.ssServerStatus);
			this.Controls.Add(this.gbLog);
			this.Controls.Add(this.gbConnectingList);
			this.Controls.Add(this.gbServer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "ServerClient";
			this.Text = "Server";
			this.Load += new System.EventHandler(this.ServerClient_Load);
			this.gbServer.ResumeLayout(false);
			this.gbServer.PerformLayout();
			this.gbConnectingList.ResumeLayout(false);
			this.gbLog.ResumeLayout(false);
			this.ssServerStatus.ResumeLayout(false);
			this.ssServerStatus.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.GroupBox gbServer;
		private System.Windows.Forms.GroupBox gbConnectingList;
		private System.Windows.Forms.Button btnServerStart;
		private System.Windows.Forms.Label labelPort;
		private System.Windows.Forms.TextBox textPort;
		private System.Windows.Forms.Button btnServerEnd;
		private System.Windows.Forms.ListBox lbConnectingList;
		private System.Windows.Forms.GroupBox gbLog;
		private System.Windows.Forms.ListBox lbLog;
		private System.Windows.Forms.StatusStrip ssServerStatus;
		private System.Windows.Forms.ToolStripStatusLabel ssServerStatusLabel;
		private System.Windows.Forms.Timer CheckConnectList;
		private System.Windows.Forms.CheckBox cbIPCheck;
		private System.Windows.Forms.CheckBox cbOnlyLocalIP;
	}
}

