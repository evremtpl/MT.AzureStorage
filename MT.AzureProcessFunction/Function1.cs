using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MT.AzureStorageLib.Entities;
using MT.AzureStorageLib.Services.Concrete;
using MT.AzureStorageLib.Services.Interfaces;

namespace MT.AzureProcessFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public async static  Task Run([QueueTrigger("textimagequeue", Connection = "mtaccountconn")] PictureWritingQueue myQueueItem, ILogger log)
        {
            //burayı reflection yap
            ConnectionString.AzureStorageConnectionString = @"Your azure conn";
            IBlobStorage blobStorage = new BlobStorage();

            TableStorage<UserPicture> noSqlStorage = new TableStorage<UserPicture>();
            foreach (var item in myQueueItem.Pictures)
            {
                using var stream = await blobStorage.DownloadAsync(item, EContainerName.pictures);//blob storageden normal resim alındı.
                using var memoryStream = AddWriting(myQueueItem.WritingText, stream); //writin  e gönder yazısı eklensin.

                await blobStorage.UploadAsync(memoryStream, item, EContainerName.writingpictures);

                log.LogInformation($"Writing was added to {item}.");

                var userPicture = await noSqlStorage.GetAsync(myQueueItem.UserId, myQueueItem.City);

                if (userPicture.WritingRawPaths != null)
                {
                    myQueueItem.Pictures.AddRange(userPicture.WritingPaths);
                }
                userPicture.WritingPaths = myQueueItem.Pictures;
                await noSqlStorage.AddAsync(userPicture);

                // Process tamamlandı. Notification Hub a bildirim gönderiliyor.
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response;
                try
                {
                     response = await httpClient.GetAsync("http://localhost:10760/api/Notification/CompleteWritingProcess/" + myQueueItem.connectionId);

                    log.LogInformation($" Client {response.StatusCode } {response.Content} was informed.");
                    log.LogInformation($" Client {myQueueItem.connectionId} was informed.");
                }
                catch (Exception ex)
                {

                    log.LogInformation($" hataaaa {ex.Message} ");
                }
                
                
            }
    }


        public static MemoryStream AddWriting(string writingText, Stream pictureStream)
        {
            var ms = new MemoryStream();

            using (System.Drawing.Image image = Bitmap.FromStream(pictureStream))
            {
                using (Bitmap tempBitmap = new Bitmap(image.Width, image.Height))
                {
                    using (Graphics gph = Graphics.FromImage(tempBitmap))
                    {
                        gph.DrawImage(image, 0, 0);
                        var font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                        var color = Color.FromArgb(252, 0, 0);

                        var brush = new SolidBrush(color);
                        var point = new Point(20, image.Height - 50);
                        gph.DrawString(writingText, font, brush, point);

                        tempBitmap.Save(ms, ImageFormat.Png);
                    }
                }
            }

            ms.Position = 0;// stream alınırken başından itibaren atılır.
            return ms;
        }
    }
}
