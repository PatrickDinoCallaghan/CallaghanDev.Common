// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.BatchManager
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections;
using System.Net;
using System.Text;

namespace Lightstreamer.DotNet.Client
{
  internal class BatchManager
  {
    private BatchingHttpProvider batchingProvider;
    private long limit;
    private static ILog protLogger = LogManager.GetLogger("com.lightstreamer.ls_client.protocol");
    private CookieContainer cookies;
    private ConnectionInfo info;

    public BatchManager(CookieContainer cookies, ConnectionInfo info)
    {
      this.cookies = cookies;
      this.info = info;
    }

    internal virtual long Limit
    {
      set => this.limit = value;
    }

    internal virtual LSStreamReader GetAnswer(
      string controlUrl,
      IDictionary parameters,
      BatchMonitor batch)
    {
      BatchingHttpProvider batchToClose = (BatchingHttpProvider) null;
      BatchingHttpProvider.BufferedReaderMonitor bufferedReaderMonitor = (BatchingHttpProvider.BufferedReaderMonitor) null;
      bool flag = false;
      lock (batch)
      {
        lock (this)
        {
          if (!batch.Filled)
          {
            batch.UseOne();
            if (this.batchingProvider != null)
            {
              BatchManager.protLogger.Info("Batching control request");
              if (BatchManager.protLogger.IsDebugEnabled)
                BatchManager.protLogger.Debug("Control params: " + CollectionsSupport.ToString((ICollection) parameters));
              bufferedReaderMonitor = this.batchingProvider.AddCall(parameters);
              if (bufferedReaderMonitor != null)
              {
                if (batch.Filled)
                {
                  batchToClose = this.batchingProvider;
                  this.batchingProvider = (BatchingHttpProvider) null;
                }
              }
              else if (this.batchingProvider.Empty)
              {
                BatchManager.protLogger.Info("Batching failed; trying without batch");
                if (batch.Filled)
                  this.batchingProvider = (BatchingHttpProvider) null;
              }
              else
              {
                BatchManager.protLogger.Info("Batching failed; trying a new batch");
                batchToClose = this.batchingProvider;
                batch.Expand(1);
                this.batchingProvider = new BatchingHttpProvider(controlUrl, this.limit, this.cookies, this.info.HttpExtraHeaders, this.info.ReadTimeoutMillis, this.info.ConnectTimeoutMillis);
                flag = true;
              }
            }
          }
          else if (this.batchingProvider != null)
          {
            this.batchingProvider.Abort((Exception) new SubscrException("wrong requests batch"));
            this.batchingProvider = (BatchingHttpProvider) null;
          }
        }
      }
      if (batchToClose != null)
        BatchManager.DoAsyncPost(batchToClose);
      if (bufferedReaderMonitor != null)
      {
        batch.BatchedOne();
        return bufferedReaderMonitor.GetReader();
      }
      return flag ? this.GetAnswer(controlUrl, parameters, batch) : this.GetNotBatchedAnswer(controlUrl, parameters);
    }

    internal virtual LSStreamReader GetNotBatchedAnswer(string controlUrl, IDictionary parameters)
    {
      HttpProvider httpProvider = new HttpProvider(controlUrl, this.cookies, this.info.HttpExtraHeaders, this.info.ReadTimeoutMillis, this.info.ConnectTimeoutMillis);
      BatchManager.protLogger.Info("Opening control connection");
      if (BatchManager.protLogger.IsDebugEnabled)
        BatchManager.protLogger.Debug("Control params: " + CollectionsSupport.ToString((ICollection) parameters));
      IDictionary parameters1 = parameters;
      return new LSStreamReader(httpProvider.DoHTTP(parameters1, true), Encoding.UTF8);
    }

    internal virtual void StartBatch(string controlUrl)
    {
      lock (this)
      {
        if (this.batchingProvider != null)
          this.batchingProvider.Abort((Exception) new SubscrException("requests batch discarded"));
        this.batchingProvider = new BatchingHttpProvider(controlUrl, this.limit, this.cookies, this.info.HttpExtraHeaders, this.info.ReadTimeoutMillis, this.info.ConnectTimeoutMillis);
      }
    }

    internal virtual void CloseBatch()
    {
      BatchingHttpProvider batchingProvider;
      lock (this)
      {
        batchingProvider = this.batchingProvider;
        this.batchingProvider = (BatchingHttpProvider) null;
      }
      if (batchingProvider == null)
        return;
      BatchManager.DoAsyncPost(batchingProvider);
    }

    private static void DoAsyncPost(BatchingHttpProvider batchToClose) => new BatchManager.AnonymousClassThread(batchToClose).Start(true);

    internal virtual void AbortBatch()
    {
      lock (this)
      {
        if (this.batchingProvider == null)
          return;
        this.batchingProvider.Abort((Exception) new SubscrException("requests batch aborted"));
        this.batchingProvider = (BatchingHttpProvider) null;
      }
    }

    private class AnonymousClassThread : ThreadSupport
    {
      private BatchingHttpProvider batchToClose;

      public AnonymousClassThread(BatchingHttpProvider batchToClose) => this.batchToClose = batchToClose;

      public override void Run()
      {
        try
        {
          BatchManager.protLogger.Info("Opening control connection to send batched requests");
          this.batchToClose.DoPosts();
        }
        catch (Exception ex)
        {
          BatchManager.protLogger.Error("Error in batch operation: " + ex.Message);
          this.batchToClose.Abort(ex);
        }
      }
    }
  }
}
