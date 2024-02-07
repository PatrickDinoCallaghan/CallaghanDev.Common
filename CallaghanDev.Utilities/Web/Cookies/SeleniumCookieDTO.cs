using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Web
{
    public class SeleniumCookieDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("domain", NullValueHandling = NullValueHandling.Ignore)]
        public string Domain { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("secure")]
        public bool Secure { get; set; }

        [JsonProperty("HttpOnly")]
        public bool HttpOnly { get; set; }

        [JsonProperty("Expires")]
        public DateTime? Expires { get; set; }

        [JsonProperty("SameSite")]
        public string SameSite { get; set; }
        public SeleniumCookieDTO(string name, string value, string domain, string path, bool secure, bool isHttpOnly, string sameSite, DateTime? expiry)
        {
            Name = name;
            Value = value;
            Domain = domain;
            Path = path;
            Secure = secure;
            HttpOnly = isHttpOnly;
            Expires = expiry;
            SameSite = sameSite;
            // The properties for expiry, isHttpOnly, and sameSite are not defined in your provided class.
            // If they exist in the actual class, then you should add:
            // Expiry = expiry;
            // IsHttpOnly = isHttpOnly;
            // SameSite = sameSite;
        }
    }

}
