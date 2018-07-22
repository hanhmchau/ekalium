using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Kalium.Shared.Models;
using MoreLinq;

namespace Kalium.Shared.Email
{
    public class OrderEmail
    {
        public Order Order { get; set; }

        private string GetItem(Product product)
        {
            var imageUrl = product.MainImage?.Url;
            if (imageUrl == null)
            {
                imageUrl = "http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/01/09/huiGyY6IU5oem731WAQgBHz4/StampReady/img/pic-left-bg.jpg";
            }
            if (!imageUrl.Contains("http"))
            {
                imageUrl = $"http://localhost:52747/{imageUrl.Replace("\\", "/")}";
            }
            return
                $@"<table style=""border-collapse: collapse; "" width=""100 % "" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                                                                       <tbody><tr>
                                                                               <td class=""erase"" width=""5%""><br></td>                                                           
                                                                            <td class=""display-block padding"" width=""40%"" valign=""middle"" align=""center"">
                                                                                <table class=""text-center"" style=""border-collapse: collapse;"" width=""120"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""left"">
                                                                                    <tbody>
                                                                                        <tr>
                                                                                            <td style = ""line-height:1px;"" width=""100%"" valign=""middle"" height=""80"" align=""center"">
                                                                                                <img editable = ""true"" src=""{imageUrl}"" alt=""Product"" style=""display:block;"" width=""136"" height=""76"" border=""0"">
                                                                                            </td>
                                                                                        </tr>
                                                                                    </tbody>
                                                                                </table>
                                                                            </td>
                                                                            <td class=""display-block padding"" width=""40%"" valign=""top"" align=""center"">
                                                                                <table class=""text-center"" style=""border-collapse: collapse;"" width=""120"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""right"">
                                                                                    <tbody>
                                                                                        <tr>
                                                                                            <td class=""text-center"" data-size=""Product Name"" data-min=""12"" data-max=""20"" data-color=""Product Name"" style=""margin:0px; padding:0px; font-size:16px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:bold;"" width=""100%"" valign=""middle"" align=""right"">{product.Name}</td>
                                                                                        </tr>
                                                                                        <tr>
                                                                                        	<td width = ""100%"" height= ""10""><br></ td>
                                                                                        </ tr>
                                                                                       <tr>
                                                                                            <td class=""text-center"" data-size=""Product Price"" data-min=""8"" data-max=""16"" data-color=""Product Price"" style=""margin:0px; padding:0px; font-size:12px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal;"" width=""100%"" valign=""middle"" align=""right"">${product.DiscountedPrice}</td>
                                                                                        </tr>
                                                                                        <tr>
                                                                                            <td class=""text-center"" data-size=""Product Price 2"" data-min=""8"" data-max=""16"" data-color=""Product Price 2"" style=""margin:0px; padding:0px; padding-top:3px; font-size:12px; color:#969696; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal;"" width=""100%"" valign=""middle"" align=""right""><s>${product.Price}</s></td>
                                                                                        </tr>
                                                                                    </tbody>
                                                                                </table>
                                                                            </td>
                                                                            <td class=""erase"" width=""5%""><br></td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>";
        }

        public string GetItems()
        {
            var builder = new StringBuilder();
            var products = Order.OrderItems.Select(oi => oi.Product).DistinctBy(p => p.Id).ToList();
            products.ForEach(p => { builder.Append(GetItem(p)); });
            return builder.ToString();
        }

