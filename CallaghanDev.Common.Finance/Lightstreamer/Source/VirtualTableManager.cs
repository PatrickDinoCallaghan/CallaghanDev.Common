// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.VirtualTableManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;

namespace Lightstreamer.DotNet.Client
{
  internal class VirtualTableManager : ITableManager
  {
    private ExtendedTableInfo table;
    private FullTableManager managerWithListener;
    private static ILog actionsLogger = LogManager.GetLogger("com.lightstreamer.ls_client.actions");

    public virtual string Group => this.table.group;

    public virtual string Mode => this.table.mode;

    public virtual string Schema => this.table.schema;

    public virtual string DataAdapter => this.table.dataAdapter;

    public virtual string Selector => this.table.selector;

    public virtual bool Snapshot => this.table.snapshot;

    public virtual int DistinctSnapshotLength => this.table.distinctSnapshotLength;

    public virtual int Start => this.table.start;

    public virtual int End => this.table.end;

    public virtual int MaxBufferSize => this.table.bufferSize;

    public virtual double MaxFrequency => this.table.maxFrequency;

    public virtual bool Unfiltered => this.table.unfiltered;

    public virtual int NumItems => this.table.items.Length;

    internal VirtualTableManager(ExtendedTableInfo table, IHandyTableListener listener)
    {
      this.table = (ExtendedTableInfo) table.Clone();
      this.managerWithListener = new FullTableManager((SimpleTableInfo) table, listener, false);
    }

    public virtual void DoUpdate(ServerUpdateEvent values) => throw new PushServerException(12);

    public virtual void NotifyUnsub()
    {
    }

    public virtual object GetItemName(int i) => (object) this.table.items[i];

    public virtual object GetItemManager(int i) => (object) new VirtualTableManager.MonoTableManager(this, i);

    public override string ToString() => this.Mode + " items [" + this.Group + "] with fields [" + this.Schema + "]";

    private class MonoTableManager : ITableManager
    {
      private VirtualTableManager enclosingInstance;
      private int itemIndex;

      public virtual string Group => this.enclosingInstance.table.items[this.itemIndex];

      public virtual string Mode => this.enclosingInstance.Mode;

      public virtual string Schema => this.enclosingInstance.Schema;

      public virtual string DataAdapter => this.enclosingInstance.table.dataAdapter;

      public virtual string Selector => this.enclosingInstance.table.selector;

      public virtual bool Snapshot => this.enclosingInstance.Snapshot;

      public virtual int DistinctSnapshotLength => this.enclosingInstance.DistinctSnapshotLength;

      public virtual int Start => -1;

      public virtual int End => -1;

      public virtual int MaxBufferSize => this.enclosingInstance.MaxBufferSize;

      public virtual double MaxFrequency => this.enclosingInstance.MaxFrequency;

      public virtual bool Unfiltered => this.enclosingInstance.Unfiltered;

      public MonoTableManager(VirtualTableManager enclosingInstance, int itemIndex)
      {
        this.enclosingInstance = enclosingInstance;
        this.itemIndex = itemIndex;
      }

      public virtual void DoUpdate(ServerUpdateEvent values)
      {
        if (values.ItemCode != 1)
          throw new PushServerException(2);
        this.enclosingInstance.managerWithListener.ProcessUpdate(values, this.itemIndex + 1, this.itemIndex);
      }

      public virtual void NotifyUnsub() => this.enclosingInstance.managerWithListener.NotifyUnsubForItem(this.itemIndex + 1, this.enclosingInstance.table.items[this.itemIndex]);

      public override string ToString() => this.Mode + " item " + this.Group + " with fields [" + this.Schema + "]";
    }
  }
}
