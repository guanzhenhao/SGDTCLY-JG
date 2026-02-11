using GZH_ClassLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace 丝杠动态测量仪
{
    public partial class Form1 : Form
    {
        #region IK220.DLL相关
        public bool m_StatusOn;
        public bool m_IK220found;
        bool[] m_Active = new bool[16];
        UInt32[] IKCard = new UInt32[16];
        UInt32[] OldSta = new UInt32[8];
        UInt32[] m_SignalType = new UInt32[8];//设置信号类型
        UInt32[] m_EncoderType = new UInt32[8];//设置编码器类型
        Double[] m_SignalPeriod = new Double[8];//设置信号周期
        [DllImport("IK220Dll.dll")] extern static bool IK220Find(UInt32[] ikcard);//发现轴地址
        [DllImport("IK220Dll.dll")] extern static bool IK220Init(UInt16 ax);//初始化轴
        [DllImport("IK220Dll.dll")] extern static bool IK220WritePar(UInt16 Axis, UInt16 ParNum, UInt32 ParVal);
        [DllImport("IK220Dll.dll")] extern static bool IK220Read48(UInt16 Axis, UInt16 Latch, ref Double pData);
        [DllImport("IK220Dll.dll")] extern static bool IK220ReadEn(UInt16 Axis, ref UInt16 pStatus, ref Double pData, ref UInt16 pAlarm);
        [DllImport("IK220Dll.dll")] extern static bool IK220ResetEn(UInt16 Axis, ref UInt16 pStatus);
        [DllImport("IK220Dll.dll")] extern static bool IK220ConfigEn(UInt16 Axis, ref UInt16 pStatus, ref UInt16 pType, ref UInt32 pPeriod, ref UInt32 pStep, ref UInt16 pTurns, ref UInt16 pRefDist, ref UInt16 pCntDir);
        [DllImport("IK220Dll.dll")] extern static bool IK220Reset(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220Start(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220Stop(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220ResetRef(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220StartRef(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220StopRef(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220LatchRef(UInt16 Axis);
        [DllImport("IK220Dll.dll")] extern static bool IK220Latched(UInt16 Axis, UInt16 Latch, ref bool pStatus);
        [DllImport("IK220Dll.dll")] extern static bool IK220Get48(UInt16 Axis, UInt16 Latch, ref double data);

        #endregion
        #region GT400.DLL相关
        Int16 rtn;
        [DllImport("GT400.dll")] extern static Int16 GT_Open();
        [DllImport("GT400.dll")] extern static Int16 GT_Reset();
        [DllImport("GT400.dll")] extern static Int16 GT_SwitchtoCardNo(ushort card_no);
        [DllImport("GT400.dll")] extern static Int16 GT_SetSmplTm(Double Timer);
        [DllImport("GT400.dll")] extern static Int16 GT_Axis(ushort num);
        [DllImport("GT400.dll")] extern static Int16 GT_SetIntrMsk(ushort Mask);
        [DllImport("GT400.dll")] extern static Int16 GT_ClrSts();
        [DllImport("GT400.dll")] extern static Int16 GT_CtrlMode(int mode);
        [DllImport("GT400.dll")] extern static Int16 GT_StepPulse();
        [DllImport("GT400.dll")] extern static Int16 GT_LmtsOff();
        [DllImport("GT400.dll")] extern static Int16 GT_AxisOn();
        [DllImport("GT400.dll")] extern static Int16 GT_ZeroPos();
        [DllImport("GT400.dll")] extern static Int16 GT_PrflT();
        [DllImport("GT400.dll")] extern static Int16 GT_SetAcc(Double Acc);
        [DllImport("GT400.dll")] extern static Int16 GT_SetVel(double Vel);
        [DllImport("GT400.dll")] extern static Int16 GT_SetPos(long Pos);
        [DllImport("GT400.dll")] extern static Int16 GT_Update();
        [DllImport("GT400.dll")] extern static Int16 GT_SmthStp();
        [DllImport("GT400.dll")] extern static Int16 GT_AxisOff();
        [DllImport("GT400.dll")] extern static Int16 GT_Close();
        [DllImport("GT400.dll")] extern static short GT_GetSts(ref ushort Status);
        [DllImport("GT400.dll")] extern static short GT_ExInpt(ref ushort Data);

        #endregion
        #region 定义变量
        public static int QuanXian = 0, LeiXing = 0;
        string path_shujuku = System.Environment.CurrentDirectory + @"/设置/数据库.mdb";
        string path_shujuku_1 = System.Environment.CurrentDirectory + @"/设置/软件设置.mdb";
        string path_image = "D:\\image1.Bmp";
        string Pwd = "HJJCLMSDBCS";
        int XZZB, YZZB, SBBJ;
        bool First_load = true, CL_start = false, LingWei = false;
        double Daocheng, Changdu, JDWZ, Click_X1, Click_X2, Move_X, CDB = 0.3, Celiang_Z, Celiang_C, Error_Z, Lvbo, Start_Z, Start_C;
        double Ep, V2π, V25, V100, V300, Vup;
        List<double> ZXBC_Z = new List<double>();
        List<double> ZXBC_error = new List<double>();
        List<double> SJ_error = new List<double>();
        public static List<double> CL_Z = new List<double>();
        public static List<double> CL_error = new List<double>();
        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            First_load = false;
            Load_ini();
            comboBox1.SelectedIndex = 19;
            comboBox2.SelectedIndex = 3;
            comboBox3.SelectedIndex = 18;
            comboBox4.SelectedIndex = 0;
            First_load = true;
            chart_ini();
        }
        private void 丝杠参数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(CL_start==false)
            {
                F2_Show();
            }

        }
        private void 光栅尺数据补偿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form gscbc = new 光栅尺补偿();
            gscbc.Show();
        }
        private void 温度补偿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PCI1713 pci = new PCI1713();
            pci.Show();
        }
        private void 误差分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CL_start == false)
            {
                误差分析 wcfx = new 误差分析();
                if (wcfx.ShowDialog() == DialogResult.OK)
                {
                    chart1.Series[1].Points.Clear();
                    chart1.Series[1].Points.AddXY(wcfx.qdx, wcfx.qdy0);
                    chart1.Series[1].Points.AddXY(wcfx.zdx, wcfx.zdy0);
                    string a = "全程变动量Vup=" + wcfx.DaiKuan;
                    chart1.Titles[0].Text = a;
                    chart1.Titles[0].Visible = true;
                    Ep = wcfx.Ep;
                    V2π = wcfx.V2π;
                    V25 = wcfx.V25;
                    V100 = wcfx.V100;
                    V300 = wcfx.V300;
                    Vup = wcfx.Vup;
                    chart1.SaveImage(path_image, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Bmp);
                }
            }
        }
        private void 打印报表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CL_start == false)
            {
                // 将设置好的打印页 用作 PrintDocument进行打印。
                pageSetupDialog1.PageSettings.Landscape = true;
                pageSetupDialog1.PageSettings.Margins.Left = 200;
                pageSetupDialog1.PageSettings.Margins.Right = 100;
                pageSetupDialog1.PageSettings.Margins.Top = 100;
                pageSetupDialog1.PageSettings.Margins.Bottom = 100;
                pageSetupDialog1.ShowDialog();
                printDocument1.DefaultPageSettings = pageSetupDialog1.PageSettings;
                // 设置要预览的文档
                printPreviewDialog1.Document = printDocument1;
                // 开启操作系统的抗锯齿功能
                printPreviewDialog1.UseAntiAlias = true;
                printPreviewDialog1.ShowDialog();
            }
        }
        private void 用户登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form gly = new 登录();
            gly.ShowDialog();
        }
        private void 密码修改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            密码修改 mmxg = new 密码修改();
            mmxg.Show();
        }
        private void timer1_Tick(object sender, EventArgs e) 
        {
            timer1.Stop();
            IK220_ReadValue();
            textBox2.Text = Celiang_Z.ToString("f4");
            if(CL_start==true)
            {
                double clcd = Math.Abs(Celiang_Z- Start_Z);
                textBox5.Text = clcd.ToString("f2");
                if (clcd <Daocheng)
                {
                    Error_Z = 0;
                    Start_Z = Celiang_Z;//记录有效起点Z
                    Start_C = Celiang_C;//记录有效起点C
                }
                else
                {
                    if(SJ_error.Count==10)
                    {
                        SJ_error.RemoveAt(0);
                    }
                    else
                    {
                        double z1 = Math.Abs(Celiang_Z - Start_Z);
                        double z2 = Daocheng * (Math.Abs(Celiang_C - Start_C) / 360);
                        SJ_error.Add((z1-z2)*1000);
                    }
                    //冒泡排序
                    for (int j = SJ_error.Count-1; j > 0; j--)  //每排一次，剩下的无序数减一
                    {
                        for (int i = 0; i < j; i++)    //一个for循环获得一个最大的数
                        {
                            if (SJ_error[i] > SJ_error[i + 1])  //数值互换
                            {
                                var sap = SJ_error[i];
                                SJ_error[i] = SJ_error[i + 1];
                                SJ_error[i + 1] = sap;
                            }
                        }
                    }
                    Error_Z = SJ_error[(int)(SJ_error.Count / 2)];
                }
                CL_Z.Add(clcd);
                CL_error.Add(Error_Z);
                textBox4.Text = Error_Z.ToString("f2");
                chart1.Series[0].Points.AddXY(clcd, Error_Z);
                if (clcd >= Changdu)
                {
                    Stop_CL();
                }
            }
            timer1.Start();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //清空原有数据
            ZXBC_Z.Clear();
            ZXBC_error.Clear();
            SJ_error.Clear();
            CL_Z.Clear();
            CL_error.Clear();
            if(LingWei==true)
            {                
                button1.BackColor = Color.Green;
                button2.BackColor = Color.Red;
                button3.BackColor = Color.Red;
                chart1.Titles[0].Visible = false;
                DianJi_Start();
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                Error_Z = 0;
                CL_start = true;
                int timer_interval= Convert.ToInt32(600 / (Daocheng * Convert.ToDouble(comboBox1.Text.ToString().Trim())));
                if(timer_interval<=40)
                {
                    timer_interval = 40;
                }
                timer1.Interval = timer_interval;
                timer1.Start();
            }
            else
            {
                MessageBox.Show("请回零！");
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Interval = 75;
            timer1.Start();
            Stop_CL();
            button1.BackColor = Color.Red;
            button2.BackColor = Color.Green;
            button3.BackColor = Color.Red;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            IK220_Readstop();
            IK220_Readreset();
            GT400_Initial();
            button1.BackColor = Color.Red;
            button2.BackColor = Color.Red;
            button3.BackColor = Color.Green;
            button4.BackColor = Color.Red;
            CL_start = false;
            LingWei = false;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("是否向上剔除数据？", "提示", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                double x1 = Convert.ToDouble(textBox1.Text.ToString());
                double x2 = Convert.ToDouble(textBox7.Text.ToString());
                List<double> error = new List<double>();
                for (int i = 0; i < 0; i--)
                {
                    if ((CL_Z[i] > x1) || (CL_Z[i] < x2))
                    {
                        error.Add(CL_error[i]);
                    }
                }

                chart1.Series[0].Points.Clear();
                for (int j = 0; j < CL_Z.Count() - 1; j++)
                {
                    chart1.Series[0].Points.AddXY(CL_Z[j], CL_error[j]);//绘制折线图
                }
            }
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ZXBC_Z = CL_Z;
            ZXBC_error = CL_error;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            DianJi_Start();
            HuiLing_Start();
            LingWei = true;
            timer1.Interval = 75;
            button3.BackColor = Color.Red;
            button4.BackColor = Color.Green;
            timer1.Start();
            Stop_CL();
        }
        private void 数据存档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CL_start == false)
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                fileDialog.Title = "请选择文件";
                fileDialog.Filter = "数据文件(*.CSV)|*.CSV";
                fileDialog.FileName = F2.F2_BL[2].ToString() + "_" + F2.F2_BL[3].ToString() + ".CSV";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    string Savepath = fileDialog.FileName;
                    FileStream fs = new FileStream(Savepath, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);
                    for (int i = 0; i < CL_Z.Count(); i++)
                    {
                        sw.WriteLine(CL_Z[i].ToString() + "," + CL_error[i].ToString());
                    }
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    MessageBox.Show("数据存储完成!");
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("是否截取数据", "提示", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                double x1 = Convert.ToDouble(textBox1.Text.ToString());
                double x2 = Convert.ToDouble(textBox7.Text.ToString());
                List<double> clz = new List<double>();
                List<double> clerror = new List<double>();
                for (int i = 0; i<CL_Z.Count; i++)
                {
                    if ((CL_Z[i] > x1) && (CL_Z[i] < x2))
                    {
                        clz.Add(CL_Z[i]);
                        clerror.Add(CL_error[i]);
                    }
                }
                CL_Z.Clear();
                CL_error.Clear();
                chart1.Series[0].Points.Clear();
                chart1.Series[2].Points.Clear();
                for (int j=0;j<clz.Count;j++)
                {
                    CL_Z.Add(clz[j] - clz[0]);
                    CL_error.Add(clerror[j] - clerror[0]);
                    chart1.Series[0].Points.AddXY(clz[j] - clz[0], clerror[j] - clerror[0]);
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            IK220_Readstop();
            IK220_Readreset();
            GT400_Stop();
            System.Environment.Exit(0);
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            YZZB = Convert.ToInt32(comboBox2.Text);
            chart1.ChartAreas[0].AxisY.Minimum = -1 * YZZB;
            chart1.ChartAreas[0].AxisY.Maximum = YZZB;
            chart1.ChartAreas[0].AxisY.Interval = YZZB / 2;
            chart1.ChartAreas[0].AxisY.MinorGrid.Interval = 1;
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = "0";
            textBox7.Text = comboBox3.Text;
            XZZB = Convert.ToInt32(comboBox3.Text);
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = XZZB;
            chart1.ChartAreas[0].AxisX.Interval = XZZB / 10;
            chart1.ChartAreas[0].AxisX.ScrollBar.Size = 5;
        }
        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            Move_X = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);//xv是定义好的全局变量
            StripLine sl1 = new StripLine();//新建一个插入线
            StripLine sl2 = new StripLine();//新建一个插入线
            if (SBBJ == 0)
            {
                SBBJ = 1;
                Click_X1 = Move_X;
                sl1.BackColor = System.Drawing.Color.Red;
                sl1.IntervalOffset = Click_X1;//通过IntervalOffset来设置线的位置        
                sl1.StripWidth = 0.2;//设置线的宽度
                sl1.Tag = 1;//设置线的标记，用于区分选中的哪个
                chart1.ChartAreas[0].AxisX.StripLines.Add(sl1);//插入垂直于X轴的StripLine，若垂直于Y轴则用AxisY。
            }
            else
            {
                if (SBBJ == 1)
                {
                    SBBJ = 2;
                    Click_X2 = Move_X;
                    sl1.BackColor = System.Drawing.Color.Red;
                    sl1.IntervalOffset = Click_X2;//通过IntervalOffset来设置线的位置        
                    sl1.StripWidth = 0.2;//设置线的宽度
                    sl1.Tag = 1;//设置线的标记，用于区分选中的哪个
                    chart1.ChartAreas[0].AxisX.StripLines.Add(sl1);//插入垂直于X轴的StripLine，若垂直于Y轴则用AxisY。
                    if (Click_X1 > Click_X2)
                    {
                        textBox1.Text = Click_X2.ToString();
                        textBox7.Text = Click_X1.ToString();
                    }
                    else
                    {
                        textBox1.Text = Click_X1.ToString();
                        textBox7.Text = Click_X2.ToString();
                    }
                    double x1 = Convert.ToDouble(textBox1.Text.ToString());
                    double x2 = Convert.ToDouble(textBox7.Text.ToString());
                    for (int i = 0; i < CL_Z.Count; i++)
                    {
                        if ((CL_Z[i] > x1) && (CL_Z[i] < x2))
                        {
                            chart1.Series[2].Points.AddXY(CL_Z[i], CL_error[i]);
                        }
                    }
                    button5.BackColor = Color.Green;
                    button5.Enabled = true;
                }
                else
                {
                    SBBJ = 0;
                    chart1.Series[2].Points.Clear();
                    chart1.ChartAreas[0].AxisX.StripLines.Clear();
                    textBox1.Text = "0";
                    textBox7.Text = comboBox3.Text;
                    button5.BackColor = Color.Firebrick;
                    button5.Enabled = false;
                    textBox1.Text = "0";
                    textBox7.Text = comboBox3.Text;
                }
            }
        }
        private void Load_ini() //读取panel1相关信息
        {
            //IK220_Ini();
            //读取基础设置信息
            First_load = false;
            F2_Show();
        }
        private void chart_ini() //chart初始化
        {
            chart1.Series[0].Color = Color.Blue;
            chart1.Series[0].BorderWidth = 1;
            chart1.Series[0].ChartType = SeriesChartType.Line;//设置chart控件图表类型为折线图
            chart1.Series[1].ChartType = SeriesChartType.Line;
            chart1.Series[0].Points.AddXY(0, 0);
            //chart1.SaveImage("e:\\dd.Bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Daocheng>=4)
            {
                if(Convert.ToInt32(comboBox1.Text)>=45)
                {
                    comboBox1.SelectedIndex = 44;
                }
            }
            if((Daocheng>4)&&(Daocheng<=5))
            {
                if (Convert.ToInt32(comboBox1.Text) >= 40)
                {
                    comboBox1.SelectedIndex = 39;
                }
            }
            if ((Daocheng > 5) && (Daocheng <= 6))
            {
                if (Convert.ToInt32(comboBox1.Text) >= 35)
                {
                    comboBox1.SelectedIndex = 34;
                }
            }
            if ((Daocheng > 6) && (Daocheng <= 8))
            {
                if (Convert.ToInt32(comboBox1.Text) >= 25)
                {
                    comboBox1.SelectedIndex = 24;
                }
            }
            if (Daocheng > 8)
            {
                if (Convert.ToInt32(comboBox1.Text) >= 20)
                {
                    comboBox1.SelectedIndex = 19;
                }
            }
        }
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)            //绘制表格
        {
            string datatime = DateTime.Now.ToLongDateString().ToString();
            if (F2.F2_BL[3]=="0")//Txing
            {
                Pen myPen0 = new Pen(Color.Black, 2);
                Pen myPen1 = new Pen(Color.Black, 1f);
                var pointY = 7;
                var pointX = 10;
                e.Graphics.DrawRectangle(myPen0, new Rectangle(pointX, pointY, 1000, 740));//空心矩形 
                e.Graphics.DrawLine(myPen0, pointX, pointY + 75, pointX + 1000, pointY + 75);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 90, pointX + 1000, pointY + 90);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 105, pointX + 1000, pointY + 105);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 120, pointX + 1000, pointY + 120);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 135, pointX + 1000, pointY + 135);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 150, pointX + 1000, pointY + 150);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 165, pointX + 1000, pointY + 165);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 180, pointX + 1000, pointY + 180);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 195, pointX + 1000, pointY + 195);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 210, pointX + 1000, pointY + 210);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 225, pointX + 1000, pointY + 225);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 240, pointX + 1000, pointY + 240);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 255, pointX + 1000, pointY + 255);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 270, pointX + 1000, pointY + 270);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 285, pointX + 1000, pointY + 285);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 300, pointX + 1000, pointY + 300);
                e.Graphics.DrawLine(myPen0, pointX, pointY + 315, pointX + 1000, pointY + 315);

                e.Graphics.DrawLine(myPen0, pointX + 500, pointY, pointX + 500, pointY + 75);
                e.Graphics.DrawLine(myPen1, pointX + 500, pointY + 75, pointX + 500, pointY + 285);
                e.Graphics.DrawLine(myPen1, pointX + 250, pointY + 75, pointX + 250, pointY + 120);
                e.Graphics.DrawLine(myPen1, pointX + 750, pointY + 75, pointX + 750, pointY + 285);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 165, pointX + 125, pointY + 225);
                e.Graphics.DrawLine(myPen1, pointX + 250, pointY + 285, pointX + 250, pointY + 315);
                e.Graphics.DrawLine(myPen1, pointX + 500, pointY + 300, pointX + 500, pointY + 315);
                e.Graphics.DrawLine(myPen1, pointX + 750, pointY + 300, pointX + 750, pointY + 315);
                e.Graphics.DrawLine(myPen1, pointX + 750, pointY + 150, pointX + 1000, pointY + 135);
                //绘制文字
                var printFont1 = new Font("Times New Roman", 24, System.Drawing.FontStyle.Underline | System.Drawing.FontStyle.Bold);//粗体带下划线
                var printFont2 = new Font("Times New Roman", 12, System.Drawing.FontStyle.Regular | System.Drawing.FontStyle.Bold);
                var printFont3 = new Font("Times New Roman", 8.5f, System.Drawing.FontStyle.Regular | System.Drawing.FontStyle.Bold);
                var printFont4 = new Font("Times New Roman", 8f, System.Drawing.FontStyle.Regular | System.Drawing.FontStyle.Bold);
                var printColor = System.Drawing.Brushes.Black;
                e.Graphics.DrawString("螺距鉴定报告", printFont1, printColor, 150, pointY + 15);
                e.Graphics.DrawString("Precision Ball Screw Travel Error Report", printFont2, printColor, 120, pointY + 50);
                e.Graphics.DrawString("仪器：HJY-028", printFont3, printColor, 550, pointY + 10);
                e.Graphics.DrawString("Instr：3-Meter Ball-Screw Travel Error Dynamic Measuring System", printFont3, printColor, 550, pointY + 25);
                e.Graphics.DrawString("单位：MTAR Technologies Pvt.Ltd,UNIT3", printFont3, printColor, 550, pointY + 40);
                e.Graphics.DrawString("Corp：MTAR Technologies Pvt.Ltd,UNIT3", printFont3, printColor, 550, pointY + 55);

                e.Graphics.DrawString(@"件号/ Part NO.", printFont3, printColor, 70, pointY + 78);
                e.Graphics.DrawString(@"导程/ Lead", printFont3, printColor, 70, pointY + 93);
                e.Graphics.DrawString(@"送检/ Producer", printFont3, printColor, 70, pointY + 108);

                e.Graphics.DrawString(@"编号/ Serial NO.", printFont3, printColor, 550, pointY + 78);
                e.Graphics.DrawString(@"螺纹全长/ Useful travel", printFont3, printColor, 550, pointY + 93);
                e.Graphics.DrawString(@"节径/ Pitch ciecle diameter", printFont3, printColor, 550, pointY + 108);

                e.Graphics.DrawString(@"测量项目/ Measuring  item", printFont3, printColor, 150, pointY + 123);
                e.Graphics.DrawString(@"要求(um)/ Permissible value", printFont3, printColor, 550, pointY + 123);
                e.Graphics.DrawString(@"实测(um)/ Actual value", printFont3, printColor, 840, pointY + 123);
                e.Graphics.DrawString(@"行程补偿值C/ Travel  compenstaion", printFont3, printColor, 150, pointY + 138);
                e.Graphics.DrawString(@"行程偏差E/ Actual mean travel devlation", printFont3, printColor, 150, pointY + 153);
                e.Graphics.DrawString(@"行程变动V", printFont3, printColor, 35, pointY + 173);
                e.Graphics.DrawString(@"Travel variation", printFont3, printColor, 15, pointY + 188);
                e.Graphics.DrawString(@"V2π/ Travel variation in random 1 revolution 2πrad", printFont3, printColor, 150, pointY + 168);
                e.Graphics.DrawString(@"V25/ Travel variation in random 25m", printFont3, printColor, 150, pointY + 183);
                e.Graphics.DrawString(@"V100/ Travel variation in random 100m", printFont3, printColor, 150, pointY + 198);
                e.Graphics.DrawString(@"V300/ Travel variation in random 300m", printFont3, printColor, 150, pointY + 213);
                e.Graphics.DrawString(@"滚到半径误差/ Error of ball track radius", printFont3, printColor, 40, pointY + 228);
                e.Graphics.DrawString(@"滚到法相截面形状误差/ Shape error of ball track normal cross-section", printFont3, printColor, 40, pointY + 243);
                e.Graphics.DrawString(@"螺纹节径尺寸变动量/ Pitch circle diameter variation", printFont3, printColor, 40, pointY + 258);
                e.Graphics.DrawString(@"螺纹滚到面对轴线径向圆跳动/ Variation of ball track section", printFont3, printColor, 40, pointY + 273);
                e.Graphics.DrawString(@"结论/ Conclusion", printFont3, printColor, 100, pointY + 288);
                e.Graphics.DrawString(@"审核/ Additor", printFont3, printColor, 100, pointY + 303);
                e.Graphics.DrawString(F2.F2_BL[11], printFont3, printColor, 375, pointY + 303);
                e.Graphics.DrawString(@"检验/ Inspector：", printFont3, printColor, 600, pointY + 303);
                e.Graphics.DrawString(F2.F2_BL[12], printFont3, printColor, 875, pointY + 288);
                e.Graphics.DrawString(F2.F2_BL[1], printFont3, printColor, 865, pointY + 78);
                e.Graphics.DrawString(F2.F2_BL[5], printFont3, printColor, 865, pointY + 93);
                e.Graphics.DrawString(F2.F2_BL[2], printFont3, printColor, 865, pointY + 108);
                e.Graphics.DrawString(F2.F2_BL[12], printFont3, printColor, 865, pointY + 78);
                e.Graphics.DrawString(F2.F2_BL[8], printFont3, printColor, 865, pointY + 93);
                e.Graphics.DrawString(F2.F2_BL[6], printFont3, printColor, 865, pointY + 108);

                e.Graphics.DrawString(@"0", printFont3, printColor, 620, pointY + 138);
                e.Graphics.DrawString(F2.F2_BL[13], printFont3, printColor, 620, pointY + 153);//Ep
                e.Graphics.DrawString(F2.F2_BL[14], printFont3, printColor, 620, pointY + 168);//V2pai
                e.Graphics.DrawString(F2.F2_BL[15], printFont3, printColor, 620, pointY + 183);//V25
                e.Graphics.DrawString(F2.F2_BL[16], printFont3, printColor, 620, pointY + 198);//V100
                e.Graphics.DrawString(F2.F2_BL[16], printFont3, printColor, 620, pointY + 113);//V300
                e.Graphics.DrawString(Ep.ToString("f3"), printFont3, printColor, 870, pointY + 153);
                e.Graphics.DrawString(V2π.ToString("f3"), printFont3, printColor, 870, pointY + 168);
                e.Graphics.DrawString(V25.ToString("f3"), printFont3, printColor, 870, pointY + 183);
                e.Graphics.DrawString(V100.ToString("f3"), printFont3, printColor, 870, pointY + 198);
                e.Graphics.DrawString(V300.ToString("f3"), printFont3, printColor, 870, pointY + 213);
                string S0 = "丝杠温度/ Ball-Serew Temperature： " + "℃" + " 空气温度/ Air Temperature：" + "℃" + " 光栅温度/ Machine Temperature：" + "℃";
                e.Graphics.DrawString(S0, printFont4, printColor, 20, pointY + 725);
                string S1 = "日期/ Data:"+ DateTime.Now.ToString() + " 测量转速/ Speed："+comboBox1.Text+" rap/min";
                e.Graphics.DrawString(S1, printFont4, printColor, 650, pointY + 710);
                string S2 = "国标 / Standard：GB /T 17587.1/2/3-2002 ISO 3408-1/2/3-2006";
                e.Graphics.DrawString(S2, printFont4, printColor, 650, pointY + 725);
                //加载折线图
                Image image = Image.FromFile("e:/dd.Bmp");
                e.Graphics.DrawImage(image, 20, 330, image.Width * 0.6f, image.Height * 0.4f);
                e.HasMorePages = false;
            }
            else
            {
                Pen myPen0 = new Pen(Color.Black, 2);
                Pen myPen1 = new Pen(Color.Black, 1f);
                var pointY = 7;
                var pointX = 10;
                e.Graphics.DrawRectangle(myPen0, new Rectangle(pointX, pointY, 1000, 740));//空心矩形 
                e.Graphics.DrawLine(myPen0, pointX, pointY + 75, pointX + 1000, pointY + 75);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 90, pointX + 1000, pointY + 90);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 105, pointX + 1000, pointY + 105);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 120, pointX + 1000, pointY + 120);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 135, pointX + 1000, pointY + 135);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 150, pointX + 1000, pointY + 150);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 165, pointX + 1000, pointY + 165);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 180, pointX + 1000, pointY + 180);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 195, pointX + 1000, pointY + 195);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 210, pointX + 1000, pointY + 210);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 225, pointX + 1000, pointY + 225);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 240, pointX + 1000, pointY + 240);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 255, pointX + 1000, pointY + 255);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 270, pointX + 1000, pointY + 270);
                e.Graphics.DrawLine(myPen1, pointX, pointY + 285, pointX + 1000, pointY + 285);
                e.Graphics.DrawLine(myPen0, pointX, pointY + 300, pointX + 1000, pointY + 300);

                e.Graphics.DrawLine(myPen0, pointX + 500, pointY, pointX + 500, pointY + 75);
                e.Graphics.DrawLine(myPen1, pointX + 500, pointY + 75, pointX + 500, pointY + 270);
                e.Graphics.DrawLine(myPen1, pointX + 250, pointY + 75, pointX + 250, pointY + 120);
                e.Graphics.DrawLine(myPen1, pointX + 750, pointY + 75, pointX + 750, pointY + 270);
                e.Graphics.DrawLine(myPen1, pointX + 125, pointY + 165, pointX + 125, pointY + 210);
                e.Graphics.DrawLine(myPen1, pointX + 250, pointY + 270, pointX + 250, pointY + 300);
                e.Graphics.DrawLine(myPen1, pointX + 500, pointY + 285, pointX + 500, pointY + 300);
                e.Graphics.DrawLine(myPen1, pointX + 750, pointY + 285, pointX + 750, pointY + 300);
                e.Graphics.DrawLine(myPen1, pointX + 750, pointY + 150, pointX + 1000, pointY + 135);
                //绘制文字
                var printFont1 = new Font("Times New Roman", 24, System.Drawing.FontStyle.Underline | System.Drawing.FontStyle.Bold);//粗体带下划线
                var printFont2 = new Font("Times New Roman", 12, System.Drawing.FontStyle.Regular | System.Drawing.FontStyle.Bold);
                var printFont3 = new Font("Times New Roman", 8.5f, System.Drawing.FontStyle.Regular | System.Drawing.FontStyle.Bold);
                var printFont4 = new Font("Times New Roman", 8f, System.Drawing.FontStyle.Regular | System.Drawing.FontStyle.Bold);
                var printColor = System.Drawing.Brushes.Black;
                e.Graphics.DrawString("螺距鉴定报告", printFont1, printColor, 150, pointY + 15);
                e.Graphics.DrawString("Precision Ball Screw Travel Error Report", printFont2, printColor, 120, pointY + 50);
                e.Graphics.DrawString("仪器：HJY-028", printFont3, printColor, 550, pointY + 10);
                e.Graphics.DrawString("Instr：3-Meter Ball-Screw Travel Error Dynamic Measuring System", printFont3, printColor, 550, pointY + 25);
                e.Graphics.DrawString("单位：MTAR Technologies Pvt.Ltd,UNIT3", printFont3, printColor, 550, pointY + 40);
                e.Graphics.DrawString("Corp：MTAR Technologies Pvt.Ltd,UNIT3", printFont3, printColor, 550, pointY + 55);

                e.Graphics.DrawString(@"件号/ Part NO.", printFont3, printColor, 70, pointY + 78);
                e.Graphics.DrawString(@"导程/ Lead", printFont3, printColor, 70, pointY + 93);
                e.Graphics.DrawString(@"送检/ Producer", printFont3, printColor, 70, pointY + 108);

                e.Graphics.DrawString(@"编号/ Serial NO.", printFont3, printColor, 530, pointY + 78);
                e.Graphics.DrawString(@"螺纹全长/ Useful travel", printFont3, printColor, 530, pointY + 93);
                e.Graphics.DrawString(@"节径/ Pitch ciecle diameter", printFont3, printColor, 530, pointY + 108);

                e.Graphics.DrawString(@"测量项目/ Measuring  item", printFont3, printColor, 150, pointY + 123);
                e.Graphics.DrawString(@"要求(um)/ Permissible value", printFont3, printColor, 530, pointY + 123);
                e.Graphics.DrawString(@"实测(um)/ Actual value", printFont3, printColor, 780, pointY + 123);
                e.Graphics.DrawString(@"行程补偿值C/ Travel  compenstaion", printFont3, printColor, 150, pointY + 138);
                e.Graphics.DrawString(@"行程偏差E/ Actual mean travel devlation", printFont3, printColor, 150, pointY + 153);
                e.Graphics.DrawString(@"行程变动V", printFont3, printColor, 35, pointY + 173);
                e.Graphics.DrawString(@"Travel variation", printFont3, printColor, 15, pointY + 188);
                e.Graphics.DrawString(@"V2π/ Travel variation in random 1 revolution 2πrad", printFont3, printColor, 150, pointY + 168);
                e.Graphics.DrawString(@"V300/ Travel variation in random 300m", printFont3, printColor, 150, pointY + 183);
                e.Graphics.DrawString(@"Vu/ Travel variation in useful travel", printFont3, printColor, 150, pointY + 198);
                e.Graphics.DrawString(@"滚到半径误差/ Error of ball track radius", printFont3, printColor, 40, pointY + 213);
                e.Graphics.DrawString(@"滚到法相截面形状误差/ Shape error of ball track normal cross-section", printFont3, printColor, 40, pointY + 228);
                e.Graphics.DrawString(@"螺纹节径尺寸变动量/ Pitch circle diameter variation", printFont3, printColor, 40, pointY + 243);
                e.Graphics.DrawString(@"螺纹滚到面对轴线径向圆跳动/ Variation of ball track section", printFont3, printColor, 40, pointY + 258);
                e.Graphics.DrawString(@"结论/ Conclusion", printFont3, printColor, 100, pointY + 273);
                e.Graphics.DrawString(@"审核/ Additor", printFont3, printColor, 100, pointY + 288);
                e.Graphics.DrawString(F2.F2_BL[11], printFont3, printColor, 375, pointY + 288);
                e.Graphics.DrawString(@"检验/ Inspector：", printFont3, printColor, 600, pointY + 288);
                e.Graphics.DrawString(F2.F2_BL[12], printFont3, printColor, 875, pointY + 288);
                e.Graphics.DrawString(F2.F2_BL[1], printFont3, printColor, 275, pointY + 78);
                e.Graphics.DrawString(F2.F2_BL[5], printFont3, printColor, 275, pointY + 93);
                e.Graphics.DrawString(F2.F2_BL[2], printFont3, printColor, 275, pointY + 108);
                e.Graphics.DrawString(F2.F2_BL[12].ToString(), printFont3, printColor, 685, pointY + 78);
                e.Graphics.DrawString(F2.F2_BL[8], printFont3, printColor, 685, pointY + 93);
                e.Graphics.DrawString(F2.F2_BL[6], printFont3, printColor, 685, pointY + 108);

                e.Graphics.DrawString(@"0", printFont3, printColor, 620, pointY + 138);
                e.Graphics.DrawString(F2.F2_BL[13], printFont3, printColor, 620, pointY + 153);//Ep
                e.Graphics.DrawString(F2.F2_BL[14], printFont3, printColor, 620, pointY + 168);//V2pai
                e.Graphics.DrawString(F2.F2_BL[15], printFont3, printColor, 620, pointY + 183);//V300
                e.Graphics.DrawString(F2.F2_BL[16], printFont3, printColor, 620, pointY + 198);//Vup

                e.Graphics.DrawString(Ep.ToString("f3"), printFont3, printColor, 870, pointY + 153);
                e.Graphics.DrawString(V2π.ToString("f3"), printFont3, printColor, 870, pointY + 168);
                e.Graphics.DrawString(V300.ToString("f3"), printFont3, printColor, 870, pointY + 183);
                e.Graphics.DrawString(Vup.ToString("f3"), printFont3, printColor, 870, pointY + 198);

                string S0 = "丝杠温度/ Ball-Serew Temperature： " + "℃" + " 空气温度/ Air Temperature：" + "℃" + " 光栅温度/ Machine Temperature：" + "℃";
                e.Graphics.DrawString(S0, printFont4, printColor, 20, pointY + 725);
                string S1 = "日期/ Data:" + DateTime.Now.ToString() + " 测量转速/ Speed：" + comboBox1.Text + " rap/min";
                e.Graphics.DrawString(S1, printFont4, printColor, 650, pointY + 710);
                string S2 = "国标 / Standard：GB /T 17587.1/2/3-2002 ISO 3408-1/2/3-2006";
                e.Graphics.DrawString(S2, printFont4, printColor, 650, pointY + 725);
                //加载折线图
                Image image = Image.FromFile(path_image);
                e.Graphics.DrawImage(image, 30, 330, image.Width * 0.7f, image.Height * 0.85f);
                e.HasMorePages = false;
            }
        }
        private void F2_Show()
        {
            Form sgcs = new F2();
            if (sgcs.ShowDialog() == DialogResult.OK)
            {
                comboBox5.SelectedIndex = Convert.ToInt32(F2.F2_BL[4]);//旋向
                textBox8.Text = F2.F2_BL[5].ToString();
                Daocheng = Convert.ToDouble(F2.F2_BL[5]);
                Changdu = Convert.ToDouble(F2.F2_BL[8]);
                LeiXing = Convert.ToInt32(F2.F2_BL[3]);
            }
        }
        private void DianJi_Start()
        {
            double vel=0;
            int ZYX = comboBox4.SelectedIndex;//左旋=0，右旋=1                                      
            int CLFX = comboBox5.SelectedIndex;//正向=0，反向=1
            long Pos = 1000000000;
            if (((ZYX == 1) && (CLFX == 0)) || ((ZYX == 0) && (CLFX == 1)))
            {
                Pos = -1 * Pos;
            }
            if(LingWei==false)
            {
                vel = 20*CDB;
            }
            else
            {
                vel = Convert.ToDouble(comboBox1.Text) * CDB;
            }

            GT400_Start(Pos, vel, 0.01);
        }
        private void HuiLing_Start()
        {
            DianJi_Start();
            //寻找光栅绝对位置
            bool mark = false;
            double value1, value2, Countervalue = 0;
            IK220_Ini();
            IK220ResetRef(1);
            IK220StartRef(1);
            IK220LatchRef(1);
            while (mark == false)//检测光栅尺标记
            {
                IK220Latched(1, 2, ref mark);
            }
            IK220Get48(1, 2, ref Countervalue);//记录脉冲1
            value1 = Countervalue;
            IK220LatchRef(1);
            mark = false;
            Countervalue = 0;
            while (mark == false)//检测光栅尺标记
            {
                IK220Latched(1, 2, ref mark);
            }
            IK220_Readreset();
            IK220_Readstart();//启动计数器     
            IK220Get48(1, 2, ref Countervalue);//记录脉冲2
            value2 = Countervalue;
            double Mrr = Math.Abs(value2 - value1);
            double R = 2 * Mrr - 1000;
            if (comboBox4.SelectedIndex == 0)//正向测量
            {
                JDWZ = 0.02 * ((Math.Abs(R) - Math.Sign(R) - 1) * 500 + (Math.Sign(R) - 1) * Mrr / 2 + Mrr);
            }
            else
            {
                JDWZ = 0.02 * ((Math.Abs(R) - Math.Sign(R) - 1) * 500 + (Math.Sign(R) - 1) * Mrr / 2 - Mrr);
            }
        }
        private void Stop_CL()//停止测量
        {
            CL_start = false;
            GT400_Stop();
        }
        public void IK220_Ini()//初始化
        {
            IK220_Readstop();
            for (int i = 0; i < 8; i++)
            {
                IKCard[i] = 0;
            }
        BiaoQian1:
            if (!IK220Find(IKCard))//检查是否有轴
            {
                DialogResult dr = MessageBox.Show("轴信号连接失败！", "提示", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    goto BiaoQian1;
                }
                else
                {
                    System. Environment.Exit(0);
                }
            }
            else
            {
                for (int Ax = 0; Ax < 8; Ax++)
                {
                    m_Active[Ax] = false;
                    OldSta[Ax] = 0xFFFF;
                    m_SignalPeriod[Ax] = 0.020;                     // 默认信号周期:20
                    m_EncoderType[Ax] = 0;                          // 默认编码器类型:增量式
                    m_SignalType[Ax] = 1;                           // 默认信号类型:1Vss
                }

                for (UInt16 Ax = 0; Ax < 8; Ax++)
                {
                    if (IKCard[Ax] != 0)
                    {
                        IK220Init(Ax);
                        if (IK220WritePar(Ax, 1, m_EncoderType[Ax]) == false) MessageBox.Show("编码器类型设置出错");
                        if (IK220WritePar(Ax, 2, m_SignalType[Ax]) == false) MessageBox.Show("信号周期设置出错");
                        m_Active[Ax] = true;
                    }
                }
            }

        }
        public void IK220_Readstart()
        {
            for (UInt16 Ax = 0; Ax < 2; Ax++)
            {
                if (IKCard[Ax] != 0)
                {
                    if (m_Active[Ax])
                    {
                        IK220Start(Ax);
                    }
                }
            }
        }
        public void IK220_Readreset()
        {
            for (UInt16 Ax = 0; Ax < 2; Ax++)
            {
                if (IKCard[Ax] != 0)
                {
                    if (m_Active[Ax])
                    {
                        IK220Reset(Ax);
                    }
                }
            }
        }
        public void IK220_Readstop()
        {
            for (UInt16 Ax = 0; Ax < 2; Ax++)
            {
                if (IKCard[Ax] != 0)
                {
                    if (m_Active[Ax])
                    {
                        IK220Stop(Ax);
                    }
                }
            }
        }
        public void IK220_ReadValue()
        {
            Double wert_Z = 0, wert_C = 0;
            if ((IKCard[0] != 0) && (IKCard[1] != 0))
            {
                if (IK220Read48(1, 0, ref wert_Z))
                {
                    Celiang_Z = wert_Z * m_SignalPeriod[1] + JDWZ;
                }
                if (IK220Read48(0, 0, ref wert_C))
                {
                    Celiang_C = wert_C * 360 / 1200;
                }
            }
        }
        private void GT400_Initial()
        {
            rtn = GT_Open();     //打开运动控制器
            rtn = GT_Reset();      //运动控制器复位
            rtn = GT_SwitchtoCardNo(0);      //用于切换当前卡
            rtn = GT_SetSmplTm(200);     //设置运动控制器伺服周期(默认200微妙：48-1966)
            rtn = GT_Axis(1); 
            rtn = GT_SetIntrMsk(0); 

            rtn = GT_Axis(1);     //指定轴为当前轴
            rtn = GT_ClrSts();    //清除当前轴不正确状态
            rtn = GT_CtrlMode(1);
            rtn = GT_StepPulse();      //将脉冲输出的方式设置为“正负脉冲”方式
            rtn = GT_LmtsOff();     //关闭限位开关
            rtn = GT_AxisOn();     //使当前轴处于控制状态，并使当前轴驱动器使能
        }
        private void GT400_Start(long position, double vel, Double acc)
        {
            GT400_Initial();
            rtn = GT_ZeroPos();//设置零位
            rtn = GT_PrflT();//设置当前轴的运动模式为梯形曲线模式
            rtn = GT_SetAcc(acc);//设置当前轴的加速度0~16384
            rtn = GT_SetVel(vel);//设置当前轴的目标速度0~16384
            rtn = GT_SetPos(position);//设置当前轴的目标位置为 –1073741824～1073741823
            rtn = GT_Update();//参数刷新（参数生效）
        }
        private void GT400_Stop()
        {
            rtn = GT_SmthStp();//按照设定的加速度参数减速停止当前轴的运动
            rtn = GT_AxisOff();//使当前轴处于非控制状态并关闭轴驱动使能
            rtn = GT_Close();//关闭运动控制器设备
        }
        private void GT400_ExInpt()//检测轴状态
        {
            ushort data = 0;
            GT_ExInpt(ref data);
            if ((data & 0x8000) == 0x8000)
            {
                Stop_CL();
                MessageBox.Show("本方向(正)到达极限位置......");
            }
            if ((data & 0x4000) == 0x4000)
            {
                Stop_CL();
                MessageBox.Show("本方向(负)到达极限位置......");
            }
            if ((data & 0x1) == 0x1)
            {
                Stop_CL();
                MessageBox.Show("紧急停止");
            }
        }

    }
}
