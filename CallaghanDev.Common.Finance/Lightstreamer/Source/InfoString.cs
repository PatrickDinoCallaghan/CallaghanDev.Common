// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.InfoString
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

namespace Lightstreamer.DotNet.Client
{
  internal class InfoString
  {
    public long holdingMillis;
    public string value;
    public long enforcedEventProg = -1;

    public InfoString(long holdingMillis) => this.holdingMillis = holdingMillis;

    public InfoString(string value) => this.value = value;

    public InfoString(string value, long enforcedEventProg)
    {
      this.value = value;
      this.enforcedEventProg = enforcedEventProg;
    }
  }
}
