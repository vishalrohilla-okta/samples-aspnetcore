﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Okta.AspNetCore;
using okta_aspnetcore_mvc_example.Models;

namespace okta_aspnetcore_mvc_example.Controllers
{
    public class AccountController : Controller
    {
        private OktaSettings _oktaSettings;

        public AccountController(IOptions<OktaSettings> oktaSettings)
        {
            _oktaSettings = oktaSettings.Value;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([FromForm]string sessionToken)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                var properties = new AuthenticationProperties();
                properties.Items.Add("sessionToken", sessionToken);
                properties.RedirectUri = "/Home/About";

                return Challenge(properties, OktaDefaults.MvcAuthenticationScheme);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        /*  public IActionResult Logout()
          {
              return new SignOutResult(new[] { CookieAuthenticationDefaults.AuthenticationScheme, OktaDefaults.MvcAuthenticationScheme });
          }
  */
        public IActionResult Logout()

        {

            if (HttpContext.User.Identity.IsAuthenticated)

            {

                return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

            }



            return RedirectToAction("Index", "Home");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginWithIdp([FromForm]string idp)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                var properties = new AuthenticationProperties();
                properties.Items.Add("idp", idp);
                properties.RedirectUri = "/Home/About";
                return Challenge(properties, OktaDefaults.MvcAuthenticationScheme);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}