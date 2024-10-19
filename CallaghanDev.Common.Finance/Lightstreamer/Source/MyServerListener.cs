// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.MyServerListener
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;

namespace Lightstreamer.DotNet.Client
{
  internal class MyServerListener : ServerManager.IServerListener
  {
    private LSClient owner;
    private IConnectionListener initialListener;
    private int currPhase;
    private bool failed;
    private NotificationQueue queue = new NotificationQueue("Notifications queue", false);
    private NotificationQueue messageQueue = new NotificationQueue("Messages notifications queue", false);
    private static ILog actionsLogger = LogManager.GetLogger("com.lightstreamer.ls_client.actions");

    internal MyServerListener(LSClient owner, IConnectionListener initialListener, int currPhase)
    {
      this.owner = owner;
      this.initialListener = initialListener;
      this.currPhase = currPhase;
      this.queue.Start(false);
      this.messageQueue.Start(false);
    }

    public virtual void OnConnectionEstablished() => this.queue.Add((NotificationQueue.Notify) (() => this.initialListener.OnConnectionEstablished()));

    public virtual void OnSessionStarted(bool isPolling) => this.queue.Add((NotificationQueue.Notify) (() => this.initialListener.OnSessionStarted(isPolling)));

    public virtual bool OnUpdate(ITableManager table, ServerUpdateEvent values)
    {
      if (this.owner.GetActiveListener(this.currPhase) == null)
        return false;
      try
      {
        table.DoUpdate(values);
      }
      catch (PushServerException ex)
      {
        MyServerListener.actionsLogger.Debug("Error in received values", (Exception) ex);
        this.OnDataError(ex);
      }
      return true;
    }

    public bool OnMessageOutcome(
      MessageManager message,
      SequenceHandler sequence,
      ServerUpdateEvent values,
      Exception problem)
    {
      if (!(values != null ? message.SetOutcome(values) : message.SetAbort(problem)))
      {
        this.OnDataError(new PushServerException(13));
        return false;
      }
      if (message.Sequence == "UNORDERED_MESSAGES")
      {
        int num = message.Prog;
        this.messageQueue.Add((NotificationQueue.Notify) (() =>
        {
          MessageManager it = sequence.IfHasOutcomeExtractIt(num);
          if (it == null)
            return;
          try
          {
            it.NotifyListener();
          }
          catch (PushServerException ex)
          {
            this.OnDataError(ex);
          }
          catch (Exception ex)
          {
          }
        }));
      }
      else
        this.messageQueue.Add((NotificationQueue.Notify) (() =>
        {
          MessageManager it;
          while ((it = sequence.IfFirstHasOutcomeExtractIt()) != null)
          {
            try
            {
              it.NotifyListener();
            }
            catch (PushServerException ex)
            {
              this.OnDataError(ex);
            }
            catch (Exception ex)
            {
            }
          }
        }));
      return true;
    }

    public void OnEndMessages() => this.messageQueue.End();

    public virtual bool OnNewBytes(long bytes)
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnNewBytes(bytes)));
        return true;
      }
    }

    public virtual bool OnDataError(PushServerException e)
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnDataError(e)));
        return true;
      }
    }

    public virtual void OnStreamingReturned()
    {
      if (!(this.initialListener is ExtConnectionListener))
        return;
      this.queue.Add((NotificationQueue.Notify) (() => ((ExtConnectionListener) this.initialListener).OnStreamingReturned()));
    }

    public virtual bool OnEnd(int cause)
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.failed = true;
        this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnEnd(cause)));
        return true;
      }
    }

    public virtual bool OnActivityWarning(bool warningOn)
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnActivityWarning(warningOn)));
        return true;
      }
    }

    public virtual bool OnReconnectTimeout()
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.failed = true;
        PushServerException exc = new PushServerException(11);
        if (activeListener is ExtConnectionListener)
          this.queue.Add((NotificationQueue.Notify) (() => ((ExtConnectionListener) activeListener).OnConnectTimeout(exc)));
        else
          this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnFailure(exc)));
        return true;
      }
    }

    public virtual void OnConnectTimeout()
    {
      if (!(this.initialListener is ExtConnectionListener))
        return;
      PushServerException exc = new PushServerException(11);
      this.queue.Add((NotificationQueue.Notify) (() => ((ExtConnectionListener) this.initialListener).OnConnectTimeout(exc)));
    }

    public virtual void OnConnectException(Exception e)
    {
      if (!(this.initialListener is ExtConnectionListener))
        return;
      this.queue.Add((NotificationQueue.Notify) (() => ((ExtConnectionListener) this.initialListener).OnConnectException(e)));
    }

    public virtual bool OnFailure(PushServerException e)
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.failed = true;
        this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnFailure(e)));
        return true;
      }
    }

    public virtual bool OnFailure(PushConnException e)
    {
      IConnectionListener activeListener = this.owner.GetActiveListener(this.currPhase);
      lock (this)
      {
        if (activeListener == null || this.failed)
          return false;
        this.failed = true;
        this.queue.Add((NotificationQueue.Notify) (() => activeListener.OnFailure(e)));
        return true;
      }
    }

    public virtual bool OnClose()
    {
      if (this.owner.GetActiveListener(this.currPhase) == null)
        return false;
      this.owner.CloseConnection();
      return true;
    }

    public virtual void OnClosed(IConnectionListener closedListener)
    {
      lock (this)
      {
        this.failed = true;
        if (closedListener != null)
          this.queue.Add((NotificationQueue.Notify) (() => closedListener.OnClose()));
        this.queue.End();
      }
    }
  }
}
