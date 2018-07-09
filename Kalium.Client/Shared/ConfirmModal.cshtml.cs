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
        protected Func<Task> YesEventHandler { get; set; }
        protected async Task OnClickYes()
        {
            if (YesEventHandler != null)
            {
                await YesEventHandler?.Invoke();
            }
        }

        protected void OnClickNo()
        {
            MegaService.Util.HideModal();
        }
    }
}
