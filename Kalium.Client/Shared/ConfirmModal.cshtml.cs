using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Microsoft.AspNetCore.Blazor.Components;

namespace Kalium.Client.Shared
{
    public class ConfirmModalModel: BlazorComponent
    {
        [Inject]
        protected IMegaService MegaService { get; set; }
        [Parameter]
        protected string Id { get; set; }
        [Parameter]
        protected string Title { get; set; }
        [Parameter]
        protected string Message { get; set; }
        [Parameter]
        protected Action YesEventHandler { get; set; }

        protected void OnClickYes()
        {
            YesEventHandler();
        }

        protected void OnClickNo()
        {
            MegaService.Util.HideModal();
        }
    }
}
