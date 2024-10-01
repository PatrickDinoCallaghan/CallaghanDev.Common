// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.NotificationQueue
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lightstreamer.DotNet.Client
{
  internal class NotificationQueue
  {
    private bool closed;
    private string name;
    private List<NotificationQueue.Notify> myList = new List<NotificationQueue.Notify>();
    private ManualResetEvent available = new ManualResetEvent(false);

    public NotificationQueue(string name, bool started)
    {
      this.name = name;
      if (!started)
        return;
      lock (this)
        this.Start(true);
    }

    public void Add(NotificationQueue.Notify fun)
    {
      lock (this)
      {
        if (this.closed)
          return;
        this.myList.Add(fun);
        this.available.Set();
      }
    }

    public void Start(bool isBackground) => Task.Run((Action) (() => this.dequeue()));

    public void End()
    {
      lock (this)
      {
        this.closed = true;
        this.available.Set();
      }
    }

    private void dequeue()
    {
      while (true)
      {
        NotificationQueue.Notify notify = (NotificationQueue.Notify) null;
        lock (this)
        {
          if (this.myList.Count > 0)
          {
            notify = this.myList[0];
            this.myList.RemoveAt(0);
          }
          else
          {
            if (this.closed)
              break;
            this.available.Reset();
          }
        }
        if (notify != null)
        {
          try
          {
            notify();
          }
          catch (Exception ex)
          {
          }
        }
        else
          this.available.WaitOne();
      }
    }

    public delegate void Notify();
  }
}
