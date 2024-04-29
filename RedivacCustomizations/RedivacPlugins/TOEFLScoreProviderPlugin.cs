using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedivacPlugins
{
    public class TOEFLScoreProviderPlugin : IPlugin
    {
        private const string TOEFL_SERVICE_URL_CONFIG = "TOEFL_SERVICE_URL";

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider?.GetService(typeof(IPluginExecutionContext));
                var validation = context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is EntityReference;

                if (!validation)
                {
                    throw new ApplicationException("Invalid Entity Reference Specifieid!");
                }

                var organizationServiceFactory = (IOrganizationServiceFactory)serviceProvider?.GetService(typeof(IOrganizationServiceFactory));
                var organizationService = organizationServiceFactory?.CreateOrganizationService(context.UserId);
                var tracingService = (ITracingService)serviceProvider?.GetService(typeof(ITracingService));

                validation = organizationServiceFactory != null &&
                    organizationService != null && tracingService != null;

                if (!validation)
                {
                    throw new ApplicationException("Invalid Context / Organization Service Provided!");
                }

                var aadhaarNumber = string.Empty;
                var score = 0;

                validation = context.InputParameters.Contains("AadhaarNumber");

                if (!validation)
                {
                    tracingService.Trace("Invalid Aadhaar Number Input Parameter Specified!");

                    return;
                }

                aadhaarNumber = context.InputParameters["AadhaarNumber"].ToString();

                validation = !string.IsNullOrEmpty(aadhaarNumber);

                if (!validation)
                {
                    tracingService.Trace("Invalid Aadhaar Number Specified!");

                    return;
                }

                var toelfServiceBaseUrl = EnvironmentValueProvider.GetEnvironmentValue(organizationService, TOEFL_SERVICE_URL_CONFIG);

                if (string.IsNullOrEmpty(toelfServiceBaseUrl))
                {
                    tracingService.Trace("Invalid TOEFL Service URL (Environment Settings) Specified!");

                    return;
                }

                var toeflServiceUrl = string.Format(@"{0}/{1}", toelfServiceBaseUrl, aadhaarNumber);

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(15000);
                    httpClient.DefaultRequestHeaders.ConnectionClose = true;

                    var response = httpClient.GetAsync(toeflServiceUrl).Result;

                    response.EnsureSuccessStatusCode();

                    var responseText = response.Content.ReadAsStringAsync().Result;
                    var toeflScoreModel = JsonConvert.DeserializeObject<TOEFLScoreModel>(responseText);

                    score = toeflScoreModel.Score;
                }

                context.OutputParameters["Score"] = score;

                tracingService.Trace("TOEFL Score Successfully Processed ... " + score.ToString());
            }
            catch (Exception exceptionObject)
            {
                throw new InvalidPluginExecutionException(exceptionObject.Message);
            }
        }
    }
}
