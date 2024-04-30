using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedivacPlugins
{
    public class SimpleCustomAPIPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider?.GetService(typeof(ITracingService));
            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider?.GetService(typeof(IPluginExecutionContext));

            var validation = tracingService != null && pluginExecutionContext != null;

            if (!validation)
                throw new InvalidPluginExecutionException("Invalid Pipeline Execution Context or Tracing Service Specified!");

            var messageName = pluginExecutionContext.MessageName;
            var stage = pluginExecutionContext.Stage;

            validation = messageName.ToLower().Equals("csp_simplecustomapi") && stage == 30;

            if (!validation)
                throw new InvalidPluginExecutionException("Invalid Message Name or Stage Specified!");

            try
            {
                var universityId = (string)pluginExecutionContext.InputParameters["csp_universityid"];
                var universityName = (string)pluginExecutionContext.InputParameters["csp_universityName"];

                var rank = string.Format(@"{0} Ranked at the position of {1} - Internal Id : {2}",
                    universityName, new Random().Next(1, 50), universityId);

                pluginExecutionContext.OutputParameters["csp_response"] = rank;

                tracingService.Trace("Custom API Successfully Executed ...");
            }
            catch (Exception exceptionObject)
            {
                tracingService.Trace("Exception Occurred, Details : " + exceptionObject.Message);

                throw;
            }
        }
    }
}
