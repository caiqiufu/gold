using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using CQG;

namespace AccountsAndPositions
{
    public class frmAccountsAndPositions : System.Windows.Forms.Form
    {

        [STAThread]
        static void Main()
        {
            Application.Run(new frmAccountsAndPositions());
        }

        private const string N_A = "N/A";
        private const int LIST_ACC_COL_WIDTH = 35;

        private const string DATE_OF_LAST_STATEMENT = "Date Of Last Statement:";
        private const string FCM_ACCOUNT_ID = "Fcm Account ID:";
        private const string FCM_ID = "Fcm ID:";
        private const string FCM_NAME = "Fcm Name:";
        private const string GW_ACCOUNT_ID = "GW Account ID:";
        private const string GW_ACCOUNT_NAME = "GW Account Name:";
        private const string POSITION_SUBSCRIPTION_LEVEL = "Position Subcription Level:";
        private const string REPORTING_CURRENCY = "Reporting Currency:";
        private const string SERVER_TIMESTAMP = "Server Timestamp:";
        private const string TIMESTAMP = "Timestamp:";
        private const string ACCOUNT_BALANCE = "Account Balance:";
        private const string INITIAL_MARGIN = "Initial Margin:";
        private const string MAINTENANCE_MARGIN = "Maintenance Margin:";
        private const string OPEN_TRADE_EQUITY = "Open Trade Equity:";
        private const string UNREALIZED_PROFIT_LOSS_FOR_OPTIONS = "Unrealized Profit/Loss for Options:";
        private const string COLLATERAL_ON_DEPOSIT = "Collateral on Deposit:";
        private const string NET_LIQUIDITY_VALUE = "Net Liquidity Value:";
        private const string MARKET_VALUE_OF_OPTIONS = "Market Value of Options:";
        private const string CASH_EXCESS = "Cash Excess:";
        private const string AVERAGE_PRICE = "Average Price:";
        private const string MARKET_VALUE_OPTION = "Market Value Option:";
        private const string PROFIT_LOSS = "Profit/Loss:";
        private const string LONG_ = "Long:";
        private const string SHORT_ = "Short:";
        private const string OFFSET = "Offset:";
        private const string CURRENCY = "Currency:";
        private const string TOTAL_MARGIN = "Total Margin:";
        private const string PURCHASING_POWER = "Total Purchasing Power:";
        private const string MARGIN_CREDIT = "Margin Credit:";
        private const string MARGIN_EXCESS = "Margin Excess:";
        private const string POSITION_TRACKING_TYPE = "Tracking type:";
        private const string POSITION_DAY = "Position day:";

        private CQGCEL m_CEL;

        /// Grids for dumping Properties/Summaries/Positions
        private CQGMiniGrid m_PropertiesGrid;
        private CQGMiniGrid m_PositionsGrid;
        private CQGMiniGrid m_SummariesGrid;

        /// HashTable which maps properties names with their rows in m_PropertiesGrid grid
        private Hashtable m_PropertyToRow;
        /// HashTable which maps items of summary with their rows in m_SummariesGrid grid
        private Hashtable m_SummaryToRow;
        /// HashTable which maps items of position with their rows in m_PositionsGrid grid
        private Hashtable m_PositionToRow;

        /// Open position key
        class PositionKey
        {
            public string InstrumentName { get; set; }
            public eOrderSide Side { get; set; }
            public EPositionDay PositionDay { get; set; }

            public static PositionKey Create(CQGPosition position)
            {
                if (position != null)
                {
                    switch (position.TrackingType)
                    {
                        case EPositionTrackingType.pttNet:
                            return new PositionKey { InstrumentName = position.InstrumentName, Side = eOrderSide.osdUndefined, PositionDay = EPositionDay.dayUndefined };
                        case EPositionTrackingType.pttImplied:
                            return new PositionKey { InstrumentName = position.InstrumentName, Side = position.Side, PositionDay = EPositionDay.dayUndefined };
                        case EPositionTrackingType.pttExplicit:
                            return new PositionKey { InstrumentName = position.InstrumentName, Side = position.Side, PositionDay = position.PositionDay };
                    }
                }

                return new PositionKey { InstrumentName = "Invalid!", Side = eOrderSide.osdUndefined, PositionDay = EPositionDay.dayUndefined };
            }

            public override bool Equals(object obj)
            {
                var item = obj as PositionKey;

                if (item == null)
                {
                    return false;
                }

                return InstrumentName == item.InstrumentName && Side == item.Side && PositionDay == item.PositionDay;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            private PositionKey() { }
        }

        /// Open position keys storage.
        private BindingList<PositionKey> m_positions = new BindingList<PositionKey>();

        #region " Windows Form Designer generated code "

        public frmAccountsAndPositions()
        {
            //This call is required by the Windows Form Designer.
            InitializeComponent();

            //Add any initialization after the InitializeComponent() call
            this.lstPositions.DataSource = m_positions;
            this.lstPositions.DisplayMember = "InstrumentName";
            this.lstPositions.ValueMember = "InstrumentName";
        }

        //Form overrides dispose to clean up the component list.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!(components == null))
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        //Required by the Windows Form Designer
        private System.ComponentModel.Container components = null;

