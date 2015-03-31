using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace Barbar.ShadowApi.GitHub
{
  public static class Packer
  {
    public static void Pack(string sourceFolder, string targetFolder, string mainAssemblyName, string mainType)
    {
      // read current config
      var configPath = Path.Combine(targetFolder, "config.json");
      var serializer = new DataContractJsonSerializer(typeof(RemoteConfig));
      RemoteConfig config;
      using(var file = File.OpenRead(configPath))
      {
        config = (RemoteConfig)serializer.ReadObject(file);
      }

      // lets put there new version
      config.Version++;
      config.StartFileName = mainAssemblyName;
      config.StartType = mainType;

      string targetVersionFolder = Path.Combine(targetFolder, string.Format("v{0}", config.Version));
      Directory.CreateDirectory(targetVersionFolder);

      // copy files & prepare release config
      var releaseConfig = new ReleaseConfig { Files = new List<ReleaseConfigFile>() };
      foreach(var file in Directory.GetFiles(sourceFolder, "*.dll"))
      {
        byte[] checksum;
        byte[] buffer = new byte[1024 * 16];
        using (var md5 = MD5.Create())
        {
          using (var fs = File.Create(Path.Combine(targetVersionFolder, Path.GetFileName(file))))
          using (var cs = new CryptoStream(fs, md5, CryptoStreamMode.Write))
          using(var src = File.OpenRead(file))
          {
            while(true)
            {
              int read = src.Read(buffer, 0, buffer.Length);
              if (read <= 0)
              {
                break;
              }
              cs.Write(buffer, 0, read);
            }
          }
          checksum = md5.Hash;
        }

        releaseConfig.Files.Add(new ReleaseConfigFile { FileName = Path.GetFileName(file), MD5 = checksum });
      }

      // write release config
      var releaseConfigSerializer = new DataContractJsonSerializer(typeof(ReleaseConfig));
      using(var file = File.Create(Path.Combine(targetVersionFolder, "config.json")))
      {
        releaseConfigSerializer.WriteObject(file, releaseConfig);
      }

      // write remote config
      using (var file = File.Create(Path.Combine(targetFolder, "config.json")))
      {
        serializer.WriteObject(file, config);
      }
    }
  }
}
