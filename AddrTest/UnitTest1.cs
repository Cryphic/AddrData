using AddrDataWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Moq;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;


namespace AddrTest
{

    [TestFixture]
    public class Tests
    {
        private InfoController _infoController;

        [SetUp]
        public void Setup()
        {
           
            var httpClientFactory = new FakeHttpClientFactory();
            _infoController = new InfoController(httpClientFactory);
        }

        [Test]
        public async Task InfoControllerReturnsObjectResponse()
        {
            // arrange
            string ipAddress = "gdfgdfg";

            // act
            var result = await _infoController.Get(ipAddress);

            // assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okObjectResult = (OkObjectResult)result;
            dynamic responseObject = okObjectResult.Value;
            Console.WriteLine(responseObject);
            Assert.That(result, Is.EqualTo(okObjectResult));

        }


    }

    
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new FakeHttpMessageHandler());
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent("{\"data\": {\"attributes\": {\"regional_internet_registry\": \"test RIR\",\"jarm\": \"test JARM\",\"network\": \"test network\",\"last_https_certificate_date\": 1234567890,\"tags\": [\"tag1\", \"tag2\"],\"crowdsourced_context\": [{\"source\": \"test source\",\"timestamp\": 1234567890,\"detail\": \"test detail\",\"severity\": \"test severity\",\"title\": \"test title\"}],\"country\": \"test country\",\"last_analysis_date\": 1234567890,\"as_owner\": \"test AS owner\",\"last_analysis_stats\": {\"harmless\": 1,\"malicious\": 2,\"suspicious\": 3,\"undetected\": 4,\"timeout\": 5},\"asn\": 12345,\"whois_date\": 1234567890}}}");
                return Task.FromResult(response);
            }
        }
    }
}