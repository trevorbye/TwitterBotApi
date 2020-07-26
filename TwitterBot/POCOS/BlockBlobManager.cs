using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;

namespace TwitterBot.POCOS
{
    public class BlockBlobManager
    {
        // get from env variable in prod
        private readonly string _key = "DefaultEndpointsProtocol=https;AccountName=tweetmedia;AccountKey=o1ANwoz6LTAzZ7b99AxRNYIDyw0orF133ycOpzMwGDi79TletTpu1B4McWj1XiyXzjfZH4nQ9Xd+/iww9P6jsQ==;EndpointSuffix=core.windows.net";
        public BlobServiceClient Client { get; set; }
        public BlobContainerClient Container { get; set; }

        public BlockBlobManager()
        {
            Client = new BlobServiceClient(this._key);
            Container = Client.GetBlobContainerClient("media");
        }

        public List<string> UploadFileStreams(List<string> base64Strings)
        {
            var blobIds = new List<string>();
            foreach (var base64 in base64Strings)
            {
                var chunked = base64.Split(',');

                // create new blob
                var newBlobId = CreateNewEmptyBlockBlob();
                blobIds.Add(newBlobId);
                BlockBlobClient blockBlob = Container.GetBlockBlobClient(newBlobId);

                // create metadata
                IDictionary<string, string> metadata = new Dictionary<string, string>();
                metadata.Add("mimeString", chunked[0] + ",");
                
                // create single block
                var blockList = new List<string>();
                blockList.Add(CreateRandId());
                blockBlob.StageBlock(blockList[0], new MemoryStream(Convert.FromBase64String(chunked[1])));

                // commit block
                blockBlob.CommitBlockList(blockList, null, metadata);
            }
            return blobIds;
        }

        public List<byte[]> DownloadFileStreams(List<string> blobIds)
        {
            List<byte[]> fileStreams = new List<byte[]>();
            foreach (var blobId in blobIds)
            {
                BlockBlobClient blockBlob = Container.GetBlockBlobClient(blobId);
                var properties = blockBlob.GetProperties();

                var blockStream = new MemoryStream();
                blockBlob.DownloadTo(blockStream);
                fileStreams.Add(blockStream.ToArray());
            }
            return fileStreams;
        }

        public List<string> DownloadBase64FileStrings(List<string> blobIds)
        {
            var base64Strings = new List<string>();
            foreach (var blobId in blobIds)
            {
                BlockBlobClient blockBlob = Container.GetBlockBlobClient(blobId);
                var properties = blockBlob.GetProperties();
                string mimeString = properties.Value.Metadata["mimeString"];
                
                var blockStream = new MemoryStream();
                blockBlob.DownloadTo(blockStream);
                base64Strings.Add(mimeString + Convert.ToBase64String(blockStream.ToArray()));
            }
            return base64Strings;
        }

        public void DeleteBlobById(string blobId)
        {
            BlockBlobClient blockBlob = Container.GetBlockBlobClient(blobId);
            blockBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public string CreateRandId()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[20];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        public string CreateNewEmptyBlockBlob()
        {
            string blobId = CreateRandId();
            using (Stream s = new MemoryStream())
            {
                Container.UploadBlob(blobId, s);
            }
            return blobId;
        }
    }
}