namespace AutoHorseRace.Utils
{
    public class UIUtils
    {
        /// <summary>
        /// 辅助方法：除第一列CheckBox外，其余文本数据列一律设为只读
        /// </summary>
        public static void ConfigureColumnsReadOnly(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Index == 0)
                {
                    col.ReadOnly = false; // 第一列允许勾选
                }
                else
                {
                    col.ReadOnly = true;  // 其它列锁死，防止鼠标双击误改文本
                }
            }
        }

        /// <summary>
        /// 统一处理整行点击（任意单元格）时，第一列 CheckBox 自动勾选并互斥的逻辑
        /// </summary>
        public static void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0) return;

            // 1. 强行结束编辑，锁定当前 UI 状态
            dgv.EndEdit();

            // 2. 获取当前行 CheckBox 单元格
            var currentCell = dgv.Rows[e.RowIndex].Cells[0];
            bool isCurrentChecked = Convert.ToBoolean(currentCell.Value);

            // 3. 如果点击的是文本列，反转状态；如果点的是复选框本身，维持其点击后的状态
            if (e.ColumnIndex != 0)
            {
                isCurrentChecked = !isCurrentChecked;
            }
            else
            {
                // 点击第 0 列时，为了防止状态未同步，重新读取一次或取反（根据 WinForms 默认行为调整）
                isCurrentChecked = !isCurrentChecked;
            }

            // 4. 【核心单选排他控制】
            if (isCurrentChecked)
            {
                // 强制把所有行的 CheckBox 全设为 false
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    dgv.Rows[i].Cells[0].Value = false;
                }
                // 仅将当前点击行设为 true
                dgv.Rows[e.RowIndex].Cells[0].Value = true;
            }
            else
            {
                // 操盘防呆：不允许取消勾选（必须选一项）。如果允许选空，把下面这行改为 false
                dgv.Rows[e.RowIndex].Cells[0].Value = true;
            }

            // 5. 提交编辑并刷新
            dgv.RefreshEdit();
        }

        /// <summary>
        /// 统一处理第一列 CheckBox 的高灵敏单选互斥事件
        /// </summary>
        public static void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0) return;

            // 🎯 精准判断：用户点中的是第一列（索引为 0 的 CheckBox 列）
            if (e.ColumnIndex == 0)
            {
                // 关键动作：强行结束当前的编辑状态，让刚刚勾选的布尔值立即写入内存内存
                dgv.EndEdit();

                // 获取当前行点击完毕后的最新选中状态
                bool isCurrentChecked = Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[0].Value);

                if (isCurrentChecked)
                {
                    // 🔄 互斥逻辑：将其余所有行的勾选全部强制抹去
                    for (int i = 0; i < dgv.Rows.Count; i++)
                    {
                        if (i != e.RowIndex)
                        {
                            dgv.Rows[i].Cells[0].Value = false;
                        }
                    }
                }
                else
                {
                    // 操盘防呆风控：如果不允许用户通过再次点击把选中的行“反选”为空，可以强行重新锁死 true
                    dgv.Rows[e.RowIndex].Cells[0].Value = true;
                }
            }
        }

        /// <summary>
        /// 为 DataGridView 指定的字段数组启用负数（带括号格式）自动显示为红色的功能
        /// </summary>
        /// <param name="dgv">目标 DataGridView 对象</param>
        /// <param name="targetColumnNames">需要应用此规则的列名字段数组</param>
        public static void EnableNegativeValueRedFormatting(DataGridView dgv, string[] targetColumnNames)
        {
            if (dgv == null || targetColumnNames == null || targetColumnNames.Length == 0)
                return;

            // 绑定 CellFormatting 事件
            dgv.CellFormatting += (sender, e) =>
            {
                // 确保行索引有效，且当前列在指定的字段数组中
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.ColumnIndex < dgv.Columns.Count)
                {
                    string currentColumnName = dgv.Columns[e.ColumnIndex].Name;

                    // 检查当前列是否在目标数组中
                    if (Array.Exists(targetColumnNames, name => name.Equals(currentColumnName)))
                    {
                        if (e.Value != null)
                        {
                            string text = e.Value.ToString();

                            // 判断是否为带括号的负数格式，如 ($9.24)
                            if (text.StartsWith("(") && text.EndsWith(")"))
                            {
                                e.CellStyle.ForeColor = Color.Red;
                            }
                            else
                            {
                                e.CellStyle.ForeColor = Color.Black; // 恢复默认颜色
                            }
                        }
                    }
                }
            };
        }
    }
}
