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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Reflection.Metadata;
using System.Xml;

namespace Worker
{
    internal class Program
    {
        public static string blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=birddetectionstorage;AccountKey=plrsXLGvDZ682VmaLgWHysjlu6xPANgSfblGF4vwLd1gfNxdd9Aaxg3gCNiuPllG5jR6yY6lxP/p+AStM7hHgw==;EndpointSuffix=core.windows.net";
        public static string blobContainerRawImage = "rawimages";
        public static string blobContainerResult = "results";
        public static string blobContainerThumbnail = "thumbnails";

        public static string trainingEndpoint = "https://birddetectionai.cognitiveservices.azure.com/";
        public static string trainingKey = "7eb92455c73b4f268a6890ae2f07a8b0";

        public static string projectName = "stormBirdFeeder3";

        public static string predictionEndpoint = "https://birddetectionai-prediction.cognitiveservices.azure.com/";
        public static string predictionKey = "48c100aca3294956ae35ba67f5ae9f33";
        public static string predictionResourceId = "/subscriptions/c0aa556b-f65a-4ea4-9ec8-0ed460de5436/resourceGroups/BirdDetectionVisionAI/providers/Microsoft.CognitiveServices/accounts/BirdDetectionAI-Prediction";

        public static string watchedFolder = "C:\\Users\\Leerlingen\\Desktop\\Vision AI\\GitClone3\\Worker\\Watched\\";

        private static string publishedModelName = "Iteration1";

        public static int thumbnailHeigt = 150;
        public static int thumbnailWidth = 150;


        static void Main(string[] args)
        {
            FileWatcher(watchedFolder);
            Console.ReadLine();
        }

        // Authenticate with the Custom Vision Training client
        public static CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey)
        {
            // Create and return the training API client
            return new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
        }

        // Authenticate with the Custom Vision Prediction client
        public static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            // Create and return the prediction API client
            return new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
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

            HandleOrder(e.FullPath);
        }

        public static RawImageModel UploadRaw(string path)
        {
            RawImageModel model = new RawImageModel();
            Result R = new Result();

            Console.Write("Starting Raw Image Upload");

            Guid uniqueID = Guid.NewGuid();
            model.GUID = uniqueID;

            var containerRaw = new BlobContainerClient(blobStorageConnectionString, blobContainerRawImage);
            var blobRaw = containerRaw.GetBlobClient(uniqueID.ToString());
            var result = blobRaw.Upload(path);

            string rawImageLink = blobRaw.Uri.ToString();
            model.ImgURL = rawImageLink;

            Console.WriteLine(" ----- done");

            //R = Prediction(rawImageLink);

            //UploadJson(R, rawImageLink);

            return model;
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


        public static void UploadJson(Result R)
        {
            Console.Write("Starting Json upload");

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

            Console.WriteLine(" ----- done");
        }
        private static List<Prediction> Prediction(string imageLink)
        {
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
            
            return P;
        }
        public static ThumbnailModel ResizeAndUploadThumbnail(string inputPath, int width, int height)
        {
            Console.Write("Resizeing thumbnail");
            ThumbnailModel model = new ThumbnailModel();
            // Load the image
            using (var image = SixLabors.ImageSharp.Image.Load(inputPath))
            {
                // Resize the image to the specified size
                image.Mutate(x => x.Resize(width, height));
                Console.WriteLine(" ----- done");
                Console.Write("Uploading thumbnail");
                // Initialize the BlobServiceClient and BlobContainerClient

                Guid guid = Guid.NewGuid();
                model.GUID = guid;
                var containerThumb = new BlobContainerClient(blobStorageConnectionString, blobContainerThumbnail);
                var blobThumb = containerThumb.GetBlobClient(guid.ToString());

                // Create the container if it does not exist
                containerThumb.CreateIfNotExists();

                
                // Get a reference to a blob
                var blobClient = containerThumb.GetBlobClient(guid.ToString());

                // Convert the ImageSharp image to a stream and upload it
                using (var stream = new MemoryStream())
                {
                    image.SaveAsJpeg(stream);
                    stream.Position = 0; // Reset stream position to the beginning
                    blobClient.Upload(stream);

                    
                }
                string imageLink = blobThumb.Uri.ToString();
                model.ImgURL = imageLink;
            }
            Console.WriteLine(" ----- done");

            return model;
        }

        public static void HandleOrder(string path)
        {
            Result R = new Result();

            R.RawImage = UploadRaw(path);
            R.Thumbnail = ResizeAndUploadThumbnail(path, thumbnailWidth, thumbnailHeigt);

            R.Predictions = Prediction(R.RawImage.ImgURL);
            UploadJson(R);
        }

    }
}
