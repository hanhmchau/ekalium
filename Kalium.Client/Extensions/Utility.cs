using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Browser.Interop;

namespace Kalium.Client.Extensions
{
    public interface IUtility
    {
        void ShowModal(string id);
        void HideModal();
        void ShowLoginModal();
        void ShowRegisterModal();
    }

    public class Utility : IUtility
    {
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
    }
}
