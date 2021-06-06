using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace SignBot.Modules
{
    public static class FfMpeg
    {
        private const string BucketName = "vrsl";
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.USEast1;
        private static readonly IAmazonS3 S3Client = new AmazonS3Client(BucketRegion);
        
        
        public static async Task<string> GetGif(string language, string word, string url, int context = 0)
        {
            var doesCacheExist = await CheckIfCacheExists(language, word.ToLower(), context);
            if (!doesCacheExist) 
                await UploadFileToS3(language, word.ToLower(), url, context);
            return "https://vrsl.s3.amazonaws.com/signs/gif/" + language.ToLower() + "/" + word.ToLower() +
                       context + ".gif";
        }

        private static async Task<bool> CheckIfCacheExists(string language, string word, int context)
        {
            try
            {
                await S3Client.GetObjectMetadataAsync("vrsl", $"signs/gif/{language.ToLower()}/{word.ToLower()}{context}.gif");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return false;
                throw;
            }
        }
        
        private static async Task UploadFileToS3(string language, string word, string url, int context)
        {
            var dirInfo = Directory.CreateDirectory(Environment.CurrentDirectory + "/" + language);
            var filePath = dirInfo.FullName +"/"+ word + context +".gif";
            Console.WriteLine($"Creating Cache for Sign: {word.Beautify()} in Language"+language.Beautify());
            
            await FFMpegArguments.FromUrlInput(new Uri(url)).OutputToFile(new Uri(filePath), true, //Convert to GIF
                options => options.WithVideoCodec("gif").UsingMultithreading(true)).ProcessAsynchronously();
            
            await new TransferUtility(S3Client).UploadAsync(new TransferUtilityUploadRequest //Upload GIF to S3
            {
                FilePath = filePath,
                BucketName = BucketName,
                Key = "signs/gif/" + language.ToLower() + "/" + word.ToLower()+context+".gif",
                CannedACL = S3CannedACL.PublicRead
            });
            
            File.Delete(filePath);
            Console.WriteLine("Cache Created for Sign: "+word.Beautify());
        }
    }
}