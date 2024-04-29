using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedivacPlugins
{
    public static class EnvironmentValueProvider
    {
        public static string GetEnvironmentValue(IOrganizationService service, string environmentVariableName)
        {
            var fetchXmlString = @"
                <fetch top=""5"" distinct=""true"">
                  <entity name=""environmentvariablevalue"">
                    <attribute name=""environmentvariablevalueid"" />
                    <attribute name=""value"" />
                    <link-entity name=""environmentvariabledefinition"" from=""environmentvariabledefinitionid"" to=""environmentvariabledefinitionid"" link-type=""inner"" alias=""environmentvariabledefinition"">
                      <attribute name=""displayname"" />
                      <filter>
                        <condition attribute=""displayname"" operator=""eq"" value=""{0}"" />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>";

            var requestFetchXmlString = string.Format(fetchXmlString, environmentVariableName);
            var result = service.RetrieveMultiple(new FetchExpression(requestFetchXmlString));
            var validation = result != null && result.Entities.Count > 0;
            var environmentVariableValue = string.Empty;

            if (validation)
            {
                environmentVariableValue = result.Entities[0].GetAttributeValue<string>("value");
            }

            return environmentVariableValue;
        }
    }
}
