using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Services;

namespace Kalium.Client.Extensions
{
    public class DateRange
    {
        public string Begin { get; set; }
        public string End { get; set; }
    }
    public interface IUtil
    {
        void ShowModal(string id);
        void HideModal();
        void ShowLoginModal();
        void ShowRegisterModal();
        void Return();
        void Checkpoint(string checkpoint);
        void InitComponents();
        void InitAdminComponents();
        T GetInput<T>(string selector);
        void InitializeSignalR();
        void NavigateToForbidden();
        void NavigateToNotFound();
        void RefreshShop();
        void AnnounceUpdateProduct(int id);
        bool ValidateForm(string selector);
        DateRange GetDates(string selector);
        void NavigateToHome();
    }

    public class Util : IUtil
    {
        private readonly IUriHelper _uriHelper;
        private readonly LocalStorage _storage;
        public Util(IUriHelper uriHelper, LocalStorage storage)
        {
            _uriHelper = uriHelper;
            _storage = storage;
        }

        public void ShowModal(string id)
        {
            HideModal();
            RegisteredFunction.Invoke<bool>("showModal", id);
        }

        public void HideModal()
        {
            RegisteredFunction.Invoke<bool>("hideModal");
        }

        public void ShowLoginModal()
        {
            ShowModal("login-modal");
        }
        public void ShowRegisterModal()
        {
            ShowModal("login-modal");
        }

        public void Return()
        {
            var lastVisited = _storage["LAST_VISITED"] ?? "/";
            _uriHelper.NavigateTo(lastVisited);
        }

        public void Checkpoint(string checkpoint)
        {
            _storage["LAST_VISITED"] = checkpoint;
        }

        public void InitComponents()
        {
            RegisteredFunction.Invoke<bool>("components");
        }

        public void InitAdminComponents()
        {
            RegisteredFunction.Invoke<bool>("admin-components");
        }

        public T GetInput<T>(string selector)
        {
            var val = RegisteredFunction.Invoke<string>("getInput", selector);
            return (T) Convert.ChangeType(val, typeof(T));
        }

        public void InitializeSignalR()
        {
            RegisteredFunction.Invoke<bool>("initializeSignalR");
        }

        public void NavigateToForbidden()
        {
            _uriHelper.NavigateTo("/403");
        }

        public void NavigateToNotFound()
        {
            _uriHelper.NavigateTo("/404");
        }

        public void NavigateToHome()
        {
            _uriHelper.NavigateTo("/");
        }

        public void RefreshShop()
        {
            RegisteredFunction.Invoke<bool>("refreshShopList");
        }

        public void AnnounceUpdateProduct(int id)
        {
            RegisteredFunction.Invoke<bool>("refreshProduct", id);
        }

        public bool ValidateForm(string selector)
        {
            return RegisteredFunction.Invoke<bool>("validateForm", selector);
        }

        public DateRange GetDates(string selector)
        {
            var range = RegisteredFunction.Invoke<DateRange>("getDates", selector);
            return range;
        }
    }
}
