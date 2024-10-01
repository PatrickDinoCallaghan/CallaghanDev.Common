// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.ExtConnectionListener
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;
using System.Threading;

namespace Lightstreamer.DotNet.Client
{
  internal class ExtConnectionListener : IConnectionListener
  {
    private IConnectionListener target;
    private NotificationQueue queue = new NotificationQueue("Stream-sense queue", false);
    private bool streamingAppended;
    private Exception connFailure;
    private ManualResetEvent streamingTested = new ManualResetEvent(false);

    public ExtConnectionListener(IConnectionListener target) => this.target = target;

    public void FlushAndStart() => this.queue.Start(true);

    public bool WaitStreamingTimeoutAnswer()
    {
      this.streamingTested.WaitOne();
      if (this.connFailure != null)
        throw this.connFailure;
      return this.streamingAppended;
    }

    public void OnConnectionEstablished() => this.queue.Add((NotificationQueue.Notify) (() => this.target.OnConnectionEstablished()));

    public void OnSessionStarted(bool isPolling) => this.queue.Add((NotificationQueue.Notify) (() => this.target.OnSessionStarted(isPolling)));

    public void OnStreamingReturned() => this.streamingTested.Set();

    public void OnNewBytes(long bytes) => this.queue.Add((NotificationQueue.Notify) (() => this.target.OnNewBytes(bytes)));

    public void OnDataError(PushServerException e) => this.queue.Add((NotificationQueue.Notify) (() => this.target.OnDataError(e)));

    public void OnActivityWarning(bool warningOn) => this.queue.Add((NotificationQueue.Notify) (() => this.target.OnActivityWarning(warningOn)));

    public void OnClose()
    {
      this.queue.Add((NotificationQueue.Notify) (() => this.target.OnClose()));
      this.queue.End();
    }

    public void OnEnd(int cause)
    {
      this.queue.Add((NotificationQueue.Notify) (() => this.target.OnEnd(cause)));
      this.streamingTested.Set();
    }

    public void OnConnectTimeout(PushServerException e)
    {
      this.streamingAppended = true;
      this.streamingTested.Set();
      this.queue.Add((NotificationQueue.Notify) (() => this.target.OnFailure(e)));
    }

    public void OnConnectException(Exception e)
    {
      this.connFailure = e;
      this.streamingTested.Set();
    }

    public void OnFailure(PushServerException e)
    {
      this.queue.Add((NotificationQueue.Notify) (() => this.target.OnFailure(e)));
      this.streamingTested.Set();
    }

    public void OnFailure(PushConnException e)
    {
      this.queue.Add((NotificationQueue.Notify) (() => this.target.OnFailure(e)));
      this.streamingTested.Set();
    }
  }
}
