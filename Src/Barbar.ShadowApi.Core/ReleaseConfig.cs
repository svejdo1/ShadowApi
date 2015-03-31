using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Barbar.ShadowApi.GitHub
{
  [DataContract]
  public class ReleaseConfig
  {
    [DataMember]
    public List<ReleaseConfigFile> Files { get; set; }
  }
}
