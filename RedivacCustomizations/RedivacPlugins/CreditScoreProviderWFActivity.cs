using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedivacPlugins
{
    public class CreditScoreProviderWFActivity : CodeActivity
    {
        private const string CREDIT_SCORE_SERVICE_URL_CONFIG = "CREDIT_SCORE_URL";

        protected override void Execute(CodeActivityContext context)
        {
            var aadhaarNumber = this.AadhaarNumber.Get(context);
            var validation = !string.IsNullOrEmpty(aadhaarNumber);

            if (!validation)
                throw new InvalidOperationException("Invalid Aadhaar Number Specified for Credit Scores!");

            var workflowExecutionContext = context?.GetExtension<IWorkflowContext>();
            var organizationServiceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var organizationService = organizationServiceFactory.CreateOrganizationService(workflowExecutionContext?.InitiatingUserId);

            validation = workflowExecutionContext != null &&
                organizationServiceFactory != null && organizationService != null;

            if (!validation)
                throw new InvalidOperationException("Invalid Workflow Execution Context Specified!");

            var creditScoreServiceBaseUrl = EnvironmentValueProvider.GetEnvironmentValue(organizationService, CREDIT_SCORE_SERVICE_URL_CONFIG);
            var creditScoreUrl = string.Format(@"{0}/{1}", creditScoreServiceBaseUrl, aadhaarNumber);
            var score = 0;

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(15000);
                httpClient.DefaultRequestHeaders.ConnectionClose = true;

                var response = httpClient.GetAsync(creditScoreUrl).Result;

                response.EnsureSuccessStatusCode();

                var responseText = response.Content.ReadAsStringAsync().Result;
                var creditScoreModel = JsonConvert.DeserializeObject<CreditScoreModel>(responseText);

                score = creditScoreModel.CreditScore;
            }

            this.CreditScore.Set(context, score);
        }

        [Input("Aadhaar Number")]
        public InArgument<string> AadhaarNumber { get; set; }

        [Output("Credit Score")]
        public OutArgument<int> CreditScore { get; set; }
    }
}
