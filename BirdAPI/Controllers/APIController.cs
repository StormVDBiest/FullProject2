using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using BirdAPI.Models;
using Azure.Storage.Blobs;
using System.Text;
using Azure.Storage.Blobs.Models;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BirdAPI.Controllers
{
    public class APIController : ApiController
    {
        public static string blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=birddetectionstorage;AccountKey=plrsXLGvDZ682VmaLgWHysjlu6xPANgSfblGF4vwLd1gfNxdd9Aaxg3gCNiuPllG5jR6yY6lxP/p+AStM7hHgw==;EndpointSuffix=core.windows.net";

        public static string blobContainerRawImage = "rawimages";
        public static string blobContainerResult = "results";

        [Route("api/getjson")]
        [HttpGet]
        public List<Result> GetJson() {
            List<Result> results = new List<Result>();

            BlobContainerClient container = new BlobContainerClient(blobStorageConnectionString, blobContainerResult);

            foreach (BlobItem blobItem in container.GetBlobs())
            {
                var blob = container.GetBlobClient(blobItem.Name);
                BlobDownloadResult downloadResult = blob.DownloadContent();
                string blobContents = downloadResult.Content.ToString();

                Result result = JsonSerializer.Deserialize<Result>(blobContents);

                results.Add(result);
            }

           
            return results;
        }
    }
}
