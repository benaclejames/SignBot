using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SignBot.Discord;

namespace SignBot.Modules
{
    public static class ReactionHandler
    {
        public static class Utils
        {
            public static DiscordEmoji GetNumberEmoji(int number) =>
                number switch
                {
                    0 => DiscordEmoji.FromName(Discord.SignBot.Client, ":zero:"),
                    1 => DiscordEmoji.FromName(Discord.SignBot.Client, ":one:"),
                    2 => DiscordEmoji.FromName(Discord.SignBot.Client, ":two:"),
                    3 => DiscordEmoji.FromName(Discord.SignBot.Client, ":three:"),
                    4 => DiscordEmoji.FromName(Discord.SignBot.Client, ":four:"),
                    5 => DiscordEmoji.FromName(Discord.SignBot.Client, ":five:"),
                    6 => DiscordEmoji.FromName(Discord.SignBot.Client, ":six:"),
                    7 => DiscordEmoji.FromName(Discord.SignBot.Client, ":seven:"),
                    8 => DiscordEmoji.FromName(Discord.SignBot.Client, ":eight:"),
                    9 => DiscordEmoji.FromName(Discord.SignBot.Client, ":nine:"),
                    _ => DiscordEmoji.FromName(Discord.SignBot.Client, ":zero:")
                };

            public static int IntFromEmoji(DiscordEmoji emoji) =>
                emoji.Name switch
                {
                    "0️⃣" => 0,
                    "1️⃣" => 1,
                    "2️⃣" => 2,
                    "3️⃣" => 3,
                    "4️⃣" => 4,
                    "5️⃣" => 5,
                    "6️⃣" => 6,
                    "7️⃣" => 7,
                    "8️⃣" => 8,
                    "9️⃣" => 9,
                    _ => 0
                };
        }

        private struct ReactionRole
        {
            public string EmojiName;
            public ulong RoleId;
        }

        private static readonly List<ReactionRole> ReactionRoles = new List<ReactionRole>()
        {
            new ReactionRole {EmojiName = "👂", RoleId = 748512876209242114},
            new ReactionRole {EmojiName = "🔇", RoleId = 748512940822626317},
            new ReactionRole {EmojiName = "🦻", RoleId = 748512908287148062},
            new ReactionRole {EmojiName = "deaf", RoleId = 748512928701087794},
            new ReactionRole {EmojiName = "bslfluent", RoleId = 748517895566524535},
            new ReactionRole {EmojiName = "bslstudent", RoleId = 748517901178503268},
            new ReactionRole {EmojiName = "aslfluent", RoleId = 748517899672616980},
            new ReactionRole {EmojiName = "aslstudent", RoleId = 748517902155644969},
            new ReactionRole {EmojiName = "🇬🇧", RoleId = 748521652215611512},
            new ReactionRole {EmojiName = "🇺🇸", RoleId = 748521656766431343},
            new ReactionRole {EmojiName = "🇫🇷", RoleId = 748521657743966228},
            new ReactionRole {EmojiName = "🇩🇪", RoleId = 748521666900131928},
            new ReactionRole {EmojiName = "🇨🇦", RoleId = 748521657244844084},
        };
        
        public static async Task HandleReactions(DiscordClient client, MessageReactionAddEventArgs reactionArgs)
        {
            if (reactionArgs.Channel.GuildId != 748265117866786926)
                return;

            if (reactionArgs.Message.ChannelId != 748298256185819177)
                return;
            
            foreach (var correspondingRole in from role in ReactionRoles where reactionArgs.Emoji.Name == role.EmojiName 
                select reactionArgs.Channel.Guild.GetRole(role.RoleId))
            {
                var reactingMember = await reactionArgs.Channel.Guild.GetMemberAsync(reactionArgs.User.Id);
                await reactingMember.GrantRoleAsync(correspondingRole);
            }
        }

        public static async Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs reactionArgs)
        {
            if (reactionArgs.Channel.GuildId != 748265117866786926)
                return;

            if (reactionArgs.Message.ChannelId != 748298256185819177)
                return;
            
            foreach (var correspondingRole in from role in ReactionRoles where reactionArgs.Emoji.Name == role.EmojiName 
                select reactionArgs.Channel.Guild.GetRole(role.RoleId))
            {
                var reactingMember = await reactionArgs.Channel.Guild.GetMemberAsync(reactionArgs.User.Id);
                await reactingMember.RevokeRoleAsync(correspondingRole, "Removed Reaction");
            }
        }
    }
}