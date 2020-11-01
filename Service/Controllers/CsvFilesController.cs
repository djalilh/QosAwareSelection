using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Service.Controllers
{
    [EnableCors("*", "*", "*")]
    public class CsvFilesController : ApiController
    {
        public static string FileFolderPath = @"D:\CoreProject\QosAwareSelection\Service\Files\";
        public string FileOnePath = FileFolderPath + "FileOne.csv";
        public string FileTwoPath = FileFolderPath + "FileTwo.csv";
        public string FileThreePath = FileFolderPath + "FileThree.csv";
        public string CloudFile = FileFolderPath + "CloudFile.csv";

        // GET api/csvfiles
        public List<Service> Get(string F1Parm, string F2Parm, string F3Parm)
        {
            var Parm1 = F1Parm.Split(',');
            var Parm2 = F2Parm.Split(',');
            var Parm3 = F3Parm.Split(',');


            GenerateFile( Convert.ToInt32(Parm1[0]), Convert.ToInt32(Parm1[1]), Convert.ToInt32(Parm1[2]), FileOnePath);
            GenerateFile(Convert.ToInt32(Parm2[0]), Convert.ToInt32(Parm2[1]), Convert.ToInt32(Parm2[2]), FileTwoPath);
            GenerateFile(Convert.ToInt32(Parm3[0]), Convert.ToInt32(Parm3[1]), Convert.ToInt32(Parm3[2]), FileThreePath);
            GenerateCloudsComunicationTime(10);
            return new List<Service> { };
        }

        // GET api/csvfiles?File=1
        public List<string> Get(int File)
        {
            string FilePath = FileOnePath;
            if (File == 1) FilePath = FileOnePath;
            if (File == 2) FilePath = FileTwoPath;
            if (File == 3) FilePath = FileThreePath;
            List<string> result = new List<string>();
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(FilePath))
                {
                    List<Service> list = new List<Service>();
                    while (!file.EndOfStream)
                    {
                        var ligne = file.ReadLine();
                        result.Add(ligne);
                    }
                   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Ocurred " + e);
            }
            return result;
        }

        #region Private Methods

        private void GenerateFile(int ServiceType,int ServiceCount, int CloudType, string FilePath)
        {
            Random radomGenerator = new Random();
            List<Cloud> Clouds = new List<Cloud>();
            for (int i = 0; i < CloudType; i++)
            {
                Clouds.Add(new Cloud()
                {
                    CloudID = i, 
                    Name = "Cloud " + i
                });
            }

            List<Service> Services = new List<Service>();
            for(int i = 0; i < ServiceType; i++)
            {
                Services.Add(new Service() { 
                    ServiceID = i,
                    Name = "Service " + i,
                });
            }

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(FilePath, false))
                {
                    for(int i = 0; i < ServiceCount; i++)
                    {
                        int CloudIndex = radomGenerator.Next(0, CloudType);
                        int ServiceIndex = radomGenerator.Next(0, ServiceType);


                        Service service = new Service()
                        {
                            ServiceID = i,
                            Name = Services.FirstOrDefault(s => s.ServiceID == ServiceIndex).Name,
                            Cloud = Clouds.FirstOrDefault(c => c.CloudID == CloudIndex),
                            ResponceTime = radomGenerator.Next(0, 1000),
                            Cost = radomGenerator.Next(0, 1000),
                            Reliability = radomGenerator.Next(0, 100),
                            Availability = radomGenerator.Next(0, 100)
                        };
                        file.WriteLine(service.ToCsvItem());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured : " + e);
            }
        }

        private void GenerateCloudsComunicationTime(int CloudType)
        {
            List<Cloud> Clouds = new List<Cloud>();
            for (int i = 0; i < CloudType; i++)
            {
                Clouds.Add(new Cloud()
                {
                    CloudID = i,
                    Name = "Cloud " + i
                });
            }

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(CloudFile, false))
                {
                    int CT = 0;
                    Random random = new Random();
                    Clouds.ForEach((cloud) => {
                        Clouds.ForEach((item) => {
                            if (cloud.CloudID == item.CloudID) {
                                CT = 0;
                            }
                            else
                            {
                                CT = random.Next(0, 1000);
                            }
                            file.WriteLine(cloud.CloudID + "," + cloud.Name + "," + item.CloudID + "," + item.Name + ","+CT);
                        });
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured : " + e);
            }


        }

        #endregion
    }
}
