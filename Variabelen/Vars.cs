using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Variabelen
{
    public class Vars
    {
        public static string pathToSeed = "../Images"; 

        public static string blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stormtestsand1;AccountKey=bc6y3LZ5BWNDV/5BBVPVFwZy4SjUHne7zOm2gOo37aglqSdg+P2p2JnDzb3yqQzhPYk8Ate0it6s+ASttRGaeg==;EndpointSuffix=core.windows.net";
        public static string blobStorageContainerNameUpload = "fileupload";

        public static string blobStorageContainerNameResult = "results";

        public static string trainingEndpoint = "https://birddetectionai.cognitiveservices.azure.com/";
        public static string trainingKey = "7eb92455c73b4f268a6890ae2f07a8b0";

        public static string projectName = "stormBirdFeeder";

        public static string predictionEndpoint = "https://birddetectionai-prediction.cognitiveservices.azure.com/";
        public static string predictionKey = "48c100aca3294956ae35ba67f5ae9f33";
        public static string predictionResourceId = "/subscriptions/c0aa556b-f65a-4ea4-9ec8-0ed460de5436/resourceGroups/BirdDetectionVisionAI/providers/Microsoft.CognitiveServices/accounts/BirdDetectionAI-Prediction";

        public static Iteration iteration;
        public static string publishedModelName = "Iteration1";

        public static MemoryStream testImage;
    }
}
