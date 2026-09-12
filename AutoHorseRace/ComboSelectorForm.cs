namespace AutoHorseRace
{
    public partial class ComboSelectorForm : Form
    {
        // 对外公开的属性：保存最终选中的组合字符串
        public string SelectedCombosResult { get; private set; }

        private TextBox txtCurrentCombos;
        private TableLayoutPanel tableMatrix;
        private int _maxHorseNo; // 最大马号（比如支持到 14 或 10）
        private string _RaceType;

        public ComboSelectorForm(string initialCombos, string type, int maxHorseNo = 14)
        {
            _maxHorseNo = maxHorseNo;
            SelectedCombosResult = initialCombos ?? string.Empty;
            _RaceType = type;

            InitializeComponentCustom();
            LoadInitialCombos(initialCombos);
        }

        // 手动构建简易 UI（如果不想用拖拽设计器，可以直接用这段代码初始化控件）
        private void InitializeComponentCustom()
        {
            this.Text = $"[{_RaceType}]快捷马号组合选择器";
            this.Width = 820;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // 1. 顶部面板（输入框 + 确定按钮）
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            txtCurrentCombos = new TextBox { Dock = DockStyle.Left, Width = 450, Font = new Font("Segoe UI", 10) };
            txtCurrentCombos.TextChanged += TxtCurrentCombos_TextChanged;

            Button btnConfirm = new Button { Text = "确定", Dock = DockStyle.Right, Width = 100 };
            btnConfirm.Click += (s, e) =>
            {
                SelectedCombosResult = txtCurrentCombos.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            pnlTop.Controls.Add(txtCurrentCombos);
            pnlTop.Controls.Add(btnConfirm);

            // 2. 底部快捷操作栏
            FlowLayoutPanel pnlBottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5) };
            Button btnSelectAll = new Button { Text = "全选" };
            Button btnClear = new Button { Text = "清空" };
            Button btnInvert = new Button { Text = "反选" };

            btnSelectAll.Click += (s, e) => ToggleAll(true);
            btnClear.Click += (s, e) => ToggleAll(false);
            btnInvert.Click += (s, e) => InvertSelection();

            pnlBottom.Controls.AddRange(new Control[] { btnSelectAll, btnClear, btnInvert });

            // 3. 中部矩阵面板（带滚动条）
            Panel pnlCenter = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };
            tableMatrix = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = _maxHorseNo,
                RowCount = _maxHorseNo
            };

            BuildMatrixGrid();
            pnlCenter.Controls.Add(tableMatrix);

            // 添加到窗体
            this.Controls.Add(pnlCenter);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
        }

        // 动态生成交叉矩阵按钮 (CheckBox 外观设为 Toggle Button)
        private void BuildMatrixGrid()
        {
            tableMatrix.SuspendLayout();
            for (int i = 1; i <= _maxHorseNo; i++)
            {
                for (int j = 1; j <= _maxHorseNo; j++)
                {
                    // Q / QP 组合通常是两匹不同的马，且为了规范（如 1-2 而非 2-1），只保留 i < j 的半个矩阵
                    if (i >= j)
                    {
                        // 留空占位保持表格整齐
                        tableMatrix.Controls.Add(new Label { Width = 35, Height = 30 }, j - 1, i - 1);
                        continue;
                    }

                    string comboStr = $"{i}-{j}";
                    CheckBox chk = new CheckBox
                    {
                        Text = comboStr,
                        Appearance = Appearance.Button,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Width = 55,
                        Height = 30,
                        Tag = comboStr,
                        Margin = new Padding(1)
                    };

                    // 绑定勾选事件
                    chk.CheckedChanged += Chk_CheckedChanged;
                    tableMatrix.Controls.Add(chk, j - 1, i - 1);
                }
            }
            tableMatrix.ResumeLayout();
        }

        // 根据初始字符串回显勾选状态
        private void LoadInitialCombos(string initialCombos)
        {
            if (string.IsNullOrWhiteSpace(initialCombos)) return;

            txtCurrentCombos.Text = initialCombos;
            var comboSet = new HashSet<string>(initialCombos.Replace('，', ',').Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            foreach (Control ctrl in tableMatrix.Controls)
            {
                if (ctrl is CheckBox chk && chk.Tag is string comboStr)
                {
                    // 如果在集合中，静默勾选（临时移除事件避免死循环或重复触发）
                    chk.CheckedChanged -= Chk_CheckedChanged;
                    chk.Checked = comboSet.Contains(comboStr);
                    chk.CheckedChanged += Chk_CheckedChanged;
                }
            }
        }

        // 单个格子勾选改变时，重新收集并刷新文本框
        private void Chk_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTextBoxFromMatrix();
        }

        // 遍历矩阵，将所有勾选的组合拼成字符串
        private void UpdateTextBoxFromMatrix()
        {
            var selected = new List<string>();
            foreach (Control ctrl in tableMatrix.Controls)
            {
                if (ctrl is CheckBox chk && chk.Checked && chk.Tag is string comboStr)
                {
                    selected.Add(comboStr);
                }
            }

            // 临时注销事件避免递归死循环
            txtCurrentCombos.TextChanged -= TxtCurrentCombos_TextChanged;
            txtCurrentCombos.Text = string.Join(",", selected);
            txtCurrentCombos.TextChanged += TxtCurrentCombos_TextChanged;
        }

        // 文本框手动输入时，反向同步勾选矩阵
        private void TxtCurrentCombos_TextChanged(object sender, EventArgs e)
        {
            string input = txtCurrentCombos.Text.Replace('，', ',').Replace(" ", "").Trim(',');
            var comboSet = new HashSet<string>(input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            foreach (Control ctrl in tableMatrix.Controls)
            {
                if (ctrl is CheckBox chk && chk.Tag is string comboStr)
                {
                    chk.CheckedChanged -= Chk_CheckedChanged;
                    chk.Checked = comboSet.Contains(comboStr);
                    chk.CheckedChanged += Chk_CheckedChanged;
                }
            }
        }

        private void ToggleAll(bool checkState)
        {
            foreach (Control ctrl in tableMatrix.Controls)
            {
                if (ctrl is CheckBox chk)
                {
                    chk.CheckedChanged -= Chk_CheckedChanged;
                    chk.Checked = checkState;
                    chk.CheckedChanged += Chk_CheckedChanged;
                }
            }
            UpdateTextBoxFromMatrix();
        }

        private void InvertSelection()
        {
            foreach (Control ctrl in tableMatrix.Controls)
            {
                if (ctrl is CheckBox chk)
                {
                    chk.CheckedChanged -= Chk_CheckedChanged;
                    chk.Checked = !chk.Checked;
                    chk.CheckedChanged += Chk_CheckedChanged;
                }
            }
            UpdateTextBoxFromMatrix();
        }
    }
}
