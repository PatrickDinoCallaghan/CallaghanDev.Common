// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.SequencesHandler
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System.Collections.Generic;

namespace Lightstreamer.DotNet.Client
{
  internal class SequencesHandler
  {
    private IDictionary<string, SequenceHandler> sequences = (IDictionary<string, SequenceHandler>) new Dictionary<string, SequenceHandler>();

    internal SequenceHandler GetSequence(string sequence)
    {
      if (!this.sequences.ContainsKey(sequence))
      {
        SequenceHandler sequenceHandler = new SequenceHandler();
        this.sequences[sequence] = sequenceHandler;
      }
      return this.sequences[sequence];
    }

    internal IEnumerator<KeyValuePair<string, SequenceHandler>> Reset()
    {
      IEnumerator<KeyValuePair<string, SequenceHandler>> enumerator = this.sequences.GetEnumerator();
      this.sequences = (IDictionary<string, SequenceHandler>) new Dictionary<string, SequenceHandler>();
      return enumerator;
    }
  }
}
