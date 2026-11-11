using System.Text.RegularExpressions;
using Coaction.KickAssCardBot.Constants;
using Coaction.KickAssCardBot.Extensions;
using Coaction.KickAssCardBot.Helpers;
using Discord;
using Microsoft.Extensions.Logging;
using MtgEmoji = Coaction.KickAssCardBot.Models.Scryfall.MtgEmoji;
using ScryFallAutoCompleteModel = Coaction.KickAssCardBot.Models.Scryfall.ScryFallAutoCompleteModel;
using ScryFallCardRulingsModel = Coaction.KickAssCardBot.Models.Scryfall.ScryFallCardRulingsModel;
using ScryfallDataModel = Coaction.KickAssCardBot.Models.Scryfall.ScryfallDataModel;

namespace Coaction.KickAssCardBot.Embed_Output
{
    public class MtgCardOutputManager
    {
        private readonly Color SuccessfulColor = Color.DarkGreen;
        private const int FailedColor = 16580608;
        private readonly ILogger<MtgCardOutputManager> _logger;

        public MtgCardOutputManager(ILogger<MtgCardOutputManager> logger)
        {
            _logger = logger;
        }

        public EmbedBuilder CardOutput(ScryfallDataModel.CardData pulledCard)
        {
            _logger.LogInformation($"Successfully acquired {pulledCard.Name}. Generating output embed.");
            var card = new EmbedBuilder();
            var footer = "";
            string title;
            pulledCard = AddEmojis(pulledCard);

            switch (pulledCard.Layout)
            {
                default:
                    title = $"{pulledCard.Name} {pulledCard.ManaCost}";
                    card.ThumbnailUrl = pulledCard.ImageUris?.Png ?? pulledCard.CardFaces?.FirstOrDefault()?.image_uris?.Png ?? "none";
                    footer = string.IsNullOrEmpty(pulledCard.FlavorText) ? string.Empty : pulledCard.FlavorText;
                    break;
                case LayoutConstants.Split:
                case LayoutConstants.Transform:
                case LayoutConstants.ModalDfc:
                case LayoutConstants.Adventure:
                case LayoutConstants.Prepare:
                case LayoutConstants.Flip:
                    var firstCardFace = pulledCard.CardFaces[0];
                    var secondCardFace = pulledCard.CardFaces[1];
                    card.ThumbnailUrl = firstCardFace.image_uris?.Png ?? pulledCard.ImageUris?.Png ?? "none";
                    title = $"{pulledCard.CardFaces[0].name} {pulledCard.CardFaces[0].mana_cost}";
                    string description;

                    if (firstCardFace.power == null || firstCardFace.toughness == null)
                    {
                        description = $"{firstCardFace.type_line} \n {firstCardFace.oracle_text}";
                    }
                    else
                    {
                        description = $"{firstCardFace.type_line} \n {firstCardFace.oracle_text} \n {firstCardFace.power}/{firstCardFace.toughness}";
                    }

                    if (firstCardFace.Defense > 0)
                    {
                        description += $"\n \n **Starting Defense: {firstCardFace.Defense}**";
                    }

                    var divider = '-'.Multiply(pulledCard.CardFaces[0].name.Length);
                    description += $"\n \n {divider} \n \n [{secondCardFace.name} {secondCardFace.mana_cost}]({pulledCard.ScryfallUri}) \n";

                    if (secondCardFace.power == null || secondCardFace.toughness == null)
                    {
                        description += $"{secondCardFace.type_line} \n {secondCardFace.oracle_text}";
                    }
                    else
                    {
                        description += $"{secondCardFace.type_line} \n {secondCardFace.oracle_text} \n {secondCardFace.power}/{secondCardFace.toughness}";
                    }

                    if (secondCardFace.Defense > 0)
                    {
                        description += $"\n **Starting Defense: {secondCardFace.Defense}**";
                    }

                    card.WithDescription(description);
                    break;
            }

            if (!string.IsNullOrEmpty(pulledCard.OracleText))
            {
                if (pulledCard.Power == null || pulledCard.Toughness == null)
                {
                    card.WithDescription($"{pulledCard.TypeLine} \n {pulledCard.OracleText}");
                }
                else if (!pulledCard.Power.Contains("*"))
                {
                    card.WithDescription($"{pulledCard.TypeLine} \n {pulledCard.OracleText} \n {pulledCard.Power}/{pulledCard.Toughness}");
                }
                else
                {
                    card.WithDescription($"{pulledCard.TypeLine} \n {pulledCard.OracleText} \n \\{pulledCard.Power}/{pulledCard.Toughness}");
                }
            }

            if (pulledCard.Loyalty > 0)
            {
                card.Description += $"\n \n **Starting Loyalty: {pulledCard.Loyalty}** \n";
            }

            card.WithColor(SuccessfulColor);
            card.Url = pulledCard.ScryfallUri;
            card.Title = title;

            card.AddField("Non-Foil Price", $"{pulledCard.Prices.Usd ?? "No Pricing Data".TrimStart('$')}", true);
            card.AddField("Foil Price", $"{pulledCard.Prices.UsdFoil ?? "No Pricing Data".TrimStart('$')}", true);

            card.AddField("Set Name", pulledCard.SetName, true);
            card.AddField("Artist", string.IsNullOrWhiteSpace(pulledCard.Artist) ? "Not available" : pulledCard.Artist, true);
            card.AddField("Reserved List", pulledCard.Reserved ? "On Reserved List" : "Not On Reserved List", true);
            card.AddField($"Rarity", $"{char.ToUpper(pulledCard.Rarity[0]) + pulledCard.Rarity.Substring(1)}", true);

            card.WithFooter(footer);

            return card;
        }

