using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kalium.Shared.Services
{
    public interface IFetcher
    {
        Task<JObject> Fetch(string url);
        Task<JObject> Fetch(string url, object body);
    }

    public class Fetcher : IFetcher
    {
        private readonly HttpClient _http;

        public Fetcher(HttpClient http)
        {
            _http = http;
        }

        public async Task<JObject> Fetch(string url)
        {
            // controller returns: return JsonConvert.SerializeObject(result);
            var json = await _http.GetJsonAsync<object>(url);

            return JsonConvert.DeserializeObject(json.ToString()) as JObject;
        }

        public async Task<JObject> Fetch(string url, object body)
        {
            // controller returns: return JsonConvert.SerializeObject(result);
            var json = await _http.PostJsonAsync<object>(url, JsonConvert.SerializeObject(body));

            return JsonConvert.DeserializeObject(json.ToString()) as JObject;
        }

    }
}
