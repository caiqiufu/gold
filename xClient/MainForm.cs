using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private ArrayList _Forms = new ArrayList();
        private void button1_Click(object sender, EventArgs e)
        {
            if (_Forms.Count < 10)
            {
                frmTrade form2 = new frmTrade();
                form2.Show();
                _Forms.Add(form2);
            }
            else
            {
                MessageBox.Show("最多只能打开10个窗口");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_Forms.Count>0)
            {
                foreach (frmTrade f in _Forms)
                {
                    f.Close();
                }
            }
        }
    }
}
