namespace uClient.Broker
{
    partial class DataLoadForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_EAExecute = new System.Windows.Forms.TabPage();
            this.label_WZCollectionStatus = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label_JRCollectionStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_Platform = new System.Windows.Forms.Label();
            this.label_dataCollectionStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_saveAccount = new System.Windows.Forms.Button();
            this.button_AccountSave = new System.Windows.Forms.Button();
            this.label_BrokePrice = new System.Windows.Forms.Label();
            this.button_platformConnect = new System.Windows.Forms.Button();
            this.lblConnectStatus = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_account = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_PlatformConfig = new System.Windows.Forms.TabPage();
            this.button_savePlatformConfig = new System.Windows.Forms.Button();
            this.checkBox_NotifyFlag = new System.Windows.Forms.CheckBox();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lstTradeRecord = new System.Windows.Forms.ListBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.bgwBroberQuote = new System.ComponentModel.BackgroundWorker();
            this.timerCheckPlatformConnectStatus = new System.Windows.Forms.Timer(this.components);
            this.timer_JR = new System.Windows.Forms.Timer(this.components);
            this.timer_WZ = new System.Windows.Forms.Timer(this.components);
            this.timer_DataCenter = new System.Windows.Forms.Timer(this.components);
            this.timer_Dukascopy = new System.Windows.Forms.Timer(this.components);
            this.timer_KLineCheck = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage_EAExecute.SuspendLayout();
            this.tabPage_PlatformConfig.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_EAExecute);
            this.tabControl1.Controls.Add(this.tabPage_PlatformConfig);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(406, 257);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage_EAExecute
            // 
            this.tabPage_EAExecute.Controls.Add(this.label_WZCollectionStatus);
            this.tabPage_EAExecute.Controls.Add(this.label5);
            this.tabPage_EAExecute.Controls.Add(this.label_JRCollectionStatus);
            this.tabPage_EAExecute.Controls.Add(this.label4);
            this.tabPage_EAExecute.Controls.Add(this.label_Platform);
            this.tabPage_EAExecute.Controls.Add(this.label_dataCollectionStatus);
            this.tabPage_EAExecute.Controls.Add(this.label1);
            this.tabPage_EAExecute.Controls.Add(this.button_saveAccount);
            this.tabPage_EAExecute.Controls.Add(this.button_AccountSave);
            this.tabPage_EAExecute.Controls.Add(this.label_BrokePrice);
            this.tabPage_EAExecute.Controls.Add(this.button_platformConnect);
            this.tabPage_EAExecute.Controls.Add(this.lblConnectStatus);
            this.tabPage_EAExecute.Controls.Add(this.textBox_password);
            this.tabPage_EAExecute.Controls.Add(this.label2);
            this.tabPage_EAExecute.Controls.Add(this.textBox_account);
            this.tabPage_EAExecute.Controls.Add(this.label3);
            this.tabPage_EAExecute.Location = new System.Drawing.Point(4, 22);
            this.tabPage_EAExecute.Name = "tabPage_EAExecute";
            this.tabPage_EAExecute.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_EAExecute.Size = new System.Drawing.Size(398, 231);
            this.tabPage_EAExecute.TabIndex = 0;
            this.tabPage_EAExecute.Text = "EA运行";
            this.tabPage_EAExecute.UseVisualStyleBackColor = true;
            // 
            // label_WZCollectionStatus
            // 
            this.label_WZCollectionStatus.BackColor = System.Drawing.Color.Green;
            this.label_WZCollectionStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_WZCollectionStatus.ForeColor = System.Drawing.SystemColors.Window;
            this.label_WZCollectionStatus.Location = new System.Drawing.Point(150, 88);
            this.label_WZCollectionStatus.Name = "label_WZCollectionStatus";
            this.label_WZCollectionStatus.Size = new System.Drawing.Size(232, 28);
            this.label_WZCollectionStatus.TabIndex = 183;
            this.label_WZCollectionStatus.Text = "NoRunning";
            this.label_WZCollectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Khaki;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(4, 87);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 28);
            this.label5.TabIndex = 182;
            this.label5.Text = "WZ数据采集状态";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_JRCollectionStatus
            // 
            this.label_JRCollectionStatus.BackColor = System.Drawing.Color.Green;
            this.label_JRCollectionStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_JRCollectionStatus.ForeColor = System.Drawing.SystemColors.Window;
            this.label_JRCollectionStatus.Location = new System.Drawing.Point(150, 49);
            this.label_JRCollectionStatus.Name = "label_JRCollectionStatus";
            this.label_JRCollectionStatus.Size = new System.Drawing.Size(232, 28);
            this.label_JRCollectionStatus.TabIndex = 181;
            this.label_JRCollectionStatus.Text = "NoRunning";
            this.label_JRCollectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Khaki;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(6, 49);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 28);
            this.label4.TabIndex = 180;
            this.label4.Text = "JR数据采集状态";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_Platform
            // 
            this.label_Platform.BackColor = System.Drawing.Color.Khaki;
            this.label_Platform.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Platform.Location = new System.Drawing.Point(5, 160);
            this.label_Platform.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Platform.Name = "label_Platform";
            this.label_Platform.Size = new System.Drawing.Size(65, 24);
            this.label_Platform.TabIndex = 179;
            this.label_Platform.Text = "Platform";
            this.label_Platform.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_dataCollectionStatus
            // 
            this.label_dataCollectionStatus.BackColor = System.Drawing.Color.Green;
            this.label_dataCollectionStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_dataCollectionStatus.ForeColor = System.Drawing.SystemColors.Window;
            this.label_dataCollectionStatus.Location = new System.Drawing.Point(150, 10);
            this.label_dataCollectionStatus.Name = "label_dataCollectionStatus";
            this.label_dataCollectionStatus.Size = new System.Drawing.Size(232, 28);
            this.label_dataCollectionStatus.TabIndex = 178;
            this.label_dataCollectionStatus.Text = "NoRunning";
            this.label_dataCollectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Khaki;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 28);
            this.label1.TabIndex = 177;
            this.label1.Text = "报价数据采集状态";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_saveAccount
            // 
            this.button_saveAccount.BackColor = System.Drawing.Color.White;
            this.button_saveAccount.Location = new System.Drawing.Point(302, 190);
            this.button_saveAccount.Name = "button_saveAccount";
            this.button_saveAccount.Size = new System.Drawing.Size(60, 30);
            this.button_saveAccount.TabIndex = 171;
            this.button_saveAccount.Text = "保存";
            this.button_saveAccount.UseVisualStyleBackColor = false;
            this.button_saveAccount.Click += new System.EventHandler(this.button_AccountSave_Click);
            // 
            // button_AccountSave
            // 
            this.button_AccountSave.BackColor = System.Drawing.Color.White;
            this.button_AccountSave.Location = new System.Drawing.Point(1649, 7);
            this.button_AccountSave.Name = "button_AccountSave";
            this.button_AccountSave.Size = new System.Drawing.Size(60, 30);
            this.button_AccountSave.TabIndex = 168;
            this.button_AccountSave.Text = "保存";
            this.button_AccountSave.UseVisualStyleBackColor = false;
            this.button_AccountSave.Click += new System.EventHandler(this.button_AccountSave_Click);
            // 
            // label_BrokePrice
            // 
            this.label_BrokePrice.BackColor = System.Drawing.Color.Black;
            this.label_BrokePrice.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_BrokePrice.ForeColor = System.Drawing.Color.Red;
            this.label_BrokePrice.Location = new System.Drawing.Point(6, 190);
            this.label_BrokePrice.Name = "label_BrokePrice";
            this.label_BrokePrice.Size = new System.Drawing.Size(100, 28);
            this.label_BrokePrice.TabIndex = 165;
            this.label_BrokePrice.Text = "0";
            this.label_BrokePrice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button_platformConnect
            // 
            this.button_platformConnect.BackColor = System.Drawing.Color.Red;
            this.button_platformConnect.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_platformConnect.ForeColor = System.Drawing.SystemColors.Info;
            this.button_platformConnect.Location = new System.Drawing.Point(117, 187);
            this.button_platformConnect.Margin = new System.Windows.Forms.Padding(4);
            this.button_platformConnect.Name = "button_platformConnect";
            this.button_platformConnect.Size = new System.Drawing.Size(97, 35);
            this.button_platformConnect.TabIndex = 163;
            this.button_platformConnect.Text = "平台连接";
            this.button_platformConnect.UseVisualStyleBackColor = false;
            this.button_platformConnect.Click += new System.EventHandler(this.button_platformConnect_Click);
            // 
            // lblConnectStatus
            // 
            this.lblConnectStatus.BackColor = System.Drawing.Color.Red;
            this.lblConnectStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblConnectStatus.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblConnectStatus.Location = new System.Drawing.Point(222, 191);
            this.lblConnectStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectStatus.Name = "lblConnectStatus";
            this.lblConnectStatus.Size = new System.Drawing.Size(73, 28);
            this.lblConnectStatus.TabIndex = 162;
            this.lblConnectStatus.Text = "未登录";
            this.lblConnectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_password
            // 
            this.textBox_password.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_password.Location = new System.Drawing.Point(285, 159);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(88, 25);
            this.textBox_password.TabIndex = 160;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Khaki;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(236, 159);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 24);
            this.label2.TabIndex = 159;
            this.label2.Text = "密码";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_account
            // 
            this.textBox_account.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_account.Location = new System.Drawing.Point(128, 159);
            this.textBox_account.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_account.Name = "textBox_account";
            this.textBox_account.Size = new System.Drawing.Size(101, 25);
            this.textBox_account.TabIndex = 158;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Khaki;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(78, 159);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 24);
            this.label3.TabIndex = 157;
            this.label3.Text = "账号";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage_PlatformConfig
            // 
            this.tabPage_PlatformConfig.Controls.Add(this.button_savePlatformConfig);
            this.tabPage_PlatformConfig.Controls.Add(this.checkBox_NotifyFlag);
            this.tabPage_PlatformConfig.Location = new System.Drawing.Point(4, 22);
            this.tabPage_PlatformConfig.Name = "tabPage_PlatformConfig";
            this.tabPage_PlatformConfig.Size = new System.Drawing.Size(398, 231);
            this.tabPage_PlatformConfig.TabIndex = 3;
            this.tabPage_PlatformConfig.Text = "平台设置";
            this.tabPage_PlatformConfig.UseVisualStyleBackColor = true;
            // 
            // button_savePlatformConfig
            // 
            this.button_savePlatformConfig.BackColor = System.Drawing.Color.White;
            this.button_savePlatformConfig.Location = new System.Drawing.Point(1065, 6);
            this.button_savePlatformConfig.Name = "button_savePlatformConfig";
            this.button_savePlatformConfig.Size = new System.Drawing.Size(60, 30);
            this.button_savePlatformConfig.TabIndex = 48;
            this.button_savePlatformConfig.Text = "保存";
            this.button_savePlatformConfig.UseVisualStyleBackColor = false;
            this.button_savePlatformConfig.Click += new System.EventHandler(this.button_savePlatformConfig_Click);
            // 
            // checkBox_NotifyFlag
            // 
            this.checkBox_NotifyFlag.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_NotifyFlag.AutoSize = true;
            this.checkBox_NotifyFlag.Location = new System.Drawing.Point(5, 7);
            this.checkBox_NotifyFlag.Name = "checkBox_NotifyFlag";
            this.checkBox_NotifyFlag.Size = new System.Drawing.Size(72, 16);
            this.checkBox_NotifyFlag.TabIndex = 43;
            this.checkBox_NotifyFlag.Text = "提醒通知";
            this.checkBox_NotifyFlag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox_NotifyFlag.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl2.Location = new System.Drawing.Point(418, 25);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(428, 229);
            this.tabControl2.TabIndex = 7;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lstTradeRecord);
            this.tabPage4.Location = new System.Drawing.Point(4, 28);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(420, 197);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "执行记录";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // lstTradeRecord
            // 
            this.lstTradeRecord.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstTradeRecord.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lstTradeRecord.FormattingEnabled = true;
            this.lstTradeRecord.HorizontalScrollbar = true;
            this.lstTradeRecord.ItemHeight = 17;
            this.lstTradeRecord.Location = new System.Drawing.Point(1, 3);
            this.lstTradeRecord.Margin = new System.Windows.Forms.Padding(4);
            this.lstTradeRecord.Name = "lstTradeRecord";
            this.lstTradeRecord.ScrollAlwaysVisible = true;
            this.lstTradeRecord.Size = new System.Drawing.Size(419, 187);
            this.lstTradeRecord.TabIndex = 5;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.lstLog);
            this.tabPage5.Location = new System.Drawing.Point(4, 28);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(420, 197);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "日志";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // lstLog
            // 
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.ItemHeight = 16;
            this.lstLog.Location = new System.Drawing.Point(3, 2);
            this.lstLog.Margin = new System.Windows.Forms.Padding(4);
            this.lstLog.Name = "lstLog";
            this.lstLog.ScrollAlwaysVisible = true;
            this.lstLog.Size = new System.Drawing.Size(421, 192);
            this.lstLog.TabIndex = 4;
            // 
            // bgwBroberQuote
            // 
            this.bgwBroberQuote.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwBroberQuote_DoWork);
            // 
            // timerCheckPlatformConnectStatus
            // 
            this.timerCheckPlatformConnectStatus.Interval = 60000;
            this.timerCheckPlatformConnectStatus.Tick += new System.EventHandler(this.timerCheckPlatformConnectStatus_Tick);
            // 
            // timer_JR
            // 
            this.timer_JR.Tick += new System.EventHandler(this.timer_JR_Tick);
            // 
            // timer_WZ
            // 
            this.timer_WZ.Tick += new System.EventHandler(this.timer_WZ_Tick);
            // 
            // timer_DataCenter
            // 
            this.timer_DataCenter.Tick += new System.EventHandler(this.timer_DataCenter_Tick);
            // 
            // timer_Dukascopy
            // 
            this.timer_Dukascopy.Tick += new System.EventHandler(this.timer_Dukascopy_Tick);
            // 
            // timer_KLineCheck
            // 
            this.timer_KLineCheck.Tick += new System.EventHandler(this.timer_KLineCheck_Tick);
            // 
            // DataLoadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 267);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DataLoadForm";
            this.Text = "数据采集运行平台";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DataLoadForm_Closed);
            this.Load += new System.EventHandler(this.DataLoadForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_EAExecute.ResumeLayout(false);
            this.tabPage_EAExecute.PerformLayout();
            this.tabPage_PlatformConfig.ResumeLayout(false);
            this.tabPage_PlatformConfig.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_EAExecute;
        public System.Windows.Forms.TabControl tabControl2;
        public System.Windows.Forms.TabPage tabPage4;
        public System.Windows.Forms.ListBox lstTradeRecord;
        public System.Windows.Forms.TabPage tabPage5;
        public System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.TabPage tabPage_PlatformConfig;
        public System.Windows.Forms.TextBox textBox_password;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox_account;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label lblConnectStatus;
        public System.Windows.Forms.Button button_platformConnect;
        private System.Windows.Forms.Label label_BrokePrice;
        private System.ComponentModel.BackgroundWorker bgwBroberQuote;
        public System.Windows.Forms.Button button_AccountSave;
        public System.Windows.Forms.CheckBox checkBox_NotifyFlag;
        public System.Windows.Forms.Button button_savePlatformConfig;
        public System.Windows.Forms.Button button_saveAccount;
        private System.Windows.Forms.Label label_dataCollectionStatus;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label_Platform;
        private System.Windows.Forms.Timer timerCheckPlatformConnectStatus;
        private System.Windows.Forms.Label label_WZCollectionStatus;
        public System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_JRCollectionStatus;
        public System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer_JR;
        private System.Windows.Forms.Timer timer_WZ;
        private System.Windows.Forms.Timer timer_DataCenter;
        private System.Windows.Forms.Timer timer_Dukascopy;
        private System.Windows.Forms.Timer timer_KLineCheck;
    }
}

