
namespace Barbar.ShadowApi.Interfaces
{
  public interface IFacebookApiRepository
  {
    IFacebookApi GetApi();
    void UnloadApi();
  }
}
