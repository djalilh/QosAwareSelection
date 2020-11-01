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
    public class SelectionController : ApiController
    {
        public static string FileFolderPath = @"D:\CoreProject\QosAwareSelection\Service\Files\";
        public string FileOnePath = FileFolderPath + "FileOne.csv";
        public string FileTwoPath = FileFolderPath + "FileTwo.csv";
        public string FileThreePath = FileFolderPath + "FileThree.csv";
        public List<Service> Services ;
        public List<Service> UserRequest ;
        public List<Candidate> Candidates ;
        public List<List<Service>> ServiceGroupes ;

        // GET api/selection?UR=Service 0,0,10,102,60,70;Service 1,0,10,102,60,70;Service 2,0,10,102,60,70
        public List<Result> Get(string UR)
        {
            InitUserRequest(UR);
            List<Result> Results = new List<Result>()
            {
                Selection(FileOnePath, 3, false),
                Selection(FileOnePath, 3, true),
                Selection(FileTwoPath, 3, false),
                Selection(FileTwoPath, 3, true),
                Selection(FileThreePath, 3, false),
                Selection(FileThreePath, 3, true),
            };  
            return Results;
        }


        #region Private Methods

        private Result Selection(string FilePath, int topNumber, bool UseSkyline)
        {
            Result result;
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                SelectAllServices(FilePath);
                EliminateNoneRequestServices();
                if (UseSkyline) CalculateSatisfactions();
                if (UseSkyline) Skyline();
                GenerateServiceGroupes();
                GenerateCandidates();
                SelectTopCandidate(topNumber);
                watch.Stop();
                long ExecutionTime = watch.ElapsedMilliseconds;
                result =  new Result()
                {
                    TopCandidates = Candidates,
                    ExecutionTime = ExecutionTime
                };
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                result = new Result()
                {
                    TopCandidates = new List<Candidate>(),
                    ExecutionTime = 0
                };
            }
            return result;
        }

        private void InitUserRequest(string UR)
        {
            UserRequest = new List<Service>();
            var Items = UR.Split(';');
            foreach(var item in Items)
            {
                var values = item.Split(',');
                Service service = new Service()
                {
                    Name = values[0],
                    ResponceTime = Convert.ToInt32(values[2]),
                    Cost = Convert.ToInt32(values[3]),
                    Reliability = Convert.ToInt32(values[4]),
                    Availability = Convert.ToInt32(values[5]),
                };
                if (Convert.ToInt32(values[1]) == 0) service.SatisfactionType = SatisfactionType.Around;
                if (Convert.ToInt32(values[1]) == 1) service.SatisfactionType = SatisfactionType.Between;
                if (Convert.ToInt32(values[1]) == 2) service.SatisfactionType = SatisfactionType.Max;
                if (Convert.ToInt32(values[1]) == 3) service.SatisfactionType = SatisfactionType.Min;
                this.UserRequest.Add(service);
            }
        }

        private void SelectAllServices(string FilePath)
        {
            Services = new List<Service>();
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(FilePath))
                {
                    List<Service> list = new List<Service>();
                    while (!file.EndOfStream)
                    {
                        var ligne = file.ReadLine();
                        var values = ligne.Split(',');
                        Service service = new Service()
                        {
                            ServiceID = Convert.ToInt32(values[0]),
                            Name = values[1],
                            Cloud = new Cloud() { CloudID = Convert.ToInt32(values[2]), Name = values[3] },
                            ResponceTime = Convert.ToInt32(values[4]),
                            Cost = Convert.ToInt32(values[5]),
                            Reliability = Convert.ToInt32(values[6]),
                            Availability = Convert.ToInt32(values[7])
                        };
                        list.Add(service);
                    }
                    Services = list.OrderBy(s => s.Name).ThenBy(s => s.Cloud.Name)
                                    .ThenBy(s => s.ResponceTime)
                                    .ThenBy(s => s.Cost)
                                    .ThenByDescending(s => s.Reliability)
                                    .ThenByDescending(s => s.Availability).ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Ocurred " + e);
            }
        }

        private void EliminateNoneRequestServices()
        {
            Services.RemoveAll(s => !this.UserRequest.Select(sr => sr.Name).ToList().Contains(s.Name));
        }

        private void CalculateSatisfactions()
        {

            this.Services.ForEach((service) => {
                var requested = this.UserRequest.FirstOrDefault(ur => ur.Name == service.Name);

                int alphaRT = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.ResponceTime).Min();
                int betaRT = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.ResponceTime).Max();
                int alphaCost = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.Cost).Min();
                int betaCost = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.Cost).Max();
                int alphaAv = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.Availability).Min();
                int betaAv = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.Availability).Max();
                int alphaRb = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.Reliability).Min();
                int betaRb = this.Services.FindAll(s => s.Name == service.Name).Select(s => s.Reliability).Max();

                if (requested.SatisfactionType == SatisfactionType.Around)
                {
                    // calculate response time satisfactions for each service
                    if (service.ResponceTime <= alphaRT && service.ResponceTime >= betaRT) service.ResponceTimeSatisfaction = 0;
                    if (service.ResponceTime > alphaRT && service.ResponceTime < requested.ResponceTime) service.ResponceTimeSatisfaction = (double)(service.ResponceTime - alphaRT) / (double)(requested.ResponceTime - alphaRT);
                    if (service.ResponceTime == requested.ResponceTime) service.ResponceTimeSatisfaction = 1;
                    if (service.ResponceTime > requested.ResponceTime && service.ResponceTime < betaRT) service.ResponceTimeSatisfaction = (double)(betaRT - service.ResponceTime)/ (double)(betaRT - requested.ResponceTime);

                    // calculate Cost satisfaction for each service
                    if (service.Cost <= alphaCost && service.Cost >= betaCost) service.CostSatisfaction = 0;
                    if (service.Cost > alphaCost && service.Cost < requested.Cost) service.CostSatisfaction = (double)(service.Cost - alphaCost) / (double)(requested.Cost - alphaCost);
                    if (service.Cost == requested.Cost) service.CostSatisfaction = 1;
                    if (service.Cost > requested.Cost && service.Cost < betaCost) service.CostSatisfaction = (double)(betaCost - service.Cost) / (double)(betaCost - requested.Cost);

                    // calculate Availability satisfaction for each service
                    if (service.Availability <= alphaAv && service.Availability >= betaAv) service.AvailabilitySatisfaction = 0;
                    if (service.Availability > alphaAv && service.Availability < requested.Availability) service.AvailabilitySatisfaction = (double)(service.Availability - alphaAv) / (double)(requested.Availability - alphaAv);
                    if (service.Availability == requested.Availability) service.AvailabilitySatisfaction = 1;
                    if (service.Availability > requested.Availability && service.Availability < betaAv) service.AvailabilitySatisfaction = (double)(betaAv - service.Availability) / (double)(betaAv - requested.Availability);

                    // calculate Reliability satisfaction for each service
                    if (service.Reliability <= alphaRb && service.Reliability >= betaRb) service.ReliabilitySatisfaction = 0;
                    if (service.Reliability > alphaRb && service.Reliability < requested.Reliability) service.ReliabilitySatisfaction = (double)(service.Reliability - alphaRb) / (double)(requested.Reliability - alphaRb);
                    if (service.Reliability == requested.Reliability) service.ReliabilitySatisfaction = 1;
                    if (service.Reliability > requested.Reliability && service.Reliability < betaRb) service.ReliabilitySatisfaction = (double)(betaRb - service.Reliability) / (double)(betaRb - requested.Reliability);

                    service.CalculateSatisfaction();
                }

                if (requested.SatisfactionType == SatisfactionType.Max)
                {
                    // calculate response time satisfactions for each service
                    if (service.ResponceTime <= alphaRT) service.ResponceTimeSatisfaction = 0;
                    if (service.ResponceTime > alphaRT && service.ResponceTime < betaRT) service.ResponceTimeSatisfaction = (service.ResponceTime - alphaRT) / (betaRT - alphaRT);
                    if (service.ResponceTime <= requested.ResponceTime && service.ResponceTime >= betaRT) service.ResponceTimeSatisfaction = 1;

                    // calculate Cost satisfaction for each service
                    if (service.Cost <= alphaCost) service.CostSatisfaction = 0;
                    if (service.Cost > alphaCost && service.Cost < betaCost) service.CostSatisfaction = (service.Cost - alphaCost) / (betaCost - alphaCost);
                    if (service.Cost <= requested.Cost && service.Cost >= betaCost) service.CostSatisfaction = 1;

                    // calculate Availability satisfaction for each service
                    if (service.Availability <= alphaAv) service.AvailabilitySatisfaction = 0;
                    if (service.Availability > alphaAv && service.Availability < betaAv) service.AvailabilitySatisfaction = (service.Availability - alphaAv) / (betaAv - alphaAv);
                    if (service.Availability <= requested.Availability && service.Availability >= betaAv) service.AvailabilitySatisfaction = 1;


                    // calculate Reliability satisfaction for each service
                    if (service.Reliability <= alphaRb) service.ReliabilitySatisfaction = 0;
                    if (service.Reliability > alphaRb && service.Reliability < betaRb) service.ReliabilitySatisfaction = (service.Reliability - alphaRb) / (betaRb - alphaRb);
                    if (service.Reliability <= requested.Reliability && service.Reliability >= betaRb) service.ReliabilitySatisfaction = 1;

                    service.CalculateSatisfaction();
                }

                if (requested.SatisfactionType == SatisfactionType.Min)
                {
                    // calculate response time satisfactions for each service
                    if (service.ResponceTime >= requested.ResponceTime && service.ResponceTime <= alphaRT) service.ResponceTimeSatisfaction = 1;
                    if (service.ResponceTime > alphaRT && service.ResponceTime < betaRT) service.ResponceTimeSatisfaction = (service.ResponceTime - alphaRT) / (betaRT - alphaRT);
                    if (service.ResponceTime >= betaRT) service.ResponceTimeSatisfaction = 0;

                    // calculate Cost satisfaction for each service
                    if (service.Cost >= requested.Cost && service.Cost <= alphaCost) service.CostSatisfaction = 1;
                    if (service.Cost > alphaCost && service.Cost < betaCost) service.CostSatisfaction = (service.Cost - alphaCost) / (betaCost - alphaCost);
                    if (service.Cost >= betaCost) service.CostSatisfaction = 0;

                    // calculate Availability satisfaction for each service
                    if (service.Availability >= requested.Availability && service.Availability <= alphaAv) service.AvailabilitySatisfaction = 1;
                    if (service.Availability > alphaAv && service.Availability < betaAv) service.AvailabilitySatisfaction = (service.Availability - alphaAv) / (betaAv - alphaAv);
                    if (service.Availability >= betaAv) service.AvailabilitySatisfaction = 0;


                    // calculate Reliability satisfaction for each service
                    if (service.Reliability >= requested.Reliability && service.Reliability <= alphaRb) service.ReliabilitySatisfaction = 1;
                    if (service.Reliability > alphaRb && service.Reliability < betaRb) service.ReliabilitySatisfaction = (service.Reliability - alphaRb) / (betaRb - alphaRb);
                    if (service.Reliability >= betaRb) service.ReliabilitySatisfaction = 0;

                    service.CalculateSatisfaction();
                }


            });
        }

        private void Skyline()
        {
            this.Services = this.Services.OrderByDescending(s => s.ServiceSatisfaction).ToList();
            List<Service> FinalServiceList = new List<Service>();
            GenerateServiceGroupes();
            ServiceGroupes.ForEach((groupe) => {
                List<List<Service>> groupedByCloud = groupe.GroupBy(s => s.Cloud.Name).Select(grp => grp.ToList()).ToList();
                groupedByCloud.ForEach((g) =>
                {
                    FinalServiceList.Add(g.FirstOrDefault());
                });
            });
            this.Services = FinalServiceList;
            
        }

        private void GenerateServiceGroupes()
        {
            ServiceGroupes = new List<List<Service>>();
            UserRequest.ForEach((serviceRequested) => {
                ServiceGroupes.Add(Services.FindAll(service => service.Name == serviceRequested.Name));
            });
        }

        private void GenerateCandidates()
        {
            Candidates = new List<Candidate>();
            Candidates.Add(new Candidate() { Combination = new List<Service>() });
            int iteration = 0;
            ServiceGroupes.ForEach((groupe) => {
                List<Candidate> CandidateCreated = new List<Candidate>();
                groupe.ForEach((service) => {
                    Candidates.ForEach((candidate) =>
                    {
                        var combinison = candidate.Combination;
                        
                        Candidate newCandidate = new Candidate()
                        {
                            Combination = combinison.Concat(new List<Service>() { service}).ToList()
                        };
                        CandidateCreated.Add(newCandidate);
                    });
                });
                CandidateCreated.ForEach((candidate) =>
                {
                    this.Candidates.Add(candidate);
                });
                Candidates.RemoveAll(c => c.Combination.Count() == iteration);
                iteration++;
            });

            Candidates.RemoveAll(c => c.Combination.Count != ServiceGroupes.Count());
            Candidates.ForEach((candidate) => { candidate.CalculateQosTotal(); candidate.CalculateSatisfaction(); });
            Candidates.OrderByDescending(c => c.Statisfaction).ThenBy(c => c.TCT);
        }
 
        private void SelectTopCandidate(int topNumber)
        {
            Candidates = Candidates.GetRange(0, topNumber);
        }
       

        #endregion
    }
}
