
using System.Collections.Generic;


namespace MT.AzureStorageLib.Entities
{
    public class PictureWritingQueue
    {
        public string UserId { get; set; }
        public string City { get; set; }

        public List<string> Pictures { get; set; }

        public string connectionId { get; set; }

        public string WritingText { get; set; }
    }
}
