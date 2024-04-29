using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    public class CreditLimit : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Check if the plugin is triggered on the creation of a contact.
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName.ToLower() == "contact")
            {
                // Check if the target entity contains the account lookup field.
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity contactEntity = (Entity)context.InputParameters["Target"];

                    // Check if the contact is associated with an account.
                    if (contactEntity.Attributes.Contains("parentcustomerid") && contactEntity["parentcustomerid"] is EntityReference)
                    {
                        EntityReference accountRef = (EntityReference)contactEntity["parentcustomerid"];

                        // Obtain the organization service from the service provider.
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                        // Retrieve the associated account's credit limit.
                        // Assuming "account" is the schema name of your Account entity.
                        ColumnSet columnSet = new ColumnSet("new_creditlimit");
                        Entity account = service.Retrieve("account", accountRef.Id, columnSet);

                        // Check if the credit limit is 0.
                        if (account.Contains("new_creditlimit") && account["new_creditlimit"] != null)
                        {
                            // Convert the credit limit to int for comparison.
                            int creditLimit = ((int)account["new_creditlimit"]);

                            // Check if the credit limit is 0.
                            if (creditLimit == 0)
                            {
                                throw new InvalidPluginExecutionException("Contact creation is not allowed for accounts with 0 credit limit.");
                            }
                        }
                        else
                        {
                            // Handle the case where the credit limit is null or not present.
                            throw new InvalidPluginExecutionException("Contact creation is not allowed for accounts with undefined credit limit.");
                        }
                    }
                }
            }
        }
    }
}
