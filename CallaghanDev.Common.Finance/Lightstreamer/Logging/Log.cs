// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.Log.ILog
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;

namespace Lightstreamer.DotNet.Client.Log
{
  internal class ILog : ILogger
  {
    private static ILogger placeholder = (ILogger) new ILogEmpty();
    private ILogger wrappedLogger = ILog.placeholder;

    internal ILog()
    {
    }

    internal ILog(ILogger iLogger) => this.wrappedLogger = iLogger;

    internal void setWrappedInstance(ILogger iLogger)
    {
      if (iLogger == null)
        this.wrappedLogger = ILog.placeholder;
      else
        this.wrappedLogger = iLogger;
    }

    public void Error(string p) => this.wrappedLogger.Error(p);

    public void Error(string p, Exception e) => this.wrappedLogger.Error(p, e);

    public void Warn(string p) => this.wrappedLogger.Warn(p);

    public void Warn(string p, Exception e) => this.wrappedLogger.Warn(p, e);

    public void Info(string p) => this.wrappedLogger.Info(p);

    public void Info(string p, Exception e) => this.wrappedLogger.Info(p, e);

    public void Debug(string p) => this.wrappedLogger.Debug(p);

    public void Debug(string p, Exception e) => this.wrappedLogger.Debug(p, e);

    public void Fatal(string p) => this.wrappedLogger.Fatal(p);

    public void Fatal(string p, Exception e) => this.wrappedLogger.Fatal(p);

    public bool IsDebugEnabled => this.wrappedLogger.IsDebugEnabled;

    public bool IsInfoEnabled => this.wrappedLogger.IsInfoEnabled;

    public bool IsWarnEnabled => this.wrappedLogger.IsWarnEnabled;

    public bool IsErrorEnabled => this.wrappedLogger.IsErrorEnabled;

    public bool IsFatalEnabled => this.wrappedLogger.IsFatalEnabled;
  }
}
