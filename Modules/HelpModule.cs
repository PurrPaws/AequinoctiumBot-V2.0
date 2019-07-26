using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
namespace AequinoctiumBot
{
    public class HelpModule : ModuleBase<CommandContext>
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public HelpModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }


        [Command("listAllHelp",RunMode = RunMode.Async)]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        [Summary("ListsAllCommands[Admin Only]")]
        public async Task ListAllHelp()
        {
            await Context.Channel.SendMessageAsync("**All Modules:**");
            EmbedBuilder AllModules = new EmbedBuilder();

            AllModules.Title = Program.botName + " - Help";

            foreach (var mod in _commands.Modules.Where(m => m.Parent == null && !m.Name.Contains("Admin")))
            {
                AddHelp(mod, ref AllModules);
            }

            AllModules.Footer = new EmbedFooterBuilder
            {
                Text = "Use 'help <module>' to get help with a module."
            };
            await Context.Channel.SendMessageAsync("", embed: AllModules.Build());
            await Context.Channel.SendMessageAsync("--------------------------------");

            foreach (var mod in _commands.Modules.Where(m => m.Parent == null && !m.Name.Contains("Admin")))
            {
                await Context.Channel.SendMessageAsync($"**{mod.Name}:**");
                string prefixstring = mod.Group != "" ? (Program.prefixString + mod.Group) : "N/A";
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = Program.botName + " - Help - " + mod.Name,
                    Description = "Prefix: `" + prefixstring + "`"
                };

                foreach (var command in mod.Commands)
                {
                    string ParameterString = "";
                    foreach (var parameter in command.Parameters)
                    {
                        ParameterString += parameter.Summary + " ";
                    }
                    embed.AddField(new EmbedFieldBuilder() { Name = "● Command: `" + command.Name + " " + ParameterString + "`", Value = command.Summary, IsInline = false });
                }
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                await Context.Channel.SendMessageAsync("--------------------------------");
            }
        }

        [Command("helpAdmin")]
        [RequireRole(new string[] { "King", "Commander"})]
        [Summary("Lists this bot's Admin commands.")]
        public async Task HelpAdmin([Summary("Module")]string ModuleText = "")
        {
            if (ModuleText != "")
            {
                var Module = _commands.Modules.FirstOrDefault(x => x.Name.Contains("Admin") && x.Name.Contains(ModuleText));
                if (Module != null)
                {
                    string prefixstring = Module.Group != "" ? (Program.prefixString + Module.Group) : "N/A";
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Title = Program.botName + " - Help - " + Module.Name,
                        Description = "Prefix: `" + prefixstring + "`"
                    };

                    foreach (var command in Module.Commands)
                    {
                        string ParameterString = "";
                        foreach (var parameter in command.Parameters)
                        {
                            ParameterString += parameter.Summary + " ";
                        }
                        embed.AddField(new EmbedFieldBuilder() { Name = "● Command: `" + command.Name + " " + ParameterString + "`", Value = command.Summary, IsInline = false });
                    }

                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                    return;
                }
                else { return; }
            }
            EmbedBuilder output = new EmbedBuilder();

            output.Title = Program.botName + " - Help";

            foreach (var mod in _commands.Modules.Where(m => m.Parent == null && m.Name.Contains("Admin")))
            {
                AddHelp(mod, ref output);
            }

            output.Footer = new EmbedFooterBuilder
            {
                Text = "Use 'help MODULENAME' to get help with a module."
            };
            await ReplyAsync("", embed: output.Build());
        }

        [Command("help")]
        [Summary("Lists this bot's commands.")]
        public async Task Help([Summary("Module")]string ModuleText = "")
        {
            if (ModuleText != "")
            {
                var Module = _commands.Modules.FirstOrDefault(x => x.Name.ToLower().Contains(ModuleText.ToLower()) && !x.Name.Contains("Admin"));
                if (Module != null)
                {
                    string prefixstring = Module.Group != "" ? (Program.prefixString + Module.Group) : "N/A";
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Title = Program.botName + " - Help - " + Module.Name,
                        Description = "Prefix: `"+ prefixstring +"`" 
                    };

                    foreach (var command in Module.Commands)
                    {
                        string ParameterString = "";
                        foreach (var parameter in command.Parameters)
                        {
                            ParameterString += parameter.Summary +" ";
                        }
                        embed.AddField(new EmbedFieldBuilder() { Name = "● Command: `" + command.Name+ " " + ParameterString +"`", Value = command.Summary, IsInline = false });
                    }
                    
                    await Context.Channel.SendMessageAsync("",false,embed.Build());
                    return;
                }
            }
            EmbedBuilder output = new EmbedBuilder();

            output.Title = Program.botName + " - Help";

            foreach (var mod in _commands.Modules.Where(m => m.Parent == null && !m.Name.Contains("Admin")))
            {
                AddHelp(mod, ref output);
            }

            output.Footer = new EmbedFooterBuilder
            {
                Text = "Use 'help MODULENAME' to get help with a module."
            };
            await ReplyAsync("", embed: output.Build());
        }

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);
            builder.AddField(f =>
            {
                f.Name = $"**{module.Name}**";
                f.Value = $"Commands: {string.Join(", ", module.Commands.Select(x => $"`{x.Name}`"))}";
            });
        }

        public void AddCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in module.Commands)
            {
                command.CheckPreconditionsAsync(Context, _map).GetAwaiter().GetResult();
                AddCommand(command, ref builder);
            }

        }

        public void AddCommand(CommandInfo command, ref EmbedBuilder builder)
        {
            builder.AddField(f =>
            {
                f.Name = $"**{command.Name}**";
                f.Value = $"{command.Summary}\n" +
                (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})\n" : "") +
                (command.Aliases.Any() ? $"**Aliases:** {string.Join(", ", command.Aliases.Select(x => $"`{x}`"))}\n" : "") +
                $"**Usage:** `{GetPrefix(command)} {GetAliases(command)}`";
            });
        }

        public string GetAliases(CommandInfo command)
        {
            StringBuilder output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var param in command.Parameters)
            {
                if (param.IsOptional)
                    output.Append($"[{param.Name} = {param.DefaultValue}] ");
                else if (param.IsMultiple)
                    output.Append($"|{param.Name}| ");
                else if (param.IsRemainder)
                    output.Append($"...{param.Name} ");
                else
                    output.Append($"<{param.Name}> ");
            }
            return output.ToString();
        }
        public string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += $"{command.Aliases.FirstOrDefault()} ";
            return output;
        }
        public string GetPrefix(ModuleInfo module)
        {
            string output = "";
            if (module.Parent != null) output = $"{GetPrefix(module.Parent)}{output}";
            if (module.Aliases.Any())
                output += string.Concat(module.Aliases.FirstOrDefault(), " ");
            return output;
        }
    }
}
