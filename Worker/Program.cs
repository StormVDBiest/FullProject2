using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Predict = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace Worker
{
    internal class Program
    {
        public static string blobStorageConnectionString = ConfigurationManager.AppSettings["ConnectionString"];

        public static string blobContainerRawImage = ConfigurationManager.AppSettings["ContainerNameRawImage"];
        public static string blobContainerResult = ConfigurationManager.AppSettings["ContainerNameResult"];
        public static string blobContainerThumbnail = ConfigurationManager.AppSettings["ContainerNameThumbnails"];

        public static string trainingEndpoint = ConfigurationManager.AppSettings["trainingEndpoint"];
        public static string trainingKey = ConfigurationManager.AppSettings["trainingKey"];

        public static string projectName = ConfigurationManager.AppSettings["projectName"];

        public static string predictionEndpoint = ConfigurationManager.AppSettings["predictionEndpoint"];
        public static string predictionKey = ConfigurationManager.AppSettings["predictionKey"];
        public static string predictionResourceId = ConfigurationManager.AppSettings["predictionResourceId"];

        public static string watchedFolder = ConfigurationManager.AppSettings["watchedFolder"];

        private static string publishedModelName = ConfigurationManager.AppSettings["publishedModelName"];

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

            UploadRaw(e.FullPath);

        }

        public static string UploadRaw(string path)
        {
            List<Prediction> P = new List<Prediction>();
            Result R = new Result();

            Console.WriteLine("Starting...");

            Guid uniqueID = Guid.NewGuid();

            var container = new BlobContainerClient(blobStorageConnectionString, blobContainerRawImage);
            var blob = container.GetBlobClient(uniqueID.ToString());
            var result = blob.Upload(path);

            string rawImageLink = blob.Uri.ToString();

            Console.WriteLine("File upload complete");

            R = Prediction(rawImageLink);

            UploadJson(R, rawImageLink);

            return rawImageLink;
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

        }


        public static void UploadJson(Result R, string imageLink)
        {
            RawImageModel rawImageModel = new RawImageModel();
            rawImageModel.GUID = Guid.NewGuid();
            rawImageModel.ImgURL = imageLink;

            R.RawImage = rawImageModel;

            Console.WriteLine("Starting Json upload...");

            R.GUID = Guid.NewGuid();

            var container = new BlobContainerClient(blobStorageConnectionString, blobContainerResult);
            var blob = container.GetBlobClient(R.GUID.ToString());

            string jsonString = JsonSerializer.Serialize(R);
            var content = Encoding.UTF8.GetBytes(jsonString);

            using (var ms = new MemoryStream(content))
                blob.Upload(ms);

            string uploadedLink = blob.Uri.ToString();

            BlobDownloadResult downloadResult = blob.DownloadContent();
            string blobContents = downloadResult.Content.ToString();

            Console.WriteLine("File upload complete");
        }
        private static Result Prediction(string imageLink)
        {
            Result R = new Result();
            List<Prediction> P = new List<Prediction>();

            Console.WriteLine("Making a prediction:");

            CustomVisionTrainingClient trainingApi = AuthenticateTraining(trainingEndpoint, trainingKey);
            CustomVisionPredictionClient predictionApi = AuthenticatePrediction(predictionEndpoint, predictionKey);
            Project project = AssertProject(trainingApi);

            Predict.ImageUrl predictURL = new Predict.ImageUrl(imageLink);

            Predict.ImagePrediction predict = predictionApi.ClassifyImageUrl(project.Id, publishedModelName, predictURL);

            foreach (var item in predict.Predictions)
            {
                Console.WriteLine(item.TagName + ":" + item.Probability + "%");

                double berekening = item.Probability * 100;
                int procent = (int)berekening;

                Prediction prediction = new Prediction();
                prediction.TagName = item.TagName;
                prediction.Probability = procent;

                P.Add(prediction);
            }

            R.Predictions = P;
            return R;
        }
    }
}
