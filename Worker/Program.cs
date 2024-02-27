using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Net.WebRequestMethods;
using Azure.Storage.Blobs;
using System.Text;
using Azure.Storage.Blobs.Models;
using System.Text.Json;
using Predict = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System.Security.Policy;

namespace Worker
{
    internal class Program
    {
        public static string blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=birddetectionstorage;AccountKey=plrsXLGvDZ682VmaLgWHysjlu6xPANgSfblGF4vwLd1gfNxdd9Aaxg3gCNiuPllG5jR6yY6lxP/p+AStM7hHgw==;EndpointSuffix=core.windows.net";
        
        public static string blobContainerRawImage = "rawimages";
        public static string blobContainerResult = "results";

        public static string trainingEndpoint = "https://birddetectionai.cognitiveservices.azure.com/";
        public static string trainingKey = "7eb92455c73b4f268a6890ae2f07a8b0";

        public static string projectName = "stormBirdFeeder3";

        public static string predictionEndpoint = "https://birddetectionai-prediction.cognitiveservices.azure.com/";
        public static string predictionKey = "48c100aca3294956ae35ba67f5ae9f33";
        public static string predictionResourceId = "/subscriptions/c0aa556b-f65a-4ea4-9ec8-0ed460de5436/resourceGroups/BirdDetectionVisionAI/providers/Microsoft.CognitiveServices/accounts/BirdDetectionAI-Prediction";

        public static string watchedFolder = "C:\\Users\\storm\\Desktop\\FullProject\\FullProject\\Worker\\Watched\\";

        private static string publishedModelName = "Iteration1";

        static void Main(string[] args)
        {
            FileWatcher(watchedFolder);
            Console.ReadLine();
        }

        public static CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey)
        {
            // Create the Api, passing in the training key
            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
            return trainingApi;
            
        }

        public static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            // Create a prediction endpoint, passing in the obtained prediction key
            CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
            return predictionApi;
        }

        private static Project AssertProject(CustomVisionTrainingClient trainingApi)
        {
            // Create a new project
            Console.WriteLine($"Searching for existing project named: {projectName}");
            List<Project> projects = trainingApi.GetProjects().ToList();

            if (projects?.Count != null && projects.Count > 0)
            {
                foreach (Project item in projects)
                {
                    if (item.Name == projectName)
                    {
                        Console.WriteLine($"Existing project found!\n");
                        return item;
                    }
                }
            }

            Console.WriteLine($"Project does not exist!\nCreating new project named: {projectName}\n");
            return trainingApi.CreateProject(projectName);
        }

        public static void FileWatcher(string folderToWatch)
        {
            using var watcher = new FileSystemWatcher(folderToWatch);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Created += OnCreated;

            watcher.Filter = "*.jpg";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);

            WriteImageToBlob(e.FullPath);
        }

        public static string WriteImageToBlob(string path)
        {
            Console.WriteLine("Starting...");

            Guid uniqueID = Guid.NewGuid();

            var container = new BlobContainerClient(blobStorageConnectionString, blobContainerRawImage);
            var blob = container.GetBlobClient(uniqueID.ToString());
            var result = blob.Upload(path);


            string imageLink = blob.Uri.ToString();


            Console.WriteLine("File upload complete");

            Console.WriteLine("Making a prediction:");

            CustomVisionTrainingClient trainingApi = AuthenticateTraining(trainingEndpoint, trainingKey);
            CustomVisionPredictionClient predictionApi = AuthenticatePrediction(predictionEndpoint, predictionKey);
            Project project = AssertProject(trainingApi);

            Predict.ImageUrl predictURL = new Predict.ImageUrl(imageLink);

            Predict.ImagePrediction predict = predictionApi.ClassifyImageUrl(project.Id, publishedModelName, predictURL);

            foreach (var item in predict.Predictions)
            {
                Console.WriteLine(item.TagName + ":" + item.Probability + "%");
            }

            string jsonString = JsonSerializer.Serialize(predict);

            WriteToBlob(jsonString);

            return imageLink;
        }
        
        private static void TestIteration(CustomVisionPredictionClient predictionApi, Project project, string URL)
        {
            Console.WriteLine("Making a prediction:");

            Predict.ImageUrl predictURL = new Predict.ImageUrl(URL);

            Predict.ImagePrediction predict = predictionApi.ClassifyImageUrl(project.Id, publishedModelName, predictURL);

            foreach (var item in predict.Predictions)
            {
                Console.WriteLine(item.TagName + ":" + item.Probability + "%");
            }

            string jsonString = JsonSerializer.Serialize(predict);

            WriteToBlob(jsonString);
        }

        public static void WriteToBlob(string path)
        {
            Console.WriteLine("Starting...");

            Guid uniqueID = Guid.NewGuid();

            var container = new BlobContainerClient(blobStorageConnectionString, blobContainerResult);
            var blob = container.GetBlobClient(uniqueID.ToString());
            //var result = blob.Upload(path);

            var content = Encoding.UTF8.GetBytes(path);
            using (var ms = new MemoryStream(content))
                blob.Upload(ms);
            string imageLink = blob.Uri.ToString();

            /*var blobs = container.GetBlobs();
            foreach (var item in blobs)
            {
                Console.WriteLine(item.Name);
            }*/


            BlobDownloadResult downloadResult = blob.DownloadContent();
            string blobContents = downloadResult.Content.ToString();

            Console.WriteLine("File upload complete");
        }
    }
    public class UploadJSON
    {

    }
}
