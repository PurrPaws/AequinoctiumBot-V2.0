namespace AequinoctiumBot
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    public class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _commandService;
        IServiceProvider _serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider)
        {
            _client = client;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _client.MessageReceived += HandleCommand;

            _commandService.AddModuleAsync<GeneralModule>(serviceProvider);
            _commandService.AddModuleAsync<AdminGeneralModule>(serviceProvider);
            _commandService.AddModuleAsync<ProfileModule>(serviceProvider);
            _commandService.AddModuleAsync<AdminProfileModule>(serviceProvider);
            _commandService.AddModuleAsync<HelpModule>(serviceProvider);
        }

        public async Task HandleCommand(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            var context = new CommandContext(_client, msg);
            int argPos = 0;
            if (msg.HasStringPrefix(Program.prefixString, ref argPos))
            {
                var result = await _commandService.ExecuteAsync(context: context, argPos: argPos, services: _serviceProvider);

                if (!result.IsSuccess)
                {
                    switch (result.ToString())
                    {
                        default:

                            await s.Channel.SendMessageAsync($"An error occurred! Details: ```" + result.Error + "    ||     " + result.ErrorReason + "```");
                            break;
                        case "UnknownCommand: Unknown command.":
                            await s.Channel.SendMessageAsync($"Command not found. {Program.prefixString}help for a list of commands.");
                            break;
                    }
                }
            }
        }
    }
}