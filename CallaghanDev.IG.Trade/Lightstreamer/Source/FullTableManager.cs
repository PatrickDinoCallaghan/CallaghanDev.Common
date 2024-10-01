// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.FullTableManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lightstreamer.DotNet.Client
{
  internal class FullTableManager : ITableManager
  {
    private SimpleTableInfo baseInfo;
    private ExtendedTableInfo extInfo;
    private IHandyTableListener listener;
    private bool isCommandLogic;
    private IDictionary fieldIndexMap = (IDictionary) new Dictionary<string, int>();
    private IList itemInfos = (IList) new List<FullTableManager.ItemInfo>();
    private bool unsubscrDone;
    private static ILog actionsLogger = LogManager.GetLogger("com.lightstreamer.ls_client.actions");
    private static object nullPlaceholder = new object();

    public virtual string Group => this.baseInfo.group;

    public virtual string Mode => this.baseInfo.mode;

    public virtual string Schema => this.baseInfo.schema;

    public virtual string DataAdapter => this.baseInfo.dataAdapter;

    public virtual string Selector => this.baseInfo.selector;

    public virtual bool Snapshot => this.baseInfo.snapshot;

    public virtual int DistinctSnapshotLength => this.baseInfo.distinctSnapshotLength;

    public virtual int Start => this.baseInfo.start;

    public virtual int End => this.baseInfo.end;

    public virtual int MaxBufferSize => this.baseInfo.bufferSize;

    public virtual double MaxFrequency => this.baseInfo.maxFrequency;

    public virtual bool Unfiltered => this.baseInfo.unfiltered;

    internal FullTableManager(
      SimpleTableInfo table,
      IHandyTableListener listener,
      bool doCommandLogic)
    {
      this.baseInfo = (SimpleTableInfo) table.Clone();
      this.isCommandLogic = doCommandLogic;
      if (table is ExtendedTableInfo)
      {
        this.extInfo = (ExtendedTableInfo) this.baseInfo;
        this.fieldIndexMap = (IDictionary) new Dictionary<string, int>();
        for (int index = 0; index < this.extInfo.fields.Length; ++index)
          this.fieldIndexMap[(object) this.extInfo.fields[index]] = (object) index;
      }
      else
      {
        this.extInfo = (ExtendedTableInfo) null;
        this.fieldIndexMap = (IDictionary) null;
      }
      this.listener = listener;
    }

    public virtual void DoUpdate(ServerUpdateEvent values)
    {
      int itemCode = values.ItemCode;
      int itemIndex = itemCode - 1;
      this.ProcessUpdate(values, itemCode, itemIndex);
    }

    internal void ProcessUpdate(ServerUpdateEvent values, int itemPos, int itemIndex)
    {
      string str = (string) null;
      if (this.extInfo != null)
      {
        if (itemIndex < 0 || itemIndex >= this.extInfo.items.Length)
          throw new PushServerException(2);
        str = this.extInfo.items[itemIndex];
      }
      FullTableManager.ItemInfo info;
      lock (this.itemInfos)
      {
        if (this.unsubscrDone)
          return;
        while (this.itemInfos.Count <= itemIndex)
          this.itemInfos.Add((object) null);
        info = (FullTableManager.ItemInfo) this.itemInfos[itemIndex];
        if (info == null)
        {
          info = !this.isCommandLogic ? new FullTableManager.ItemInfo(this, itemPos, str) : (FullTableManager.ItemInfo) new FullTableManager.CommandLogicItemInfo(this, itemPos, str);
          this.itemInfos[itemIndex] = (object) info;
        }
      }
      if (values.EOS)
      {
        info.snapshotPending = false;
        try
        {
          this.listener.OnSnapshotEnd(itemPos, str);
        }
        catch (Exception ex)
        {
        }
      }
      else if (values.Overflow > 0)
      {
        if (!this.baseInfo.hasUnfilteredData())
          throw new PushServerException(7);
        FullTableManager.actionsLogger.Warn("Got notification of updates lost for item " + (object) info);
        try
        {
          this.listener.OnRawUpdatesLost(itemPos, str, values.Overflow);
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        if (this.extInfo != null && values.Size != this.extInfo.fields.Length)
          throw new PushServerException(3);
        string[] array = values.Array;
        if (FullTableManager.actionsLogger.IsDebugEnabled)
          FullTableManager.actionsLogger.Debug("Got event for item " + (object) info + " with values " + CollectionsSupport.ToString((ICollection) array));
        bool snapshotPending = info.snapshotPending;
        string[] prevState = info.Update(array);
        if (prevState == null)
          return;
        IUpdateInfo update = (IUpdateInfo) new UpdateInfoImpl(info, prevState, array, snapshotPending);
        if (FullTableManager.actionsLogger.IsDebugEnabled)
          FullTableManager.actionsLogger.Debug("Notifying event for item " + (object) info + " with values " + (object) update);
        try
        {
          this.listener.OnUpdate(itemPos, str, update);
        }
        catch (Exception ex)
        {
        }
      }
    }

    public virtual void NotifyUnsub()
    {
      FullTableManager.ItemInfo[] objects = new FullTableManager.ItemInfo[this.itemInfos.Count];
      lock (this.itemInfos)
      {
        objects = (FullTableManager.ItemInfo[]) CollectionsSupport.ToArray((ICollection) this.itemInfos, (object[]) objects);
        this.unsubscrDone = true;
      }
      for (int index = 0; index < objects.Length; ++index)
      {
        if (objects[index] != null)
          this.NotifyUnsubForItem(objects[index].pos, objects[index].name);
      }
      try
      {
        this.listener.OnUnsubscrAll();
      }
      catch (Exception ex)
      {
      }
    }

    internal void NotifyUnsubForItem(int itemPos, string itemName)
    {
      try
      {
        this.listener.OnUnsubscr(itemPos, itemName);
      }
      catch (Exception ex)
      {
      }
    }

    public override string ToString() => this.Mode + " table [" + this.Group + " ; " + this.Schema + "]";

    internal class ItemInfo
    {
      protected FullTableManager enclosingInstance;
      public int pos;
      public string name;
      public bool snapshotPending;
      protected string[] currState;

      public ItemInfo(FullTableManager enclosingInstance, int pos, string name)
      {
        this.enclosingInstance = enclosingInstance;
        this.pos = pos;
        this.name = name;
        this.snapshotPending = enclosingInstance.baseInfo.snapshot;
      }

      public virtual string[] Update(string[] updEvent)
      {
        if (this.currState == null)
          this.currState = new string[updEvent.Length];
        string[] currState = this.currState;
        this.currState = new string[this.currState.Length];
        for (int index = 0; index < this.currState.Length; ++index)
          this.currState[index] = !(updEvent[index] != ServerUpdateEvent.UNCHANGED) ? currState[index] : updEvent[index];
        if (this.snapshotPending && this.enclosingInstance.baseInfo.mode.Equals("MERGE"))
          this.snapshotPending = false;
        return currState;
      }

      public IDictionary GetFieldIndexMap() => this.enclosingInstance.fieldIndexMap;

      public override string ToString() => this.name != null ? this.name : this.pos.ToString();
    }

    private class CommandLogicItemInfo : FullTableManager.ItemInfo
    {
      private int keyIndex;
      private int commandIndex;
      private IDictionary keyStates = (IDictionary) new Dictionary<object, string[]>();

      public CommandLogicItemInfo(FullTableManager enclosingInstance, int pos, string name)
        : base(enclosingInstance, pos, name)
      {
        if (enclosingInstance.extInfo != null)
        {
          this.keyIndex = !enclosingInstance.fieldIndexMap.Contains((object) "key") ? -1 : (int) enclosingInstance.fieldIndexMap[(object) "key"];
          if (enclosingInstance.fieldIndexMap.Contains((object) "command"))
            this.commandIndex = (int) enclosingInstance.fieldIndexMap[(object) "command"];
          else
            this.commandIndex = -1;
        }
        else
        {
          this.keyIndex = 0;
          this.commandIndex = 1;
        }
      }

      public override string[] Update(string[] updEvent)
      {
        base.Update(updEvent);
        string str1 = "ADD";
        string str2 = "UPDATE";
        string str3 = "DELETE";
        object nullPlaceholder = FullTableManager.nullPlaceholder;
        if (this.keyIndex >= 0 && this.keyIndex < this.currState.Length)
          nullPlaceholder = (object) this.currState[this.keyIndex];
        else
          FullTableManager.actionsLogger.Warn("key field not subscribed for item " + (object) this + " - null key forced for command logic");
        string str4 = (string) null;
        if (this.commandIndex >= 0 && this.commandIndex < this.currState.Length)
        {
          str4 = this.currState[this.commandIndex];
          if (str4 == null)
            FullTableManager.actionsLogger.Warn("No value found for command field for item " + (object) this + " - trying to add/update for command logic");
          else if (str4.Equals(str3))
            str4 = str3;
          else if (str4.Equals(str1))
            str4 = str1;
          else if (str4.Equals(str2))
            str4 = str2;
          else
            FullTableManager.actionsLogger.Warn("Invalid value for command field for item " + (object) this + " - trying to add/update for command logic");
        }
        else
          FullTableManager.actionsLogger.Warn("command field not subscribed for item " + (object) this + " - trying to add/update for command logic");
        string[] keyState = (string[]) this.keyStates[nullPlaceholder];
        if (str4 == str3)
        {
          if (FullTableManager.actionsLogger.IsDebugEnabled)
            FullTableManager.actionsLogger.Debug("Processing DELETE event in COMMAND logic for item " + (object) this + " and key " + (nullPlaceholder != FullTableManager.nullPlaceholder ? nullPlaceholder : (object) "null"));
          if (keyState == null)
          {
            FullTableManager.actionsLogger.Warn("Unexpected DELETE command for item " + (object) this + " - discarding the command");
            return (string[]) null;
          }
          this.keyStates.Remove(nullPlaceholder);
          for (int index = 0; index < this.currState.Length; ++index)
            updEvent[index] = index != this.keyIndex ? (index != this.commandIndex ? (string) null : str3) : ServerUpdateEvent.UNCHANGED;
          return keyState;
        }
        if (FullTableManager.actionsLogger.IsDebugEnabled)
          FullTableManager.actionsLogger.Debug("Processing ADD/UPDATE event in COMMAND logic for item " + (object) this + " and key " + (nullPlaceholder != FullTableManager.nullPlaceholder ? nullPlaceholder : (object) "null"));
        if (keyState == null)
        {
          if (str4 == str2)
            FullTableManager.actionsLogger.Warn("Unexpected UPDATE command for item " + (object) this + " - command changed into ADD");
          for (int index = 0; index < this.currState.Length; ++index)
            updEvent[index] = index != this.commandIndex ? this.currState[index] : str1;
          this.keyStates[nullPlaceholder] = (object) updEvent;
          return new string[this.currState.Length];
        }
        if (str4 == str1)
          FullTableManager.actionsLogger.Warn("Unexpected ADD command for item " + (object) this + " - command changed into UPDATE");
        for (int index = 0; index < this.currState.Length; ++index)
          updEvent[index] = index != this.keyIndex ? (index != this.commandIndex ? (this.currState[index] != null || keyState[index] != null ? (this.currState[index] == null || keyState[index] == null ? this.currState[index] : (!this.currState[index].Equals(keyState[index]) ? this.currState[index] : ServerUpdateEvent.UNCHANGED)) : ServerUpdateEvent.UNCHANGED) : (keyState[index] == str1 ? str2 : ServerUpdateEvent.UNCHANGED)) : ServerUpdateEvent.UNCHANGED;
        if (str4 == str2)
        {
          this.keyStates[nullPlaceholder] = (object) this.currState;
        }
        else
        {
          string[] strArray = new string[this.currState.Length];
          for (int index = 0; index < this.currState.Length; ++index)
            strArray[index] = index != this.commandIndex ? this.currState[index] : str2;
          this.keyStates[nullPlaceholder] = (object) strArray;
        }
        return keyState;
      }
    }
  }
}
