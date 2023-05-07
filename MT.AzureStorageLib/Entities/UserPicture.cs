

using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace MT.AzureStorageLib.Entities
{
    public class UserPicture : TableEntity
    {
        public string Name { get; set; }
        public string RawPaths { get; set; }
        [IgnoreProperty]
        public List<string> Paths {
            get => RawPaths== null ? null : JsonConvert.DeserializeObject<List<string>>(RawPaths);
            set => RawPaths = JsonConvert.SerializeObject(value);
        }
        public string WritingRawPaths  { get; set; }
        [IgnoreProperty]
        public List<string> WritingPaths
        {
            get => WritingRawPaths == null? null: JsonConvert.DeserializeObject<List<string>>(WritingRawPaths);
            set => WritingRawPaths = JsonConvert.SerializeObject(value);
        }
    }
}
