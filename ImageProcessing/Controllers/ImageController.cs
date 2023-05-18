using ImageProcessing.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Drawing;

namespace ImageProcessing.Controllers
{
    public class ImageController : Controller
    {
        private readonly IImageService _imageService;
        private readonly IImageProcessingService _imageProcessingService;
        public ImageController(IImageService imageService, IImageProcessingService imageProcessingService)
        {
            _imageService = imageService;
            _imageProcessingService = imageProcessingService;
        }


        [HttpGet]
        public async Task<IActionResult> GetProcessingImage(int id)
        {
            if (id == 0)//новый объект добавляем
                return PartialView();

            var response = await _imageService.GetImage(id);
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                return PartialView(response.Data);

            ModelState.AddModelError("", response.Description);
            return PartialView();
        }
        
        [HttpPost]
        public async Task<IActionResult> GetProcessingImage(ImageViewModel model)
        {

            byte[] imageData;
            using (var binaryReader = new BinaryReader(model.formFile.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)model.formFile.Length);
            }

            Bitmap bitmap = _imageProcessingService.FinalImage(new Bitmap(new MemoryStream(imageData)));
            ImageConverter converter = new ImageConverter();
            await _imageService.CreateImage(model, (byte[])converter.ConvertTo(bitmap, typeof(byte[]))); 

            return RedirectToAction("GetImages");

        }


        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var response = await _imageService.GetImages();
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                return View(response.Data);
            return View("Error", $"{response.Description}");
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var response = await _imageService.GetImage(id);
            if (response.StatusCode.Equals(Models.Enum.StatusCode.OK))
                return View(response.Data);
            return RedirectToAction("Error");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _imageService.DeleteImage(id);
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                return RedirectToAction("GetImages");
            return RedirectToAction("Error");
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(int id)
        {
            if (id == 0)//новый объект добавляем
                return PartialView();

            var response = await _imageService.GetImage(id);
            if (response.StatusCode == Models.Enum.StatusCode.OK)
                return PartialView(response.Data);

            ModelState.AddModelError("", response.Description);
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Save(ImageViewModel model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("DateCreate");

            if (model.Id == 0)
            {
                byte[] imageData;
                using (var binaryReader = new BinaryReader(model.formFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)model.formFile.Length);
                }
                await _imageService.CreateImage(model, imageData);
            }
            else
            {
                await _imageService.Edit(model.Id, model);
            }


            return RedirectToAction("GetImages");
        }
    }
}

