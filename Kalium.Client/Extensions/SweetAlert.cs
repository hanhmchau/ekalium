using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Browser.Interop;

namespace Kalium.Client.Extensions
{
    public interface ISweetAlert
    {
        void Success(string title, string text);
        void Confirm(string title, string text);
        void Error(string title, string text);
    }

    public class SweetAlert : ISweetAlert
    {
        public void Success(string title, string text)
        {
            Alert(title, text, "success", false);
        }

        public void Confirm(string title, string text)
        {
            Confirm(title, text, "warning");
        }
        public void Error(string title, string text)
        {
            Alert(title, text, "error", false);
        }
        private void Alert(string title, string text, string mode, bool multi)
        {
            RegisteredFunction.InvokeAsync<bool>("sweetAlert", title, text, mode);
        }
        private void Confirm(string title, string text, string mode)
        {
            RegisteredFunction.InvokeAsync<bool>("sweetAlertConfirm", title, text, mode, true);
        }
    }
}