using System;
using System.Collections.Generic;
using System.Text;
using Discord.Net;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using System.Linq;

namespace AequinoctiumBot
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly string[] _roleNames;

        public RequireRoleAttribute(string[] roleNames)
        {
            _roleNames = roleNames;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            var guildUser = context.User as IGuildUser;
            if (guildUser == null)
                return PreconditionResult.FromError("This command cannot be executed outside of a guild.");

            var guild = guildUser.Guild;
            if (guild.Roles.All(r => _roleNames.Contains(r.Name)))
                return PreconditionResult.FromError(
                    $"The guild does not have the role required to access this command. Please contact an administrator.");

            return guildUser.RoleIds.Any(rId => _roleNames.Contains(guild.GetRole(rId).Name))
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("You do not have the sufficient role required to access this command.");
        }
    }
}
