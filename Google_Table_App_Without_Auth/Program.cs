using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Google_Table_App_Without_Auth
{
    class Program
    {
        private static readonly string AppName = "TestTable";
        private static readonly string SpreadsheetId = "1B_qS-3HzAZ4zTQkrCjgVHBXzo_D89DcX_TWmVILahCw";
        private static string[] Data = new string[5];
        
        static void Main(string[] args)
        {
            Data[0] = "проверка";
            Data[1] = "проверка";
            Data[2] = "проверка";
            Data[3] = "проверка";
            Data[4] = "проверка";

            String serviceAccountEmail = "tessheet3@curious-domain-178413.iam.gserviceaccount.com";
            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new[] { SheetsService.Scope.Spreadsheets}
                }.FromCertificate(certificate));

            // Create the service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName,
            });
            
            Console.WriteLine("Fill data");
            FillSpreadsheet(service, SpreadsheetId, Data);

            Console.WriteLine("Done");
        }
       
        /// <summary>
        /// Метод реализует заполнение таблицы данными
        /// </summary>
        /// <param name="service"></param>
        /// <param name="spreadsheetId"></param>
        /// <param name="data"></param>
        private static void FillSpreadsheet(SheetsService service, string spreadsheetId, string[] data)
        {
            List<Request> requests = new List<Request>();
            List<CellData> values = new List<CellData>();


            foreach (var a in data)
            {
                values.Add(new CellData
                {
                    UserEnteredValue = new ExtendedValue { StringValue = a }
                });
            }

            requests.Add(new Request
            {
                UpdateCells = new UpdateCellsRequest
                {
                    Start = new GridCoordinate
                    {
                        SheetId = 0,
                        RowIndex = FindFreeRow(service, spreadsheetId),
                        ColumnIndex = 0
                    },
                    Rows = new List<RowData>
                    {
                        new RowData { Values = values }
                    },
                    Fields = "userEnteredValue"
                }

            });


            BatchUpdateSpreadsheetRequest busr = new BatchUpdateSpreadsheetRequest
            {
                Requests = requests
            };
            service.Spreadsheets.BatchUpdate(busr, spreadsheetId).Execute();
        }

        /// <summary>
        /// Метод реализует проверку на наличие пустой строки для удобства записи
        /// </summary>
        /// <param name="service"></param>
        /// <param name="spreadsheetId"></param>
        /// <returns></returns>
        private static int FindFreeRow(SheetsService service, string spreadsheetId)
        {
            SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = 0;
            SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum dateTimeRenderOption = 0;

            bool emptyRow = false;
            int rowNumber = 1;

            while (emptyRow == false)
            {
                string range = $"A{rowNumber}:D{rowNumber}";

                SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);
                request.ValueRenderOption = valueRenderOption;
                request.DateTimeRenderOption = dateTimeRenderOption;

                ValueRange response = request.Execute();
                var jsonobj = JsonConvert.SerializeObject(response);
                dynamic obj = JsonConvert.DeserializeObject(jsonobj);
                var values = obj.values;

                if (values != null)
                {
                    rowNumber++;
                    continue;
                }

                emptyRow = true;
            }
            return rowNumber - 1;
        }
    }
}
