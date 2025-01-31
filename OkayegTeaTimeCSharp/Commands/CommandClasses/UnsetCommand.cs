﻿using OkayegTeaTimeCSharp.Twitch;
using OkayegTeaTimeCSharp.Twitch.Bot;
using OkayegTeaTimeCSharp.Utils;
using Sterbehilfe.Strings;
using TwitchLib.Client.Models;

namespace OkayegTeaTimeCSharp.Commands.CommandClasses
{
    public static class UnsetCommand
    {
        public static void Handle(TwitchBot twitchBot, ChatMessage chatMessage, string alias)
        {
            if (chatMessage.GetMessage().IsMatch(PatternCreator.Create(alias, PrefixHelper.GetPrefix(chatMessage.Channel), @"\sprefix")))
            {
                twitchBot.SendUnsetPrefix(chatMessage);
            }
            else if (chatMessage.GetMessage().IsMatch(PatternCreator.Create(alias, PrefixHelper.GetPrefix(chatMessage.Channel), @"\sreminder\s\d+")))
            {
                twitchBot.SendUnsetReminder(chatMessage);
            }
            else if (chatMessage.GetMessage().IsMatch(PatternCreator.Create(alias, PrefixHelper.GetPrefix(chatMessage.Channel), @"\semote")))
            {
                twitchBot.SendUnsetEmoteInFront(chatMessage);
            }
        }
    }
}