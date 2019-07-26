using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace AequinoctiumBot
{
    class Program
    {
        //Variables
        public static readonly string botName = "Aequinoctium Bot";
        public static readonly string prefixString = "aq ";
        public DiscordSocketClient client;
        public static SocketGuild guild;
        public CommandService commandService;
        public IServiceProvider serviceProvider;

        public CommandHandler commandHandler;
        public EventHandler eventHandler;

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            client = new DiscordSocketClient();
            commandService = new CommandService();

            IServiceCollection services = new ServiceCollection()
                .AddSingleton<WoWService>()
                .AddSingleton<UserDataService>()
                .AddSingleton<GiveAwayService>()
                .AddSingleton(commandService)
                .AddSingleton(client);
            serviceProvider = services.BuildServiceProvider();

            commandHandler = new CommandHandler(client, commandService, serviceProvider);
            eventHandler = new EventHandler(client, serviceProvider);

            string AuthToken = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "authkey.secret");
            await client.LoginAsync(TokenType.Bot, AuthToken, true);
            

            await client.StartAsync();
            await client.SetGameAsync("Aequinoctium");

            while (true)
            {
                string command = Console.ReadLine();

                switch (command.ToLower())
                {
                    case "backup":
                        UserDataService.BackupUserData();
                        GiveAwayService.BackupGiveAways();
                        break;
                    case "save":
                        UserDataService.SaveUserData();
                        GiveAwayService.SaveGiveAways();
                        break;
                    case "load":
                        UserDataService.LoadUserData();
                        GiveAwayService.LoadGiveAways();
                        break;
                    case "stop":
                        UserDataService.SaveUserData();
                        GiveAwayService.SaveGiveAways();
                        return;
                }
            }
        }
        #region HelperFunctions
        public static void LogConsole(string _prefix, ConsoleColor _prefixColor, string logText)
        {
            Console.ForegroundColor = _prefixColor;
            Console.Write(_prefix + " >> ");
            Console.ResetColor();
            Console.WriteLine(logText);
        }
        #endregion HelperFunctions
    }

    public static class ExtentionMethods
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
