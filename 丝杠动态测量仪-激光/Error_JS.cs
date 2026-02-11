using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 丝杠动态测量仪
{
    
    public class Error_JS
    {
        public double A, B, Ep, V2π, V25, V100, V300, Vup, V2πmax, V25max, V100max, V300max;
        double Vn, Vnmax;
        public void Error_FenXi(List<double> X, List<double> Y,double DaoCheng)
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
        private double SJ_Ep(List<double> X, List<double> Y)//实际平均行程偏差Ep函数
        {
            double Ep;
            A_B(X, Y);
            Ep = B * (X[X.Count() - 1] - X[0]);
            return Ep;
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
