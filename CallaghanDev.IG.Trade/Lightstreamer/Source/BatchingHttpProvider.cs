// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.BatchingHttpProvider
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using Lightstreamer.DotNet.Client.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Lightstreamer.DotNet.Client
{
  internal class BatchingHttpProvider : HttpProvider
  {
    private BatchingHttpProvider.BufferedReaderMonitor firstReader;
    private BatchingHttpProvider.BufferedReaderMonitor lastReader;
    private bool consumed;
    private long limit;
    private static ILog streamLogger = LogManager.GetLogger("com.lightstreamer.ls_client.stream");

    public BatchingHttpProvider(
      string address,
      long limit,
      CookieContainer cookies,
      IDictionary<string, string> headers,
      int rt,
      int ct)
      : base(address, cookies, headers, rt, ct)
    {
      this.limit = limit;
    }

    public virtual BatchingHttpProvider.BufferedReaderMonitor AddCall(IDictionary parameters)
    {
      lock (this)
      {
        if (this.consumed)
          throw new SubscrException("Illegal use of a batch");
        if (!this.AddLine(parameters, this.limit))
          return (BatchingHttpProvider.BufferedReaderMonitor) null;
        BatchingHttpProvider.BufferedReaderMonitor bufferedReaderMonitor = new BatchingHttpProvider.BufferedReaderMonitor();
        if (this.lastReader == null)
          this.firstReader = bufferedReaderMonitor;
        else
          this.lastReader.Next = bufferedReaderMonitor;
        this.lastReader = bufferedReaderMonitor;
        return bufferedReaderMonitor;
      }
    }

    internal virtual bool Empty
    {
      get
      {
        lock (this)
          return this.firstReader == null;
      }
    }

    public virtual void DoPosts()
    {
      BatchingHttpProvider.BufferedReaderMonitor firstReader;
      lock (this)
      {
        if (this.consumed)
          return;
        this.consumed = true;
        firstReader = this.firstReader;
      }
      if (firstReader == null)
        return;
      Stream baseStream;
      try
      {
        baseStream = this.DoHTTP(true);
      }
      catch (IOException ex)
      {
        BatchingHttpProvider.streamLogger.Error("Error in batch operation: " + ex.Message);
        this.Abort((Exception) ex);
        return;
      }
      catch (WebException ex)
      {
        BatchingHttpProvider.streamLogger.Error("Error in batch operation: " + ex.Message);
        this.Abort((Exception) ex);
        return;
      }
      BatchingHttpProvider.MyReader answer = new BatchingHttpProvider.MyReader(new LSStreamReader(baseStream, Encoding.UTF8));
      firstReader.SetReader(answer);
    }

    public virtual void Abort(Exception error)
    {
      BatchingHttpProvider.BufferedReaderMonitor firstReader;
      lock (this)
      {
        int num = this.consumed ? 1 : 0;
        this.consumed = true;
        firstReader = this.firstReader;
      }
      if (firstReader == null)
        return;
      firstReader.Error = error;
    }

    internal class MyReader : LSStreamReader
    {
      private BatchingHttpProvider.BufferedReaderMonitor master;

      public virtual BatchingHttpProvider.BufferedReaderMonitor Master
      {
        set => this.master = value;
      }

      public MyReader(LSStreamReader reader)
        : base(reader.BaseStream, reader.CurrentEncoding)
      {
      }

      public override void Close()
      {
        if (this.master.OnReaderClose())
          return;
        BatchingHttpProvider.streamLogger.Debug("Closing control connection");
        base.Close();
      }
    }

    public class BufferedReaderMonitor
    {
      private static AutoResetEvent autoEvent = new AutoResetEvent(false);
      private Exception error;
      private BatchingHttpProvider.MyReader answer;
      private BatchingHttpProvider.BufferedReaderMonitor next;

      public virtual BatchingHttpProvider.BufferedReaderMonitor Next
      {
        set => this.next = value;
      }

      public virtual Exception Error
      {
        set
        {
          lock (this)
          {
            this.error = value;
            Monitor.Pulse((object) this);
          }
          if (this.next == null)
            return;
          this.next.Error = value;
        }
      }

      public void SetReader(BatchingHttpProvider.MyReader answer)
      {
        lock (this)
        {
          this.answer = answer;
          answer.Master = this;
          Monitor.Pulse((object) this);
        }
      }

      public virtual LSStreamReader GetReader()
      {
        lock (this)
        {
          while (this.answer == null)
          {
            if (this.error == null)
            {
              try
              {
                Monitor.Wait((object) this);
              }
              catch (Exception ex)
              {
              }
            }
            else
              break;
          }
          if (this.error == null)
            return (LSStreamReader) this.answer;
          if (this.error is IOException)
            throw (IOException) this.error;
          if (this.error is WebException)
            throw (WebException) this.error;
          if (this.error is SubscrException)
            throw (SubscrException) this.error;
          throw new SubscrException("Unexpected " + (object) this.error.GetType() + " :" + this.error.Message);
        }
      }

      public bool OnReaderClose()
      {
        if (this.next == null)
          return false;
        this.next.SetReader(this.answer);
        return true;
      }
    }
  }
}
