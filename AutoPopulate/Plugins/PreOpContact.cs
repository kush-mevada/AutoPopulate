using System;
using System.Runtime.Remoting.Services;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoPopulate.Plugins
{
    public class PreOpContact : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("Execution started");

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Plug-in business logic goes here.  
                    if (entity.LogicalName.Equals("contact", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Check if the entity is a Contact record
                        if (context.MessageName.ToLower() == "create" || context.MessageName.ToLower() == "update")
                        {
                            // Retrieve the value of the 'parentcustomerid' field
                            if (entity.Attributes.Contains("parentcustomerid") && entity["parentcustomerid"] is EntityReference)
                            {
                                EntityReference accountReference = (EntityReference)entity["parentcustomerid"];

                                // Retrieve the related Account record to get the address-related fields
                                Entity account = service.Retrieve("account", accountReference.Id, new ColumnSet(
                                    "address1_line1", "address1_line2", "address1_line3", "address1_city", "address1_stateorprovince",
                                    "address1_postalcode", "address1_country"
                                ));

                                // Update the corresponding fields in the Contact record
                                Entity contactToUpdate = new Entity("contact");
                                Entity existingContact = service.Retrieve("contact", entity.Id, new ColumnSet(true));
                                contactToUpdate.Id = entity.Id;

                                // Copy address-related fields from Account to Contact
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_line1", tracingService);
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_line2", tracingService);
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_line3", tracingService);
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_city", tracingService);
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_stateorprovince", tracingService);
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_postalcode", tracingService);
                                CopyIfDifferent(existingContact, account, contactToUpdate, "address1_country", tracingService);

                                // Perform the update
                                service.Update(contactToUpdate);
                            }
                        }
                    }
                    tracingService.Trace("Execution stopped");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

        private void CopyIfDifferent(Entity existingContact, Entity account, Entity contactToUpdate, string attributeName, ITracingService tracingService)
        {
            if (account.Attributes.Contains(attributeName))
            {
                string accountValue = account.GetAttributeValue<string>(attributeName);
                string contactValue = existingContact.GetAttributeValue<string>(attributeName);

                if (accountValue != contactValue)
                {
                    contactToUpdate[attributeName] = accountValue;
                    tracingService.Trace("Updated contact field '{0}' to '{1}'", attributeName, accountValue);
                }
            }
        }
    }
}
