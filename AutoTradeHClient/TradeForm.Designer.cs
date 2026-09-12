
namespace uClient.Broker
{
    public partial class TradeForm
    {

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
        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblMT4Speed = new System.Windows.Forms.Label();
            this.lblMT4AskDiff4 = new System.Windows.Forms.Label();
            this.lblMT4AskDiff3 = new System.Windows.Forms.Label();
            this.lblMT4AskDiff2 = new System.Windows.Forms.Label();
            this.lblMT4AskDiff1 = new System.Windows.Forms.Label();
            this.lblMT4AskDiff0 = new System.Windows.Forms.Label();
            this.lblMT4Ask = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.lblMT4BidDiff4 = new System.Windows.Forms.Label();
            this.lblMT4BidDiff3 = new System.Windows.Forms.Label();
            this.lblMT4BidDiff2 = new System.Windows.Forms.Label();
            this.lblMT4BidDiff1 = new System.Windows.Forms.Label();
            this.lblMT4BidDiff0 = new System.Windows.Forms.Label();
            this.lblMT4Bid = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.lblDSSpeed = new System.Windows.Forms.Label();
            this.lblDSAskDiff4 = new System.Windows.Forms.Label();
            this.lblDSAskDiff3 = new System.Windows.Forms.Label();
            this.lblDSAskDiff2 = new System.Windows.Forms.Label();
            this.lblDSAskDiff1 = new System.Windows.Forms.Label();
            this.lblDSAskDiff0 = new System.Windows.Forms.Label();
            this.lblDSAsk = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.lblDSBidDiff4 = new System.Windows.Forms.Label();
            this.lblDSBidDiff3 = new System.Windows.Forms.Label();
            this.lblDSBidDiff2 = new System.Windows.Forms.Label();
            this.lblDSBidDiff1 = new System.Windows.Forms.Label();
            this.lblDSBidDiff0 = new System.Windows.Forms.Label();
            this.lblDSBid = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.bgwMarketQuote = new System.ComponentModel.BackgroundWorker();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonEAConnect = new System.Windows.Forms.Button();
            this.buttonDSConnect = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.labelConnectStatus = new System.Windows.Forms.Label();
            this.labelConnectType = new System.Windows.Forms.Label();
            this.button_Save = new System.Windows.Forms.Button();
            this.comboBox_AccType = new System.Windows.Forms.ComboBox();
            this.comboBox_Broker = new System.Windows.Forms.ComboBox();
            this.comboBox_Symbol = new System.Windows.Forms.ComboBox();
            this.lblConnectStatus = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_UserCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timerAccountPosionRefresh = new System.Windows.Forms.Timer(this.components);
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lstTradeRecord = new System.Windows.Forms.ListBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.button_Open_Trade = new System.Windows.Forms.Button();
            this.button_Clear = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button_TradePara_Cancel = new System.Windows.Forms.Button();
            this.button_TradePara_Save = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.lblTradeStatus = new System.Windows.Forms.Label();
            this.gvPositions = new System.Windows.Forms.DataGridView();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblFreeMargin = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.lblMargin = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.lblEquity = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lblBlance = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.timerCheckPlatformConnectStatus = new System.Windows.Forms.Timer(this.components);
            this.nudSellLots = new System.Windows.Forms.NumericUpDown();
            this.textBox_HorseNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_betAmount = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvPositions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSellLots)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_betAmount)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Black;
            this.groupBox2.Controls.Add(this.lblMT4Speed);
            this.groupBox2.Controls.Add(this.lblMT4AskDiff4);
            this.groupBox2.Controls.Add(this.lblMT4AskDiff3);
            this.groupBox2.Controls.Add(this.lblMT4AskDiff2);
            this.groupBox2.Controls.Add(this.lblMT4AskDiff1);
            this.groupBox2.Controls.Add(this.lblMT4AskDiff0);
            this.groupBox2.Controls.Add(this.lblMT4Ask);
            this.groupBox2.Controls.Add(this.label43);
            this.groupBox2.Controls.Add(this.lblMT4BidDiff4);
            this.groupBox2.Controls.Add(this.lblMT4BidDiff3);
            this.groupBox2.Controls.Add(this.lblMT4BidDiff2);
            this.groupBox2.Controls.Add(this.lblMT4BidDiff1);
            this.groupBox2.Controls.Add(this.lblMT4BidDiff0);
            this.groupBox2.Controls.Add(this.lblMT4Bid);
            this.groupBox2.Controls.Add(this.label50);
            this.groupBox2.Controls.Add(this.label51);
            this.groupBox2.Controls.Add(this.lblDSSpeed);
            this.groupBox2.Controls.Add(this.lblDSAskDiff4);
            this.groupBox2.Controls.Add(this.lblDSAskDiff3);
            this.groupBox2.Controls.Add(this.lblDSAskDiff2);
            this.groupBox2.Controls.Add(this.lblDSAskDiff1);
            this.groupBox2.Controls.Add(this.lblDSAskDiff0);
            this.groupBox2.Controls.Add(this.lblDSAsk);
            this.groupBox2.Controls.Add(this.label34);
            this.groupBox2.Controls.Add(this.lblDSBidDiff4);
            this.groupBox2.Controls.Add(this.lblDSBidDiff3);
            this.groupBox2.Controls.Add(this.lblDSBidDiff2);
            this.groupBox2.Controls.Add(this.lblDSBidDiff1);
            this.groupBox2.Controls.Add(this.lblDSBidDiff0);
            this.groupBox2.Controls.Add(this.lblDSBid);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(3, 87);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(500, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // lblMT4Speed
            // 
            this.lblMT4Speed.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4Speed.Font = new System.Drawing.Font("SimSun", 8F, System.Drawing.FontStyle.Bold);
            this.lblMT4Speed.ForeColor = System.Drawing.Color.White;
            this.lblMT4Speed.Location = new System.Drawing.Point(101, 56);
            this.lblMT4Speed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4Speed.Name = "lblMT4Speed";
            this.lblMT4Speed.Size = new System.Drawing.Size(62, 15);
            this.lblMT4Speed.TabIndex = 34;
            this.lblMT4Speed.Text = "0ms";
            this.lblMT4Speed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMT4AskDiff4
            // 
            this.lblMT4AskDiff4.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4AskDiff4.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4AskDiff4.ForeColor = System.Drawing.Color.White;
            this.lblMT4AskDiff4.Location = new System.Drawing.Point(467, 72);
            this.lblMT4AskDiff4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4AskDiff4.Name = "lblMT4AskDiff4";
            this.lblMT4AskDiff4.Size = new System.Drawing.Size(30, 20);
            this.lblMT4AskDiff4.TabIndex = 33;
            this.lblMT4AskDiff4.Text = "0";
            this.lblMT4AskDiff4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4AskDiff3
            // 
            this.lblMT4AskDiff3.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4AskDiff3.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4AskDiff3.ForeColor = System.Drawing.Color.White;
            this.lblMT4AskDiff3.Location = new System.Drawing.Point(438, 72);
            this.lblMT4AskDiff3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4AskDiff3.Name = "lblMT4AskDiff3";
            this.lblMT4AskDiff3.Size = new System.Drawing.Size(30, 20);
            this.lblMT4AskDiff3.TabIndex = 32;
            this.lblMT4AskDiff3.Text = "0";
            this.lblMT4AskDiff3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4AskDiff2
            // 
            this.lblMT4AskDiff2.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4AskDiff2.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4AskDiff2.ForeColor = System.Drawing.Color.White;
            this.lblMT4AskDiff2.Location = new System.Drawing.Point(409, 72);
            this.lblMT4AskDiff2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4AskDiff2.Name = "lblMT4AskDiff2";
            this.lblMT4AskDiff2.Size = new System.Drawing.Size(30, 20);
            this.lblMT4AskDiff2.TabIndex = 31;
            this.lblMT4AskDiff2.Text = "0";
            this.lblMT4AskDiff2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4AskDiff1
            // 
            this.lblMT4AskDiff1.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4AskDiff1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4AskDiff1.ForeColor = System.Drawing.Color.White;
            this.lblMT4AskDiff1.Location = new System.Drawing.Point(380, 72);
            this.lblMT4AskDiff1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4AskDiff1.Name = "lblMT4AskDiff1";
            this.lblMT4AskDiff1.Size = new System.Drawing.Size(30, 20);
            this.lblMT4AskDiff1.TabIndex = 30;
            this.lblMT4AskDiff1.Text = "0";
            this.lblMT4AskDiff1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4AskDiff0
            // 
            this.lblMT4AskDiff0.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4AskDiff0.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4AskDiff0.ForeColor = System.Drawing.Color.Chartreuse;
            this.lblMT4AskDiff0.Location = new System.Drawing.Point(351, 72);
            this.lblMT4AskDiff0.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4AskDiff0.Name = "lblMT4AskDiff0";
            this.lblMT4AskDiff0.Size = new System.Drawing.Size(30, 20);
            this.lblMT4AskDiff0.TabIndex = 29;
            this.lblMT4AskDiff0.Text = "0";
            this.lblMT4AskDiff0.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4Ask
            // 
            this.lblMT4Ask.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4Ask.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4Ask.ForeColor = System.Drawing.Color.Chartreuse;
            this.lblMT4Ask.Location = new System.Drawing.Point(287, 67);
            this.lblMT4Ask.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4Ask.Name = "lblMT4Ask";
            this.lblMT4Ask.Size = new System.Drawing.Size(64, 25);
            this.lblMT4Ask.TabIndex = 28;
            this.lblMT4Ask.Text = "0";
            this.lblMT4Ask.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label43
            // 
            this.label43.BackColor = System.Drawing.Color.Transparent;
            this.label43.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label43.ForeColor = System.Drawing.Color.Chartreuse;
            this.label43.Location = new System.Drawing.Point(249, 73);
            this.label43.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(36, 18);
            this.label43.TabIndex = 27;
            this.label43.Text = "Ask";
            this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMT4BidDiff4
            // 
            this.lblMT4BidDiff4.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4BidDiff4.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4BidDiff4.ForeColor = System.Drawing.Color.White;
            this.lblMT4BidDiff4.Location = new System.Drawing.Point(220, 72);
            this.lblMT4BidDiff4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4BidDiff4.Name = "lblMT4BidDiff4";
            this.lblMT4BidDiff4.Size = new System.Drawing.Size(30, 20);
            this.lblMT4BidDiff4.TabIndex = 26;
            this.lblMT4BidDiff4.Text = "0";
            this.lblMT4BidDiff4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4BidDiff3
            // 
            this.lblMT4BidDiff3.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4BidDiff3.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4BidDiff3.ForeColor = System.Drawing.Color.White;
            this.lblMT4BidDiff3.Location = new System.Drawing.Point(192, 72);
            this.lblMT4BidDiff3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4BidDiff3.Name = "lblMT4BidDiff3";
            this.lblMT4BidDiff3.Size = new System.Drawing.Size(30, 20);
            this.lblMT4BidDiff3.TabIndex = 25;
            this.lblMT4BidDiff3.Text = "0";
            this.lblMT4BidDiff3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4BidDiff2
            // 
            this.lblMT4BidDiff2.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4BidDiff2.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4BidDiff2.ForeColor = System.Drawing.Color.White;
            this.lblMT4BidDiff2.Location = new System.Drawing.Point(163, 72);
            this.lblMT4BidDiff2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4BidDiff2.Name = "lblMT4BidDiff2";
            this.lblMT4BidDiff2.Size = new System.Drawing.Size(30, 20);
            this.lblMT4BidDiff2.TabIndex = 24;
            this.lblMT4BidDiff2.Text = "0";
            this.lblMT4BidDiff2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4BidDiff1
            // 
            this.lblMT4BidDiff1.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4BidDiff1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4BidDiff1.ForeColor = System.Drawing.Color.White;
            this.lblMT4BidDiff1.Location = new System.Drawing.Point(133, 72);
            this.lblMT4BidDiff1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4BidDiff1.Name = "lblMT4BidDiff1";
            this.lblMT4BidDiff1.Size = new System.Drawing.Size(30, 20);
            this.lblMT4BidDiff1.TabIndex = 23;
            this.lblMT4BidDiff1.Text = "0";
            this.lblMT4BidDiff1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4BidDiff0
            // 
            this.lblMT4BidDiff0.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4BidDiff0.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4BidDiff0.ForeColor = System.Drawing.Color.Red;
            this.lblMT4BidDiff0.Location = new System.Drawing.Point(103, 72);
            this.lblMT4BidDiff0.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4BidDiff0.Name = "lblMT4BidDiff0";
            this.lblMT4BidDiff0.Size = new System.Drawing.Size(30, 20);
            this.lblMT4BidDiff0.TabIndex = 22;
            this.lblMT4BidDiff0.Text = "0";
            this.lblMT4BidDiff0.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblMT4Bid
            // 
            this.lblMT4Bid.BackColor = System.Drawing.Color.Transparent;
            this.lblMT4Bid.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMT4Bid.ForeColor = System.Drawing.Color.Red;
            this.lblMT4Bid.Location = new System.Drawing.Point(39, 67);
            this.lblMT4Bid.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMT4Bid.Name = "lblMT4Bid";
            this.lblMT4Bid.Size = new System.Drawing.Size(64, 25);
            this.lblMT4Bid.TabIndex = 21;
            this.lblMT4Bid.Text = "0";
            this.lblMT4Bid.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label50
            // 
            this.label50.BackColor = System.Drawing.Color.Transparent;
            this.label50.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label50.ForeColor = System.Drawing.Color.Red;
            this.label50.Location = new System.Drawing.Point(5, 73);
            this.label50.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(39, 18);
            this.label50.TabIndex = 20;
            this.label50.Text = "Bid";
            this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label51
            // 
            this.label51.BackColor = System.Drawing.Color.DarkOliveGreen;
            this.label51.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label51.ForeColor = System.Drawing.Color.White;
            this.label51.Location = new System.Drawing.Point(3, 56);
            this.label51.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(35, 16);
            this.label51.TabIndex = 19;
            this.label51.Text = "平台";
            this.label51.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDSSpeed
            // 
            this.lblDSSpeed.BackColor = System.Drawing.Color.Transparent;
            this.lblDSSpeed.Font = new System.Drawing.Font("SimSun", 8F, System.Drawing.FontStyle.Bold);
            this.lblDSSpeed.ForeColor = System.Drawing.Color.White;
            this.lblDSSpeed.Location = new System.Drawing.Point(101, 17);
            this.lblDSSpeed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSSpeed.Name = "lblDSSpeed";
            this.lblDSSpeed.Size = new System.Drawing.Size(61, 15);
            this.lblDSSpeed.TabIndex = 18;
            this.lblDSSpeed.Text = "0ms";
            this.lblDSSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDSAskDiff4
            // 
            this.lblDSAskDiff4.BackColor = System.Drawing.Color.Transparent;
            this.lblDSAskDiff4.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSAskDiff4.ForeColor = System.Drawing.Color.White;
            this.lblDSAskDiff4.Location = new System.Drawing.Point(467, 30);
            this.lblDSAskDiff4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSAskDiff4.Name = "lblDSAskDiff4";
            this.lblDSAskDiff4.Size = new System.Drawing.Size(30, 20);
            this.lblDSAskDiff4.TabIndex = 17;
            this.lblDSAskDiff4.Text = "0";
            this.lblDSAskDiff4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSAskDiff3
            // 
            this.lblDSAskDiff3.BackColor = System.Drawing.Color.Transparent;
            this.lblDSAskDiff3.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSAskDiff3.ForeColor = System.Drawing.Color.White;
            this.lblDSAskDiff3.Location = new System.Drawing.Point(438, 30);
            this.lblDSAskDiff3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSAskDiff3.Name = "lblDSAskDiff3";
            this.lblDSAskDiff3.Size = new System.Drawing.Size(30, 20);
            this.lblDSAskDiff3.TabIndex = 16;
            this.lblDSAskDiff3.Text = "0";
            this.lblDSAskDiff3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSAskDiff2
            // 
            this.lblDSAskDiff2.BackColor = System.Drawing.Color.Transparent;
            this.lblDSAskDiff2.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSAskDiff2.ForeColor = System.Drawing.Color.White;
            this.lblDSAskDiff2.Location = new System.Drawing.Point(409, 30);
            this.lblDSAskDiff2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSAskDiff2.Name = "lblDSAskDiff2";
            this.lblDSAskDiff2.Size = new System.Drawing.Size(30, 20);
            this.lblDSAskDiff2.TabIndex = 15;
            this.lblDSAskDiff2.Text = "0";
            this.lblDSAskDiff2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSAskDiff1
            // 
            this.lblDSAskDiff1.BackColor = System.Drawing.Color.Transparent;
            this.lblDSAskDiff1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSAskDiff1.ForeColor = System.Drawing.Color.White;
            this.lblDSAskDiff1.Location = new System.Drawing.Point(380, 30);
            this.lblDSAskDiff1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSAskDiff1.Name = "lblDSAskDiff1";
            this.lblDSAskDiff1.Size = new System.Drawing.Size(30, 20);
            this.lblDSAskDiff1.TabIndex = 14;
            this.lblDSAskDiff1.Text = "0";
            this.lblDSAskDiff1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSAskDiff0
            // 
            this.lblDSAskDiff0.BackColor = System.Drawing.Color.Transparent;
            this.lblDSAskDiff0.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSAskDiff0.ForeColor = System.Drawing.Color.Chartreuse;
            this.lblDSAskDiff0.Location = new System.Drawing.Point(351, 30);
            this.lblDSAskDiff0.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSAskDiff0.Name = "lblDSAskDiff0";
            this.lblDSAskDiff0.Size = new System.Drawing.Size(30, 20);
            this.lblDSAskDiff0.TabIndex = 13;
            this.lblDSAskDiff0.Text = "0";
            this.lblDSAskDiff0.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSAsk
            // 
            this.lblDSAsk.BackColor = System.Drawing.Color.Transparent;
            this.lblDSAsk.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSAsk.ForeColor = System.Drawing.Color.Chartreuse;
            this.lblDSAsk.Location = new System.Drawing.Point(287, 25);
            this.lblDSAsk.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSAsk.Name = "lblDSAsk";
            this.lblDSAsk.Size = new System.Drawing.Size(64, 25);
            this.lblDSAsk.TabIndex = 12;
            this.lblDSAsk.Text = "0";
            this.lblDSAsk.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label34
            // 
            this.label34.BackColor = System.Drawing.Color.Transparent;
            this.label34.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label34.ForeColor = System.Drawing.Color.Chartreuse;
            this.label34.Location = new System.Drawing.Point(249, 36);
            this.label34.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(36, 18);
            this.label34.TabIndex = 11;
            this.label34.Text = "Ask";
            this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDSBidDiff4
            // 
            this.lblDSBidDiff4.BackColor = System.Drawing.Color.Transparent;
            this.lblDSBidDiff4.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSBidDiff4.ForeColor = System.Drawing.Color.White;
            this.lblDSBidDiff4.Location = new System.Drawing.Point(220, 30);
            this.lblDSBidDiff4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSBidDiff4.Name = "lblDSBidDiff4";
            this.lblDSBidDiff4.Size = new System.Drawing.Size(30, 20);
            this.lblDSBidDiff4.TabIndex = 10;
            this.lblDSBidDiff4.Text = "0";
            this.lblDSBidDiff4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSBidDiff3
            // 
            this.lblDSBidDiff3.BackColor = System.Drawing.Color.Transparent;
            this.lblDSBidDiff3.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSBidDiff3.ForeColor = System.Drawing.Color.White;
            this.lblDSBidDiff3.Location = new System.Drawing.Point(192, 30);
            this.lblDSBidDiff3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSBidDiff3.Name = "lblDSBidDiff3";
            this.lblDSBidDiff3.Size = new System.Drawing.Size(30, 20);
            this.lblDSBidDiff3.TabIndex = 9;
            this.lblDSBidDiff3.Text = "0";
            this.lblDSBidDiff3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSBidDiff2
            // 
            this.lblDSBidDiff2.BackColor = System.Drawing.Color.Transparent;
            this.lblDSBidDiff2.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSBidDiff2.ForeColor = System.Drawing.Color.White;
            this.lblDSBidDiff2.Location = new System.Drawing.Point(163, 30);
            this.lblDSBidDiff2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSBidDiff2.Name = "lblDSBidDiff2";
            this.lblDSBidDiff2.Size = new System.Drawing.Size(30, 20);
            this.lblDSBidDiff2.TabIndex = 8;
            this.lblDSBidDiff2.Text = "0";
            this.lblDSBidDiff2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSBidDiff1
            // 
            this.lblDSBidDiff1.BackColor = System.Drawing.Color.Transparent;
            this.lblDSBidDiff1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSBidDiff1.ForeColor = System.Drawing.Color.White;
            this.lblDSBidDiff1.Location = new System.Drawing.Point(133, 30);
            this.lblDSBidDiff1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSBidDiff1.Name = "lblDSBidDiff1";
            this.lblDSBidDiff1.Size = new System.Drawing.Size(30, 20);
            this.lblDSBidDiff1.TabIndex = 7;
            this.lblDSBidDiff1.Text = "0";
            this.lblDSBidDiff1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSBidDiff0
            // 
            this.lblDSBidDiff0.BackColor = System.Drawing.Color.Transparent;
            this.lblDSBidDiff0.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSBidDiff0.ForeColor = System.Drawing.Color.Red;
            this.lblDSBidDiff0.Location = new System.Drawing.Point(103, 30);
            this.lblDSBidDiff0.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSBidDiff0.Name = "lblDSBidDiff0";
            this.lblDSBidDiff0.Size = new System.Drawing.Size(30, 20);
            this.lblDSBidDiff0.TabIndex = 5;
            this.lblDSBidDiff0.Text = "0";
            this.lblDSBidDiff0.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblDSBid
            // 
            this.lblDSBid.BackColor = System.Drawing.Color.Transparent;
            this.lblDSBid.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDSBid.ForeColor = System.Drawing.Color.Red;
            this.lblDSBid.Location = new System.Drawing.Point(39, 25);
            this.lblDSBid.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDSBid.Name = "lblDSBid";
            this.lblDSBid.Size = new System.Drawing.Size(64, 25);
            this.lblDSBid.TabIndex = 4;
            this.lblDSBid.Text = "0";
            this.lblDSBid.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label18
            // 
            this.label18.BackColor = System.Drawing.Color.Transparent;
            this.label18.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label18.ForeColor = System.Drawing.Color.Red;
            this.label18.Location = new System.Drawing.Point(5, 35);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(38, 18);
            this.label18.TabIndex = 3;
            this.label18.Text = "Bid";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label17
            // 
            this.label17.BackColor = System.Drawing.Color.DarkOliveGreen;
            this.label17.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.ForeColor = System.Drawing.Color.White;
            this.label17.Location = new System.Drawing.Point(3, 19);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(35, 16);
            this.label17.TabIndex = 2;
            this.label17.Text = "市场";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lstLog
            // 
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.ItemHeight = 16;
            this.lstLog.Location = new System.Drawing.Point(0, 4);
            this.lstLog.Margin = new System.Windows.Forms.Padding(4);
            this.lstLog.Name = "lstLog";
            this.lstLog.ScrollAlwaysVisible = true;
            this.lstLog.Size = new System.Drawing.Size(397, 112);
            this.lstLog.TabIndex = 4;
            // 
            // bgwMarketQuote
            // 
            this.bgwMarketQuote.WorkerSupportsCancellation = true;
            this.bgwMarketQuote.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwMarketQuote_DoWork);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(3, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(500, 96);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonEAConnect);
            this.tabPage1.Controls.Add(this.buttonDSConnect);
            this.tabPage1.Controls.Add(this.btnLogin);
            this.tabPage1.Controls.Add(this.labelConnectStatus);
            this.tabPage1.Controls.Add(this.labelConnectType);
            this.tabPage1.Controls.Add(this.button_Save);
            this.tabPage1.Controls.Add(this.comboBox_AccType);
            this.tabPage1.Controls.Add(this.comboBox_Broker);
            this.tabPage1.Controls.Add(this.comboBox_Symbol);
            this.tabPage1.Controls.Add(this.lblConnectStatus);
            this.tabPage1.Controls.Add(this.btnLogout);
            this.tabPage1.Controls.Add(this.textBox_Password);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.textBox_UserCode);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(492, 64);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "帐户";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // buttonEAConnect
            // 
            this.buttonEAConnect.BackColor = System.Drawing.Color.Red;
            this.buttonEAConnect.ForeColor = System.Drawing.SystemColors.Info;
            this.buttonEAConnect.Location = new System.Drawing.Point(285, 34);
            this.buttonEAConnect.Name = "buttonEAConnect";
            this.buttonEAConnect.Size = new System.Drawing.Size(66, 28);
            this.buttonEAConnect.TabIndex = 33;
            this.buttonEAConnect.Text = "EA连接";
            this.buttonEAConnect.UseVisualStyleBackColor = false;
            this.buttonEAConnect.Click += new System.EventHandler(this.buttonEAConnect_Click);
            // 
            // buttonDSConnect
            // 
            this.buttonDSConnect.BackColor = System.Drawing.Color.Red;
            this.buttonDSConnect.ForeColor = System.Drawing.SystemColors.Info;
            this.buttonDSConnect.Location = new System.Drawing.Point(210, 34);
            this.buttonDSConnect.Name = "buttonDSConnect";
            this.buttonDSConnect.Size = new System.Drawing.Size(72, 28);
            this.buttonDSConnect.TabIndex = 32;
            this.buttonDSConnect.Text = "DS连接";
            this.buttonDSConnect.UseVisualStyleBackColor = false;
            this.buttonDSConnect.Click += new System.EventHandler(this.buttonDSConnect_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.Red;
            this.btnLogin.ForeColor = System.Drawing.SystemColors.Info;
            this.btnLogin.Location = new System.Drawing.Point(357, 34);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(80, 28);
            this.btnLogin.TabIndex = 20;
            this.btnLogin.Text = "平台连接";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // labelConnectStatus
            // 
            this.labelConnectStatus.BackColor = System.Drawing.Color.Red;
            this.labelConnectStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelConnectStatus.ForeColor = System.Drawing.SystemColors.Window;
            this.labelConnectStatus.Location = new System.Drawing.Point(44, 36);
            this.labelConnectStatus.Name = "labelConnectStatus";
            this.labelConnectStatus.Size = new System.Drawing.Size(78, 24);
            this.labelConnectStatus.TabIndex = 31;
            this.labelConnectStatus.Text = "未连接";
            this.labelConnectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelConnectType
            // 
            this.labelConnectType.BackColor = System.Drawing.Color.Khaki;
            this.labelConnectType.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelConnectType.Location = new System.Drawing.Point(4, 36);
            this.labelConnectType.Name = "labelConnectType";
            this.labelConnectType.Size = new System.Drawing.Size(37, 24);
            this.labelConnectType.TabIndex = 30;
            this.labelConnectType.Text = "DS";
            this.labelConnectType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_Save
            // 
            this.button_Save.BackColor = System.Drawing.Color.White;
            this.button_Save.Location = new System.Drawing.Point(441, 34);
            this.button_Save.Name = "button_Save";
            this.button_Save.Size = new System.Drawing.Size(45, 28);
            this.button_Save.TabIndex = 29;
            this.button_Save.Text = "保存";
            this.button_Save.UseVisualStyleBackColor = false;
            this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
            // 
            // comboBox_AccType
            // 
            this.comboBox_AccType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_AccType.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_AccType.FormattingEnabled = true;
            this.comboBox_AccType.Location = new System.Drawing.Point(86, 5);
            this.comboBox_AccType.Name = "comboBox_AccType";
            this.comboBox_AccType.Size = new System.Drawing.Size(60, 24);
            this.comboBox_AccType.TabIndex = 28;
            this.comboBox_AccType.SelectedIndexChanged += new System.EventHandler(this.comboBox_AccType_SelectedIndexChanged);
            // 
            // comboBox_Broker
            // 
            this.comboBox_Broker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Broker.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_Broker.FormattingEnabled = true;
            this.comboBox_Broker.Location = new System.Drawing.Point(4, 5);
            this.comboBox_Broker.Name = "comboBox_Broker";
            this.comboBox_Broker.Size = new System.Drawing.Size(80, 24);
            this.comboBox_Broker.TabIndex = 27;
            this.comboBox_Broker.SelectedIndexChanged += new System.EventHandler(this.comboBox_Broker_SelectedIndexChanged);
            // 
            // comboBox_Symbol
            // 
            this.comboBox_Symbol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Symbol.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_Symbol.FormattingEnabled = true;
            this.comboBox_Symbol.Location = new System.Drawing.Point(148, 5);
            this.comboBox_Symbol.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_Symbol.Name = "comboBox_Symbol";
            this.comboBox_Symbol.Size = new System.Drawing.Size(60, 24);
            this.comboBox_Symbol.TabIndex = 25;
            this.comboBox_Symbol.SelectedIndexChanged += new System.EventHandler(this.cmbSymbol_SelectedValueChanged);
            this.comboBox_Symbol.SelectedValueChanged += new System.EventHandler(this.cmbSymbol_SelectedValueChanged);
            // 
            // lblConnectStatus
            // 
            this.lblConnectStatus.BackColor = System.Drawing.Color.Red;
            this.lblConnectStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblConnectStatus.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblConnectStatus.Location = new System.Drawing.Point(126, 36);
            this.lblConnectStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectStatus.Name = "lblConnectStatus";
            this.lblConnectStatus.Size = new System.Drawing.Size(55, 24);
            this.lblConnectStatus.TabIndex = 24;
            this.lblConnectStatus.Text = "未登录";
            this.lblConnectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.Color.White;
            this.btnLogout.Enabled = false;
            this.btnLogout.ForeColor = System.Drawing.SystemColors.InfoText;
            this.btnLogout.Location = new System.Drawing.Point(396, 34);
            this.btnLogout.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(45, 28);
            this.btnLogout.TabIndex = 21;
            this.btnLogout.Text = "断开";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Visible = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // textBox_Password
            // 
            this.textBox_Password.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_Password.Location = new System.Drawing.Point(400, 4);
            this.textBox_Password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.PasswordChar = '*';
            this.textBox_Password.Size = new System.Drawing.Size(88, 25);
            this.textBox_Password.TabIndex = 17;
            this.textBox_Password.TextChanged += new System.EventHandler(this.textBox_Password_TextChanged);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Khaki;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(354, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 24);
            this.label2.TabIndex = 16;
            this.label2.Text = "密码";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_UserCode
            // 
            this.textBox_UserCode.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_UserCode.Location = new System.Drawing.Point(263, 4);
            this.textBox_UserCode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_UserCode.Name = "textBox_UserCode";
            this.textBox_UserCode.Size = new System.Drawing.Size(88, 25);
            this.textBox_UserCode.TabIndex = 15;
            this.textBox_UserCode.TextChanged += new System.EventHandler(this.textBox_UserCode_TextChanged);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Khaki;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(215, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 24);
            this.label1.TabIndex = 14;
            this.label1.Text = "账号";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timerAccountPosionRefresh
            // 
            this.timerAccountPosionRefresh.Enabled = true;
            this.timerAccountPosionRefresh.Interval = 1000;
            this.timerAccountPosionRefresh.Tick += new System.EventHandler(this.timerAccountPosionRefresh_Tick);
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl2.Location = new System.Drawing.Point(5, 426);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(402, 148);
            this.tabControl2.TabIndex = 6;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lstTradeRecord);
            this.tabPage4.Location = new System.Drawing.Point(4, 28);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(394, 116);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "交易记录";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // lstTradeRecord
            // 
            this.lstTradeRecord.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstTradeRecord.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lstTradeRecord.FormattingEnabled = true;
            this.lstTradeRecord.HorizontalScrollbar = true;
            this.lstTradeRecord.ItemHeight = 17;
            this.lstTradeRecord.Location = new System.Drawing.Point(-1, -2);
            this.lstTradeRecord.Margin = new System.Windows.Forms.Padding(4);
            this.lstTradeRecord.Name = "lstTradeRecord";
            this.lstTradeRecord.ScrollAlwaysVisible = true;
            this.lstTradeRecord.Size = new System.Drawing.Size(397, 119);
            this.lstTradeRecord.TabIndex = 5;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.lstLog);
            this.tabPage5.Location = new System.Drawing.Point(4, 28);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(394, 116);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "日志";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // button_Open_Trade
            // 
            this.button_Open_Trade.BackColor = System.Drawing.SystemColors.Window;
            this.button_Open_Trade.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_Open_Trade.Location = new System.Drawing.Point(285, 426);
            this.button_Open_Trade.Name = "button_Open_Trade";
            this.button_Open_Trade.Size = new System.Drawing.Size(50, 25);
            this.button_Open_Trade.TabIndex = 7;
            this.button_Open_Trade.Text = "交易";
            this.button_Open_Trade.UseVisualStyleBackColor = false;
            this.button_Open_Trade.Click += new System.EventHandler(this.button_Open_Trade_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.BackColor = System.Drawing.SystemColors.Window;
            this.button_Clear.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_Clear.Location = new System.Drawing.Point(343, 426);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(50, 26);
            this.button_Clear.TabIndex = 8;
            this.button_Clear.Text = "清空";
            this.button_Clear.UseVisualStyleBackColor = false;
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.SystemColors.Window;
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBox1.ColumnWidth = 50;
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBox1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(410, 455);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(90, 112);
            this.listBox1.TabIndex = 9;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // button_TradePara_Cancel
            // 
            this.button_TradePara_Cancel.BackColor = System.Drawing.Color.White;
            this.button_TradePara_Cancel.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_TradePara_Cancel.ForeColor = System.Drawing.Color.Black;
            this.button_TradePara_Cancel.Location = new System.Drawing.Point(421, 229);
            this.button_TradePara_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_TradePara_Cancel.Name = "button_TradePara_Cancel";
            this.button_TradePara_Cancel.Size = new System.Drawing.Size(60, 28);
            this.button_TradePara_Cancel.TabIndex = 44;
            this.button_TradePara_Cancel.Text = "取消";
            this.button_TradePara_Cancel.UseVisualStyleBackColor = false;
            this.button_TradePara_Cancel.Click += new System.EventHandler(this.button_TradePara_Cancel_Click);
            // 
            // button_TradePara_Save
            // 
            this.button_TradePara_Save.BackColor = System.Drawing.Color.White;
            this.button_TradePara_Save.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_TradePara_Save.ForeColor = System.Drawing.Color.Black;
            this.button_TradePara_Save.Location = new System.Drawing.Point(421, 196);
            this.button_TradePara_Save.Margin = new System.Windows.Forms.Padding(4);
            this.button_TradePara_Save.Name = "button_TradePara_Save";
            this.button_TradePara_Save.Size = new System.Drawing.Size(60, 28);
            this.button_TradePara_Save.TabIndex = 43;
            this.button_TradePara_Save.Text = "保存";
            this.button_TradePara_Save.UseVisualStyleBackColor = false;
            this.button_TradePara_Save.Click += new System.EventHandler(this.button_TradePara_Save_Click);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.ForeColor = System.Drawing.Color.Green;
            this.label14.Location = new System.Drawing.Point(6, 201);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 19);
            this.label14.TabIndex = 39;
            this.label14.Text = "场次";
            // 
            // lblTradeStatus
            // 
            this.lblTradeStatus.BackColor = System.Drawing.Color.Red;
            this.lblTradeStatus.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTradeStatus.ForeColor = System.Drawing.SystemColors.Window;
            this.lblTradeStatus.Location = new System.Drawing.Point(3, 269);
            this.lblTradeStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTradeStatus.Name = "lblTradeStatus";
            this.lblTradeStatus.Size = new System.Drawing.Size(103, 30);
            this.lblTradeStatus.TabIndex = 66;
            this.lblTradeStatus.Text = "自动交易未启动";
            this.lblTradeStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gvPositions
            // 
            this.gvPositions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gvPositions.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gvPositions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle25.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle25.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle25.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle25.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle25.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvPositions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle25;
            this.gvPositions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle26.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle26.SelectionBackColor = System.Drawing.Color.Transparent;
            dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gvPositions.DefaultCellStyle = dataGridViewCellStyle26;
            this.gvPositions.EnableHeadersVisualStyles = false;
            this.gvPositions.Location = new System.Drawing.Point(5, 339);
            this.gvPositions.Margin = new System.Windows.Forms.Padding(4);
            this.gvPositions.MultiSelect = false;
            this.gvPositions.Name = "gvPositions";
            dataGridViewCellStyle27.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle27.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle27.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle27.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvPositions.RowHeadersDefaultCellStyle = dataGridViewCellStyle27;
            this.gvPositions.RowHeadersVisible = false;
            this.gvPositions.RowTemplate.Height = 23;
            this.gvPositions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gvPositions.Size = new System.Drawing.Size(496, 83);
            this.gvPositions.TabIndex = 65;
            this.gvPositions.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvPositions_CellValueChanged);
            this.gvPositions.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.gvPositions_RowPostPaint);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.SystemColors.Window;
            this.btnClose.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClose.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnClose.Location = new System.Drawing.Point(415, 266);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 36);
            this.btnClose.TabIndex = 64;
            this.btnClose.Text = "下注";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.SystemColors.Window;
            this.btnStop.Enabled = false;
            this.btnStop.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.Location = new System.Drawing.Point(162, 269);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(48, 30);
            this.btnStop.TabIndex = 61;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.Red;
            this.btnStart.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.ForeColor = System.Drawing.SystemColors.Info;
            this.btnStart.Location = new System.Drawing.Point(113, 269);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(48, 30);
            this.btnStart.TabIndex = 60;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblFreeMargin
            // 
            this.lblFreeMargin.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFreeMargin.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFreeMargin.Location = new System.Drawing.Point(430, 307);
            this.lblFreeMargin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFreeMargin.Name = "lblFreeMargin";
            this.lblFreeMargin.Size = new System.Drawing.Size(60, 26);
            this.lblFreeMargin.TabIndex = 59;
            this.lblFreeMargin.Text = "8888.88";
            this.lblFreeMargin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label23
            // 
            this.label23.BackColor = System.Drawing.Color.Khaki;
            this.label23.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label23.ForeColor = System.Drawing.Color.Black;
            this.label23.Location = new System.Drawing.Point(360, 307);
            this.label23.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(65, 26);
            this.label23.TabIndex = 58;
            this.label23.Text = "可用按金";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMargin
            // 
            this.lblMargin.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblMargin.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMargin.Location = new System.Drawing.Point(298, 307);
            this.lblMargin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMargin.Name = "lblMargin";
            this.lblMargin.Size = new System.Drawing.Size(60, 26);
            this.lblMargin.TabIndex = 57;
            this.lblMargin.Text = "8888.88";
            this.lblMargin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label21
            // 
            this.label21.BackColor = System.Drawing.Color.Khaki;
            this.label21.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label21.ForeColor = System.Drawing.Color.Black;
            this.label21.Location = new System.Drawing.Point(226, 307);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(70, 26);
            this.label21.TabIndex = 56;
            this.label21.Text = "已用按金";
            this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblEquity
            // 
            this.lblEquity.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblEquity.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblEquity.Location = new System.Drawing.Point(164, 307);
            this.lblEquity.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEquity.Name = "lblEquity";
            this.lblEquity.Size = new System.Drawing.Size(60, 26);
            this.lblEquity.TabIndex = 55;
            this.lblEquity.Text = "8888.88";
            this.lblEquity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label19
            // 
            this.label19.BackColor = System.Drawing.Color.Khaki;
            this.label19.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label19.ForeColor = System.Drawing.Color.Black;
            this.label19.Location = new System.Drawing.Point(116, 307);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(45, 26);
            this.label19.TabIndex = 54;
            this.label19.Text = "净值";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblBlance
            // 
            this.lblBlance.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblBlance.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBlance.Location = new System.Drawing.Point(55, 307);
            this.lblBlance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBlance.Name = "lblBlance";
            this.lblBlance.Size = new System.Drawing.Size(60, 26);
            this.lblBlance.TabIndex = 53;
            this.lblBlance.Text = "8888.88";
            this.lblBlance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label16
            // 
            this.label16.BackColor = System.Drawing.Color.Khaki;
            this.label16.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.ForeColor = System.Drawing.Color.Black;
            this.label16.Location = new System.Drawing.Point(8, 307);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(45, 26);
            this.label16.TabIndex = 52;
            this.label16.Text = "余额";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label13.Location = new System.Drawing.Point(3, 260);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(500, 2);
            this.label13.TabIndex = 68;
            // 
            // timerCheckPlatformConnectStatus
            // 
            this.timerCheckPlatformConnectStatus.Enabled = true;
            this.timerCheckPlatformConnectStatus.Interval = 300000;
            this.timerCheckPlatformConnectStatus.Tick += new System.EventHandler(this.timerCheckPlatformConnectStatus_Tick);
            // 
            // nudSellLots
            // 
            this.nudSellLots.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.nudSellLots.ForeColor = System.Drawing.Color.OliveDrab;
            this.nudSellLots.Location = new System.Drawing.Point(45, 198);
            this.nudSellLots.Margin = new System.Windows.Forms.Padding(4);
            this.nudSellLots.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudSellLots.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSellLots.Name = "nudSellLots";
            this.nudSellLots.Size = new System.Drawing.Size(45, 25);
            this.nudSellLots.TabIndex = 40;
            this.nudSellLots.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudSellLots.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSellLots.ValueChanged += new System.EventHandler(this.nudSellLots_ValueChanged);
            // 
            // textBox_HorseNumber
            // 
            this.textBox_HorseNumber.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_HorseNumber.Location = new System.Drawing.Point(128, 199);
            this.textBox_HorseNumber.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_HorseNumber.Name = "textBox_HorseNumber";
            this.textBox_HorseNumber.Size = new System.Drawing.Size(52, 25);
            this.textBox_HorseNumber.TabIndex = 34;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.Green;
            this.label3.Location = new System.Drawing.Point(93, 202);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 19);
            this.label3.TabIndex = 69;
            this.label3.Text = "马号";
            // 
            // numericUpDown_betAmount
            // 
            this.numericUpDown_betAmount.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDown_betAmount.ForeColor = System.Drawing.Color.OliveDrab;
            this.numericUpDown_betAmount.Location = new System.Drawing.Point(219, 200);
            this.numericUpDown_betAmount.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown_betAmount.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown_betAmount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_betAmount.Name = "numericUpDown_betAmount";
            this.numericUpDown_betAmount.Size = new System.Drawing.Size(69, 25);
            this.numericUpDown_betAmount.TabIndex = 71;
            this.numericUpDown_betAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_betAmount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.Color.Green;
            this.label4.Location = new System.Drawing.Point(184, 202);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 19);
            this.label4.TabIndex = 70;
            this.label4.Text = "金额";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(333, 199);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(60, 24);
            this.comboBox1.TabIndex = 34;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.ForeColor = System.Drawing.Color.Green;
            this.label5.Location = new System.Drawing.Point(295, 202);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 19);
            this.label5.TabIndex = 72;
            this.label5.Text = "方式";
            // 
            // TradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 581);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.numericUpDown_betAmount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_HorseNumber);
            this.Controls.Add(this.button_Clear);
            this.Controls.Add(this.button_Open_Trade);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.lblTradeStatus);
            this.Controls.Add(this.gvPositions);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblFreeMargin);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.lblMargin);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.lblEquity);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.lblBlance);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.button_TradePara_Cancel);
            this.Controls.Add(this.button_TradePara_Save);
            this.Controls.Add(this.nudSellLots);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "TradeForm";
            this.Text = "交易窗口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmTrade_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmTrade_FormClosed);
            this.Load += new System.EventHandler(this.TradeForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gvPositions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSellLots)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_betAmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ListBox lstLog;
        public System.Windows.Forms.Label lblMT4Speed;
        public System.Windows.Forms.Label lblMT4AskDiff4;
        public System.Windows.Forms.Label lblMT4AskDiff3;
        public System.Windows.Forms.Label lblMT4AskDiff2;
        public System.Windows.Forms.Label lblMT4AskDiff1;
        public System.Windows.Forms.Label lblMT4AskDiff0;
        public System.Windows.Forms.Label lblMT4Ask;
        public System.Windows.Forms.Label label43;
        public System.Windows.Forms.Label lblMT4BidDiff4;
        public System.Windows.Forms.Label lblMT4BidDiff3;
        public System.Windows.Forms.Label lblMT4BidDiff2;
        public System.Windows.Forms.Label lblMT4BidDiff1;
        public System.Windows.Forms.Label lblMT4BidDiff0;
        public System.Windows.Forms.Label lblMT4Bid;
        public System.Windows.Forms.Label label50;
        public System.Windows.Forms.Label label51;
        public System.Windows.Forms.Label lblDSSpeed;
        public System.Windows.Forms.Label lblDSAskDiff4;
        public System.Windows.Forms.Label lblDSAskDiff3;
        public System.Windows.Forms.Label lblDSAskDiff2;
        public System.Windows.Forms.Label lblDSAskDiff1;
        public System.Windows.Forms.Label lblDSAskDiff0;
        public System.Windows.Forms.Label lblDSAsk;
        public System.Windows.Forms.Label label34;
        public System.Windows.Forms.Label lblDSBidDiff4;
        public System.Windows.Forms.Label lblDSBidDiff3;
        public System.Windows.Forms.Label lblDSBidDiff2;
        public System.Windows.Forms.Label lblDSBidDiff1;
        public System.Windows.Forms.Label lblDSBidDiff0;
        public System.Windows.Forms.Label lblDSBid;
        public System.Windows.Forms.Label label18;
        public System.Windows.Forms.Label label17;
        public System.ComponentModel.BackgroundWorker bgwMarketQuote;
        public System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.ComboBox comboBox_Symbol;
        public System.Windows.Forms.Label lblConnectStatus;
        public System.Windows.Forms.Button btnLogout;
        public System.Windows.Forms.Button btnLogin;
        public System.Windows.Forms.TextBox textBox_Password;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox_UserCode;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Timer timerAccountPosionRefresh;
        public System.Windows.Forms.TabPage tabPage5;
        public System.Windows.Forms.ComboBox comboBox_AccType;
        public System.Windows.Forms.ComboBox comboBox_Broker;
        public System.Windows.Forms.Button button_Save;
        public System.Windows.Forms.Label labelConnectStatus;
        public System.Windows.Forms.Label labelConnectType;
        public System.Windows.Forms.Button button_Open_Trade;
        public System.Windows.Forms.Button button_Clear;
        public System.Windows.Forms.Button button_TradePara_Cancel;
        public System.Windows.Forms.Button button_TradePara_Save;
        public System.Windows.Forms.Label label14;
        public System.Windows.Forms.Label lblTradeStatus;
        public System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.Label lblFreeMargin;
        public System.Windows.Forms.Label label23;
        public System.Windows.Forms.Label lblMargin;
        public System.Windows.Forms.Label label21;
        public System.Windows.Forms.Label lblEquity;
        public System.Windows.Forms.Label label19;
        public System.Windows.Forms.Label lblBlance;
        public System.Windows.Forms.Label label16;
        public System.Windows.Forms.Label label13;
        public System.Windows.Forms.TabPage tabPage4;
        public System.Windows.Forms.Button buttonDSConnect;
        public System.Windows.Forms.DataGridView gvPositions;
        public System.Windows.Forms.TabControl tabControl2;
        public System.Windows.Forms.ListBox listBox1;
        public System.Windows.Forms.ListBox lstTradeRecord;
        public System.Windows.Forms.Button btnStart;
        public System.Windows.Forms.Button btnStop;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Timer timerCheckPlatformConnectStatus;
        public System.Windows.Forms.NumericUpDown nudSellLots;
        public System.Windows.Forms.Button buttonEAConnect;
        public System.Windows.Forms.TextBox textBox_HorseNumber;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.NumericUpDown numericUpDown_betAmount;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.Label label5;
    }
}