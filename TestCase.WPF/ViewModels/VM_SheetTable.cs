using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using sbGoogleApi.Sheets;
using sbdotnet;
using System.IO;

namespace TestCase.WPF.ViewModels
{
    internal partial class VM_SheetTable : VM_Base
    {
        ///////////////////////////////////////////////////////////
        #region Properties

        [ObservableProperty]
        public GSheetDataTable sheetTable;

        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Interface

        public VM_SheetTable()
        {

        }

        #endregion Interface
        ///////////////////////////////////////////////////////////


        
        ///////////////////////////////////////////////////////////
        #region Commands

        [RelayCommand]
        public async Task Run()
        {
            GoogleCredential? credential = null;

            try
            {
                // Note: if you're creating your own project you'll need your own service account & key.
                // The .json is not included in the repo for obvious reasons.
                string json = File.ReadAllText(@"e:\code\GSheetsDemo\sheetsdemo-447919-9dd8b3095b6a.json");
                credential = GoogleCredential
                    .FromJson(json)
                    .CreateScoped(SheetsService.ScopeConstants.SpreadsheetsReadonly);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return;
            }

            try
            {
                using var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential
                });
                var request = service.Spreadsheets.Get("1nRTuARmYpSPYi1NaWp-MbXhIaVuxpQczVmxT86i9nEc");
                request.IncludeGridData = true;
                var workbook = await request.ExecuteAsync();
                if (workbook is null)
                {
                    Logger.Warning("Sheet was null");
                    return;
                }
                Logger.Information("Sheet accessed successfully");

                Sheet sheet = workbook.Sheets[0];
                var sheetdata = sheet.Data.FirstOrDefault();
                if (sheetdata is null)
                {
                    Logger.Warning("sheetdata is null");
                    return;
                }

                SheetTable = new(sheetdata, "test", string.Empty);
                SheetTable.Parse();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Logger.Error($"InnerEx: {ex.InnerException?.Message}");
            }
        }

        #endregion Commands
        ///////////////////////////////////////////////////////////
    }
}
