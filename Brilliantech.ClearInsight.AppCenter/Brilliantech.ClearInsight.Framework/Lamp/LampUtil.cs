using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Brilliantech.ClearInsight.Framework.Lamp
{
    public class LampUtil
    {
        //[DllImport("ThridPart\\QUvc_dll.dll")]
        //public static extern void Usb_Qu_Open();
        //[DllImport("ThridPart\\QUvc_dll.dll")]
        //public static extern void Usb_Qu_Close();
        //[DllImport("ThridPart\\QUvc_dll.dll")]
        //public static unsafe extern bool Usb_Qu_write(byte Q_index, byte Q_type, byte* pQ_data);
        //[DllImport("ThridPart\\QUvc_dll.dll")]
        //public static extern int Usb_Qu_Getstate();

        [DllImport("ThridPart\\Ux64_dllc.dll")]
        public static extern void Usb_Qu_Open();
        [DllImport("ThridPart\\Ux64_dllc.dll")]
        public static extern void Usb_Qu_Close();
        [DllImport("ThridPart\\Ux64_dllc.dll")]
        public static unsafe extern bool Usb_Qu_write(byte Q_index, byte Q_type, byte* pQ_data);
        [DllImport("ThridPart\\Ux64_dllc.dll")]
        public static extern int Usb_Qu_Getstate();


        /// <summary>
        /// byte
        /// </summary>
        /// <param name="sound">from 1 to 4, 0 if off</param>
        /// <param name="blink"></param>
        /// <returns></returns>
        unsafe public static bool TurnOn(byte sound = 0, bool blink = true)
        {
            try
            {
                byte* bbb = stackalloc byte[6];

                byte CS = (byte)(blink ? 2 : 1);

                bbb[0] = CS;
                bbb[1] = CS;
                bbb[2] = CS;
                bbb[3] = CS;
                bbb[4] = CS;

                bbb[5] = sound;

                return Usb_Qu_write(0, 0, bbb);
            }
            catch (Exception e) {
                Console.Write(e.Message);
            }
            return false;
        }

        unsafe public static bool TurnOff() {

            const byte C_lampoff = 0;

            byte* bbb = stackalloc byte[6];


            bbb[0] = C_lampoff;
            bbb[1] = C_lampoff;
            bbb[2] = C_lampoff;
            bbb[3] = C_lampoff;
            bbb[4] = C_lampoff;

            bbb[5] = 0;

            return Usb_Qu_write(0, 0, bbb);
        }
    }
}
