using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    public class CloneAcoountPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service from the service provider
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Obtain the tracing service from the service provider
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                tracing.Trace("started");

                // Get the input parameter value
                bool activateClone = (bool)context.InputParameters["Flag"];
                int creditlimit = (int)context.InputParameters["CreLim"];
                tracing.Trace("credit limit : " + creditlimit);

                // Get the current record's Id
                Guid recordId = context.PrimaryEntityId;

                if (activateClone)
                {
                    tracing.Trace("in if block");
                    Guid newAccountId = CloneAccountRecord(service, recordId);

                    if(creditlimit == 0)
                    {
                        tracing.Trace("here comed == 0");
                        throw new InvalidPluginExecutionException("Credit limit should not be zero.");
                    }

                    // Set the new account ID as the output parameter
                    context.OutputParameters["CloneID"] = newAccountId.ToString();
                }
                else
                {
                    tracing.Trace("Code executed for flag No");

                    // Retrieve the original account record
                    Entity originalEntity = service.Retrieve("account", recordId, new ColumnSet("rel_cloneaccountactivation"));

                    // Check if the flag is set to "no"
                    if (!(originalEntity.GetAttributeValue<bool>("rel_cloneaccountactivation")))
                    {
                        // Update the flag to "yes"
                        originalEntity["rel_cloneaccountactivation"] = true;
                        service.Update(originalEntity);

                        // Create the cloned entity
                        Guid clonedEntityId = CloneAccountRecord(service, recordId);
                        context.OutputParameters["CloneID"] = clonedEntityId.ToString();
                    }

                    //throw new InvalidPluginExecutionException("Else Part");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here or log them using the tracing service
                throw new InvalidPluginExecutionException($"An error occurred in the CloneAcoountPlugin: {ex.Message}", ex);
            }

        }

        private Guid CloneAccountRecord(IOrganizationService service, Guid accountId)
        {
            try
            {
                // Retrieve the original account record
                Entity originalAccount = service.Retrieve("account", accountId, new ColumnSet(
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
                    "primarycontactid",
                    "new_creditlimit"
                 ));

                // Create a new account entity and set its attributes
                Entity clonedAccount = new Entity("account");

                // to create a different name
                clonedAccount["name"] = "Clone - " + originalAccount.GetAttributeValue<string>("name");

                clonedAccount["rel_cloneaccountactivation"] = originalAccount.GetAttributeValue<bool>("rel_cloneaccountactivation");
                clonedAccount["new_creditlimit"] = originalAccount.GetAttributeValue<int>("new_creditlimit");
                clonedAccount["address1_line1"] = originalAccount.GetAttributeValue<string>("address1_line1");
                clonedAccount["address1_line2"] = originalAccount.GetAttributeValue<string>("address1_line2");
                clonedAccount["address1_line3"] = originalAccount.GetAttributeValue<string>("address1_line3");
                clonedAccount["address1_city"] = originalAccount.GetAttributeValue<string>("address1_city");
                clonedAccount["address1_stateorprovince"] = originalAccount.GetAttributeValue<string>("address1_stateorprovince");
                clonedAccount["address1_postalcode"] = originalAccount.GetAttributeValue<string>("address1_postalcode"); // Change to string
                clonedAccount["address1_country"] = originalAccount.GetAttributeValue<string>("address1_country");

                // New added fields
                clonedAccount["rel_bank"] = originalAccount.GetAttributeValue<OptionSetValue>("rel_bank"); // Option Set
                clonedAccount["rel_typesofpayment"] = originalAccount.GetAttributeValue<OptionSetValueCollection>("rel_typesofpayment"); // MultiSelect Option Set
                clonedAccount["rel_amountinbank"] = originalAccount.GetAttributeValue<Money>("rel_amountinbank"); // Money Field
                clonedAccount["rel_dateofcreation"] = originalAccount.GetAttributeValue<DateTime?>("rel_dateofcreation"); // Date and Time Field
                clonedAccount["primarycontactid"] = originalAccount.GetAttributeValue<EntityReference>("primarycontactid"); // Lookup Field

                // Create the cloned account record
                Guid newAccountId = service.Create(clonedAccount);

                return newAccountId;
            }
            catch (Exception ex)
            {
                // Handle exceptions here or log them using the tracing service
                throw new InvalidPluginExecutionException($"An error occurred in the CloneAccountRecord: {ex.Message}", ex);
            }
        }
    }
}
