using BirdMVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;

namespace BirdMVC.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginForm loginForm)
        {
            if (loginForm.Email == "b@b.be" && loginForm.Password == "KAGII")
            {
                Session["isAdmin"] = true;
                return Redirect("Panel");
            }
            return View();
        }

        public ActionResult Panel() 
        
        {
            if (Session["isAdmin"] != null &&(bool)Session["isAdmin"] == true)
            {
                AdminClass adminClass = new AdminClass();

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

                    adminClass.Results = results;
                    adminClass.ResultCount = results.Count;
                    return View(adminClass);
                }


                
            }
            else
            {
                return Redirect("Login");
            }
            
        }
        public ActionResult List()
        {
            if (Session["isAdmin"] != null && (bool)Session["isAdmin"] == true)
            {
                AdminClass adminClass = new AdminClass();

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

                    adminClass.Results = results;
                    adminClass.ResultCount = results.Count;
                    return View(adminClass);
                }
            }
            else
            {
                return Redirect("Login");
            }
        }

        public ActionResult Delete (Guid guid)
        {
            if (Session["isAdmin"] != null && (bool)Session["isAdmin"] == true)
            {
                string baseAddress = ConfigurationManager.AppSettings["APIUrl"];
                List<Result> results = new List<Result>();

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseAddress);

                    client.DefaultRequestHeaders.Accept.Add(
                                           new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync($"api/deleteresult?guid={guid}").Result;

                    return Redirect("List");
                }
            }
            else
            {
                return Redirect("Login");
            }
        }

        public ActionResult? Details(Guid guid)
        {
            if (Session["isAdmin"] != null && (bool)Session["isAdmin"] == true)
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
            else
            {
                return Redirect("Login");
            }
        }
    }
}