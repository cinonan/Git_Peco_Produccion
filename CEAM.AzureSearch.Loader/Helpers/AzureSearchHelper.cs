using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;

namespace CEAM.AzureSearch.Loader.Helpers
{
    public class AzureSearchHelper
    {
        public const string ApiVersionString = "api-version=2020-06-30";

        private static readonly JsonSerializerOptions _jsonOptions;

        static AzureSearchHelper()
        {
            _jsonOptions = new JsonSerializerOptions { };

            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public static string SerializeJson(object value)
        {
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        public static T DeserializeJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public static HttpResponseMessage SendSearchRequest(HttpClient client, HttpMethod method, Uri uri, string json = null)
        {
            UriBuilder builder = new UriBuilder(uri);
            string separator = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : "&";
            builder.Query = builder.Query.TrimStart('?') + separator + ApiVersionString;

            var request = new HttpRequestMessage(method, builder.Uri);

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return client.SendAsync(request).Result;
        }

        public static void EnsureSuccessfulSearchResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string error = response.Content == null ? null : response.Content.ReadAsStringAsync().Result;
                throw new Exception("Search request failed: " + error);
            }
        }
    }
}
