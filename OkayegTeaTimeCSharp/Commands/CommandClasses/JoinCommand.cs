﻿using OkayegTeaTimeCSharp.Properties;
using OkayegTeaTimeCSharp.Twitch;
using OkayegTeaTimeCSharp.Twitch.Bot;
using OkayegTeaTimeCSharp.Utils;
using Sterbehilfe.Strings;
using TwitchLib.Client.Models;

namespace OkayegTeaTimeCSharp.Commands.CommandClasses
{
    public static class JoinCommand
    {
        public static void Handle(TwitchBot twitchBot, ChatMessage chatMessage, string alias)
        {
            if (chatMessage.GetMessage().IsMatch(PatternCreator.Create(alias, PrefixHelper.GetPrefix(chatMessage.Channel), @"\s#?\w+")))
            {
                if (chatMessage.Username == Resources.Owner)
                {
                    twitchBot.JoinChannel(chatMessage.GetLowerSplit()[1]);
                }
            }
        }
    }
}