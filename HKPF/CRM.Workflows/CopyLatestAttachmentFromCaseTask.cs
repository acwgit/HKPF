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
    public class CopyLatestAttachmentFromCaseTask : CodeActivity
    {
        #region Workflow Parameters

        [RequiredArgument]
        [Input("Source Entity URL")]
        public InArgument<string> SourceEntityURL { get; set; }

        [RequiredArgument]
        [Input("Target Entity Logical Name")]
        public InArgument<string> TargetEntityLogicalName { get; set; }

        [RequiredArgument]
        [Input("Target Entity URL")]
        public InArgument<string> TargetEntityURL { get; set; }

        #endregion Input Parameters


        protected override void Execute(CodeActivityContext executionContext)
        {
            #region Manage Tracing and Organization Service


            // Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService orgService = serviceFactory.CreateOrganizationService(context.UserId);

            #endregion

            try
            {
                //var sourceAttribute = this.SourceAttributeName.Get(executionContext);
                var targetEntity = this.TargetEntityURL.Get(executionContext);
                var sourceEntity = this.SourceEntityURL.Get(executionContext);
                var targetEntityLogName = this.TargetEntityLogicalName.Get(executionContext);
                
                string targetEntityID = GetRecordID(targetEntity);
                string sourceEntityID = GetRecordID(sourceEntity);


                // Fetch all Notes and Attachment for associated Leave Request
                string fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='annotation'>
    <attribute name='subject' />
    <attribute name='notetext' />
    <attribute name='filename' />
    <attribute name='documentbody' />
    <attribute name='mimetype' />
    <attribute name='annotationid' />

    <order attribute='createdon' descending='true' />
    <filter type='and'>
      <condition attribute='isdocument' operator='eq' value='1' />
    </filter>
    <link-entity name='acwapp_casetasks' from='acwapp_casetasksid' to='objectid' link-type='inner' alias='ah'>
      <filter type='and'>
        <condition attribute='acwapp_casetasksid' operator='eq'  uitype='acwapp_casetasks' value='" + sourceEntityID + "' />"+
      @"</filter>
    </link-entity>
  </entity>
</fetch>";

                EntityCollection NotesRetrieve = orgService.RetrieveMultiple(new FetchExpression(fetchxml));
                if (NotesRetrieve.Entities.Count > 0)
                {
                    string createdon = ((DateTime)NotesRetrieve.Entities[0].Attributes["createdon"]).ToString("yyyyMMddHHmm");
                    foreach (Entity entity in NotesRetrieve.Entities)
                    {
                        string createdon1 = ((DateTime)entity.Attributes["createdon"]).ToString("yyyyMMddHHmm");
                        if(createdon1 == createdon)
                        {
                            Entity note = new Entity("annotation");
                            note["objectid"] = new EntityReference(targetEntityLogName, new Guid(targetEntityID));
                            note["objecttypecode"] = targetEntityLogName;// Associate Notes to Request Response

                            if (entity.Attributes.Contains("isdocument"))
                                note.Attributes["isdocument"] = entity.Attributes["isdocument"];

                            if (entity.Attributes.Contains("notetext"))
                                note.Attributes["notetext"] = entity.Attributes["notetext"];

                            if (entity.Attributes.Contains("filename"))
                                note.Attributes["filename"] = entity.Attributes["filename"];

                            if (entity.Attributes.Contains("documentbody"))
                                note.Attributes["documentbody"] = entity.Attributes["documentbody"];

                            if (entity.Attributes.Contains("mimetype"))
                                note.Attributes["mimetype"] = entity.Attributes["mimetype"];

                            orgService.Create(note);
                        }
                        //Entity entity = NotesRetrieve.Entities[0];
                        
                    }


                }
                    
                
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString(), ex);
            }
        }
        public string GetRecordID(string recordURL)
        {

            if (recordURL == null || recordURL == "")
            {
                return "";
            }
            string[] urlParts = recordURL.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string objectTypeCode = urlParams[0].Replace("etc=", "");
            //  entityName =  sGetEntityNameFromCode(objectTypeCode, service);
            string objectId = urlParams[1].Replace("id=", "");
            return objectId;
        }

    }
    
}
