using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    //
    public class NotUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Check if the entity is a Contact record
                    if (entity.LogicalName.Equals("contact", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Check if the message is 'Create' or 'Update'
                        if (context.MessageName.ToLower() == "create" || context.MessageName.ToLower() == "update")
                        {
                            // Retrieve the value of the 'parentcustomerid' field
                            if (entity.Attributes.Contains("parentcustomerid") && entity["parentcustomerid"] is EntityReference)
                            {
                                EntityReference accountReference = (EntityReference)entity["parentcustomerid"];

                                // Check if PreEntityImages is not null and contains the key "address_details"
                                if (context.PreEntityImages != null && context.PreEntityImages.Contains("address_details"))
                                {
                                    Entity preImage = context.PreEntityImages["address_details"];

                                    // Retrieve the related Account record to get the address-related fields
                                    // Update the corresponding fields in the Contact record
                                    Entity contactToUpdate = new Entity("contact");
                                    contactToUpdate.Id = entity.Id;

                                    // Copy address-related fields from pre-image to Contact even if data is the same 
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_line1", tracingService);
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_line2", tracingService);
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_line3", tracingService);
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_city", tracingService);
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_stateorprovince", tracingService);
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_postalcode", tracingService);
                                    UpdateIfDifference(preImage, entity, contactToUpdate, "address1_country", tracingService);

                                    // Perform the update
                                    service.Update(contactToUpdate);
                                }
                            }
                        }
                    }
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

        //mention details
        private void UpdateIfDifference(Entity preImage, Entity account, Entity contactToUpdate, string attributeName, ITracingService tracingService)
        {
            string functionName = "UpdateIfDifference";
            tracingService.Trace("into "+ functionName);

            if (account.Attributes.Contains(attributeName) && preImage.Attributes.Contains(attributeName))
            {
                string accountValue = account.GetAttributeValue<string>(attributeName);
                string preImageValue = preImage.GetAttributeValue<string>(attributeName);

                if (preImageValue == null)
                {
                    contactToUpdate[attributeName] = accountValue;
                    tracingService.Trace("updated");
                }
                if (accountValue != preImageValue)
                {
                    contactToUpdate[attributeName] = accountValue;
                    tracingService.Trace("Updated contact field '{0}' to '{1}'", attributeName, accountValue);
                }
                
            }
        }
    }
}



