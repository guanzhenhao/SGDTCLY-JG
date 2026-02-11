using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 丝杠动态测量仪
{
    public class JD_T_Value
    {

        public double Value_V2π, Value_V25, Value_V100, Value_V300, Value_Vup;
        public void Jd_data(double Changdu, int jddj)
        {
            Value_V2π = 0;
            Value_V25 = 0;
            Value_V100 = 0;
            Value_V300 = 0;
            Value_Vup = 0;
            if (jddj == 0)//精度等级3
            {
                Value_V2π = 0.9;
                Value_V25 = 1.2;
                Value_V100 = 1.8;
                Value_V300 = 2.5;
                if (Changdu > 0 && Changdu <= 1000) Value_Vup = 4;
            }

            if (jddj == 1)//精度等级4
            {
                Value_V2π = 1.5;
                Value_V25 = 2;
                Value_V100 = 3;
                Value_V300 = 4;
                if (Changdu > 0 && Changdu <= 1000) Value_Vup = 6;
                if (Changdu > 1000 && Changdu <= 2000) Value_Vup = 8;
                if (Changdu > 2000 && Changdu <= 3000) Value_Vup = 12;
            }

            if (jddj == 2)//精度等级5
            {
                Value_V2π = 2.5;
                Value_V25 = 3.5;
                Value_V100 = 4.5;
                Value_V300 = 6.5;
                if (Changdu > 0 && Changdu <= 1000) Value_Vup = 10;
                if (Changdu > 1000 && Changdu <= 2000) Value_Vup = 12;
                if (Changdu > 2000 && Changdu <= 3000) Value_Vup = 19;
            }

            if (jddj == 3)//精度等级6
            {
                Value_V2π = 4;
                Value_V25 = 7;
                Value_V100 = 8;
                Value_V300 = 11;
                if (Changdu > 0 && Changdu <= 1000) Value_Vup = 16;
                if (Changdu > 1000 && Changdu <= 2000) Value_Vup = 21;
                if (Changdu > 2000 && Changdu <= 3000) Value_Vup = 27;
                if (Changdu > 3000 && Changdu <= 4000) Value_Vup = 33;
                if (Changdu > 4000 && Changdu <= 5000) Value_Vup = 39;
            }


        }
    }
}