        private ScryfallDataModel.CardData AddEmojis(ScryfallDataModel.CardData cardData)
        {
            _logger.LogInformation("Adding Emojis");
            var emojiObjectData = EmojiHelper.GetSavedDiscordEmojis();

            switch (cardData.Layout)
            {
                case LayoutConstants.Split:
                case LayoutConstants.Transform:
                case LayoutConstants.ModalDfc:
                case LayoutConstants.Adventure:
                case LayoutConstants.Prepare:
                case LayoutConstants.Flip:
                    cardData.CardFaces[0].mana_cost = AddEmojisToText(emojiObjectData, cardData.CardFaces[0].mana_cost);
                    cardData.CardFaces[1].mana_cost = AddEmojisToText(emojiObjectData, cardData.CardFaces[1].mana_cost);
                    cardData.CardFaces[0].oracle_text = AddEmojisToText(emojiObjectData, cardData.CardFaces[0].oracle_text);
                    cardData.CardFaces[1].oracle_text = AddEmojisToText(emojiObjectData, cardData.CardFaces[1].oracle_text);
                    break;
                default:
                    cardData.ManaCost = AddEmojisToText(emojiObjectData, cardData.ManaCost);
                    cardData.OracleText = AddEmojisToText(emojiObjectData, cardData.OracleText);
                    break;
            }

            return cardData;
        }

