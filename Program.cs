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

            await Task.Delay(-1); //Keep this task from ever shutting down.
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
}
