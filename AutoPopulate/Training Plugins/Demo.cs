using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulate.Training_Plugins
{
    public class Demo : IPlugin
    {
        /* to use configs, we have to create global variables */
        string secure = null, unsecure = null;

        /* - to pass configs, we have to create Constructor
           - with that, we have to pass configs as parameter to pass values */
        public Demo(string unsecureCfg, string secureCfg) 
        {
            secure = secureCfg;
            unsecure = unsecureCfg;
        }
        public void Execute(IServiceProvider serviceProvider)
        {
            /* - serviceProvider -> having all the details of steps, entity, pipeline stages 
               - we can get all the details of serviceProvider
               - details will be stored in crud format 
               - we have to extract all the things from provider 
               - First step to extract is below 
               - this 'context' will contain all the input parameter of our plugin
               - like -> message, user, primary entity, operation */
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            /* - serviceFactory contains all the user details  */
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            /* - this is the output (input from context) 
               - using service we can do CRUD operations 
               - we have to create service using 'context.UserId' 
               - passing which user ('context.UserId') we need to create service 
               - passed user based security role based service will be created (on runtime) */
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            /* - 'Target' keyword will give you triggering record information 
               - the record which particularly triggered plugin */
            Entity target = (Entity)context.InputParameters["Target"];

            /* - how to get value from target (Method 1) 
               - in this syntax, if you didnt give any target field than it will consider the default value */
            string name = target.GetAttributeValue<string>("name"); 
            int credit = target.GetAttributeValue<int>("new_creditlimit"); 
            string website = target.GetAttributeValue<string>("websiteurl");

            /* - how to get value from target (Method 2) 
               - in this syntax, if you didnt give any target field than it will throw an error that 'error occurred from ISV code.' 
               - You can get meaningful error with 'try catch' block at higher level 
               - having try catch block is good practise */
            /* string name2 = (string)target["name"];
            int credit2 = (int)target["new_creditlimit"];
            string website2 = (string)target["websiteurl"]; */

            /* - special data types of crm 
               - Money (.Value necessery), OptionSetValue (.Value necessery), OptionSetValueCollection (.Count necessery), Lookup -> EntityRefrence (.LogicalName or .Id or .Name necessery) 
               - Will throw an error if we dont give value (null.Value doesnt make sense) */
            EntityReference contact = target.GetAttributeValue<EntityReference>("primarycontactid");


            throw new InvalidPluginExecutionException("contact : " + contact.LogicalName + " " + "ID : " + contact.Id + " " + "Name : " + contact.Name);


            

        }
    }
}
