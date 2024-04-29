using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    public class CheckPluginExecution : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracing.Trace("Execution Started");

            // Obtaining Guid
            Guid recordId = context.PrimaryEntityId;

            try
            {
                tracing.Trace("In Try block");

                //Entity accountEntity = service.Retrieve("account", recordId, new ColumnSet( "rel_cloneaccountid"));

                //accountEntity["rel_cloneaccountid"] = "updated from plugin";

                //service.Update(accountEntity);

                Entity entity = (Entity)context.InputParameters["Target"];

                entity["rel_cloneaccountid"] = "updated from plugin";

                tracing.Trace("Execution Stopped in Try block");

                service.Update(entity);

            }
            catch (Exception ex)
            {
                tracing.Trace("Execution Stopped in catch block");
                throw new InvalidPluginExecutionException($"An error occurred in the CloneAcoountPlugin: {ex.Message}", ex);
            }
        }
    }
}
