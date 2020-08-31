using Filevine.PublicApi.Models;
using FilevineLibrary.FilevineWebAPI.Request;
using FilevineLibrary.FilevineWebAPI.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilevineLibrary.FilevineWebAPI
{
    public class FilevineOperatingAccounts
    {
        public FilevineWebClient webClient { get; set; }

        public FilevineOperatingAccounts(FilevineWebClient _webClient)
        {
            webClient = _webClient;
        }
        
        public OperatingAccountResponse GetOperatingAccountItemList()
        {
            var projectId = "7570383"; //7399078 used to be because of expenses
            var OperatingAccountResponse = new OperatingAccountResponse();// webClient);
            var sectionSelector = "operatingAccounts"; //shot in dark bc worst case I need to check with projtype function from old prog
            var res = webClient.GetRequest($"core/projects/{projectId}/collections/{sectionSelector}");
            try
            {
                //Console.WriteLine("GET_Operating_AccountITEM_LIST");
                Console.WriteLine(res);
                OperatingAccountResponse = OperatingAccountResponse.FromJSON(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return OperatingAccountResponse;
        }

        public OperatingAccountResponse PostCollectionItem(OperatingAccountRequest request, string userinput)//Expense request)//string fName, mName, lName, ssn, birthDate, gender)
        {
            var operatingAccountResponse = new OperatingAccountResponse();
            var projectId = "7570383";//proj num
            var sectionSelector = "operatingAccounts";
            request.itemId.partner = userinput;

            var res = webClient.PostRequest($"core/projects/{projectId}/collections/{sectionSelector}/", request.ToJSON());//7229227 my project "@"post by partnerID
            try
            {
                operatingAccountResponse = OperatingAccountResponse.FromJSON(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return operatingAccountResponse;
        }
        
    }
}
