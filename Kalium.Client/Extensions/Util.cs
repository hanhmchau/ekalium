using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Services;

namespace Kalium.Client.Extensions
{
    public interface IUtil
    {
        void ShowModal(string id);
        void HideModal();
        void ShowLoginModal();
        void ShowRegisterModal();
        void Return();
        void Checkpoint(string checkpoint);
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
    }
}
