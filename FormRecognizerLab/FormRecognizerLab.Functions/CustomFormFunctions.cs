using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Shared;
using System.Linq;
using FormRecognizerLab.Functions.Models;
using Azure.AI.FormRecognizer.Models;

namespace FormRecognizerLab.Functions
{
    public static class CustomFormFunction
    {
        [FunctionName("Analyze")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            string fileName = req.Query["fileName"];
            string model = req.Query["model"];
            var container = Environment.GetEnvironmentVariable("BlobUrl");
            var sass = Environment.GetEnvironmentVariable("Sassblob");
            var file = await GetDocumentFromStorage(fileName, container, sass);
            var streamFile = new MemoryStream(file);

            var frService = Environment.GetEnvironmentVariable("FormRecognizerService");
            var suscriptionKey = Environment.GetEnvironmentVariable("SuscriptionKeyForms");

            FormRecognizerService frClient = new FormRecognizerService(frService, suscriptionKey);
            var dataForm = await frClient.AnalyzeCustomFormFromStream(streamFile,model);
            var mappingForm = MappingForm(dataForm.FirstOrDefault());
          
            return new OkObjectResult(mappingForm);
        }

        private static async Task<byte[]> GetDocumentFromStorage(string fileName,string containerUri,string sasToken)
        {
           
            var uri = $"{containerUri}/{fileName}{sasToken}";
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(uri);

                    var response = await client.SendAsync(request).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        return new byte[] { };
                    }
                }
            }
        }

        private static InvoceModel MappingForm(RecognizedForm form)
        {
            var result = new InvoceModel()
            {
                CompanyPhone = form.Fields.Where(f => f.Key == "CompanyPhone").FirstOrDefault().Value?.ValueData?.Text,
                Date = form.Fields.Where(f => f.Key == "DatePurchase").FirstOrDefault().Value?.ValueData?.Text,
                Email = form.Fields.Where(f => f.Key == "Mail").FirstOrDefault().Value?.ValueData?.Text,
                PurchaseOrder = form.Fields.Where(f => f.Key == "PurchaseOrder").FirstOrDefault().Value?.ValueData?.Text,
                ShippedFromCompany = form.Fields.Where(f => f.Key == "ShippedFromCompany").FirstOrDefault().Value?.ValueData?.Text,
                ShippedtoCompany = form.Fields.Where(f => f.Key == "ShippedToCompany").FirstOrDefault().Value?.ValueData?.Text,
                ShippedToVendor = form.Fields.Where(f => f.Key == "ShippedToVendor").FirstOrDefault().Value?.ValueData?.Text,
                Subtotal = form.Fields.Where(f => f.Key == "Subtotal").FirstOrDefault().Value?.ValueData?.Text,
                Tax = form.Fields.Where(f => f.Key == "Tax").FirstOrDefault().Value?.ValueData?.Text,
                Total = form.Fields.Where(f => f.Key == "Total").FirstOrDefault().Value?.ValueData?.Text,
                WebSite = form.Fields.Where(f => f.Key == "WebSite").FirstOrDefault().Value?.ValueData?.Text

            };

            return result;
        }
    }
}
