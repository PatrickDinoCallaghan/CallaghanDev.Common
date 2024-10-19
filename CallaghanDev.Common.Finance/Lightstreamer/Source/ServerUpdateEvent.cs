// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.ServerUpdateEvent
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lightstreamer.DotNet.Client
{
  internal class ServerUpdateEvent
  {
    internal static readonly string UNCHANGED = new StringBuilder(nameof (UNCHANGED)).ToString();
    private string tableCode;
    private string itemCode;
    private IList values;
    private bool eos;
    private int overflow;
    private int messageProg;
    private string messageSequence;
    private int errorCode;
    private string errorMessage;
    private bool isLoop;
    private long holdingMillis;
    private long recoveryProg;

    internal virtual int TableCode => this.tableCode == null ? -1 : int.Parse(this.tableCode);

    internal virtual int ItemCode => this.itemCode == null ? -1 : int.Parse(this.itemCode);

    internal virtual int Overflow => this.overflow;

    internal virtual bool EOS => this.eos;

    internal virtual int Size => this.values.Count;

    internal virtual string[] Array
    {
      get
      {
        string[] array = new string[this.values.Count];
        for (int index = 0; index < array.Length; ++index)
          array[index] = (string) this.values[index];
        return array;
      }
    }

    internal bool TableUpdate => this.tableCode != null;

    internal int MessageProg => this.messageProg;

    internal string MessageSequence => this.messageSequence;

    internal int ErrorCode => this.errorCode;

    internal string ErrorMessage => this.errorMessage;

    public long HoldingMillis => this.holdingMillis;

    public long RecoveryProg => this.recoveryProg;

    public bool Loop => this.isLoop;

    internal ServerUpdateEvent(string tableCode, string itemCode)
    {
      this.tableCode = tableCode;
      this.itemCode = itemCode;
      this.values = (IList) new List<string>();
    }

    internal ServerUpdateEvent(string tableCode, string itemCode, bool eos)
    {
      this.tableCode = tableCode;
      this.itemCode = itemCode;
      this.eos = eos;
      if (eos)
        return;
      this.values = (IList) new List<string>();
    }

    internal ServerUpdateEvent(string tableCode, string itemCode, int overflow)
    {
      this.tableCode = tableCode;
      this.itemCode = itemCode;
      this.overflow = overflow;
    }

    internal ServerUpdateEvent(string messageSequence, int messageProg)
    {
      this.messageSequence = messageSequence;
      this.messageProg = messageProg;
    }

    internal ServerUpdateEvent(
      string messageSequence,
      int messageProg,
      int errorCode,
      string errorMessage)
    {
      this.messageSequence = messageSequence;
      this.messageProg = messageProg;
      this.errorCode = errorCode;
      this.errorMessage = errorMessage;
    }

    public ServerUpdateEvent(long holdingMillis)
    {
      this.holdingMillis = holdingMillis;
      this.recoveryProg = -1L;
      this.isLoop = true;
    }

    public ServerUpdateEvent(long holdingMillis, long recoveryProg)
    {
      this.holdingMillis = holdingMillis;
      this.recoveryProg = recoveryProg;
      this.isLoop = true;
    }

    internal virtual void AddValue(string val) => this.values.Add((object) val);

    internal virtual string GetValue(int pos) => (string) this.values[pos];

    internal virtual IDictionary GetMap(string[] fieldNames)
    {
      IDictionary map = (IDictionary) new Dictionary<string, string>();
      for (int index = 0; index < fieldNames.Length; ++index)
      {
        string str = (string) this.values[index];
        if ((object) str != (object) ServerUpdateEvent.UNCHANGED)
          map[(object) fieldNames[index]] = (object) str;
      }
      return map;
    }

    public override string ToString() => this.TableUpdate ? "event for item n°" + this.itemCode + " in table n°" + this.tableCode + " with values " + CollectionsSupport.ToString((ICollection) this.values) : "event for message n°" + (object) this.messageProg + " in sequence " + this.messageSequence + " with error message (if any): " + this.errorMessage;
  }
}
