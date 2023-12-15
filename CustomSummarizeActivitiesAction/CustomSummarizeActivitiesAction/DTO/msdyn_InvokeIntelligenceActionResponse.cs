using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomSummarizeActivitiesAction.DTO
{
    //public class msdyn_InvokeIntelligenceActionResponse
    //{
    //    [JsonPropertyName("@odatatype")]
    //    public string odatacontext { get; set; }
    //    public object Metadata { get; set; }
    //    public ResultObj Result { get; set; }
    //}
    public class msdyn_InvokeIntelligenceActionResponse
    {
        public string ResponseName { get; set; }
        public Result[] Results { get; set; }
    }

    public class Result
    {
        public string Key { get; set; }
        public Value Value { get; set; }
    }

    public class Value
    {
        public object LogicalName { get; set; }
        public string Id { get; set; }
        public Attribute[] Attributes { get; set; }
        public object EntityState { get; set; }
        public object[] FormattedValues { get; set; }
        public object[] RelatedEntities { get; set; }
        public object RowVersion { get; set; }
        public object[] KeyAttributes { get; set; }
    }

    public class Attribute
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }
}




