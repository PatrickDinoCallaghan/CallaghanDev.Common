// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.LSClient
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Threading.Tasks;

namespace Lightstreamer.DotNet.Client
{
  public class LSClient
  {
    private object stateMutex = new object();
    private int phase;
    private ServerManager connManager;
    private IConnectionListener connListener;
    private MyServerListener asyncListener;
    private LSClient subClient;
    private static ILog actionsLogger = LogManager.GetLogger("com.lightstreamer.ls_client.actions");
    private bool sendMessageAutoBatchingEnabled = true;

    private ServerManager ConnManager
    {
      get
      {
        lock (this.stateMutex)
        {
          if (this.subClient != null)
            return this.subClient.ConnManager;
          return this.connManager != null ? this.connManager : throw new SubscrException("Connection closed");
        }
      }
    }

    internal IConnectionListener GetActiveListener(int currPhase)
    {
      lock (this.stateMutex)
      {
        if (this.subClient != null)
          return this.subClient.GetActiveListener(currPhase);
        return currPhase == this.phase ? this.connListener : (IConnectionListener) null;
      }
    }

    public static void SetLoggerProvider(ILoggerProvider loggerProvider) => LogManager.SetLoggerProvider(loggerProvider);

    public virtual void OpenConnection(ConnectionInfo info, IConnectionListener listener)
    {
      int currPhase;
      lock (this.stateMutex)
      {
        this.CloseConnection();
        currPhase = ++this.phase;
      }
      ConnectionInfo info1 = (ConnectionInfo) info.Clone();
      info1.useGetForStreaming = true;
      if (info1.EnableStreamSense && !info1.Polling)
      {
        LSClient testClient = new LSClient();
        ExtConnectionListener myListener = new ExtConnectionListener(listener);
        ConnectionInfo mySubInfo = (ConnectionInfo) info.Clone();
        mySubInfo.EnableStreamSense = false;
        Task.Run((Action) (() =>
        {
          try
          {
            testClient.OpenConnection(mySubInfo, (IConnectionListener) myListener);
          }
          catch (Exception ex)
          {
          }
        }));
        if (!myListener.WaitStreamingTimeoutAnswer())
        {
          lock (this.stateMutex)
          {
            if (currPhase == this.phase)
            {
              this.subClient = testClient;
            }
            else
            {
              LSClient.AsynchCloseConnection(testClient);
              return;
            }
          }
          myListener.FlushAndStart();
        }
        else
        {
          LSClient.AsynchCloseConnection(testClient);
          lock (this.stateMutex)
          {
            if (currPhase != this.phase)
              return;
          }
          LSClient testClient1 = new LSClient();
          info1.Polling = true;
          testClient1.OpenConnection(info1, listener);
          lock (this.stateMutex)
          {
            if (currPhase == this.phase)
              this.subClient = testClient1;
            else
              LSClient.AsynchCloseConnection(testClient1);
          }
        }
      }
      else
      {
        MyServerListener myServerListener = new MyServerListener(this, listener, currPhase);
        bool flag = false;
        try
        {
          ServerManager closingManager = new ServerManager(info1, (ServerManager.IServerListener) myServerListener);
          closingManager.Connect();
          flag = true;
          lock (this.stateMutex)
          {
            if (currPhase == this.phase)
            {
              this.connListener = listener;
              this.asyncListener = myServerListener;
              this.connManager = closingManager;
            }
            else
            {
              LSClient.CloseFlushing(closingManager, myServerListener, listener);
              return;
            }
          }
          closingManager.Start();
        }
        finally
        {
          if (!flag)
          {
            myServerListener.OnClosed((IConnectionListener) null);
            myServerListener.OnEndMessages();
          }
        }
      }
    }

    public virtual void CloseConnection()
    {
      IConnectionListener activeListener = (IConnectionListener) null;
      ServerManager closingManager = (ServerManager) null;
      MyServerListener closeListener = (MyServerListener) null;
      LSClient lsClient = (LSClient) null;
      lock (this.stateMutex)
      {
        ++this.phase;
        if (this.subClient != null)
        {
          lsClient = this.subClient;
          this.subClient = (LSClient) null;
        }
        else
        {
          if (this.connManager == null)
            return;
          closingManager = this.connManager;
          activeListener = this.connListener;
          closeListener = this.asyncListener;
          this.connManager = (ServerManager) null;
          this.connListener = (IConnectionListener) null;
          this.asyncListener = (MyServerListener) null;
        }
      }
      if (lsClient != null)
        lsClient.CloseConnection();
      else
        LSClient.CloseFlushing(closingManager, closeListener, activeListener);
    }

    private static void AsynchCloseConnection(LSClient testClient) => Task.Run((Action) (() => testClient.CloseConnection()));

    private static void CloseFlushing(
      ServerManager closingManager,
      MyServerListener closeListener,
      IConnectionListener activeListener)
    {
      foreach (ITableManager tableManager in closingManager.Close())
        tableManager.NotifyUnsub();
      closeListener.OnClosed(activeListener);
    }

    public virtual void SendMessage(string message)
    {
      ServerManager connManager;
      try
      {
        connManager = this.ConnManager;
      }
      catch (SubscrException ex)
      {
        return;
      }
      try
      {
        connManager.SendMessage(message);
      }
      catch (PhaseException ex)
      {
      }
    }

