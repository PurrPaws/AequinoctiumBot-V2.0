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
    public class AdminGiveAwayModule : ModuleBase<CommandContext>
    {
        [Command("giveaway create")]
        [RequireRole(new string[] { "King", "Commander" })]
        [Summary("Create a giveaway\n\u200B")]
        public async Task CreateGiveAway([Summary("endsInDays")] int endsInDays, [Summary("entryCost")] int entryCost, params string[] rewards)
        {
            GiveAwayService.CreateGiveAway(entryCost, endsInDays, rewards);
        }
        [Command("giveaway addReward")]
        [RequireRole(new string[] { "King", "Commander" })]
        [Summary("adds a reward to the giveaway\n\u200B")]
        public async Task AddReward([Summary("ID")] int giveawayID, [Summary("Reward")] string reward)
        {
            GiveAwayService.AddReward(giveawayID, reward);
        }
        [Command("giveaway setStatus")]
        [RequireRole(new string[] { "King", "Commander" })]
        [Summary("Open a giveaway\n\u200B")]
        public async Task SetGiveawayStatus([Summary("GiveAwayID")] int giveAwayID, string status)
        {
            GiveAwayService.SetGiveawayStatus(giveAwayID, status);
        }

        [Command("giveaway end")]
        [RequireRole(new string[] { "King", "Commander" })]
        [Summary("End a giveaway\n\u200B")]
        public async Task EndGiveAway([Summary("GiveAwayID")] int giveAwayID)
        {
            GiveAwayService.EndGiveAway(giveAwayID);
        }
    }
    public class GiveAwayModule : ModuleBase<CommandContext>
    {
        [Command("giveaway buyTickets")]
        [Summary("Enter a giveaway by buying tickets!\nexample: `aq buyTickets 1 5`\nThe above will buy 5 tickets for the giveaway with the ID 1\n\u200B")]
        public async Task BuyTickets([Summary("GiveawayID")] int giveAwayID,[Summary("Amount")] int amountToBuy)
        {
            GiveAwayService.BuyTickets(Context,giveAwayID, amountToBuy);
        }
        [Command("giveaway getTickets")]
        [Summary("Check how many tickets you own for the giveaway!\nexample: `aq getTickets 1`\nThe above will check how many tickets you have for the giveaway with the ID 1\n\u200B")]
        public async Task getTickets([Summary("GiveawayID")] int giveAwayID)
        {
            GiveAwayService.getTickets(giveAwayID,Context);
        }
    }
}
