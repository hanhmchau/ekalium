using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kalium.Client.Extensions;
using Kalium.Shared.Consts;
using Kalium.Shared.Models;
using Kalium.Shared.Services;
using Microsoft.AspNetCore.Blazor.Components;
using Newtonsoft.Json;

namespace Kalium.Client.Pages
{
    public class CheckoutConfirmationModel: BlazorComponent
    {
        protected string Name { get; set; }
        protected string Country { get; set; }
        protected string Address { get; set; }
        protected string Phone { get; set; }
        protected string AlternateName { get; set; }
        protected string AlternateCountry { get; set; }
        protected string AlternateAddress { get; set; }
        protected string AlternatePhone { get; set; }
        protected string Note { get; set; }
        protected ECart ECart { get; set; }
        protected ICollection<string> Messages { get; set; } = new List<string>();
        protected bool SendToDifferentAddress { get; set; }
        protected Consts.PaymentMethod PaymentMethod { get; set; }

        [Inject]
        public IMegaService MegaService { get; set; }

        protected override async Task OnInitAsync()
        {
            await ImportCart();
            if (MegaService.AccountService.GetCurrentUser() == null)
            {
                MegaService.UriHelper.NavigateTo("/login");
                return;
            }
            if (ECart == null || !ECart.Contents.Any())
            {
                MegaService.UriHelper.NavigateTo("/cart");
                return;
            }
            MegaService.Util.InitComponents();
        }

        protected void ValidateInfo()
        {
            Messages = new List<string>();
            if (string.IsNullOrEmpty(Name) || Name.Length < 4)
            {
                Messages.Add("Name cannot be empty.");
            }
            if (string.IsNullOrEmpty(Address) || Address.Length < 4)
            {
                Messages.Add("Address must have at least 4 characters.");
            }
            if (string.IsNullOrEmpty(Phone))
            {
                Messages.Add("Phone cannot be empty.");
            }
            else if (ValidatorUtils.IsValidPhone(Phone))
            {
                Messages.Add("Phone in incorrect format.");
            }

            if (SendToDifferentAddress)
            {
                if (string.IsNullOrEmpty(AlternateName) || AlternateName.Length < 4)
                {
                    Messages.Add("Alternative name cannot be empty.");
                }
                if (string.IsNullOrEmpty(AlternateAddress) || AlternateAddress.Length < 4)
                {
                    Messages.Add("Alternative address must have at least 4 characters.");
                }
                if (string.IsNullOrEmpty(AlternatePhone))
                {
                    Messages.Add("Alternative phone number cannot be empty.");
                }
                else if (ValidatorUtils.IsValidPhone(AlternatePhone))
                {
                    Messages.Add("Phone number in incorrect format.");
                }
            }
        }

        protected async Task PlaceOrder()
        {
            ValidateInfo();
            if (Messages.Any())
            {
                StateHasChanged();
                return;
            }
            var pseudoCart = JsonConvert.SerializeObject(ECart.ToPseudo());
            var resultObj = await MegaService.Fetcher.Fetch("/api/Cart/CheckOut/", new
            {
                PseudoCart = pseudoCart,
                Name,
                Address,
                Phone,
                AlternateName,
                AlternateAddress,
                AlternatePhone,
                SendToDifferentAddress,
                PaymentMethod = (int) PaymentMethod,
                Note
            });

            var jsonToStr = resultObj.ToString();
            var checkoutResult = resultObj["Result"].ToObject<CheckOutResult>();
            Console.WriteLine(checkoutResult.Succeeded);
            if (checkoutResult.Succeeded)
            {
                var orderId = checkoutResult.OrderId;
                MegaService.UriHelper.NavigateTo($"/order/{orderId}");
            }
            else
            {
                Messages = checkoutResult.Messages;
                Console.WriteLine(Messages);
                StateHasChanged();
            }
        }

        protected async Task ImportCart()
        {
            var cartJson = MegaService.LocalStorage["CART"];
            if (cartJson != null)
            {
                var pseudoCart = JsonConvert.DeserializeObject<PseudoCart>(cartJson, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                });
                if (pseudoCart.Cart.Any())
                {
                    var ecartJson = await MegaService.Fetcher.Fetch("/api/Cart/Get/", pseudoCart);
                    ECart = ecartJson["ECart"].ToObject<ECart>();
                }
            }
        }
    }
}
