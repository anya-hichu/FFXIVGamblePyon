namespace GamblePyon {
    public class BlackjackConfig {
        public int MaxRoll { get; set; } = 13;
        public int MinBet { get; set; } = 20000;
        public int MaxBet { get; set; } = 500000;
        public float NormalWinMultiplier { get; set; } = 2f;
        public float BlackjackWinMultiplier { get; set; } = 2.5f;
        public int DealerStandsOn { get; set; } = 17;
        public bool AutoDouble { get; set; } = true;
        public bool PushAllowBet { get; set; } = false;
        public bool PushAllowDouble { get; set; } = true;

        public string Message_PlaceBets { get; set; } = "Round Starting, place your bets!  #minbet# ~ #maxbet#  <se.12>";
        public string Message_BetsPlaced { get; set; } = "Bets have been placed. Good luck!! ^^ <se.12>";

        public string Message_PlayerBet { get; set; } = " #player#  bets #bet#!";
        public string Message_PlayerBetPushed { get; set; } = " #player#  bets with push of #bet#!";
        public string Message_PlayerBetDouble { get; set; } = " #player#  doubles their bet to #bet#!! ＜Stand/Hit＞";
        public string Message_PlayerDraw2 { get; set; } = " #player#  draws 2 cards: #cards# (#value#)";
        public string Message_PlayerDraw2Blackjack { get; set; } = " #player#  draws 2 cards: #cards# (#value#) ★BLACKJACK★ <se.7>";
        public string Message_PlayerStandHit { get; set; } = " #player#  Hand: #cards# (#value#) ＜Stand/Hit＞";
        public string Message_PlayerStandHitDouble { get; set; } = " #player#  Hand: #cards# (#value#) ＜Stand/Hit/Double＞";
        public string Message_PlayerHitUnder21 { get; set; } = " #player#  Hand: #cards# (#value#) ＜Stand/Hit＞";
        public string Message_PlayerHitUnder21Doubled { get; set; } = " #player#  Hand: #cards# (#value#) ＜Stand!＞";
        public string Message_PlayerHit21 { get; set; } = " #player#  Hand: #cards# (#value#) ★BLACKJACK★ <se.7>";
        public string Message_PlayerHitOver21 { get; set; } = " #player#  Hand: #cards# (#value#) BUST ;w; <se.11>";

        public string Message_DealerDraw1 { get; set; } = " #dealer#  reveals 1st card: #cards#  (#value#)";
        public string Message_DealerDraw2UnderStand { get; set; } = " #dealer#  reveals 2nd card: #cards# (#value#) 《#stand# ＜Hit!＞";
        public string Message_DealerDraw2Stand { get; set; } = " #dealer#  reveals 2nd card: #cards# (#value#) 》#stand# ＜Stand!＞";
        public string Message_DealerDraw2Blackjack { get; set; } = " #dealer#  reveals 2nd card: #cards# (#value#) ★BLACKJACK★ <se.4>";

        public string Message_DealerHitUnderStand { get; set; } = " #dealer#  Hand: #cards# (#value#) 《#stand# ＜Hit!＞";
        public string Message_DealerHitStand { get; set; } = " #dealer#  Hand: #cards# (#value#) 》#stand# ＜Stand!＞";
        public string Message_DealerHit21 { get; set; } = " #dealer#  Hand: #cards# (#value#) ★BLACKJACK★ <se.4>";
        public string Message_DealerOver21 { get; set; } = " #dealer#  Hand: #cards# (#value#) BUST ;w; <se.11>";

        public string Message_Win { get; set; } = " #player#  wins with Hand: #cards# (#value#) #winnings#!! <se.15>";
        public string Message_Loss { get; set; } = "";
        public string Message_Draw { get; set; } = " #player#  matched dealer's Hand: #cards# (#value#) #bet# ＜Push/Refund＞";
        public string Message_NoWinners { get; set; } = " #dealer#  wins this round, sorry! ^^ <se.11>";
    }
}
