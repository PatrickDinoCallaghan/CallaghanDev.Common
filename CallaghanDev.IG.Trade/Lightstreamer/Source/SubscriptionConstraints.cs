// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.SubscriptionConstraints
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;

namespace Lightstreamer.DotNet.Client
{
  public class SubscriptionConstraints
  {
    private double maxFrequency = -1.0;

    public double MaxFrequency
    {
      set => this.maxFrequency = value;
      get => this.maxFrequency < 0.0 ? -1.0 : this.maxFrequency;
    }

    public override string ToString()
    {
      string str = (string) null;
      if (this.maxFrequency != -1.0)
        str = "FREQ = " + (object) this.maxFrequency;
      return str ?? "NO CONSTRAINTS";
    }

    public virtual object Clone() => (object) (SubscriptionConstraints) this.MemberwiseClone();

    public override bool Equals(object other) => other == this || other != null && (ValueType) ((SubscriptionConstraints) other).maxFrequency == (ValueType) this.maxFrequency;

    public override int GetHashCode() => base.GetHashCode();
  }
}
