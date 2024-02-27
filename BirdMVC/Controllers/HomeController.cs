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
            string baseAddress = ConfigurationManager.AppSettings["APIUrl"];
            Result waardes = new Result();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync($"api/getjson").Result;

                string dataAsString = response.Content.ReadAsStringAsync().Result;

                waardes = JsonConvert.DeserializeObject<Result>(dataAsString);

                return View(waardes);
            }

        }

    }
}