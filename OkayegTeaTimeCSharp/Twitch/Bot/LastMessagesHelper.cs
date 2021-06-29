﻿using OkayegTeaTimeCSharp.Utils;
using System.Collections.Generic;

namespace OkayegTeaTimeCSharp.Twitch.Bot
{
    public static class LastMessagesHelper
    {
        public static Dictionary<string, string> FillDictionary()
        {
            Dictionary<string, string> dic = new();
            Config.GetChannels().ForEach(channel =>
            {
                dic.Add($"#{channel}", "");
            });
            return dic;
        }

        public static string GetLastMessage(string channel, string message)
        {
            if (TwitchBot.LastMessages.TryGetValue($"#{channel.ReplaceHashtag()}", out string lastMessage))
            {
                return lastMessage;
            }
            else
            {
                AddChannel(channel, message);
                return string.Empty;
            }
        }

        public static void AddChannel(string channel, string message)
        {
            TwitchBot.LastMessages.Add($"#{channel.ReplaceHashtag()}", message);
        }
    }
}
