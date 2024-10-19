// Decompiled with JetBrains decompiler
// Type: Lightstreamer.DotNet.Client.TimerSupport
// Assembly: Lightstreamer_DotNet_PCL_Client, Version=3.1.6640.22649, Culture=neutral, PublicKeyToken=null
// MVID: 34D227FF-FB77-472F-96EF-5762687E571A
// Assembly location: C:\Users\patri\.nuget\packages\lightstreamer.dotnet.client\3.1.6640.22649\lib\portable45-net45+win8+wp8\Lightstreamer_DotNet_PCL_Client.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lightstreamer.DotNet.Client
{
  internal class TimerSupport
  {
    private IList _timers;

    public TimerSupport() => this._timers = (IList) new List<TimerSupport.TTimer>();

    public void Schedule(IThreadRunnable task, long delay)
    {
      TimerSupport.TimerCallback callback = new TimerSupport.TimerCallback(this.OnTimedEvent);
      TimerSupport.TimerContext timerContext = new TimerSupport.TimerContext();
      timerContext.task = task;
      int num = (int) delay;
      TimerSupport.TimerContext state = timerContext;
      int millisecondsDueTime = num;
      TimerSupport.TTimer ttimer = new TimerSupport.TTimer(callback, (object) state, millisecondsDueTime, -1);
      timerContext.timer = ttimer;
      lock (this)
        this._timers.Add((object) ttimer);
    }

    private void OnTimedEvent(object info)
    {
      TimerSupport.TimerContext timerContext = (TimerSupport.TimerContext) info;
      lock (this)
        this._timers.Remove((object) timerContext.timer);
      timerContext.task.Run();
      timerContext.timer.Dispose();
    }

    public static long getTimeMillis() => (DateTime.Now.Ticks - 621355968000000000L) / 10000L;

    internal delegate void TimerCallback(object state);

    internal sealed class TTimer : CancellationTokenSource, IDisposable
    {
      internal TTimer(
        TimerSupport.TimerCallback callback,
        object state,
        int millisecondsDueTime,
        int period)
      {
        Task.Delay(millisecondsDueTime, this.Token).ContinueWith((Action<Task, object>) ((t, s) =>
        {
          Tuple<TimerSupport.TimerCallback, object> tuple = (Tuple<TimerSupport.TimerCallback, object>) s;
          tuple.Item1(tuple.Item2);
        }), (object) Tuple.Create<TimerSupport.TimerCallback, object>(callback, state), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
      }

      public new void Dispose() => this.Cancel();
    }

    private class TimerContext
    {
      public TimerSupport.TTimer timer;
      public IThreadRunnable task;
    }
  }
}
