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
    public partial class F2 : Form
    {
        bool AddJianhao = false;
        public static string JianHao;
        public static double[] JD_value = new double[6];
        public static string[] F2_BL = new string[18];
        public static string Workname = null;
        string path_P = System.Environment.CurrentDirectory + @"/设置/P型.mdb";
        string path_shujuku = System.Environment.CurrentDirectory + @"/设置/数据库.mdb";
        string path_wcfx = System.Environment.CurrentDirectory + @"/设置/误差分析.xlsx";
        string Pwd = "HJJCLMSDBCS";
        public F2()
        {
            InitializeComponent();
        }

        private void F2_Load(object sender, EventArgs e)
        {
            //检查数据库中是否存在“测量参数”表,如果不存在新建表
            AccessClass ac1 = new AccessClass();
            ac1.Arr.Clear();
            ac1.TableName(path_shujuku, Pwd);
            if (ac1.Arr.Contains("测量参数") == false)
            {
                Creat_shujuku();
            }
            else
            {
                combox3_ini();
            }
        }

        private void 新增件号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            comboBox3.DropDownStyle = ComboBoxStyle.DropDown;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            JDXZ();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AddJianhao == false)
            {
                Read_shujuku();
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            int[] PP = { 1, 2, 3, 4, 5, 7 };
            int[] TT = { 3, 4, 5, 6 };
            if (comboBox4.SelectedIndex == 0)
            {
                comboBox1.DataSource = TT;
                label18.Text = "行程变动量V2π";
                label19.Text = "行程变动量V25";
                label20.Text = "行程变动量V100";
                label21.Text = "行程变动量V300p";
                label7.Visible = true;
                textBox11.Visible = true;
            }
            else
            {
                comboBox1.DataSource = PP;
                label18.Text = "行程偏差Ep";
                label19.Text = "行程变动量V2π";
                label20.Text = "行程变动量V300p";
                label21.Text = "行程变动量Vup";
                label7.Visible = false;
                textBox11.Visible = false;
            }
            JDXZ();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            JDXZ();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            F2_BL[0] = comboBox2.SelectedIndex.ToString();       //公英制
            F2_BL[1] = comboBox3.Text.ToString().Trim();          //丝杠件号
            F2_BL[2] = textBox8.Text.ToString().Trim();          //丝杠编号
            F2_BL[3] = comboBox4.SelectedIndex.ToString();       //丝杠类型
            F2_BL[4] = comboBox5.SelectedIndex.ToString();       //丝杠旋向
            F2_BL[5] = textBox9.Text.ToString().Trim();          //丝杠导程
            F2_BL[6] = textBox10.Text.ToString().Trim();         //丝杠中径
            F2_BL[7] = textBox12.Text.ToString().Trim();         //丝杠全长
            F2_BL[8] = textBox13.Text.ToString().Trim();         //有效行程
            F2_BL[9] =  comboBox1.Text.ToString().Trim();        //精度等地
            F2_BL[10] = textBox5.Text.ToString().Trim();          //送检人员
            F2_BL[11] = textBox6.Text.ToString().Trim();          //审核人员
            F2_BL[12] = textBox7.Text.ToString().Trim();          //检测人员
            F2_BL[13] = textBox1.Text.ToString().Trim();          //Ep
            F2_BL[14] = textBox2.Text.ToString().Trim();          //V2pai
            F2_BL[15] = textBox3.Text.ToString().Trim();          //V300
            F2_BL[16] = textBox4.Text.ToString().Trim();          //Vup
            F2_BL[17] = textBox11.Text.ToString().Trim();
            Workname = F2_BL[2] + F2_BL[3];
            AccessClass ac1 = new AccessClass();
            ac1.Arr.Clear();
            ac1.Read_col_cells(path_shujuku, Pwd, "测量参数", "丝杠件号");
            if (ac1.Arr.Contains(comboBox3.Text.Trim()) == false)//检测丝杠件号是否存在
            {
                int id;
                ac1.MaxId(path_shujuku, Pwd, "测量参数");
                if (ac1.Arr.Count != 0)
                {
                    id = Convert.ToInt32(ac1.return_data) + 1;
                }
                else
                {
                    id = 1;
                }
                string data_0 = id.ToString() + ",'" + F2_BL[0] + "','" + F2_BL[1] + "','" + F2_BL[2] + "','" + F2_BL[3] + "','" + F2_BL[4] + "','" + F2_BL[5] + "','" + F2_BL[6] + "','" + F2_BL[7] + "','" + F2_BL[8] + "','" + F2_BL[9] + "','" + F2_BL[10] + "','" + F2_BL[11] + "','" + F2_BL[12] + "'";
                ac1.Into_rows(path_shujuku, Pwd, "测量参数", data_0);
                MessageBox.Show("新丝杠测量信息储存成功");
            }
            else
            {
                string[] aa = { "公英制", "丝杠件号", "丝杠编号", "丝杠类型", "丝杠旋向", "丝杠导程", "丝杠中径", "丝杠全长", "有效行程", "精度等级", "送检人员", "审核人员", "检测人员" };
                string name_data = null;
                for (int i = 0; i < 12; i++)
                {
                    name_data = name_data + aa[i] + "='" + F2_BL[i] + "',";
                }
                name_data = name_data + aa[12] + "='" + F2_BL[12] + "'";
                ac1.Up_data(path_shujuku, Pwd, "测量参数", name_data, "丝杠件号", comboBox3.Text);
                MessageBox.Show(comboBox3.Text + "测量参数信息更新完成");
            }
            combox3_ini();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void Creat_shujuku()
        {
            AccessClass ac1 = new AccessClass();
            ac1.CreateAccess_table(path_shujuku, Pwd, "测量参数");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "公英制", "  Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠件号", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠编号", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠类型", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠旋向", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠导程", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠中径", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "丝杠全长", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "有效行程", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "精度等级", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "送检人员", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "审核人员", "Memo");
            ac1.Add_col(path_shujuku, Pwd, "测量参数", "检测人员", "Memo");
        }
        private void combox3_ini()
        {
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            AccessClass ac1 = new AccessClass();
            ac1.Arr.Clear();
            ac1.Read_col_cells(path_shujuku, Pwd, "测量参数", "丝杠件号");
            if (ac1.Arr.Count != 0)
            {
                comboBox3.DataSource = ac1.Arr;
            }
            comboBox3.SelectedIndex = 0;

        }
        private void Read_shujuku()
        {
            JianHao = comboBox3.Text.Trim();
            AccessClass ac1 = new AccessClass();
            string tiaojian = "丝杠件号 = " + "'" + JianHao + "'";
            ac1.Read_line(path_shujuku, Pwd, "测量参数", tiaojian);
            comboBox2.SelectedIndex = Convert.ToInt32(ac1.myobj[1].ToString().Trim());  //公英制
            comboBox3.Text = ac1.myobj[2].ToString().Trim();  //丝杠件号
            textBox8.Text = ac1.myobj[3].ToString().Trim();  //丝杠编号
            comboBox4.SelectedIndex = Convert.ToInt32(ac1.myobj[4].ToString().Trim());  //丝杠类型
            comboBox5.SelectedIndex = Convert.ToInt32(ac1.myobj[5].ToString().Trim());  //丝杠旋向
            textBox9.Text = ac1.myobj[6].ToString().Trim();  //丝杠导程
            textBox10.Text = ac1.myobj[7].ToString().Trim();  //丝杠中径
            textBox12.Text = ac1.myobj[8].ToString().Trim();  //丝杠全长
            textBox13.Text = ac1.myobj[9].ToString().Trim();  //测量行程
            comboBox1.SelectedText = ac1.myobj[10].ToString().Trim();  //精度等地
            textBox5.Text = ac1.myobj[11].ToString().Trim();  //送检人员
            textBox6.Text = ac1.myobj[12].ToString().Trim();  //审核人员
            textBox7.Text = ac1.myobj[13].ToString().Trim();  //检测人员 
        }
        private void JDXZ()
        {
            //判断有效行程
            int jddj = comboBox1.SelectedIndex;
            if (textBox13.Text.Trim() != "")
            {
                double changdu = Convert.ToDouble(textBox13.Text.Trim());//有效测量长度
                if ((changdu > 0) && (changdu <= 6300))
                {
                    if (comboBox4.SelectedIndex == 0)//T型
                    {
                        JD_T_Value jdt = new JD_T_Value();
                        jdt.Jd_data(changdu, jddj);
                        textBox1.Text = jdt.Value_V2π.ToString();
                        textBox2.Text = jdt.Value_V25.ToString();
                        textBox3.Text = jdt.Value_V100.ToString();
                        textBox4.Text = jdt.Value_V300.ToString();
                        textBox11.Text = jdt.Value_Vup.ToString();
                    }
                    else//P型
                    {
                        JD_P_Value jdp = new JD_P_Value();
                        jdp.Jd_data(changdu, jddj);
                        textBox1.Text = jdp.Value_Ep.ToString();
                        textBox2.Text = jdp.Value_V2π.ToString();
                        textBox3.Text = jdp.Value_V300.ToString();
                        textBox4.Text = jdp.Value_Vup.ToString();
                    }
                }
            }
        }


    }
}
