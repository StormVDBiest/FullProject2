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
        [JsonProperty(PropertyName = "guid")]
        public Guid GUID { get; set; }

        [JsonProperty(PropertyName = "rawimage")]
        public string RawImage { get; set; }

    }
    public class Prediction()
    {
        [JsonProperty(PropertyName = "probability")]
        public double probability { get; set; }

        [JsonProperty(PropertyName = "tagname")]
        public string TagName { get; set; }

        
    }
}