        private string AddEmojisToText(MtgEmoji emojiObjectData, string oracleText)
        {
            if (string.IsNullOrEmpty(oracleText))
            {
                return string.Empty;
            }

            var rx = new Regex(@"\{(.*?)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var symbols = rx.Matches(oracleText);

            foreach (var match in symbols)
            {
                var symbol = match.ToString();
                if (symbol != null)
                {
                    oracleText = oracleText.Replace(symbol, symbol.Replace("/", ""));
                    symbol = symbol.Replace("/", "");
                    if (emojiObjectData.Emojis.TryGetValue(symbol, out var emojiId))
                    {
                        switch (symbol)
                        {
                            case "{½}":
                                symbol = "onehalf";
                                break;
                            case "{∞}":
                                symbol = "infinity";
                                break;
                            default:
                                break;
                        }
                        if (Emote.TryParse($"<:{symbol.Replace("{", "").Replace("}", "")}:{emojiId}>", out var emoji))
                        {
                            oracleText = oracleText.Replace(symbol, $"<:{symbol.Replace("{", "").Replace("}", "")}:{emoji.Id}> ");
                        }
                    }
                }
            }

            _logger.LogInformation($"Setting Text To: {oracleText}");
            return oracleText;
        }

        public EmbedBuilder RulingOutput(ScryFallCardRulingsModel rulings, ScryfallDataModel.CardData cardData, ScryFallCardRulingsModel.Rule targetRule)
        {
            var rulingEmbed = new EmbedBuilder();
            rulingEmbed.WithColor(SuccessfulColor);
            rulingEmbed.Title = $"Rulings for {cardData.Name}";
            rulingEmbed.Url = cardData.ScryfallUri;

            if (targetRule != null)
            {
                var rule = rulings.Rules.FirstOrDefault(x => x == targetRule);

                if (rule != null)
                {
                    var totalRules = rulings.Rules.Count;
                    var currentIndex = rulings.Rules.IndexOf(rule) + 1;

                    rulingEmbed.AddField($"{rule.published_at}", rule.comment, false);
                    rulingEmbed.WithFooter($"Rule {currentIndex} of {totalRules}");
                    return rulingEmbed;
                }
            }

            rulingEmbed.AddField($"Not Found", $"Unable to locate rules for {cardData.Name}");

            return rulingEmbed;
        }

        public EmbedBuilder LegalitiesOutput(ScryfallDataModel.CardData cardData)
        {
            var embed = new EmbedBuilder();
            embed.WithColor(SuccessfulColor);
            embed.Title = $"Legalities for {cardData.Name}";
            embed.Url = cardData.ScryfallUri;
            embed.Fields.AddRange(cardData.AllLegalities.Select(x => new EmbedFieldBuilder
            {
                Name = x.Key,
                Value = x.Value,
                IsInline = true
            }).ToList());

            return embed;
        }

        private EmbedBuilder ApiSpellFailure(ScryFallAutoCompleteModel entry, string nameGiven)
        {
            var mtgFailure = new EmbedBuilder();
            mtgFailure.WithColor(FailedColor);
            mtgFailure.AddField("Incorrect Spelling: ", "Make sure to correctly type the name of the card you'd like to look up, or select a possible selection below.", true);
            if (entry != null)
            {
                mtgFailure.Title = nameGiven != null ? $"Card Lookup Failed. Name Provided: {nameGiven}" : "Card Lookup Failed.";
            }
            return mtgFailure;
        }

        private EmbedBuilder ApiMultipleEntryFailure()
        {
            var mtgFailure = new EmbedBuilder { Title = "Card Lookup Failed." };
            mtgFailure.WithColor(FailedColor);
            mtgFailure.AddField("Too many card look ups: ", "Please make sure to only link to one card per message.", true);

            return mtgFailure;
        }

        private EmbedBuilder GenericError()
        {
            var mtgFailure = new EmbedBuilder { Title = "Card Lookup Failed." };
            mtgFailure.WithColor(FailedColor);
            mtgFailure.AddField("Whoopsies! ", "I'm not sure what exploded, but something did. Please contact my owner.", true);

            return mtgFailure;
        }

        public EmbedBuilder DetermineFailure(int num, ScryFallAutoCompleteModel entry = null, string entryValue = null)
        {
            switch (num)
            {
                default:
                    return GenericError();
                case 0:
                    return ApiSpellFailure(entry, entryValue);
            }
        }

        public EmbedBuilder GettingHelp()
        {
            var helping = new EmbedBuilder
            {
                Title = "Help Center",
                Description = "Below is a list of commands and features of this bot! It's a work in progress."
            };
            helping.AddField("Card Lookup: ", "Cards can be located with double open and closing brackets [[like this]]. They can be anywhere in your sentence!");
            helping.AddField("Rules Lookup:", "Rulings of cards can be acquired with double open brackets followed by a question mark and closing brackets. Example: [[?Tarmogoyf]].");
            //helping.AddField("Movers and Shakers:", "This bot has the ability to scrape the Mover and Shaker data from MTGGoldFish. Type mtg!setchannel #<channel name> to get started! [Currently Not Working]");
            helping.AddField("Dice Roller:", "Triggered by doing mtg!roll <numberOfDice>d<numberOfSides> Example: mtg!roll 1d20.");
            helping.AddField("Slash Commands:", "A number of slash commands have been added to make looking up information easier. You can type / in chat to see what commands by the bot are available.");
            helping.WithFooter("Powered By Scryfall API. Contact Coaction#5994 for suggestions or bugs");
            helping.WithColor(SuccessfulColor);

            return helping;
        }
    }

}
