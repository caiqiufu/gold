using AutoHorseRace.Utils;
using System.ComponentModel;

namespace AutoHorseRace
{
    public partial class SettledHistoryForm : Form
    {
        /// <summary>
        /// 业务日志
        /// </summary>
        public Log _Log;
        /// <summary>
        /// 已结算历史记录列表
        /// </summary>
        private BindingList<SettledHistoryInfo> _SettledHistoryInfoList = new BindingList<SettledHistoryInfo>();

        /// <summary>
        /// 已结算汇总信息列表
        /// </summary>
        private BindingList<SettledSummaryInfo> _SettledSummaryInfoList = new BindingList<SettledSummaryInfo>();

        /// <summary>
        /// 赛马交易或对冲记录明细列表
        /// </summary>
        private BindingList<RaceBettingRecordInfo> _RaceBettingRecordInfoList = new BindingList<RaceBettingRecordInfo>();

        private float _ProcessingAngle = 0;       // 旋转角度
        private System.Windows.Forms.Timer _SettledHistoryTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer _SettledSummaryTimer = new System.Windows.Forms.Timer();



        private void InitProcessingAnimation()
        {
            _SettledHistoryTimer.Interval = 30; // 刷新频率，越小越平滑
            _SettledHistoryTimer.Tick += (s, e) =>
            {
                _ProcessingAngle = (_ProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxSettledHistoryProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxSettledHistoryProcessing.BackColor = Color.Transparent;

            _SettledSummaryTimer.Interval = 30; // 刷新频率，越小越平滑
            _SettledSummaryTimer.Tick += (s, e) =>
            {
                _ProcessingAngle = (_ProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxSettledSummaryProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxSettledSummaryProcessing.BackColor = Color.Transparent;

        }

        public void UpdateSettledHistoryUI(bool isProcessing)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateSettledHistoryUI(isProcessing)));
                return;
            }

            if (isProcessing)
            {
                // 开始扫描
                pictureBoxSettledHistoryProcessing.Visible = true;
                _SettledHistoryTimer.Start();
            }
            else
            {
                // 停止扫描
                _SettledHistoryTimer.Stop();
                pictureBoxSettledHistoryProcessing.Visible = false;
            }
        }
        public void UpdateSettledSummaryUI(bool isProcessing)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateSettledSummaryUI(isProcessing)));
                return;
            }

            if (isProcessing)
            {
                // 开始扫描
                pictureBoxSettledSummaryProcessing.Visible = true;
                _SettledSummaryTimer.Start();
            }
            else
            {
                // 停止扫描
                _SettledSummaryTimer.Stop();
                pictureBoxSettledSummaryProcessing.Visible = false;
            }
        }

        public string _ServerAddress = "";
        public string _Username = "";

        public SettledHistoryForm()
        {
            InitializeComponent();
            this.FormClosing += SettledHistory_FormClosing;
            this.Text = "已结算交易明细";
        }

        private void SettledHistory_Load(object sender, EventArgs e)
        {
            InitialPara();
        }
        public void InitialPara()
        {
            // 1. 必须在指定 DataSource 之前，先关闭自动生成列
            dataGridViewSettledHistoryInfo.AutoGenerateColumns = false;
            dataGridViewSettledSummaryInfo.AutoGenerateColumns = false;
            dataGridViewRaceBettingRecordInfo.AutoGenerateColumns = false;
            // 禁止用户在表格最下方戳出新的空行
            dataGridViewSettledHistoryInfo.AllowUserToAddRows = false;
            dataGridViewSettledHistoryInfo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewSettledSummaryInfo.AllowUserToAddRows = false;
            dataGridViewSettledSummaryInfo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewRaceBettingRecordInfo.AllowUserToAddRows = false;
            dataGridViewRaceBettingRecordInfo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            // 2. 配置连赢表：整行选中、开启整表编辑权限
            dataGridViewSettledHistoryInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewSettledHistoryInfo.ReadOnly = false;
            dataGridViewSettledSummaryInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewSettledSummaryInfo.ReadOnly = false;
            dataGridViewRaceBettingRecordInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRaceBettingRecordInfo.ReadOnly = false;
            // 6. 绑定列的只读状态（分别限制两张表：第一列可动，其余锁死）
            UIUtils.ConfigureColumnsReadOnly(dataGridViewSettledHistoryInfo);
            UIUtils.ConfigureColumnsReadOnly(dataGridViewSettledSummaryInfo);
            UIUtils.ConfigureColumnsReadOnly(dataGridViewRaceBettingRecordInfo);
            // 7. 【修改这里】统一改用 CellClick 事件，实现点击整行任意触碰即勾选
            dataGridViewSettledHistoryInfo.CellClick += UIUtils.DataGridView_CellClick;
            dataGridViewSettledSummaryInfo.CellClick += UIUtils.DataGridView_CellClick;
            dataGridViewRaceBettingRecordInfo.CellClick += UIUtils.DataGridView_CellClick;

            // 8. 进行数据源绑定
            dataGridViewSettledHistoryInfo.DataSource = _SettledHistoryInfoList;
            dataGridViewSettledSummaryInfo.DataSource = _SettledSummaryInfoList;
            dataGridViewRaceBettingRecordInfo.DataSource = _RaceBettingRecordInfoList;

            //初始化加载动画
            InitProcessingAnimation();
        }


        private void SettledHistory_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 用户点击 X
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void LoadDataSettledHistoryInfo(Log _log, IDictionary<string, string> data)
        {
            this._Log = _log;
            _ServerAddress = data["serverAddress"];
            _Username = data["username"];
            UpdateSettledHistoryUI(true);
            // 耗时的网络请求可以继续在后台线程（Task）中跑
            string serverProcessTime = "0ms";
            List<SettledHistoryInfo> SettledHistoryInfos = HTTPHelper.querySettledHistoryInfo(_ServerAddress, _Username, out serverProcessTime);
            _Log.LogInfo($"[性能监控][LoadDataSettledHistoryInfo][querySettledHistoryInfo] 后端服务耗时: {serverProcessTime}");
            // 将所有涉及到 UI 控件和数据源更新的代码放入 Invoke 中切回主线程执行
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDataGridView(SettledHistoryInfos)));
            }
            else
            {
                UpdateDataGridView(SettledHistoryInfos);
            }
            UpdateSettledHistoryUI(false);
        }
        public void LoadDataSettledSummaryInfo(Log _log, string raceDate, string raceType)
        {
            this._Log = _log;
            UpdateSettledSummaryUI(true);
            string serverProcessTime = "0ms";
            // 1. 网络请求（如果此方法本身是在 Task.Run 中调用的，这里会跑在后台线程）
            IDictionary<string, Object> SettledInfo = HTTPHelper.querySettledInfo(_ServerAddress, _Username, raceDate, raceType, out serverProcessTime);
            _Log.LogInfo($"[性能监控][LoadDataSettledSummaryInfo][querySettledInfo] 后端服务耗时: {serverProcessTime}");
            if (SettledInfo != null && SettledInfo.Count > 0)
            {
                // 2. 将所有涉及到 UI 控件和数据源更新的代码放入 Invoke 中切回主线程执行
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateSummaryAndRecordsUI(SettledInfo)));
                }
                else
                {
                    UpdateSummaryAndRecordsUI(SettledInfo);
                }
            }
            UpdateSettledSummaryUI(false);
        }

        /// <summary>
        /// 专门在主线程更新两个 DataGridView 的辅助方法
        /// </summary>
        private void UpdateSummaryAndRecordsUI(IDictionary<string, Object> SettledInfo)
        {
            // 处理汇总表
            if (SettledInfo.ContainsKey("summary_rows"))
            {
                List<SettledSummaryInfo> SettledSummaryInfos = (List<SettledSummaryInfo>)SettledInfo["summary_rows"];
                dataGridViewSettledSummaryInfo.Rows.Clear();
                if (SettledSummaryInfos != null && SettledSummaryInfos.Count > 0)
                {
                    _SettledSummaryInfoList.RaiseListChangedEvents = false;
                    _SettledSummaryInfoList.Clear();
                    foreach (var item in SettledSummaryInfos)
                    {
                        _SettledSummaryInfoList.Add(item);
                    }
                    _SettledSummaryInfoList.RaiseListChangedEvents = true;
                    _SettledSummaryInfoList.ResetBindings();
                }
            }

            // 处理投注记录表
            if (SettledInfo.ContainsKey("forecast_races"))
            {
                IDictionary<string, List<RaceBettingRecordInfo>> RaceBettingRecordInfo = (IDictionary<string, List<RaceBettingRecordInfo>>)SettledInfo["forecast_races"];
                dataGridViewRaceBettingRecordInfo.Rows.Clear();
                if (RaceBettingRecordInfo != null && RaceBettingRecordInfo.Count > 0)
                {
                    _RaceBettingRecordInfoList.RaiseListChangedEvents = false;
                    _RaceBettingRecordInfoList.Clear();
                    foreach (var kvp in RaceBettingRecordInfo)
                    {
                        string raceNo = kvp.Key;
                        List<RaceBettingRecordInfo> RaceBettingRecordInfos = kvp.Value;
                        foreach (var item in RaceBettingRecordInfos)
                        {
                            item.raceNo = raceNo; // 将 raceNo 设置到每个 RaceBettingRecordInfo 对象中
                            _RaceBettingRecordInfoList.Add(item);
                        }
                    }
                    _RaceBettingRecordInfoList.RaiseListChangedEvents = true;
                    _RaceBettingRecordInfoList.ResetBindings();
                }
            }
        }

        private void dataGridViewSettledHistoryInfo_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            UIUtils.EnableNegativeValueRedFormatting(dataGridViewSettledHistoryInfo, new string[] { "WinLoss" });
        }

        private void dataGridViewSettledSummaryInfo_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            UIUtils.EnableNegativeValueRedFormatting(dataGridViewSettledSummaryInfo, new string[] { "BetAmount", "BetRefund", "EatAmount", "EatRefund", "SettledSummaryWinLoss" });
        }

        private void dataGridViewRaceBettingRecordInfo_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            UIUtils.EnableNegativeValueRedFormatting(dataGridViewRaceBettingRecordInfo, new string[] { "payout", "dataGridViewTextBoxWinLoss" });
        }

        private void dataGridViewSettledHistoryInfo_SelectionChanged(object sender, EventArgs e)
        {
            // 确保有行被选中，并且不是处于空白行或正在添加的状态
            if (dataGridViewSettledHistoryInfo.SelectedRows.Count > 0)
            {
                // 获取当前选中的第一行
                DataGridViewRow selectedRow = dataGridViewSettledHistoryInfo.SelectedRows[0];

                // 核心：直接获取该行绑定的强类型对象
                SettledHistoryInfo item = selectedRow.DataBoundItem as SettledHistoryInfo;

                if (item != null && this._Log != null)
                {
                    // 无论这个字段有没有在界面上显示，都可以直接安全地拿到它的值！
                    string raceDate = item.raceDate;
                    string raceType = item.raceType;
                    Task.Run(() =>
                    {
                        LoadDataSettledSummaryInfo(this._Log, raceDate, raceType);
                    });
                }
            }
        }

        // 抽取一个专门用来更新界面的辅助方法
        private void UpdateDataGridView(List<SettledHistoryInfo> SettledHistoryInfos)
        {
            dataGridViewSettledHistoryInfo.Rows.Clear();
            if (SettledHistoryInfos != null && SettledHistoryInfos.Count > 0)
            {
                _SettledHistoryInfoList.RaiseListChangedEvents = false;
                _SettledHistoryInfoList.Clear();
                foreach (var item in SettledHistoryInfos)
                {
                    _SettledHistoryInfoList.Add(item);
                }
                _SettledHistoryInfoList.RaiseListChangedEvents = true;
                _SettledHistoryInfoList.ResetBindings();
            }
        }

        private void pictureBoxSettledHistoryProcessing_Paint(object sender, PaintEventArgs e)
        {
            // 如果没有在扫描中，就不画任何东西
            if (!_SettledHistoryTimer.Enabled) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 定义圆环的区域
            Rectangle rect = new Rectangle(2, 2, pictureBoxSettledHistoryProcessing.Width - 6, pictureBoxSettledHistoryProcessing.Height - 6);

            // 使用深蓝色或你喜欢的颜色画圆弧
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 3))
            {
                // 绘制一段120度的弧线
                e.Graphics.DrawArc(pen, rect, _ProcessingAngle, 120);
            }
        }

        private void pictureBoxSettledSummaryProcessing_Paint(object sender, PaintEventArgs e)
        {
            // 如果没有在扫描中，就不画任何东西
            if (!_SettledSummaryTimer.Enabled) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 定义圆环的区域
            Rectangle rect = new Rectangle(2, 2, pictureBoxSettledSummaryProcessing.Width - 6, pictureBoxSettledSummaryProcessing.Height - 6);

            // 使用深蓝色或你喜欢的颜色画圆弧
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 3))
            {
                // 绘制一段120度的弧线
                e.Graphics.DrawArc(pen, rect, _ProcessingAngle, 120);
            }
        }
    }
}
