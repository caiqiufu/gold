using AutoHorseRace.Utils;
using System.ComponentModel;

namespace AutoHorseRace
{
    /// <summary>
    /// 按 (场次, 类型, combo) 汇总后的赌/吃统计结果，供汇总表格绑定展示。
    /// </summary>
    public class ComboSummary
    {
        public string RaceNo { get; set; }
        public string Type { get; set; }
        public string Combo { get; set; }

        /// <summary>赌（BET）累计票数总额</summary>
        public int BetTotal { get; set; }
        /// <summary>吃（EAT）累计票数总额</summary>
        public int EatTotal { get; set; }
        /// <summary>赌提交笔数</summary>
        public int BetCount { get; set; }
        /// <summary>吃提交笔数</summary>
        public int EatCount { get; set; }
        /// <summary>差额 = 赌总额 - 吃总额，用于快速判断是否对冲平衡（理想情况应为0）</summary>
        public int Diff => BetTotal - EatTotal;

        public string DictKey => $"{RaceNo}_{Type}_{Combo}";
    }

    public partial class AutoBettingStatusForm : Form
    {

        public string _ServerAddress = string.Empty;
        public string _Username = string.Empty;
        public string _RaceType = string.Empty;
        public string _RaceDate = string.Empty;
        public string _RaceNo = string.Empty;

        /// 自动下注数据列表（明细，可能因用户点击汇总行而被过滤到单个 combo）
        /// </summary>
        private BindingList<BetInfo> _BettingDetailsList = new BindingList<BetInfo>();

        /// <summary>
        /// 原始全量明细缓存（未过滤），用于汇总计算和"点击汇总行 -> 过滤明细"联动。
        /// 每次 RefreshData() 从数据库拉取新数据后同步刷新。
        /// </summary>
        private List<BetInfo> _AllBettingDetails = new List<BetInfo>();

        /// <summary>
        /// 按 combo 汇总后的赌/吃统计列表，绑定到新增的汇总表格。
        /// </summary>
        private BindingList<ComboSummary> _ComboSummaryList = new BindingList<ComboSummary>();

        /// <summary>
        /// 运行时创建的汇总表格，放置在原有明细表格上方。
        /// </summary>
        private DataGridView dataGridViewComboSummary;

        /// <summary>
        /// 汇总表格与明细表格之间的高度分配（汇总表固定高度，明细表随窗体自适应）。
        /// </summary>
        private const int ComboSummaryGridHeight = 280;

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

            // 保存全量原始数据，供汇总计算和联动过滤使用
            _AllBettingDetails = bettingDetails ?? new List<BetInfo>();

            // 1. 刷新汇总表
            RefreshComboSummary();

            // 2. 明细表默认展示全部（未选中任何汇总行时）
            ApplyDetailList(_AllBettingDetails);
        }

        /// <summary>
        /// 按 (RaceNo, Type, Combo) 重新计算赌/吃汇总，并刷新绑定到汇总表格。
        /// </summary>
        private void RefreshComboSummary()
        {
            var summary = BuildComboSummary(_AllBettingDetails);

            _ComboSummaryList.RaiseListChangedEvents = false;
            _ComboSummaryList.Clear();
            foreach (var item in summary)
            {
                _ComboSummaryList.Add(item);
            }
            _ComboSummaryList.RaiseListChangedEvents = true;
            _ComboSummaryList.ResetBindings();
        }

        /// <summary>
        /// 汇总逻辑：
        /// 1. 先按"真实提交动作"去重，排除 RefreshTradeListToDB 周期性写入的
        ///    "Auto Refresh" 状态回写行（这类行只是对同一笔已存在交易的状态快照更新，
        ///    不是新增的一笔下注，如果不过滤会导致同一 combo 的金额被重复累加多次）。
        /// 2. 对去重后的记录按 (场次, 类型, combo) 分组，分别累加 BET/EAT 的票数总额与笔数。
        /// </summary>
        public static List<ComboSummary> BuildComboSummary(List<BetInfo> allRows)
        {
            if (allRows == null || allRows.Count == 0)
            {
                return new List<ComboSummary>();
            }

            // 只保留"要求下注成功"/"要求吃注成功"这类真实提交产生的行，
            // 过滤掉备注为 "Auto Refresh" 的周期性状态回写行。
            var realSubmissions = allRows
                .Where(r => !string.IsNullOrEmpty(r.remark)
                         && (r.remark.Contains("要求下注成功") || r.remark.Contains("要求吃注成功")))
                .ToList();

            // 进一步按唯一标识去重（优先用 seq，若无则退化为业务字段组合），
            // 防止同一笔真实提交因为多次查询/多次落库而在列表里出现多条一样的记录。
            var deduped = realSubmissions
                .GroupBy(r => !string.IsNullOrEmpty(r.seq)
                    ? r.seq
                    : $"{r.raceNo}_{r.type}_{r.combo}_{r.action}_{r.odds}_{r.stakeAmount}")
                .Select(g => g.First())
                .ToList();

            var summary = deduped
                .GroupBy(r => new { r.raceNo, r.type, r.combo })
                .Select(g => new ComboSummary
                {
                    RaceNo = g.Key.raceNo,
                    Type = g.Key.type,
                    Combo = g.Key.combo,
                    BetTotal = g.Where(x => string.Equals(x.action, "BET", StringComparison.OrdinalIgnoreCase)).Sum(x => x.stakeAmount),
                    EatTotal = g.Where(x => string.Equals(x.action, "EAT", StringComparison.OrdinalIgnoreCase)).Sum(x => x.stakeAmount),
                    BetCount = g.Count(x => string.Equals(x.action, "BET", StringComparison.OrdinalIgnoreCase)),
                    EatCount = g.Count(x => string.Equals(x.action, "EAT", StringComparison.OrdinalIgnoreCase)),
                })
                .OrderBy(s => s.RaceNo).ThenBy(s => s.Type).ThenBy(s => s.Combo)
                .ToList();

            return summary;
        }

