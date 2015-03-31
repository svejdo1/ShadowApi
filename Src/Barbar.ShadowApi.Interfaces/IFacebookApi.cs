using System.Collections.Generic;

namespace Barbar.ShadowApi.Interfaces
{
    public interface IFacebookApi
    {
      IList<FacebookUser> GetFacebookFriends(string email, string password, int? maxTimeoutInMilliseconds);
    }
}
