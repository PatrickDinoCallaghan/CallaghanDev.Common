// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.MessageManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;

namespace Lightstreamer.DotNet.Client
{
  internal class MessageManager
  {
    private MessageInfo message;
    private ISendMessageListener listener;
    private int prog;
    private Exception problem;
    private bool aborted;
    private ServerUpdateEvent processed;

    internal MessageManager(MessageInfo message, ISendMessageListener listener)
    {
      this.message = message;
      this.listener = listener;
    }

    internal void Enqueued(int prog)
    {
      lock (this)
        this.prog = prog;
    }

    internal int Prog
    {
      get
      {
        lock (this)
          return this.prog;
      }
    }

    internal int DelayTimeout
    {
      get
      {
        lock (this)
          return this.message.DelayTimeout;
      }
    }

    internal string Sequence
    {
      get
      {
        lock (this)
          return this.message.Sequence;
      }
    }

    internal string Message
    {
      get
      {
        lock (this)
          return this.message.Message;
      }
    }

    public bool SetAbort(Exception problem)
    {
      lock (this)
      {
        if (this.HasOutcome())
          return false;
        this.aborted = true;
        this.problem = problem;
        return true;
      }
    }

    public bool SetOutcome(ServerUpdateEvent values)
    {
      lock (this)
      {
        if (this.HasOutcome())
          return false;
        this.processed = values;
        return true;
      }
    }

    public bool HasOutcome()
    {
      lock (this)
        return this.aborted || this.processed != null;
    }

    public void NotifyListener()
    {
      lock (this)
      {
        if (!this.HasOutcome())
          throw new PushServerException(13);
        if (this.listener == null)
          return;
        if (this.aborted)
        {
          this.listener.OnAbort(this.message, this.prog, this.problem);
        }
        else
        {
          if (this.processed == null)
            return;
          if (this.processed.ErrorMessage == null)
            this.listener.OnProcessed(this.message, this.prog);
          else
            this.listener.OnError(this.processed.ErrorCode, this.processed.ErrorMessage, this.message, this.prog);
        }
      }
    }
  }
}
