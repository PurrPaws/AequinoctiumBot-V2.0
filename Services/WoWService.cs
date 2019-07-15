using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Timers;

namespace AequinoctiumBot
{
    public class WoWService
    {
        #region WarcraftClientVars
        public static WarcraftClient warcraftClient;
        readonly static string clientID = "e3dc15a80b124f73be57ba5b056b350a";
        readonly static string clientSecret = "aYosLRo60z0iIc0wOix4zx65uLhKWvXr";
        #endregion

        public DiscordSocketClient _client;
        public WoWService(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task Initialize()
        {
            warcraftClient = new WarcraftClient(clientID, clientSecret, Region.Europe, Locale.en_GB);
            if (warcraftClient == null) { Program.LogConsole("WOW SERVICE", ConsoleColor.Red, "WowClient NOT Found"); }
            return Task.CompletedTask;
        }

        public static async Task InspectCharacter(string _characterName, string _realmName, ICommandContext _context)
        {
            var requestResultCharacter = await warcraftClient.GetCharacterAsync(_realmName, _characterName, CharacterFields.All);
            if (requestResultCharacter.Value == null) { await _context.Channel.SendMessageAsync("Character not found."); return; }
            Character character = requestResultCharacter.Value;

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = character.Name + "-" + character.Realm,
                Description = "Level " + character.Level + " " + character.Race + " " + character.Class,
                ThumbnailUrl = "https://render-eu.worldofwarcraft.com/character/" + character.Thumbnail,
                Color = character.Faction == Faction.Alliance ? Color.Blue : Color.Red,
                Footer = new EmbedFooterBuilder() { Text = "Last updated on: " + character.LastModified }
            };

            if (character.Guild != null) { embed.AddField(new EmbedFieldBuilder() { Name = "<" + character.Guild.Name + ">", Value = "\u200B" }); }
            embed.AddField(new EmbedFieldBuilder() { Name = "__**Achievement Points:**__ " + character.AchievementPoints.ToString() + "    " + "__**Honorable Kills:**__ " + character.TotalHonorableKills.ToString(), Value = "\u200B" });

            List<Proffessions> bfaProffessionIds = new List<Proffessions> { Proffessions.Alchemy, Proffessions.Blacksmithing, Proffessions.Enchanting, Proffessions.Engineering, Proffessions.Herbalism, Proffessions.Inscription, Proffessions.Jewelcrafting, Proffessions.Leatherworking, Proffessions.Mining, Proffessions.Skinning, Proffessions.Tailoring };
            Profession firstPrimaryProf = (character.Professions.Primary as List<Profession>).Find(x => bfaProffessionIds.Contains((Proffessions)x.Id));
            if (firstPrimaryProf != null) { bfaProffessionIds.Remove((Proffessions)firstPrimaryProf.Id); }
            Profession SecondPrimaryProf = (character.Professions.Primary as List<Profession>).Find(x => bfaProffessionIds.Contains((Proffessions)x.Id));
            if (SecondPrimaryProf != null) { bfaProffessionIds.Remove((Proffessions)SecondPrimaryProf.Id); }
            string firstPrimaryProfString = firstPrimaryProf == null ? "N/A" : firstPrimaryProf.Name + "[" + firstPrimaryProf.Rank + "/" + firstPrimaryProf.Max + "]";
            string secondPrimaryProfString = SecondPrimaryProf == null ? "N/A" : SecondPrimaryProf.Name + "[" + SecondPrimaryProf.Rank + "/" + SecondPrimaryProf.Max + "]";
            embed.AddField(new EmbedFieldBuilder() { Name = "__**Proffesions:**__\n" + "- " + firstPrimaryProfString + "\n" + "- " + secondPrimaryProfString, Value = "\u200B" });

            string offhandString = character.Items.OffHand != null ? "__Offhand:__ " + character.Items.OffHand.Name + " [Ilvl: " + character.Items.OffHand.ItemLevel + " " + GetItemQualityString(character.Items.OffHand) + "]\n" : "__Offhand:__ N/A";

            embed.AddField(new EmbedFieldBuilder()
            {
                Name = "__**AvgIlvl:**__ " + character.Items.AverageItemLevelEquipped + "\n__**Gear:**__",
                Value = "__Head:__ " + character.Items.Head.Name + " [Ilvl " + character.Items.Head.ItemLevel + " " + GetItemQualityString(character.Items.Head) + "]\n" +
                "__Neck:__ " + character.Items.Neck.Name + " [Ilvl " + character.Items.Neck.ItemLevel + " " + GetItemQualityString(character.Items.Neck) + "]\n" +
                "__Shoulders:__ " + character.Items.Shoulder.Name + " [Ilvl " + character.Items.Shoulder.ItemLevel + " " + GetItemQualityString(character.Items.Shoulder) + "]\n" +
                "__Back:__ " + character.Items.Back.Name + " [Ilvl " + character.Items.Back.ItemLevel + " " + GetItemQualityString(character.Items.Back) + "]\n" +
                "__Chest:__ " + character.Items.Chest.Name + " [Ilvl " + character.Items.Chest.ItemLevel + " " + GetItemQualityString(character.Items.Chest) + "]\n" +
                "__Wrists:__ " + character.Items.Wrist.Name + " [Ilvl " + character.Items.Wrist.ItemLevel + " " + GetItemQualityString(character.Items.Wrist) + "]\n" +
                "__Hands:__ " + character.Items.Hands.Name + " [Ilvl " + character.Items.Hands.ItemLevel + " " + GetItemQualityString(character.Items.Hands) + "]\n" +
                "__Waist:__ " + character.Items.Waist.Name + " [Ilvl " + character.Items.Waist.ItemLevel + " " + GetItemQualityString(character.Items.Waist) + "]\n" +
                "__Leggs:__ " + character.Items.Legs.Name + " [Ilvl " + character.Items.Legs.ItemLevel + " " + GetItemQualityString(character.Items.Legs) + "]\n" +
                "__Feet:__ " + character.Items.Feet.Name + " [Ilvl " + character.Items.Feet.ItemLevel + " " + GetItemQualityString(character.Items.Feet) + "]\n" +
                "__Ring 1:__ " + character.Items.Finger1.Name + " [Ilvl " + character.Items.Finger1.ItemLevel + " " + GetItemQualityString(character.Items.Finger1) + "]\n" +
                "__Ring 2:__ " + character.Items.Finger2.Name + " [Ilvl " + character.Items.Finger2.ItemLevel + " " + GetItemQualityString(character.Items.Finger2) + "]\n" +
                "__Trinket 1:__ " + character.Items.Trinket1.Name + " [Ilvl " + character.Items.Trinket1.ItemLevel + " " + GetItemQualityString(character.Items.Trinket1) + "]\n" +
                "__Trinket 2:__ " + character.Items.Trinket2.Name + " [Ilvl " + character.Items.Trinket2.ItemLevel + " " + GetItemQualityString(character.Items.Trinket2) + "]\n" +
                "__Mainhand:__ " + character.Items.MainHand.Name + " [Ilvl " + character.Items.MainHand.ItemLevel + " " + GetItemQualityString(character.Items.MainHand) + "]\n" +
                offhandString
            });

            embed.AddField(new EmbedFieldBuilder()
            {
                Name = "__**Talents:**__",
                Value = ListTalents(character)
            });

            await _context.Channel.SendMessageAsync("", false, embed.Build());
        }
        public static async Task ListRealmInfo(ICommandContext _context, string realmName)
        {
            var realms = await warcraftClient.GetRealmStatusAsync(Region.Europe, Locale.en_GB);
            if (realms.Success)
            {
                Realm realm = realms.Value.FirstOrDefault(x => x.Name == realmName);
                if (realm == null) { await _context.Channel.SendMessageAsync("Realm was not found."); return; }

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "Realm Status for " + realm.Name,
                    Color = realm.Status ? Color.Green : Color.Red
                };

                string realmStatusString = realm.Status ? "Online" : "Offline";
                string queueString = realm.Queue ? "Yes." : "No.";
                string connectedRealmsString = "";
                foreach (string connectedRealmName in realm.ConnectedRealms)
                {
                    connectedRealmsString += "- " + connectedRealmName + "\n";
                }

                embed.AddField(new EmbedFieldBuilder() { Name = "Status: `" + realmStatusString + "`", Value = "\u200B" });
                embed.AddField(new EmbedFieldBuilder() { Name = "Population: `" + realm.Population + "`", Value = "\u200B" });
                embed.AddField(new EmbedFieldBuilder() { Name = "Timezone: `" + realm.Timezone + "`", Value = "\u200B" });
                embed.AddField(new EmbedFieldBuilder() { Name = "BattleGroup: `" + realm.Battlegroup + "`", Value = "\u200B" });
                embed.AddField(new EmbedFieldBuilder() { Name = "Queue: `" + queueString + "`", Value = "\u200B" });
                embed.AddField(new EmbedFieldBuilder() { Name = "Connected Realms:", Value = connectedRealmsString });
                await _context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                await _context.Channel.SendMessageAsync("There was a problem with contacting the Blizzard API server.\n" +
                    "Error message: `" + realms.Error.Detail + "`");
            }
        }

