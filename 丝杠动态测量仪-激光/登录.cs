using GZH_ClassLibrary;
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
    public partial class 登录 : Form
    {
        public static double QuanXian = 0;
        string path_shujuku_1 = System.Environment.CurrentDirectory + @"/设置/软件设置.mdb";
        string Pwd = "HJJCLMSDBCS";
        public 登录()
        {
            InitializeComponent();
        }

        private void 登录_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0; 
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string TiaoJian = $"权限='{comboBox1.Text.ToString().Trim()}'";
            AccessClass ac1 = new AccessClass();
            ac1.Read_cell(path_shujuku_1, Pwd, "登录", "口令", TiaoJian);
            if (textBox1.Text == ac1.return_data.Trim())
            {

                if (comboBox1.SelectedIndex == 0)
                {
                    QuanXian = 1;
                }
                if (comboBox1.SelectedIndex == 1)
                {
                    QuanXian = 2;
                }
                if (comboBox1.SelectedIndex == 2)
                {
                    QuanXian = 3;
                }
                this.Hide();
                Form f1 = new Form1();
                f1.Show();
            }
            else
            {
                MessageBox.Show("密码输入错误，请重新输入");
            }
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != (char)Keys.Back && e.KeyChar != (char)Keys.Delete && e.KeyChar != (char)Keys.ShiftKey && e.KeyChar != (char)Keys.Enter)//如果不是字符 也不是数字
            {
                e.Handled = true; //当前输入处理置为已处理。即文本框不再显示当前按键信息   
                MessageBox.Show("只能输入字母,数字和'.'");
            }
            else
            {
                e.Handled = false;
            }
        }
    }
}
