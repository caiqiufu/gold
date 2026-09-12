namespace uClient.Broker
{
    partial class AutoTradeForm
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
            this.label8 = new System.Windows.Forms.Label();
            this.button_executeAnalysis = new System.Windows.Forms.Button();
            this.comboBox_LastBetTime = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_MaxBets = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_lastMaxBetAmount = new System.Windows.Forms.NumericUpDown();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lstTradeRecord = new System.Windows.Forms.ListBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.comboBox_LastBetTimeDuration = new System.Windows.Forms.ComboBox();
            this.textBox_RaceTime = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.listTicketsList = new System.Windows.Forms.ListBox();
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
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_raceResult = new System.Windows.Forms.TextBox();
            this.textBox_filePatch = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_simulateTest = new System.Windows.Forms.Button();
            this.button_savePlatformConfig = new System.Windows.Forms.Button();
            this.checkBox_NotifyFlag = new System.Windows.Forms.CheckBox();
            this.bgwBroberQuote = new System.ComponentModel.BackgroundWorker();
            this.timerCheckPlatformConnectStatus = new System.Windows.Forms.Timer(this.components);
            this.timer_TicketList = new System.Windows.Forms.Timer(this.components);
            this.timer_Token = new System.Windows.Forms.Timer(this.components);
            this.timer_DataCenter = new System.Windows.Forms.Timer(this.components);
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_testRaceTime = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_EAExecute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_MaxBets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_lastMaxBetAmount)).BeginInit();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage_PlatformConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_EAExecute);
            this.tabControl1.Controls.Add(this.tabPage_PlatformConfig);
            this.tabControl1.Location = new System.Drawing.Point(3, -1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(763, 507);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage_EAExecute
            // 
            this.tabPage_EAExecute.Controls.Add(this.label8);
            this.tabPage_EAExecute.Controls.Add(this.button_executeAnalysis);
            this.tabPage_EAExecute.Controls.Add(this.comboBox_LastBetTime);
            this.tabPage_EAExecute.Controls.Add(this.label6);
            this.tabPage_EAExecute.Controls.Add(this.label1);
            this.tabPage_EAExecute.Controls.Add(this.numericUpDown_MaxBets);
            this.tabPage_EAExecute.Controls.Add(this.numericUpDown_lastMaxBetAmount);
            this.tabPage_EAExecute.Controls.Add(this.tabControl2);
            this.tabPage_EAExecute.Controls.Add(this.label17);
            this.tabPage_EAExecute.Controls.Add(this.label16);
            this.tabPage_EAExecute.Controls.Add(this.comboBox_LastBetTimeDuration);
            this.tabPage_EAExecute.Controls.Add(this.textBox_RaceTime);
            this.tabPage_EAExecute.Controls.Add(this.label15);
            this.tabPage_EAExecute.Controls.Add(this.listTicketsList);
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
            this.tabPage_EAExecute.Size = new System.Drawing.Size(755, 481);
            this.tabPage_EAExecute.TabIndex = 0;
            this.tabPage_EAExecute.Text = "交易执行";
            this.tabPage_EAExecute.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(575, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 215;
            this.label8.Text = "(12:00:00)";
            // 
            // button_executeAnalysis
            // 
            this.button_executeAnalysis.BackColor = System.Drawing.Color.White;
            this.button_executeAnalysis.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_executeAnalysis.Location = new System.Drawing.Point(661, 10);
            this.button_executeAnalysis.Name = "button_executeAnalysis";
            this.button_executeAnalysis.Size = new System.Drawing.Size(84, 30);
            this.button_executeAnalysis.TabIndex = 214;
            this.button_executeAnalysis.Text = "启动执行";
            this.button_executeAnalysis.UseVisualStyleBackColor = false;
            this.button_executeAnalysis.Click += new System.EventHandler(this.button_executeAnalysis_Click);
            // 
            // comboBox_LastBetTime
            // 
            this.comboBox_LastBetTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_LastBetTime.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_LastBetTime.FormattingEnabled = true;
            this.comboBox_LastBetTime.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.comboBox_LastBetTime.Location = new System.Drawing.Point(577, 77);
            this.comboBox_LastBetTime.Name = "comboBox_LastBetTime";
            this.comboBox_LastBetTime.Size = new System.Drawing.Size(61, 24);
            this.comboBox_LastBetTime.TabIndex = 213;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Khaki;
            this.label6.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(479, 77);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 24);
            this.label6.TabIndex = 212;
            this.label6.Text = "最后跟注时间";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Khaki;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(298, 80);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 24);
            this.label1.TabIndex = 211;
            this.label1.Text = "最大跟注数量";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDown_MaxBets
            // 
            this.numericUpDown_MaxBets.Location = new System.Drawing.Point(407, 81);
            this.numericUpDown_MaxBets.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDown_MaxBets.Name = "numericUpDown_MaxBets";
            this.numericUpDown_MaxBets.Size = new System.Drawing.Size(68, 21);
            this.numericUpDown_MaxBets.TabIndex = 210;
            this.numericUpDown_MaxBets.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // numericUpDown_lastMaxBetAmount
            // 
            this.numericUpDown_lastMaxBetAmount.Location = new System.Drawing.Point(407, 46);
            this.numericUpDown_lastMaxBetAmount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_lastMaxBetAmount.Name = "numericUpDown_lastMaxBetAmount";
            this.numericUpDown_lastMaxBetAmount.Size = new System.Drawing.Size(68, 21);
            this.numericUpDown_lastMaxBetAmount.TabIndex = 209;
            this.numericUpDown_lastMaxBetAmount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl2.Location = new System.Drawing.Point(296, 119);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(453, 360);
            this.tabControl2.TabIndex = 7;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lstTradeRecord);
            this.tabPage4.Location = new System.Drawing.Point(4, 28);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(445, 328);
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
            this.lstTradeRecord.Location = new System.Drawing.Point(6, 7);
            this.lstTradeRecord.Margin = new System.Windows.Forms.Padding(4);
            this.lstTradeRecord.Name = "lstTradeRecord";
            this.lstTradeRecord.ScrollAlwaysVisible = true;
            this.lstTradeRecord.Size = new System.Drawing.Size(448, 323);
            this.lstTradeRecord.TabIndex = 5;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.lstLog);
            this.tabPage5.Location = new System.Drawing.Point(4, 28);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(445, 328);
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
            this.lstLog.Location = new System.Drawing.Point(4, 4);
            this.lstLog.Margin = new System.Windows.Forms.Padding(4);
            this.lstLog.Name = "lstLog";
            this.lstLog.ScrollAlwaysVisible = true;
            this.lstLog.Size = new System.Drawing.Size(456, 320);
            this.lstLog.TabIndex = 4;
            // 
            // label17
            // 
            this.label17.BackColor = System.Drawing.Color.Khaki;
            this.label17.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.Location = new System.Drawing.Point(298, 44);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(102, 24);
            this.label17.TabIndex = 207;
            this.label17.Text = "最小跟注金额";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label16
            // 
            this.label16.BackColor = System.Drawing.Color.Khaki;
            this.label16.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.Location = new System.Drawing.Point(479, 44);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(91, 24);
            this.label16.TabIndex = 206;
            this.label16.Text = "跟注时间段";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBox_LastBetTimeDuration
            // 
            this.comboBox_LastBetTimeDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_LastBetTimeDuration.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_LastBetTimeDuration.FormattingEnabled = true;
            this.comboBox_LastBetTimeDuration.Items.AddRange(new object[] {
            "30",
            "35",
            "40",
            "45",
            "50",
            "55",
            "60"});
            this.comboBox_LastBetTimeDuration.Location = new System.Drawing.Point(577, 46);
            this.comboBox_LastBetTimeDuration.Name = "comboBox_LastBetTimeDuration";
            this.comboBox_LastBetTimeDuration.Size = new System.Drawing.Size(61, 24);
            this.comboBox_LastBetTimeDuration.TabIndex = 205;
            // 
            // textBox_RaceTime
            // 
            this.textBox_RaceTime.Location = new System.Drawing.Point(394, 10);
            this.textBox_RaceTime.Multiline = true;
            this.textBox_RaceTime.Name = "textBox_RaceTime";
            this.textBox_RaceTime.Size = new System.Drawing.Size(176, 25);
            this.textBox_RaceTime.TabIndex = 204;
            // 
            // label15
            // 
            this.label15.BackColor = System.Drawing.Color.Khaki;
            this.label15.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label15.Location = new System.Drawing.Point(298, 10);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(89, 24);
            this.label15.TabIndex = 203;
            this.label15.Text = "本次赛事时间";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listTicketsList
            // 
            this.listTicketsList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listTicketsList.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listTicketsList.FormattingEnabled = true;
            this.listTicketsList.HorizontalScrollbar = true;
            this.listTicketsList.ItemHeight = 17;
            this.listTicketsList.Location = new System.Drawing.Point(4, 80);
            this.listTicketsList.Margin = new System.Windows.Forms.Padding(4);
            this.listTicketsList.Name = "listTicketsList";
            this.listTicketsList.ScrollAlwaysVisible = true;
            this.listTicketsList.Size = new System.Drawing.Size(285, 391);
            this.listTicketsList.TabIndex = 6;
            // 
            // button_saveAccount
            // 
            this.button_saveAccount.BackColor = System.Drawing.Color.White;
            this.button_saveAccount.Location = new System.Drawing.Point(685, 83);
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
            this.label_BrokePrice.Location = new System.Drawing.Point(6, 40);
            this.label_BrokePrice.Name = "label_BrokePrice";
            this.label_BrokePrice.Size = new System.Drawing.Size(144, 28);
            this.label_BrokePrice.TabIndex = 165;
            this.label_BrokePrice.Text = "0";
            this.label_BrokePrice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button_platformConnect
            // 
            this.button_platformConnect.BackColor = System.Drawing.Color.Red;
            this.button_platformConnect.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_platformConnect.ForeColor = System.Drawing.SystemColors.Info;
            this.button_platformConnect.Location = new System.Drawing.Point(209, 37);
            this.button_platformConnect.Margin = new System.Windows.Forms.Padding(4);
            this.button_platformConnect.Name = "button_platformConnect";
            this.button_platformConnect.Size = new System.Drawing.Size(82, 35);
            this.button_platformConnect.TabIndex = 163;
            this.button_platformConnect.Text = "数据源连接";
            this.button_platformConnect.UseVisualStyleBackColor = false;
            this.button_platformConnect.Click += new System.EventHandler(this.button_platformConnect_Click);
            // 
            // lblConnectStatus
            // 
            this.lblConnectStatus.BackColor = System.Drawing.Color.Red;
            this.lblConnectStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblConnectStatus.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblConnectStatus.Location = new System.Drawing.Point(152, 40);
            this.lblConnectStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectStatus.Name = "lblConnectStatus";
            this.lblConnectStatus.Size = new System.Drawing.Size(59, 28);
            this.lblConnectStatus.TabIndex = 162;
            this.lblConnectStatus.Text = "未登录";
            this.lblConnectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_password
            // 
            this.textBox_password.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_password.Location = new System.Drawing.Point(193, 9);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(96, 25);
            this.textBox_password.TabIndex = 160;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Khaki;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(144, 9);
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
            this.textBox_account.Location = new System.Drawing.Point(55, 9);
            this.textBox_account.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_account.Name = "textBox_account";
            this.textBox_account.Size = new System.Drawing.Size(84, 25);
            this.textBox_account.TabIndex = 158;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Khaki;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 24);
            this.label3.TabIndex = 157;
            this.label3.Text = "账号";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage_PlatformConfig
            // 
            this.tabPage_PlatformConfig.Controls.Add(this.label10);
            this.tabPage_PlatformConfig.Controls.Add(this.label9);
            this.tabPage_PlatformConfig.Controls.Add(this.textBox_testRaceTime);
            this.tabPage_PlatformConfig.Controls.Add(this.label7);
            this.tabPage_PlatformConfig.Controls.Add(this.label5);
            this.tabPage_PlatformConfig.Controls.Add(this.textBox_raceResult);
            this.tabPage_PlatformConfig.Controls.Add(this.textBox_filePatch);
            this.tabPage_PlatformConfig.Controls.Add(this.label4);
            this.tabPage_PlatformConfig.Controls.Add(this.button_simulateTest);
            this.tabPage_PlatformConfig.Controls.Add(this.button_savePlatformConfig);
            this.tabPage_PlatformConfig.Controls.Add(this.checkBox_NotifyFlag);
            this.tabPage_PlatformConfig.Location = new System.Drawing.Point(4, 22);
            this.tabPage_PlatformConfig.Name = "tabPage_PlatformConfig";
            this.tabPage_PlatformConfig.Size = new System.Drawing.Size(755, 481);
            this.tabPage_PlatformConfig.TabIndex = 3;
            this.tabPage_PlatformConfig.Text = "平台设置";
            this.tabPage_PlatformConfig.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(399, 8);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 24);
            this.label7.TabIndex = 216;
            this.label7.Text = "(20250205-1.txt)";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Khaki;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(6, 75);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 24);
            this.label5.TabIndex = 215;
            this.label5.Text = "赛果";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_raceResult
            // 
            this.textBox_raceResult.Location = new System.Drawing.Point(84, 75);
            this.textBox_raceResult.Multiline = true;
            this.textBox_raceResult.Name = "textBox_raceResult";
            this.textBox_raceResult.Size = new System.Drawing.Size(308, 70);
            this.textBox_raceResult.TabIndex = 214;
            // 
            // textBox_filePatch
            // 
            this.textBox_filePatch.Location = new System.Drawing.Point(84, 8);
            this.textBox_filePatch.Multiline = true;
            this.textBox_filePatch.Name = "textBox_filePatch";
            this.textBox_filePatch.Size = new System.Drawing.Size(308, 25);
            this.textBox_filePatch.TabIndex = 213;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Khaki;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(6, 8);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 24);
            this.label4.TabIndex = 212;
            this.label4.Text = "文件路径";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_simulateTest
            // 
            this.button_simulateTest.BackColor = System.Drawing.Color.White;
            this.button_simulateTest.Location = new System.Drawing.Point(314, 151);
            this.button_simulateTest.Name = "button_simulateTest";
            this.button_simulateTest.Size = new System.Drawing.Size(78, 30);
            this.button_simulateTest.TabIndex = 211;
            this.button_simulateTest.Text = "回测";
            this.button_simulateTest.UseVisualStyleBackColor = false;
            this.button_simulateTest.Click += new System.EventHandler(this.button_simulateTest_Click);
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
            this.checkBox_NotifyFlag.Location = new System.Drawing.Point(617, 10);
            this.checkBox_NotifyFlag.Name = "checkBox_NotifyFlag";
            this.checkBox_NotifyFlag.Size = new System.Drawing.Size(72, 16);
            this.checkBox_NotifyFlag.TabIndex = 43;
            this.checkBox_NotifyFlag.Text = "提醒通知";
            this.checkBox_NotifyFlag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox_NotifyFlag.UseVisualStyleBackColor = true;
            // 
            // bgwBroberQuote
            // 
            this.bgwBroberQuote.WorkerSupportsCancellation = true;
            this.bgwBroberQuote.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwBroberQuote_DoWork);
            // 
            // timerCheckPlatformConnectStatus
            // 
            this.timerCheckPlatformConnectStatus.Interval = 60000;
            this.timerCheckPlatformConnectStatus.Tick += new System.EventHandler(this.timerCheckPlatformConnectStatus_Tick);
            // 
            // timer_TicketList
            // 
            this.timer_TicketList.Tick += new System.EventHandler(this.timer_TicketList_Tick);
            // 
            // timer_Token
            // 
            this.timer_Token.Tick += new System.EventHandler(this.timer_Token_Tick);
            // 
            // timer_DataCenter
            // 
            this.timer_DataCenter.Tick += new System.EventHandler(this.timer_DataCenter_Tick);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(684, 46);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 218;
            this.label9.Text = "(12:00:00)";
            // 
            // textBox_testRaceTime
            // 
            this.textBox_testRaceTime.Location = new System.Drawing.Point(84, 39);
            this.textBox_testRaceTime.Multiline = true;
            this.textBox_testRaceTime.Name = "textBox_testRaceTime";
            this.textBox_testRaceTime.Size = new System.Drawing.Size(594, 25);
            this.textBox_testRaceTime.TabIndex = 217;
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.Khaki;
            this.label10.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(6, 40);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 24);
            this.label10.TabIndex = 219;
            this.label10.Text = "赛马时间";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AutoTradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 508);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AutoTradeForm";
            this.Text = "跑马交易平台";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DataLoadForm_Closed);
            this.Load += new System.EventHandler(this.DataLoadForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_EAExecute.ResumeLayout(false);
            this.tabPage_EAExecute.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_MaxBets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_lastMaxBetAmount)).EndInit();
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage_PlatformConfig.ResumeLayout(false);
            this.tabPage_PlatformConfig.PerformLayout();
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
        private System.Windows.Forms.Timer timerCheckPlatformConnectStatus;
        private System.Windows.Forms.Timer timer_TicketList;
        private System.Windows.Forms.Timer timer_Token;
        private System.Windows.Forms.Timer timer_DataCenter;
        public System.Windows.Forms.ListBox listTicketsList;
        private System.Windows.Forms.TextBox textBox_RaceTime;
        public System.Windows.Forms.Label label15;
        public System.Windows.Forms.Label label16;
        public System.Windows.Forms.ComboBox comboBox_LastBetTimeDuration;
        public System.Windows.Forms.Label label17;
        private System.Windows.Forms.NumericUpDown numericUpDown_lastMaxBetAmount;
        public System.Windows.Forms.Button button_simulateTest;
        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown_MaxBets;
        private System.Windows.Forms.TextBox textBox_filePatch;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_raceResult;
        public System.Windows.Forms.ComboBox comboBox_LastBetTime;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Button button_executeAnalysis;
        public System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_testRaceTime;
    }
}

