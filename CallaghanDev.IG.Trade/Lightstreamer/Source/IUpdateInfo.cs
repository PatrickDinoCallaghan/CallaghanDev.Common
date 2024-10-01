// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.IUpdateInfo
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

namespace Lightstreamer.DotNet.Client
{
  public interface IUpdateInfo
  {
    int ItemPos { get; }

    string ItemName { get; }

    bool IsValueChanged(int fieldPos);

    bool IsValueChanged(string fieldName);

    string GetNewValue(int fieldPos);

    string GetNewValue(string fieldName);

    string GetOldValue(int fieldPos);

    string GetOldValue(string fieldName);

    int NumFields { get; }

    bool Snapshot { get; }
  }
}
