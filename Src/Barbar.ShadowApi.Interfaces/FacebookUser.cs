using System;
using System.Runtime.Serialization;

namespace Barbar.ShadowApi.Interfaces
{
  [Serializable]
  [DataContract]
  public class FacebookUser
  {
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public long Id { get; set; }
  }
}
