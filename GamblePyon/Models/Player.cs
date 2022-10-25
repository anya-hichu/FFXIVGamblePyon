namespace GamblePyon.Models {
    public class Player {
        public int ID;
        public string Name = "";
        public string Alias = "";
        public int Bet = 0;
        public int Winnings = 0;
        public int TotalWinnings = 0;
        
        public BlackjackProps Blackjack = new BlackjackProps();
    }
}
