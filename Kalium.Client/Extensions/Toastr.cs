using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Shared.Consts;
using Microsoft.AspNetCore.Blazor.Browser.Interop;

namespace Kalium.Client.Extensions
{
    public abstract class ToastrBase
    {
        public object Options = new
        {

        };

        private void Show(Consts.Toastr mode, string message)
        {
            RegisteredFunction.Invoke<bool>("toastr", mode.ToString().ToLower(), message);
        }

        public void Success(string message)
        {
            Show(Consts.Toastr.Success, message);
        }

        public void Info(string message)
        {
            Show(Consts.Toastr.Info, message);
        }

        public void Warning(string message)
        {
            Show(Consts.Toastr.Warning, message);
        }

        public void Error(string message)
        {
            Show(Consts.Toastr.Error, message);
        }
    }

    public class Toastr: ToastrBase
    {

    }
}