        //Required by the Windows Form Designer

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        internal System.Windows.Forms.Label lblDataConnection;
        private System.Windows.Forms.LinkLabel llWeb;
        internal System.Windows.Forms.Label lblGWConnection;
        internal System.Windows.Forms.GroupBox GroupBox2;
        internal System.Windows.Forms.GroupBox GroupBox4;
        internal System.Windows.Forms.GroupBox GroupBox3;
        internal System.Windows.Forms.GroupBox gbProperties;
        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.GroupBox gbSummaries;
        internal System.Windows.Forms.GroupBox GroupBox5;
        internal System.Windows.Forms.ListBox lstCurrencies;
        internal System.Windows.Forms.ListBox lstAccounts;
        internal System.Windows.Forms.GroupBox GroupBox7;
        internal System.Windows.Forms.GroupBox gbPositions;
        internal System.Windows.Forms.GroupBox GroupBox6;
        internal System.Windows.Forms.ListBox lstPositions;
        internal System.Windows.Forms.PictureBox PictureBox1;
        internal System.Windows.Forms.ContextMenu ctxAccountsDetMenu;
        internal ComboBox cmbMarginsDetailingLevel;
        internal Label label1;
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.lblDataConnection = new System.Windows.Forms.Label();
            this.llWeb = new System.Windows.Forms.LinkLabel();
            this.lblGWConnection = new System.Windows.Forms.Label();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.lstAccounts = new System.Windows.Forms.ListBox();
            this.GroupBox4 = new System.Windows.Forms.GroupBox();
            this.gbProperties = new System.Windows.Forms.GroupBox();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.GroupBox5 = new System.Windows.Forms.GroupBox();
            this.lstCurrencies = new System.Windows.Forms.ListBox();
            this.gbSummaries = new System.Windows.Forms.GroupBox();
            this.GroupBox7 = new System.Windows.Forms.GroupBox();
            this.gbPositions = new System.Windows.Forms.GroupBox();
            this.GroupBox6 = new System.Windows.Forms.GroupBox();
            this.lstPositions = new System.Windows.Forms.ListBox();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.cmbMarginsDetailingLevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ctxAccountsDetMenu = new System.Windows.Forms.ContextMenu();
            this.GroupBox2.SuspendLayout();
            this.gbProperties.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.GroupBox5.SuspendLayout();
            this.GroupBox7.SuspendLayout();
            this.GroupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDataConnection
            // 
            this.lblDataConnection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDataConnection.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblDataConnection.Location = new System.Drawing.Point(40, 8);
            this.lblDataConnection.Name = "lblDataConnection";
            this.lblDataConnection.Size = new System.Drawing.Size(216, 16);
            this.lblDataConnection.TabIndex = 17;
            this.lblDataConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // llWeb
            // 
            this.llWeb.BackColor = System.Drawing.SystemColors.Control;
            this.llWeb.LinkArea = new System.Windows.Forms.LinkArea(18, 40);
            this.llWeb.Location = new System.Drawing.Point(704, 8);
            this.llWeb.Name = "llWeb";
            this.llWeb.Size = new System.Drawing.Size(336, 16);
            this.llWeb.TabIndex = 22;
            this.llWeb.TabStop = true;
            this.llWeb.Text = "CQG API web page: http://www.cqg.com/Products/CQG-API.aspx";
            this.llWeb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.llWeb.UseCompatibleTextRendering = true;
            this.llWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llWeb_LinkClicked);
            // 
            // lblGWConnection
            // 
            this.lblGWConnection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblGWConnection.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblGWConnection.Location = new System.Drawing.Point(312, 8);
            this.lblGWConnection.Name = "lblGWConnection";
            this.lblGWConnection.Size = new System.Drawing.Size(216, 16);
            this.lblGWConnection.TabIndex = 48;
            this.lblGWConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.lstAccounts);
            this.GroupBox2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GroupBox2.Location = new System.Drawing.Point(8, 48);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(1320, 128);
            this.GroupBox2.TabIndex = 51;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Accounts";
            // 
            // lstAccounts
            // 
            this.lstAccounts.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstAccounts.ItemHeight = 16;
            this.lstAccounts.Location = new System.Drawing.Point(8, 20);
            this.lstAccounts.Name = "lstAccounts";
            this.lstAccounts.Size = new System.Drawing.Size(1305, 100);
            this.lstAccounts.TabIndex = 1;
            this.lstAccounts.SelectedIndexChanged += new System.EventHandler(this.lstAccounts_SelectedIndexChanged);
            this.lstAccounts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstAccounts_MouseDown);
            // 
            // GroupBox4
            // 
            this.GroupBox4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GroupBox4.Location = new System.Drawing.Point(8, 296);
            this.GroupBox4.Name = "GroupBox4";
            this.GroupBox4.Size = new System.Drawing.Size(816, 240);
            this.GroupBox4.TabIndex = 45;
            this.GroupBox4.TabStop = false;
            // 
            // gbProperties
            // 
            this.gbProperties.Controls.Add(this.GroupBox3);
            this.gbProperties.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.gbProperties.Location = new System.Drawing.Point(8, 176);
            this.gbProperties.Name = "gbProperties";
            this.gbProperties.Size = new System.Drawing.Size(472, 284);
            this.gbProperties.TabIndex = 51;
            this.gbProperties.TabStop = false;
            this.gbProperties.Text = "Account\'s Properties";
            // 
            // GroupBox3
            // 
            this.GroupBox3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GroupBox3.Location = new System.Drawing.Point(8, 296);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(816, 240);
            this.GroupBox3.TabIndex = 45;
            this.GroupBox3.TabStop = false;
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.GroupBox5);
            this.GroupBox1.Controls.Add(this.gbSummaries);
            this.GroupBox1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.GroupBox1.Location = new System.Drawing.Point(488, 217);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(840, 491);
            this.GroupBox1.TabIndex = 55;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Account\'s Summaries";
            // 
            // GroupBox5
            // 
            this.GroupBox5.Controls.Add(this.lstCurrencies);
            this.GroupBox5.Font = new System.Drawing.Font("Arial", 8.25F);
            this.GroupBox5.Location = new System.Drawing.Point(753, 21);
            this.GroupBox5.Name = "GroupBox5";
            this.GroupBox5.Size = new System.Drawing.Size(80, 464);
            this.GroupBox5.TabIndex = 56;
            this.GroupBox5.TabStop = false;
            this.GroupBox5.Text = "Currencies";
            // 
            // lstCurrencies
            // 
            this.lstCurrencies.ItemHeight = 14;
            this.lstCurrencies.Location = new System.Drawing.Point(6, 19);
            this.lstCurrencies.Name = "lstCurrencies";
            this.lstCurrencies.Size = new System.Drawing.Size(64, 438);
            this.lstCurrencies.TabIndex = 0;
            this.lstCurrencies.SelectedIndexChanged += new System.EventHandler(this.lstCurrencies_SelectedIndexChanged);
            // 
            // gbSummaries
            // 
            this.gbSummaries.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.gbSummaries.Location = new System.Drawing.Point(9, 21);
            this.gbSummaries.Name = "gbSummaries";
            this.gbSummaries.Size = new System.Drawing.Size(738, 464);
            this.gbSummaries.TabIndex = 55;
            this.gbSummaries.TabStop = false;
            // 
            // GroupBox7
            // 
            this.GroupBox7.Controls.Add(this.gbPositions);
            this.GroupBox7.Controls.Add(this.GroupBox6);
            this.GroupBox7.Font = new System.Drawing.Font("Arial", 9.75F);
            this.GroupBox7.Location = new System.Drawing.Point(8, 466);
            this.GroupBox7.Name = "GroupBox7";
            this.GroupBox7.Size = new System.Drawing.Size(472, 242);
            this.GroupBox7.TabIndex = 58;
            this.GroupBox7.TabStop = false;
            this.GroupBox7.Text = "Account\'s Positions";
            // 
            // gbPositions
            // 
            this.gbPositions.Font = new System.Drawing.Font("Arial", 9.5F);
            this.gbPositions.Location = new System.Drawing.Point(170, 6);
            this.gbPositions.Name = "gbPositions";
            this.gbPositions.Size = new System.Drawing.Size(296, 230);
            this.gbPositions.TabIndex = 59;
            this.gbPositions.TabStop = false;
            // 
            // GroupBox6
            // 
            this.GroupBox6.Controls.Add(this.lstPositions);
            this.GroupBox6.Font = new System.Drawing.Font("Arial", 8.25F);
            this.GroupBox6.Location = new System.Drawing.Point(6, 15);
            this.GroupBox6.Name = "GroupBox6";
            this.GroupBox6.Size = new System.Drawing.Size(152, 221);
            this.GroupBox6.TabIndex = 58;
            this.GroupBox6.TabStop = false;
            this.GroupBox6.Text = "Positions";
            // 
            // lstPositions
            // 
            this.lstPositions.HorizontalScrollbar = true;
            this.lstPositions.ItemHeight = 14;
            this.lstPositions.Location = new System.Drawing.Point(8, 20);
            this.lstPositions.Name = "lstPositions";
            this.lstPositions.Size = new System.Drawing.Size(136, 186);
            this.lstPositions.TabIndex = 1;
            this.lstPositions.SelectedIndexChanged += new System.EventHandler(this.lstPositions_SelectedIndexChanged);
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Black;
            this.PictureBox1.Location = new System.Drawing.Point(0, 32);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(1208, 8);
            this.PictureBox1.TabIndex = 59;
            this.PictureBox1.TabStop = false;
            // 
            // cmbMarginsDetailingLevel
            // 
            this.cmbMarginsDetailingLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMarginsDetailingLevel.Location = new System.Drawing.Point(1114, 176);
            this.cmbMarginsDetailingLevel.Name = "cmbMarginsDetailingLevel";
            this.cmbMarginsDetailingLevel.Size = new System.Drawing.Size(214, 21);
            this.cmbMarginsDetailingLevel.TabIndex = 60;
            this.cmbMarginsDetailingLevel.SelectedIndexChanged += new System.EventHandler(this.cmbMarginsDetailingLevel_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(939, 179);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 13);
            this.label1.TabIndex = 61;
            this.label1.Text = "All accounts margin detailing level:";
            // 
            // ctxAccountsDetMenu
            // 
            this.ctxAccountsDetMenu.Popup += new System.EventHandler(this.ctxAccountsDetMenu_Popup);
            // 
            // frmAccountsAndPositions
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1340, 720);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.GroupBox7);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.cmbMarginsDetailingLevel);
            this.Controls.Add(this.gbProperties);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.lblGWConnection);
            this.Controls.Add(this.lblDataConnection);
            this.Controls.Add(this.llWeb);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmAccountsAndPositions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Accounts and Positions";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmAccountsAndPositions_Closing);
            this.Load += new System.EventHandler(this.frmAccountsAndPositions_Load);
            this.GroupBox2.ResumeLayout(false);
            this.gbProperties.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox5.ResumeLayout(false);
            this.GroupBox7.ResumeLayout(false);
            this.GroupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// Creates an m_CEL object, change its configurations, starts up and initializes HashTable and grids.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void frmAccountsAndPositions_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                InitHashTables();
                InitPropertyFields();
                InitSummariesFields();
                InitPositionsFields();
                InitMarginsDetailingSelectors();

                CEL_DataConnectionStatusChanged(eConnectionStatus.csConnectionDown);
                CEL_GWConnectionStatusChanged(eConnectionStatus.csConnectionDown);

                m_CEL = new CQGCEL();
                m_CEL.DataConnectionStatusChanged += new CQG._ICQGCELEvents_DataConnectionStatusChangedEventHandler(CEL_DataConnectionStatusChanged);
                m_CEL.GWConnectionStatusChanged += new CQG._ICQGCELEvents_GWConnectionStatusChangedEventHandler(CEL_GWConnectionStatusChanged);
                m_CEL.AccountChanged += new CQG._ICQGCELEvents_AccountChangedEventHandler(CEL_AccountChanged);
                m_CEL.InstrumentResolved += new CQG._ICQGCELEvents_InstrumentResolvedEventHandler(CEL_InstrumentResolved);
                m_CEL.DataError += new CQG._ICQGCELEvents_DataErrorEventHandler(CEL_DataError);
                m_CEL.APIConfiguration.CollectionsThrowException = false;
                m_CEL.APIConfiguration.DefPositionSubscriptionLevel = ePositionSubscriptionLevel.pslSnapshotAndUpdates;
                m_CEL.APIConfiguration.FireEventOnChangedPrices = true;
                m_CEL.APIConfiguration.UseOrderSide = true;
                m_CEL.APIConfiguration.PositionDetailing = ePositionDetailing.pdAllTrades;
                m_CEL.APIConfiguration.ReadyStatusCheck = eReadyStatusCheck.rscOff;
                m_CEL.APIConfiguration.DefaultAccountMarginDetailing = eAccountMarginDetailing.amdNoMargin;
                m_CEL.APIConfiguration.AccountMarginAndPositionsThrottleInterval = 300;
                m_CEL.Startup();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "frmAccountsAndPositions_Load", ex);
            }
        }

        /// <summary>
        /// Initializes the margins detailing level combobox and the context menu
        /// </summary>
        private void InitMarginsDetailingSelectors()
        {
            EventHandler contextMenuItemEventHandler = new EventHandler(ctxAccountsDetMenu_ItemClicked);
            cmbMarginsDetailingLevel.Items.Add(string.Empty);
            foreach (string curMarginDetailing in Enum.GetNames(typeof(eAccountMarginDetailing)))
            {
                if (curMarginDetailing == "amdMarginSnapshot") // Depricated
                {
                    continue;
                }
                string item = curMarginDetailing.Substring(3);
                ctxAccountsDetMenu.MenuItems.Add(item, contextMenuItemEventHandler);
                cmbMarginsDetailingLevel.Items.Add(item);
            }

            cmbMarginsDetailingLevel.Enabled = false;
        }

        /// <summary>
        /// Initializes the HashTables.
        /// </summary>
        private void InitHashTables()
        {
            m_PropertyToRow = new Hashtable();
            m_PropertyToRow.Add(DATE_OF_LAST_STATEMENT, 0);
            m_PropertyToRow.Add(FCM_ACCOUNT_ID, 1);
            m_PropertyToRow.Add(FCM_ID, 2);
            m_PropertyToRow.Add(FCM_NAME, 3);
            m_PropertyToRow.Add(GW_ACCOUNT_ID, 4);
            m_PropertyToRow.Add(GW_ACCOUNT_NAME, 5);
            m_PropertyToRow.Add(POSITION_SUBSCRIPTION_LEVEL, 6);
            m_PropertyToRow.Add(REPORTING_CURRENCY, 7);
            m_PropertyToRow.Add(SERVER_TIMESTAMP, 8);
            m_PropertyToRow.Add(TIMESTAMP, 9);
            m_PropertyToRow.Add(MARGIN_EXCESS, 10);
            m_PropertyToRow.Add(TOTAL_MARGIN, 11);
            m_PropertyToRow.Add(PURCHASING_POWER, 12);
            m_PropertyToRow.Add(MARGIN_CREDIT, 13);

            m_SummaryToRow = new Hashtable();
            m_SummaryToRow.Add(ACCOUNT_BALANCE, 1);
            m_SummaryToRow.Add(INITIAL_MARGIN, 2);
            m_SummaryToRow.Add(MAINTENANCE_MARGIN, 3);
            m_SummaryToRow.Add(OPEN_TRADE_EQUITY, 4);
            m_SummaryToRow.Add(UNREALIZED_PROFIT_LOSS_FOR_OPTIONS, 5);
            m_SummaryToRow.Add(COLLATERAL_ON_DEPOSIT, 6);
            m_SummaryToRow.Add(NET_LIQUIDITY_VALUE, 7);
            m_SummaryToRow.Add(MARKET_VALUE_OF_OPTIONS, 8);
            m_SummaryToRow.Add(CASH_EXCESS, 9);

            m_PositionToRow = new Hashtable();
            m_PositionToRow.Add(AVERAGE_PRICE, 0);
            m_PositionToRow.Add(MARKET_VALUE_OPTION, 1);
            m_PositionToRow.Add(OPEN_TRADE_EQUITY, 2);
            m_PositionToRow.Add(PROFIT_LOSS, 3);
            m_PositionToRow.Add(LONG_, 4);
            m_PositionToRow.Add(SHORT_, 5);
            m_PositionToRow.Add(OFFSET, 6);
            m_PositionToRow.Add(SERVER_TIMESTAMP, 7);
            m_PositionToRow.Add(TIMESTAMP, 8);
            m_PositionToRow.Add(CURRENCY, 9);
            m_PositionToRow.Add(POSITION_TRACKING_TYPE, 10);
            m_PositionToRow.Add(POSITION_DAY, 11);

        }

        /// <summary>
        /// Creates and initializes the grid of properties.
        /// </summary>
        private void InitPropertyFields()
        {
            m_PropertiesGrid = new CQGMiniGrid(2, m_PropertyToRow.Count, gbProperties);

            m_PropertiesGrid.SetColumnWidth(0, 172);
            m_PropertiesGrid.SetColumnWidth(1, 278);

            m_PropertiesGrid.SetHeaderValue(0, "Property Name");
            m_PropertiesGrid.SetHeaderValue(1, "Value");

            foreach (DictionaryEntry item in m_PropertyToRow)
            {
                m_PropertiesGrid[(int)(item.Value), 0].Text = item.Key.ToString();
            }

            m_PropertiesGrid.ReDraw();
        }

        /// <summary>
        /// Creates and initializes the grid of summaries.
        /// </summary>
        private void InitSummariesFields()
        {
            int itemsCount = m_SummaryToRow.Count + 1;

            m_SummariesGrid = new CQGMiniGrid(5, itemsCount * 2, gbSummaries);

            m_SummariesGrid.SetColumnWidth(0, 225);
            m_SummariesGrid.SetColumnAlign(0, ContentAlignment.MiddleLeft);
            m_SummariesGrid.SetHeaderValue(0, "");

            m_SummariesGrid.SetColumnWidth(1, 125);
            m_SummariesGrid.SetColumnAlign(1, ContentAlignment.MiddleRight);
            m_SummariesGrid.SetHeaderValue(1, "Total");

            m_SummariesGrid.SetColumnWidth(2, 125);
            m_SummariesGrid.SetColumnAlign(2, ContentAlignment.MiddleRight);
            m_SummariesGrid.SetHeaderValue(2, "Total (w/o NA)");

            m_SummariesGrid.SetColumnWidth(3, 125);
            m_SummariesGrid.SetColumnAlign(3, ContentAlignment.MiddleRight);
            m_SummariesGrid.SetHeaderValue(3, "");

            m_SummariesGrid.SetColumnWidth(4, 125);
            m_SummariesGrid.SetColumnAlign(4, ContentAlignment.MiddleRight);
            m_SummariesGrid.SetHeaderValue(4, "");

            m_SummariesGrid[0, 0].Text = "CURRENT VALUES";
            m_SummariesGrid[0, 0].TextAlign = ContentAlignment.MiddleCenter;
            m_SummariesGrid[0, 0].Font = new Font("Arial", 10, FontStyle.Italic);

            m_SummariesGrid[itemsCount, 0].Text = "YESTERDAY'S CLOSE VALUES";
            m_SummariesGrid[itemsCount, 0].TextAlign = ContentAlignment.MiddleCenter;
            m_SummariesGrid[itemsCount, 0].Font = new Font("Arial", 10, FontStyle.Italic);

            int row;
            foreach (DictionaryEntry item in m_SummaryToRow)
            {
                /// Every item here must be added twice: for current values and yesterdays close values
                /// Also we are adding 1, because we have headers seperated from the hashtable
                for (int i = 0; i <= 1; i++)
                {
                    row = (int)(item.Value) + itemsCount * i;
                    m_SummariesGrid[row, 0].Text = item.Key.ToString();
                }
            }

            m_SummariesGrid.ReDraw();
        }

        /// <summary>
        /// Create and initialize the grid of positions.
        /// </summary>
        private void InitPositionsFields()
        {
            m_PositionsGrid = new CQGMiniGrid(2, m_PositionToRow.Count, gbPositions);

            m_PositionsGrid.SetColumnWidth(0, 130);
            m_PositionsGrid.SetColumnAlign(0, ContentAlignment.MiddleLeft);
            m_PositionsGrid.SetHeaderValue(0, "Properties");

            m_PositionsGrid.SetColumnWidth(1, 150);
            m_PositionsGrid.SetColumnAlign(1, ContentAlignment.MiddleLeft);
            m_PositionsGrid.SetHeaderValue(1, "Value");

            foreach (DictionaryEntry item in m_PositionToRow)
            {
                m_PositionsGrid[(int)(item.Value), 0].Text = item.Key.ToString();
            }

            m_PositionsGrid.ReDraw();
        }

        /// <summary>
        /// Shuts down m_CEL.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A CancelEventArgs that contains the event data.
        /// </param>
        private void frmAccountsAndPositions_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                m_CEL.Shutdown();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "frmAccountsAndPositions_Closing", ex);
            }
        }

        /// <summary>
        /// This event is fired, when some changes occur in the connection with the CQG data server.
        /// </summary>
        /// <param name="newStatus">
        /// The current status of the connection with the data server.
        /// </param>
        private void CEL_DataConnectionStatusChanged(eConnectionStatus newStatus)
        {
            try
            {
                string info;
                System.Drawing.Color BackCol;

                if (newStatus != eConnectionStatus.csConnectionUp)
                {
                    BackCol = System.Drawing.Color.FromArgb(255, 114, 0);
                    info = "DATA Connection is " + (newStatus == eConnectionStatus.csConnectionDelayed ? "Delayed" : "Down").ToString();
                }
                else
                {
                    BackCol = System.Drawing.Color.FromArgb(192, 209, 205);
                    info = "DATA Connection is UP";
                }

                lblDataConnection.BackColor = BackCol;
                lblDataConnection.Text = info;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "CEL_DataConnectionStatusChanged", ex);
            }
        }

        /// <summary>
        /// This event is fired when the status of the connection with the CQG Gateway is changed.
        /// </summary>
        /// <param name="newStatus">
        /// The current status of the connection with CQG Gateway.
        /// </param>
        private void CEL_GWConnectionStatusChanged(CQG.eConnectionStatus newStatus)
        {
            try
            {
                string info;
                System.Drawing.Color BackCol;

                if (newStatus != eConnectionStatus.csConnectionUp)
                {
                    BackCol = System.Drawing.Color.FromArgb(255, 114, 0);
                    info = "GW Connection is " + (newStatus == eConnectionStatus.csConnectionDelayed ?
                                                                 "Delayed" : "Down").ToString();
                    /// Clearing all shown data, because after GW connection up we will receive all data again.
                    lstAccounts.Items.Clear();
                    DisplayProperties(null, false);
                }
                else
                {
                    BackCol = System.Drawing.Color.FromArgb(192, 209, 205);
                    info = "GW Connection is UP";
                    m_CEL.AccountSubscriptionLevel = eAccountSubscriptionLevel.aslAccountsAndUpdates;
                }

                lblGWConnection.BackColor = BackCol;
                lblGWConnection.Text = info;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "CEL_GWConnectionStatusChanged", ex);
            }
        }

        /// <summary>
        /// This event is fired, when the account or position information is changed.
        /// </summary>
        /// <param name="change">
        /// Account change type, which allows to differentiate the occurred changes.
        /// </param>
        /// <param name="Account">
        /// A CQGAccount object, representing the account to which the current change refers.
        /// </param>
        /// <param name="position">
        /// A CQGPosition object, representing the position to which the current change refers.
        /// </param>
        private void CEL_AccountChanged(CQG.eAccountChangeType change, CQGAccount account, CQG.CQGPosition position)
        {
            try
            {
                switch (change)
                {
                    case eAccountChangeType.actAccountsReloaded:

                        lstAccounts.Items.Clear();
                        foreach (CQGAccount acc in m_CEL.Accounts)
                        {
                            lstAccounts.Items.Add(acc.GWAccountID.ToString().PadRight(LIST_ACC_COL_WIDTH) +
                                              acc.GWAccountName.PadRight(LIST_ACC_COL_WIDTH) +
                                              acc.FcmAccountID.PadRight(LIST_ACC_COL_WIDTH) +
                                              acc.FcmID.ToString().PadRight(LIST_ACC_COL_WIDTH));
                            acc.AutoSubscribeInstruments = true;
                        }
                        lstAccounts.SelectedIndex = 0;
                        cmbMarginsDetailingLevel.Enabled = true;
                        cmbMarginsDetailingLevel.Text = m_CEL.APIConfiguration.DefaultAccountMarginDetailing.ToString().Substring(3);
                        break;
                    case eAccountChangeType.actAccountChanged:

                        if (account.GWAccountID == GetSelectedGWID())
                        {
                            DisplayProperties(account, false);
                        }
                        break;
                    case eAccountChangeType.actPositionsReloaded:

                        if (account.GWAccountID == GetSelectedGWID())
                        {
                            DisplaySummaries(account.CurrencySummaries, false);
                            DisplaySummary(account.Summary, 1);
                            DisplaySummary(account.Summary, 2);

                            DisplayPositions(account.Positions);
                            if (lstPositions.Items.Count > 0)
                            {
                                lstPositions.SelectedIndex = 0;
                            }
                        }
                        break;
                    case eAccountChangeType.actPositionChanged:

                        if (account.GWAccountID == GetSelectedGWID())
                        {
                            DisplayDynamicProperties(account);
                            DisplaySummaries(account.CurrencySummaries, false);
                            DisplaySummary(account.Summary, 1);
                            DisplaySummary(account.Summary, 2);

                            if (lstPositions.SelectedIndex != -1)
                            {
                                var key = m_positions[lstPositions.SelectedIndex];

                                if (PositionKey.Create(position).Equals(key))
                                {
                                    DisplayPosition(position);
                                }
                            }
                        }
                        break;
                    case eAccountChangeType.actPositionAdded:

                        if (account.GWAccountID == GetSelectedGWID())
                        {
                            DisplaySummaries(account.CurrencySummaries, false);
                            DisplaySummary(account.Summary, 1);
                            DisplaySummary(account.Summary, 2);

                            m_positions.Add(PositionKey.Create(position));

                            if (m_positions.Count == 1)
                            {
                                lstPositions.SelectedIndex = 0;
                            }
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "CEL_AccountChanged", ex);
            }
        }
        /// <summary>
        /// This event is fired when a new instrument is resolved and subscribed.
        /// </summary>
        /// <param name="symbol">
        /// The commodity symbol that was requested by user in NewInstrument method.
        /// </param>
        /// <param name="instrument">
        /// Resolved CQGInstrument object
        /// </param>
        /// <param name= "cqg_error">
        /// Error happened during a resolution process.
        /// </param>
        private void CEL_InstrumentResolved(string symbol, CQG.CQGInstrument instrument, CQG.CQGError cqg_error)
        {
            try
            {
                if (m_CEL.IsValid(cqg_error))
                {
                    ErrorHandler.HandleError("frmAccountsAndPositions", "CEL_InstrumentResolved", cqg_error);
                    return;
                }

                int selectedGWID = GetSelectedGWID();
                if (selectedGWID > 0)
                {
                    DisplayPosition(m_CEL.Accounts[selectedGWID].Positions[GetListboxText(lstPositions)]);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "CEL_InstrumentResolved", ex);
            }
        }

        /// <summary>
        /// This event is fired, when CQGCEL detects some abnormal discrepancy between data expected and data received.
        /// </summary>
        /// <param name="obj">
        /// Object, in which the error occurred.
        /// </param>
        /// <param name="errorDescription">
        /// String, describing the error.
        /// </param>
        private void CEL_DataError(object obj, string errorDescription)
        {
            try
            {
                CQGError cqgErr = obj as CQGError;
                if (m_CEL.IsValid(cqgErr))
                {
                    if (cqgErr.Code == 102)
                    {
                        errorDescription += " Restart the application.";
                    }
                    else if (cqgErr.Code == 125)
                    {
                        errorDescription += " Turn on CQG Client and restart the application.";
                    }
                }

                MessageBox.Show(errorDescription, "AccountsAndPositions", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "CEL_DataError", ex);
            }
        }

        /// <summary>
        /// Displays all available data for given account.
        /// </summary>
        /// <param name="acc">
        /// Account, which data must be shown.
        /// </param>
        /// <param name="reloaded">
        /// Boolean parameter, which shows if info must be shown for the first time, or must be updated
        /// </param>
        private void DisplayProperties(CQGAccount acc, bool reloaded)
        {
            if (acc == null)
            {
                m_PropertiesGrid.ClearColumn(1);
                DisplaySummaries(null, false);
                DisplayPositions(null);
                return;
            }

            DisplaySummaries(acc.CurrencySummaries, reloaded);
            DisplayPositions(acc.Positions);
            if (m_CEL.IsValid(acc.DateOfLastStatement))
            {
                m_PropertiesGrid[(int)(m_PropertyToRow[DATE_OF_LAST_STATEMENT]), 1].Text = acc.DateOfLastStatement.ToShortDateString();
            }
            else
            {
                m_PropertiesGrid[(int)(m_PropertyToRow[DATE_OF_LAST_STATEMENT]), 1].Text = N_A;
            }
            m_PropertiesGrid[(int)(m_PropertyToRow[FCM_ACCOUNT_ID]), 1].Text = acc.FcmAccountID;
            m_PropertiesGrid[(int)(m_PropertyToRow[FCM_ID]), 1].Text = acc.FcmID.ToString();
            m_PropertiesGrid[(int)(m_PropertyToRow[FCM_NAME]), 1].Text = acc.FcmName;
            m_PropertiesGrid[(int)(m_PropertyToRow[GW_ACCOUNT_ID]), 1].Text = acc.GWAccountID.ToString();
            m_PropertiesGrid[(int)(m_PropertyToRow[GW_ACCOUNT_NAME]), 1].Text = acc.GWAccountName;
            m_PropertiesGrid[(int)(m_PropertyToRow[POSITION_SUBSCRIPTION_LEVEL]), 1].Text = acc.PositionSubcriptionLevel.ToString();
            m_PropertiesGrid[(int)(m_PropertyToRow[REPORTING_CURRENCY]), 1].Text = acc.ReportingCurrency;
            m_PropertiesGrid[(int)(m_PropertyToRow[TIMESTAMP]), 1].Text = ValToString(acc.Timestamp);
            m_PropertiesGrid[(int)(m_PropertyToRow[SERVER_TIMESTAMP]), 1].Text = ValToString(acc.ServerTimestamp);
            DisplayDynamicProperties(acc);
            DisplaySummary(acc.Summary, 1);
            DisplaySummary(acc.Summary, 2);
        }

        private void DisplayDynamicProperties(CQGAccount acc)
        {
            m_PropertiesGrid[(int)(m_PropertyToRow[TOTAL_MARGIN]), 1].Text = ValToString(acc.TotalMarginValue);
            m_PropertiesGrid[(int)(m_PropertyToRow[MARGIN_CREDIT]), 1].Text = ValToString(acc.MarginCredit);
            m_PropertiesGrid[(int)(m_PropertyToRow[PURCHASING_POWER]), 1].Text = ValToString(acc.TotalPurchasingPower);
            m_PropertiesGrid[(int)(m_PropertyToRow[MARGIN_EXCESS]), 1].Text = ValToString(acc.MarginExcess);
        }


        /// <summary>
        /// If param "reloaded" is true, adds currencies of all summaries to listbox and
        /// shows all available data for the first summary.
        /// </summary>
        /// <param name="summaries">
        /// Collection of all available summaries for the selected account
        /// </param>
        /// <param name="reloaded">
        /// Boolean parameter, which shows if info must be shown for the first time, or must be updated
        /// </param>
        private void DisplaySummaries(CQGAccountSummaries summaries, bool reloaded)
        {
            if (summaries == null)
            {
                m_SummariesGrid.ClearColumn(1);
                m_SummariesGrid.ClearColumn(2);
                m_SummariesGrid.ClearColumn(3);
                m_SummariesGrid.ClearColumn(4);
                lstCurrencies.Items.Clear();
                return;
            }

            if (reloaded)
            {
                lstCurrencies.Items.Clear();
                foreach (CQGAccountSummary summary in summaries)
                {
                    lstCurrencies.Items.Add(summary.CurrencyName);
                }
                if (lstCurrencies.Items.Count > 0)
                {
                    lstCurrencies.SelectedIndex = 0;
                }
                else
                {
                    m_SummariesGrid.ClearColumn(3);
                    m_SummariesGrid.ClearColumn(4);
                }
                m_SummariesGrid.SetHeaderValue(3, GetListboxText(lstCurrencies));
                m_SummariesGrid.SetHeaderValue(4, makeNASuppressHeader(GetListboxText(lstCurrencies)));
            }

            foreach (CQGAccountSummary summary in summaries)
            {
                if (summary.CurrencyName == GetListboxText(lstCurrencies))
                {
                    DisplaySummary(summary, 3);
                    DisplaySummary(summary, 4);
                    break;
                }
            }
        }

        /// <summary>
        /// Displays all available data for the given summary.
        /// </summary>
        /// <param name="summary">
        /// Summary, which data must be shown.
        /// </param>
        /// <param name="col">
        /// Column in which data must be dumped.
        /// </param>
        private void DisplaySummary(CQGAccountSummary summary, int col)
        {
            int row;

            summary.SuppressNotAvailableValues = (col == 2 || col == 4);

            for (int i = 0; i <= 1; i++)
            {
                /// Calculating first row in grid for today's and yesterday's summaries.
                int summaryStartRow = (m_SummaryToRow.Count + 1) * i;

                if (i == 1)
                {
                    row = (int)(m_SummaryToRow[COLLATERAL_ON_DEPOSIT]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = ValToString(summary.Collaterals(i));

                    row = (int)(m_SummaryToRow[CASH_EXCESS]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = ValToString(summary.CashExcess(i));

                    row = (int)(m_SummaryToRow[INITIAL_MARGIN]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = ValToString(summary.InitialMargin(i));

                    row = (int)(m_SummaryToRow[MAINTENANCE_MARGIN]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = ValToString(summary.MaintenanceMargin(i));
                }
                else
                {
                    row = (int)(m_SummaryToRow[COLLATERAL_ON_DEPOSIT]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = N_A;

                    row = (int)(m_SummaryToRow[CASH_EXCESS]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = N_A;

                    row = (int)(m_SummaryToRow[INITIAL_MARGIN]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = N_A;

                    row = (int)(m_SummaryToRow[MAINTENANCE_MARGIN]) + summaryStartRow;
                    m_SummariesGrid[row, col].Text = N_A;
                }

                row = (int)(m_SummaryToRow[ACCOUNT_BALANCE]) + summaryStartRow;
                m_SummariesGrid[row, col].Text = ValToString(summary.Balance(i));

                row = (int)(m_SummaryToRow[MARKET_VALUE_OF_OPTIONS]) + summaryStartRow;
                m_SummariesGrid[row, col].Text = ValToString(summary.MVO(i));

                row = (int)(m_SummaryToRow[NET_LIQUIDITY_VALUE]) + summaryStartRow;
                m_SummariesGrid[row, col].Text = ValToString(summary.NLV(i));

                row = (int)(m_SummaryToRow[OPEN_TRADE_EQUITY]) + summaryStartRow;
                m_SummariesGrid[row, col].Text = ValToString(summary.OTE(i));

                row = (int)(m_SummaryToRow[UNREALIZED_PROFIT_LOSS_FOR_OPTIONS]) + summaryStartRow;
                m_SummariesGrid[row, col].Text = ValToString(summary.UPL(i));
            }
        }

        /// <summary>
        /// Adds all positions' instrument's fullnames to listbox.
        /// </summary>
        /// <param name="positions">
        /// Collection of all available positions for selected account
        /// </param>
        private void DisplayPositions(CQGPositions positions)
        {
            m_positions.Clear();
            lstPositions.SelectedIndex = -1;
            m_PositionsGrid.ClearColumn(1);

            if (positions == null)
            {
                DisplayPosition(null);
                return;
            }

            foreach (CQGPosition pos in positions)
            {
                m_positions.Add(PositionKey.Create(pos));
            }
        }

        /// <summary>
        /// Displays all available data for the given position.
        /// </summary>
        /// <param name="pos">
        /// Position, which data must be shown.
        /// </param>
        private void DisplayPosition(CQGPosition pos)
        {
            if (pos == null)
            {
                m_PositionsGrid.ClearColumn(1);
                return;
            }

            m_PositionsGrid[(int)(m_PositionToRow[MARKET_VALUE_OPTION]), 1].Text = ValToString(pos.MVO);
            m_PositionsGrid[(int)(m_PositionToRow[OPEN_TRADE_EQUITY]), 1].Text = ValToString(pos.OTE);
            m_PositionsGrid[(int)(m_PositionToRow[PROFIT_LOSS]), 1].Text = ValToString(pos.ProfitLoss);
            m_PositionsGrid[(int)(m_PositionToRow[POSITION_TRACKING_TYPE]), 1].Text = ValToString(pos.TrackingType);
            m_PositionsGrid[(int)(m_PositionToRow[POSITION_DAY]), 1].Text = ValToString(pos.PositionDay);

            string quantity = (pos.QuantityFractional == 0 ? "-" : pos.QuantityFractional.ToString());
            if (pos.Side == eOrderSide.osdBuy)
            {
                m_PositionsGrid[(int)(m_PositionToRow[SHORT_]), 1].Text = "-";
                m_PositionsGrid[(int)(m_PositionToRow[LONG_]), 1].Text = quantity;
            }
            else
            {
                m_PositionsGrid[(int)(m_PositionToRow[SHORT_]), 1].Text = quantity;
                m_PositionsGrid[(int)(m_PositionToRow[LONG_]), 1].Text = "-";
            }

            if (pos.MatchedTrades != null)
            {
                decimal offset = 0;
                foreach (CQGTrade Trade in pos.MatchedTrades)
                {
                    offset += Trade.QuantityFractional;
                }
                m_PositionsGrid[(int)(m_PositionToRow[OFFSET]), 1].Text = ValToString(offset);
            }
            else
            {
                m_PositionsGrid[(int)(m_PositionToRow[OFFSET]), 1].Text = N_A;
            }

            m_PositionsGrid[(int)(m_PositionToRow[SERVER_TIMESTAMP]), 1].Text = ValToString(pos.ServerTimestamp);
            m_PositionsGrid[(int)(m_PositionToRow[TIMESTAMP]), 1].Text = ValToString(pos.Timestamp);

            CQGInstrument instrument = pos.Instrument;
            if (instrument != null)
            {
                m_PositionsGrid[(int)(m_PositionToRow[CURRENCY]), 1].Text = ValToString(instrument.Currency);
                if (m_CEL.IsValid(pos.AveragePrice))
                {
                    m_PositionsGrid[(int)(m_PositionToRow[AVERAGE_PRICE]), 1].Text = instrument.ToDisplayPrice(pos.AveragePrice);
                }
                else
                {
                    m_PositionsGrid[(int)(m_PositionToRow[AVERAGE_PRICE]), 1].Text = N_A;
                }
            }
            else
            {
                m_PositionsGrid[(int)(m_PositionToRow[CURRENCY]), 1].Text = N_A;
                m_PositionsGrid[(int)(m_PositionToRow[AVERAGE_PRICE]), 1].Text = N_A;
            }
        }

        /// <summary>
        /// Displays information for the selected account.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void lstAccounts_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (GetSelectedGWID() > 0)
                {
                    lstAccounts.ContextMenu = ctxAccountsDetMenu;
                    DisplayProperties(m_CEL.Accounts[GetSelectedGWID()], true);
                }
                else
                {
                    lstAccounts.ContextMenu = null;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "lstAccounts_SelectedIndexChanged", ex);
            }
        }

        /// <summary>
        /// Makes header for 3-rd column in summary grid.
        /// </summary>
        private static string makeNASuppressHeader(string headerWithoutNASuppress)
        {
            return string.IsNullOrEmpty(headerWithoutNASuppress) ? "" : headerWithoutNASuppress + " (w/o NA)";
        }

        /// <summary>
        /// Displays summary for selected currency.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void lstCurrencies_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            try
            {
                m_SummariesGrid.SetHeaderValue(3, GetListboxText(lstCurrencies));
                m_SummariesGrid.SetHeaderValue(4, makeNASuppressHeader(GetListboxText(lstCurrencies)));
                DisplaySummaries(m_CEL.Accounts[GetSelectedGWID()].CurrencySummaries, false);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "lstCurrencies_SelectedIndexChanged", ex);
            }
        }

        /// <summary>
        /// Gets selected position object
        /// </summary>
        private CQGPosition getSelectedPosition()
        {
            if (lstPositions.SelectedIndex == -1)
            {
                return null;
            }

            var positions = m_CEL.Accounts[GetSelectedGWID()].Positions.FiterByInstrumentName(GetListboxText(lstPositions));
            var selectedPositionKey = m_positions[lstPositions.SelectedIndex];
            foreach (CQGPosition position in positions)
            {
                if (selectedPositionKey.Equals(PositionKey.Create(position)))
                {
                    return position;
                }
            }

            return null;
        }

        /// <summary>
        /// Displays position info for the selected instrument.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        private void lstPositions_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (GetSelectedGWID() > 0)
                {
                    DisplayPosition(getSelectedPosition());
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "lstPositions_SelectedIndexChanged", ex);
            }
        }

        /// <summary>
        /// Open CQG API web page.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A LinkLabelLinkClickedEventArgs that contains the event data.
        /// </param>
        private void llWeb_LinkClicked(System.Object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("iexplore.exe", "http://www.cqg.com/Products/CQG-API.aspx");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError("frmAccountsAndPositions", "llWeb_LinkClicked", ex);
            }
        }

        /// <summary>
        /// Converts input param to string.
        /// </summary>
        /// <param name="val">
        /// Input param, which must be converted to string, if is not nothing and is valid.
        /// </param>
        /// <returns>
        /// "0" if input is nothing or is not valid, otherwise input param is converted to string.
        /// </returns>
        private string ValToString(object val)
        {
            string dispVal;
            if ((val != null) && m_CEL.IsValid(val))
            {
                if (val is double)
                {
                    double dVal = (double)val;
                    dispVal = dVal.ToString("0.00");
                }
                else
                {
                    dispVal = val.ToString();
                }
            }
            else
            {
                dispVal = N_A;
            }

            return dispVal;
        }

        /// <summary>
        /// Returns GWAccountId for the selected account in lstAccounts.
        /// </summary>
        /// <returns>
        /// 0 if there is no selection, otherwise: GWAccountId
        /// </returns>
        private int GetSelectedGWID()
        {
            string gwID = GetListboxText(lstAccounts);

            if (gwID.Trim().Length == 0)
            {
                return 0;
            }
            else
            {
                gwID = gwID.Substring(0, LIST_ACC_COL_WIDTH);
            }
            return int.Parse(gwID);
        }

        /// <summary>
        /// This function is for workarounding problem of text property(of listbox)
        /// </summary>
        /// <param name="lb">
        /// The listbox, which text must be returned
        /// </param>
        /// <returns>
        /// Text of textbox.
        /// </returns>
        private string GetListboxText(ListBox lb)
        {
            string text;
            try
            {
                text = lb.Text;
            }
            catch
            {
                text = "";
            }
            return text;
        }

        /// <summary>
        /// The context menu item click event handler
        /// </summary>
        /// <param name="sender">The clicked menu item</param>
        /// <param name="e">Event arguments</param>
        private void ctxAccountsDetMenu_ItemClicked(object sender, EventArgs e)
        {
            MenuItem clickedItem = (MenuItem)sender;
            eAccountMarginDetailing level = (eAccountMarginDetailing)Enum.Parse(typeof(eAccountMarginDetailing),
                                                                            clickedItem.Text.Insert(0, "amd"));
            CQGAccount account = m_CEL.Accounts[GetSelectedGWID()];
            if (account != null)
            {
                account.AccountMarginDetailing = level;
                cmbMarginsDetailingLevel.Text = string.Empty;
            }
        }

        /// <summary>
        /// The margins detailing level combobox selection index changed event handler
        /// </summary>
        /// <param name="sender">The margins detailing level combobox</param>
        /// <param name="e">The event arguments</param>
        private void cmbMarginsDetailingLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_CEL == null || cmbMarginsDetailingLevel.Text == string.Empty)
            {
                return;
            }
            eAccountMarginDetailing level = (eAccountMarginDetailing)Enum.Parse(typeof(eAccountMarginDetailing),
                                                                     cmbMarginsDetailingLevel.Text.Insert(0, "amd"));
            foreach (CQGAccount account in m_CEL.Accounts)
            {
                account.AccountMarginDetailing = level;
            }
        }

        /// <summary>
        /// The context menu population event handler
        /// </summary>
        /// <param name="sender">The context menu object</param>
        /// <param name="e">The event arguments</param>
        private void ctxAccountsDetMenu_Popup(object sender, EventArgs e)
        {
            CQGAccount account = m_CEL.Accounts[GetSelectedGWID()];
            if (account != null)
            {
                string level = account.AccountMarginDetailing.ToString().Substring(3);
                foreach (MenuItem item in ctxAccountsDetMenu.MenuItems)
                {
                    item.Checked = (item.Text == level);
                }
            }
        }

        /// <summary>
        /// The mouse down event handler for accounts listbox.
        /// </summary>
        /// <param name="sender">The accounts listbox</param>
        /// <param name="e">The event arguments</param>
        private void lstAccounts_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                lstAccounts.SelectedIndex = lstAccounts.IndexFromPoint(e.X, e.Y);
            }
        }
    }

}
