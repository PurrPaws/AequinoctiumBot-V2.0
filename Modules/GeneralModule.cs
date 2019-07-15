using Discord.Commands;
using Discord;
using Discord.Commands.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AequinoctiumBot
{
    public class AdminGeneralModule : ModuleBase<CommandContext>
    {
        [Command("say")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        [Summary("Let the bot say something.\n\u200B")]
        public async Task Say([Summary("String to Say")] string WhatToSay)
        {
            await Context.Channel.SendMessageAsync(WhatToSay);
            await Context.Message.DeleteAsync();
        }

        [Command("dm")]
        [Summary("Direct Message a user.\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task DirectMessage([Summary("UserToDM")] IUser User, [Summary("String to Say")] string WhatToSay)
        {
            await User.SendMessageAsync(WhatToSay);
            await (Context.Client.GetUserAsync(243450437389385728) as IGuildUser).SendMessageAsync("UserSent Message \nUser: " + Context.User.Username + "To: " + User.Username + "\n Message: " + WhatToSay);
            await Context.Message.DeleteAsync();
        }

        [Command("purge", RunMode = RunMode.Async)]
        [Summary("Removes the last specified amount of messages.\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task PurgeMessages([Summary("Amount Of Messages")] int amount)
        {
            var messages = Context.Channel.GetMessagesAsync(amount).FlattenAsync();
            foreach (IMessage message in messages.Result) { if (message == null) { break; } await message.DeleteAsync(); }
        }

        [Command("giveAway", RunMode = RunMode.Async)]
        [Summary("Selects a random user that is within the General voice channel.\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task GiveAway()
        {
            var users = await Context.Guild.GetUsersAsync();
            List<IGuildUser> possibleWinners = new List<IGuildUser>();
            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Any(x => Context.Guild.Roles.Any(y => y.Id == x)) && user.Status != UserStatus.Offline)
                {
                    possibleWinners.Add(user);
                }
            }
            Random rand = new Random();
            var selectedUser = possibleWinners[rand.Next(0, possibleWinners.Capacity)];

            await Context.Channel.SendMessageAsync($"The winner of the giveaway is " + selectedUser.Mention + "! Congratulations!!");
        }
    }
    public class GeneralModule : ModuleBase<CommandContext>
    {
        [Command("inviteLink")]
        [Summary("Links the invite link in the channel you use this command in.\n\u200B")]
        public async Task InviteLink()
        {
            await ReplyAsync("Server invite link: https://discord.gg/89Cx8aV");
            await Context.Message.DeleteAsync();
        }

        [Command("link")]
        [Summary("create a link, but embed it in style! \nexample: `aq link \"This is Google.com!\" https:://www.google.com \"This is the description for the embed of google.com!\" `\n\u200B")]
        public async Task Link([Summary("\"Title Of Link\"")] string Title, [Summary("Link_Adress")] string linkaddr, [Summary("[Optional:]\"Description Of Link\"")] string Description = null)
        {
            var embed = new EmbedBuilder
            {
                Title = Title,
                Description = Description,
                ThumbnailUrl = linkaddr,
                Url = linkaddr,
            };
            embed.Footer = new EmbedFooterBuilder { Text = "Made by: " + Context.User.Username };
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            await Context.Message.DeleteAsync();
        }

        [Command("poke")]
        [Summary("Poke the specified user.\n\u200B")]
        public async Task Say([Summary("@USER")] IUser user)
        {
            await user.SendMessageAsync("Hey! You have been poked by: " + Context.User.Username + "!!");
            await Context.Message.DeleteAsync();
        }

        [Command("coinflip")]
        [Summary("Flips a coin, 50|50 Heads|tails!\n\u200B")]
        public async Task FlipCoin()
        {
            Random rand = new Random();
            switch(rand.Next(0, 2))
            {
                case 0:
                    await Context.Channel.SendMessageAsync("***Coinflip:*** Heads!");
                    break;
                case 1:
                    await Context.Channel.SendMessageAsync("***Coinflip:*** Tails!");
                    break;
            }
        }
    }
}