// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.ITableManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

namespace Lightstreamer.DotNet.Client
{
  internal interface ITableManager
  {
    string Group { get; }

    string Mode { get; }

    string Schema { get; }

    string DataAdapter { get; }

    string Selector { get; }

    bool Snapshot { get; }

    int DistinctSnapshotLength { get; }

    int Start { get; }

    int End { get; }

    int MaxBufferSize { get; }

    double MaxFrequency { get; }

    bool Unfiltered { get; }

    void DoUpdate(ServerUpdateEvent values);

    void NotifyUnsub();
  }
}
