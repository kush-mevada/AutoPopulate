using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    public class custom_new_api : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IOrganizationService crmService = null;
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.MessageName.Equals("rel_testapi"))
            {
                try
                {
                    tracing.Trace("Execution started");

                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    crmService = serviceFactory.CreateOrganizationService(context.UserId);

                    // retrieve the inputs
                    string accountname = (string)context.InputParameters["accountname"];
                    string accountId = (string)context.InputParameters["parentaccountid"];
                    tracing.Trace("accountname : " + accountname);
                    tracing.Trace("parentaccountid : " + accountId);

                    // create new account from the inputs
                    Entity createAccount = new Entity("account");
                    createAccount["name"] = accountname;
                    createAccount["parentaccountid"] = new EntityReference("account", Guid.Parse(accountId));
                    tracing.Trace("account created");

                    Guid accId = crmService.Create(createAccount);

                    context.OutputParameters["apiresponse"] = accId.ToString();
                    tracing.Trace("Execution completed");
                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
