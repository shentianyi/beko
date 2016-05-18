using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brilliantech.ClearInsight.AppCenter.PLC
{
    public class Sensor
    {

        public int Id { get; set; }
        
        public string Code { get; set; }
        public byte OnFlag { get; set; }
        public byte OffFlag { get; set; }

        public bool TrigOn { get; set; }
        public bool TrigOff { get; set; }

        public byte CurrentFlag { get; set; }

        public DateTime OnFlagTime { get; set; }
        public DateTime OffFlagTime { get; set; }
        
        public bool IsEmergency { get; set; }

        public Sensor ChangeTo(byte flag) {
            
            if (TrigOn) {
                if (flag != this.OnFlag) {
                    if (flag == this.OffFlag && this.CurrentFlag == this.OnFlag) { 
                    }
                } 
            }

            this.CurrentFlag = flag;
            return null;
        }
    }
}
