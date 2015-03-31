using Barbar.ShadowApi.Implementation;
using System;

namespace Barbar.ShadowApiExampleOne
{
  class Program
  {
    /// <summary>
    /// This demo how to use api directly; disadvantage - should facebook change UI 
    /// you'll have to re-release whole application
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
      var api = new FacebookApi();
      var friends = api.GetFacebookFriends("fbemail@server.com", "fbpassword", 30000);
    }
  }
}
