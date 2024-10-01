// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.ConnectionInfo
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System.Collections.Generic;

namespace Lightstreamer.DotNet.Client
{
  public class ConnectionInfo
  {
    private int connectTimeout = -1;
    private int readTimeout = -1;
    private long probeTimeoutMillis = 3000;
    private long probeWarningMillis = 2000;
    private long keepaliveMillis;
    private long reconnectionTimeoutMillis = 5000;
    private long recoveryTimeoutMillis = 5000;
    private bool enableStreamSense = true;
    private long streamingTimeoutMillis = 5000;
    private long contentLength = 50000000;
    private string pushServerUrl;
    private string pushServerControlUrl;
    private string user;
    private string password;
    private string adapter;
    private bool isPolling;
    internal bool useGetForStreaming;
    private long pollingMillis;
    private long pollingIdleMillis = 30000;
    private IDictionary<string, string> httpExtraHeaders;
    private ConnectionConstraints constraints = new ConnectionConstraints();

    public int ConnectTimeoutMillis
    {
      get => this.connectTimeout;
      set => this.connectTimeout = value;
    }

    public int ReadTimeoutMillis
    {
      get => this.readTimeout;
      set => this.readTimeout = value;
    }

    public long ProbeTimeoutMillis
    {
      set => this.probeTimeoutMillis = value;
      get => this.probeTimeoutMillis;
    }

    public long ProbeWarningMillis
    {
      set => this.probeWarningMillis = value;
      get => this.probeWarningMillis;
    }

    public long KeepaliveMillis
    {
      set => this.keepaliveMillis = value;
      get => this.keepaliveMillis;
    }

    public long ReconnectionTimeoutMillis
    {
      set => this.reconnectionTimeoutMillis = value;
      get => this.reconnectionTimeoutMillis;
    }

    public long RecoveryTimeoutMillis
    {
      set => this.recoveryTimeoutMillis = value;
      get => this.recoveryTimeoutMillis;
    }

    public bool EnableStreamSense
    {
      set => this.enableStreamSense = value;
      get => this.enableStreamSense;
    }

    public long StreamingTimeoutMillis
    {
      set => this.streamingTimeoutMillis = value;
      get => this.streamingTimeoutMillis;
    }

    public long ContentLength
    {
      set => this.contentLength = value;
      get => this.contentLength;
    }

    public string PushServerUrl
    {
      set => this.pushServerUrl = value;
      get => this.pushServerUrl;
    }

    public string PushServerControlUrl
    {
      set => this.pushServerControlUrl = value;
      get => this.pushServerControlUrl;
    }

    public string User
    {
      set => this.user = value;
      get => this.user;
    }

    public string Password
    {
      set => this.password = value;
      get => this.password;
    }

    public string Adapter
    {
      set => this.adapter = value;
      get => this.adapter;
    }

    public bool Polling
    {
      set => this.isPolling = value;
      get => this.isPolling;
    }

    public long PollingMillis
    {
      set => this.pollingMillis = value;
      get => this.pollingMillis;
    }

    public long PollingIdleMillis
    {
      set => this.pollingIdleMillis = value;
      get => this.pollingIdleMillis;
    }

    public IDictionary<string, string> HttpExtraHeaders
    {
      set => this.httpExtraHeaders = value;
      get => this.httpExtraHeaders;
    }

    public ConnectionConstraints Constraints
    {
      get => this.constraints;
      internal set => this.constraints = value;
    }

    internal string GetAdapterSet() => this.Adapter != null ? this.Adapter : "DEFAULT";

    internal bool StartsWithPoll() => this.Polling || this.useGetForStreaming;

    public override string ToString() => this.PushServerUrl + " - " + this.Constraints.ToString();

    public virtual object Clone()
    {
      ConnectionInfo connectionInfo = (ConnectionInfo) this.MemberwiseClone();
      connectionInfo.Constraints = (ConnectionConstraints) connectionInfo.Constraints.Clone();
      return (object) connectionInfo;
    }

    public override bool Equals(object other)
    {
      if (other == this)
        return true;
      if (other == null)
        return false;
      ConnectionInfo connectionInfo = (ConnectionInfo) other;
      return connectionInfo.ProbeTimeoutMillis == this.ProbeTimeoutMillis && connectionInfo.ProbeWarningMillis == this.ProbeWarningMillis && connectionInfo.KeepaliveMillis == this.KeepaliveMillis && connectionInfo.ReconnectionTimeoutMillis == this.ReconnectionTimeoutMillis && connectionInfo.RecoveryTimeoutMillis == this.RecoveryTimeoutMillis && connectionInfo.EnableStreamSense == this.EnableStreamSense && connectionInfo.StreamingTimeoutMillis == this.StreamingTimeoutMillis && connectionInfo.ContentLength == this.ContentLength && connectionInfo.Polling == this.Polling && connectionInfo.useGetForStreaming == this.useGetForStreaming && connectionInfo.PollingMillis == this.PollingMillis && connectionInfo.PollingIdleMillis == this.PollingIdleMillis && this.Equals((object) connectionInfo.PushServerUrl, (object) this.PushServerUrl) && this.Equals((object) connectionInfo.PushServerControlUrl, (object) this.PushServerControlUrl) && this.Equals((object) connectionInfo.User, (object) this.User) && this.Equals((object) connectionInfo.Password, (object) this.Password) && this.Equals((object) connectionInfo.GetAdapterSet(), (object) this.GetAdapterSet()) && this.Equals((object) connectionInfo.Constraints, (object) this.Constraints);
    }

    private bool Equals(object o1, object o2)
    {
      if (o1 == null && o2 == null)
        return true;
      return o1 != null && o2 != null && o1.Equals(o2);
    }

    public override int GetHashCode() => base.GetHashCode();
  }
}
