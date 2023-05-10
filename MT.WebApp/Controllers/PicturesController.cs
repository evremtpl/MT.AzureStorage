using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MT.AzureStorageLib.Entities;
using MT.AzureStorageLib.Services.Concrete;
using MT.AzureStorageLib.Services.Interfaces;
using MT.WebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
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
            var Blobs = _blobStorage.GetNames(EContainerName.writingpictures);

            if (Blobs.Any())
            {
                foreach (var blob in Blobs)
                {
                    if(userPicture.WritingPaths.Contains(blob))
                    {
                        fileBlobs.Add(new FileBlob { Name = blob, Url = $"{_blobStorage.BlobUrl}/{EContainerName.writingpictures}/{blob}" });
                    }
                }
                
                    userPicture.WritingPaths.ForEach(x =>
                    {
                        if (Blobs.Contains(x))
                        {
                           
                        }
                       
                    });
             
            }
           

            ViewBag.fileBlobs = fileBlobs;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddWriting( PictureWritingQueue pictureWritingQueue)
        
        {
            if (pictureWritingQueue.Pictures != null)
            {
                var jsonString = JsonConvert.SerializeObject(pictureWritingQueue);

                var jsonStringBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

                // to get AzureQueue Object by Reflection
                Assembly disAssembly = Assembly.Load("MT.AzureStorageLib");
                var azQueueType = disAssembly.GetType($"{disAssembly.GetName().Name}.Services.Concrete.AzQueue");

                var azQueue = Activator.CreateInstance(azQueueType, new object[] { "textimagequeue" });
                await (Task)azQueueType.GetTypeInfo()
                  .GetDeclaredMethod("SendMessageAsync")
                  .Invoke(azQueue, new object[] { $"{jsonStringBase64}" });

                return Ok("Writing  is being added to your images.");
            }
            else { return Ok("You didnt pick any image"); }





            //AzQueue azQueue = new AzQueue("textimagequeue");
            // await azQueue.SendMessageAsync(jsonStringBase64);

          
        }
    }
}
