
namespace uClient.Broker
{
    partial class TradeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public new void InitializeComponent()
        {
            //在子类方法中执行，否则在父类中执行的数据不能赋值给子类
            this.Load += new System.EventHandler(this.TradeForm_Load);
            base.InitializeComponent();
        }   
    }
}