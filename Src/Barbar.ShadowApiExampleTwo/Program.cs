using Barbar.ShadowApi.GitHub;
using System;
using System.IO;

namespace Barbar.ShadowApiExampleTwo
{
  class Program
  {
    /// <summary>
    /// This demo how to use api indirectly; it uses github as repository for built facebook
    /// api accessor - should facebook change ui, you don't have to release new application,
    /// just update the connector
    /// Disadvantage - for first api retrieval you have to query github if there is a new version
    /// available
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      string currentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);

      var repository = new FacebookApiRepository(Path.Combine(currentDirectory, "FacebookApi"));
      var api = repository.GetApi();
      var friends = api.GetFacebookFriends("fbemail@server.com", "fbpassword", 30000);
    }
  }
}
