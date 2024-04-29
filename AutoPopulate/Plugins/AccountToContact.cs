using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace AutoPopulate.Plugins
{
    public class AccountToContact : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                Guid recordId = context.PrimaryEntityId;
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                Entity accountEntity = service.Retrieve("account",recordId, new ColumnSet(
                    "new_creditlimit",
                    "name",
                    "address1_line1"
                ));

                tracing.Trace("Execution Started");


                int creditLimit = accountEntity.GetAttributeValue<int>("new_creditlimit");
                tracing.Trace("credit limit : " + creditLimit);

                string accountName = accountEntity.GetAttributeValue<string>("name");
                tracing.Trace("account name : " + accountName);
                string accountAdd = accountEntity.GetAttributeValue<string>("address1_line1");
                tracing.Trace("account email : " + accountAdd);

                if (creditLimit > 1000)
                {
                    tracing.Trace("In if block");

                    accountEntity["new_creditlimit"] = 1000;

                    // Create a new contact record
                    Entity contactEntity = new Entity("contact");
                    contactEntity["lastname"] = accountName; 
                    contactEntity["address1_line1"] = accountAdd; 

                    Guid contactId =  service.Create(contactEntity);
                    tracing.Trace("contactID : " + contactId);

                    tracing.Trace("Execution Stopped in IF part");
                    service.Update(accountEntity);
                }
                else if (creditLimit < 1000)
                {
                    tracing.Trace("1");
                    accountEntity["new_creditlimit"] = 1000;

                    tracing.Trace("2");
                    Entity contactEntity = new Entity("contact");
                    contactEntity["lastname"] = accountName;
                    contactEntity["address1_line1"] = accountAdd;

                    tracing.Trace("4");
                    service.Update(accountEntity);

                    tracing.Trace("3");
                    Guid contactId = service.Create(contactEntity);
                    tracing.Trace("contactID : " + contactId);

                    tracing.Trace("Execution Stopped in ELSE part");
                }
                /*else
                {
                    tracing.Trace("1");
                    accountEntity["new_creditlimit"] = 1000;

                    tracing.Trace("2");
                    Entity contactEntity = new Entity("contact");
                    contactEntity["lastname"] = accountName;
                    contactEntity["address1_line1"] = accountAdd;

                    tracing.Trace("4");                   
                    service.Update(accountEntity);

                    tracing.Trace("3");
                    Guid contactId = service.Create(contactEntity);
                    tracing.Trace("contactID : " + contactId);

                    tracing.Trace("Execution Stopped in ELSE part");
                }*/
            }
            catch (Exception ex)
            {
                tracing.Trace("In catch block execution stopped");
                throw new InvalidPluginExecutionException("An error occurred in the plugin: " + ex.Message, ex);
            }
        }

    }
}
