using BirdMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                return View("Panel");
            }
            return View();
        }

        public ActionResult Panel() 
        {
            if (Session["isAdmin"] != null &&(bool)Session["isAdmin"] == true)
            {
                return View();
            }
            else
            {
                return Redirect("Login");
            }
            
        }
    }
}