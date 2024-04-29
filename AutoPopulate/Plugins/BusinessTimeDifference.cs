using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    public class BusinessTimeDifference : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("Execution started");

            if (context.MessageName.ToLower() == "update" && context.Stage == 40)
            {
                tracingService.Trace("Step 1");

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    tracingService.Trace("Step 2");

                    DateTime date3 = (DateTime)entity.Attributes["rel_startdate"];
                    tracingService.Trace("date1" + date3);
                    DateTime date4 = (DateTime)entity.Attributes["rel_enddate"];
                    tracingService.Trace("date2" + date4);

                    if (entity.Attributes.Contains("rel_startdate") && entity.Attributes.Contains("rel_enddate"))
                    {
                        tracingService.Trace("Step 3");

                        DateTime? date1 = entity.GetAttributeValue<DateTime?>("rel_startdate");
                        tracingService.Trace("date1" + date1);
                        DateTime? date2 = entity.GetAttributeValue<DateTime?>("rel_enddate");
                        tracingService.Trace("date2" + date2);

                        if (date1 != null && date2 != null)
                        {
                            TimeSpan difference = date2.Value - date1.Value;
                            tracingService.Trace("Difference between dates: " + difference.TotalDays);
                        }

                        //TimeSpan difference = date2 - date1;
                        //tracingService.Trace("Difference between dates: " + difference.TotalDays);
                    }
                    else
                    {
                        tracingService.Trace("Else part");
                    }
                }
            }
        }
    }
}
