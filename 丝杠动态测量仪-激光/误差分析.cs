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
    public partial class 误差分析 : Form
    {
        public string DaiKuan;
        double A, B, V2πmax, V25max, V100max, V300max, Vn, Vnmax, daocheng;
        public double qdx, qdy0,qdy1, qdy2, zdx, zdy0,zdy1, zdy2;
        public double Ep, V2π, V25, V100, V300, Vup;
        public 误差分析()
        {
            InitializeComponent();
        }
        private void 误差分析_Load(object sender, EventArgs e)
        {
            if(Form1.CL_Z.Count()==0)
            {
                MessageBox.Show("无有效测量数据！");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            daocheng = Convert.ToDouble(F2.F2_BL[5]);
            if (Form1.LeiXing == 0)//T型
            {
                textBox1.Text = "0";                    //Ep
                textBox2.Text = F2.F2_BL[13].ToString();//V2pai
                textBox3.Text = F2.F2_BL[14].ToString();//V25
                textBox4.Text = F2.F2_BL[15].ToString();//V100
                textBox5.Text = F2.F2_BL[16].ToString();//V300
                textBox6.Text = F2.F2_BL[17].ToString();//Vup
            }
            else
            {
                textBox1.Text = F2.F2_BL[13].ToString();//Ep
                textBox2.Text = F2.F2_BL[14].ToString();//V2pai
                textBox3.Text = "0";
                textBox4.Text = "0";
                textBox5.Text = F2.F2_BL[15].ToString();//V300
                textBox6.Text = F2.F2_BL[16].ToString();//Vup
            }
            jisuan(Form1.CL_Z, Form1.CL_error);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void jisuan(List<double> X, List<double> Y)
        {
            Error_FenXi(X, Y, daocheng);
            textBox7.Text = Ep.ToString("f2");
            textBox8.Text = V2π.ToString("f2");
            textBox9.Text = V25.ToString("f2");
            textBox10.Text = V100.ToString("f2");
            textBox11.Text = V300.ToString("f2");
            textBox12.Text = Vup.ToString("f2");
            textBox19.Text = "0";
            textBox20.Text = V2πmax.ToString("f2");
            textBox21.Text = V25max.ToString("f2");
            textBox22.Text = V100max.ToString("f2");
            textBox23.Text = V300max.ToString("f2");
            textBox24.Text = "0";
            qdx =X[0];
            zdx = X[X.Count - 1]-X[0];
            qdy0 = A + B * qdx;
            zdy0 = A + B * zdx;
        }

        private void Error_FenXi(List<double> X, List<double> Y, double DaoCheng)
        {
            int count = X.Count;
            A_B(X, Y);
            SJ_Ep(X, Y);
            SJ_Vup(X, Y);

            SJ_Vn(X, Y, DaoCheng);
            V2π = Vn;
            V2πmax = Vnmax;

            SJ_Vn(X, Y, 25);
            V25 = Vn;
            V25max = Vnmax;

            SJ_Vn(X, Y, 100);
            V100 = Vn;
            V100max = Vnmax;

            SJ_Vn(X, Y, 300);
            V300 = Vn;
            V300max = Vnmax;

        }
        private void A_B(List<double> X, List<double> Y)//回归直线参数 A,B
        {
            double sr = 0;
            double srr = 0;
            double se = 0;
            double sre = 0;
            int n = X.Count();
            for (int i = 0; i < n; i++)
            {
                sr = sr + X[i];
                srr = srr + X[i] * X[i];
                se = se + Y[i];
                sre = sre + X[i] * Y[i];
            }
            A = (srr * se - sr * sre) / (n * srr - sr * sr);
            B = (n * sre - sr * se) / (n * srr - sr * sr);
        }
        private void SJ_Ep(List<double> X, List<double> Y)//实际平均行程偏差Ep函数
        {
            A_B(X, Y);
            Ep = B * (X[X.Count() - 1] - X[0]);
        }
        private void SJ_Vup(List<double> X, List<double> Y)//全程跳动度Vup函数
        {
            Vup = 0;
            double Vup_max = Y[0];
            double Vup_min = Y[0];
            for (int i = 0; i < X.Count(); i++)
            {
                if (Y[i] > Vup_max) Vup_max = Y[i];
                if (Y[i] > Vup_min) Vup_min = Y[i];
            }
            Vup = Vup_max - Vup_min;
        }
        private void SJ_Vn(List<double> X, List<double> Y, double L)//计算一定长度（2π、25、100、300）跳动度函数
        {
            Vn = 0;
            double DH, DL;
            int n = X.Count();
            if (X[n - 1] - X[0] < L)
            {
                SJ_Vup(X, Y);
                Vn = Vup;
                Vnmax = 0;
            }
            for (int i = 0; i < n; i++)
            {
                if (X[i] > X[n - 1] - L) break;
                DH = Y[i];
                DL = Y[i];
                for (int j = 0; j < n; j++)
                {
                    if (X[i + j] > X[i] + L) break;
                    if (Y[i + j] > DH) DH = Y[i + j];
                    if (Y[i + j] < DL) DL = Y[i + j];
                    double D_cha = DH - DL;
                    if (D_cha > Vn)
                    {
                        Vn = D_cha;
                        Vnmax = X[i];
                    }
                }
            }
        }
    }
}
