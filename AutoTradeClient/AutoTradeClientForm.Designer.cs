namespace uClient.Broker
{
    public partial class AutoTradeClientForm : uClient.TradeForm
    {
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.myComponents != null))
            {
                myComponents.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// 子类自定义组件
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public new void InitializeComponent()
        {
            //this.components = new System.ComponentModel.Container();
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "AutoTradeClientForm";


            // 解除父类绑定的事件处理程序
            buttonDSConnect.Click -= buttonDSConnect_Click;

            // 绑定子类的事件处理程序
            buttonDSConnect.Click += buttonDSConnectClient_Click;

            // 
            // timerCheckPlatformConnectStatus
            // 
            this.timerCheckPlatformConnectStatus = new System.Windows.Forms.Timer(this.myComponents);
            this.timerCheckPlatformConnectStatus.Enabled = false;
            this.timerCheckPlatformConnectStatus.Interval = 300000;
            this.timerCheckPlatformConnectStatus.Tick += new System.EventHandler(this.timerCheckPlatformConnectStatus_Tick);
            // 
            // timerRefreshAutoTrade
            // 
            this.timerRefreshAutoTrade = new System.Windows.Forms.Timer(this.myComponents);
            this.timerRefreshAutoTrade.Enabled = true;
            this.timerRefreshAutoTrade.Interval = 60000;
            this.timerRefreshAutoTrade.Tick += new System.EventHandler(this.timerRefreshAutoTrade_Tick);
        }

        #endregion
        //自定义组件列表
        /// <summary>
        /// 拉起平台连接
        /// </summary>
        public new System.Windows.Forms.Timer timerCheckPlatformConnectStatus;
        /// <summary>
        /// 刷新EA交易端参数
        /// </summary>
        public System.Windows.Forms.Timer timerRefreshAutoTrade;
    }
}