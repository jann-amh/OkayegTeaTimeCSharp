using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkayegTeaTimeCSharp.Commands.CommandEnums;
using OkayegTeaTimeCSharp.Twitch;
using OkayegTeaTimeCSharp.Twitch.Bot;
using TwitchLib.Client.Models;
using TwitchLib.Client.Extensions;


namespace OkayegTeaTimeCSharp.Commands.CommandClasses
{
    public static class VanishCommand
    {
        public static void Handle(TwitchBot twitchBot, ChatMessage chatMessage, string alias)
        {
            twitchBot.TwitchClient.TimeoutUser(chatMessage.Channel, chatMessage.Username, TimeSpan.FromSeconds(1));
        }
    }
}



