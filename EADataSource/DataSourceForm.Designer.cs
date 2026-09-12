namespace uClient.Broker
{
    partial class DataSourceForm
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
            this.comboBox_Broker = new System.Windows.Forms.ComboBox();
            this.button_saveAccount = new System.Windows.Forms.Button();
            this.button_AccountSave = new System.Windows.Forms.Button();
            this.label_BrokePrice = new System.Windows.Forms.Label();
            this.button_platformConnect = new System.Windows.Forms.Button();
            this.lblConnectStatus = new System.Windows.Forms.Label();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_UserCode = new System.Windows.Forms.TextBox();
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
            this.tabPage_EAExecute.Controls.Add(this.comboBox_Broker);
            this.tabPage_EAExecute.Controls.Add(this.button_saveAccount);
            this.tabPage_EAExecute.Controls.Add(this.button_AccountSave);
            this.tabPage_EAExecute.Controls.Add(this.label_BrokePrice);
            this.tabPage_EAExecute.Controls.Add(this.button_platformConnect);
            this.tabPage_EAExecute.Controls.Add(this.lblConnectStatus);
            this.tabPage_EAExecute.Controls.Add(this.textBox_Password);
            this.tabPage_EAExecute.Controls.Add(this.label2);
            this.tabPage_EAExecute.Controls.Add(this.textBox_UserCode);
            this.tabPage_EAExecute.Controls.Add(this.label3);
            this.tabPage_EAExecute.Location = new System.Drawing.Point(4, 22);
            this.tabPage_EAExecute.Name = "tabPage_EAExecute";
            this.tabPage_EAExecute.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_EAExecute.Size = new System.Drawing.Size(398, 231);
            this.tabPage_EAExecute.TabIndex = 0;
            this.tabPage_EAExecute.Text = "EA运行";
            this.tabPage_EAExecute.UseVisualStyleBackColor = true;
            // 
            // comboBox_Broker
            // 
            this.comboBox_Broker.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F);
            this.comboBox_Broker.FormattingEnabled = true;
            this.comboBox_Broker.ItemHeight = 16;
            this.comboBox_Broker.Location = new System.Drawing.Point(5, 8);
            this.comboBox_Broker.Name = "comboBox_Broker";
            this.comboBox_Broker.Size = new System.Drawing.Size(88, 24);
            this.comboBox_Broker.TabIndex = 180;
            this.comboBox_Broker.SelectedIndexChanged += new System.EventHandler(this.comboBox_Broker_SelectedIndexChanged);
            // 
            // button_saveAccount
            // 
            this.button_saveAccount.BackColor = System.Drawing.Color.White;
            this.button_saveAccount.Location = new System.Drawing.Point(302, 39);
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
            this.label_BrokePrice.Location = new System.Drawing.Point(6, 39);
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
            this.button_platformConnect.Location = new System.Drawing.Point(117, 36);
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
            this.lblConnectStatus.Location = new System.Drawing.Point(222, 40);
            this.lblConnectStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectStatus.Name = "lblConnectStatus";
            this.lblConnectStatus.Size = new System.Drawing.Size(73, 28);
            this.lblConnectStatus.TabIndex = 162;
            this.lblConnectStatus.Text = "未登录";
            this.lblConnectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_Password
            // 
            this.textBox_Password.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_Password.Location = new System.Drawing.Point(304, 8);
            this.textBox_Password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.PasswordChar = '*';
            this.textBox_Password.Size = new System.Drawing.Size(88, 25);
            this.textBox_Password.TabIndex = 160;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Khaki;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(255, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 24);
            this.label2.TabIndex = 159;
            this.label2.Text = "密码";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_UserCode
            // 
            this.textBox_UserCode.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_UserCode.Location = new System.Drawing.Point(147, 8);
            this.textBox_UserCode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_UserCode.Name = "textBox_UserCode";
            this.textBox_UserCode.Size = new System.Drawing.Size(101, 25);
            this.textBox_UserCode.TabIndex = 158;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Khaki;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(97, 8);
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
            // DataSourceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(857, 267);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DataSourceForm";
            this.Text = "策略数据源平台";
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
        public System.Windows.Forms.TextBox textBox_Password;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox_UserCode;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label lblConnectStatus;
        public System.Windows.Forms.Button button_platformConnect;
        private System.Windows.Forms.Label label_BrokePrice;
        private System.ComponentModel.BackgroundWorker bgwBroberQuote;
        public System.Windows.Forms.Button button_AccountSave;
        public System.Windows.Forms.CheckBox checkBox_NotifyFlag;
        public System.Windows.Forms.Button button_savePlatformConfig;
        public System.Windows.Forms.Button button_saveAccount;
        private System.Windows.Forms.Timer timerCheckPlatformConnectStatus;
        private System.Windows.Forms.ComboBox comboBox_Broker;
    }
}

