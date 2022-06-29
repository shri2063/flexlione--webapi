using System;
using Newtonsoft.Json;

namespace flexli_erp_webapi.Models
{
    public class Error
    {
        [JsonProperty(Order = 1)]
        public String title { get; set; }

        [JsonProperty(Order = 2)]
        public string detail;

        public Error(string title, String detail)
        {
            this.title = title;
            this.detail = detail;
        }  
    }
}