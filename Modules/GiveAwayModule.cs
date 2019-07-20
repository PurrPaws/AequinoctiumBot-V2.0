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
    [Group("giveaway")]
    public class AdminGiveAwayModule : ModuleBase<CommandContext>
    {
        [Command("create")]
        [RequireRole(new string[] { "King", "Commander" })]
        [Summary("Create a giveaway\n\u200B")]
        public async Task CreateGiveAway([Summary("GiveAway Item")] string itemString, [Summary("endsInDays")] int endsInDays, [Summary("entryCost")] float entryCost = 10f)
        {
            GiveAwayService.CreateGiveAway(itemString,entryCost,endsInDays);
        }
        [Command("setStatus")]
        [RequireRole(new string[] { "King", "Commander" })]
        [Summary("Open a giveaway\n\u200B")]
        public async Task SetGiveawayStatus([Summary("GiveAwayID")] int giveAwayID, string status)
        {
            GiveAwayService.SetGiveawayStatus(giveAwayID,status);
        }
    }
    [Group("giveaway")]
    public class GiveAwayModule : ModuleBase<CommandContext>
    {
        [Command("buyTickets")]
        [Summary("Enter a giveaway by buying tickets!\nexample: `aq buyTickets 1 5`\nThe above will buy 5 tickets for the giveaway with the ID 1\n\u200B")]
        public async Task BuyTickets([Summary("GiveawayID")] int giveAwayID,[Summary("Amount")] int amountToBuy)
        {
            GiveAwayService.BuyTickets(Context,giveAwayID, amountToBuy);
        }
    }
}
