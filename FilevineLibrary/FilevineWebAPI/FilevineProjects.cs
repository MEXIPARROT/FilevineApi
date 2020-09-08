using Filevine.PublicApi.Models;
using FilevineLibrary.FilevineWebAPI.Request;
using FilevineLibrary.FilevineWebAPI.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilevineLibrary.FilevineWebAPI.Objects;
using FilevineSDK;

namespace FilevineLibrary.FilevineWebAPI
{
    public class FilevineProjects
    {
        public FilevineWebClient webClient { get; set; }
        public FilevineProjects(FilevineWebClient _webClient)
        {
            webClient = _webClient;
        }

        public projects GetProjectByNumber(string number) //returns existing projectIds
        {
            //var projectResponse
            var response = new projects();
            var webResponse = new ProjectNumberResponse();

            var res = webClient.GetRequest($"core/projects?number={number}");
            try
            {
                Console.WriteLine("In_GetProjectByNumber");
                Console.WriteLine(res);
                //Console.WriteLine(webResponse.count);
                //Console.WriteLine(webResponse.items[0]);
                webResponse = ProjectNumberResponse.FromJSON(res);
                if (webResponse.count > 0)
                    response = webResponse.items[0];
                Console.WriteLine(response.projectId.native);
                Console.WriteLine("DONE");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return response;
        }

        public bool CheckFilevinePartner(string partner, string projectId) //getCollectionByPartner returns a true or false if existing already //true already exists 
        {
            var response = new projects();
            var webResponse = new ProjectTypeResponse();
            var sectionSelector = "operatingAccounts";
            //projectId = "178985";

            Console.WriteLine($"core/projects/{projectId}/collections/{sectionSelector}/@{partner}");
            var res = webClient.GetRequest($"core/projects/{projectId}/collections/{sectionSelector}/@{partner}");//perfoming this will literally stop the program entirely so this doesn't work even if most efficient, can catch and return false but then catch is no longer catching real errors
            try
            {
                Console.WriteLine("CHECK_FILEVINE");
                Console.WriteLine(res);
                webResponse = ProjectTypeResponse.FromJSON(res);
                if (webResponse != null) //something exists at least one thing
                    return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
    }
}