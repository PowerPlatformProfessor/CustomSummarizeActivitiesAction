using CustomSummarizeActivitiesAction.DTO;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomSummarizeActivitiesAction
{
    public class CustomSummarizeActivitiesActionAPI : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            if (context.MessageName.Equals("dse_CustomSummarizeActivitiesAPI") && context.Stage.Equals(30))
            {

                try
                {
                    string entityid = (string)context.InputParameters["entityId"];
                    string entityType = (string)context.InputParameters["entityType"];
                    string inputText = (string)context.InputParameters["inputText"];
                    string language = (string)context.InputParameters["language"]; //optional

                    if (!string.IsNullOrEmpty(inputText))
                    {
                        var gptresponse = GetCopilotResponse(service, "You know the following:\\nFrom Customer A \\nTo Customer B\\nMessage: Could you send me a quote on the price of kebab\\n\\nFrom Customer B \\nTo Customer A\\nMessage: Sure the price of kebab is 105 sek,\\n\\nMake a summary of this conversation");
                        //Simply reversing the characters of the string
                        context.OutputParameters["outputText"] = gptresponse;
                    }
                }
                catch (Exception ex)
                {
                    tracingService.Trace("dse_CustomSummarizeActivitiesAPI: {0}", ex.ToString());
                    throw new InvalidPluginExecutionException("An error occurred in dse_CustomSummarizeActivitiesAPI.", ex);
                }
            }
            else
            {
                tracingService.Trace("dse_CustomSummarizeActivitiesAPI plug-in is not associated with the expected message or is not registered for the main operation.");
            }
        }

        private string GetCopilotResponse(IOrganizationService service, string inputText)
        {

            
            var requestPayload = new RequestPayload()
            {
                odatatype = "#Microsoft.Dynamics.CRM.expando",
                version = "3.0",
                conversation = new Conversation()
                {
                    odatatype = "#Microsoft.Dynamics.CRM.expando",
                    messagesodatatype = "#Collection(Microsoft.Dynamics.CRM.expando)",
                    messages = new Message[] { new Message() { datetime = DateTime.Now, user="customer", text = inputText} },
                    
                },
                kbarticlesodatatype = "#Collection(Microsoft.Dynamics.CRM.expando)",
                kbarticles = new Kbarticle[] {
                    new Kbarticle()
                    {
                        id = "a36e4326-4a4a-4ad9-8422-617ae0bd04da",
                        extract = "# Customer service knowledge article *Title:* How to [easy to understand title] *Task/goal:* Brief description of the task/goal to be completed *Prerequisites (if applicable):* Brief description of which products this articles applies to *Instructions [and remember that a picture is worth 1000 words]:* * Step 1 * Step 2 * Step 3 *Outcome:*Brief description of what should be possible once the task is completed *Further reading:*Links to related articles",
                        relevance = 0.00001F,
                        title ="Customer Service Trial article",
                        source="internal_kb"
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestPayload);

            var request = new OrganizationRequest("msdyn_InvokeIntelligenceAction")
            {
                ["RequestPayload"] = json,
                ["ScenarioType"] = "AgentAssistGPT"
            };

            var response = service.Execute(request);

            var jsonResponse = (string)response.Results["Result"];

            //msdyn_InvokeIntelligenceActionResponse responseObj = JsonSerializer.Deserialize<msdyn_InvokeIntelligenceActionResponse>(jsonResponse);

            //dynamic suggestionAttribure = responseObj.Results.FirstOrDefault(resp => resp.Key == "responsev2")
            //    ?.Value?.Attributes?.FirstOrDefault(attr => attr.Key == "suggestions");

            //string text = (string)((List<dynamic>)suggestionAttribure.Value.Entities.Attributes).FirstOrDefault(attr => attr.Key == "sub_context")?.Value;

            return jsonResponse;

        }
    }
}
