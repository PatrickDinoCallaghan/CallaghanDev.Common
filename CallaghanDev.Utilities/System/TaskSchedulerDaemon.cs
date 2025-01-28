using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Apps.Trade.DataCapture
{
    internal class TaskSchedulerDaemon
    {
        private System.Threading.Timer _timer;

        public enum ScheduleType
        {
            Daily,
            Weekly,
            Monthly
        }

        public void Schedule(ScheduleType scheduleType, int hour, int minute, Func<Task> task)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = CalculateNextRunTime(scheduleType, now, hour, minute);

            TimeSpan timeToGo = firstRun - now;

            _timer = new Timer(async _ =>
            {
                await task();
                DateTime nextRun = CalculateNextRunTime(scheduleType, DateTime.Now, hour, minute);
                TimeSpan nextRunInterval = nextRun - DateTime.Now;

                _timer?.Change(nextRunInterval, Timeout.InfiniteTimeSpan);
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private DateTime CalculateNextRunTime(ScheduleType scheduleType, DateTime now, int hour, int minute)
        {
            switch (scheduleType)
            {
                case ScheduleType.Daily:
                    return CalculateNextDailyRun(now, hour, minute);

                case ScheduleType.Weekly:
                    return CalculateNextWeeklyRun(now, hour, minute);

                case ScheduleType.Monthly:
                    return CalculateNextMonthlyRun(now, hour, minute);

                default:
                    throw new ArgumentException("Unsupported schedule type");
            }
        }

        private DateTime CalculateNextDailyRun(DateTime now, int hour, int minute)
        {
            DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            return nextRun;
        }

        private DateTime CalculateNextWeeklyRun(DateTime now, int hour, int minute)
        {
            DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            // Calculate the next Monday
            int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            nextRun = nextRun.AddDays(daysUntilNextMonday);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(7); // Schedule for the next Monday
            }

            return nextRun;
        }

        private DateTime CalculateNextMonthlyRun(DateTime now, int hour, int minute)
        {
            DateTime nextRun = new DateTime(now.Year, now.Month, 1, hour, minute, 0);

            if (now > nextRun)
            {
                nextRun = nextRun.AddMonths(1); // Schedule for the 1st of the next month
            }

            return nextRun;
        }
    }

}

