using GamblePyon;
using GamblePyon.Models;

namespace GlamblePyon.model;

public class Player
{
    public int ID;

    public string Name = "";

    public string Alias = "";

    public int TotalBet;

    public int BetPerHand;

    public int Winnings;

    public int TotalWinnings;

    public BlackjackProps Blackjack = new();

    public Player(int id, string name = "", string alias = "")
    {
        ID = id;
        Name = name;
        Alias = alias;
    }

    public string GetAlias(NameMode nameMode)
    {
        return GetAlias(Name, nameMode);
    }

    public string GetAlias(string name, NameMode nameMode)
    {
        switch (nameMode)
        {
            case NameMode.First:
                if (name.Contains(' '))
                {
                    return name.Substring(0, name.IndexOf(" ")).Trim();
                }
                return name;
            case NameMode.Last:
                if (name.Contains(' '))
                {
                    return name.Substring(name.IndexOf(" ")).Trim();
                }
                return name;
            default:
                return name;
        }
    }

    public string GetNameFromDisplayType(string name)
    {
        return name;
    }

    public void UpdateTotalBet()
    {
        TotalBet = 0;
        foreach (var hand in Blackjack.Hands)
        {
            TotalBet += (hand.Doubled ? (BetPerHand * 2) : BetPerHand);
        }
    }

    public void Reset()
    {
        TotalBet = (Blackjack.Pushed ? TotalBet : 0);
        BetPerHand = TotalBet;
        Winnings = 0;
        Blackjack.IsPush = Blackjack.Pushed;
        Blackjack.Pushed = false;
        Blackjack.Hands[0].AceLowValue = 0;
        Blackjack.Hands[0].AceHighValue = 0;
        Blackjack.Hands[0].Doubled = false;
        Blackjack.Hands[0].DoubleHit = false;
        Blackjack.Hands[0].Cards = new();
        if (Blackjack.Hands.Count > 1)
        {
            Blackjack.Hands.RemoveAt(1);
        }
    }
}
