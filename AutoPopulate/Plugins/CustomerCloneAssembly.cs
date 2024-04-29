using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;

namespace AutoPopulate.Plugins
{
    public class CustomerCloneAssembly : CodeActivity
    {
        [Output("NewAccountId")]
        public OutArgument<string> newAccountIdOutput { get; set; }
        protected override void Execute(CodeActivityContext actContext)
        {
            // Get services
            IWorkflowContext context = actContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory factory = actContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService tracing = actContext.GetExtension<ITracingService>();

            // Get the current record's Id
            Guid recordId = context.PrimaryEntityId;

            // Clone the account record
            Guid newAccountId = CloneAccountRecord(service, recordId);

            string newAccountIdString = newAccountId.ToString();

            newAccountIdOutput.Set(actContext, newAccountIdString);

            tracing.Trace("Here comed");

            // You can use the clonedAccountId as needed
        }

        private Guid CloneAccountRecord(IOrganizationService service, Guid accountId)
        {
            // Retrieve the existing account record
            Entity existingAccount = service.Retrieve("rel_customer", accountId, new ColumnSet(
                "rel_name",
                "rel_email",
                "rel_gender",
                "rel_vehiclesusing",
                "rel_income",
                "rel_dateofbirth",
                "ownerid",
                "rel_serviceproviderid",
                "rel_address",
                "rel_phonenumber"
            ));

            // Create a new account record with the same attribute values
            Entity clonedAccount = new Entity("rel_customer");

            clonedAccount["rel_name"] = "Clone - " + existingAccount.GetAttributeValue<string>("rel_name");

            clonedAccount["rel_email"] = existingAccount.GetAttributeValue<string>("rel_email");
            clonedAccount["rel_gender"] = existingAccount.GetAttributeValue<OptionSetValue>("rel_gender"); // Option Set
            clonedAccount["rel_vehiclesusing"] = existingAccount.GetAttributeValue<OptionSetValueCollection>("rel_vehiclesusing"); // MultiSelect Option Set
            clonedAccount["rel_income"] = existingAccount.GetAttributeValue<Money>("rel_income"); // Money Field
            clonedAccount["rel_dateofbirth"] = existingAccount.GetAttributeValue<DateTime?>("rel_dateofbirth"); // Date and Time Field
            clonedAccount["ownerid"] = existingAccount.GetAttributeValue<EntityReference>("ownerid"); // Lookup Field
            clonedAccount["rel_serviceproviderid"] = existingAccount.GetAttributeValue<EntityReference>("rel_serviceproviderid"); // Lookup Field

            clonedAccount["rel_address"] = existingAccount.GetAttributeValue<string>("rel_address"); // Multiline Text Field
            clonedAccount["rel_phonenumber"] = existingAccount.GetAttributeValue<int?>("rel_phonenumber"); // Whole Number Field

            service.Create(clonedAccount);
            // Create the new record
            Guid clonedAccountId = service.Create(clonedAccount);

            return clonedAccountId;
        }
    }
}
