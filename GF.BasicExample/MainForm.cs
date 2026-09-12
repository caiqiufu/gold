using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GF.Api;
using GF.Api.Connection;
using GF.Api.Contracts;
using GF.Api.Orders;
using GF.Api.Positions;
using GF.Api.Utils;
using GF.Api.Values.Orders;
using GF.BasicExample.Converters;
using GF.BasicExample.Managers;
using GF.BasicExample.Processors;
using GF.BasicExample.Runner;
using NetMQ;
using NetMQ.Sockets;
using NLog;
using DS;
using Newtonsoft.Json;

namespace GF.BasicExample
{
    public partial class MainForm : Form
    {
        private PriceProcessor _priceProcessor;
        private OrdersProcessor _ordersProcessor;
        private PositionsProcessor _positionsProcessor;
        private DescriptionManager _listBoxDescriptionManager;

        private readonly IClientRunner _runner;
        private readonly ProcessorsManager _processorsManager;

        private double _totalVol = 0;
        private string _configFile = System.Windows.Forms.Application.StartupPath + "\\MyDSConfig.json";
        Config _config;
        string _DSType = "OEC";
        string _BindAddress = "";
        string _AccountName = "";
        string _Password = "";
        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();


        private PublisherSocket publisher;
        public IGFClient Client => _runner.Client;
        public IServerConnectionApi Connection => Client.Connection.Aggregate;

        public MainForm(IClientRunner runner)
        {
            _runner = runner ?? throw new ArgumentNullException();
            _processorsManager = new ProcessorsManager(UpdateStatus);

            InitializeComponent();
            InitializeProcessors();
            SubscribeEvents();
            InitializeControls();
        }
        private void LoadConfig()
        {
            if (System.IO.File.Exists(_configFile))
            {
                _config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText(_configFile));
                if (_config != null)
                {
                    //_DSType = _config.DSType;
                    if ("OEC".Equals(_DSType))
                    {
                        _BindAddress = _config.BindAddressOEC;
                    }
                    if ("T4".Equals(_DSType))
                    {
                        _BindAddress = _config.BindAddressT4;
                    }
                    _AccountName = _config.AccountName;
                    _Password = _config.Password;

                }
                InitialPara();
            }
        }
        private void InitialPara()
        {
            txtPublishIP.Text = _BindAddress;
            tbLogin.Text = _AccountName;
            tbPassword.Text = _Password;

        }
        private void SubscribeEvents()
        {
            Client.Logging.ErrorOccurred += OnError;
            Connection.LoginFailed += OnLoginFailed;
            Connection.Disconnected += OnDisconnected;
            Connection.LoginCompleted += OnLoginComplete;

            if (lbOrders.DataSource is IBindingList ordersDataSource)
                ordersDataSource.ListChanged += lbOrders_DataSource_ListChanged;

            if (cbContract.DataSource is IBindingList contractDataSource)
                contractDataSource.ListChanged += cbContract_DataSource_ListChanged;
            Client.Subscriptions.Price.PriceTick += Price_PriceTick;
        }



        private void InitializeProcessors()
        {
            _processorsManager.RegisterProcessor(new ContractsProcessor(Client, "ES"), cbContract);
            _priceProcessor = _processorsManager.RegisterProcessor(new PriceProcessor(Client), lbPrice);
            _ordersProcessor = _processorsManager.RegisterProcessor(new OrdersProcessor(Client), lbOrders);
            _positionsProcessor = _processorsManager.RegisterProcessor(new PositionsProcessor(Client), lbPositions);
        }

        /// <summary>
        ///     Usually called when login or password is wrong
        /// </summary>
        private void OnLoginFailed(IGFClient client, LoginFailedEventArgs e)
        {
            OnDisconnected(e.FailReason.ToString());
        }

