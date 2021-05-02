﻿using OkayegTeaTimeCSharp.Properties;
using OkayegTeaTimeCSharp.Twitch.Bot;
using OkayegTeaTimeCSharp.Utils;
using TwitchLib.Client.Models;
using OkayegTeaTimeCSharp.Commands.CommandEnums;

namespace OkayegTeaTimeCSharp.Commands.CommandClasses
{
    public class HelpCommand
    {
        public const CommandType Type = CommandType.Help; 

        public static void Handle(TwitchBot twitchBot, ChatMessage chatMessage, string alias)
        {
            twitchBot.Send(chatMessage.Channel, $"{Emoji.PointRight} {chatMessage.Username}, here you can find a list of commands and the repository: {Resources.GitHubRepoLink}");
        }
    }
}
