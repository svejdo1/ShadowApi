using Barbar.ShadowApi.Interfaces;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;

namespace Barbar.ShadowApi.GitHub
{
  public class FacebookApiRepository : IFacebookApiRepository
  {
    private readonly string m_RootUrl;
    private readonly string m_LocalFolder;

    private IFacebookApi m_Api;
    private AppDomain m_Domain;
    private static object s_SyncRoot = new object();

    public FacebookApiRepository(string rootUrl, string localFolder)
    {
      if (string.IsNullOrEmpty(rootUrl))
      {
        throw new ArgumentNullException("rootUrl");
      }
      if (string.IsNullOrEmpty(localFolder))
      {
        throw new ArgumentNullException("localFolder");
      }

      m_RootUrl = rootUrl;
      m_LocalFolder = localFolder;
    }

    public FacebookApiRepository(string localFolder)
      : this("https://raw.githubusercontent.com/svejdo1/ShadowApi/master/Implementation/", localFolder)
    {
    }

    private LocalConfig GetLocalConfig()
    {
      var serializer = new DataContractJsonSerializer(typeof(LocalConfig));
      string configPath = Path.Combine(m_LocalFolder, "config.json");
      if (File.Exists(configPath))
      {
        using(var stream = File.OpenRead(configPath))
        {
          return (LocalConfig)serializer.ReadObject(stream);
        }
      }

      return new LocalConfig();
    }

    private RemoteConfig GetRemoteConfig()
    {
      var serializer = new DataContractJsonSerializer(typeof(RemoteConfig));
      using(var client = new WebClient())
      {
        using(var stream = client.OpenRead(m_RootUrl + "config.json"))
        {
          return (RemoteConfig)serializer.ReadObject(stream);
        }
      }
    }

    private void DownloadRemoteFile(int remoteConfigVersion, ReleaseConfigFile file)
    {
      string url = string.Format(CultureInfo.InvariantCulture, "{0}v{1}/{2}", m_RootUrl, remoteConfigVersion, file.FileName);
      byte[] buffer = new byte[4096];
      byte[] checksum;
      string filePath = Path.Combine(m_LocalFolder, string.Format(CultureInfo.InvariantCulture, "v{0}", remoteConfigVersion), file.FileName);

      using (var client = new WebClient())
      {
        var stream = client.OpenRead(url);
        using (var md5 = MD5.Create())
        {
          using (var fs = File.Create(filePath))
          using (var cs = new CryptoStream(fs, md5, CryptoStreamMode.Write))
          {
            while (true)
            {
              int read = stream.Read(buffer, 0, buffer.Length);
              if (read <= 0)
              {
                break;
              }

              cs.Write(buffer, 0, read);
            }
          }

          checksum = md5.Hash;
        }
      }

      if (!Enumerable.SequenceEqual(file.MD5, checksum))
      {
        try
        {
          File.Delete(filePath);
        }
        catch(Exception e)
        {
          throw new Exception("MD5 doesn't match.", e);
        }
        throw new Exception("MD5 doesn't match.");
      }
    }

    private LocalConfig UpdateLocalVersion(RemoteConfig remoteConfig)
    {
      string localDirectory = Path.Combine(m_LocalFolder, string.Format(CultureInfo.InvariantCulture, "v{0}", remoteConfig.Version));
      if (!Directory.Exists(localDirectory))
      {
        Directory.CreateDirectory(localDirectory);
      }

      var remoteConfigSerializer = new DataContractJsonSerializer(typeof(ReleaseConfig));
      ReleaseConfig releaseConfig;
      using(var client = new WebClient())
      {
        var stream = client.OpenRead(string.Format(CultureInfo.InvariantCulture, "{0}v{1}/config.json", m_RootUrl, remoteConfig.Version));
        releaseConfig = (ReleaseConfig)remoteConfigSerializer.ReadObject(stream);
      }

      foreach(var file in releaseConfig.Files)
      {
        DownloadRemoteFile(remoteConfig.Version, file);
      }

      var localConfig = new LocalConfig
      {
        StartFileName = remoteConfig.StartFileName,
        StartType = remoteConfig.StartType,
        Version = remoteConfig.Version
      };

      var localConfigSerializer = new DataContractJsonSerializer(typeof(LocalConfig));
      string configPath = Path.Combine(m_LocalFolder, "config.json");
      using(var stream = File.Create(configPath))
      {
        localConfigSerializer.WriteObject(stream, localConfig);
      }

      return localConfig;
    }

    public IFacebookApi GetApi()
    {
      if (m_Api != null)
      {
        return m_Api;
      }

      lock (s_SyncRoot)
      {
        if (m_Api != null)
        {
          return m_Api;
        }
        if (!Directory.Exists(m_LocalFolder))
        {
          Directory.CreateDirectory(m_LocalFolder);
        }
        var localConfig = GetLocalConfig();
        var remoteConfig = GetRemoteConfig();
        if (localConfig.Version != remoteConfig.Version)
        {
          localConfig = UpdateLocalVersion(remoteConfig);
        }
        
        m_Domain = AppDomain.CreateDomain("FacebookAPI", null, new AppDomainSetup
        {
          ApplicationBase = Path.Combine(m_LocalFolder, string.Format("v{0}", localConfig.Version))
        });
        m_Api = (IFacebookApi)m_Domain.CreateInstanceAndUnwrap(localConfig.StartFileName, localConfig.StartType);

      }
      return m_Api;
    }

    public void UnloadApi()
    {
      lock(s_SyncRoot)
      {
        if (m_Api != null && m_Domain != null)
        {
          AppDomain.Unload(m_Domain);
        }
        m_Api = null;
        m_Domain = null;
      }
    }
  }
}
