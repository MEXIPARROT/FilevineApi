using FilevineLibrary;
using FilevineLibrary.FilevineWebAPI;
using FilevineLibrary.FilevineWebAPI.Objects;
using FilevineLibrary.FilevineWebAPI.Request;
using FilevineLibrary.FilevineWebAPI.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PCLawData;
using PCLawData.Contracts;
using PCLawData.Operations;
using System.Configuration;
using PCLawData.Logging;

namespace FilevineLibrary
{

    class TestConsole
    {
        static void Main(string[] args)
        {
            var projs = new FilevineWebAPI.FilevineProjects(CreateConnectionToFilevine());
            var res = projs.GetProjectByNumber("testxid1");
            Console.WriteLine(res.projectId.native);
            Console.ReadLine();

        }

        private static void MainProgram()
        {
            //       Task t = MainAsync(args);
            //       t.Wait();
            //}
            //
            //static async Task MainAsync(string[] args)
            //{ 
            //var _client = CreateConnectionToFilevine();
            Logging Reports = new Logging();
            //Reports.Report("Testing1", ChangeOperation.Update);
            //Reports.Report("Testing2", -1);
            ///*
            FilevineOperatingAccounts OpAcc = new FilevineWebAPI.FilevineOperatingAccounts(CreateConnectionToFilevine());// _client);
            //appID=1 is Goldman NearRealtimeis   
            //appID=2 is Goldman DailyValidator
            //appID=3 is Testing            
            List<ChangeTracker> trackers = ChangeTrackingOperations.SelectActiveTrackers(1); //appID = 1          

            foreach (var tracker in trackers)
            {
                //Console.WriteLine("TableName:{0}    TablePKName: {1}    ID:{2}  LastVersion:{3}", tracker.TableName, tracker.TablePKName, tracker.ID, tracker.LastVersion);
                var changes = ChangeTrackingOperations.SelectChanges(tracker.TableName, tracker.TablePKName, tracker.LastVersion);
                if (changes == null)//check what specific operation it is 0 or 1 Update or insert 2 is delete
                    return;

                long newLastVersion = tracker.LastVersion;
                foreach (var change in changes)//changes is list of changetracker objects as well, but this time it's comparing query results from PCLaw Database
                {
                    if (change.SysChangeVersion > newLastVersion)
                        newLastVersion = change.SysChangeVersion;//update to the higher number *note if lastversion = 1 then display all changes.

                    //if (change.SysChangeOperation == ChangeOperation.Delete)
                    //{
                    //    try {
                    //        UPDATEaDELETE(OpAcc, change.Identity.ToString(), Reports);
                    //    }
                    //    catch(Exception ex)
                    //    {
                    //        Reports.Report(ex.ToString(),-1);                         
                    //    }
                    //}                    
                    //run here check + update FileV
                    else if (change.SysChangeOperation == ChangeOperation.Update || change.SysChangeOperation == ChangeOperation.Insert)
                    {
                        try
                        {
                            var grabAffectedRow = OperatingCostsOperations.SelectCost(change.Identity); //get the row
                            UPDATEchangeTableAndFilevine(OpAcc, grabAffectedRow, change.Identity.ToString(), Reports, change);//change.Identity.ToString());//pk_id //delete is not actual delete, status changes
                        }
                        catch (Exception ex)
                        {
                            Reports.Report(ex.ToString(), -1);
                        }
                    }
                }
                try
                {
                    var update = ChangeTrackingOperations.UpdateTrackerLastVersion(tracker, newLastVersion);
                }
                catch (Exception ex)
                {
                    Reports.Report(ex.ToString(), -1);
                }
            }
            //OpAcc.GetOperatingAccountItemList();
            //*/
            Console.WriteLine("Program done");
            Console.ReadLine();
        }

        private static FilevineWebClient CreateConnectionToFilevine()
        {
            var appSettings = ConfigurationManager.AppSettings.AllKeys; //get key and secret from app.config as string array
            string url = "https://api.filevine.io/";
            string key = appSettings[0];
            string secret = appSettings[1];

            string apiTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ");
            string hashInput = $"{key}/{apiTimestamp}/{secret}";
            var hash = CreateMD5(hashInput);
            string apiHash = hash;
            //Console.WriteLine("apiTimeStamp");
            //Console.WriteLine(apiTimestamp);
            //Console.WriteLine("hashInput");
            //Console.WriteLine(hashInput);
            //Console.WriteLine("hash");
            //Console.WriteLine(hash);

            var Filevinesettings = new FilevineSetting(url, key, secret);
            var Filevinesession = new FilevineSession(Filevinesettings);
            var _client = new FilevineWebClient(Filevinesettings);
            return _client;
        }

        private static void UPDATEaDELETE(FilevineOperatingAccounts OpAcc, string partnerId, Logging Reports)
        {
            //Console.WriteLine("Uploading a delete, empty but only with a hardcode explanation");
            OperatingAccountRequest operatingAccountRequest = new OperatingAccountRequest();
            itemId operatingAccountID = new itemId();
            PCLawOperatingExpense temp = new PCLawOperatingExpense();
            temp.Explanation = "delete Operation";
            OperatingAccount operatingObject = new OperatingAccount(temp);
            operatingAccountRequest.dataObject = operatingObject;
            operatingAccountRequest.itemId = operatingAccountID;
            try
            {
                OpAcc.PostCollectionItem(operatingAccountRequest, partnerId);
                Reports.Report("SuccessfulPostToFilevine_Deletion", ChangeOperation.Delete);
            }
            catch(Exception ex)
            {
                Reports.Report(ex.ToString(),-1);//BETTER TO HARDCODE "FAIL TO UPLOAD DELETE TO FILEVINE"?
            }
        }

        public static void UPDATEchangeTableAndFilevine(FilevineOperatingAccounts OpAcc, PCLawOperatingExpense temp, string partnerId, Logging Reports, ChangeTracking change)
        {
            //Console.WriteLine("{0},{1},{2},{3},{4},{5}", temp.Date, temp.MatterID, temp.Type, temp.ExplanationCodeName, temp.CheckNum, temp.InvNum);
            //var UPDATEchangeTableAndFilevine(grabquery,"inloop3");
            OperatingAccountRequest operatingAccountRequest = new OperatingAccountRequest();
            itemId operatingAccountItemID = new itemId();
            OperatingAccount operatingObject = new OperatingAccount(temp);
            operatingAccountRequest.dataObject = operatingObject; //the row in PCLAWDB
            operatingAccountRequest.itemId = operatingAccountItemID;
            try
            {
                OpAcc.PostCollectionItem(operatingAccountRequest, partnerId);
                Reports.Report("SuccessfulPostToFilevine", change.SysChangeOperation); //NOTE THIS WILL OCCUR EVEN THOUGH NOT UPLOADED, IF NO NEW ITEM ON FILEVINE IS BECAUSE PARTNER ID IS ALREADY EXISTED AND CAN'T BE REUSED
            }
            catch(Exception ex)
            {
                Reports.Report(ex.ToString(), -1);
            }
            //OpAcc.PostCollectionItem(operatingAccountRequest)
        }
        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
