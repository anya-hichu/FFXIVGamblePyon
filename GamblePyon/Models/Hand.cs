using System.Collections.Generic;
using System.Linq;

namespace GamblePyon.Models;

public class Hand
{
    public int AceLowValue;

    public int AceHighValue;

    public bool Doubled;

    public bool DoubleHit;

    public List<Card> Cards = [];

    public string GetCards(bool forceOnlyValue = false)
    {
        return string.Join(" ", Cards.Select((Card c) => (!forceOnlyValue) ? c.Text : c.TextNoSuit).ToArray());
    }

    public string GetStrValue()
    {
        int value = 0;
        AceLowValue = 0;
        AceHighValue = 0;
        foreach (var card in Cards)
        {
            if (!card.Ace)
            {
                value += card.Value;
            }
        }
        List<Card> aces = Cards.FindAll((Card x) => x.Ace);
        if (aces != null && aces.Count > 0)
        {
            if (value + 10 + aces.Count <= 21)
            {
                AceLowValue = value + aces.Count;
                AceHighValue = value + 10 + aces.Count;
                return $"{value + 10 + aces.Count}";
            }
            return $"{value + aces.Count}";
        }
        return $"{value}";
    }

    public int GetIntValue()
    {
        string value = GetStrValue();
        if (AceHighValue == 0)
        {
            return int.Parse(value);
        }
        return AceHighValue;
    }
}

