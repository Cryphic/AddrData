
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AddrDataWeb;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Net;

namespace AddrDataWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : Controller
    {
        public async Task<List<String>> GetUniqueClients()
        {
            var collection = HttpContext.RequestServices.GetService(typeof(IMongoCollection<PacketData>)) as IMongoCollection<PacketData>;
            var distinctSender = collection.Distinct<string>("Sender", new BsonDocument()).ToList();
            return distinctSender;

        }
        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var distinctSender = await GetUniqueClients();
            return Ok(distinctSender);

        }

    

    }
}