    public virtual int SendMessage(MessageInfo message, ISendMessageListener listener)
    {
      ServerManager connManager;
      try
      {
        connManager = this.ConnManager;
      }
      catch (SubscrException ex)
      {
        return 0;
      }
      try
      {
        return connManager.SendMessage(new MessageManager(message, listener), this.sendMessageAutoBatchingEnabled);
      }
      catch (PhaseException ex)
      {
        return 0;
      }
      catch (SubscrException ex)
      {
        return 0;
      }
    }

    public virtual void ChangeConstraints(ConnectionConstraints constraints)
    {
      ServerManager connManager;
      try
      {
        connManager = this.ConnManager;
      }
      catch (SubscrException ex)
      {
        return;
      }
      try
      {
        connManager.ChangeConstraints(constraints);
      }
      catch (PhaseException ex)
      {
      }
    }

    public virtual SubscribedTableKey SubscribeTable(
      SimpleTableInfo table,
      IHandyTableListener listener,
      bool commandLogic)
    {
      ServerManager connManager = this.ConnManager;
      ITableManager table1 = (ITableManager) new FullTableManager(table, listener, commandLogic);
      try
      {
        return connManager.SubscrTable(table1, true);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual SubscribedTableKey SubscribeTable(
      ExtendedTableInfo table,
      IHandyTableListener listener,
      bool commandLogic)
    {
      ServerManager connManager = this.ConnManager;
      ITableManager table1 = (ITableManager) new FullTableManager((SimpleTableInfo) table, listener, commandLogic);
      try
      {
        return connManager.SubscrTable(table1, true);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual SubscribedTableKey[] SubscribeItems(
      ExtendedTableInfo items,
      IHandyTableListener listener)
    {
      ServerManager connManager = this.ConnManager;
      VirtualTableManager table = new VirtualTableManager(items, listener);
      try
      {
        return connManager.SubscrItems(table, true);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void ChangeSubscription(
      SubscribedTableKey tableKey,
      SubscriptionConstraints constraints)
    {
      ServerManager connManager = this.ConnManager;
      SubscribedTableKey[] subscrKeys = new SubscribedTableKey[1]
      {
        tableKey
      };
      if (connManager.FindTables(subscrKeys)[0] == null)
        return;
      try
      {
        connManager.ConstrainTables(subscrKeys, constraints);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void ChangeSubscriptions(
      SubscribedTableKey[] tableKeys,
      SubscriptionConstraints constraints)
    {
      ServerManager connManager = this.ConnManager;
      ITableManager[] tables = connManager.FindTables(tableKeys);
      int length = 0;
      for (int index = 0; index < tables.Length; ++index)
      {
        if (tables[index] != null)
          ++length;
      }
      if (length == 0)
        return;
      SubscribedTableKey[] subscrKeys = new SubscribedTableKey[length];
      int index1 = 0;
      for (int index2 = 0; index2 < tables.Length; ++index2)
      {
        if (tables[index2] != null)
        {
          subscrKeys[index1] = tableKeys[index2];
          ++index1;
        }
      }
      try
      {
        connManager.ConstrainTables(subscrKeys, constraints);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void UnsubscribeTable(SubscribedTableKey tableKey)
    {
      ServerManager connManager = this.ConnManager;
      SubscribedTableKey[] subscrKeys = new SubscribedTableKey[1]
      {
        tableKey
      };
      ITableManager[] tableManagerArray = connManager.DetachTables(subscrKeys);
      if (tableManagerArray[0] == null)
      {
        try
        {
          connManager.UnsubscrTables(new SubscribedTableKey[0], true);
        }
        catch (PhaseException ex)
        {
        }
        throw new SubscrException("Table not found");
      }
      tableManagerArray[0].NotifyUnsub();
      try
      {
        connManager.UnsubscrTables(subscrKeys, true);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void UnsubscribeTables(SubscribedTableKey[] tableKeys)
    {
      ServerManager connManager = this.ConnManager;
      ITableManager[] tableManagerArray = connManager.DetachTables(tableKeys);
      int length = 0;
      for (int index = 0; index < tableManagerArray.Length; ++index)
      {
        if (tableManagerArray[index] != null)
        {
          tableManagerArray[index].NotifyUnsub();
          ++length;
        }
      }
      SubscribedTableKey[] subscrKeys = new SubscribedTableKey[length];
      int index1 = 0;
      for (int index2 = 0; index2 < tableManagerArray.Length; ++index2)
      {
        if (tableManagerArray[index2] != null)
        {
          subscrKeys[index1] = tableKeys[index2];
          ++index1;
        }
      }
      try
      {
        connManager.UnsubscrTables(subscrKeys, true);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void ForceUnsubscribeTable(SubscribedTableKey tableKey)
    {
      ServerManager connManager = this.ConnManager;
      if (tableKey.KeyValue == -1)
        return;
      SubscribedTableKey[] subscrKeys = new SubscribedTableKey[1]
      {
        tableKey
      };
      try
      {
        connManager.UnsubscrTables(subscrKeys, false);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void BatchRequests(int batchSize)
    {
      ServerManager connManager = this.ConnManager;
      try
      {
        connManager.BatchRequests(batchSize);
      }
      catch (PhaseException ex)
      {
        throw new SubscrException("Connection closed");
      }
    }

    public virtual void UnbatchRequest()
    {
      try
      {
        this.ConnManager.UnbatchRequest();
      }
      catch (SubscrException ex)
      {
        LSClient.actionsLogger.Debug("Unbatch request received with no open session");
      }
    }

    public virtual void CloseBatch()
    {
      try
      {
        this.ConnManager.CloseBatch();
      }
      catch (SubscrException ex)
      {
        LSClient.actionsLogger.Debug("Unbatch request received with no open session");
      }
    }
  }
}
