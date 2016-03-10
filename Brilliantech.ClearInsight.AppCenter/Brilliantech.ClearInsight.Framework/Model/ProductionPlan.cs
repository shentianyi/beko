using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Brilliantech.ClearInsight.Framework.Model
{
    [DataContract]
    public class ProductionPlan
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Assembly { get; set; }

        [DataMember]
        public string Prod_Line { get; set; }

        [DataMember]
        public int Planned { get; set; }

        [DataMember]
        public int Produced { get; set; }

        [DataMember]
        public int Rest
        {
            get; set;
        }

        [DataMember]
        public string Status { get; set; }
    }
}
