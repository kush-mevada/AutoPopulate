using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoPopulate.Plugins
{
    public class contacts : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
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
                                    "address1_city", 
                                    "address1_line1", 
                                    "address1_line2", 
                                    "address1_line3",
                                    "address1_postalcode", 
                                    "address1_country", 
                                    "address1_stateorprovince"
                                ));

                                // Update the corresponding fields in the Contact record
                                Entity contactToUpdate = new Entity("contact");
                                contactToUpdate.Id = entity.Id;

                                // Copy address-related fields from Account to Contact
                                contactToUpdate["address1_line1"] = account.GetAttributeValue<string>("address1_line1");
                                contactToUpdate["address1_line2"] = account.GetAttributeValue<string>("address1_line2");
                                contactToUpdate["address1_line3"] = account.GetAttributeValue<string>("address1_line3");
                                contactToUpdate["address1_city"] = account.GetAttributeValue<string>("address1_city");
                                contactToUpdate["address1_postalcode"] = account.GetAttributeValue<string>("address1_postalcode");
                                contactToUpdate["address1_country"] = account.GetAttributeValue<string>("address1_country");
                                contactToUpdate["address1_stateorprovince"] = account.GetAttributeValue<string>("address1_stateorprovince");

                                // Perform the update
                                service.Update(contactToUpdate);
                            }
                        }
                    }
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin" + ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

    }
}