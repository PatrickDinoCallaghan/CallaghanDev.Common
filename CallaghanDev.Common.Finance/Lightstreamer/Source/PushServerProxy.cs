// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.PushServerProxy
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Lightstreamer.DotNet.Client
{
  internal class PushServerProxy
  {
    private PushServerTranslator serverTranslator;
    private PushServerProxy.PushServerProxyInfo serverInfo;
    private Stream pushLowLevelStream;
    private LSStreamReader pushStream;
    private bool streamCompleted;
    private bool closed = true;
    private long totalBytes;
    private long totalReceived;
    private bool isRecovering;
    private static int currCode = 0;
    private static object codes = new object();
    private static ILog streamLogger = LogManager.GetLogger("com.lightstreamer.ls_client.stream");
    private static ILog sessionLogger = LogManager.GetLogger("com.lightstreamer.ls_client.session");
    private static ILog protLogger = LogManager.GetLogger("com.lightstreamer.ls_client.protocol");

    internal virtual long TotalBytes
    {
      get
      {
        lock (this)
          return this.totalBytes;
      }
    }

    internal virtual string SessionId => this.serverInfo.sessionId;

    internal virtual long KeepaliveMillis => this.serverInfo.keepaliveMillis;

    internal virtual SubscribedTableKey TableCode
    {
      get
      {
        lock (PushServerProxy.codes)
        {
          ++PushServerProxy.currCode;
          return new SubscribedTableKey(PushServerProxy.currCode);
        }
      }
    }

    internal PushServerProxy(ConnectionInfo info) => this.serverTranslator = new PushServerTranslator(info);

    internal virtual Stream ConnectForSession()
    {
      PushServerProxy.sessionLogger.Info("Connecting for a new session");
      Stream stream;
      try
      {
        stream = this.serverTranslator.CallSession();
      }
      catch (FormatException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful connection for new session");
        PushServerProxy.sessionLogger.Debug("Unsuccessful connection for new session", (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful connection for new session");
        PushServerProxy.sessionLogger.Debug("Unsuccessful connection for new session", (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      catch (IOException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful connection for new session");
        PushServerProxy.sessionLogger.Debug("Unsuccessful connection for new session", (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      bool flag = false;
      lock (this)
      {
        if (!this.closed)
          flag = true;
      }
      if (flag)
      {
        PushServerProxy.sessionLogger.Info("Connection started but no longer requested");
        try
        {
          PushServerProxy.streamLogger.Debug("Closing stream connection");
          stream.Dispose();
        }
        catch (IOException ex)
        {
          PushServerProxy.streamLogger.Debug("Error closing the stream connection", (Exception) ex);
        }
        throw new PhaseException();
      }
      return stream;
    }

    internal virtual void StartSession(Stream stream)
    {
      PushServerProxy.sessionLogger.Info("Starting new session");
      LSStreamReader lsStreamReader;
      PushServerProxy.PushServerProxyInfo closingInfo;
      try
      {
        lsStreamReader = new LSStreamReader(stream, Encoding.UTF8);
        this.serverTranslator.CheckAnswer(lsStreamReader);
        closingInfo = this.serverTranslator.ReadSessionId(lsStreamReader);
      }
      catch (PushEndException ex)
      {
        throw new PushServerException(7);
      }
      catch (IOException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful start of new session");
        PushServerProxy.sessionLogger.Debug("Unsuccessful start of new session", (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful start of new session");
        PushServerProxy.sessionLogger.Debug("Unsuccessful start of new session", (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      bool flag = false;
      lock (this)
      {
        if (!this.closed)
        {
          flag = true;
        }
        else
        {
          this.pushLowLevelStream = stream;
          this.pushStream = lsStreamReader;
          this.streamCompleted = false;
          this.serverInfo = closingInfo;
          this.closed = false;
        }
      }
      if (!flag)
      {
        PushServerProxy.sessionLogger.Info("Started session " + this.serverInfo.sessionId);
      }
      else
      {
        PushServerProxy.sessionLogger.Info("Session started but no longer requested");
        this.DisposeStreams(stream, lsStreamReader, closingInfo);
        throw new PhaseException();
      }
    }

    internal virtual bool IsTableCodeConsumed(int tableCode)
    {
      lock (PushServerProxy.codes)
      {
        int num = tableCode;
        return num > 0 && num <= PushServerProxy.currCode;
      }
    }

    internal virtual void SendMessage(string message)
    {
      this.Check();
      try
      {
        this.serverTranslator.CallSendMessageRequest(this.serverInfo, message);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void RequestSendMessage(MessageManager message, int prog, BatchMonitor batch)
    {
      this.Check();
      try
      {
        this.serverTranslator.CallGuaranteedSendMessageRequest(this.serverInfo, Convert.ToString(prog), message, batch);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void RequestNewConstraints(ConnectionConstraints constraints)
    {
      this.Check();
      try
      {
        this.serverTranslator.CallConstrainRequest(this.serverInfo, constraints);
      }
      catch (PushUserException ex)
      {
        PushServerProxy.protLogger.Debug("Refused constraints request", (Exception) ex);
        throw new PushServerException(9);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void RequestSubscr(
      ITableManager table,
      SubscribedTableKey subscrKey,
      BatchMonitor batch)
    {
      string tableCode = subscrKey.KeyValue.ToString();
      this.Check();
      try
      {
        this.serverTranslator.CallTableRequest(this.serverInfo, tableCode, table, batch);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void RequestItemsSubscr(
      VirtualTableManager table,
      SubscribedTableKey[] subscrKeys,
      BatchMonitor batch)
    {
      string[] tableCodes = new string[subscrKeys.Length];
      for (int index = 0; index < subscrKeys.Length; ++index)
        tableCodes[index] = subscrKeys[index].KeyValue.ToString();
      this.Check();
      try
      {
        this.serverTranslator.CallItemsRequest(this.serverInfo, tableCodes, table, batch);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void ConstrainSubscrs(
      SubscribedTableKey[] subscrKeys,
      SubscriptionConstraints constraints)
    {
      string[] tableCodes = new string[subscrKeys.Length];
      for (int index = 0; index < subscrKeys.Length; ++index)
        tableCodes[index] = subscrKeys[index].KeyValue.ToString();
      this.Check();
      try
      {
        this.serverTranslator.CallReconf(this.serverInfo, tableCodes, constraints);
      }
      catch (PushUserException ex)
      {
        if (ex.ErrorCode == 13)
          throw new PushServerException(14);
        PushServerProxy.protLogger.Debug("Refused reconf request", (Exception) ex);
        throw new PushServerException(9);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void DelSubscrs(SubscribedTableKey[] subscrKeys, BatchMonitor batch)
    {
      string[] tableCodes = new string[subscrKeys.Length];
      for (int index = 0; index < subscrKeys.Length; ++index)
        tableCodes[index] = subscrKeys[index].KeyValue.ToString();
      this.Check();
      try
      {
        this.serverTranslator.CallDelete(this.serverInfo, tableCodes, batch);
      }
      catch (PushUserException ex)
      {
        PushServerProxy.protLogger.Debug("Refused delete request", (Exception) ex);
        throw new PushServerException(9);
      }
      catch (IOException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        throw new PushConnException((Exception) ex);
      }
      this.Check();
    }

    internal virtual void ResyncSession(long recoveryProg)
    {
      if (recoveryProg >= 0L)
        PushServerProxy.sessionLogger.Info("Recovering session " + this.serverInfo.sessionId + " from " + (object) recoveryProg);
      else
        PushServerProxy.sessionLogger.Info("Rebinding session " + this.serverInfo.sessionId);
      this.Check();
      lock (this)
      {
        if (!this.closed)
          this.Dispose(false);
      }
      Stream stream;
      LSStreamReader lsStreamReader;
      PushServerProxy.PushServerProxyInfo pushServerProxyInfo;
      try
      {
        stream = this.serverTranslator.CallResync(this.serverInfo, (ConnectionConstraints) null, recoveryProg);
        lsStreamReader = new LSStreamReader(stream, Encoding.UTF8);
        this.serverTranslator.CheckAnswer(lsStreamReader);
        pushServerProxyInfo = this.serverTranslator.ReadSessionId(lsStreamReader);
      }
      catch (PushUserException ex)
      {
        PushServerProxy.sessionLogger.Info("Refused resync request " + this.serverInfo.sessionId);
        PushServerProxy.protLogger.Debug("Refused resync request", (Exception) ex);
        throw new PushServerException(9);
      }
      catch (IOException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful rebinding of session " + this.serverInfo.sessionId);
        PushServerProxy.sessionLogger.Debug("Unsuccessful rebinding of session " + this.serverInfo.sessionId, (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        PushServerProxy.sessionLogger.Info("Unsuccessful rebinding of session " + this.serverInfo.sessionId);
        PushServerProxy.sessionLogger.Debug("Unsuccessful rebinding of session " + this.serverInfo.sessionId, (Exception) ex);
        throw new PushConnException((Exception) ex);
      }
      bool flag = false;
      lock (this)
      {
        if (!this.closed)
        {
          this.Dispose(false);
          this.pushLowLevelStream = stream;
          this.pushStream = lsStreamReader;
          this.streamCompleted = false;
          this.serverInfo = pushServerProxyInfo;
        }
        else
          flag = true;
      }
      if (!flag)
      {
        PushServerProxy.sessionLogger.Info("Rebind successful on session " + this.serverInfo.sessionId);
      }
      else
      {
        PushServerProxy.sessionLogger.Info("Rebind successful but no longer requested");
        this.DisposeStreams(stream, lsStreamReader, (PushServerProxy.PushServerProxyInfo) null);
        throw new PhaseException();
      }
    }

    private InfoString WaitCommand(
      ServerManager.ActivityController activityController)
    {
      LSStreamReader pushStream;
      lock (this)
      {
        this.Check();
        pushStream = this.pushStream;
      }
      try
      {
        InfoString infoString = this.serverTranslator.WaitCommand(pushStream);
        if (infoString != null && infoString.value == null)
        {
          this.Check();
          activityController.StopKeepalives();
        }
        else
          activityController.OnActivity();
        return infoString;
      }
      catch (PushEndException ex)
      {
        lock (this)
        {
          this.Check();
          this.streamCompleted = true;
        }
        throw ex;
      }
      catch (IOException ex)
      {
        lock (this)
        {
          this.Check();
          this.streamCompleted = true;
        }
        throw new PushConnException((Exception) ex);
      }
      catch (WebException ex)
      {
        lock (this)
        {
          this.Check();
          this.streamCompleted = true;
        }
        throw new PushConnException((Exception) ex);
      }
    }

    internal virtual ServerUpdateEvent WaitUpdate(
      ServerManager.ActivityController activityController)
    {
      this.Check();
      InfoString infoString;
      try
      {
        infoString = this.WaitCommand(activityController);
      }
      catch (PushConnException ex)
      {
        PushServerProxy.streamLogger.Debug("Error in connection", (Exception) ex);
        long totalReceived;
        lock (this)
        {
          totalReceived = this.totalReceived;
          if (totalReceived != -1L)
            this.isRecovering = true;
        }
        if (totalReceived != -1L)
        {
          PushServerProxy.sessionLogger.Error("Error while listening for data in session " + this.SessionId + "; trying a recovery");
          return new ServerUpdateEvent(0L, totalReceived);
        }
        PushServerProxy.sessionLogger.Error("Error while listening for data in session " + this.SessionId);
        throw ex;
      }
      if (infoString == null)
        return (ServerUpdateEvent) null;
      if (infoString.value == null)
        return new ServerUpdateEvent(infoString.holdingMillis);
      long totalReceived1;
      lock (this)
      {
        if (this.isRecovering)
        {
          if (infoString.enforcedEventProg == -1L)
          {
            this.totalReceived = -1L;
            throw new PushConnException("Unexpected missing support for recovering after disconnection");
          }
          if (this.totalReceived == -1L)
            throw new PushConnException("Unmanageable error while recovering after disconnection");
          if (infoString.enforcedEventProg > this.totalReceived + 1L)
          {
            this.totalReceived = -1L;
            throw new PushConnException("Unexpected error while recovering after disconnection");
          }
          if (infoString.enforcedEventProg == this.totalReceived + 1L)
            this.isRecovering = false;
        }
        totalReceived1 = this.totalReceived;
        this.totalReceived = -1L;
      }
      ServerUpdateEvent pushData;
      try
      {
        pushData = this.serverTranslator.ParsePushData(infoString.value);
      }
      catch (PushServerException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        throw new PushServerException(12, ex);
      }
      lock (this)
      {
        this.totalBytes += (long) (infoString.value.Length + 2);
        if (this.isRecovering)
        {
          this.totalReceived = totalReceived1;
          PushServerProxy.streamLogger.Debug("Skipping redundant recovery data: " + infoString.value);
          PushServerProxy.protLogger.Debug("Skipping redundant recovery: " + (object) pushData);
          return (ServerUpdateEvent) null;
        }
        if (totalReceived1 != -1L)
          this.totalReceived = totalReceived1 + 1L;
      }
      this.Check();
      return pushData;
    }

    internal virtual void Dispose(bool alsoCloseSession)
    {
      Stream closingLowLevelStream = (Stream) null;
      LSStreamReader closingStream = (LSStreamReader) null;
      bool flag1 = false;
      PushServerProxy.PushServerProxyInfo closingInfo = (PushServerProxy.PushServerProxyInfo) null;
      bool flag2 = false;
      lock (this)
      {
        if (!this.closed)
        {
          closingLowLevelStream = this.pushLowLevelStream;
          closingStream = this.pushStream;
          flag1 = this.streamCompleted;
          this.pushLowLevelStream = (Stream) null;
          this.pushStream = (LSStreamReader) null;
          this.streamCompleted = false;
          closingInfo = this.serverInfo;
          if (alsoCloseSession)
          {
            this.closed = true;
            this.serverTranslator.AbortBatches();
          }
        }
        else
          flag2 = true;
      }
      if (!flag2)
      {
        if (closingLowLevelStream == null && closingStream == null)
          return;
        if ((!alsoCloseSession ? 0 : (!flag1 ? 1 : 0)) != 0)
          this.DisposeStreams(closingLowLevelStream, closingStream, closingInfo);
        else
          this.DisposeStreams(closingLowLevelStream, closingStream, (PushServerProxy.PushServerProxyInfo) null);
      }
      else
        PushServerProxy.sessionLogger.Info("Session " + this.SessionId + " already terminated");
    }

    internal virtual void DisposeStreams(
      Stream closingLowLevelStream,
      LSStreamReader closingStream,
      PushServerProxy.PushServerProxyInfo closingInfo)
    {
      new PushServerProxy.AnonymousClassThread1(closingLowLevelStream, closingStream, this).Start(true);
      if (closingInfo == null)
        return;
      new PushServerProxy.AnonymousClassThread2(closingInfo, this).Start(true);
    }

    internal virtual void StartBatch()
    {
      lock (this)
      {
        this.Check();
        this.serverTranslator.StartControlBatch(this.serverInfo);
      }
    }

    internal virtual void StartMessageBatch()
    {
      lock (this)
      {
        this.Check();
        this.serverTranslator.StartMessageBatch(this.serverInfo);
      }
    }

    internal virtual void CloseBatch() => this.serverTranslator.CloseControlBatch();

    internal virtual void CloseMessageBatch() => this.serverTranslator.CloseMessageBatch();

    private void Check()
    {
      lock (this)
      {
        if (this.closed)
          throw new PhaseException();
      }
    }

    private class AnonymousClassThread1 : ThreadSupport
    {
      private Stream closingLowLevelStream;
      private LSStreamReader closingStream;
      private PushServerProxy enclosingInstance;

      public AnonymousClassThread1(
        Stream closingLowLevelStream,
        LSStreamReader closingStream,
        PushServerProxy enclosingInstance)
      {
        this.closingLowLevelStream = closingLowLevelStream;
        this.closingStream = closingStream;
        this.enclosingInstance = enclosingInstance;
      }

      public override void Run()
      {
        try
        {
          this.closingLowLevelStream.Dispose();
        }
        catch (IOException ex)
        {
          PushServerProxy.streamLogger.Debug("Error closing the stream connection", (Exception) ex);
        }
        try
        {
          PushServerProxy.streamLogger.Debug("Closing stream connection");
          this.closingStream.Close();
        }
        catch (IOException ex)
        {
          PushServerProxy.streamLogger.Debug("Error closing the stream connection", (Exception) ex);
        }
      }
    }

    private class AnonymousClassThread2 : ThreadSupport
    {
      private PushServerProxy.PushServerProxyInfo closingServerInfo;
      private PushServerProxy enclosingInstance;

      public AnonymousClassThread2(
        PushServerProxy.PushServerProxyInfo closingServerInfo,
        PushServerProxy enclosingInstance)
      {
        this.closingServerInfo = closingServerInfo;
        this.enclosingInstance = enclosingInstance;
      }

      public override void Run()
      {
        try
        {
          this.enclosingInstance.serverTranslator.CallDestroyRequest(this.closingServerInfo);
        }
        catch (Exception ex)
        {
        }
      }
    }

    internal class PushServerProxyInfo
    {
      public string sessionId;
      public string controlAddress;
      public string rebindAddress;
      public long keepaliveMillis;

      public PushServerProxyInfo(
        string sessionId,
        string controlAddress,
        string rebindAddress,
        long keepaliveMillis)
      {
        this.sessionId = sessionId;
        this.controlAddress = controlAddress;
        this.rebindAddress = rebindAddress;
        this.keepaliveMillis = keepaliveMillis;
      }

      public override string ToString() => "[ Session ID: " + this.sessionId + " - Control Address to be used: " + this.controlAddress + " - Rebind Address to be used: " + this.rebindAddress + " - Keepalive millis: " + (object) this.keepaliveMillis + "]";
    }
  }
}
