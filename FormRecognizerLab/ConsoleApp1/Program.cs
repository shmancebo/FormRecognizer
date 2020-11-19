using Azure.AI.FormRecognizer.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {

        static void Main(string[] args)
        {
            string frService = "https://frservice-lab.cognitiveservices.azure.com/";
            string suscriptionKey = "632277708e184e1c8a736a0509a5a847";
            Console.WriteLine("Quiere procesar un formulario (1) o un recibo (2)");
            string option = Console.ReadLine();

            AnalyzeFile(option, frService, suscriptionKey).Wait();
          
        }


        public static async Task AnalyzeFile(string option,string frService,string suscriptionKey)
        {
            if (option.Equals("1"))
            {
                var form = File.OpenRead("../../../../Shared/Samples/Form_1.jpg");
                FormRecognizerService frClient = new FormRecognizerService(frService, suscriptionKey);

                Console.WriteLine("Introduzca el modelo de entrenamiento:");
                var model = Console.ReadLine();
                if (string.IsNullOrEmpty(model))
                {
                    var formContent = await frClient.AnalyzeFormFromStream(form);
                    PrintForm(formContent);
                }

                else
                {
                    var formContent = await frClient.AnalyzeCustomFormFromStream(form, model);
                    PrintCustomForm(formContent);
                }
            }
            else
            {
                var receipt = File.OpenRead("../../../../Shared/Samples/contoso-allinone.jpg");
                FormRecognizerService frClient = new FormRecognizerService(frService, suscriptionKey);
                var receiptContent = await frClient.AnalyzeReceiptFromStream(receipt);
                PrintReceipt(receiptContent);
            }

            Console.ReadLine();
        }

        public static void PrintForm(FormPageCollection formPages)
        {
            foreach (FormPage page in formPages)
            {
                Console.WriteLine($"Form Page {page.PageNumber} has {page.Lines.Count} lines.");

                for (int i = 0; i < page.Lines.Count; i++)
                {
                    FormLine line = page.Lines[i];
                    Console.WriteLine($"    Line {i} has {line.Words.Count} word{(line.Words.Count > 1 ? "s" : "")}, and text: '{line.Text}'.");
                }

                for (int i = 0; i < page.Tables.Count; i++)
                {
                    FormTable table = page.Tables[i];
                    Console.WriteLine($"Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.");
                    foreach (FormTableCell cell in table.Cells)
                    {
                        Console.WriteLine($"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) contains text: '{cell.Text}'.");
                    }
                }
            }
        }

        public static void PrintCustomForm(RecognizedFormCollection forms)
        {
            foreach (RecognizedForm form in forms)
            {
                Console.WriteLine($"Form of type: {form.FormType}");
                foreach (FormField field in form.Fields.Values)
                {
                    Console.WriteLine($"Field '{field.Name}: ");

                    if (field.LabelData != null)
                    {
                        Console.WriteLine($"    Label: '{field.LabelData.Text}");
                    }

                    Console.WriteLine($"    Value: '{field.ValueData.Text}");
                    Console.WriteLine($"    Confidence: '{field.Confidence}");
                }
            }
        }

        public static void PrintReceipt(RecognizedFormCollection receipts)
        {
            foreach (RecognizedForm receipt in receipts)
            {
                FormField merchantNameField;
                if (receipt.Fields.TryGetValue("MerchantName", out merchantNameField))
                {
                    if (merchantNameField.Value.ValueType == FieldValueType.String)
                    {
                        string merchantName = merchantNameField.Value.AsString();

                        Console.WriteLine($"Merchant Name: '{merchantName}', with confidence {merchantNameField.Confidence}");
                    }
                }

                FormField transactionDateField;
                if (receipt.Fields.TryGetValue("TransactionDate", out transactionDateField))
                {
                    if (transactionDateField.Value.ValueType == FieldValueType.Date)
                    {
                        DateTime transactionDate = transactionDateField.Value.AsDate();

                        Console.WriteLine($"Transaction Date: '{transactionDate}', with confidence {transactionDateField.Confidence}");
                    }
                }

                FormField itemsField;
                if (receipt.Fields.TryGetValue("Items", out itemsField))
                {
                    if (itemsField.Value.ValueType == FieldValueType.List)
                    {
                        foreach (FormField itemField in itemsField.Value.AsList())
                        {
                            Console.WriteLine("Item:");

                            if (itemField.Value.ValueType == FieldValueType.Dictionary)
                            {
                                IReadOnlyDictionary<string, FormField> itemFields = itemField.Value.AsDictionary();

                                FormField itemNameField;
                                if (itemFields.TryGetValue("Name", out itemNameField))
                                {
                                    if (itemNameField.Value.ValueType == FieldValueType.String)
                                    {
                                        string itemName = itemNameField.Value.AsString();

                                        Console.WriteLine($"    Name: '{itemName}', with confidence {itemNameField.Confidence}");
                                    }
                                }

                                FormField itemTotalPriceField;
                                if (itemFields.TryGetValue("TotalPrice", out itemTotalPriceField))
                                {
                                    if (itemTotalPriceField.Value.ValueType == FieldValueType.Float)
                                    {
                                        float itemTotalPrice = itemTotalPriceField.Value.AsFloat();

                                        Console.WriteLine($"    Total Price: '{itemTotalPrice}', with confidence {itemTotalPriceField.Confidence}");
                                    }
                                }
                            }
                        }
                    }
                }

                FormField totalField;
                if (receipt.Fields.TryGetValue("Total", out totalField))
                {
                    if (totalField.Value.ValueType == FieldValueType.Float)
                    {
                        float total = totalField.Value.AsFloat();

                        Console.WriteLine($"Total: '{total}', with confidence '{totalField.Confidence}'");
                    }
                }
            }
        }
    }
}
