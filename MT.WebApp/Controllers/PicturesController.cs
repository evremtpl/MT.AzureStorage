using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MT.AzureStorageLib.Entities;
using MT.AzureStorageLib.Services.Concrete;
using MT.AzureStorageLib.Services.Interfaces;
using MT.WebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MT.WebApp.Controllers
{
    public class PicturesController : Controller
    {
        public string UserId { get; set; } = "1";
        public string City { get; set; } = "ankara";
        public string Name { get; set; } = "ipek";

        private readonly TableStorage<UserPicture> _noSqlStorage;
        private readonly IBlobStorage _blobStorage;

        public PicturesController(TableStorage<UserPicture> noSqlStorage, IBlobStorage blobStorage)
        {
            _noSqlStorage = noSqlStorage;
            _blobStorage = blobStorage;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.UserId = UserId;
            ViewBag.City = City;

            List<FileBlob> fileBlobs = new List<FileBlob>();

            var user = await _noSqlStorage.GetAsync(UserId, City);

            if (user != null)
            {
                user.Paths?.ForEach(x =>
                {
                    fileBlobs.Add(new FileBlob { Name = x, Url = $"{_blobStorage.BlobUrl}/{EContainerName.pictures}/{x}" });
                });
            }
            ViewBag.fileBlobs = fileBlobs;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IEnumerable<IFormFile> pictures)
        {
            List<string> picturesList = new List<string>();
            foreach (var item in pictures)
            {
                var newPictureName = $"{Guid.NewGuid()}{Path.GetExtension(item.FileName)}";

                await _blobStorage.UploadAsync(item.OpenReadStream(), newPictureName, EContainerName.pictures);

                picturesList.Add(newPictureName);
            }

            var isUser = await _noSqlStorage.GetAsync(UserId, City);

            if (isUser != null)
            {
                if(isUser.Paths!=null) picturesList.AddRange(isUser.Paths);

                isUser.Paths = picturesList;
            }
            else
            {
                isUser = new UserPicture();

                isUser.RowKey = UserId;
                isUser.PartitionKey = City;
                isUser.Name = Name;
                isUser.Paths = picturesList;
            }

            await _noSqlStorage.AddAsync(isUser);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ShowWritingImages()
        {
            List<FileBlob> fileBlobs = new List<FileBlob>();
            UserPicture userPicture = await _noSqlStorage.GetAsync(UserId, City);

            userPicture.WritingPaths.ForEach(x =>
            {
                fileBlobs.Add(new FileBlob { Name = x, Url = $"{_blobStorage.BlobUrl}/{EContainerName.writingpictures}/{x}" });
            });

            ViewBag.fileBlobs = fileBlobs;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddWriting( PictureWritingQueue pictureWritingQueue)
        
        {
            var jsonString=JsonConvert.SerializeObject(pictureWritingQueue);

            var jsonStringBase64=Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

            AzQueue azQueue = new AzQueue("textimagequeue");

           await azQueue.SendMessageAsync(jsonStringBase64);

            return Ok();
        }
    }
}
