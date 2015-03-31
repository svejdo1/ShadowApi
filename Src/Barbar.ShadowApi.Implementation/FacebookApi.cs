using Barbar.ShadowApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using WatiN.Core;
using WatiN.Core.Exceptions;
using WatiN.Core.Native.InternetExplorer;

namespace Barbar.ShadowApi.Implementation
{
  public class FacebookApi : MarshalByRefObject, IFacebookApi, IFacebookApiRepository
  {
    public IList<FacebookUser> GetFacebookFriends(string email, string password, int? maxTimeoutInMilliseconds)
    {
      var users = new List<FacebookUser>();
      Settings.Instance.MakeNewIeInstanceVisible = false;
      using (var browser = new IE("https://www.facebook.com"))
      {
        try
        {
          browser.TextField(Find.ByName("email")).Value = "svejdo1@gmail.com";
          browser.TextField(Find.ByName("pass")).Value = "fb@jackal12";
          browser.Form(Find.ById("login_form")).Submit();
          browser.WaitForComplete();
        }
        catch (ElementNotFoundException)
        {
          // we're already logged in
        }
        browser.GoTo("https://www.facebook.com/friends");
        var watch = new Stopwatch();
        watch.Start();

        Link previousLastLink = null;
        while (maxTimeoutInMilliseconds.HasValue && watch.Elapsed.TotalMilliseconds < maxTimeoutInMilliseconds.Value)
        {
          var lastLink = browser.Links.Where(l => l.GetAttributeValue("data-hovercard") != null
    && l.GetAttributeValue("data-hovercard").Contains("user.php")
    && l.Text != null
  ).LastOrDefault();
          if (lastLink == null || previousLastLink == lastLink)
          {
            break;
          }

          var ieElement = lastLink.NativeElement as IEElement;
          if (ieElement != null)
          {
            var htmlElement = ieElement.AsHtmlElement;
            htmlElement.scrollIntoView();
            browser.WaitForComplete();
          }

          previousLastLink = lastLink;
        }

        var links = browser.Links.Where(l => l.GetAttributeValue("data-hovercard") != null
          && l.GetAttributeValue("data-hovercard").Contains("user.php")
          && l.Text != null
        ).ToList();

        var idRegex = new Regex("id=(?<id>([0-9]+))");
        foreach (var link in links)
        {
          string hovercard = link.GetAttributeValue("data-hovercard");
          var match = idRegex.Match(hovercard);
          long id = 0;
          if (match.Success)
          {
            id = long.Parse(match.Groups["id"].Value);
          }
          users.Add(new FacebookUser
          {
            Name = link.Text,
            Id = id
          });
        }
      }
      return users;
    }

    public IFacebookApi GetApi()
    {
      return this;
    }
    
    public void UnloadApi()
    {
    }
  }
}
