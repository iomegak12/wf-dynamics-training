using RedivacRESTServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RedivacRESTServices.Controllers
{
    [EnableCors("*", "*", "*")]
    public class TransportDetailController : ApiController
    {
        [HttpGet]
        [Route("transport-detail/{location}")]
        public TransportDetailModel GetTransportDetails(string location)
        {
            var transportDetail = new TransportDetailModel
            {
                Location = location,
                TransportationCharges = new Random().Next(1000, 10000)
            };

            return transportDetail;
        }
    }
}
