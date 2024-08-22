using System.Collections.Generic;
using System.Linq;

namespace GamblePyon.Models;

public class BlackjackProps
{
    public List<Hand> Hands = [new()];

    public bool Pushed;

    public bool IsPush;

    public string GetCards()
    {
        return string.Join(" & ", Hands.Select((Hand h) => h.GetCards()).ToArray());
    }

    public string GetValues()
    {
        return string.Join(" & ", Hands.Select((Hand h) =>
        {
            if (h.AceLowValue != 0)
            {
                if (h.AceHighValue < 21)
                {
                    return $"{h.AceLowValue}/{h.AceHighValue}";
                }
                if (h.AceHighValue == 21)
                {
                    return $"{h.AceHighValue}";
                }
                return $"{h.AceLowValue}";
            }
            return h.GetStrValue();
        }).ToArray());
    }
}
