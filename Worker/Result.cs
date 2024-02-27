using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worker
{
    internal class Result
    {
        public Guid GUID { get; set; }

        public List<Probability> Predictions { get; set; }


    }
    public class Probability()
    {
        public string PredictTag { get; set; }
        public float PredictValue { get; set; }
    }
}