        private void OnError(IGFClient client, ErrorEventArgs e)
        {
            UpdateStatus(null);
            MessageBox.Show(e.Exception?.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnDisconnected(IGFClient client, DisconnectedEventArgs e)
        {
            OnDisconnected(e.Message ?? e.Exception?.ToString());
        }

        private void OnLoginComplete(IGFClient client, LoginCompleteEventArgs e)
        {
            UpdateStatus("Logged in");
        }

        private void OnDisconnected(string errorMessage)
        {
            _runner.Stop();
            UpdateStatus("Disconnected");

            if (!string.IsNullOrEmpty(errorMessage))
                MessageBox.Show(errorMessage);
        }

        private void InitializeControls()
        {
            _listBoxDescriptionManager = new DescriptionManager(toolTip);
            _listBoxDescriptionManager.Register(lbOrders, _ordersProcessor);
            _listBoxDescriptionManager.Register(lbPositions, _positionsProcessor);

            Text = $"{Text} - {_runner.GetType().Name} runner";
            cbSide.BindTo(new[] { OrderSide.Buy, OrderSide.Sell });
            cbType.BindTo(Enum.GetValues(typeof(OrderType)).Cast<OrderType>().ToArray());
        }

        private void UpdateStatus(string text)
        {
            if (InvokeRequired)
                Invoke(new Action(() => UpdateStatus(text)));
            else
            {
                if (text != null)
                    lbStatus.Text = text;

                btnSubmit.Enabled = Connection.IsConnected;
                btnConnect.Enabled = !Connection.IsConnected && (!Connection.IsConnecting || Connection.IsClosed);
                btnDisconnect.Enabled = (Connection.IsConnected || Connection.IsConnecting) && !Connection.IsClosed;

                tbLogin.Enabled = btnConnect.Enabled;
                tbPassword.Enabled = btnConnect.Enabled;
                pOrderTicket.Enabled = btnDisconnect.Enabled;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateStatus("Ready");
            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _runner.Dispose();
            _processorsManager.Dispose();
            _listBoxDescriptionManager.Dispose();
            base.OnClosing(e);
        }

        /// <summary>
        ///     Connects to GF Server
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string uuid = "e2d70574-5200-45f3-a981-8e856bfdf8a5";
            string url = "prod.gainfutures.com";
            if (checkBox1.Checked)
            {
                uuid = "09ccfb40-1436-46ea-9b52-603a3d967026";
                url = "api.gainfutures.com";
                tbLogin.Text = "CQiufu";
                tbPassword.Text = "API69382";
            }
            else
            {
                uuid = "e2d70574-5200-45f3-a981-8e856bfdf8a5";
                url = "prod.gainfutures.com";
            }
            try
            {

                var context = new ConnectionContextBuilder()
                    .WithUserName(tbLogin.Text)
                    .WithPassword(tbPassword.Text)
                    .WithUUID(uuid) //test  9e61a8bc-0a31-4542-ad85-33ebab0e4e86   09ccfb40-1436-46ea-9b52-603a3d967026  live e2d70574-5200-45f3-a981-8e856bfdf8a5
                    .WithPort(9210)//Port 9210 Order Server Port 9211 Price Server
                    .WithHost(url)//prod.gainfutures.com  api.gainfutures.com
                    .WithForceLogin(true)
                    .Build();

                _runner.Start();
                Connection.Connect(context);
                UpdateStatus($"Connecting to {context.Host}...");
            }
            catch (Exception ex)
            {
                UpdateStatus("Connect failed: " + ex.Message);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Connection.Disconnect();
        }

        private void cbType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            nPrice.Enabled = cbType.SelectedItem as OrderType? != OrderType.Market;
        }

        /// <summary>
        ///     When user selects a contract, it displays current price or subscribe for it.
        /// </summary>
        private void cbContract_SelectionChangeCommitted(object sender, EventArgs e)
        {
            _priceProcessor.Subscribe(cbContract.SelectedItem as IContract);
            toolTip.SetToolTip(cbContract, cbContract.Text);
        }

        private void cbContract_DataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            // ensure contract subscribed after initial data binding
            if (_priceProcessor.SubscribedContract == null && cbContract.SelectedItem != null)
                _priceProcessor.Subscribe(cbContract.SelectedItem as IContract);

            if (Connection.IsConnected)
                UpdateStatus($"Updating contracts - {cbContract.Items.Count} loaded...");

            UpdateDropDownWidth(cbContract);
        }

        private void lbOrders_DataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            lbOrders.TopIndex = lbOrders.Items.Count - 1;

            if (Connection.IsConnected)
                UpdateStatus($"Total orders: {lbOrders.Items.Count}, working: {lbOrders.Items.Cast<IOrder>().Count(o => !o.IsFinalState)}");
        }

