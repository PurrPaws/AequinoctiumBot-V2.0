using System;
using System.Collections.Generic;
using System.Text;

namespace AequinoctiumBot
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    public class EventHandler
    {
        DiscordSocketClient _client;
        IServiceProvider _serviceProvider;
        Timer midnightTimer;

        public EventHandler(DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _client.Log += Client_Log;
            _client.Ready += Client_Ready;
            _client.UserJoined += Client_UserJoined;
            _client.UserLeft += Client_UserLeft;
            _client.UserBanned += Client_UserBanned;
            _client.MessageReceived += Client_MessageReceived;
            _client.UserVoiceStateUpdated += OnVoiceStateUpdated;
            
            //MidnightTimer
            midnightTimer = new Timer((DateTime.Today.AddDays(1) - DateTime.Now).TotalMilliseconds);
            midnightTimer.AutoReset = false;
            midnightTimer.Elapsed += On_MidnightTimer;
            midnightTimer.Start();

            //VoiceRewardTimer
            Timer VoiceRewardTimer = new Timer(300000);
            VoiceRewardTimer.AutoReset = true;
            VoiceRewardTimer.Elapsed += OnVoiceRewardTimer_Elapse;
            VoiceRewardTimer.Start();
        }

        private void OnVoiceRewardTimer_Elapse(object sender, ElapsedEventArgs e)
        {
            Program.LogConsole("VoiceRewardTimer", ConsoleColor.Magenta, $"Elapsed -- {DateTime.Now}");
            foreach (IGuildUser user in Program.guild.Users)
            {
                if (user.VoiceChannel != null)
                {
                    UserDataService.GrantExp(2, user);
                    UserDataService.GrantDrak(0.5f,user);
                }
            }
        }

        private Task OnVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (!UserDataService.HasGottenFirstConnectionToVoiceOfDay(arg1) && arg3.VoiceChannel != null)
            {
                UserDataService.GrantExp(20, arg1, false, true);
            }
            return Task.CompletedTask;
        }

        private void On_MidnightTimer(object sender, ElapsedEventArgs e)
        {
            UserDataService.On_MidnightTimer();
            midnightTimer = new Timer((DateTime.Today.AddDays(1) - DateTime.Now).TotalMilliseconds);
            midnightTimer.AutoReset = false;
            midnightTimer.Elapsed += On_MidnightTimer;
        }

        private Task Client_Log(LogMessage arg)
        {
            Program.LogConsole("CLIENT LOG", ConsoleColor.Red, arg.Message);
            if (arg.Exception != null) { Console.WriteLine(arg.Exception); }
            return Task.CompletedTask;
        }

        public Task Client_Ready()
        {
            var wowService = _serviceProvider.GetRequiredService<WoWService>();
            var  userDataService = _serviceProvider.GetRequiredService<UserDataService>();
            userDataService.LoadData();
            userDataService.InitializeRanks(_client);
            wowService.Initialize();
            return Task.CompletedTask;
        }

        private Task Client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            Program.LogConsole("ADMINISTRATION", ConsoleColor.Red, $"User {arg1.Username} has been banned.");

            UserDataService.On_UserLeft(arg1 as SocketGuildUser);

            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Channel as IDMChannel != null && arg.Author.IsBot == false)
            {
                Program.LogConsole("MESSAGELOG", ConsoleColor.Yellow, "Messaged by user: " + arg.Author.Username + " \n" +
                "Message: \n" + arg.Content);
            }
            else
            {
                if (!UserDataService.HasGottenFirstMessageOfTheDay(arg.Author))
                {
                    UserDataService.GrantExp(15, arg.Author,true);
                }
            }
            return Task.CompletedTask;
        }

        public Task Client_UserJoined(SocketGuildUser user)
        {
            if (user.Nickname != "")
                Program.LogConsole("JOINLEAVEHANDLE", ConsoleColor.Cyan, $"User: {user.Nickname} has joined the Server.");
            else
                Program.LogConsole("JOINLEAVEHANDLE", ConsoleColor.Cyan, $"User: {user.Username} has joined the Server.");

            UserDataService.On_UserJoined(user);

            return Task.CompletedTask;
        }

        public Task Client_UserLeft(SocketGuildUser user)
        {
            if (user.Nickname != "")
                Program.LogConsole("JOINLEAVEHANDLE", ConsoleColor.Cyan, $"User: {user.Nickname} has left the Server.");
            else
                Program.LogConsole("JOINLEAVEHANDLE", ConsoleColor.Cyan, $"User: {user.Username} has left the Server.");

            UserDataService.On_UserLeft(user);

            return Task.CompletedTask;
        }
    }
}
