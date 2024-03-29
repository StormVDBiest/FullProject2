﻿using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Net.WebRequestMethods;
using System.Configuration;

namespace Seeder
{

    internal class Program
    {
        public static string pathToSeed = ConfigurationManager.AppSettings["pathToSeed"];

        public static string trainingEndpoint = ConfigurationManager.AppSettings["trainingEndpoint"];
        public static string trainingKey = ConfigurationManager.AppSettings["trainingKey"];

        public static string projectName = ConfigurationManager.AppSettings["projectName"];


        static void Main(string[] args)
        {
            CustomVisionTrainingClient trainingApi = AuthenticateTraining(trainingEndpoint, trainingKey);

            Project project = AssertProject(trainingApi);
            UploadImages(trainingApi, project);
            Console.WriteLine("Done Uploading\nPress any key to close");
            Console.ReadLine();
        }


        private static CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey)
        {
            // Create the Api, passing in the training key
            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
            return trainingApi;
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


        private static Tag AssertTag(CustomVisionTrainingClient trainingApi, Project project, string TagToCheck)
        {
            List<Tag> allTags = trainingApi.GetTags(project.Id).ToList();
            Tag soort = new Tag();

            Console.WriteLine($"\nSearching for existing tag named {TagToCheck}");
            foreach (Tag tagCheck in allTags)
            {
                if (tagCheck.Name == TagToCheck)
                {
                    Console.WriteLine("Existing tag found!\n");
                    return tagCheck;
                }
            }
            Console.WriteLine($"Tag does not exist!\nCreating new tag named: {TagToCheck}\n");
            return trainingApi.CreateTag(project.Id, TagToCheck);
        }

        private static void UploadImages(CustomVisionTrainingClient trainingApi, Project project)
        {
            // Add some images to the tags
            Console.WriteLine("\tUploading images");

            List<string> getDir = Directory.GetDirectories(pathToSeed).ToList();

            foreach (string item in getDir)
            {
                List<string> splitDir = item.Split('/').ToList();

                Tag tag = AssertTag(trainingApi, project, splitDir[1]);

                Console.WriteLine(splitDir[1]);

                List<string> getImages = Directory.GetFiles(item).ToList();

                foreach (var image in getImages)
                {
                    List<string> splitImg = image.Split('\\').ToList();
                    Console.WriteLine(splitImg[1]);

                    using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(image)))
                    {
                        trainingApi.CreateImagesFromData(project.Id, stream, new List<Guid>() { tag.Id });
                        Thread.Sleep(5);
                    }
                }
            }
        }
    }
}
