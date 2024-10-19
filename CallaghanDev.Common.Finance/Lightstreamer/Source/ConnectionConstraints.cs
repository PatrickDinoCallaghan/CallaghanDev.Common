// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.ConnectionConstraints
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;

namespace Lightstreamer.DotNet.Client
{
  public class ConnectionConstraints
  {
    private double maxBandwidth = -1.0;

    public double MaxBandwidth
    {
      set => this.maxBandwidth = value;
      get => this.maxBandwidth;
    }

    public override string ToString()
    {
      string str = (string) null;
      if (this.maxBandwidth != -1.0)
        str = "BW = " + (object) this.maxBandwidth;
      return str ?? "NO CONSTRAINTS";
    }

    public virtual object Clone() => (object) (ConnectionConstraints) this.MemberwiseClone();

    public override bool Equals(object other) => other == this || other != null && (ValueType) ((ConnectionConstraints) other).maxBandwidth == (ValueType) this.maxBandwidth;

    public override int GetHashCode() => base.GetHashCode();
  }
}
