using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace HKPF.Workflows
{
    public class GetConfigSettingSecretariatUser : CodeActivity
    {


        //[Output("test")]
        //public OutArgument<string> Test { get; set; }
        [Output("SecretariatUser")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> SecretariatUser { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            
            string fetchQuery = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='acwapp_configurationsetting'>
    <attribute name='acwapp_configurationsettingid' />
    <attribute name='acwapp_defaultsecretariat' />
    <attribute name='acwapp_name' />
    <attribute name='createdon' />
    <order attribute='acwapp_name' descending='false' />
  </entity>
</fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchQuery));
            if (ec.Entities.Count > 0)
            {
                Entity TargetEntity = ec.Entities[0];
                if (TargetEntity.Attributes.Contains("acwapp_defaultsecretariat"))
                {
                    EntityReference er = (EntityReference)TargetEntity.Attributes["acwapp_defaultsecretariat"];

                    SecretariatUser.Set(executionContext, er);
                }
                
            }

        }
    }
}
