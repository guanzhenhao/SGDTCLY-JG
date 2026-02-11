using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 丝杠动态测量仪
{
    public partial class 添加件号 : Form
    {
        public string New_Jianhao = null;
        public 添加件号()
        {
            InitializeComponent();
        }

        private void 添加件号_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            New_Jianhao = textBox1.Text.ToString();
        }
    }
}
