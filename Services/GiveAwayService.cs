using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using System.Globalization;


namespace AequinoctiumBot
{
    public class GiveAwayService
    {
        public static List<GiveAway> GiveAways = new List<GiveAway>();
        public readonly static ulong giveawayChannelID = 601771906126970880;

        public static void CreateGiveAway(string rewardString, float entryCost, int endsInDays)
        {
            GiveAway newGiveAway = new GiveAway();

            newGiveAway.id = GiveAways.Count()+1;
            newGiveAway.giveAwayItemString = rewardString;
            newGiveAway.ticketEntryCost = entryCost;
            newGiveAway.endDateTime = DateTime.Now.AddDays(endsInDays);

            IUserMessage userMessage = (Program.guild.GetChannel(giveawayChannelID) as IMessageChannel).SendMessageAsync("",false,CreateMessageEmbed(newGiveAway).Build()).Result;
            newGiveAway.giveAwayMessageId = userMessage.Id;
            GiveAways.Add(newGiveAway);
            SaveGiveAways();
        }
        public static void SetGiveawayStatus(int id, string statusString)
        {
            GiveAwayState status = GiveAwayState.Open;

            switch (statusString.ToLower())
            {
                case "open":
                    status = GiveAwayState.Open;
                    break;
                case "ended":
                    status = GiveAwayState.Ended;
                    break;
                case "pending":
                    status = GiveAwayState.Pending;
                    break;
                case "cancelled":
                    status = GiveAwayState.Cancelled;
                    break;
            }

            GiveAways[id - 1].state = status;

            UpdateGiveAwayMessage(GiveAways[id - 1]);
            SaveGiveAways();
        }
        public static void BuyTickets(ICommandContext context,int id, int amountToBuy)
        {
            GiveAway giveAway = GiveAways[id - 1];
            switch (giveAway.state)
            {
                case GiveAwayState.Cancelled:
                    context.Channel.SendMessageAsync("Sorry, That giveaway is cancelled! You cannot buy tickets.");
                    return;
                case GiveAwayState.Ended:
                    context.Channel.SendMessageAsync("Sorry, That giveaway has aleady ended! You cannot buy tickets.");
                    return;
                case GiveAwayState.Pending:
                    context.Channel.SendMessageAsync("Sorry, That giveaway has not yet started and is in pending status! You cannot buy tickets yet. Wait untill the status is `open`.");
                    return;
            }

            UserDataSet userDataSet = UserDataService.UserData.FirstOrDefault(x => x.userID == context.User.Id);
            if (userDataSet.drak - (amountToBuy * giveAway.ticketEntryCost) < 0) { context.Channel.SendMessageAsync("Sorry, you don't have enough Draks to complete this purchase."); return; }
            if (giveAway.tickets.Exists(x => x.userDataSet.userID == userDataSet.userID))
            {
                userDataSet.drak -= giveAway.ticketEntryCost * amountToBuy;

                TicketData ticketData = giveAway.tickets.FirstOrDefault(x => x.userDataSet.userID == userDataSet.userID);
                ticketData.userDataSet = userDataSet;
                ticketData.ticketAmount += amountToBuy;
                context.Channel.SendMessageAsync("Tickets have been successfully bought!");
            }
            else
            {
                userDataSet.drak -= giveAway.ticketEntryCost * amountToBuy;

                TicketData ticketData = new TicketData();
                ticketData.userDataSet = userDataSet;
                ticketData.ticketAmount += amountToBuy;

                giveAway.tickets.Add(ticketData);
                context.Channel.SendMessageAsync("Tickets have been successfully bought!");
            }
            UserDataService.SaveUserData();
            SaveGiveAways();
            UpdateGiveAwayMessage(giveAway);
        }
        static EmbedBuilder CreateMessageEmbed(GiveAway newGiveAway)
        {
            string statusString = "";
            Color statusColor = new Color();
            switch (newGiveAway.state)
            {
                case GiveAwayState.Cancelled:
                    statusString = "Cancelled";
                    statusColor = Color.Red;
                    break;
                case GiveAwayState.Ended:
                    statusString = "Ended";
                    statusColor = Color.LightGrey;
                    break;
                case GiveAwayState.Open:
                    statusString = "Open";
                    statusColor = Color.Green;
                    break;
                case GiveAwayState.Pending:
                    statusString = "Pending";
                    statusColor = Color.LightOrange;
                    break;
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = statusColor,
                Title = $"GiveAway #{newGiveAway.id}"
            };
            embed.AddField(new EmbedFieldBuilder() { Name = "Ticket Cost:", Value = newGiveAway.ticketEntryCost + " Ξ" });
            embed.AddField(new EmbedFieldBuilder() { Name = "Reward:", Value = newGiveAway.giveAwayItemString });
            embed.AddField(new EmbedFieldBuilder() { Name = "Status:", Value = statusString });
            embed.AddField(new EmbedFieldBuilder() { Name = "Entries:", Value = newGiveAway.GetTotalTickets() });
            embed.AddField(new EmbedFieldBuilder() { Name = "End Date:", Value = newGiveAway.endDateTime.Date.ToString("dd/MM/yy") });

            if (newGiveAway.winner != null)
            {
                embed.AddField(new EmbedFieldBuilder() { Name = "__Winner:__", Value = "**" + Program.guild.GetUser(newGiveAway.winner.userID).Mention+ "**" });
            }
            

            embed.Footer = new EmbedFooterBuilder() { Text = "Giveaway created: " + newGiveAway.openDateTime.Date.ToString("dd/MM/yy") };

            return embed;
        }
        public static void getTickets(int GiveAwayID,ICommandContext context)
        {
            GiveAway giveAway = GiveAways[GiveAwayID - 1];
            int amount = 0;
            amount = giveAway.tickets.FirstOrDefault(x => x.userDataSet.userID == context.User.Id).ticketAmount;
            context.Channel.SendMessageAsync($"You own {amount} Tickets of the pool of {giveAway.GetTotalTickets()} Tickets.");
        }
        public static void EndGiveAway(int giveAwayID)
        {
            GiveAway giveAway = GiveAways[giveAwayID - 1];

            List<UserDataSet> PossibleWinners = new List<UserDataSet>();

            foreach(TicketData ticket in giveAway.tickets)
            {
                for(int i = 0; i < ticket.ticketAmount; i++)
                {
                    PossibleWinners.Add(ticket.userDataSet);
                }
            }

            ExtentionMethods.Shuffle(PossibleWinners);

            Random WinnerSelector = new Random();

            giveAway.winner = PossibleWinners[WinnerSelector.Next(0, PossibleWinners.Count)];

            giveAway.state = GiveAwayState.Ended;
            UpdateGiveAwayMessage(giveAway);
        }

