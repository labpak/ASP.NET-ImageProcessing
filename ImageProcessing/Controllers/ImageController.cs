using Azure;
using ImageProcessing.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using ImageProcessing.Models.Enum;
using ImageProcessing.Models.Response;
using Microsoft.AspNetCore.Authorization;
using ImageProcessing.Models.ViewModels;

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
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                return View(response.Data.ToList());
            return RedirectToAction("Error");
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var response = await _imageService.GetImage(id);
            if (response.StatusCode.Equals(Models.Enum.StatusCode.OK))
                return View(response.Data);
            return RedirectToAction("Error");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _imageService.DeleteImage(id);
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                RedirectToAction("GetImages");
            return RedirectToAction("Error");
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(int id)
        {

            ModelState.Remove("DateCreate");
            if (id == 0)//новый объект добавляем
                return PartialView();

            var response = await _imageService.GetImage(id);
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                return View(response.Data);

            return RedirectToAction("Error");
        }
        [HttpPost]
        public async Task<IActionResult> Save(ImageViewModel model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("DateCreate");
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    byte[] imageData;
                    using (var binaryReader = new BinaryReader(model.formImage.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)model.Image.Length);
                    }
                    await _imageService.CreateImage(model, imageData);
                }
                else
                {
                    await _imageService.Edit(model.Id, model);
                }
            }
            return RedirectToAction("GetImages");
        }
    }
}

