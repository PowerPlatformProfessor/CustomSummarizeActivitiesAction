using CustomSummarizeActivitiesAction.DTO;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
                    string entityId = (string)context.InputParameters["entityId"];
                    string language = "english"; //optional

                    if ( string.IsNullOrEmpty(entityId))
                    {
                        throw new InvalidPluginExecutionException("Missing entityId dse_CustomSummarizeActivitiesAPI.");
                    }

                    var activities = GetRelatedActivities(service, entityId);

                    if(activities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("Missing completed activities");
                    }

                    var activityContext = GenerateActivitiesContext(tracingService, activities, language);

                    var gptresponse = GetCopilotResponse(service, activityContext);

                    context.OutputParameters["outputText"] = gptresponse;

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

        private DataCollection<Entity> GetRelatedActivities(IOrganizationService service, string entityId)
        {

            var query = new QueryExpression("activitypointer");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, entityId);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1);
            query.AddOrder("modifiedon", OrderType.Ascending);

            return service.RetrieveMultiple(query)?.Entities;
        }

        private string GenerateActivitiesContext(ITracingService trace, DataCollection<Entity> activities, string language)
        {
            var activitiesContext = new StringBuilder();

            activitiesContext.AppendLine($"You know the following:");
            activitiesContext.AppendLine($"\\n");

            foreach (var entity in activities)
            {
                var description = string.IsNullOrEmpty(entity.GetAttributeValue<string>("description")) ? "" :
                    Regex.Replace(entity.GetAttributeValue<string>("description"), "<.*?>", String.Empty);

                activitiesContext.AppendLine(
                    $"Activity of type: {entity.GetAttributeValue<string>("activitytypecode")} " +
                    $"completed on: {entity.GetAttributeValue<DateTime>("modifiedon")} " +
                    $"subject of activity is: {entity.GetAttributeValue<string>("subject")} " +
                    $"description of actvity is:{description} " +
                    "\\n"
                );
            }

            activitiesContext.AppendLine($"Could you please generate a short summary of this information in {language}");

            trace.Trace(activitiesContext.ToString());

            return activitiesContext.ToString();
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
                    messages = new Message[] { new Message() { datetime = DateTime.Now, user = "customer", text = inputText } },

                },
                kbarticlesodatatype = "#Collection(Microsoft.Dynamics.CRM.expando)",
                kbarticles = new Kbarticle[] {
                    new Kbarticle()
                    {
                        id = "a36e4326-4a4a-4ad9-8422-617ae0bd04da",
                        extract = "# Customer service knowledge article *Title:* How to [easy to understand title] *Task/goal:* Brief description of the task/goal to be completed *Prerequisites (if applicable):* Brief description of which products this articles applies to *Instructions [and remember that a picture is worth 1000 words]:* * Step 1 * Step 2 * Step 3 *Outcome:*Brief description of what should be possible once the task is completed *Further reading:*Links to related articles",
                        relevance = 0.03333333507180214F,
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

            return jsonResponse;

        }
    }
}
