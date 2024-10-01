// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.SequenceHandler
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System.Collections.Generic;

namespace Lightstreamer.DotNet.Client
{
  internal class SequenceHandler
  {
    private Dictionary<int, MessageManager> messages = new Dictionary<int, MessageManager>();
    private int peek = 1;
    private int prog;

    public int Enqueue(MessageManager message)
    {
      lock (this)
      {
        ++this.prog;
        this.messages[this.prog] = message;
        message.Enqueued(this.prog);
        return this.prog;
      }
    }

    public MessageManager[] Iterator()
    {
      lock (this)
      {
        MessageManager[] array = new MessageManager[this.messages.Count];
        this.messages.Values.CopyTo(array, 0);
        return array;
      }
    }

    public MessageManager IfFirstHasOutcomeExtractIt()
    {
      lock (this)
      {
        MessageManager it = this.IfHasOutcomeExtractIt(this.peek);
        if (it == null)
          return (MessageManager) null;
        ++this.peek;
        return it;
      }
    }

    public MessageManager GetMessage(int prog)
    {
      lock (this)
        return this.messages.Count <= 0 || !this.messages.ContainsKey(prog) ? (MessageManager) null : this.messages[prog];
    }

    public MessageManager IfHasOutcomeExtractIt(int num)
    {
      lock (this)
      {
        MessageManager message = this.GetMessage(num);
        if (message == null || !message.HasOutcome())
          return (MessageManager) null;
        this.messages.Remove(num);
        return message;
      }
    }
  }
}
