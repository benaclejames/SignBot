using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SignBot.Modules.Sign
{
    public static class SignModule
    {
        public static async Task HandleNullResponse(MessageCreateEventArgs messageContext, string requestedSign)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = "https://cdn.discordapp.com/avatars/748523443812827137/2597a7e07fd6060a5d1f16d41f2b0354.png?size=128",
                    Text = "SignBot"
                },
                Title = "Whoops!",
                Description =
                    $"Apologies, but your request for the sign for '{requestedSign.Beautify()}' yielded no results."
            };
            await messageContext.Message.RespondAsync(embed: embed);
        }

        public static async Task HandleReaction(DiscordClient client, MessageReactionAddEventArgs reactionArgs)
        {
           if (reactionArgs.User != client.CurrentUser)
                await SignSavvy.HandleUserReaction(reactionArgs);
        }
    }
}