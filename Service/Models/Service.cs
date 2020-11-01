using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service
{
    public class Service
    {
        public int ServiceID { get; set; }
        public string Name { get; set; }
        public Cloud Cloud { get; set; }
        public int ResponceTime { get; set; }
        public double ResponceTimeSatisfaction { get; set; }
        public int Cost { get; set; }
        public double CostSatisfaction { get; set; }
        public int Availability { get; set; }
        public double AvailabilitySatisfaction { get; set; }
        public int Reliability { get; set; }
        public double ReliabilitySatisfaction { get; set; }
        public double ServiceSatisfaction { get; set; }
        public SatisfactionType SatisfactionType { get; set; }

        public string ToCsvItem()
        {
            return ServiceID + "," + Name + "," + Cloud.CloudID + "," + Cloud.Name + "," + ResponceTime + "," + Cost + "," + Availability + "," + Reliability;
        }


        public void CalculateSatisfaction()
        {
            ServiceSatisfaction = (double)(ResponceTimeSatisfaction + CostSatisfaction + AvailabilitySatisfaction + ReliabilitySatisfaction) / (double) 4;
        }
        
    }

    public enum SatisfactionType : int
    {
        Around = 0,
        Between = 1,
        Max = 2,
        Min = 3
    }
}