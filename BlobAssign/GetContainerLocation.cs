using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Azure.Storage.Sas;
using Azure.Storage;

namespace BlobAssign
{
    public static class GetContainerLocation
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            try
            {
                string blobname = req.Query["blobname"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                blobname = blobname ?? data?.name;

                if (string.IsNullOrEmpty(blobname))
                    return new OkObjectResult("This HTTP triggered function executed successfully. Pass a blobname in the query string or in the request body to generate the list of URLs.");

                else
                {
                    var result = GenerateUploadURLs(blobname, context.FunctionAppDirectory);

                    return new OkObjectResult(result);
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message + ex.InnerException);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        static List<KeyValuePair<Uri, string>> GenerateUploadURLs(string blobName, string currentDir)
        {
            StorageAccountSettings[] settings;
            using (StreamReader r = new StreamReader(Path.Combine(currentDir, "storageaccounts.json")))
            {
                string json = r.ReadToEnd();
                settings = JsonConvert.DeserializeObject<StorageAccountSettings[]>(json);
            }

            var result = new List<KeyValuePair<Uri, string>>();
            foreach (var item in settings)
            {
                result.Add(CreateUploadAddresses(item, blobName));
            }
            return result;
        }

        static KeyValuePair<Uri, string> CreateUploadAddresses(StorageAccountSettings settings, string blobName)
        {
            Uri blobUri = new($"https://{settings.FrontDoorName}/{settings.ShardName}/{blobName}");

            var sas = new BlobSasBuilder(
                permissions: BlobSasPermissions.Write | BlobSasPermissions.Create,
                expiresOn: DateTimeOffset.UtcNow.AddHours(1))
            {
                BlobContainerName = settings.ContainerName,
                BlobName = blobName,
                Resource = "b",
            }.ToSasQueryParameters(sharedKeyCredential: new StorageSharedKeyCredential(
                    accountName: settings.AccountName,
                    accountKey: settings.AccountKey))
                .ToString();

            return new KeyValuePair<Uri, string>(blobUri, sas);
        }
    }
}