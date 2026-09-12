using AutoHorseRace.Utils;
using System.ComponentModel;

namespace AutoHorseRace
{
    public partial class TradeDetailForm : Form
    {


        /// 自动下注数据列表
        /// </summary>
        private BindingList<BetInfo> _BettingDetailsList = new BindingList<BetInfo>();

        public TradeDetailForm()
        {
            InitializeComponent();
            this.FormClosing += TradeDetailForm_FormClosing;
            this.Text = "下注明细";
        }

        private void TradeDetailForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 用户点击 X
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void LoadData(IDictionary<string, string> marketData)
        {
            string tradeRecordId = marketData["seq"];
            List<BetInfo> bettingDetails = DBHelper.queryBettingDetailList(tradeRecordId);
            dataGridViewBettingDetail.Rows.Clear();
            if (bettingDetails != null && bettingDetails.Count > 0)
            {
                _BettingDetailsList.RaiseListChangedEvents = false;
                _BettingDetailsList.Clear();
                foreach (var item in bettingDetails)
                {
                    _BettingDetailsList.Add(item);
                }
                _BettingDetailsList.RaiseListChangedEvents = true;
                _BettingDetailsList.ResetBindings();
            }
        }

        private void TradeDetailForm_Load(object sender, EventArgs e)
        {

            // 1. 必须在指定 DataSource 之前，先关闭自动生成列
            dataGridViewBettingDetail.AutoGenerateColumns = false;

            // 禁止用户在表格最下方戳出新的空行
            dataGridViewBettingDetail.AllowUserToAddRows = false;
            dataGridViewBettingDetail.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 2. 配置连赢表：整行选中、开启整表编辑权限
            dataGridViewBettingDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBettingDetail.ReadOnly = false;

            // 5. 进行数据源绑定
            dataGridViewBettingDetail.DataSource = _BettingDetailsList;
            // 6. 绑定列的只读状态（分别限制两张表：第一列可动，其余锁死）
            UIUtils.ConfigureColumnsReadOnly(dataGridViewBettingDetail);
            // 7. 【修改这里】统一改用 CellClick 事件，实现点击整行任意触碰即勾选
            //dataGridViewBettingDetail.CellClick += UIUtils.DataGridView_CellClick;
        }
    }
}
