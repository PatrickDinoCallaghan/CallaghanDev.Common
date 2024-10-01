// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.PushServerException
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;

namespace Lightstreamer.DotNet.Client
{
  public class PushServerException : ClientException
  {
    public const int TABLE_ERROR = 1;
    public const int ITEMS_ERROR = 2;
    public const int FIELDS_ERROR = 3;
    public const int UNEXPECTED_END = 4;
    public const int SYNTAX_ERROR = 5;
    public const int NO_ANSWER = 6;
    public const int PROTOCOL_ERROR = 7;
    public const int SYNC_ERROR = 8;
    public const int SERVER_REFUSAL = 9;
    public const int SERVER_TIMEOUT = 10;
    public const int RECONNECTION_TIMEOUT = 11;
    public const int UNEXPECTED_ERROR = 12;
    public const int MESSAGE_ERROR = 13;
    public const int MAXFREQ_ERROR = 14;
    private static readonly string[] defaultMsgs = new string[15]
    {
      "Unspecified error",
      "Wrong table number",
      "Wrong item number",
      "Wrong number of fields",
      "Answer was interrupted",
      "Incorrect answer",
      "No answer",
      "Unexpected answer",
      "Session not found",
      "Server refusal",
      "No data from server",
      "No more answer from server",
      "Unexpected error",
      "Wrong message sequence or progressive number",
      "Maximum frequency cannot be set"
    };
    private int errorCode;

    public virtual int ErrorCode => this.errorCode;

    public PushServerException(int errorCode, string extraMsg)
      : base(PushServerException.GetMsg(errorCode, extraMsg))
    {
      this.errorCode = errorCode;
    }

    public PushServerException(int errorCode)
      : base(PushServerException.GetMsg(errorCode, (string) null))
    {
      this.errorCode = errorCode;
    }

    public PushServerException(int errorCode, Exception cause)
      : base(PushServerException.GetMsg(errorCode, cause.Message), cause)
    {
      this.errorCode = errorCode;
    }

    private static string GetMsg(int errorCode, string extraMsg)
    {
      string msg = PushServerException.defaultMsgs[0];
      if (errorCode > 0 && errorCode < PushServerException.defaultMsgs.Length)
        msg = PushServerException.defaultMsgs[errorCode];
      if (extraMsg != null)
        msg = msg + ": " + extraMsg;
      return msg;
    }
  }
}