        /// <summary>
        ///     Gathers data from contract to order draft and sends the order.
        /// </summary>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // Price2 omitted for simplicity - it is needed only for STPLMT orders
                _ordersProcessor.SendOrder(
                    cbSide.SelectedItem as OrderSide? ?? OrderSide.None,
                    (int)nQty.Value,
                    _priceProcessor.SubscribedContract,
                    cbType.SelectedItem as OrderType? ?? OrderType.Market,
                    (double)nPrice.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending order: {ex.Message}");
            }
        }

        private void lbOrders_DoubleClick(object sender, EventArgs e)
        {
            if (lbOrders.SelectedItem is IOrder order && !order.IsFinalState)
            {
                if (MessageBox.Show($"Are you sure you'd like to cancel order {order}", $"Cancel order #{order.ID}", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _ordersProcessor.CancelOrder(order);
            }
        }

        private void lbPositions_DoubleClick(object sender, EventArgs e)
        {
            if (lbPositions.SelectedItem is IPosition position && _positionsProcessor.CanExitPosition(position))
            {
                if (MessageBox.Show($"Are you sure you'd like to exit position:{Environment.NewLine}{TypeConverterBase.ToString(position)}", "Exit position", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _positionsProcessor.ExitPosition(position, _ordersProcessor);
            }
        }

        private static void UpdateDropDownWidth(ComboBox control)
        {
            if (control.Items.Count == 0)
                return;

            var font = control.Font;
            var g = control.CreateGraphics();
            var isDroppedDown = control.DroppedDown;

            var vertScrollBarWidth = control.Items.Count > control.MaxDropDownItems
                ? SystemInformation.VerticalScrollBarWidth
                : 0;

            var width = (int)control.Items
                .Cast<object>()
                .Select(i => g.MeasureString(TypeConverterBase.ToString(i), font).Width + vertScrollBarWidth)
                .Max();

            control.DropDownWidth = Math.Max(control.Width, width);
            control.DroppedDown = isDroppedDown;
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            if (btnPublish.Text.ToLower() == "bind")
            {
                if (txtPublishIP.Text != null)
                {
                    try
                    {

                        //_logger.Debug("bind to :" + txtPublishIP.Text);
                        publisher = new PublisherSocket();
                        publisher.Bind(txtPublishIP.Text);

                        btnPublish.Text = "Unbind";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Bind Error:" + ex.Message);
                        _logger.Error(ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    if (publisher != null)
                    {
                        //_logger.Debug("Unbind to :" + txtPublishIP.Text);
                        publisher.Unbind(txtPublishIP.Text);
                        btnPublish.Text = "Bind";

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("UnBind Error:" + ex.Message);
                    _logger.Error(ex.Message);
                }

            }

        }


        private void Price_PriceTick(IGFClient client, PriceChangedEventArgs e)
        {

            if (publisher != null && publisher.IsDisposed == false)
            {
                //if (e.Price.TotalVol != _totalVol)
                {
                    _totalVol = e.Price.TotalVol;
                    string msg = string.Format("{0} {1} O:{2} H:{3} L:{4} Last:{5}, Bid:{6} Ask:{7} TotalVol:{8}",
                                  e.Contract.Symbol,
                                  e.Price.LastDateTime,
                                  e.Contract.PriceToString(e.Price.OpenPrice),
                                  e.Contract.PriceToString(e.Price.HighPrice),
                                  e.Contract.PriceToString(e.Price.LowPrice),
                                  e.Contract.PriceToString(e.Price.LastPrice),
                                  e.Contract.PriceToString(e.Price.BidPrice),
                                  e.Contract.PriceToString(e.Price.AskPrice),
                                  e.Price.TotalVol);
                    //Console.WriteLine(msg);
                    //_logger.Info(msg);

                    string symbol = "Gold";

                    publisher.SendFrame($"{symbol} {e.Price.BidPrice} {e.Price.AskPrice} {e.Price.TotalVol}");
                }

            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }
        private void SaveConfig()
        {
            if ("OEC".Equals(_DSType))
            {
                _config.BindAddressOEC = _BindAddress;
            }
            if ("T4".Equals(_DSType))
            {
                _config.BindAddressT4 = _BindAddress;
            }
            _config.DSType = _DSType;
            _config.AccountName = _AccountName;
            _config.Password = _Password;
            System.IO.File.WriteAllText(_configFile, JsonConvert.SerializeObject(_config));
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //SaveConfig();
        }

        private void txtPublishIP_TextChanged(object sender, EventArgs e)
        {
            _BindAddress = txtPublishIP.Text;
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            SaveConfig();
            MessageBox.Show("±£´æ³É¹¦");
        }

        private void tbLogin_TextChanged(object sender, EventArgs e)
        {
            _AccountName = tbLogin.Text;
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            _Password = tbPassword.Text;
        }
    }
}