using ImageProcessing.Models.Entity;
using ImageProcessing.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ImageProcessing.Models.ViewModels;
using Service.Interfaces;
using System.Drawing;
using Service.Implementation;

namespace ImageProcessing.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        public HomeController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}