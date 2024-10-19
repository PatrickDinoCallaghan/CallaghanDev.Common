// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.BatchMonitor
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

namespace Lightstreamer.DotNet.Client
{
  internal class BatchMonitor
  {
    private bool empty = true;
    private IBatchListener listener;
    private bool unlimitedBatch;
    private int pendingCalls;

    internal virtual bool Filled
    {
      get
      {
        lock (this)
          return !this.unlimitedBatch && this.pendingCalls == 0;
      }
    }

    internal virtual bool Empty
    {
      get
      {
        lock (this)
          return this.empty;
      }
    }

    public virtual IBatchListener Listener
    {
      set
      {
        lock (this)
          this.listener = value;
      }
    }

    internal virtual bool Unlimited
    {
      get
      {
        lock (this)
          return this.unlimitedBatch;
      }
    }

    internal virtual void Expand(int batchSize)
    {
      lock (this)
      {
        if (batchSize <= 0)
        {
          this.unlimitedBatch = true;
        }
        else
        {
          if (this.unlimitedBatch)
            return;
          this.pendingCalls += batchSize;
        }
      }
    }

    internal virtual void UseOne()
    {
      lock (this)
      {
        this.empty = false;
        if (this.unlimitedBatch || this.pendingCalls <= 0)
          return;
        --this.pendingCalls;
      }
    }

    internal virtual void BatchedOne()
    {
      lock (this)
      {
        if (this.listener == null)
          return;
        this.listener.OnMessageBatched();
      }
    }

    internal virtual void Clear()
    {
      lock (this)
      {
        this.unlimitedBatch = false;
        this.pendingCalls = 0;
        this.empty = true;
      }
    }

    public bool HasListener()
    {
      lock (this)
        return this.listener != null;
    }
  }
}
