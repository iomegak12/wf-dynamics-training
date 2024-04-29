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
    public class TOEFLScoreController : ApiController
    {
        [HttpGet]
        [Route("toefl-score/{aadhaarNumber}")]
        public TOEFLScoreModel GetTOEFLScore(string aadhaarNumber)
        {
            var toeflScore = new TOEFLScoreModel
            {
                AadhaarNumber = aadhaarNumber,
                Score = new Random().Next(100, 1000)
            };

            return toeflScore;
        }
    }
}
