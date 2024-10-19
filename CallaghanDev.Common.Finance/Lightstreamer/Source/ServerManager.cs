// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.ServerManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lightstreamer.DotNet.Client
{
  internal class ServerManager
  {
    private ServerManager.SessionActivityManager worker;
    private ServerManager.ActivityController activityController;
    private PushServerProxy localPushServerProxy;
    private ServerManager.IServerListener serverListener;
    private IDictionary tables = (IDictionary) new Dictionary<int, ITableManager>();
    private ConnectionInfo connInfo;
    private BatchMonitor batchMonitor = new BatchMonitor();
    private BatchMonitor mexBatchMonitor = new BatchMonitor();
    private SequencesHandler sequencesHandler = new SequencesHandler();
    private MessageParallelizer mexParallelizer;
    private static ILog streamLogger = LogManager.GetLogger("com.lightstreamer.ls_client.stream");
    private static ILog sessionLogger = LogManager.GetLogger("com.lightstreamer.ls_client.session");
    private static ILog actionsLogger = LogManager.GetLogger("com.lightstreamer.ls_client.actions");
    private static ILog protLogger = LogManager.GetLogger("com.lightstreamer.ls_client.protocol");
    private static TimerSupport activityTimer = new TimerSupport();
    private static NotificationQueue notificationsSender = new NotificationQueue("Session events queue", true);

    internal ServerManager(ConnectionInfo info, ServerManager.IServerListener asyncListener)
    {
      this.connInfo = info;
      this.localPushServerProxy = new PushServerProxy(info);
      this.activityController = new ServerManager.ActivityController(this);
      this.mexParallelizer = new MessageParallelizer(this.mexBatchMonitor, this);
      this.serverListener = asyncListener;
    }

    internal virtual void Connect()
    {
      Stream stream = (Stream) null;
      bool flag = true;
      this.activityController.StartConnection(true, -1L);
      try
      {
        stream = this.localPushServerProxy.ConnectForSession();
        this.serverListener.OnConnectionEstablished();
        this.localPushServerProxy.StartSession(stream);
        this.serverListener.OnSessionStarted(this.connInfo.Polling);
        flag = false;
      }
      catch (PhaseException ex)
      {
      }
      catch (Exception ex)
      {
        ServerManager.actionsLogger.Debug("Notifying an exception on the current connection");
        this.serverListener.OnConnectException(ex);
        throw ex;
      }
      finally
      {
        this.activityController.StopConnection();
        if (flag)
        {
          ServerManager.streamLogger.Debug("Closing create connection");
          try
          {
            stream?.Dispose();
          }
          catch (IOException ex)
          {
            ServerManager.streamLogger.Debug("Error closing create connection", (Exception) ex);
          }
        }
      }
    }

    internal virtual void Start()
    {
      this.worker = new ServerManager.SessionActivityManager(this, "Lightstreamer listening thread");
      this.worker.Start(true);
    }

    internal virtual ITableManager[] Close()
    {
      this.activityController.OnCloseRequested();
      ITableManager[] array;
      lock (this.tables.SyncRoot)
      {
        array = (ITableManager[]) CollectionsSupport.ToArray(this.tables.Values, (object[]) new ITableManager[0]);
        this.tables.Clear();
      }
      this.AbortPendingMessages();
      ServerManager.sessionLogger.Info("Terminating session " + this.localPushServerProxy.SessionId);
      this.localPushServerProxy.Dispose(true);
      this.activityController.OnInterrupt();
      this.CloseBatch();
      this.CloseMessageBatch();
      if (ServerManager.actionsLogger.IsInfoEnabled)
      {
        for (int index = 0; index < array.Length; ++index)
          ServerManager.actionsLogger.Info("Discarded " + (object) array[index] + " from session " + this.localPushServerProxy.SessionId);
      }
      return array;
    }

    internal virtual void SendMessage(string message) => this.localPushServerProxy.SendMessage(message);

    internal int SendMessage(MessageManager message, bool sendAsynchronously)
    {
      int prog = 0;
      lock (this.sequencesHandler)
        prog = this.sequencesHandler.GetSequence(message.Sequence).Enqueue(message);
      if (sendAsynchronously)
      {
        if (!this.mexBatchMonitor.Unlimited)
          this.BatchMessageRequests(0);
        this.mexParallelizer.EnqueueMessage(message, prog);
        return prog;
      }
      this.SendMessage(message, prog);
      return prog;
    }

    internal void SendMessage(MessageManager message, int prog)
    {
      bool flag = false;
      Exception problem = (Exception) null;
      try
      {
        this.localPushServerProxy.RequestSendMessage(message, prog, this.mexBatchMonitor);
        flag = true;
      }
      catch (PhaseException ex)
      {
        problem = (Exception) ex;
        throw ex;
      }
      catch (PushConnException ex)
      {
        problem = (Exception) ex;
        throw ex;
      }
      catch (PushServerException ex)
      {
        problem = (Exception) ex;
        throw ex;
      }
      catch (PushUserException ex)
      {
        problem = (Exception) ex;
        throw ex;
      }
      catch (SubscrException ex)
      {
        problem = (Exception) ex;
        throw ex;
      }
      finally
      {
        if (!flag)
        {
          ServerManager.actionsLogger.Info("Undoing sending of " + (object) message + " to session " + this.localPushServerProxy.SessionId);
          lock (this.sequencesHandler)
          {
            SequenceHandler sequence = this.sequencesHandler.GetSequence(message.Sequence);
            if (message != null)
              this.serverListener.OnMessageOutcome(message, sequence, (ServerUpdateEvent) null, problem);
          }
        }
      }
    }

    internal virtual void ChangeConstraints(ConnectionConstraints constraints) => this.localPushServerProxy.RequestNewConstraints(constraints);

    internal virtual SubscribedTableKey SubscrTable(ITableManager table, bool batchable)
    {
      ServerManager.actionsLogger.Info("Adding " + (object) table + " to session " + this.localPushServerProxy.SessionId);
      SubscribedTableKey tableCode;
      lock (this.tables.SyncRoot)
      {
        tableCode = this.localPushServerProxy.TableCode;
        this.tables[(object) tableCode.KeyValue] = (object) table;
      }
      bool flag = false;
      try
      {
        this.localPushServerProxy.RequestSubscr(table, tableCode, batchable ? this.batchMonitor : (BatchMonitor) null);
        flag = true;
      }
      finally
      {
        if (!flag)
        {
          ServerManager.actionsLogger.Info("Undoing add of " + (object) table + " to session " + this.localPushServerProxy.SessionId);
          lock (this.tables.SyncRoot)
            this.tables.Remove((object) tableCode.KeyValue);
        }
      }
      return tableCode;
    }

    internal virtual SubscribedTableKey[] SubscrItems(VirtualTableManager table, bool batchable)
    {
      if (table.NumItems == 0)
      {
        if (batchable)
          this.UnbatchRequest();
        return new SubscribedTableKey[0];
      }
      SubscribedTableKey[] subscrKeys = new SubscribedTableKey[table.NumItems];
      ServerManager.actionsLogger.Info("Adding " + (object) table + " to session " + this.localPushServerProxy.SessionId);
      lock (this.tables.SyncRoot)
      {
        for (int i = 0; i < table.NumItems; ++i)
        {
          subscrKeys[i] = this.localPushServerProxy.TableCode;
          this.tables[(object) subscrKeys[i].KeyValue] = table.GetItemManager(i);
        }
      }
      bool flag = false;
      try
      {
        this.localPushServerProxy.RequestItemsSubscr(table, subscrKeys, batchable ? this.batchMonitor : (BatchMonitor) null);
        flag = true;
      }
      finally
      {
        if (!flag)
        {
          ServerManager.actionsLogger.Info("Undoing add of " + (object) table + " to session " + this.localPushServerProxy.SessionId);
          lock (this.tables.SyncRoot)
          {
            for (int index = 0; index < subscrKeys.Length; ++index)
              this.tables.Remove((object) subscrKeys[index].KeyValue);
          }
        }
      }
      return subscrKeys;
    }

    internal virtual ITableManager[] FindTables(SubscribedTableKey[] subscrKeys)
    {
      ITableManager[] tables = new ITableManager[subscrKeys.Length];
      lock (this.tables.SyncRoot)
      {
        for (int index = 0; index < subscrKeys.Length; ++index)
        {
          if (subscrKeys[index].KeyValue != -1)
          {
            object table = this.tables[(object) subscrKeys[index].KeyValue];
            tables[index] = (ITableManager) table;
          }
          else
            tables[index] = (ITableManager) null;
        }
      }
      return tables;
    }

    internal virtual void ConstrainTables(
      SubscribedTableKey[] subscrKeys,
      SubscriptionConstraints constraints)
    {
      if (subscrKeys.Length == 0)
        return;
      this.localPushServerProxy.ConstrainSubscrs(subscrKeys, constraints);
    }

    internal virtual ITableManager[] DetachTables(SubscribedTableKey[] subscrKeys)
    {
      ITableManager[] tableManagerArray = new ITableManager[subscrKeys.Length];
      lock (this.tables.SyncRoot)
      {
        for (int index = 0; index < subscrKeys.Length; ++index)
        {
          if (subscrKeys[index].KeyValue != -1)
          {
            object table = this.tables[(object) subscrKeys[index].KeyValue];
            this.tables.Remove((object) subscrKeys[index].KeyValue);
            tableManagerArray[index] = (ITableManager) table;
          }
          else
            tableManagerArray[index] = (ITableManager) null;
        }
      }
      if (ServerManager.actionsLogger.IsInfoEnabled)
      {
        for (int index = 0; index < subscrKeys.Length; ++index)
        {
          if (tableManagerArray[index] != null)
            ServerManager.actionsLogger.Info("Removed " + (object) tableManagerArray[index] + " from session " + this.localPushServerProxy.SessionId);
        }
      }
      return tableManagerArray;
    }

    internal virtual void UnsubscrTables(SubscribedTableKey[] subscrKeys, bool batchable)
    {
      if (subscrKeys.Length == 0)
      {
        if (!batchable)
          return;
        this.UnbatchRequest();
      }
      else
        this.localPushServerProxy.DelSubscrs(subscrKeys, batchable ? this.batchMonitor : (BatchMonitor) null);
    }

    internal virtual void BatchRequests(int batchSize) => this.BatchRequests(batchSize, this.batchMonitor, false);

    internal virtual void BatchMessageRequests(int batchSize) => this.BatchRequests(batchSize, this.mexBatchMonitor, true);

    private void BatchRequests(int batchSize, BatchMonitor monitor, bool messageBatch)
    {
      lock (monitor)
      {
        if (monitor.Filled)
        {
          if (messageBatch)
            this.localPushServerProxy.StartMessageBatch();
          else
            this.localPushServerProxy.StartBatch();
          if (batchSize <= 0)
            ServerManager.actionsLogger.Debug("Starting a new batch for unlimited requests in session " + this.localPushServerProxy.SessionId);
          else
            ServerManager.actionsLogger.Debug("Starting a new batch for " + (object) batchSize + " requests in session " + this.localPushServerProxy.SessionId);
        }
        else if (batchSize <= 0)
          ServerManager.actionsLogger.Debug("Extending the current batch with unlimited requests in session " + this.localPushServerProxy.SessionId);
        else
          ServerManager.actionsLogger.Debug("Extending the current batch with " + (object) batchSize + " requests in session " + this.localPushServerProxy.SessionId);
        monitor.Expand(batchSize);
      }
    }

    internal virtual void UnbatchRequest()
    {
      lock (this.batchMonitor)
      {
        if (this.batchMonitor.Filled)
          return;
        this.batchMonitor.UseOne();
        if (this.batchMonitor.Filled)
        {
          ServerManager.actionsLogger.Debug("Shrinking and executing the current batch in session " + this.localPushServerProxy.SessionId);
          this.localPushServerProxy.CloseBatch();
        }
        else
          ServerManager.actionsLogger.Debug("Shrinking the current batch in session " + this.localPushServerProxy.SessionId);
      }
    }

    internal virtual void CloseMessageBatch() => this.CloseBatch(this.mexBatchMonitor, true);

    internal virtual void CloseBatch() => this.CloseBatch(this.batchMonitor, false);

    internal virtual void CloseBatch(BatchMonitor monitor, bool messageBatch)
    {
      lock (monitor)
      {
        ServerManager.actionsLogger.Debug("Executing the current batch in session " + this.localPushServerProxy.SessionId);
        if (messageBatch)
          this.localPushServerProxy.CloseMessageBatch();
        else
          this.localPushServerProxy.CloseBatch();
        monitor.Clear();
      }
    }

    private ITableManager GetUpdatedTable(ServerUpdateEvent values)
    {
      lock (this.tables.SyncRoot)
        return (ITableManager) this.tables[(object) values.TableCode];
    }

    private void AbortPendingMessages()
    {
      lock (this.sequencesHandler)
      {
        IEnumerator<KeyValuePair<string, SequenceHandler>> enumerator = this.sequencesHandler.Reset();
        try
        {
          enumerator.Reset();
        }
        catch (NotSupportedException ex)
        {
        }
        while (enumerator.MoveNext())
        {
          SequenceHandler sequence = enumerator.Current.Value;
          MessageManager[] messageManagerArray = sequence.Iterator();
          for (int index = 0; index < messageManagerArray.Length; ++index)
          {
            if (!messageManagerArray[index].HasOutcome())
              this.serverListener.OnMessageOutcome(messageManagerArray[index], sequence, (ServerUpdateEvent) null, (Exception) null);
          }
        }
        this.serverListener.OnEndMessages();
      }
    }

    private void TableUpdate(ServerUpdateEvent values)
    {
      ITableManager updatedTable = this.GetUpdatedTable(values);
      if (updatedTable == null)
      {
        if (this.localPushServerProxy.IsTableCodeConsumed(values.TableCode))
          return;
        this.serverListener.OnDataError(new PushServerException(1));
      }
      else
        this.serverListener.OnUpdate(updatedTable, values);
    }

    private void MessageUpdate(ServerUpdateEvent values)
    {
      if (values.ErrorCode == 39)
      {
        this.ExpandMultipleMessageUpdate(values);
      }
      else
      {
        lock (this.sequencesHandler)
        {
          SequenceHandler sequence = this.sequencesHandler.GetSequence(values.MessageSequence);
          MessageManager message = sequence.GetMessage(values.MessageProg);
          if (message == null)
            this.serverListener.OnDataError(new PushServerException(13));
          else
            this.serverListener.OnMessageOutcome(message, sequence, values, (Exception) null);
        }
      }
    }

    private void ExpandMultipleMessageUpdate(ServerUpdateEvent values)
    {
      int num = 0;
      bool flag = false;
      try
      {
        num = Convert.ToInt32(values.ErrorMessage);
        flag = true;
      }
      catch (FormatException ex)
      {
      }
      catch (OverflowException ex)
      {
      }
      finally
      {
        if (!flag)
          this.serverListener.OnDataError(new PushServerException(7));
      }
      if (!flag || num <= 0)
        return;
      for (int messageProg = values.MessageProg - num + 1; messageProg <= values.MessageProg; ++messageProg)
        this.MessageUpdate(new ServerUpdateEvent(values.MessageSequence, messageProg, 38, "Message discarded"));
    }

    internal virtual async Task WaitEvents()
    {
      long giaLetti = 0;
      try
      {
        this.activityController.OnConnectionReturned();
        ServerManager.sessionLogger.Info("Listening for updates on session " + this.localPushServerProxy.SessionId);
        while (true)
        {
          ServerUpdateEvent values;
          try
          {
            values = this.localPushServerProxy.WaitUpdate(this.activityController);
          }
          catch (PushServerException ex)
          {
            ServerManager.protLogger.Debug("Error in received data", (Exception) ex);
            ServerManager.sessionLogger.Error("Error while listening for data in session " + this.localPushServerProxy.SessionId);
            this.serverListener.OnDataError(ex);
            continue;
          }
          if (values != null)
          {
            if (values.TableUpdate)
            {
              this.TableUpdate(values);
              if (ServerManager.actionsLogger.IsDebugEnabled)
                ServerManager.actionsLogger.Debug("Updated values consumed, going to wait for new ones.");
            }
            else
            {
              if (values.Loop)
              {
                long holdingMillis = values.HoldingMillis;
                long recoveryProg = values.RecoveryProg;
                if (holdingMillis > 0L)
                {
                  try
                  {
                    await Task.Delay((int) holdingMillis);
                  }
                  catch (Exception ex)
                  {
                  }
                }
                if (recoveryProg >= 0L)
                  this.activityController.OnActivityWarning(true);
                if (await this.Rebind(this.activityController, recoveryProg))
                {
                  if (recoveryProg >= 0L)
                    this.activityController.OnActivityWarning(false);
                  this.activityController.OnConnectionReturned();
                  continue;
                }
                break;
              }
              this.MessageUpdate(values);
            }
          }
          long totalBytes = this.localPushServerProxy.TotalBytes;
          this.serverListener.OnNewBytes(totalBytes - giaLetti);
          giaLetti = totalBytes;
          values = (ServerUpdateEvent) null;
        }
      }
      catch (PushConnException ex)
      {
        ServerManager.streamLogger.Debug("Error in connection", (Exception) ex);
        ServerManager.sessionLogger.Error("Error while listening for data in session " + this.localPushServerProxy.SessionId);
        this.serverListener.OnFailure(ex);
      }
      catch (PushEndException ex)
      {
        ServerManager.streamLogger.Debug("Forced connection end", (Exception) ex);
        if (this.activityController.IsCloseUnexpected())
          ServerManager.sessionLogger.Error("Connection forcibly closed by the Server in session " + this.localPushServerProxy.SessionId);
        this.serverListener.OnEnd(ex.EndCause);
      }
      catch (PhaseException ex)
      {
        ServerManager.sessionLogger.Info("Listening loop closed for session " + this.localPushServerProxy.SessionId);
      }
      finally
      {
        this.activityController.SetTerminated();
      }
    }

    internal virtual async Task<bool> Rebind(
      ServerManager.ActivityController activityController,
      long recoveryProg)
    {
      long limitTime = TimerSupport.getTimeMillis() + this.connInfo.RecoveryTimeoutMillis;
      while (true)
      {
        long timeMillis1 = TimerSupport.getTimeMillis();
        if (recoveryProg >= 0L)
        {
          if (timeMillis1 >= limitTime)
            activityController.StartConnection(false, 0L);
          else
            activityController.StartConnection(false, limitTime - timeMillis1);
        }
        else
          activityController.StartConnection(false, -1L);
        try
        {
          try
          {
            this.localPushServerProxy.ResyncSession(recoveryProg);
            return true;
          }
          catch (PushEndException ex)
          {
            ServerManager.streamLogger.Debug("Forced connection end", (Exception) ex);
            ServerManager.sessionLogger.Error("Connection forcibly closed by the Server while trying to rebind to session " + this.localPushServerProxy.SessionId);
            this.serverListener.OnEnd(ex.EndCause);
          }
          catch (PushServerException ex)
          {
            ServerManager.protLogger.Debug("Error in rebinding to the session", (Exception) ex);
            ServerManager.sessionLogger.Error("Error while trying to rebind to session " + this.localPushServerProxy.SessionId);
            this.serverListener.OnFailure(ex);
          }
          catch (PushConnException ex1)
          {
            ServerManager.streamLogger.Debug("Error in connection", (Exception) ex1);
            if (recoveryProg >= 0L)
            {
              long num = timeMillis1 + this.connInfo.ReconnectionTimeoutMillis;
              if (num >= limitTime)
              {
                ServerManager.sessionLogger.Error("Error while trying to recover session " + this.localPushServerProxy.SessionId);
                this.serverListener.OnFailure(ex1);
              }
              else
              {
                ServerManager.sessionLogger.Warn("Error while trying to recover session " + this.localPushServerProxy.SessionId + " (will retry)");
                activityController.StopConnection();
                long timeMillis2 = TimerSupport.getTimeMillis();
                if (timeMillis2 < num)
                {
                  try
                  {
                    await Task.Delay((int) (num - timeMillis2));
                    continue;
                  }
                  catch (Exception ex2)
                  {
                    continue;
                  }
                }
                else
                  continue;
              }
            }
            else
            {
              ServerManager.sessionLogger.Error("Error while trying to rebind to session " + this.localPushServerProxy.SessionId);
              this.serverListener.OnFailure(ex1);
            }
            ex1 = (PushConnException) null;
            break;
          }
          catch (PhaseException ex)
          {
            ServerManager.sessionLogger.Info("Listening loop closed for session " + this.localPushServerProxy.SessionId);
          }
          break;
        }
        finally
        {
          activityController.StopConnection();
        }
      }
      return false;
    }

    private class SessionActivityManager : ThreadSupport
    {
      private ServerManager enclosingInstance;

      internal SessionActivityManager(ServerManager enclosingInstance, string Param1)
        : base(Param1)
      {
        this.enclosingInstance = enclosingInstance;
      }

      public override async void Run()
      {
        try
        {
          await this.enclosingInstance.WaitEvents();
        }
        catch (Exception ex)
        {
          PushServerException e = new PushServerException(12, ex);
          ServerManager.protLogger.Debug("Error in received data", ex);
          ServerManager.sessionLogger.Error("Unrecoverable error while listening to data in session " + this.enclosingInstance.localPushServerProxy.SessionId);
          this.enclosingInstance.serverListener.OnFailure(e);
        }
        finally
        {
          this.enclosingInstance.serverListener.OnClose();
        }
      }
    }

    internal class ActivityController
    {
      private ServerManager enclosingInstance;
      private long lastActivity;
      private bool warningPending;
      private bool connectionCheck;
      private bool isFirstConn;
      internal bool streamingConfirmed;
      internal bool streamingNotified;
      private int phase = 1;
      private bool expectingInterruptedConnection;
      private bool terminated;

      public ActivityController(ServerManager enclosingInstance)
      {
        this.enclosingInstance = enclosingInstance;
        this.streamingConfirmed = enclosingInstance.connInfo.Polling;
      }

      public virtual void StartKeepalives()
      {
        lock (this)
        {
          this.warningPending = false;
          this.connectionCheck = false;
          this.lastActivity = 0L;
          ++this.phase;
          this.Launch(this.enclosingInstance.localPushServerProxy.KeepaliveMillis + this.enclosingInstance.connInfo.ProbeWarningMillis, this.phase);
        }
      }

      public void OnConnectionReturned()
      {
        lock (this)
        {
          if (!this.enclosingInstance.connInfo.Polling && this.streamingConfirmed && !this.streamingNotified)
          {
            this.OnStreamingResponse();
            this.streamingNotified = true;
          }
          this.StartKeepalives();
        }
      }

      public virtual void OnActivity()
      {
        lock (this)
        {
          if (this.warningPending)
          {
            this.OnActivityWarning(false);
            this.warningPending = false;
            this.lastActivity = 0L;
            ++this.phase;
            this.Launch(this.enclosingInstance.localPushServerProxy.KeepaliveMillis + this.enclosingInstance.connInfo.ProbeWarningMillis, this.phase);
          }
          else
            this.lastActivity = TimerSupport.getTimeMillis();
        }
      }

      public void OnCloseRequested()
      {
        lock (this)
          this.expectingInterruptedConnection = true;
      }

      public bool IsCloseUnexpected()
      {
        lock (this)
          return !this.expectingInterruptedConnection;
      }

      public void SetTerminated()
      {
        lock (this)
          this.terminated = true;
      }

      public void OnInterrupt()
      {
        lock (this)
        {
          if (!this.terminated)
            this.enclosingInstance.worker.Abort();
        }
        if (this.terminated)
          return;
        this.enclosingInstance.serverListener.OnClose();
      }

      public virtual void StopKeepalives()
      {
        lock (this)
        {
          this.OnActivity();
          ++this.phase;
        }
      }

      public virtual void StartConnection(bool isFirstConnect, long enforcedCheckTime)
      {
        lock (this)
        {
          this.connectionCheck = true;
          this.isFirstConn = isFirstConnect;
          ++this.phase;
          long millis;
          if (!isFirstConnect)
          {
            millis = this.enclosingInstance.connInfo.ReconnectionTimeoutMillis;
            if (enforcedCheckTime >= 0L)
              millis = enforcedCheckTime;
            else if (this.enclosingInstance.connInfo.Polling)
              millis += this.enclosingInstance.connInfo.PollingIdleMillis;
            else if (!this.streamingConfirmed)
              millis = this.enclosingInstance.connInfo.StreamingTimeoutMillis;
          }
          else
          {
            if (this.enclosingInstance.connInfo.StartsWithPoll() || this.streamingConfirmed)
              return;
            millis = this.enclosingInstance.connInfo.StreamingTimeoutMillis;
          }
          this.Launch(millis, this.phase);
        }
      }

      public virtual void StopConnection()
      {
        lock (this)
        {
          if (!this.isFirstConn)
          {
            if (!this.enclosingInstance.connInfo.Polling && !this.streamingConfirmed)
              this.streamingConfirmed = true;
          }
          else if (!this.enclosingInstance.connInfo.StartsWithPoll() && !this.streamingConfirmed)
            this.streamingConfirmed = true;
          ++this.phase;
        }
      }

      public virtual void OnTimeout(int refPhase)
      {
        lock (this)
        {
          if (refPhase != this.phase)
            return;
          if (this.connectionCheck)
          {
            this.OnConnectionTimeout(this.isFirstConn);
            ++this.phase;
          }
          else if (this.warningPending)
          {
            this.OnNoActivity();
            ++this.phase;
          }
          else if (this.lastActivity == 0L)
          {
            this.OnActivityWarning(true);
            this.warningPending = true;
            this.Launch(this.enclosingInstance.connInfo.ProbeTimeoutMillis, this.phase);
          }
          else
          {
            long millis = this.lastActivity + (this.enclosingInstance.localPushServerProxy.KeepaliveMillis + this.enclosingInstance.connInfo.ProbeWarningMillis) - TimerSupport.getTimeMillis();
            this.lastActivity = 0L;
            if (millis > 0L)
              this.Launch(millis, refPhase);
            else
              this.OnTimeout(refPhase);
          }
        }
      }

      private void Launch(long millis, int currPhase) => ServerManager.activityTimer.Schedule((IThreadRunnable) new ServerManager.ActivityController.AnonymousClassTimerTask(currPhase, this), millis);

      internal void OnStreamingResponse() => ServerManager.notificationsSender.Add((NotificationQueue.Notify) (() =>
      {
        ServerManager.actionsLogger.Debug("Notifying return on the current connection");
        this.enclosingInstance.serverListener.OnStreamingReturned();
      }));

      private void OnNoActivity() => ServerManager.notificationsSender.Add((NotificationQueue.Notify) (() =>
      {
        if (!this.enclosingInstance.serverListener.OnFailure(new PushServerException(10)))
          return;
        ServerManager.sessionLogger.Info("Terminating session " + this.enclosingInstance.localPushServerProxy.SessionId + " because of an activity timeout");
        this.enclosingInstance.localPushServerProxy.Dispose(true);
        this.enclosingInstance.activityController.OnInterrupt();
      }));

      private void OnConnectionTimeout(bool isFirstConn) => ServerManager.notificationsSender.Add((NotificationQueue.Notify) (() =>
      {
        if (isFirstConn)
        {
          ServerManager.actionsLogger.Debug("Notifying a timeout check on the current connection");
          this.enclosingInstance.serverListener.OnConnectTimeout();
        }
        else
        {
          if (!this.enclosingInstance.serverListener.OnReconnectTimeout())
            return;
          ServerManager.sessionLogger.Info("Terminating session " + this.enclosingInstance.localPushServerProxy.SessionId + " because of a reconnection timeout");
          this.enclosingInstance.localPushServerProxy.Dispose(true);
          this.enclosingInstance.activityController.OnInterrupt();
        }
      }));

      internal void OnActivityWarning(bool warningOn) => ServerManager.notificationsSender.Add((NotificationQueue.Notify) (() =>
      {
        if (!this.enclosingInstance.serverListener.OnActivityWarning(warningOn))
          return;
        if (warningOn)
          ServerManager.sessionLogger.Info("Session " + this.enclosingInstance.localPushServerProxy.SessionId + " stalled");
        else
          ServerManager.sessionLogger.Info("Session " + this.enclosingInstance.localPushServerProxy.SessionId + " no longer stalled");
      }));

      private class AnonymousClassTimerTask : IThreadRunnable
      {
        private int currPhase;
        private ServerManager.ActivityController enclosingInstance;

        public AnonymousClassTimerTask(
          int currPhase,
          ServerManager.ActivityController enclosingInstance)
        {
          this.currPhase = currPhase;
          this.enclosingInstance = enclosingInstance;
        }

        public void Run() => this.enclosingInstance.OnTimeout(this.currPhase);
      }
    }

    internal interface IServerListener
    {
      void OnConnectionEstablished();

      void OnSessionStarted(bool isPolling);

      bool OnUpdate(ITableManager table, ServerUpdateEvent values);

      bool OnMessageOutcome(
        MessageManager message,
        SequenceHandler sequence,
        ServerUpdateEvent values,
        Exception problem);

      void OnEndMessages();

      bool OnNewBytes(long bytes);

      void OnStreamingReturned();

      bool OnActivityWarning(bool warningOn);

      bool OnDataError(PushServerException e);

      bool OnEnd(int endCause);

      bool OnReconnectTimeout();

      void OnConnectTimeout();

      void OnConnectException(Exception e);

      bool OnFailure(PushServerException e);

      bool OnFailure(PushConnException e);

      bool OnClose();
    }
  }
}
