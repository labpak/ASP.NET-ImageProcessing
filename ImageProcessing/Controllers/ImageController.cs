using ImageProcessing.DAL.Interfaces;
using ImageProcessing.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessing.Controllers
{
    public class ImageController : Controller
    {
        private readonly IImageRepository _imageRepository;
        public ImageController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var response = await _imageRepository.GetAll();

            byte[] num = { 1, 2, 3 };
            var image = new ImageP()
            {
                Name = "new",
                Description = "Тут могла быть ваша реклама",
                DateCreate = DateTime.UtcNow,
                TypeImage = ImageProcessing.Domain.Enum.TypeImage.jpg,
                Image = num
                
            };

            await _imageRepository.Create(image);
            await _imageRepository.Delete(image);
            //var response1 = await _imageRepository.GetByName("new");

            return View(response);
        }
    }
}
