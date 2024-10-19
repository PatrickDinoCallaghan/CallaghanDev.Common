// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.MessageParallelizer
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lightstreamer.DotNet.Client
{
  internal class MessageParallelizer : IBatchListener
  {
    private ServerManager manager;
    private BatchMonitor monitor;
    private Queue<MessageManager> queue = new Queue<MessageManager>();
    private const int EMPTY = 1;
    private const int BATCHING = 2;
    private const int SENDING = 3;
    private int status = 1;
    private int waitingToBeBatched;
    private int batched;
    private static ILog iLogger = LogManager.GetLogger("com.lightstreamer.ls_client.actions");

    public MessageParallelizer(BatchMonitor monitor, ServerManager manager)
    {
      this.monitor = monitor;
      monitor.Listener = (IBatchListener) this;
      this.manager = manager;
    }

    internal void EnqueueMessage(MessageManager message, int prog)
    {
      lock (this)
      {
        if (this.status == 1)
          this.status = 2;
        this.queue.Enqueue(message);
        ++this.waitingToBeBatched;
        if (this.waitingToBeBatched != 1)
          return;
        this.BatchMessage();
      }
    }

    public void OnMessageBatched()
    {
      lock (this)
      {
        --this.waitingToBeBatched;
        ++this.batched;
        this.BatchMessage();
        if (this.status != 2)
          return;
        this.PrepareCloseBatch();
        this.status = 3;
      }
    }

    private void OnProcessed()
    {
      lock (this)
      {
        --this.batched;
        if (this.batched == 0 && this.waitingToBeBatched == 0)
          this.status = 1;
        else if (this.batched > 0)
        {
          this.PrepareCloseBatch();
          this.status = 3;
        }
        else
          this.status = 2;
      }
    }

    private void PrepareCloseBatch() => Task.Run((Action) (() => MessageParallelizer.PrepareCloseBatchImpl((object) this)));

    private static void PrepareCloseBatchImpl(object stateInfo)
    {
      MessageParallelizer messageParallelizer = MessageParallelizer.castParallelizer(stateInfo);
      if (messageParallelizer == null)
        return;
      BatchMonitor monitor = messageParallelizer.getMonitor();
      lock (monitor)
      {
        if (monitor.Empty)
          return;
        ServerManager manager = messageParallelizer.getManager();
        manager.CloseMessageBatch();
        try
        {
          manager.BatchMessageRequests(0);
        }
        catch (PhaseException ex)
        {
        }
      }
    }

    private void BatchMessage()
    {
      lock (this)
        Task.Run((Action) (() => MessageParallelizer.BatchMessageImpl((object) this)));
    }

    private static void BatchMessageImpl(object stateInfo)
    {
      MessageParallelizer messageParallelizer = MessageParallelizer.castParallelizer(stateInfo);
      if (messageParallelizer == null)
        return;
      MessageManager messageManager = (MessageManager) null;
      lock (stateInfo)
      {
        try
        {
          messageManager = messageParallelizer.getQueue().Dequeue();
        }
        catch (InvalidOperationException ex)
        {
        }
      }
      if (messageManager == null)
        return;
      try
      {
        ServerManager manager = messageParallelizer.getManager();
        MessageManager message = messageManager;
        int prog = message.Prog;
        manager.SendMessage(message, prog);
      }
      catch (PhaseException ex)
      {
      }
      catch (PushConnException ex)
      {
      }
      catch (PushServerException ex)
      {
      }
      catch (PushUserException ex)
      {
      }
      catch (SubscrException ex)
      {
      }
      finally
      {
        messageParallelizer.OnProcessed();
      }
    }

    internal Queue<MessageManager> getQueue() => this.queue;

    internal ServerManager getManager() => this.manager;

    internal BatchMonitor getMonitor() => this.monitor;

    private static MessageParallelizer castParallelizer(object obj)
    {
      if (obj == null)
      {
        MessageParallelizer.iLogger.Warn("Unexpected null context for parallelizer");
        return (MessageParallelizer) null;
      }
      try
      {
        return (MessageParallelizer) obj;
      }
      catch (InvalidCastException ex)
      {
        MessageParallelizer.iLogger.Warn("Wrong Object instance for parallelizer");
        return (MessageParallelizer) null;
      }
    }
  }
}
