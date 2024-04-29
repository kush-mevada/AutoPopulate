using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System.ServiceModel;

namespace AutoPopulate.Custom_Workflows
{
    // Program for create clone of record when pressing the ribbon button

    /* - for custom workflow we have to add CodeActivity (Sytem.Activities), intead of plugin
       - similar to context, service provider which we get */
    public class CloneAccount : CodeActivity
    {
        /* - we also have execute function, as in plugin 
           - we get details from actContext 
           - from the custom workflow, we can define what are Input and Output Arguments (You can get some inputs and expect some output) */

        /* - way to give input 
           - uses Microsoft.Xrm.Sdk.Workflow */
        [Input("rel_cloneaccountactivation")]
        [RequiredArgument]
        public InArgument<bool> activation { get; set; }

        /* - way to give input 
           - uses Microsoft.Xrm.Sdk.Workflow */
        [Output("NewAccountId")]
        public OutArgument<string> newAccountIdOutput { get; set; }

        

        protected override void Execute(CodeActivityContext actContext)
        {
            /* - to get services */
            IWorkflowContext context = actContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory factory = actContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService tracing = actContext.GetExtension<ITracingService>();

            tracing.Trace("Execution started");

            // Get the input parameter value
            bool activateClone = activation.Get(actContext);

            // Get the current record's Id
            Guid recordId = context.PrimaryEntityId;

            try
            {
                if (activateClone)
                {
                    Guid newAccountId = CloneAccountRecord(service, recordId, tracing);

                    //context.OutputParameters["Hello"] = newAccountId;

                    // Convert Guid to string
                    string newAccountIdString = newAccountId.ToString();

                    // Set the new account ID as the output parameter
                    newAccountIdOutput.Set(actContext, newAccountIdString);

                    tracing.Trace("Execution stopped in IF part");

                }
                else
                {
                    tracing.Trace("Code executed for flag No");
                    Entity originalEntity = service.Retrieve("account", recordId, new ColumnSet("rel_cloneaccountactivation"));

                    // Check if the flag is set to "no"
                    // Update the flag to "yes"
                    originalEntity["rel_cloneaccountactivation"] = true;
                    service.Update(originalEntity);


                    // Create the cloned entity
                    Guid clonedEntityId = CloneAccountRecord(service, recordId, tracing);
                    newAccountIdOutput.Set(actContext, clonedEntityId.ToString());

                    tracing.Trace("Execution stopped in IF part");
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracing.Trace($"FaultException occurred: {ex.Message}");
                throw new InvalidPluginExecutionException($"FaultException occurred: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                tracing.Trace($"Exception occurred: {ex.Message}");
                throw new InvalidPluginExecutionException($"Exception occurred: {ex.Message}", ex);
            }

            
        }

        private Guid CloneAccountRecord(IOrganizationService service, Guid accountId, ITracingService tracing)
        {
            // Retrieve the original account record
            /*Entity originalAccount = service.Retrieve("account", accountId, new ColumnSet(
                "name", 
                "new_creditlimit", 
                "address1_line1", 
                "address1_line2", 
                "address1_line3", 
                "address1_city", 
                "address1_stateorprovince", 
                "address1_postalcode", 
                "address1_country",
                "rel_bank",
                "rel_typesofpayment",
                "rel_amountinbank",
                "rel_dateofcreation",
                "primarycontactid"
            ));*/

            Entity originalAccount = service.Retrieve("account", accountId, new ColumnSet(true));

            tracing.Trace("Here comed");


            // Create a new account entity and set its attributes
            Entity clonedAccount = new Entity("account");

            // to create different name
            /*
            clonedAccount["name"] = "Clone - " + originalAccount.GetAttributeValue<string>("name");

            clonedAccount["rel_cloneaccountactivation"] = originalAccount.GetAttributeValue<bool>("rel_cloneaccountactivation");
            clonedAccount["new_creditlimit"] = originalAccount.GetAttributeValue<int>("new_creditlimit");
            clonedAccount["address1_line1"] = originalAccount.GetAttributeValue<string>("address1_line1");
            clonedAccount["address1_line2"] = originalAccount.GetAttributeValue<string>("address1_line2");
            clonedAccount["address1_line3"] = originalAccount.GetAttributeValue<string>("address1_line3");
            clonedAccount["address1_city"] = originalAccount.GetAttributeValue<string>("address1_city");
            clonedAccount["address1_stateorprovince"] = originalAccount.GetAttributeValue<string>("address1_stateorprovince");
            clonedAccount["address1_postalcode"] = originalAccount.GetAttributeValue<string>("address1_postalcode");
            clonedAccount["address1_country"] = originalAccount.GetAttributeValue<string>("address1_country");

            // New added fields
            clonedAccount["rel_bank"] = originalAccount.GetAttributeValue<OptionSetValue>("rel_bank"); // Option Set
            clonedAccount["rel_typesofpayment"] = originalAccount.GetAttributeValue<OptionSetValueCollection>("rel_typesofpayment"); // MultiSelect Option Set
            clonedAccount["rel_amountinbank"] = originalAccount.GetAttributeValue<Money>("rel_amountinbank"); // Money Field
            clonedAccount["rel_dateofcreation"] = originalAccount.GetAttributeValue<DateTime?>("rel_dateofcreation"); // Date and Time Field
            clonedAccount["primarycontactid"] = originalAccount.GetAttributeValue<EntityReference>("primarycontactid"); // Lookup Field
            */

            foreach (var attribute in originalAccount.Attributes)
            {
                // Exclude the GUID field from being copied
                if (attribute.Key != originalAccount.LogicalName + "id")
                {
                    try
                    {
                        clonedAccount[attribute.Key] = attribute.Value;
                    }
                    catch (Exception ex)
                    {
                        tracing.Trace($"Error copying attribute '{attribute.Key}': {ex.Message}");
                        throw;
                    }
                }
            }

            if (clonedAccount.Contains("name"))
            {
                string originalName = clonedAccount.GetAttributeValue<string>("name");
                clonedAccount["name"] = "Clone - " + originalName;
            }

            // Create the cloned account record
            Guid newAccountId = service.Create(clonedAccount);

            return newAccountId;
        }
    }
}
