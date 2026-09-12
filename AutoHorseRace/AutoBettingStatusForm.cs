using AutoHorseRace.Utils;
using System.ComponentModel;

namespace AutoHorseRace
{
    public partial class AutoBettingStatusForm : Form
    {

        public string _ServerAddress = string.Empty;
        public string _Username = string.Empty;
        public string _RaceType = string.Empty;
        public string _RaceDate = string.Empty;
        public string _RaceNo = string.Empty;

        /// 自动下注数据列表
        /// </summary>
        private BindingList<BetInfo> _BettingDetailsList = new BindingList<BetInfo>();
        private float _ProcessingAngle = 0;       // 旋转角度
        private System.Windows.Forms.Timer _AutoBettingStatusTimer = new System.Windows.Forms.Timer();
        public AutoBettingStatusForm()
        {
            InitializeComponent();
            this.FormClosing += AutoBettingStatusForm_FormClosing;
            this.Text = "下注明细";
        }

        private void InitProcessingAnimation()
        {
            _AutoBettingStatusTimer.Interval = 30; // 刷新频率，越小越平滑
            _AutoBettingStatusTimer.Tick += (s, e) =>
            {
                _ProcessingAngle = (_ProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxAutoBettingStatus.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxAutoBettingStatus.BackColor = Color.Transparent;
        }
        public void UpdateAutoBettingStatusUI(bool isProcessing)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateAutoBettingStatusUI(isProcessing)));
                return;
            }

            if (isProcessing)
            {
                // 开始扫描
                pictureBoxAutoBettingStatus.Visible = true;
                _AutoBettingStatusTimer.Start();
            }
            else
            {
                // 停止扫描
                _AutoBettingStatusTimer.Stop();
                pictureBoxAutoBettingStatus.Visible = false;
            }
        }
        private void pictureBoxAutoBettingStatus_Paint(object sender, PaintEventArgs e)
        {
            // 如果没有在扫描中，就不画任何东西
            if (!_AutoBettingStatusTimer.Enabled) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 定义圆环的区域
            Rectangle rect = new Rectangle(2, 2, pictureBoxAutoBettingStatus.Width - 6, pictureBoxAutoBettingStatus.Height - 6);

            // 使用深蓝色或你喜欢的颜色画圆弧
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 3))
            {
                // 绘制一段120度的弧线
                e.Graphics.DrawArc(pen, rect, _ProcessingAngle, 120);
            }
        }

        private void AutoBettingStatusForm_FormClosing(object sender, FormClosingEventArgs e)
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
            _ServerAddress = marketData["serverAddress"];
            _Username = marketData["username"];
            _RaceType = marketData["raceType"];
            _RaceDate = marketData["raceDate"];
            _RaceNo = marketData["raceNo"];
            comboBoxRaceNo.SelectedItem = _RaceNo;
        }

        public void RefreshData()
        {
            List<BetInfo> bettingDetails = DBHelper.queryAutoBettingStatusList(_Username, _RaceType, _RaceDate, _RaceNo, textBoxCombo.Text);
            _BettingDetailsList.RaiseListChangedEvents = false;
            _BettingDetailsList.Clear();
            if (bettingDetails != null && bettingDetails.Count > 0)
            {
                foreach (var item in bettingDetails)
                {
                    _BettingDetailsList.Add(item);
                }
            }
            _BettingDetailsList.RaiseListChangedEvents = true;
            _BettingDetailsList.ResetBindings();
        }

        private void AutoBettingStatusForm_Load(object sender, EventArgs e)
        {
            InitialPara();
        }

        public void InitialPara()
        {
            // 1. 必须在指定 DataSource 之前，先关闭自动生成列
            dataGridViewAutoBettingStatus.AutoGenerateColumns = false;

            // 禁止用户在表格最下方戳出新的空行
            dataGridViewAutoBettingStatus.AllowUserToAddRows = false;
            dataGridViewAutoBettingStatus.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 2. 配置连赢表：整行选中、开启整表编辑权限
            dataGridViewAutoBettingStatus.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAutoBettingStatus.ReadOnly = false;

            // 5. 进行数据源绑定
            dataGridViewAutoBettingStatus.DataSource = _BettingDetailsList;
            // 6. 绑定列的只读状态（分别限制两张表：第一列可动，其余锁死）
            UIUtils.ConfigureColumnsReadOnly(dataGridViewAutoBettingStatus);
            // 7. 【修改这里】统一改用 CellClick 事件，实现点击整行任意触碰即勾选
            //dataGridViewBettingDetail.CellClick += UIUtils.DataGridView_CellClick;

            //初始化加载动画
            InitProcessingAnimation();
        }

        private void buttonRefreshAutoBettingStatus_Click(object sender, EventArgs e)
        {
            UpdateAutoBettingStatusUI(true);
            RefreshData();
            UpdateAutoBettingStatusUI(false);

        }

        private void buttonCleanQuery_Click(object sender, EventArgs e)
        {
            textBoxCombo.Text = "";
        }

        private void comboBoxRaceNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            _RaceNo = comboBoxRaceNo.Text;
        }

        private void buttonOpenQSelector_Click(object sender, EventArgs e)
        {
            // 传入当前文本框里已有的内容，以及当前赛事的最大马匹数（比如 14）
            using (var selectorForm = new ComboSelectorForm(textBoxCombo.Text, "QP", maxHorseNo: 14))
            {
                if (selectorForm.ShowDialog(this) == DialogResult.OK)
                {
                    // 用户点击了确定，将子窗体选择好的规范字符串赋回给主界面的输入框
                    textBoxCombo.Text = selectorForm.SelectedCombosResult;
                }
            }
        }
    }
}