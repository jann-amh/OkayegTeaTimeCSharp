﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkayegTeaTimeCSharp.Commands.CommandEnums;
using OkayegTeaTimeCSharp.Twitch.Bot;
using TwitchLib.Client.Models;

namespace OkayegTeaTimeCSharp.Commands.CommandClasses
{
    public static class CookieCommand
    {
        public const CommandType Type = CommandType.Cookie;

        public static void Handle(TwitchBot twitchBot, ChatMessage chatMessage, string alias)
        {
            twitchBot.SendRandomCookie(chatMessage);
        }
    }
}
