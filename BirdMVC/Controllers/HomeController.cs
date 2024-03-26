using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using BirdMVC.Models;

namespace BirdMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(Guid guid)
        {
            string baseAddress = ConfigurationManager.AppSettings["APIUrl"];
            Result result = new Result();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync($"api/getresult?guid={guid}").Result;

                string dataAsString = response.Content.ReadAsStringAsync().Result;

                result = JsonConvert.DeserializeObject<Result>(dataAsString);

                return View(result);
            }
        }

        public ActionResult List()
        {
            string baseAddress = ConfigurationManager.AppSettings["APIUrl"];
            List<Result> results = new List<Result>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync($"api/getallresults").Result;

                string dataAsString = response.Content.ReadAsStringAsync().Result;

                results = JsonConvert.DeserializeObject<List<Result>>(dataAsString);

                return View(results);
            }
        }

    }
}