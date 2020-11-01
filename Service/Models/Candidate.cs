using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service
{
    public class Candidate
    {
        public int CandidateID { get; set; }
        public string Name { get; set; }
        public List<Service> Combination { get; set; }
        public int TCT { get; set; }
        public int RTT { get; set; }
        public int CT { get; set; }
        public int Availability { get; set; }
        public int Reliability { get; set; }
        public double Statisfaction { get; set; }
        public bool Expand { get; set; }



        public void CalculateQosTotal()
        {
            TCT = 0; RTT = 0; CT = 0; Availability = 0; Reliability = 0; Expand = false;
            Combination.ForEach((service) => {
                Name += service.Name + "(" + service.Cloud.Name + ") ";
            });
            CalculateQos();
            CaluculateTCT();
        }

        private void CalculateQos()
        {
            Combination.ForEach((service) => {
                RTT = RTT + service.ResponceTime;
                CT = CT + service.Cost;
                Availability = (Availability + service.Availability) / 2;
                Reliability = (Reliability + service.Reliability) / 2;
            });
        }

        private void CaluculateTCT()
        {
            string FileFolderPath = @"D:\CoreProject\QosAwareSelection\Service\Files\";
            string CloudFile = FileFolderPath + "CloudFile.csv";
            List<CloudCT> _CloudCT = new List<CloudCT>();
            using (System.IO.StreamReader file = new System.IO.StreamReader(CloudFile))
            {
                while (!file.EndOfStream)
                {
                    var ligne = file.ReadLine();
                    var values = ligne.Split(',');

                    _CloudCT.Add(new CloudCT() { 
                        CloudOne = new Cloud() { CloudID = Convert.ToInt32(values[0]), Name = values[1] },
                        CloudTwo = new Cloud() { CloudID = Convert.ToInt32(values[2]), Name = values[3] },
                        CT = Convert.ToInt32(values[4])
                    });
                }
            }

            Combination.ForEach((One) => {
                Combination.ForEach((Two) => {
                    TCT = TCT + _CloudCT.FirstOrDefault(cloudCT => cloudCT.CloudOne.CloudID == One.Cloud.CloudID && cloudCT.CloudTwo.CloudID == Two.Cloud.CloudID).CT;
                });
            });

        }

        public void CalculateSatisfaction()
        {
            double somme = 0;
            Combination.ForEach((service) => {
                somme += service.ServiceSatisfaction;
            });

            Statisfaction = (double)somme / (double)Combination.Count();
        }
        
    }

    public class CloudCT 
    {
        public Cloud CloudOne { get; set; }
        public Cloud CloudTwo { get; set; }
        public int CT { get; set; }
    }
}