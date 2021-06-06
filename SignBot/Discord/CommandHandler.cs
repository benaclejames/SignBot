using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace SignBot.Discord
{
    public class Command : Attribute
    {
        public readonly string CommandName;
        public readonly string CommandDescription;
        public Command(string name, string description = null)
        {
            CommandName = name;
            CommandDescription = description;
        }
    }
    
    public class CommandHandler
    {
        public readonly Dictionary<string, MethodInfo> Commands = new Dictionary<string, MethodInfo>();

        public CommandHandler()
        {

            var attributedMethods = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(Command), false).Length > 0)
                .ToArray();

            foreach (var method in attributedMethods)
            {
                var attribute = (Command) method.GetCustomAttribute(typeof(Command));
                if (attribute == null) continue;
                
                Console.WriteLine($"Command Name: {attribute.CommandName}, Method Name: {method.Name}");
                Commands.Add(attribute.CommandName, method);
            }
        }

        public async Task HandleCommand(MessageCreateEventArgs messageContext)
        {
            var commandWithoutPrefix = messageContext.Message.Content.Remove(0, SignBot.CommandPrefix.Length).Split(" ")[0].ToLower();
            if (commandWithoutPrefix.Contains(SignBot.CommandPrefix))
            {
                await messageContext.Message.RespondAsync("Please specify a word to search after the command!");
                return;
            }

            if (Commands.ContainsKey(commandWithoutPrefix))
                Commands[commandWithoutPrefix].Invoke(null, new object[] {messageContext});
            else
                await messageContext.Message.RespondAsync("Command Not Found! :(");
        }

        public static string GetCommandMessageParameters(MessageCreateEventArgs args) => args.Message.Content[(args.Message.Content.IndexOf(" ", StringComparison.Ordinal) + 1)..];
    }
}