        /// <summary>
        /// 把指定的明细集合绑定到原有明细表格。
        /// </summary>
        private void ApplyDetailList(IEnumerable<BetInfo> rows)
        {
            _BettingDetailsList.RaiseListChangedEvents = false;
            _BettingDetailsList.Clear();
            foreach (var item in rows)
            {
                _BettingDetailsList.Add(item);
            }
            _BettingDetailsList.RaiseListChangedEvents = true;
            _BettingDetailsList.ResetBindings();
        }

        /// <summary>
        /// 汇总表选中行变化时，联动过滤下方明细表，只展示该 combo 的所有原始记录（含 Auto Refresh 行，
        /// 方便核对某个 combo 完整的状态变化时间线）。
        /// </summary>
        private void DataGridViewComboSummary_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewComboSummary.CurrentRow?.DataBoundItem is ComboSummary selected)
            {
                var filtered = _AllBettingDetails
                    .Where(r => string.Equals(r.raceNo, selected.RaceNo)
                             && string.Equals(r.type, selected.Type)
                             && string.Equals(r.combo, selected.Combo))
                    .ToList();
                ApplyDetailList(filtered);
            }
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

            // 8. 新增：运行时创建并插入 combo 汇总表格（放在原明细表格上方，随窗体一起自适应）
            InitComboSummaryGrid();
        }

        /// <summary>
        /// 运行时构建按 combo 汇总的表格，插入到原有明细表格 (dataGridViewAutoBettingStatus) 的正上方，
        /// 并把明细表格上移/收缩以腾出空间。两者共用同一个父容器，因此不依赖具体的设计器布局细节。
        /// </summary>
        private void InitComboSummaryGrid()
        {
            var parent = dataGridViewAutoBettingStatus.Parent;
            int detailTop = dataGridViewAutoBettingStatus.Top;
            int detailLeft = dataGridViewAutoBettingStatus.Left;
            int detailWidth = dataGridViewAutoBettingStatus.Width;
            var detailAnchor = dataGridViewAutoBettingStatus.Anchor;

            dataGridViewComboSummary = new DataGridView
            {
                Name = "dataGridViewComboSummary",
                Left = detailLeft,
                Top = detailTop,
                Width = detailWidth,
                Height = ComboSummaryGridHeight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
            };
            dataGridViewComboSummary.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.RaceNo),
                HeaderText = "场次",
                Width = 60,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.Type),
                HeaderText = "类型",
                Width = 60,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.Combo),
                HeaderText = "组合",
                Width = 100,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.BetTotal),
                HeaderText = "赌总额",
                Width = 80,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.EatTotal),
                HeaderText = "吃总额",
                Width = 80,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.Diff),
                HeaderText = "差额",
                Width = 80,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.BetCount),
                HeaderText = "赌笔数",
                Width = 60,
            });
            dataGridViewComboSummary.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ComboSummary.EatCount),
                HeaderText = "吃笔数",
                Width = 60,
            });

            dataGridViewComboSummary.DataSource = _ComboSummaryList;
            dataGridViewComboSummary.SelectionChanged += DataGridViewComboSummary_SelectionChanged;

            // 差额不为 0 时高亮标红，一眼看出该 combo 赌/吃没有对冲平衡
            // （比如强平预占未正确释放导致的重复吃注/超发，在这里会直接体现为差额异常）
            dataGridViewComboSummary.CellFormatting += (s, e) =>
            {
                if (dataGridViewComboSummary.Columns[e.ColumnIndex].DataPropertyName == nameof(ComboSummary.Diff)
                    && e.Value != null && int.TryParse(e.Value.ToString(), out int diff) && diff != 0)
                {
                    e.CellStyle.BackColor = Color.LightYellow;
                    e.CellStyle.ForeColor = Color.DarkRed;
                }
            };

            parent.Controls.Add(dataGridViewComboSummary);
            dataGridViewComboSummary.BringToFront();

            // 把原有明细表格下移、并收缩高度，腾出汇总表格所占的空间；
            // 保留原有 Anchor 中 Bottom/Right 部分，使其仍能随窗体拉伸自适应，
            // 只是整体向下平移了 ComboSummaryGridHeight 的距离。
            const int gapBetweenGrids = 6;
            int shift = ComboSummaryGridHeight + gapBetweenGrids;
            dataGridViewAutoBettingStatus.Top = detailTop + shift;
            if ((detailAnchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                dataGridViewAutoBettingStatus.Height -= shift;
            }
            dataGridViewAutoBettingStatus.Anchor = detailAnchor; // 保持原锚点设置不变
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
            RefreshData();
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