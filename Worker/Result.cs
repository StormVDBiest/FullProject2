using Newtonsoft.Json;
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
        public RawImageModel RawImage { get; set; }
        public List<Prediction> Predictions { get; set; } = new List<Prediction>();
    }

    public class Prediction()
    {
        public double probability { get; set; }
        public string TagName { get; set; }
    }

    public class RawImageModel()
    {
        public Guid GUID { get; set; }
        public string ImgURL { get; set; }
    }
    

}