        public static async Task<int> GetCharacterGuildRank(string _characterName, string _realmName, ICommandContext _context, bool CheckIfCharacterIsValid = true)
        {
            if (CheckIfCharacterIsValid)
            {
                var req = await warcraftClient.GetCharacterAsync(_realmName, _characterName, CharacterFields.All);
                Character character = req.Value;
                if (character == null)
                {
                    await _context.Channel.SendMessageAsync("Did not find Character in the WoW Database.");
                    return -1;
                }
            }

            //Get Their Guild Rank
            var user = Program.guild.Users.FirstOrDefault(x => x.Id == _context.User.Id);
            var Guild = await warcraftClient.GetGuildAsync("Silvermoon", "Aequinoctium", GuildFields.All);
            GuildMember member = Guild.Value.Members.FirstOrDefault(x => x.Character.Name.ToLower() == _characterName.ToLower());
            if (member == null) { return -12; }
            return member.Rank;
        }


        public static async Task<String> GetCharacterName(UserCharacter userCharacter,ICommandContext _context)
        {
            Character character = (await warcraftClient.GetCharacterAsync(userCharacter.realm, userCharacter.name, CharacterFields.All)).Value;
            if (character == null) {await _context.Channel.SendMessageAsync($"Could not find Character: {userCharacter.name}-{userCharacter.realm} in the World of Warcraft Database. "); return null; }

            return character.Name;
        }

        //#endregion WoWService

        #region HelperFunctions
        static string GetItemQualityString(CharacterItem item)
        {
            switch (item.Quality)
            {
                case ItemQuality.Poor:
                    return "POOR";
                case ItemQuality.Common:
                    return "COMMON";
                case ItemQuality.Uncommon:
                    return "UNCOMMON";
                case ItemQuality.Rare:
                    return "RARE";
                case ItemQuality.Epic:
                    return "EPIC";
                case ItemQuality.Heirloom:
                    return "HEIRLOOM";
                case ItemQuality.Artifact:
                    return "ARTIFACT";
                default:
                    return "Null";
            }
        }
        static string ListTalents(Character character) //TODO sort talents by tier.
        {
            string TalentString = "";
            foreach (Talent talent in character.Talents.FirstOrDefault(x => x.Selected == true).Talents)
            {
                TalentString += "Tier " + talent.Tier + ": " + talent.Spell.Name + "\n";
            }

            return TalentString;
        }
        #endregion HelperFunctions
    }
}
