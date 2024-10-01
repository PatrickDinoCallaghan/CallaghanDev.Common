// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.Log.ILogEmpty
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;

namespace Lightstreamer.DotNet.Client.Log
{
  internal class ILogEmpty : ILogger
  {
    public void Error(string p)
    {
    }

    public void Error(string p, Exception e)
    {
    }

    public void Warn(string p)
    {
    }

    public void Warn(string p, Exception e)
    {
    }

    public void Info(string p)
    {
    }

    public void Info(string p, Exception e)
    {
    }

    public void Debug(string p)
    {
    }

    public void Debug(string p, Exception e)
    {
    }

    public void Fatal(string p)
    {
    }

    public void Fatal(string p, Exception e)
    {
    }

    public bool IsDebugEnabled => false;

    public bool IsInfoEnabled => false;

    public bool IsWarnEnabled => false;

    public bool IsErrorEnabled => false;

    public bool IsFatalEnabled => false;
  }
}
