using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TEST.Console1
{
    class Program
    {
        const int CONTROLS = 48;
        const int RETURN_DATA_LENGTH = 16;
        const string FXType = "3U";
        const int RETURN_DATA_GROUP_LENGTH=3;

        static void Main(string[] args)
        {
            string s = "";
            for (var i = 0; i < 992; i++) {
               
                    s += (i + "#" + "X" + ",false,false,false$");
                

                //if (i < 10)
                //{
                //    s += (i + "#" + "1N-C0" + (i + 1) + ",true,true,false$");
                //}
                //else {
                //    s += (i + "#" + "1N-C" + (i + 1) + ",true,true,false$");
                //}
            }
            Console.WriteLine(s);
           // DateTime st = DateTime.Now;

           //// byte[] data=new byte[RETURN_DATA_LENGTH]{2, 70, 70, 48, 48, 48, 48, 48, 48, 48, 48, 48, 32, 3, 54, 70};

           // byte[] data = new byte[RETURN_DATA_LENGTH] { 2, 70, 70, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 3, 54, 70 };
           // byte[] state=new byte[CONTROLS];

           //state = getOnOffState(data);
           //  Thread.Sleep(1000);
           // DateTime et = DateTime.Now;
           
          
           

           // TimeSpan ts = et - st;
           // Console.WriteLine(ts.TotalMilliseconds);
           // Console.WriteLine((int)(et - st).TotalMilliseconds);
           // Console.WriteLine((int)ts.Milliseconds);

            Console.Read();
        }



        static byte[] getOnOffState(byte[] data)
        {
            byte[] state = new byte[CONTROLS];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = 0;
            }
            if (FXType == "Q02U")
            {
                for (int i = 0; i < data.Length; i += 2)
                {

                    byte[] group_data = new byte[2] { data[i + 0], data[i + 1] };

                    for (int j = 0; j < group_data.Length; j++)
                    {
                        string bitstring = new string(Convert.ToString(group_data[j], 2).Reverse().ToArray());
                        for (int m = 0; m < bitstring.Length; m++)
                        {
                            //                          char s = bitstring[m];
                            //                        bool ss = bitstring[m].Equals((char)49);
                            //                      int iii = i * 8 + j * 8 + m;
                            state[i * 8 + j * 8 + m] = bitstring[m].Equals((char)49) ? (byte)1 : (byte)0;
                        }
                    }

                    //

                    // int ii=Convert.ToInt32(hs, 16); 
                    // string bitstring = new string(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());
                    // string s = bitstring;
                }
            }
            else
            {
                for (int i = 0; i < RETURN_DATA_GROUP_LENGTH; i++)
                {
                    byte[] group_data = new byte[4] { data[i * 4 + 1 + 2], data[i * 4 + 1 + 3], data[i * 4 + 1 + 0], data[i * 4 + 1 + 1] };

                    string asciistring = ASCIIEncoding.ASCII.GetString(group_data);
                    int i16 = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16);
                    string str2 = Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2);

                    var str2revers = Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray();
                    var newbitstring = new string(str2revers);
                    string bitstring = new string(Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(group_data), 16), 2).Reverse().ToArray());

                    for (int j = 0; j < bitstring.Length; j++)
                    {
                        state[16 * i + j] = bitstring[j].Equals((char)49) ? (byte)1 : (byte)0;//Convert.ToByte(Convert.ToString(bitstring[j],10));//Encoding.Default.GetBytes(bitstring[j]);//Convert.ToByte( Convert.ToInt16(bitstring[j]));
                    }
                }
            }
            return state;
        }
    }
}
