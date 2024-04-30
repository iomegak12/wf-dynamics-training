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
    public class CreditScoreController : ApiController
    {
        [HttpGet]
        [Route("credit-score/{aadhaarNumber}")]
        public CreditScoreModel GetCreditScore(string aadhaarNumber)
        {
            var creditScoreModel = new CreditScoreModel
            {
                AadhaarNumber = aadhaarNumber,
                CreditScore = new Random().Next(100, 1000)
            };

            return creditScoreModel;
        }
    }
}
