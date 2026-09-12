namespace GF.BasicExample
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.lbStatus = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPublishIP = new System.Windows.Forms.TextBox();
            this.btnPublish = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.lbPrice = new System.Windows.Forms.Label();
            this.cbContract = new System.Windows.Forms.ComboBox();
            this.pOrderTicket = new System.Windows.Forms.FlowLayoutPanel();
            this.cbSide = new System.Windows.Forms.ComboBox();
            this.nQty = new System.Windows.Forms.NumericUpDown();
            this.cbType = new System.Windows.Forms.ComboBox();
            this.nPrice = new System.Windows.Forms.NumericUpDown();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lbOrders = new System.Windows.Forms.ListBox();
            this.lbPositions = new System.Windows.Forms.ListBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.pOrderTicket.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPrice)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer1.Size = new System.Drawing.Size(535, 153);
            this.splitContainer1.SplitterDistance = 120;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.label8);
            this.splitContainer2.Panel2.Controls.Add(this.txtPublishIP);
            this.splitContainer2.Panel2.Controls.Add(this.btnPublish);
            this.splitContainer2.Size = new System.Drawing.Size(535, 120);
            this.splitContainer2.SplitterDistance = 217;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbLogin, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbPassword, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnConnect, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnDisconnect, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbStatus, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(210, 114);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(66, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "Login";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(108, 4);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(94, 21);
            this.tbLogin.TabIndex = 1;
            this.tbLogin.Text = "CQiufu";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(48, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 27);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(108, 32);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(94, 21);
            this.tbPassword.TabIndex = 3;
            this.tbPassword.Text = "API69382";
            this.tbPassword.UseSystemPasswordChar = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(4, 60);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(97, 21);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisconnect.Location = new System.Drawing.Point(108, 60);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(98, 21);
            this.btnDisconnect.TabIndex = 5;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lbStatus, 2);
            this.lbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStatus.Location = new System.Drawing.Point(4, 85);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(202, 28);
            this.lbStatus.TabIndex = 6;
            this.lbStatus.Text = "Idle";
            this.lbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 12);
            this.label8.TabIndex = 20;
            this.label8.Text = "Publish Address";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPublishIP
            // 
            this.txtPublishIP.Font = new System.Drawing.Font("ËÎÌו", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPublishIP.Location = new System.Drawing.Point(99, 3);
            this.txtPublishIP.Name = "txtPublishIP";
            this.txtPublishIP.Size = new System.Drawing.Size(168, 21);
            this.txtPublishIP.TabIndex = 21;
            this.txtPublishIP.Text = "tcp://172.31.47.203:5556";
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(99, 35);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(79, 27);
            this.btnPublish.TabIndex = 22;
            this.btnPublish.Text = "Bind";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.lbPrice);
            this.splitContainer3.Panel1.Controls.Add(this.cbContract);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.pOrderTicket);
            this.splitContainer3.Size = new System.Drawing.Size(535, 29);
            this.splitContainer3.SplitterDistance = 25;
            this.splitContainer3.TabIndex = 0;
            this.splitContainer3.TabStop = false;
            // 
            // lbPrice
            // 
            this.lbPrice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbPrice.AutoSize = true;
            this.lbPrice.Location = new System.Drawing.Point(111, 6);
            this.lbPrice.Name = "lbPrice";
            this.lbPrice.Size = new System.Drawing.Size(77, 12);
            this.lbPrice.TabIndex = 6;
            this.lbPrice.Text = "Market Price";
            this.lbPrice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbContract
            // 
            this.cbContract.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbContract.FormattingEnabled = true;
            this.cbContract.Location = new System.Drawing.Point(9, 3);
            this.cbContract.Name = "cbContract";
            this.cbContract.Size = new System.Drawing.Size(96, 20);
            this.cbContract.TabIndex = 2;
            this.cbContract.SelectionChangeCommitted += new System.EventHandler(this.cbContract_SelectionChangeCommitted);
            // 
            // pOrderTicket
            // 
            this.pOrderTicket.AutoSize = true;
            this.pOrderTicket.Controls.Add(this.cbSide);
            this.pOrderTicket.Controls.Add(this.nQty);
            this.pOrderTicket.Controls.Add(this.cbType);
            this.pOrderTicket.Controls.Add(this.nPrice);
            this.pOrderTicket.Controls.Add(this.btnSubmit);
            this.pOrderTicket.Controls.Add(this.lbOrders);
            this.pOrderTicket.Controls.Add(this.lbPositions);
            this.pOrderTicket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pOrderTicket.Location = new System.Drawing.Point(0, 0);
            this.pOrderTicket.Name = "pOrderTicket";
            this.pOrderTicket.Size = new System.Drawing.Size(535, 98);
            this.pOrderTicket.TabIndex = 0;
            // 
            // cbSide
            // 
            this.cbSide.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSide.FormattingEnabled = true;
            this.cbSide.Location = new System.Drawing.Point(3, 3);
            this.cbSide.Name = "cbSide";
            this.cbSide.Size = new System.Drawing.Size(61, 20);
            this.cbSide.TabIndex = 0;
            this.cbSide.Visible = false;
            // 
            // nQty
            // 
            this.nQty.Location = new System.Drawing.Point(70, 3);
            this.nQty.Name = "nQty";
            this.nQty.Size = new System.Drawing.Size(56, 21);
            this.nQty.TabIndex = 1;
            this.nQty.Visible = false;
            // 
            // cbType
            // 
            this.cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbType.FormattingEnabled = true;
            this.cbType.Location = new System.Drawing.Point(132, 3);
            this.cbType.Name = "cbType";
            this.cbType.Size = new System.Drawing.Size(93, 20);
            this.cbType.TabIndex = 3;
            this.cbType.Visible = false;
            this.cbType.SelectionChangeCommitted += new System.EventHandler(this.cbType_SelectionChangeCommitted);
            // 
            // nPrice
            // 
            this.nPrice.DecimalPlaces = 2;
            this.nPrice.Enabled = false;
            this.nPrice.Location = new System.Drawing.Point(231, 3);
            this.nPrice.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nPrice.Name = "nPrice";
            this.nPrice.Size = new System.Drawing.Size(75, 21);
            this.nPrice.TabIndex = 4;
            this.nPrice.Visible = false;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(312, 2);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 21);
            this.btnSubmit.TabIndex = 5;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Visible = false;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // lbOrders
            // 
            this.lbOrders.FormattingEnabled = true;
            this.lbOrders.IntegralHeight = false;
            this.lbOrders.ItemHeight = 12;
            this.lbOrders.Location = new System.Drawing.Point(393, 3);
            this.lbOrders.Name = "lbOrders";
            this.lbOrders.Size = new System.Drawing.Size(81, 32);
            this.lbOrders.TabIndex = 0;
            this.lbOrders.Visible = false;
            this.lbOrders.DoubleClick += new System.EventHandler(this.lbOrders_DoubleClick);
            // 
            // lbPositions
            // 
            this.lbPositions.FormattingEnabled = true;
            this.lbPositions.IntegralHeight = false;
            this.lbPositions.ItemHeight = 12;
            this.lbPositions.Location = new System.Drawing.Point(3, 41);
            this.lbPositions.Name = "lbPositions";
            this.lbPositions.Size = new System.Drawing.Size(107, 30);
            this.lbPositions.TabIndex = 0;
            this.lbPositions.Visible = false;
            this.lbPositions.DoubleClick += new System.EventHandler(this.lbPositions_DoubleClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 153);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "GF.Publish.Server";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.pOrderTicket.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPrice)).EndInit();
            this.ResumeLayout(false);

		}

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbLogin;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbPassword;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnDisconnect;
		private System.Windows.Forms.Label lbStatus;
		private System.Windows.Forms.ListBox lbPositions;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.ListBox lbOrders;
		private System.Windows.Forms.FlowLayoutPanel pOrderTicket;
		private System.Windows.Forms.ComboBox cbSide;
		private System.Windows.Forms.NumericUpDown nQty;
		private System.Windows.Forms.ComboBox cbContract;
		private System.Windows.Forms.ComboBox cbType;
		private System.Windows.Forms.NumericUpDown nPrice;
		private System.Windows.Forms.Button btnSubmit;
		private System.Windows.Forms.Label lbPrice;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtPublishIP;
        private System.Windows.Forms.Button btnPublish;
    }
}