        public static void UpdateGiveAwayMessage(GiveAway giveAway)
        {
            ((Program.guild.GetChannel(giveawayChannelID) as IMessageChannel).GetMessageAsync(giveAway.giveAwayMessageId).Result as IUserMessage).ModifyAsync(x => x.Embed = CreateMessageEmbed(giveAway).Build());
        }
        public static void LoadGiveAways()
        {
            try
            {
                using (FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "GiveAwayDatabase.xml", FileMode.Open))
                {
                    XmlSerializer XML = new XmlSerializer(typeof(List<GiveAway>));
                    GiveAways = (List<GiveAway>)XML.Deserialize(stream);
                    Program.LogConsole("USERDATASERVICE", ConsoleColor.Green, "Sucessfully loaded " + GiveAways.Count + " GiveAways.");
                }
            }
            catch (Exception arg)
            {
                Console.WriteLine(arg.Message);
                Program.LogConsole("USERDATASERVICE", ConsoleColor.Green, arg.Message);
                GiveAways = new List<GiveAway>();
            }
        }

        public static void SaveGiveAways()
        {
            using (FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "GiveAwayDatabase.xml", FileMode.Create))
            {
                XmlSerializer XML = new XmlSerializer(typeof(List<GiveAway>));
                XML.Serialize(stream, GiveAways);
            }
        }

        public static void BackupGiveAways()
        {
            using (FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + $"/Backups/GiveAwayBackups/GiveAwayBackup {DateTime.Now.ToString("dd/MM/yy")}.xml", FileMode.Create))
            {
                XmlSerializer XML = new XmlSerializer(typeof(List<GiveAway>));
                XML.Serialize(stream, GiveAways);
            }
        }
    }

    [Serializable]
    public class GiveAway
    {
        public int id;
        public string giveAwayItemString;
        public ulong giveAwayMessageId;
        public float ticketEntryCost = 10f;
        public DateTime openDateTime = DateTime.Now;
        public DateTime endDateTime;
        public UserDataSet winner = null;
        public List<TicketData> tickets = new List<TicketData>();
        public GiveAwayState state = GiveAwayState.Pending;

        public int GetTotalTickets()
        {
            return tickets.Sum(x => x.ticketAmount);
        }

    }
    [Serializable]
    public class TicketData
    {
        public UserDataSet userDataSet;
        public int ticketAmount = 0;
    }
}
