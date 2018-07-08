using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Kalium.Shared.Services;
using Microsoft.AspNetCore.Blazor.Services;

namespace Kalium.Client.Extensions
{
    public interface IMegaService
    {
        IAccountService AccountService { get; }
        IHttpApiClientRequestBuilderFactory HttpApiClientRequestBuilderFactory { get; }
        Toastr Toastr { get; }
        LocalStorage LocalStorage { get; }
        HttpClient HttpClient { get; }
        IUtil Util { get; }
        IFetcher Fetcher { get; }
        IUriHelper UriHelper { get; }
    }

    public class MegaService : IMegaService
    {
        public MegaService(IAccountService accountService,
            IHttpApiClientRequestBuilderFactory iHttpApiClientRequestBuilderFactory, Toastr toastr,
            LocalStorage localStorage, HttpClient httpClient, IUtil util, IFetcher fetcher, IUriHelper uriHelper)
        {
            AccountService = accountService;
            HttpApiClientRequestBuilderFactory = iHttpApiClientRequestBuilderFactory;
            Toastr = toastr;
            LocalStorage = localStorage;
            HttpClient = httpClient;
            Util = util;
            Fetcher = fetcher;
            UriHelper = uriHelper;
        }

        public IAccountService AccountService { get; }

        public IHttpApiClientRequestBuilderFactory HttpApiClientRequestBuilderFactory { get; }

        public Toastr Toastr { get; }

        public LocalStorage LocalStorage { get; }

        public HttpClient HttpClient { get; }

        public IUtil Util { get; }

        public IFetcher Fetcher { get; }

        public IUriHelper UriHelper { get; }
    }
}
