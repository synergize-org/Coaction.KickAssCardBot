using Coaction.KickAssCardBot.Models.Scryfall;
using Discord;
using ScryfallDataModel = Coaction.KickAssCardBot.Models.Scryfall.ScryfallDataModel;

namespace Coaction.KickAssCardBot.Helpers
{
    public static class ButtonComponentHelper
    {
        public static ComponentBuilder BuildBuyButtons(ScryfallDataModel.CardData cardData, bool displayRulesButton = true, List<SelectMenuOptionBuilder> selectItems = null)
        {
            var purchaseCardButtons = new ComponentBuilder();

            if (cardData != null)
            {
                if (displayRulesButton)
                {
                    purchaseCardButtons.WithButton("Rules", style: ButtonStyle.Primary, customId: $"rules-{cardData.Id}");
                }

                if (cardData.AllLegalities?.Count > 0)
                {
                    purchaseCardButtons.WithButton($"Legalities", style: ButtonStyle.Secondary, customId: $"legalities-{cardData.Id}");
                }

                if (cardData.PurchaseUris != null)
                {
                    if (cardData.PurchaseUris.Tcgplayer != null)
                    {
                        purchaseCardButtons.WithButton("TCGplayer", style: ButtonStyle.Link, url: cardData.PurchaseUris.Tcgplayer);
                    }

                    if (cardData.PurchaseUris.Cardhoarder != null)
                    {
                        purchaseCardButtons.WithButton("Cardhoarder", style: ButtonStyle.Link, url: cardData.PurchaseUris.Cardhoarder);
                    }
                }

                if (cardData.RelatedUris != null)
                {
                    if (!string.IsNullOrEmpty(cardData.RelatedUris.Edhrec))
                    {
                        purchaseCardButtons.WithButton($"Edhrec", style: ButtonStyle.Link, url: cardData.RelatedUris.Edhrec);
                    }
                }

                if (selectItems != null && selectItems.Count > 0)
                {
                    purchaseCardButtons.WithSelectMenu($"cardset-{cardData.Name}", selectItems, "Printings");
                }
            }

            return purchaseCardButtons;
        }

        public static ComponentBuilder BuildRuleButtons(ScryFallCardRulingsModel rules, ScryFallCardRulingsModel.Rule currentRule, Guid cardId, bool buttonDisabled = false)
        {
            var rule = rules.Rules.FirstOrDefault(x => x == currentRule);
            var component = new ComponentBuilder();

            if (rule != null)
            {
                var index = rules.Rules.IndexOf(rule);

                // Checks if there's a previous element to provide previous button.
                if (index - 1 > -1)
                {
                    component.WithButton("Previous", style: ButtonStyle.Secondary, customId: $"CardRule-Previous-{cardId}#{index}", disabled: buttonDisabled);
                }

                // Checks if there's a next element to provide a next button.
                if (index + 1 < rules.Rules.Count)
                {
                    component.WithButton("Next", style: ButtonStyle.Primary, customId: $"CardRule-Next-{cardId}#{index}", disabled: buttonDisabled);
                }

                component.WithButton("Post", style: ButtonStyle.Success, customId: $"CardRule-Send-{cardId}#{index}");
            }

            return component;
        }
    }
}
