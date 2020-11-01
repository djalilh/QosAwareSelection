using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service
{
    public class Result
    {
        public List<Candidate> TopCandidates { get; set; }
        public long ExecutionTime { get; set; }
    }
}