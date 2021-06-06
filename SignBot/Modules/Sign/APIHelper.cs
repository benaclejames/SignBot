using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SignBot.Modules.Sign
{
    public static class ApiHelper
    {
        public static class VRSL
        {
            public abstract class SearchResult
            {
                public abstract class SearchResults    {
                    public string sign { get; set; } 
                    public string url { get; set; } 
                    public string category { get; set; } 
                }

                public class Root    {
                    public SearchResults searchResults { get; set; } 
                }
            }
            
            public static async Task<SearchResult.Root> QueryVrsl(string sign, string language)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("sign", sign);
                var httpResponse =
                    await client.GetAsync("https://5t77ip5on5.execute-api.eu-west-2.amazonaws.com/prod/"+language);
                var apiRespString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SearchResult.Root>(apiRespString);
            }
        }

        public class ASL
        {
            public enum ApiResponseClass
            {
                NONE,
                SEARCH,
                PAGE
            }
            
            public class PageResponse
            {
                public class Variation    {
                    public string type { get; set; } 
                    public string url { get; set; } 
                }

                public class PageDetails    {
                    public string meaning { get; set; } 
                    public string context { get; set; } 
                    public string sentence { get; set; } 
                    public List<string> synonyms { get; set; } 
                }

                public class PageResults    {
                    public string videoURL { get; set; } 
                    public List<Variation> variations { get; set; } 
                    public PageDetails pageDetails { get; set; } 
                }

                public class Root    {
                    public PageResults pageResults { get; set; } 
                }
            }
            public class SearchResponse
            {
                public class Result    {
                    public string pageLink { get; set; } 
                    public string context { get; set; } 
                }

                public class SearchResults    {
                    public string sign { get; set; } 
                    public List<Result> results { get; set; } 
                }

                public class Root    {
                    public SearchResults searchResults { get; set; } 
                }
            }

            public class APIResponse
            {
                public ApiResponseClass type = ApiResponseClass.NONE;
                public PageResponse.Root pageResponse;
                public SearchResponse.Root searchResponse;
            }
            
            public static async Task<APIResponse> QueryAsl(string sign)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("sign", sign);
                var httpResponse = await client.GetAsync("https://5t77ip5on5.execute-api.eu-west-2.amazonaws.com/prod/asl");
                var apiResp = new APIResponse();
                var apiRespString = await httpResponse.Content.ReadAsStringAsync();
                apiResp.pageResponse = JsonConvert.DeserializeObject<PageResponse.Root>(apiRespString);
                if (apiResp.pageResponse.pageResults != null)
                {
                    apiResp.type = ApiResponseClass.PAGE;
                    return apiResp;
                }
                apiResp.searchResponse = JsonConvert.DeserializeObject<SearchResponse.Root>(apiRespString);
                if (apiResp.searchResponse.searchResults != null)
                    apiResp.type = ApiResponseClass.SEARCH;
                return apiResp;
            }
        }
    }
}