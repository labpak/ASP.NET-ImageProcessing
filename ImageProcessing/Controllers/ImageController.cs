using Azure;
using ImageProcessing.DAL.Interfaces;
using ImageProcessing.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using ImageProcessing.Models.Enum;
using ImageProcessing.Models.Response;

namespace ImageProcessing.Controllers
{
    public class ImageController : Controller
    {
        private readonly IImageService _imageService;
        public ImageController(IImageService imageService)
        {
           _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var response = await _imageService.GetImages();
            if(response.StatusCode == Models.Enum.StatusCode.OK)
                return View(response.Data);
            return RedirectToAction("Index");
        }
           
    }
}

