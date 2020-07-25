﻿using Azure.Storage.Blobs;
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
            var contentStreams = new List<byte[]>();
            foreach (var base64 in base64Strings)
            {
                var stripped = base64.Split(',')[1];
                contentStreams.Add(Convert.FromBase64String(stripped));
            }

            var blobIds = new List<string>();
            foreach (var contentStream in contentStreams)
            {
                // create new blob
                var newBlobId = CreateNewEmptyBlockBlob();
                blobIds.Add(newBlobId);
                BlockBlobClient blockBlob = Container.GetBlockBlobClient(newBlobId);

                // create single block
                var blockList = new List<string>();
                blockList.Add(CreateRandId());
                blockBlob.StageBlock(blockList[0], new MemoryStream(contentStream));

                // commit block
                blockBlob.CommitBlockList(blockList);
            }
            return blobIds;
        }

        public List<byte[]> DownloadFileStreams(List<string> blobIds)
        {
            List<byte[]> fileStreams = new List<byte[]>();
            foreach (var blobId in blobIds)
            {
                BlockBlobClient blockBlob = Container.GetBlockBlobClient(blobId);
                var blockStream = new MemoryStream();
                blockBlob.DownloadTo(blockStream);
                fileStreams.Add(blockStream.ToArray());

                // delete blob after fetching byte stream
                blockBlob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
            }
            return fileStreams;
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