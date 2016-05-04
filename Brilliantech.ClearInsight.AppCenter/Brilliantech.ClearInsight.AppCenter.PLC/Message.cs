using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    public class Message
    {
        public byte[] Data { get; set; }
        public DateTime ReceivedTime { get; set; }
    }
}
