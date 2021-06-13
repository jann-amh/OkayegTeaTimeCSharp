using OkayegTeaTimeCSharp.HttpRequests;
using OkayegTeaTimeCSharp.Twitch;
using OkayegTeaTimeCSharp.Twitch.Bot;
using OkayegTeaTimeCSharp.Utils;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client.Models;

namespace OkayegTeaTimeCSharp.Commands.CommandClasses
{
    public static class ChatNeighbourCommand
    {
        public static void Handle(TwitchBot twitchBot, ChatMessage chatMessage, string alias)
        {
            List<string> chatters = HttpRequest.GetChatters(chatMessage.Channel).OrderByDescending(chatters => chatters).ToList();
            string chatter1 = chatters[chatters.IndexOf(chatMessage.Username) - 1];
            string chatter2 = chatters[chatters.IndexOf(chatMessage.Username) + 1];
            twitchBot.Send(chatMessage.Channel, "Your chatneighbours are " + chatter1 + " and " + chatter2);
        }
    }

}
