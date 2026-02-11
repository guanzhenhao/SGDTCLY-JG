using GZH_ClassLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 丝杠动态测量仪
{
    public partial class 密码修改 : Form
    {
        ArrayList arl = new ArrayList();
        string path_shujuku_1 = System.Environment.CurrentDirectory + @"/设置/软件设置.mdb";
        string Pwd = "HJJCLMSDBCS";
        public 密码修改()
        {
            InitializeComponent();
        }

        private void 密码修改_Load(object sender, EventArgs e)
        {
            if (登录.QuanXian == 1)
            {
                arl.Clear();
                arl.Add("操作人员");
            }
            if (登录.QuanXian == 2)
            {
                arl.Clear();
                arl.Add("操作人员");
                arl.Add("管理人员");
            }
            if (登录.QuanXian == 3)
            {
                arl.Clear();
                arl.Add("操作人员");
                arl.Add("管理人员");
                arl.Add("维护人员");
            }
            comboBox1.DataSource = arl;
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != textBox2.Text)
            {
                MessageBox.Show("密码输入不一致!请重新输入");
            }
            else
            {
                AccessClass ac1 = new AccessClass();
                string name_data = "口令='" + textBox2.Text.Trim() + "'";
                ac1.Up_data(path_shujuku_1, Pwd, "登录", name_data, "权限", comboBox1.Text);
                MessageBox.Show(comboBox1.Text + ":密码修改成功！" +
                    "新密码为:" + textBox2.Text.Trim());
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
