using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEST.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string a = "a";
            //string b = "b";
            //b = a;
            //a = "aa";

           // byte[] state=new byte[16];
           // byte[] data = new byte[] { 0x02, 0x30, 0x30, 0x30, 0x33, 0x03, 0x43, 0x36 };

           //// byte[] group_data = new byte[] {  0x30 ,0x30,  0x30 , 0x33};

           // int i = 0;
           // byte[] group_data = new byte[4] { data[i * 4 + 1 + 2], data[i * 4 + 1 + 3], data[i * 4 + 1 + 0], data[i * 4 + 1 + 1] };

           // string bitstring = new string(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());

           // Console.WriteLine(bitstring);

           // for (int j = 0; j < bitstring.Length; j++)
           // {
           //     state[j] = bitstring[j].Equals((char)49) ? (byte)1 : (byte)0;//Convert.ToByte(Convert.ToString(bitstring[j],10));//Encoding.Default.GetBytes(bitstring[j]);//Convert.ToByte( Convert.ToInt16(bitstring[j]));
           // }
           // Console.WriteLine(state);

            string s = "";
            string prefix="";
            //for (int i = 1; i <= 960; i++) {
            //    prefix= i<10 ? "Q-C0" : "Q-C";
            //    s = s + "," + i + "#" + prefix + i;
            //}
            for (int i = 1; i <= 1156; i++)
            {
                prefix = i < 10 ? "Q-C0" : "Q-C";
                s = s + "," + i + "#X";
            }
            Console.WriteLine(s);

            Console.Read();
        }
    }
}
