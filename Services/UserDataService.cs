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
    public class UserDataService
    {
        public static List<UserDataSet> UserData = new List<UserDataSet>();

        readonly static ulong NotifyChannelID = 600385155131113497;
        public static List<IRole> ServerRoles;



        public void InitializeRanks(DiscordSocketClient _client)
        {
            Program.guild = _client.GetGuild(578979368143945729);
            ServerRoles = new List<IRole>();
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "king"));
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "ruler"));
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "commander"));
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "knight"));
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "trial"));
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "soldier"));
            ServerRoles.Add(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "apprentice"));
        }

        public static async Task SyncChar(ICommandContext _context)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == _context.User.Id);
            if (userDataSet == null) { return; }
            UserCharacter mainCharacter = userDataSet.characters.FirstOrDefault(x => x.main == true);
            if (mainCharacter == null) { return; }

            if (mainCharacter.pending) { await _context.Channel.SendMessageAsync("That character is still pending confirmation."); return; }

            string characterName = await WoWService.GetCharacterName(mainCharacter, _context);
            if (characterName == null) { return; }
            try { await (_context.User as IGuildUser).ModifyAsync(x => x.Nickname = characterName); }
            catch { }

            int RoleNumber = await WoWService.GetCharacterGuildRank(mainCharacter.name, mainCharacter.realm, _context, false);
            if (RoleNumber == -12)
            {
                await (_context.User as IGuildUser).AddRoleAsync(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "temp"));
            }
            else
            {
                try { await (_context.User as IGuildUser).AddRoleAsync(ServerRoles[RoleNumber]); }
                catch {}
            }
            userDataSet.LastSync = DateTime.Now;
            await _context.Channel.SendMessageAsync("Successfully synced.");
        }

        public static async Task InspectUser(ICommandContext _Context, bool isIUser, IGuildUser guildUser, string characterName, string CharacterServer)
        {
            if (isIUser)
            {
                if (guildUser == null) { return; }
                UserDataSet userToInspect = UserData.FirstOrDefault(x => x.userID == guildUser.Id);
                UserCharacter character = userToInspect.characters.FirstOrDefault(x => x.main == true);
                await WoWService.InspectCharacter(character.name, character.realm, _Context);
            }
            else
            {
                await WoWService.InspectCharacter(characterName, CharacterServer, _Context);
            }
        }

        public static void On_MidnightTimer()
        {
            foreach (UserDataSet dataSet in UserData)
            {
                dataSet.FirstMessageOfDay = false;
                dataSet.FirstConnectionToVoiceOfDay = false;
            }
            SaveUserData();
        }

        public static async Task LinkCharacter(string _characterName, string _realmName, ICommandContext _context)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == _context.User.Id);
            if (userDataSet == null) { return; }

            if (UserData.Any(x => x.characters.Any(y => y.name == _characterName && y.realm == _realmName)))
            {
                await _context.Channel.SendMessageAsync("That character is already linked to someone. Contact an administrator if this is your character.");
                return;
            }

            UserCharacter charToLink = new UserCharacter();
            charToLink.Initialize(_characterName, _realmName);
            userDataSet.characters.Add(charToLink);
            SaveUserData();

            await _context.Channel.SendMessageAsync("A Commander has to confirm the character have requested to link is yours. The commanders have been notified and will whisper you asap. The bot will notify you with a DM once your pending status has been cleared.");
            IMessageChannel notifyChannel = Program.guild.Channels.FirstOrDefault(x => x.Id == NotifyChannelID) as IMessageChannel;

            int rankNumber = await WoWService.GetCharacterGuildRank(_characterName, _realmName, _context);
            if (rankNumber == -1) { return; }

            string assignedRankString = rankNumber != 12 ? ServerRoles[rankNumber].Name : "UNKNOWN";

            await notifyChannel.SendMessageAsync(
                "__**New pending user:**__ \n" +
                "Name: `" + _context.User.Username + "`\n" +
                "Character: `" + charToLink.name + "`\n" +
                "Realm: `" + charToLink.realm + "`\n" +
                "GuildRank: `" + assignedRankString + "`\n" +
                "DiscordMention: `" + _context.User.Mention + "`");
        }

        public static async Task ConfirmCharacterLink(IUser user, string _charactername, ICommandContext _context)
        {
            bool setToMain = false;
            bool notFoundInGuild = false;
            string messageString = "**Exceptions:**";

            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find user: `{user.Id}` in the database."); return; }

            UserCharacter userCharacter = userDataSet.characters.FirstOrDefault(x => x.name.ToLower() == _charactername.ToLower());
            if (userCharacter == null) {await _context.Channel.SendMessageAsync($"An Exception occured. Could not find Character: `{_charactername}` for userd `{user.Id}`"); return; }
            userCharacter.pending = false;

            if (!userDataSet.characters.Any(x => x.main == true))
            {
                userCharacter.main = true;
                setToMain = true;

                string characterName = await WoWService.GetCharacterName(userCharacter, _context);
                if (characterName == null) { return; }
                try { await (user as IGuildUser).ModifyAsync(x => x.Nickname = characterName); }
                catch { messageString += "\n - A problem occured while changing the nickname."; }

                int RoleNumber = await WoWService.GetCharacterGuildRank(userCharacter.name, userCharacter.realm, _context, false);
                if (RoleNumber == -12)
                {
                    await (user as IGuildUser).AddRoleAsync(Program.guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "temp"));
                    notFoundInGuild = true;
                }
                else
                {
                    try { await (user as IGuildUser).AddRoleAsync(ServerRoles[RoleNumber]); }
                    catch { messageString += "\n- Could not synchronise ranks."; }
                }
            }

            if (messageString == "**Exceptions:**") { messageString = ""; }
            await _context.Channel.SendMessageAsync("Successfully confirmed the charlink. User has been notified.\n" + messageString);

            string DmString = $"The pending status of your character `{userCharacter.name}` has been cleared and you are now linked to your WoW Character!\n\n";
            if (setToMain) { DmString += "Since this is your first character you have linked it has been automatically set as your main. \nIf you wish to resynchronise your discord rank with your in-game WoW Rank then use the command `aq sync`! \n\n"; }
            if (!setToMain) { DmString += $"Since this is not your first character you have linked it will not be set to your main. If you do want this character to be your main you can use the command `aq setMain {userCharacter.name}`\n\n"; }
            if (notFoundInGuild) { DmString += "Your character was not found in the guild. This could mean that the World of Warcraft API is lagging behind a bit (or you have just joined the guild), You've been granted a temporary rank. **You should use the command `aq sync` every hour untill your rank has been succesfully synchronised.**\n\n"; }
            DmString += "**Thank you for your patience and enjoy your stay!**";
            await user.SendMessageAsync(DmString);
            SaveUserData();
        }

        public static async Task DenyCharacterLink(IUser user, string _charactername, ICommandContext _context)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { return; }
            userDataSet.characters.Remove(userDataSet.characters.FirstOrDefault(x => x.name == _charactername));
            SaveUserData();
            await _context.Channel.SendMessageAsync($"Successfully denied the charlink for character ´{_charactername}´. User has been notified.");
            await user.SendMessageAsync($"Unfortunetly it has been found that the character you are trying to link to named {_charactername} is not yours. Your characterlink request has been cleared. \n\nIf this truly is you character, please contact an administrator.");
        }

        public static async Task RemoveCharlink(IUser user, string _charactername, ICommandContext _context)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find user: `{user.Id}` in the database."); return; }

            UserCharacter userCharacter = userDataSet.characters.FirstOrDefault(x => x.name.ToLower() == _charactername.ToLower());
            if (userCharacter == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find Character: `{_charactername}` for userd `{user.Id}`"); return; }
            if (userCharacter.main)
            {
                try { await (user as IGuildUser).ModifyAsync(x => x.Nickname = ""); } catch { }
                try { await (user as IGuildUser).RemoveRolesAsync(ServerRoles); } catch { }
                if (userDataSet.characters.Any(x => x.pending = false))
                {
                    await SetMain(userDataSet.characters.FirstOrDefault(x => x.pending == false).name, user, _context);
                }
                else
                {
                    await user.SendMessageAsync("You have no other confirmed characterlinks available. Please link a new character or ask to be confirmed.");
                }
            }
            else
            {
                await user.SendMessageAsync($"Your characterlink for your alt `{userCharacter.name}` has been reset by an administrator.");
            }
            userDataSet.characters.Remove(userCharacter);
            await _context.Channel.SendMessageAsync("User has been reset.");
            SaveUserData();
        }

        public static async Task SetMain(string _charactername,ICommandContext _context)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == _context.User.Id);
            if (userDataSet == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find user: `{_context.User.Id}` in the database."); return; }

            UserCharacter userCharacter = userDataSet.characters.FirstOrDefault(x => x.name.ToLower() == _charactername.ToLower());
            if (userCharacter == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find Character: `{_charactername}` for userd `{_context.User.Id}`"); return; }

            if (userCharacter.pending) { await _context.Channel.SendMessageAsync("That character is still pending confirmation."); return; }

            foreach (UserCharacter character in userDataSet.characters)
            {
                character.main = false;
            }
            userCharacter.main = true;

            try { await (_context.User as IGuildUser).ModifyAsync(x => x.Nickname = ""); } catch { }
            try { await (_context.User as IGuildUser).RemoveRolesAsync(ServerRoles); } catch { }

            await _context.User.SendMessageAsync("You have been assigned a new main character. Please run `aq sync` to resynchronise to your new main.");
        }
        public static async Task SetMain(string _charactername, IUser user, ICommandContext _context)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find user: `{user.Id}` in the database."); return; }

            UserCharacter userCharacter = userDataSet.characters.FirstOrDefault(x => x.name.ToLower() == _charactername.ToLower());
            if (userCharacter == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find Character: `{_charactername}` for userd `{user.Id}`"); return; }

            if (userCharacter.pending) { await _context.Channel.SendMessageAsync("That character is still pending confirmation."); return; }

            foreach (UserCharacter character in userDataSet.characters)
            {
                character.main = false;
            }
            userCharacter.main = true;

            try { await (user as IGuildUser).ModifyAsync(x => x.Nickname = ""); } catch { }
            try { await (user as IGuildUser).RemoveRolesAsync(ServerRoles); } catch { }

            await user.SendMessageAsync("You have been assigned a new main character. Please run `aq sync` to resynchronise to your new main.");
            SaveUserData();
        }

        public static bool HasGottenFirstMessageOfTheDay(IUser user)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { Program.LogConsole("USERDATASERVICE", ConsoleColor.Red, $"GrantEXP error - userdata == null for user {user.Username} ({user.Id})"); return true; };

            return userDataSet.FirstMessageOfDay;
        }
        public static bool HasGottenFirstConnectionToVoiceOfDay(IUser user)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { Program.LogConsole("USERDATASERVICE", ConsoleColor.Red, $"GrantEXP error - userdata == null for user {user.Username} ({user.Id})"); return true; };

            return userDataSet.FirstConnectionToVoiceOfDay;
        }
        //TODO: make Embedd.
        public static void On_LevelUp(UserDataSet userDataSet) { (Program.guild.GetChannel(600380855537500202) as IMessageChannel).SendMessageAsync($"{Program.guild.GetUser(userDataSet.userID).Mention} has reached level {userDataSet.level}"); userDataSet.drak += 10; Program.guild.GetUser(userDataSet.userID).SendMessageAsync("You've gained 10 Ξ for leveling up!"); }

        public static float CalculateRequiredExp(int level)
        {
            float baseXpRequirement = 10;
            float modifier = 1.1f;
            float multiplicationValue = MathF.Pow(modifier, level);
            float totalXpRequirement = (level * baseXpRequirement) * multiplicationValue;
            return totalXpRequirement;
        }

        public static void GrantExp(float amount, IUser user, bool isFirstMessageOfDay = false, bool isFirstVoiceConnectionOfDay = false)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) {Program.LogConsole("USERDATASERVICE",ConsoleColor.Red, $"GrantEXP error - userdata == null for user {user.Username} ({user.Id})"); return; };

            while (amount > 0)
            {
                if ((userDataSet.experience + amount) > CalculateRequiredExp(userDataSet.level))
                {
                    float xpToNextLevel = CalculateRequiredExp(userDataSet.level) - userDataSet.experience;
                    userDataSet.level++;
                    userDataSet.experience += xpToNextLevel;
                    amount -= xpToNextLevel;
                    On_LevelUp(userDataSet);
                }
                else
                {
                    userDataSet.experience += amount;
                    amount = 0;
                }
            }
            if (isFirstMessageOfDay) { userDataSet.FirstMessageOfDay = true; }
            if (isFirstVoiceConnectionOfDay) { userDataSet.FirstConnectionToVoiceOfDay = true; }
            SaveUserData();
        }
        public static void GrantDrak(float amount, IUser user)
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { Program.LogConsole("USERDATASERVICE", ConsoleColor.Red, $"GrantEXP error - userdata == null for user {user.Username} ({user.Id})"); return; };

            userDataSet.drak += amount;
            SaveUserData();
        }

        public static async Task ViewProfile(IGuildUser user, ICommandContext _context) //TODO: Make embedd.
        {
            UserDataSet userDataSet = UserData.FirstOrDefault(x => x.userID == user.Id);
            if (userDataSet == null) { await _context.Channel.SendMessageAsync($"An Exception occured. Could not find user: `{user.Id}` in the database."); return; }

            Color color;
            switch (user.Status)
            {
                case UserStatus.Offline:
                    color = Color.LightGrey;
                    break;
                case UserStatus.Online:
                    color = Color.Green;
                    break;
                case UserStatus.Idle:
                    color = Color.LightOrange;
                    break;
                case UserStatus.AFK:
                    color = Color.LightOrange;
                    break;
                case UserStatus.DoNotDisturb:
                    color = Color.Red;
                    break;
                case UserStatus.Invisible:
                    color = Color.LightGrey;
                    break;
                default:
                    color = Color.LightGrey;
                    break;
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = (user.Nickname == "" ? user.Username : user.Nickname),
                Description = $"ID: <{user.Id}>\n\u200B",
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = color,
                Footer = new EmbedFooterBuilder() { Text = $"User Joined on {user.JoinedAt.Value.Date.ToString("dd/MM/yy")}" }
            };

            embed.AddField(new EmbedFieldBuilder() { Value = $"**Level:** `{userDataSet.level}`\n**Experience:** `{Math.Truncate(100 * userDataSet.experience) / 100}/{Math.Truncate(100 * CalculateRequiredExp(userDataSet.level))/100}`\n**Draks:** `{userDataSet.drak}` Ξ\n\u200B", Name = "Profile:"});

            string valueString = "";
            // 21 - NameLength= spacing

            foreach (UserCharacter character in userDataSet.characters)
            {
                string pendingstring = character.pending ? "pending" : "confirmed";
                string mainString = character.main ? "[MAIN]" : "[ALT]";
                valueString += $"• {character.name.First().ToString().ToUpper() + character.name.Substring(1)}-{character.realm.First().ToString().ToUpper() + character.realm.Substring(1)} | Status: {pendingstring} | {mainString}\n";
            }
            valueString += "\n\u200B";
            embed.AddField(new EmbedFieldBuilder()
            {
                Name = "Characters:",
                Value = valueString
            });

            await _context.Channel.SendMessageAsync(null, false, embed.Build());
        }



        public static void On_UserJoined(SocketGuildUser user)
        {
            UserDataSet newUserData = new UserDataSet();
            newUserData.Initialize(user.Id, 10, 213f, 100f); //TODO: remove values when event ends!
            UserData.Add(newUserData);
            SaveUserData();
        }
        public static void On_UserLeft(SocketGuildUser user)
        {
            UserData.Remove(UserData.FirstOrDefault(x => x.userID == user.Id));
            SaveUserData();
        }

        public void LoadUserData()
        {
            try
            {
                using (FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "UserDatabase.xml", FileMode.Open))
                {
                    XmlSerializer XML = new XmlSerializer(typeof(List<UserDataSet>));
                    UserData = (List<UserDataSet>)XML.Deserialize(stream);
                    Program.LogConsole("USERDATASERVICE", ConsoleColor.Green, "Sucessfully loaded " + UserData.Count + " Users.");
                }
            }
            catch (Exception arg)
            {
                Console.WriteLine(arg.Message);
                Program.LogConsole("USERDATASERVICE", ConsoleColor.Green, arg.Message);
                UserData = new List<UserDataSet>();
            }
        }

        public static void SaveUserData()
        {
            using (FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "UserDatabase.xml", FileMode.Create))
            {
                XmlSerializer XML = new XmlSerializer(typeof(List<UserDataSet>));
                XML.Serialize(stream, UserData);
            }
        }
    }
  
    //Serializable Classes
    [Serializable]
    public class UserDataSet
    {
        public ulong userID;
        public int level = 1;
        public float experience = 0;
        public float drak = 0;
        public List<UserCharacter> characters = new List<UserCharacter>();
        
        public DateTime LastSync;
        public DateTime LastSyncMessage;

        public bool FirstMessageOfDay = false;
        public bool FirstConnectionToVoiceOfDay = false;

        public void Initialize(ulong _userID, int _level = 1, float _experience = 0, float _drak = 0)
        {
            userID = _userID;
            level = _level;
            experience = _experience;
            drak = _drak;
        }
    }
    [Serializable]
    public class UserCharacter
    {
        public string name;
        public string realm;
        public bool pending = true;
        public bool main = false;
        public void Initialize(string _name, string _realm, bool _pending = true, bool _main = false)
        {
            name = _name;
            realm = _realm;
            pending = _pending;
            main = _main;
        }
    }
}




