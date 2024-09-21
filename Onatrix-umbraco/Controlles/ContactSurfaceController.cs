using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Onatrix_umbraco.Models;
using System.Net.Http;
using System.Text;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace Onatrix_umbraco.Controlles
{
    public class ContactSurfaceController : SurfaceController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ContactSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IHttpClientFactory httpClientFactory) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> HandelSubmitAsync(ContactFormModel form)
        {
            if (!ModelState.IsValid)
            {
                ViewData["name"] = form.Name;
                ViewData["email"] = form.Email;
                ViewData["massege"] = form.Massege;
                ViewData["phonenumber"] = form.phonenumber;

                ViewData["error_name"] = string.IsNullOrEmpty(form.Name);
                ViewData["error_email"] = string.IsNullOrEmpty(form.Email);
                ViewData["error_massege"] = string.IsNullOrEmpty(form.Massege);
                ViewData["error_phonenumber"] = form.phonenumber;
                return CurrentUmbracoPage();
            }
            var jsonData = new
            {
                Name = form.Name,
                Email = form.Email,
                Message = form.Massege,
                PhoneNumber = form.phonenumber
            };

            // Convert the data to a JSON string
            var jsonString = JsonConvert.SerializeObject(jsonData);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // Send the data to the Azure Function
            var client = _httpClientFactory.CreateClient();
            var azureFunctionUrl = "https://email-sender-provider.azurewebsites.net/api/SendEmailToServiceBus?code=ocAnVCYCvXP4d76l7NMgJdOMZBS_wERDf5nTeGQtXohWAzFuFGih2Q%3D%3D"; 

            var response = await client.PostAsync(azureFunctionUrl, content);

            if (response.IsSuccessStatusCode)
            {
                
                return RedirectToCurrentUmbracoPage();
            }
            else
            {
                
                ModelState.AddModelError("", "There was an error submitting the form. Please try again later.");
                return CurrentUmbracoPage();
            }
        }
    }
}
