using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SignBot.Modules.Sign;
using static SignBot.Modules.ReactionHandler;

namespace SignBot.Discord
{
    public static class SignBot
    {
        public static DiscordClient Client;
        public static CommandHandler CommandHandler;
        
        public const string CommandPrefix = "s!";

        private static async Task Main(string[] args)
        {
            if (args?[0] == null)
            {
                Console.WriteLine("[ERROR] A Discord token is required.");
                return;
            }
            
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = args[0],
                TokenType = TokenType.Bot
            });

            CommandHandler = new CommandHandler();
            
            Client.Ready += OnBotReady;

            Client.MessageCreated += HandleMessage;

            Client.MessageReactionAdded += HandleReactions;
            Client.MessageReactionAdded += SignModule.HandleReaction;
            
            Client.MessageReactionRemoved += HandleReactionRemove;

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task OnBotReady(DiscordClient client, ReadyEventArgs eventArgs)
        {
            Console.WriteLine("Client Ready...");
            await client.UpdateStatusAsync(new DiscordActivity("s!help", ActivityType.ListeningTo));
        }

        private static async Task HandleMessage(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Author.IsBot || !eventArgs.Message.Content.StartsWith(CommandPrefix))
                return;
            
            await CommandHandler.HandleCommand(eventArgs);
        }
    }
}