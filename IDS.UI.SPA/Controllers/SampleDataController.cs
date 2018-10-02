using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IDS.UI.SPA.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public IActionResult Login()
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = "/counter"
                },
                "oidc");
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IEnumerable<WeatherForecast>> WeatherForecasts(int startDateIndex)
        {
            string idToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            var disco = await DiscoveryClient.GetAsync("https://localhost:5001");

            var userInfoClient = new UserInfoClient(disco.UserInfoEndpoint); //, idToken);

            var response = await userInfoClient.GetAsync(idToken); //, new System.Threading.CancellationToken()).Result;

            var claims = response.Claims;

            //var metaDataResponse = await DiscoveryClient.GetAsync("https://localhost:5001");
            //var userInfoClient1 = new UserInfoClient(metaDataResponse.UserInfoEndpoint);

            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var response1 = await userInfoClient.GetAsync(accessToken);

            var address = response1.Claims.FirstOrDefault(c => c.Type == "address")?.Value;

            var rng = new Random();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index + startDateIndex).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get
                {
                    return 32 + (int)(TemperatureC / 0.5556);
                }
            }
        }
    }
}
