using FakeXrmEasy;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Plugins.PluginSteps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;

namespace RedivacPlugins.Tests
{
    [TestClass]
    public class ApplicationRegistrationNumberPluginTest
    {
        [TestMethod]
        public void ShouldGenerateApplicationRegistrationNumber()
        {
            var _context =
                MiddlewareBuilder
                    .New()
                    .AddCrud()
                    .AddFakeMessageExecutors()
                    .AddPipelineSimulation()
                    .UsePipelineSimulation()
                    .UseCrud()
                    .UseMessages()
                    .SetLicense(FakeXrmEasy.Abstractions.Enums.FakeXrmEasyLicense.NonCommercial)
                    .Build();

            var _service = _context.GetOrganizationService();
            var targetEntity = new Entity("csp_registeredapplication");
            var targetId = Guid.NewGuid();
            var inputParameters = new ParameterCollection();

            targetEntity.Id = targetId;
            inputParameters.Add("Target", targetEntity);

            _context.RegisterPluginStep<ApplicationRegistrationNumberPlugin>(new PluginStepDefinition
            {
                MessageName = "Create",
                EntityLogicalName = targetEntity.LogicalName,
                Stage = ProcessingStepStage.Preoperation
            });

            var fakePluginExecutionContext = new XrmFakedPluginExecutionContext
            {
                MessageName = "Create",
                Stage = 20,
                UserId = Guid.NewGuid(),
                PrimaryEntityName = "csp_registeredapplication",
                PrimaryEntityId = targetId,
                InputParameters = inputParameters
            };

            var entityApplicationConfiguration = new Entity("csp_autonumberconfiguration");

            entityApplicationConfiguration.Attributes.Add("csp_prefix", "SGP");
            entityApplicationConfiguration.Attributes.Add("csp_suffix", "GLO");
            entityApplicationConfiguration.Attributes.Add("csp_separator", "/");
            entityApplicationConfiguration.Attributes.Add("csp_currentnumber", 5);
            entityApplicationConfiguration.Attributes.Add("csp_configurationname", "ApplicationIDConfiguration");

            _service.Create(entityApplicationConfiguration);

            var secureConfiguration = "{\"Name\":\"WELLS\"}";
            var unsecureConfiguration = "{\"Name\":\"HYD\"}";

            _context.ExecutePluginWithConfigurations<ApplicationRegistrationNumberPlugin>(
                fakePluginExecutionContext,
                unsecureConfiguration, secureConfiguration);

            var today = DateTime.Now;
            var day = today.Day.ToString("00");
            var month = today.Month.ToString("00");
            var year = today.Year.ToString("00");
            var dateString = string.Format(@"{0}{1}{2}", year, month, day);

            var expectedRegistrationNumber = string.Format(@"{0}/{1}/{2}/{3}/{4}-{5}",
                "SGP", dateString, "GLO", "000006", "HYD", "WELLS");
            var actualRegistrationNumber = targetEntity.GetAttributeValue<string>("csp_applicationregistrationnumber");

            Assert.AreEqual<string>(expectedRegistrationNumber, actualRegistrationNumber);
        }
    }
}
