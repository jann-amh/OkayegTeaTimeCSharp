﻿using Microsoft.EntityFrameworkCore;
using OkayegTeaTimeCSharp.Commands.CommandEnums;
using OkayegTeaTimeCSharp.Database.Models;
using OkayegTeaTimeCSharp.Exceptions;
using OkayegTeaTimeCSharp.Messages;
using OkayegTeaTimeCSharp.Properties;
using OkayegTeaTimeCSharp.Twitch;
using OkayegTeaTimeCSharp.Twitch.Bot;
using OkayegTeaTimeCSharp.Utils;
using Sterbehilfe.Strings;
using Sterbehilfe.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client.Models;

namespace OkayegTeaTimeCSharp.Database
{
    public static class DataBase
    {
        public static void AddChannel(string channel)
        {
            OkayegTeaTimeContext database = new();
            database.Bots.Where(b => b.Id == 1).FirstOrDefault().Channels += $" {channel.ReplaceHashtag().Trim()}";
            database.SaveChanges();
        }

        public static void AddNewToken(string username, string accessToken, string refreshToken)
        {
            OkayegTeaTimeContext database = new();
            if (database.Spotify.Any(s => s.Username == username))
            {
                Models.Spotify user = database.Spotify.Where(s => s.Username == username).FirstOrDefault();
                user.AccessToken = accessToken;
                user.RefreshToken = refreshToken;
                user.Time = TimeHelper.Now();
            }
            else
            {
                Models.Spotify user = new(username, accessToken, refreshToken);
                database.Spotify.Add(user);
                database.SaveChanges();
            }
        }

        public static void AddNuke(Nuke nuke)
        {
            OkayegTeaTimeContext database = new();
            database.Nukes.Add(nuke);
            database.SaveChanges();
        }

        public static int AddReminder(Reminder reminder)
        {
            try
            {
                OkayegTeaTimeContext database = new();
                if (reminder.ToTime == 0)
                {
                    if (database.Reminders.Where(r => r.ToUser == reminder.ToUser && r.ToTime == 0).Count() >= Config.MaximumReminders)
                    {
                        throw new TooManyReminderException();
                    }
                }
                else
                {
                    if (database.Reminders.Where(r => r.ToUser == reminder.ToUser && r.ToTime != 0).Count() >= Config.MaximumReminders)
                    {
                        throw new TooManyReminderException();
                    }
                }
                database.Reminders.Add(reminder);
                database.SaveChanges();
                return database.Reminders.Where(r => r.FromUser == reminder.FromUser && r.ToUser == reminder.ToUser && r.Message == reminder.Message && r.ToTime == reminder.ToTime && r.Time == reminder.Time).FirstOrDefault().Id;
            }
            catch (TooManyReminderException)
            {
                throw;
            }
        }

        public static void AddSugestion(ChatMessage chatMessage, string suggestion)
        {
            OkayegTeaTimeContext database = new();
            database.Suggestions.Add(new(chatMessage.Username, suggestion.MakeInsertable(), $"#{chatMessage.Channel}"));
            database.SaveChanges();
        }

        public static void CheckForNukes(TwitchBot twitchBot, ChatMessage chatMessage)
        {
            if (!MessageHelper.IsAnyCommand(chatMessage))
            {
                OkayegTeaTimeContext database = new();
                if (database.Nukes.Any(n => n.Channel == $"#{chatMessage.Channel}"))
                {
                    database.Nukes.Where(n => n.Channel == $"#{chatMessage.Channel}").ToList().ForEach(n =>
                    {
                        if (n.ForTime > TimeHelper.Now())
                        {
                            if (!chatMessage.IsModOrBroadcaster())
                            {
                                if (chatMessage.GetMessage().IsMatch(n.Word.Decode()))
                                {
                                    twitchBot.Timeout(chatMessage.Channel, chatMessage.Username, n.TimeoutTime, Nuke.Reason);
                                }
                            }
                        }
                        else
                        {
                            database.Nukes.Remove(n);
                            database.SaveChanges();
                        }
                    });
                }
            }
        }

        public static void CheckForReminder(TwitchBot twitchBot, ChatMessage chatMessage)
        {
            OkayegTeaTimeContext database = new();
            if (database.Reminders.Any(reminder => reminder.ToTime == 0 && reminder.ToUser == chatMessage.Username))
            {
                List<Reminder> listReminder = database.Reminders.Where(reminder => reminder.ToTime == 0 && reminder.ToUser == chatMessage.Username).ToList();
                twitchBot.SendReminder(chatMessage, listReminder);
                database.RemoveReminder(listReminder);
            }
        }

        public static void CheckForTimedReminder(TwitchBot twitchBot)
        {
            OkayegTeaTimeContext database = new();
            if (database.Reminders.Any(reminder => reminder.ToTime != 0))
            {
                List<Reminder> listReminder = database.Reminders.Where(reminder => reminder.ToTime != 0).ToList();
                listReminder.ForEach(reminder =>
                {
                    if (reminder.ToTime <= TimeHelper.Now())
                    {
                        twitchBot.SendTimedReminder(reminder);
                        database.Reminders.Remove(reminder);
                    }
                });
                database.SaveChanges();
            }
        }

