using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    class ButtonEx : System.Windows.Forms.Button
    {
        private Timer timer = new Timer();
        public delegate void ChangColor(Color c);
        public ButtonEx()
        {
            timer.Enabled = true;
            timer.Interval = 500;
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {

            if (this.BackColor == Color.Transparent)
            {
                this.BackColor = Color.Green;
            }
            else
            {
                this.BackColor = (Color.Transparent);
            }
        }

        private void Changing(Color c)
        {
            this.BackColor = c;
        }
        protected override void Dispose(bool disposing)
        {
            this.timer.Enabled = false;
            this.timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
