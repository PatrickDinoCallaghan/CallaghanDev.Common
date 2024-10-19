// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.Log.LogManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System.Collections.Generic;

namespace Lightstreamer.DotNet.Client.Log
{
  internal class LogManager
  {
    private static IDictionary<string, ILog> logInstances = (IDictionary<string, ILog>) new Dictionary<string, ILog>();
    private static ILoggerProvider currentLoggerProvider = (ILoggerProvider) null;

    internal static void SetLoggerProvider(ILoggerProvider ilp)
    {
      lock (LogManager.logInstances)
      {
        LogManager.currentLoggerProvider = ilp;
        foreach (KeyValuePair<string, ILog> logInstance in (IEnumerable<KeyValuePair<string, ILog>>) LogManager.logInstances)
        {
          if (ilp == null)
            logInstance.Value.setWrappedInstance((ILogger) null);
          else
            logInstance.Value.setWrappedInstance(LogManager.currentLoggerProvider.GetLogger(logInstance.Key));
        }
      }
    }

    internal static ILog GetLogger(string category)
    {
      lock (LogManager.logInstances)
      {
        if (!LogManager.logInstances.ContainsKey(category))
          LogManager.logInstances[category] = LogManager.currentLoggerProvider == null ? new ILog() : new ILog(LogManager.currentLoggerProvider.GetLogger(category));
        return LogManager.logInstances[category];
      }
    }
  }
}
