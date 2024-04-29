using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedivacPlugins
{
    public class ApplicationRegistrationNumberPlugin : IPlugin
    {
        private string additionalConfigurationInfo = string.Empty;

        public ApplicationRegistrationNumberPlugin(string unsecureConfiguration = "", string secureConfiguration = "")
        {
            var validation = !string.IsNullOrEmpty(unsecureConfiguration);
            var unsecureName = string.Empty;
            var secureName = string.Empty;

            if (validation)
            {
                var configuration = JsonConvert.DeserializeObject<Configuration>(unsecureConfiguration);

                unsecureName = configuration.Name;
            }

            validation = !string.IsNullOrEmpty(secureConfiguration);

            if (validation)
            {
                var configuration = JsonConvert.DeserializeObject<Configuration>(secureConfiguration);

                secureName = configuration.Name;
            }

            this.additionalConfigurationInfo = string.Format(@"{0}-{1}", unsecureName, secureName);
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider?.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider?.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory?.CreateOrganizationService(context?.UserId);
            var tracingServie = (ITracingService)serviceProvider?.GetService(typeof(ITracingService));

            var validation = context != null &&
                serviceFactory != null && service != null && tracingServie != null;

            if (!validation)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, "Invalid Context Details Provided!");
            }

            try
            {
                if (context.Depth > 2)
                {
                    return;
                }

                validation = context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is Entity;

                if (!validation)
                {
                    tracingServie.Trace("Executing the Plugin without Entity References, and Not Supported!");

                    return;
                }

                var targetEntity = context.InputParameters["Target"] as Entity;
                var updateAutoNumberConfiguration = new Entity("csp_autonumberconfiguration");
                var autoNumber = new StringBuilder();

                string prefix, suffix, separator, day, month, year;
                int currentNumber;

                var today = DateTime.Now;

                day = today.Day.ToString("00");
                month = today.Month.ToString("00");
                year = today.Year.ToString("00");

                var qeAutoNumberConfiguration = new QueryExpression
                {
                    EntityName = updateAutoNumberConfiguration.LogicalName,
                    ColumnSet = new ColumnSet("csp_prefix", "csp_separator", "csp_suffix", "csp_currentnumber", "csp_configurationname"),
                };

                var executionResult = service.RetrieveMultiple(qeAutoNumberConfiguration);

                if (executionResult.Entities.Count == 0)
                {
                    tracingServie.Trace("Unable to Find Auto Number Configurations!");

                    return;
                }

                foreach (var entityRecord in executionResult.Entities)
                {
                    var configurationName = entityRecord["csp_configurationname"].ToString().ToLower();

                    if (configurationName.Equals("applicationidconfiguration"))
                    {
                        prefix = entityRecord.GetAttributeValue<string>("csp_prefix");
                        suffix = entityRecord.GetAttributeValue<string>("csp_suffix");
                        separator = entityRecord.GetAttributeValue<string>("csp_separator");
                        currentNumber = entityRecord.GetAttributeValue<int>("csp_currentnumber");

                        var newCurrent = currentNumber;

                        newCurrent++;

                        var currentString = newCurrent.ToString("000000");

                        updateAutoNumberConfiguration.Id = entityRecord.Id;
                        updateAutoNumberConfiguration["csp_currentnumber"] = newCurrent;

                        service.Update(updateAutoNumberConfiguration);

                        autoNumber.Append(prefix + separator + year + month + day + separator + suffix + separator + currentString);

                        if (!string.IsNullOrEmpty(additionalConfigurationInfo))
                        {
                            autoNumber.Append(separator + additionalConfigurationInfo);
                        }

                        break;
                    }
                }

                targetEntity["csp_applicationregistrationnumber"] = autoNumber.ToString();

                service.Update(targetEntity);

                tracingServie.Trace("Target Entity Updated ...");
                tracingServie.Trace("Application Number Generated Successfully ... " + autoNumber.ToString());
            }
            catch (Exception exceptionObject)
            {
                tracingServie.Trace("Error Occurred, Details : " + exceptionObject.Message);

                throw;
            }
        }
    }
}
