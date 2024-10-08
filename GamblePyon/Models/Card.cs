using System;

namespace GamblePyon.Models;

public class Card
{
    public string[] Suits = { "♥", "♦", "♣", "♠" };

    public string Text { get; set; }

    public string TextNoSuit { get; set; }

    public int Value { get; set; }

    public string Suit { get; set; }

    public bool Ace { get; set; }

    public Card(int value, bool showSuit)
    {
        Value = ((value <= 10) ? value : 10);
        Ace = value == 1;
        Suit = (showSuit ? Suits[new Random().Next(4)] : "");
        string JQK = value switch
        {
            13 => "K",
            12 => "Q",
            11 => "J",
            _ => "",
        };
        Text = (Ace ? ("A" + Suit) : ((JQK != "") ? (JQK + Suit) : $"{Value}{Suit}"));
        TextNoSuit = (Ace ? "A" : ((JQK != "") ? (JQK ?? "") : $"{Value}"));
    }
}
