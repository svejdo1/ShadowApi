using System.Runtime.Serialization;

namespace Barbar.ShadowApi.GitHub
{
  [DataContract]
  public class ReleaseConfigFile
  {
    [DataMember]
    public string FileName { get; set; }
    [DataMember]
    public byte[] MD5 { get; set; }
  }
}
