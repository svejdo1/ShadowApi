using System.Runtime.Serialization;

namespace Barbar.ShadowApi.GitHub
{
  [DataContract]
  public class RemoteConfig
  {
    [DataMember]
    public int Version { get; set; }
    [DataMember]
    public string StartFileName { get; set; }
    [DataMember]
    public string StartType { get; set; }
  }
}
