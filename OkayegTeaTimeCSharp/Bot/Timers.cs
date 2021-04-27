﻿using System.Linq;
using System.Timers;

namespace OkayegTeaTimeCSharp.Bot
{
    public static class Timers
    {
        public static void InitializeTimers()
        {
            CreateTimer(1000);
        }

        private static void CreateTimer(int interval)
        {
            Timer timer = new()
            {
                Enabled = false,
                Interval = 1000,
                AutoReset = true,
            };
            TwitchBot.ListTimer.Add(timer);
        }

        public static Timer GetTimer(int interval)
        {
            return TwitchBot.ListTimer.Where(timer => timer.Interval == interval).FirstOrDefault();
        }
    }
}
