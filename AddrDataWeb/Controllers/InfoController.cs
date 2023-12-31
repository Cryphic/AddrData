﻿
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

/* Read API data into these classes */

#region API classes for deserialization
public class VirusTotal
{

    public class Response
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        public string regional_internet_registry { get; set; }
        public string jarm { get; set; }
        public string network { get; set; }
        public long last_https_certificate_date { get; set; }
        public List<string> tags { get; set; }
        public List<CrowdsourcedContext> crowdsourced_context { get; set; }
        public string country { get; set; }
        public long last_analysis_date { get; set; }
        public string as_owner { get; set; }
        public LastAnalysisStats last_analysis_stats { get; set; }
        public int asn { get; set; }
        public long whois_date { get; set; }
        // public Dictionary<string, AnalysisResult> last_analysis_results { get; set; }
    }

    public class CrowdsourcedContext
    {
        public string source { get; set; }
        public long timestamp { get; set; }
        public string detail { get; set; }
        public string severity { get; set; }
        public string title { get; set; }
    }

    public class LastAnalysisStats
    {
        public int harmless { get; set; }
        public int malicious { get; set; }
        public int suspicious { get; set; }
        public int undetected { get; set; }
        public int timeout { get; set; }
    }

    public class AnalysisResult
    {
        public string category { get; set; }
        public string result { get; set; }
        public string method { get; set; }
        public string engine_name { get; set; }
    }
}
public class AbuseIPDB
{
    public class Response
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string ipAddress { get; set; }
        public bool? isPublic { get; set; }
        public int ipVersion { get; set; }
        public bool? isWhitelisted { get; set; }
        public int abuseConfidenceScore { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string usageType { get; set; }
        public string isp { get; set; }
        public string domain { get; set; }
        public List<object> hostnames { get; set; }
        public int totalReports { get; set; }
        public int numDistinctUsers { get; set; }
        public DateTime? lastReportedAt { get; set; }
        public List<Report> reports { get; set; }
    }

    public class Report
    {
        public DateTime reportedAt { get; set; }
        public string comment { get; set; }
        public List<int> categories { get; set; }
        public int reporterId { get; set; }
        public string reporterCountryCode { get; set; }
        public string reporterCountryName { get; set; }
    }
}
#endregion


namespace AddrDataWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public InfoController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

      //API Controllers IP PATH on GET request run the QueryFunc and return results
        [HttpGet("{ip}")]
        public async Task<IActionResult> Get(string ip)
        {

            //Query APIs
            try
            {
            VirusTotal.Response total = await QueryFunc<VirusTotal.Response>(url: $"https://www.virustotal.com/api/v3/ip_addresses/{ip}",
                                     apikey: "c25594a277e9143407bc57d1bb89d1c5dc94cc5d930d5eecbbeaf69a83c18c39",
                                     header: "x-apikey");

            AbuseIPDB.Response abuse =  await QueryFunc<AbuseIPDB.Response>(url: $"https://api.abuseipdb.com/api/v2/check?ipAddress={ip}",
                                       apikey: "86a9ee610b400c5be869343dfd079d0ec54a6d46ffde6ddcdc5d8f470ef290eb534b9289eb109b05",
                                      header: "Key");


                //combine results and return w/ OkObjectResult aka OK answer code 200 
                return Ok(new { VirusTotal = total, AbuseIPDB = abuse });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
         
        }



        //Deserialization class, url, api, headers
        /*method sends an HTTP GET request to the specified URL with an API key in the header*/
        public async Task<T> QueryFunc<T>(string url, string apikey, string header)
        {
            //Init result var and create client
            T result = default;
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add(header, apikey); //set headers
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<T>(jsonString); //Read on success and deserialize
            }
            return result;
        }

       





    }
}

