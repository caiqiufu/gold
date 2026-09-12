using M4.Demo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mF4Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new Client();
            var (isSuccessful, message) = client.Login("90002019", "qazwsx123", false);
            if (!isSuccessful)
            {
                Console.WriteLine("登录失败");
            }
            else
            {
                Console.WriteLine("登录成功");
            }
            if(message!=null)
            {
                Console.WriteLine(message);
            }
        }
    }
}
