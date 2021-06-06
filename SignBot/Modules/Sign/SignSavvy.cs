using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SignBot.Discord;

namespace SignBot.Modules.Sign
{
    public static class SignSavvy
    { 
        private struct UserSignRequests
        {
            public UserSignRequests(DiscordUser user, ApiHelper.ASL.SearchResponse.Root response, DiscordMessage message)
            {
                this.user = user;
                this.response = response;
                this.message = message;
            }
            
            public DiscordUser user;
            public ApiHelper.ASL.SearchResponse.Root response;
            public DiscordMessage message;
        }
        private static readonly List<UserSignRequests> StoredResponses = new List<UserSignRequests>();

        
        [Command("asl", "Searches IRL ASL Signs")]
        public static async Task HandleSign(MessageCreateEventArgs messageContext)
        {
            await messageContext.Channel.TriggerTypingAsync();
            var requestedSign = CommandHandler.GetCommandMessageParameters(messageContext);
            var response = await ApiHelper.ASL.QueryAsl(requestedSign);

            switch (response.type)
            {
                case ApiHelper.ASL.ApiResponseClass.SEARCH:
                    await HandleMultipleSigns(messageContext, response.searchResponse);
                    break;
                case ApiHelper.ASL.ApiResponseClass.PAGE:
                    await HandleSingleSign(messageContext.Channel, messageContext.Author, response.pageResponse);
                    break;
                default:
                    await SignModule.HandleNullResponse(messageContext, requestedSign);
                    break;
            }
        }

        private static DiscordEmbed CreateSignResponseEmbed(DiscordUser user, ApiHelper.ASL.PageResponse.Root signResponse,
            string gifUrl)
        {
            var signMeaning = signResponse.pageResults.pageDetails.meaning.Beautify();
            
            var builder = new DiscordEmbedBuilder();
            builder.WithColor(DiscordColor.Green);
            builder.WithAuthor("Signing Savvy");
            builder.WithThumbnail("https://i.pinimg.com/originals/11/d1/ae/11d1ae485e9b8e9fabedbed3db0a1dc6.png");
            builder.WithTitle(signMeaning);
            builder.WithDescription($"Sign for \"{signMeaning}\" as requested by {user.Mention}");
            builder.WithFooter("SignBot",
                "https://cdn.discordapp.com/avatars/748523443812827137/2597a7e07fd6060a5d1f16d41f2b0354.png?size=128");
            
            if (signResponse.pageResults.pageDetails.context != null)
                builder.AddField("Sign Context", signResponse.pageResults.pageDetails.context.Beautify(), true);
            if (signResponse.pageResults.pageDetails.sentence != null)
                builder.AddField("Sign Used in a Sentence", signResponse.pageResults.pageDetails.sentence, true);
            
            builder.WithImageUrl(gifUrl);
            return builder;
        }

        private static async Task HandleSingleSign(DiscordChannel channel, DiscordUser user, ApiHelper.ASL.PageResponse.Root apiResponse, int context = 0)
        {
            var gifUrl = await FfMpeg.GetGif("asl", apiResponse.pageResults.pageDetails.meaning.ToLower(),
                apiResponse.pageResults.videoURL, context);
            
            var embed = CreateSignResponseEmbed(user, apiResponse, gifUrl);
            await channel.SendMessageAsync(embed: embed);
        }

        private static async Task HandleMultipleSigns(MessageCreateEventArgs messageContext, ApiHelper.ASL.SearchResponse.Root signResponse)
        {
            var embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithColor(DiscordColor.Green);
            var respondedMeaning = signResponse.searchResults.sign.Beautify();
            embedBuilder.WithTitle(respondedMeaning);
            embedBuilder.WithDescription("Please react with your chosen context.");
            embedBuilder.WithUrl("https://www.signingsavvy.com/search/"+signResponse.searchResults.sign.ToLower());
            embedBuilder.WithThumbnail(
                "https://i.pinimg.com/originals/11/d1/ae/11d1ae485e9b8e9fabedbed3db0a1dc6.png");
            embedBuilder.WithAuthor("Signing Savvy");
            embedBuilder.WithFooter("SignBot",
                "https://cdn.discordapp.com/avatars/748523443812827137/2597a7e07fd6060a5d1f16d41f2b0354.png?size=128");

            var reactionCount = 0;
            for (var i = 0; i < signResponse.searchResults.results.Count; i++)
            {
                var currSign = respondedMeaning + " " + signResponse.searchResults.results[i].context;
                var emoji = ReactionHandler.Utils.GetNumberEmoji(i + 1);
                embedBuilder.AddField(emoji, currSign);
                reactionCount += 1;
            }
            var returnedEmbed = await messageContext.Message.RespondAsync(embed: embedBuilder);

            StoredResponses.Add(new UserSignRequests(messageContext.Author, signResponse, returnedEmbed));

            for (var i = 0; i < reactionCount; i++)
            {
                var currReactionEmoji = ReactionHandler.Utils.GetNumberEmoji(i + 1);
                await returnedEmbed.CreateReactionAsync(currReactionEmoji);
            }
        }

        public static async Task HandleUserReaction(MessageReactionAddEventArgs reactionArgs)
        {
            foreach (var request in StoredResponses.Where(request => request.user == reactionArgs.User && request.message == reactionArgs.Message))
            {
                await reactionArgs.Channel.TriggerTypingAsync();
                StoredResponses.Remove(request);
                var contextInt = ReactionHandler.Utils.IntFromEmoji(reactionArgs.Emoji)-1;
                await request.message.DeleteAsync();
                var response = await ApiHelper.ASL.QueryAsl(request.response.searchResults.results[contextInt].pageLink);
                if (response.type != ApiHelper.ASL.ApiResponseClass.PAGE)
                {
                    await reactionArgs.Channel.SendMessageAsync("lol wtf just happened");
                    Console.WriteLine("Error while trying to get sign page for "+request.response.searchResults.sign+". Page registered as searchpage not resultpage");
                    return;
                }

                await HandleSingleSign(request.message.Channel, request.user, response.pageResponse, contextInt);
                return;
            }
        }
    }
}