        public string GetContent()
        {
            return $@"<div id = ""frame"" class=""ui-sortable"" style=""min-height: 250px;"">
  <div class=""parentOfBg""><table class=""width_100 currentTable"" style=""border-collapse:collapse;"" data-module=""Order-Conﬁrmation 1"" data-thumb=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/thumbnails/Order-Conformation-1.jpg"" width=""800"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
    <tbody>
        <tr>
        	<td style = ""background-image: url(&quot;http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/bg-image-1.png&quot;); background-position: center center; background-repeat: repeat; background-size: cover; background-color: rgb(234, 225, 225);"" data-bg=""Order-Conﬁrmation 1"" data-bgcolor=""Order-Conﬁrmation 1"" width=""100%"" valign=""middle"" bgcolor=""#fe6363"" background=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/bg-image-1.png"" align=""center"">
            	<table style = ""border-collapse: collapse;"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                	<tbody>
                        <tr>
                            <td style = ""line-height:1px;"" width=""100%"" height=""53""></td>
                        </tr>
                    	<tr>
                        	<td style = ""line-height:1px;"" width=""100%"" valign=""middle"" align=""center"">
                            	<a href = ""http://www.yourwebsiteurl.com/"" target=""_blank"" style=""display:inline-block;""><img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/logo-2.png"" alt=""Logo"" style=""display:block;"" width=""164"" height=""29"" border=""0""></a>
                            </td>
                        </tr>
                        <tr>
                            <td style = ""line-height:1px;"" width=""100%"" height=""32""></td>
                        </tr>
                        <tr>
                        	<td class=""editable"" width=""100%"" valign=""middle"" contenteditable=""false"" align=""center"">
                            	<table class=""width_90percent"" style=""border-collapse: collapse; max-width:90%; -webkit-border-radius: 10px; border-radius: 10px;"" data-bgcolor=""Box Color"" width=""400"" cellspacing=""0"" cellpadding=""0"" border=""0"" bgcolor=""#FFFFFF"" align=""center"">
                                    <tbody>
                                        <tr>
                                        	<td class=""editable"" width=""100%"" valign=""middle"" contenteditable=""false"" align=""center"">
                                                <table class=""width_90percent"" style=""border-collapse: collapse; max-width:90%;"" width=""327"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                                                    <tbody>
                                                        <tr>
                                                            <td style = ""line-height:1px;"" width=""100%"" height=""76""><br></td>
                                                        </tr>
                                                        <tr>
                                                            <td style = ""line-height:1px;"" width=""100%"" valign=""middle"" align=""center"">
                                                                <img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/icon-1.png"" alt=""Icon"" style=""display:block;"" width=""253"" height=""133"" border=""0"">
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""display-block padding"" style=""line-height:1px;"" width=""100%"" height=""57""><br></td>
                                                        </tr>
                                        				<tr>
                                                            <td data-size=""Main Title"" data-min=""24"" data-max=""32"" data-color=""Main Title"" style=""margin:0px; padding:0px; font-size:28px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:bold; text-transform:uppercase;"" width=""100%"" valign=""middle"" align=""center"">ORDER<br> confirmation</td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""erase"" style=""line-height:1px;"" width=""100%"" height=""20""><br></td>
                                                        </tr>
                                                        <tr>
                                                            <td width = ""100%"" valign=""middle"" align=""center"">
                                                                <table style = ""border-collapse: collapse;"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                                                                    <tbody>
                                                                        <tr>                                                            
                                                                            <td data-border-bottom-color=""Title Color"" style=""line-height: 1px; border-bottom: 5px solid rgb(255, 79, 64);"" width=""72"" height=""16""><br></td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""display-block padding"" style=""line-height:1px;"" width=""100%"" height=""30""><br></td>
                                                        </tr>
                                        				<tr>
                                                            <td data-size=""Subtitle"" data-min=""16"" data-max=""24"" data-color=""Subtitle"" style=""margin:0px; padding:0px; font-size:20px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:bold; line-height:30px;"" class=""editable"" width=""100%"" valign=""middle"" contenteditable=""false"" align=""center"">Your order has been placed<br> successfully
</td>
                                                        </tr>
                                                        <tr>
                                                            <td style = ""line-height:1px;"" width= ""100%"" height= ""20""><br></ td>
                                                        </ tr>
                                                        <tr>
                                                            <td width= ""100%"" valign= ""middle"" align= ""center"">
                                                                <table style= ""border-collapse: collapse;"" width= ""100%"" cellspacing= ""0"" cellpadding= ""0"" border= ""0"" align= ""center"">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td data-border-bottom-color= ""Separator"" style= ""line-height:1px; border-bottom:1px solid #cdcdcd;"" width= ""100%"" height= ""11""><br></ td>
                                                                        </ tr>
                                                                    </ tbody>
                                                                </ table>
                                                            </ td>
                                                        </ tr>
                                                        <tr>
                                                            <td style= ""line-height:1px;"" width= ""100%"" height= ""21""><br></ td>
                                                        </ tr>
                                                        <tr>
                                                            <td width= ""100%"" valign= ""middle"" align= ""center"">
                                                              <!--ONE ORDER ROW --> {{row}}
                                                                                         <!-- END ONE ORDER ROW -->
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td width = ""100%"" valign=""middle"" align=""center"">
                                                                <table style = ""border-collapse: collapse;"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                                                                    <tbody>
                                                                        <tr>                                                            
                                                                            <td data-border-bottom-color=""Separator"" style=""line-height:1px; border-bottom:1px solid #cdcdcd;"" width=""100%"" height=""22""><br></td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""display-block padding"" style=""line-height:1px;"" width=""100%"" height=""31""><br></td>
                                                        </tr>
                                                        <tr>
                                                            <td data-size=""Amount"" data-min=""18"" data-max=""26"" data-color=""Amount"" style=""margin:0px; padding:0px; font-size:22px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:bold;"" width=""100%"" valign=""middle"" align=""center"">Price : ${{total}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td style = ""line-height:1px;"" width=""100%"" height=""10""><br></td>
                                                        </tr>
                                                        <tr>
                                                            <td data-size=""Subtitle-2"" data-min=""10"" data-max=""18"" data-color=""Subtitle-2"" style=""margin:0px; padding:0px; font-size:14px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:bold;"" width=""100%"" valign=""middle"" align=""center"">Order ID #{{orderId}} l Date {{date}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""display-block padding"" style=""line-height:1px;"" width=""100%"" height=""31""><br></td>
                                                        </tr>
                                                        <tr>
                                                            <td data-size=""Subtitle-3"" data-min=""14"" data-max=""22"" data-color=""Subtitle-3"" style=""margin:0px; padding:0px; font-size:12px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal;"" width=""100%"" valign=""middle"" align=""center""> Shopping Address :<br>{{address}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td style = ""line-height:1px;"" width=""100%"" height=""32""><br></td>
                                                        </tr>
                                                        <tr>
                                                            <td width = ""100%"" valign=""middle"" align=""center"">
                                                                <table class=""width_90percent"" style=""border-collapse: collapse; max-width: 100%; border-radius: 30px; background-color: rgb(255, 79, 64);"" data-bgcolor=""Button Bg Color"" width=""255"" cellspacing=""0"" cellpadding=""0"" border=""0"" bgcolor=""#387a18"" align=""center"">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td data-size=""Button"" data-color=""Button"" data-min=""10"" data-max=""18"" style=""max-width:255px; margin:0px; padding: 8px 20px; font-size:14px; color:#FFFFFF; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal;"" width=""100%"" align=""center"">
                                                                                 <a href = ""http://localhost:52747/order/{{orderId}}"" target=""_blank"" data-color=""Button"" style=""width:100%; color:#FFFFFF; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; text-decoration:none; display:block;"">View order</a>
                                                                              </td>
                                                                        </tr>
                                                                    </tbody>
																</table>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""display-block padding"" style=""line-height:1px;"" width=""100%"" height=""28""><br></td>
                                                        </tr>
                                        				<tr>
                                                            <td data-size=""Description"" data-min=""9"" data-max=""16"" data-color=""Description"" style=""margin:0px; padding:0px; font-size:12px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal; line-height:24px;"" width=""100%"" valign=""middle"" align=""center"">For your reference, your username is</td>
                                                        </tr>                                                        
                                        				<tr>
                                                            <td data-size=""Description"" data-min=""9"" data-max=""16"" data-color=""Description"" style=""margin:0px; padding:0px; font-size:12px; color:#000000; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal; line-height:24px;"" width=""100%"" valign=""middle"" align=""center"">{{username}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td class=""display-block padding"" style=""line-height:1px;"" width=""100%"" height=""32""><br></td>
                                                        </tr>
                                                        <tr>
                                                        	<td width = ""100%"" valign=""middle"" align=""center"">
                                                            	<table style = ""border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt;"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"">
                                                                    <tbody>
                                                                        <tr>                                            
                                                                            <td style = ""line-height:1px;"" width=""25"" valign=""middle"" align=""center"">
                                                                                <a href = ""http://www.facebook.com/"" target=""_blank"" style=""display:block;""><img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/facebook.png"" alt=""Facebook"" style=""display:block"" width=""25"" height=""25"" border=""0""></a>
                                                                            </td>
                                                                            <td style = ""line-height:1px;"" width=""20""><br></td>
                                                                            <td style = ""line-height:1px;"" width=""25"" valign=""middle"" align=""center"">
                                                                                <a href = ""https://twitter.com/"" target=""_blank"" style=""display:block;""><img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/twitter.png"" alt=""Twitter"" style=""display:block"" width=""25"" height=""25"" border=""0""></a>
                                                                            </td>
                                                                            <td style = ""line-height:1px;"" width=""20""><br></td>
                                                                            <td style = ""line-height:1px;"" width=""25"" valign=""middle"" align=""center"">
                                                                                <a href = ""https://in.linkedin.com/"" target=""_blank"" style=""display:block;""><img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/instagram.png"" alt=""Linkedin"" style=""display:block"" width=""25"" height=""25"" border=""0""></a>
                                                                            </td>
                                                                            <td style = ""line-height:1px;"" width=""20""><br></td>
                                                                            <td style = ""line-height:1px;"" width=""25"" valign=""middle"" align=""center"">
                                                                                <a href = ""https://www.youtube.com/"" target=""_blank"" style=""display:block;""><img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/youtube.png"" alt=""Youtube"" style=""display:block"" width=""25"" height=""25"" border=""0""></a>
                                                                            </td> 
                                                                            <td style = ""line-height:1px;"" width=""20""><br></td>
                                                                            <td style = ""line-height:1px;"" width=""25"" valign=""middle"" align=""center"">
                                                                                <a href = ""https://www.google.com/gmail/"" target=""_blank"" style=""display:block;""><img editable = ""true"" src=""http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/07/20/6FL1uCKPBbhXAjNfM4rTyeQZ/zips-for-upload/images/gmail.png"" alt=""Gmail"" style=""display:block"" width=""25"" height=""25"" border=""0""></a>
                                                                            </td>                                           
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style = ""line-height:1px;"" width=""100%"" height=""49""><br></td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                   </tbody>
                               </table>
                            </td>
                        </tr>
                        <tr>
                            <td style = ""line-height:1px;"" width=""100%"" height=""46""></td>
                        </tr>                        
                        <tr>
                            <td data-size=""Footer Description"" data-min=""9"" data-max=""16"" data-color=""Footer Description"" style=""margin:0px; padding:0px; font-size:12px; color:#ffffff; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal; line-height:24px;"" width=""100%"" valign=""middle"" align=""center"">This email was sent to sr_email. <a href=""sr_unsubscribe"" data-color=""Footer Description"" target=""_blank"" style=""color:#ffffff; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; text-decoration:none; display:inline-block;"">Unsubscribe</a></td>
                        </tr>
                        <tr>
                            <td data-size= ""Footer Description"" data-min= ""9"" data-max= ""16"" data-color= ""Footer Description"" style= ""margin:0px; padding:0px; font-size:12px; color:#ffffff; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal; line-height:24px;"" width= ""100%"" valign= ""middle"" align= ""center"">©2018 Our Company Ltd.</td>
                        </tr>
                        <tr>
                            <td data-size= ""Footer Description"" data-min= ""9"" data-max= ""16"" data-color= ""Footer Description"" style= ""margin:0px; padding:0px; font-size:12px; color:#ffffff; font-family: 'Open Sans', Helvetica, Arial, Verdana, sans-serif; font-weight:normal; line-height:24px;"" width= ""100%"" valign= ""middle"" align= ""center""> Consectetur adipisicing elit. USA 87654</td>
                        </tr>
                        <tr>
                            <td style = ""line-height:1px;"" width= ""100%"" height= ""45""></ td>
                        </ tr>
                    </ tbody>
                </ table>
            </ td>
        </ tr>
    </ tbody>
</ table></ div><div id= ""edit_link"" class=""hidden"" style=""display: none;"">

						<!-- Close Link -->
						<div class=""close_link""></div>

						<!-- Edit Link Value -->
						<input id = ""edit_link_value"" class=""createlink"" placeholder=""Your URL"" type=""text"">

						<!-- Change Image Wrapper-->
						<div id = ""change_image_wrapper"">


                            <!--Change Image Tooltip -->
							<div id = ""change_image"">


                                <!--Change Image Button -->
								<p id = ""change_image_button""> Change & nbsp; <span class=""pixel_result""></span></p>

							</div>

							<!-- Change Image Link Button -->
							<input value = """" id=""change_image_link"" type=""button"">

							<!-- Remove Image -->
							<input value = """" id=""remove_image"" type=""button"">

						</div>

						<!-- Tooltip Bottom Arrow-->
						<div id = ""tip""></ div>


                    </ div></ div>";
        }
    }
}
