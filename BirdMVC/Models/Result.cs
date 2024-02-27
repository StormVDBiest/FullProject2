using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BirdMVC.Models
{
    public class Result
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