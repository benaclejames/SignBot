using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SignBot.Discord;

namespace SignBot.Modules
{
    public class Help
    {
        [Command("help", "Shows a list of available commands")]
        public static async Task HandleHelp(MessageCreateEventArgs messageContext)
        {
            var builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Author = new DiscordEmbedBuilder.EmbedAuthor() {Name = "Help"},
                Title = "SignBot",
                Description = "All commands are case insensitive and start with the prefix `"+Discord.SignBot.CommandPrefix+"`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail{ Url = "https://cdn.discordapp.com/avatars/748523443812827137/2597a7e07fd6060a5d1f16d41f2b0354.png" },
            };
            var commands = "";
            foreach (var command in Discord.SignBot.CommandHandler.Commands)
            {
                var commandKey = command.Key;
                var commandDescription =
                    ((Command) command.Value.GetCustomAttribute(typeof(Command)))?.CommandDescription ?? "<Unknown Usage...>";
                commands += $"`{Discord.SignBot.CommandPrefix}{commandKey}` - {commandDescription}\n";
            }

            builder.AddField("Commands", commands);
            await messageContext.Message.RespondAsync(embed: builder);
        }
    }
}