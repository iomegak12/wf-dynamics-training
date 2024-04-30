using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedivacPlugins
{
    public class FeesCalculatorWFActivity : CodeActivity
    {
        private const string CustomEntityName = "csp_registeredapplication";

        protected override void Execute(CodeActivityContext executionContext)
        {
            var currentRegisteredApplication = this.CurrentApplication.Get(executionContext);

            if (currentRegisteredApplication == null)
            {
                throw new InvalidOperationException("Current Registered Application Must reference a valid Application Entity!");
            }

            var workflowExecutionContext = executionContext?.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext?.GetExtension<IOrganizationServiceFactory>();
            var organizationService = serviceFactory?.CreateOrganizationService(workflowExecutionContext.InitiatingUserId);
            var tracingService = executionContext?.GetExtension<ITracingService>();

            var validation = workflowExecutionContext != null &&
                serviceFactory != null && organizationService != null && tracingService != null;

            if (!validation)
                throw new InvalidOperationException("Current Workflow Execution Context Not Valid!");

            Entity applicationEntity;
            {
                var retrieveRequest = new RetrieveRequest
                {
                    ColumnSet = new ColumnSet(new string[] { "csp_course", "csp_university", "csp_transportationfees" }),
                    Target = currentRegisteredApplication
                };

                var retrieveResponse = (RetrieveResponse)organizationService.Execute(retrieveRequest);

                applicationEntity = retrieveResponse.Entity as Entity;
            };

            validation = applicationEntity != null;

            if (!validation)
                throw new InvalidOperationException("Invalid Application Entity Details Specified!");


            Entity universityEntity;
            {
                var universityId = (EntityReference)applicationEntity["csp_university"];
                var retrieveRequest = new RetrieveRequest
                {
                    ColumnSet = new ColumnSet(new string[] { "csp_universityfees" }),
                    Target = universityId
                };

                var retrieveResponse = (RetrieveResponse)organizationService.Execute(retrieveRequest);

                universityEntity = retrieveResponse.Entity as Entity;
            }

            Entity courseEntity;
            {
                var courseId = (EntityReference)applicationEntity["csp_course"];

                var retrieveRequest = new RetrieveRequest
                {
                    ColumnSet = new ColumnSet(new string[] { "csp_tuitionfees" }),
                    Target = courseId
                };

                var retrieveResponse = (RetrieveResponse)organizationService.Execute(retrieveRequest);

                courseEntity = retrieveResponse.Entity as Entity;
            }

            validation = universityEntity != null && courseEntity != null;

            if (!validation)
                throw new InvalidOperationException("Invalid University and Course Details Specified!");

            var universityFees = (Money)universityEntity["csp_universityfees"];
            var tuitionFees = (Money)courseEntity["csp_tuitionfees"];
            var transportationCharges = (Money)applicationEntity["csp_transportationfees"];

            if (transportationCharges.Value < 0 || transportationCharges == default(Money))
            {
                transportationCharges = new Money(0);
            }

            var totalFees = new Money(universityFees.Value +
                tuitionFees.Value + transportationCharges.Value);

            this.TuitionFees.Set(executionContext, tuitionFees);
            this.UniversityFees.Set(executionContext, universityFees);
            this.TotalFees.Set(executionContext, totalFees);

            tracingService.Trace("Current Registration Application Fees Calculation Completed Successfully ...");

            try
            {
                if (this.UpdateEntity != null &&
                        this.UpdateEntity.Get(executionContext) == true)
                {
                    var updateEntity = new Entity(currentRegisteredApplication.LogicalName);

                    updateEntity["csp_registeredapplicationid"] = applicationEntity["csp_registeredapplicationid"];
                    updateEntity["csp_tuitionfees"] = this.TuitionFees.Get(executionContext);
                    updateEntity["csp_universityfees"] = this.UniversityFees.Get(executionContext);
                    updateEntity["csp_totalfees"] = this.TotalFees.Get(executionContext);

                    organizationService.Update(updateEntity);
                }

                tracingService.Trace("Current Registration Application Updated Successfully ...");
            }
            catch (Exception exceptionObject)
            {
                tracingService.Trace("Exception Occurred, Details : " + exceptionObject.Message);

                throw;
            }
        }

        [Input("Current Registered Application")]
        [ReferenceTarget(CustomEntityName)]
        public InArgument<EntityReference> CurrentApplication { get; set; }

        [Input("Update Entities Fees")]
        [Default("true")]
        public InArgument<bool> UpdateEntity { get; set; }

        [Output("Tuition Fees")]
        [AttributeTarget(CustomEntityName, "csp_tuitionfees")]
        public OutArgument<Money> TuitionFees { get; set; }

        [Output("University Fees")]
        [AttributeTarget(CustomEntityName, "csp_universityfees")]
        public OutArgument<Money> UniversityFees { get; set; }

        [Output("Total Fees")]
        [AttributeTarget(CustomEntityName, "csp_totalfees")]
        public OutArgument<Money> TotalFees { get; set; }
    }
}
