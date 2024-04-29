using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Plugins
{
    public class PostUpdateContact : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    Entity entity = (Entity)context.InputParameters["Target"];  // Updated Value (ALL VALUES)
                    Entity preImage = (Entity)context.PreEntityImages["PreImage"];  // Previous Value
                    Entity postImage = (Entity)context.PostEntityImages["PostImage"]; // Updated Value (SELECT RESPECTIVE)

                    Entity updateContact = new Entity("contact");
                    updateContact.Id = entity.Id;

                    //not working when i give jobtitle empty
                    //updateContact["new_previousjobtitle"] = preImage.Attributes["jobtitle"];
                    //updateContact["new_newjobtitle"] = postImage.Attributes["jobtitle"];

                    if (preImage.Attributes.Contains("jobtitle"))
                    {
                        updateContact["new_previousjobtitle"] = preImage.Attributes["jobtitle"];
                    }

                    if (postImage.Attributes.Contains("jobtitle"))
                    {
                        updateContact["new_newjobtitle"] = postImage.Attributes["jobtitle"];
                    }

                    service.Update(updateContact);
                }
                catch (InvalidPluginExecutionException ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message.ToString());
                }
            }
        }
    }
}
