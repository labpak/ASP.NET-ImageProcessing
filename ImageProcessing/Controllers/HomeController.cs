using ImageProcessing.DAL.Interfaces;
using ImageProcessing.DAL.Repositories;
using ImageProcessing.Models.Entity;
using ImageProcessing.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        public IActionResult Privacy()
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