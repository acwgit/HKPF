using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.IO;
using System.Net;
using System.Xml;

namespace HKPF.Workflows
{
    public class SendSMS : CodeActivity
    {
        #region Workflow Parameters

        [Input("Gateway Url")]
        [RequiredArgument]
        public InArgument<string> _gatewayUrl { get; set; }



        [RequiredArgument]
        [Input("Gateway Username")]
        public InArgument<string> _gatewayUsername { get; set; }



        [Input("Gateway Password")]
        [RequiredArgument]
        public InArgument<string> _gatewayPassword { get; set; }



        [Input("Country Code")]
        [RequiredArgument]
        public InArgument<string> _countryCode { get; set; }



        [Input("phonenumber")]
        [RequiredArgument]
        public InArgument<string> _phonenumber { get; set; }



        [Input("Sender Id")]
        [RequiredArgument]
        public InArgument<string> _senderId { get; set; }



        [RequiredArgument]
        [Input("Content")]
        public InArgument<string> _content { get; set; }



        [Output("Success")]
        public OutArgument<bool> _isSuccess { get; set; }



        [Output("Response Text")]
        public OutArgument<string> _ResponseText { get; set; }

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
              
                ITracingService tracingService = executionContext.GetExtension<ITracingService>();
                string requestUriString = this._gatewayUrl.Get(executionContext);
                string str1 = this._gatewayUsername.Get(executionContext);
                string str2 = this._gatewayPassword.Get(executionContext);
                string str3 = this._countryCode.Get(executionContext);
                string str4 = this._phonenumber.Get(executionContext);
                string str5 = this._senderId.Get(executionContext);
                tracingService.Trace("Request XML: " + this._content.Get(executionContext));
                string str6 = EscapeXml(this._content.Get(executionContext));
                string str7 = string.Format("<?xml version='1.0' encoding='UTF-8'?><sendrequest><correlationid>123</correlationid><username>{0}</username><password>{1}</password><messages><message><scheduledatetime>{2}</scheduledatetime><phonenumbers>{3}</phonenumbers><content><![CDATA[{4}]]></content><senderid>{5}</senderid><starttime>0</starttime><endtime>24</endtime></message></messages></sendrequest>", str1, str2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), (str3.Trim() + str4.Trim()), str6, str5);
                tracingService.Trace("Request XML: " + str7);
                //tracingService.Trace("str7: ", str7);
                //tracingService.Trace("", new object[0]);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                byte[] bytes = Encoding.UTF8.GetBytes(string.Format("sendrequest={0}", str7));
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);
                string end = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
                tracingService.Trace("Response XML:" + end);
                //tracingService.Trace(end, new object[0]);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(end);
                XmlNode xmlNode = xmlDocument.SelectSingleNode("/sendresponse/statuscode");
                this._isSuccess.Set(executionContext, xmlNode.InnerXml == "0");
                this._ResponseText.Set(executionContext, end);


            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString(), ex);
            }
        }
        public string EscapeXml(string requestxml)
        {

            return requestxml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("\'", "&apos;").Replace("'", "&apos;");
        }

    }

}
