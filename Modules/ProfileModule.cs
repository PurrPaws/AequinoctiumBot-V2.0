using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace AequinoctiumBot
{
    public class AdminProfileModule : ModuleBase
    {
        //TODO: ADD REMOVELEVEL, reset level, addlevel, remoive exp, remove drak, reset drak functions.
        [Command("removeCharLink")]
        [Summary("Removes The charlink of the specified user. \nexample: `aq removeCharLink @Bleak Himei`\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task resetLink([Summary("UserToReset")] IUser user, [Summary("CharacterName")] string charactername)
        {
            await UserDataService.RemoveCharlink(user, charactername, Context);
        }

        [Command("grantXP")]
        [Summary("Grants the specified user the specified amount of XP. \nexample: `aq grantXP @Bleak 500`\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task GrantXP([Summary("UserToGrant")] IUser user, [Summary("AmountToGrant")] float Amount)
        {
            UserDataService.GrantExp(Amount, user,false);
        }
        [Command("grantDrak")]
        [Summary("Grants the specified user the specified amount of Drak. \nexample: `aq grantDrak @Bleak Himei`\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task GrantDrak([Summary("UserToGrant")] IUser user, [Summary("AmountToGrant")] float Amount)
        {
            UserDataService.GrantDrak(Amount, user);
        }

        [Command("confirm")]
        [Summary("Confirms the charlink and clears the pending status of the specified user. \nexample: `aq confirm @Bleak Rururu`\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task ConfirmLink([Summary("UserToConfirm")] IUser user, [Summary("CharacterName")] string charactername)
        {
            await UserDataService.ConfirmCharacterLink(user, charactername, Context);
        }

        [Command("deny")]
        [Summary("Denies the charlink and removes the user from pending status. \nexample: `aq deny @Bleak Rururu`\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task DenyLink([Summary("UserToReset")] IUser user, [Summary("CharacterName")] string charactername)
        {
            await UserDataService.DenyCharacterLink(user, charactername, Context);
        }

        [Command("createLeaderboard")]
        [Summary("Denies the charlink and removes the user from pending status. \nexample: `aq deny @Bleak Rururu`\n\u200B")]
        [RequireRole(new string[] { "King", "Ruler", "Commander" })]
        public async Task CreateLeaderboard()
        {
            await UserDataService.CreateLeaderboard();
        }

    }
    public class ProfileModule : ModuleBase
    {
        [Command("viewProfile")]
        [Summary("Shows an user's profile, their main, what characters they have, their level etc.\nexample: `aq viewProfile @bleak`\n\u200B")]
        public async Task ViewProfile([Summary("WhatUserToView")] IUser user)
        {
            await UserDataService.ViewProfile(user as IGuildUser,Context);
        }
        [Command("viewProfile")]
        [Summary("Shows your user profile, your main, what characters they have, your level,etc...\nexample: `aq viewProfile`\n\u200B")]
        public async Task ViewProfile()
        {
            await UserDataService.ViewProfile(Context.User as IGuildUser, Context);
        }

        [Command("sync")]
        [Summary("synchronises your name and rank with your main Character.\nexample: `aq sync`\n\u200B")]
        public async Task Sync()
        {
            await UserDataService.SyncChar(Context);
        }

        [Command("realmStatus")]
        [Summary("Shows you the status of a specific realm.\nexample: `aq realmStatus Silvermoon`\n\u200B")]
        public async Task ListRealms([Summary("Name_Of_Realm")]string realmName)
        {
           await WoWService.ListRealmInfo(Context, realmName);
        }

        [Command("inspect", RunMode = RunMode.Async)]
        [Summary("Shows information about a WoW Character\nexample: `aq inspect Himei Silvermoon`\n\u200B")]
        public async Task Inspect([Summary("CharacterName")]string characterName, [Summary("CharacterRealm")]string realmName = "Silvermoon")
        {
            await UserDataService.InspectUser(Context, false, null, characterName, realmName);
        }
        [Command("inspect", RunMode = RunMode.Async)]
        [Summary("Shows information about a WoW Character\nexample: `aq inspect Himei Silvermoon`\n\u200B")]
        public async Task Inspect([Summary("User")]IGuildUser user)
        {
            await UserDataService.InspectUser(Context, true, user, null, null);
        }

        [Command("linkchar", RunMode = RunMode.Async)]
        [Summary("Links your WoW character to your discord and synchronised data between them. \nexample: `aq linkchar Hikagenotora Silvermoon`\n\u200B")]
        public async Task LinkCharacter([Summary("CharacterName")]string characterName, [Summary("CharacterRealm")]string realmName = "Silvermoon")
        {
            await UserDataService.LinkCharacter(characterName, realmName, Context);
        }
        [Command("setMain")]
        [Summary("Sets your main of all your linked characters.\nexample: `aq setMain Hikagenotora`\n\u200B")]
        public async Task SetMain([Summary("CharacterName")]string characterName)
        {
            await UserDataService.SetMain(characterName, Context);
        }

        [Command("shop")]
        [Summary("Sets your main of all your linked characters.\nexample: `aq setMain Hikagenotora`\n\u200B")]
        public async Task Shop([Summary("CharacterName")]string characterName)
        {
            //TODO: add shop logic...
            //await UserDataService.SetMain(characterName, Context);
        }

        [Command("giftDrak")]
        [Summary("Gifts x amount of drak to another user!\nexample: `aq giftDrak @Rururu 20`\n\u200B")]
        public async Task GiftDrak([Summary("CharacterName")]IGuildUser user, [Summary("Amount of Drak")]float Amount)
        {
            UserDataService.GiftDrak(user, Amount, Context);
        }
    }
}
