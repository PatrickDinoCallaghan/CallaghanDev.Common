// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.SimpleTableInfo
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

namespace Lightstreamer.DotNet.Client
{
  public class SimpleTableInfo
  {
    public const string MERGE = "MERGE";
    public const string DISTINCT = "DISTINCT";
    public const string RAW = "RAW";
    public const string COMMAND = "COMMAND";
    internal string group;
    internal string mode;
    internal string schema;
    internal string dataAdapter;
    internal string selector;
    internal bool snapshot;
    internal int distinctSnapshotLength = -1;
    internal int start = -1;
    internal int end = -1;
    internal int bufferSize = -1;
    internal double maxFrequency = -1.0;
    internal bool unfiltered;

    public virtual string DataAdapter
    {
      set => this.dataAdapter = value;
      get => this.dataAdapter;
    }

    public virtual string Selector
    {
      set => this.selector = value;
      get => this.selector;
    }

    public virtual int RequestedBufferSize
    {
      set
      {
        if (!this.mode.Equals("MERGE") && !this.mode.Equals("DISTINCT"))
          throw new SubscrException("Buffer size ineffective for mode " + this.mode);
        this.bufferSize = value >= 0 ? value : -1;
      }
      get => this.bufferSize == -1 ? -1 : this.bufferSize;
    }

    public virtual double RequestedMaxFrequency
    {
      set
      {
        if (!this.mode.Equals("MERGE") && !this.mode.Equals("DISTINCT") && !this.mode.Equals("COMMAND"))
          throw new SubscrException("Max frequency ineffective for mode " + this.mode);
        this.maxFrequency = value > 0.0 ? value : -1.0;
      }
      get => this.maxFrequency == -1.0 ? 0.0 : this.maxFrequency;
    }

    public virtual int RequestedDistinctSnapshotLength
    {
      set
      {
        if (!this.mode.Equals("DISTINCT"))
          throw new SubscrException("Snapshot length ineffective for mode " + this.mode);
        if (!this.snapshot)
          throw new SubscrException("Snapshot not requested for the item");
        this.distinctSnapshotLength = value > 0 ? value : -1;
      }
      get => this.distinctSnapshotLength == -1 ? 0 : this.distinctSnapshotLength;
    }

    public SimpleTableInfo(string group, string mode, string schema, bool snap)
    {
      this.group = group;
      this.mode = mode;
      this.schema = schema;
      if (snap)
      {
        if (!mode.Equals(nameof (MERGE)) && !mode.Equals(nameof (DISTINCT)) && !mode.Equals(nameof (COMMAND)))
          throw new SubscrException("Snapshot ineffective for mode " + mode);
        this.snapshot = true;
      }
      else
        this.snapshot = false;
    }

    public virtual string Group => this.group;

    public virtual string Mode => this.mode;

    public virtual string Schema => this.schema;

    public virtual bool Snapshot => this.snapshot;

    public virtual object Clone() => this.MemberwiseClone();

    public virtual void SetRange(int start, int end)
    {
      this.start = start;
      this.end = end;
    }

    public virtual int RangeStart => this.start == -1 ? 0 : this.start;

    public virtual int RangeEnd => this.end == -1 ? 0 : this.end;

    public virtual bool RequestedUnfilteredDispatching
    {
      set
      {
        if (value)
          this.RequestUnfilteredDispatching();
        else
          this.unfiltered = value;
      }
      get => this.unfiltered;
    }

    private void RequestUnfilteredDispatching()
    {
      if (!this.mode.Equals("MERGE") && !this.mode.Equals("DISTINCT") && !this.mode.Equals("COMMAND"))
        throw new SubscrException("Unfiltered dispatching cannot be specified for mode " + this.mode);
      this.unfiltered = true;
    }

    internal bool hasUnfilteredData() => this.mode.Equals("RAW") || this.unfiltered || this.mode.Equals("COMMAND");
  }
}
