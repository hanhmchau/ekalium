using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kalium.Client.Extensions
{
    public interface IHttpApiClientRequestBuilderFactory
    {
        IHttpApiClientRequestBuilder Create(string url);
    }

    public class HttpApiClientRequestBuilderFactory : IHttpApiClientRequestBuilderFactory
    {
        private readonly IUriHelper _uriHelper;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpApiClientRequestBuilder> _logger;
        private readonly Toastr _toastr;
        private readonly IUtil _util;

        public HttpApiClientRequestBuilderFactory(HttpClient httpClient, IUriHelper uriHelper, ILogger<HttpApiClientRequestBuilder> logger, Toastr toastr, IUtil util)
        {
            _uriHelper = uriHelper;
            _httpClient = httpClient;
            _logger = logger;
            _toastr = toastr;
            _util = util;
        }

        public IHttpApiClientRequestBuilder Create(string url)
        {
            return new HttpApiClientRequestBuilder(url, _httpClient, _uriHelper, _logger, _toastr, _util);
        }
    }

    public interface IHttpApiClientRequestBuilder
    {
        Task Post<T>(T data);
        Task Get();
        HttpApiClientRequestBuilder OnOk<T>(Action<T> todo);
        HttpApiClientRequestBuilder OnOk(Func<Task> todo);
        HttpApiClientRequestBuilder OnOk(Action todo);
    }
    public class HttpApiClientRequestBuilder : IHttpApiClientRequestBuilder
    {
        private readonly Func<HttpResponseMessage, Task> _onBadRequest;
        private Func<HttpResponseMessage, Task> _onOK;
        private readonly string _url;
        private readonly HttpClient _httpClient;
        private readonly IUriHelper _uriHelper;
        private readonly ILogger<HttpApiClientRequestBuilder> _logger;
        private readonly Toastr _toastr;
        private readonly IUtil _util;

        public HttpApiClientRequestBuilder(string url, HttpClient httpClient, IUriHelper uriHelper, ILogger<HttpApiClientRequestBuilder> logger,
            Toastr toastr, IUtil util)
        {
            _url = url;
            _httpClient = httpClient;
            _uriHelper = uriHelper;
            _logger = logger;
            _toastr = toastr;
            _util = util;
        }

        private async Task ExecuteQuery(Func<Task<HttpResponseMessage>> httpCall)
        {
            try
            {
                var response = await httpCall();
                await HandleResponse(response);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, e.Message);
            }
        }

        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                await _onOK(response);
            }
            else
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        _util.ShowLoginModal();
                        break;
                    case HttpStatusCode.Forbidden:
                        _uriHelper.NavigateTo("/403");
                        break;
                    case HttpStatusCode.InternalServerError:
                        _toastr.Error("An error happened at server. Please try again.");
                        break;
                    case HttpStatusCode.NotFound:
                        _uriHelper.NavigateTo("/404");
                        break;
                }
            }
        }

        public async Task Post<T>(T data)
        {
            await ExecuteQuery(async () =>
            {
                var requestJson = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                return await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, _url)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                });
            });
        }
        
        public async Task Get()
        {
            await ExecuteQuery(async () => await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, _url)));
        }

        public HttpApiClientRequestBuilder OnOk<T>(Action<T> todo)
        {
            _onOK = async r =>
            {
                var response = JsonUtil.Deserialize<T>(await r.Content.ReadAsStringAsync());
                todo(response);
            };
            return this;
        }

        public HttpApiClientRequestBuilder OnOk(Func<Task> todo)
        {
            _onOK = async r =>
            {
                await todo();
            };
            return this;
        }

        public HttpApiClientRequestBuilder OnOk(Action todo)
        {
            _onOK = r =>
            {
                todo();
                return Task.CompletedTask;
            };
            return this;
        }
    }
}