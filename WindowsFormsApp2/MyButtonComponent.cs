using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public partial class MyButtonComponent : Component
    {
        public MyButtonComponent()
        {
            InitializeComponent();
        }

        public MyButtonComponent(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
