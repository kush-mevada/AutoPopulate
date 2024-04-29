using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugin_Base
{
    public abstract class PluginBase : IPlugin
    {
        protected class LocalPluginExecution
        {
            internal IServiceProvider ServiceProvider { get; set; }
            internal IOrganizationServiceFactory OrganizationServiceFactory { get; set; }
            internal IOrganizationService orgService { get; set; }
            internal IPluginExecutionContext pluginContext { get; set; }
            internal ITracingService tracingService { get; set; }
        }
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException("Error");
            }
        }
    }
}
