// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.UpdateInfoImpl
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;
using System.Collections;
using System.Text;

namespace Lightstreamer.DotNet.Client
{
  internal class UpdateInfoImpl : IUpdateInfo
  {
    private FullTableManager.ItemInfo info;
    private string[] prevState;
    private string[] updEvent;
    private bool isSnapshot;

    internal UpdateInfoImpl(
      FullTableManager.ItemInfo info,
      string[] prevState,
      string[] updEvent,
      bool isSnapshot)
    {
      this.info = info;
      this.prevState = prevState;
      this.updEvent = updEvent;
      this.isSnapshot = isSnapshot;
    }

    public int ItemPos => this.info.pos;

    public int GetItemPos() => this.info.pos;

    public string ItemName => this.info.name;

    public string GetItemName() => this.info.name;

    public bool IsValueChanged(int fieldPos) => this.updEvent[this.GetIndex(fieldPos)] != ServerUpdateEvent.UNCHANGED;

    public bool IsValueChanged(string fieldName) => this.updEvent[this.GetIndex(fieldName)] != ServerUpdateEvent.UNCHANGED;

    public string GetNewValue(int fieldPos)
    {
      int index = this.GetIndex(fieldPos);
      string str = this.updEvent[index];
      return str != ServerUpdateEvent.UNCHANGED ? str : this.prevState[index];
    }

    public string GetNewValue(string fieldName)
    {
      int index = this.GetIndex(fieldName);
      string str = this.updEvent[index];
      return str != ServerUpdateEvent.UNCHANGED ? str : this.prevState[index];
    }

    public string GetOldValue(int fieldPos) => this.prevState[this.GetIndex(fieldPos)];

    public string GetOldValue(string fieldName) => this.prevState[this.GetIndex(fieldName)];

    public int NumFields => this.prevState.Length;

    public int GetNumFields() => this.prevState.Length;

    public bool Snapshot => this.isSnapshot;

    public bool IsSnapshot() => this.isSnapshot;

    private int GetIndex(int fieldPos)
    {
      if (fieldPos <= 0 || fieldPos > this.prevState.Length)
        throw new ArgumentException();
      return fieldPos - 1;
    }

    private int GetIndex(string fieldName)
    {
      IDictionary fieldIndexMap = this.info.GetFieldIndexMap();
      if (fieldIndexMap == null)
        throw new ArgumentException();
      return fieldIndexMap.Contains((object) fieldName) ? (int) fieldIndexMap[(object) fieldName] : throw new ArgumentException();
    }

    public override string ToString()
    {
      int num = this.updEvent.Length - 1;
      if (num < 0)
        return "[]";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("[ ");
      int index = 0;
      while (true)
      {
        string str1 = this.updEvent[index];
        if (str1 == ServerUpdateEvent.UNCHANGED)
        {
          string str2 = this.prevState[index];
          stringBuilder.Append('(');
          stringBuilder.Append(str2 ?? "null");
          stringBuilder.Append(')');
        }
        else
          stringBuilder.Append(str1 ?? "null");
        if (index != num)
        {
          stringBuilder.Append(", ");
          ++index;
        }
        else
          break;
      }
      stringBuilder.Append(" ]");
      return stringBuilder.ToString();
    }
  }
}
