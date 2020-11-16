using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared
{
    public class FormRecognizerService
    {
        private FormRecognizerClient _frClient = null;
        public FormRecognizerService(string endpointService, string keyService)
        {
            var credentials = new AzureKeyCredential(keyService);
            _frClient = new FormRecognizerClient(new Uri(endpointService), credentials);
        }

        public async Task<FormPageCollection> AnalyzeFormFromStream(Stream form)
        {
            FormPageCollection collection = await _frClient.StartRecognizeContent(form).WaitForCompletionAsync();
            return collection;
        }

        public async Task<RecognizedFormCollection> AnalyzeCustomFormFromStream(Stream form,string modelId)
        {
            RecognizedFormCollection collection = await _frClient.StartRecognizeCustomFormsAsync(modelId,form).WaitForCompletionAsync();
            return collection;
        }

        public async Task<RecognizedFormCollection> AnalyzeReceiptFromStream(Stream receipt)
        {
            RecognizedFormCollection collection = await _frClient.StartRecognizeReceiptsAsync(receipt).WaitForCompletionAsync();
            return collection;
        }

       
    }
}
