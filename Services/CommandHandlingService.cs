using Coaction.KickAssCardBot.Embed_Output;
using Coaction.KickAssCardBot.Extensions;
using Coaction.KickAssCardBot.Helpers;
using Coaction.KickAssCardBot.Manager;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Coaction.KickAssCardBot.Services;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandHandlingService> _logger;
    private readonly ScryfallManagerService _scryfallManagerService;
    private readonly MtgCardOutputManager _mtgCardOutputManager;


    public CommandHandlingService(ILogger<CommandHandlingService> logger, DiscordSocketClient client, IServiceProvider serviceProvider, ScryfallManagerService scryfallManagerService, MtgCardOutputManager mtgCardOutputManager)
    {
        _logger = logger;
        _commands = new CommandService(new CommandServiceConfig()
        {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Debug
        });
        _services = serviceProvider;
        _scryfallManagerService = scryfallManagerService;
        _mtgCardOutputManager = mtgCardOutputManager;
        _client = client;

        _commands.CommandExecuted += CommandExecutedAsync;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {

        if (rawMessage is SocketUserMessage {Source: MessageSource.User} message)
        {
            var context = new SocketCommandContext(_client, message);
            if (context.Guild.Id == 596104949503361050)
            {
                switch (context.User.Id)
                {
                    case 129804455964049408:
                        await ReactWithEmoteAsync(context.Message, "<:WeebsOut:627783662708064256>"); // Respond To Me
                        break;
                    case 153323090397364224:
                        await ReactWithEmoteAsync(context.Message, "<:LETSFUCKINGGOOOOOO:1107690288236998666>"); // Respond To Nick
                        break;
                    case 526144280113184768:
                        await ReactWithEmoteAsync(context.Message, "<:okinform:1168637852528152687>"); // Respond to David O
                        break;
                    case 471176257804042250:
                        await ReactWithEmoteAsync(context.Message, "<:SlonePog:1206825595590152192>"); // Respond to Ryan S
                        break;
                }
            }

            try
            {
                await CheckForCardInBrackets(message, context);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var argPos = 0;

            if (!message.HasStringPrefix(Environment.GetEnvironmentVariable("DISCORD_BOT_COMMAND_PREFIX") ?? "!", ref argPos)
                || !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                return;
            }

            await _commands.ExecuteAsync(context, argPos, _services);
        }
    }

    private async Task ReactWithEmoteAsync(SocketUserMessage userMsg, string escapedEmote)
    {
        if (Emote.TryParse(escapedEmote, out var emote))
        {
            await userMsg.AddReactionAsync(emote);
        }
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified)
            return;

        if (result.IsSuccess)
            return;

        await context.Channel.SendMessageAsync($"Command execution failed due to: {result}");
        _logger.LogError($"Command execution failed due to: {result}");
    }

    private async Task CheckForCardInBrackets(SocketUserMessage message, SocketCommandContext context)
    {
        if ((message.Content.Contains("[") && message.Content.Contains("]")))
        {
            try
            {
                var rx = new Regex(@"\[\[(.*?)\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = rx.Matches(message.Content);
                var messages = new List<(EmbedBuilder, ComponentBuilder)>();
                foreach (var entryName in matches.DistinctBy(x => x.ToString()))
                {
                    var entryString = entryName.ToString();
                    if (entryString != null && entryString.Contains("?"))
                    {
                        var cardData = await _scryfallManagerService.PullScryfallData(entryString);
                        var rulingData = await _scryfallManagerService.PullScryFallRuleData(cardData.Id);
                        var firstRule = rulingData.Rules.FirstOrDefault();
                        var buttons =  ButtonComponentHelper.BuildRuleButtons(rulingData, firstRule, cardData.Id);
                        messages.Add((_mtgCardOutputManager.CardOutput(cardData), buttons));
                    }
                    else if (entryString != null)
                    {
                        var cardData = await _scryfallManagerService.PullScryfallData(entryString);
                        var scryFallSetData = await _scryfallManagerService.PullScryfallSetData(cardData?.PrintsSearchUri);
                        var purchaseCardButtons = ButtonComponentHelper.BuildBuyButtons(cardData, selectItems: scryFallSetData?.BuildPrintingSelectMenu());
                        messages.Add((_mtgCardOutputManager.CardOutput(cardData), purchaseCardButtons));
                    }
                }

                var discordBotUser = context.Guild.GetUser(_client.CurrentUser.Id);
                if (discordBotUser is { GuildPermissions: { CreatePublicThreads: true, SendMessagesInThreads: true } })
                {
                    switch (messages.Count)
                    {
                        case > 1:
                            {
                                if (message.Channel is SocketTextChannel socketTextChannel)
                                {
                                    var sanitizedMentions = message.Content;
                                    sanitizedMentions = message.MentionedRoles.Aggregate(sanitizedMentions, (current, role) => current.Replace(role.Mention, role.Name));
                                    sanitizedMentions = message.MentionedUsers.Aggregate(sanitizedMentions, (current, user) => current.Replace(user.Mention, user.GlobalName));

                                    var thread = await socketTextChannel.CreateThreadAsync(string.Join("", sanitizedMentions.Take(100)), ThreadType.PublicThread, ThreadArchiveDuration.OneHour, message);
                                    foreach (var (embedBuilder, componentBuilder) in messages)
                                    {
                                        if (messages.Count > 5)
                                        {
                                            Thread.Sleep(500);
                                        }
                                        _logger.LogDebug(embedBuilder.ToJsonString());
                                        await thread.SendMessageAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
                                    }
                                }

                                break;
                            }
                        case 1:
                            {
                                var (embedBuilder, componentBuilder) = messages.First();
                                await message.ReplyAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
                                break;
                            }
                    }
                }
                else
                {
                    foreach (var (embedBuilder, componentBuilder) in messages)
                    {
                        await message.ReplyAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
                        return;
                    }
                }
            }
            catch (Exception msg)
            {
                _logger.LogError(msg, $"Could not find card name in brackets due to {msg.Message}");
                await context.Message.ReplyAsync("", false, _mtgCardOutputManager.DetermineFailure(3).Build());
            }
        }
    }
}