        public static void CheckIfAFK(TwitchBot twitchBot, ChatMessage chatMessage)
        {
            OkayegTeaTimeContext database = new();
            User user = database.Users.Where(user => user.Username == chatMessage.Username).FirstOrDefault();
            if (user.IsAfk == "true")
            {
                twitchBot.SendComingBack(user, chatMessage);
                if (!MessageHelper.IsAfkCommand(chatMessage))
                {
                    database.SetAfk(chatMessage.Username, "false");
                }
            }
        }

        public static List<string> GetChannels()
        {
            return new OkayegTeaTimeContext().Bots.Where(b => b.Username == Resources.Username).FirstOrDefault().Channels.Split().ToList();
        }

        public static Dictionary<string, string> GetEmotesInFront()
        {
            return new OkayegTeaTimeContext().EmoteInFronts.ToDictionary(e => e.Channel, e => e.Emote?.Decode());
        }

        public static Message GetFirst(ChatMessage chatMessage)
        {
            try
            {
                OkayegTeaTimeContext database = new();
                return database.Messages.Where(m => m.Username == chatMessage.Username).FirstOrDefault();
            }
            catch (Exception)
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetFirstChannel(ChatMessage chatMessage, string channel)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.Where(m => m.Username == chatMessage.Username && m.Channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetFirstMessageUserChannel(string username, string channel)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.Where(m => m.Username == username && channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetFirstUser(string username)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.Where(m => m.Username == username).FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetLastMessage(string username)
        {

            OkayegTeaTimeContext database = new();
            Message message = database.Messages.Where(m => m.Username == username).OrderByDescending(m => m.Id).FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new UserNotFoundException();
            }
        }

        public static string GetPrefix(string channel)
        {
            return new OkayegTeaTimeContext().Prefixes.Where(p => p.Channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault().PrefixString?.Decode();
        }

        public static Dictionary<string, string> GetPrefixes()
        {
            return new OkayegTeaTimeContext().Prefixes.ToDictionary(p => p.Channel, p => p.PrefixString?.Decode());
        }
        public static Pechkekse GetRandomCookie()
        {
            OkayegTeaTimeContext database = new();
            return database.Pechkekse.FromSqlRaw($"SELECT * FROM pechkekse ORDER BY RAND() LIMIT 1").FirstOrDefault();
        }

        public static Gachi GetRandomGachi()
        {
            OkayegTeaTimeContext database = new();
            return database.Gachi.FromSqlRaw($"SELECT * FROM gachi ORDER BY RAND() LIMIT 1").FirstOrDefault();
        }

        public static Message GetRandomMessage(ChatMessage chatMessage)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE channel = '#{chatMessage.Channel}' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetRandomMessage(string username)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE username = '{username.MakeQueryable()}' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetRandomMessage(string username, string channel)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE username ='{username.MakeQueryable()}' AND channel = '#{channel.MakeQueryable()}' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Yourmom GetRandomYourmom()
        {
            OkayegTeaTimeContext database = new();
            return database.Yourmom.FromSqlRaw($"SELECT * FROM yourmom ORDER BY RAND() LIMIT 1").FirstOrDefault();
        }

        public static string GetRefreshToken(string username)
        {
            return new OkayegTeaTimeContext().Spotify.Where(s => s.Username == username).FirstOrDefault().RefreshToken;
        }

        public static Message GetSearch(string keyword)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE CONVERT(MessageText USING latin1) LIKE '%{keyword.MakeQueryable()}%' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetSearchChannel(string keyword, string channel)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE CONVERT(MessageText USING latin1) LIKE '%{keyword.MakeQueryable()}%' AND Channel = '#{channel.ReplaceHashtag().MakeQueryable().ToLower()}' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetSearchUser(string keyword, string username)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE CONVERT(MessageText USING latin1) LIKE '%{keyword.MakeQueryable()}%' AND Username = '{username.MakeQueryable().ToLower()}' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Message GetSearchUserChannel(string keyword, string username, string channel)
        {
            OkayegTeaTimeContext database = new();
            Message message = database.Messages.FromSqlRaw($"SELECT * FROM messages WHERE CONVERT(MessageText USING latin1) LIKE '%{keyword.MakeQueryable()}%' AND Username = '{username.MakeQueryable().ToLower()}' AND Channel = '#{channel.ReplaceHashtag().MakeQueryable().ToLower()}' ORDER BY RAND() LIMIT 1").FirstOrDefault();
            if (message != null)
            {
                return message;
            }
            else
            {
                throw new MessageNotFoundException();
            }
        }

        public static Models.Spotify GetSpotifyUser(string username)
        {
            return new OkayegTeaTimeContext().Spotify.Where(s => s.Username == username).FirstOrDefault();
        }

        public static User GetUser(string username)
        {
            OkayegTeaTimeContext database = new();
            User user = database.Users.Where(u => u.Username == username).FirstOrDefault();
            if (user != null)
            {
                return user;
            }
            else
            {
                throw new UserNotFoundException();
            }
        }

        public static void InsertNewUser(string username)
        {
            OkayegTeaTimeContext database = new();
            if (!database.Users.Any(u => u.Username == username))
            {
                database.AddUser(username);
            }
        }

        public static void LogMessage(ChatMessage chatMessage)
        {
            if (!Config.GetNotLoggedChannels().Contains(chatMessage.Channel))
            {
                OkayegTeaTimeContext database = new();
                database.Messages.Add(new Message(chatMessage.Username, chatMessage.Message.MakeInsertable(), chatMessage.Channel));
                database.SaveChanges();
            }
        }

        public static void ResumeAfkStatus(string username)
        {
            OkayegTeaTimeContext database = new();
            database.SetAfk(username, "true");
            database.SaveChanges();
        }

        public static void SetAfk(ChatMessage chatMessage, AfkCommandType type)
        {
            OkayegTeaTimeContext database = new();
            User user = database.Users.Where(u => u.Username == chatMessage.Username).FirstOrDefault();
            string message = chatMessage.GetSplit().Length > 1 ? chatMessage.GetSplit()[1..].ArrayToString() : "";
            user.MessageText = message.MakeInsertable();
            user.Type = type.ToString();
            user.Time = TimeHelper.Now();
            database.SaveChanges();
            database.SetAfk(chatMessage.Username, "true");
        }

        public static void SetEmoteInFront(string channel, string emote)
        {
            OkayegTeaTimeContext database = new();
            if (database.EmoteInFronts.Any(e => e.Channel == $"#{channel.ReplaceHashtag()}"))
            {
                database.EmoteInFronts.Where(e => e.Channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault().Emote = emote.MakeInsertable();
                database.SaveChanges();
            }
            else
            {
                database.EmoteInFronts.Add(new(channel, emote.MakeInsertable()));
                database.SaveChanges();
            }
        }

        public static void SetPrefix(string channel, string prefix)
        {
            OkayegTeaTimeContext database = new();
            if (database.Prefixes.Any(p => p.Channel == $"#{channel.ReplaceHashtag()}"))
            {
                database.Prefixes.Where(p => p.Channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault().PrefixString = prefix.MakeUsable().Encode();
                database.SaveChanges();
                PrefixHelper.Update(channel);
            }
            else
            {
                database.Prefixes.Add(new Prefix(channel, prefix.MakeUsable().Encode()));
                database.SaveChanges();
                PrefixHelper.Add(channel);
            }
        }

        public static void UnsetEmoteInFront(string channel)
        {
            OkayegTeaTimeContext database = new();
            if (database.EmoteInFronts.Any(e => e.Channel == $"#{channel.ReplaceHashtag()}"))
            {
                database.EmoteInFronts.Where(e => e.Channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault().Emote = null;
                database.SaveChanges();
            }
            else
            {
                database.EmoteInFronts.Add(new(channel, null));
                database.SaveChanges();
            }
        }
        public static void UnsetPrefix(string channel)
        {
            OkayegTeaTimeContext database = new();
            if (database.Prefixes.Any(p => p.Channel == $"#{channel.ReplaceHashtag()}"))
            {
                database.Prefixes.Where(p => p.Channel == $"#{channel.ReplaceHashtag()}").FirstOrDefault().PrefixString = null;
                database.SaveChanges();
                PrefixHelper.Update(channel);
            }
            else
            {
                database.Prefixes.Add(new Prefix(channel, null));
                database.SaveChanges();
                PrefixHelper.Add(channel);
            }
        }

        public static void UnsetReminder(ChatMessage chatMessage)
        {
            try
            {
                OkayegTeaTimeContext database = new();
                Reminder reminder = database.Reminders.Where(r => r.Id == chatMessage.GetSplit()[2].ToInt()).FirstOrDefault();
                if (reminder != null)
                {
                    if (reminder.FromUser == chatMessage.Username || (reminder.ToUser == chatMessage.Username && reminder.ToTime != 0))
                    {
                        database.Reminders.Remove(reminder);
                        database.SaveChanges();
                    }
                    else
                    {
                        throw new NoPermissionException("you have no permission to delete the reminder of someone else");
                    }
                }
                else
                {
                    throw new ReminderNotFoundException();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void UpdateAccessToken(string username, string accessToken)
        {
            OkayegTeaTimeContext database = new();
            Models.Spotify user = database.Spotify.Where(s => s.Username == username).FirstOrDefault();
            user.AccessToken = accessToken;
            user.Time = TimeHelper.Now();
            database.SaveChanges();
        }
    }
}