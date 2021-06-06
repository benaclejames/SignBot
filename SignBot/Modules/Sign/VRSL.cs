using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SignBot.Discord;

namespace SignBot.Modules.Sign
{
    public class VRSL
    {
        [Command("vrsl", "Searches VR ASL Signs")]
        public static async Task HandleSign(MessageCreateEventArgs messageContext)
        {
            await messageContext.Channel.TriggerTypingAsync();
            var queryWord = CommandHandler.GetCommandMessageParameters(messageContext);
            var apiResponse = await ApiHelper.VRSL.QueryVrsl(queryWord, "vrsl");
            if (apiResponse.searchResults == null)
            {
                await SignModule.HandleNullResponse(messageContext, queryWord);
                return;
            }
            
            var gifUrl = await FfMpeg.GetGif("vrsl", apiResponse.searchResults.sign, apiResponse.searchResults.url);
            await messageContext.Message.RespondAsync(embed: CreateVrslEmbed(messageContext.Author, apiResponse, gifUrl, "VRSL"));
        }
        
        [Command("vrgsl", "Searches VR GSL Signs")]
        public static async Task HandleGslSign(MessageCreateEventArgs messageContext)
        {
            await messageContext.Channel.TriggerTypingAsync();
            var queryWord = CommandHandler.GetCommandMessageParameters(messageContext);
            var apiResponse = await ApiHelper.VRSL.QueryVrsl(queryWord, "vrgsl");
            if (apiResponse.searchResults == null)
            {
                await SignModule.HandleNullResponse(messageContext, queryWord);
                return;
            }
            
            var gifUrl = await FfMpeg.GetGif("vrgsl", apiResponse.searchResults.sign, apiResponse.searchResults.url);
            await messageContext.Message.RespondAsync(embed: CreateVrslEmbed(messageContext.Author, apiResponse, gifUrl, "VRGSL"));
        }

        static DiscordEmbed CreateVrslEmbed(DiscordUser author, ApiHelper.VRSL.SearchResult.Root apiResponse, string gifUrl, string language)
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithColor(DiscordColor.Green);
            builder.WithAuthor(language);
            builder.WithUrl(apiResponse.searchResults.url);
            builder.WithThumbnail("https://vrsl.s3.amazonaws.com/Helping_hands_logo+(1).png");
            builder.WithTitle(apiResponse.searchResults.sign.Beautify());
            builder.WithDescription($"Sign for \"{apiResponse.searchResults.sign.Beautify()}\" as requested by {author.Mention}");
            builder.WithFooter("SignBot",
                "https://cdn.discordapp.com/avatars/748523443812827137/2597a7e07fd6060a5d1f16d41f2b0354.png?size=128");
            
            if (apiResponse.searchResults.category != null)
                builder.AddField("Sign Category", apiResponse.searchResults.category.Beautify(), true);

            builder.WithImageUrl(gifUrl);
            return builder;
        }
    }
}