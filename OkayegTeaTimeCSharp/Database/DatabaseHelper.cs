﻿using OkayegTeaTimeCSharp.Database.Models;
using Sterbehilfe.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OkayegTeaTimeCSharp.Database
{
    public static class DatabaseHelper
    {
        public static void AddUser(this OkayegTeaTimeContext database, string username)
        {
            database.Users.Add(new User(username));
            database.SaveChanges();
        }

        public static int CountChannelMessages(this OkayegTeaTimeContext database, string givenChannel)
        {
            return database.Messages.Where(m => m.Channel == givenChannel).Count();
        }

        public static int CountMessages(this OkayegTeaTimeContext database)
        {
            return database.Messages.Count();
        }

        public static int CountUserMessages(this OkayegTeaTimeContext databse, string givenUsername)
        {
            return databse.Messages.Where(m => m.Username == givenUsername).Count();
        }

        public static void RemoveReminder(this OkayegTeaTimeContext database, List<Reminder> listReminder)
        {
            listReminder.ForEach(reminder =>
            {
                database.Reminders.Remove(reminder);
            });
            database.SaveChanges();
        }

        public static void SetAfk(this OkayegTeaTimeContext database, string username, string state)
        {
            if (state.IsMatch(@"^(t(rue)?)|(f(alse)?)$"))
            {
                database.Users.Where(u => u.Username == username).FirstOrDefault().IsAfk = state.IsMatch(@"^t(rue)?$") ? "true" : "false";
                database.SaveChanges();
            }
            else
            {
                throw new Exception("state doesn't match the required pattern");
            }
        }
    }
}