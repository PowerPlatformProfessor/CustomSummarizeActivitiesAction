using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (context.MessageName.Equals("sample_CustomAPIExample") && context.Stage.Equals(30))
            {

                try
                {
                    string entityid = (string)context.InputParameters["StringParameter"];
                    string entityType = (string)context.InputParameters["StringParameter"];
                    string inputText = (string)context.InputParameters["StringParameter"];
                    string language = (string)context.InputParameters["StringParameter"]; //optional

                    if (!string.IsNullOrEmpty(inputText))
                    {
                        //Simply reversing the characters of the string
                        context.OutputParameters["StringProperty"] = new string(inputText.Reverse().ToArray());
                    }
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Sample_CustomAPIExample: {0}", ex.ToString());
                    throw new InvalidPluginExecutionException("An error occurred in Sample_CustomAPIExample.", ex);
                }
            }
            else
            {
                tracingService.Trace("Sample_CustomAPIExample plug-in is not associated with the expected message or is not registered for the main operation.");
            }
        }
    }
}
