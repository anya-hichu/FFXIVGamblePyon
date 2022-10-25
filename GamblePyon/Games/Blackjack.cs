using System.Collections.Generic;
using Dalamud.Interface.Colors;
using Dalamud.Interface;
using GamblePyon.Extensions;
using GamblePyon.Models;
using ImGuiNET;
using System;
using System.Text.RegularExpressions;

namespace GamblePyon.Games {
    public class Blackjack {
        public event EventHandler<MessageEventArgs> Send_Message;
        public bool Enabled = false;

        public Config Config;

        private Action CurrentAction = Action.None;
        private enum Action { None, DealerDraw1, DealerDraw2, DealerHit, PlayerDraw2, PlayerHit }

        private Event CurrentEvent = Event.PlaceBets;
        private enum Event { PlaceBets, BetsPlaced, CardActions }

        private Player CurrentPlayer;
        public Player Dealer = new Player();
        public List<Player> Players = new List<Player>() {
            new Player(){ ID = 0 },
            new Player(){ ID = 1 },
            new Player(){ ID = 2 },
            new Player(){ ID = 3 },
            new Player(){ ID = 4 },
            new Player(){ ID = 5 },
            new Player(){ ID = 6 }
        };

        public Blackjack() {
            Dealer.Name = GamblePyon.ClientState.LocalPlayer.Name.TextValue;
        }

        public void ResetRound() {
            EndRound();
            Dealer.Bet = 0;
            Dealer.Blackjack.AceLowValue = 0;
            Dealer.Blackjack.AceHighValue = 0;
            Dealer.Blackjack.Cards = new List<Card>();

            foreach(Player player in Players) {
                player.Bet = player.Blackjack.Pushed ? player.Bet : 0;
                player.Winnings = 0;
                player.Blackjack.AceLowValue = 0;
                player.Blackjack.AceHighValue = 0;
                player.Blackjack.Doubled = false;
                player.Blackjack.DoubleHit = false;
                player.Blackjack.IsPush = player.Blackjack.Pushed;
                player.Blackjack.Pushed = false;
                player.Blackjack.Cards = new List<Card>();
            }
        }

        private void EndRound() {
            CurrentEvent = Event.PlaceBets;
        }

        private string FormatMessage(string message, Player player) {
            if(string.IsNullOrWhiteSpace(message)) { return ""; }

            return message.Replace("#dealer#", player.Alias)
                .Replace("#player#", player.Alias)
                .Replace("#bet#", player.Bet.ToString("N0"))
                .Replace("#cards#", player.Blackjack.GetCards())
                .Replace("#value#", player.Blackjack.AceLowValue != 0 ? (player.Blackjack.AceHighValue < 21 ? $"{player.Blackjack.AceLowValue}/{player.Blackjack.AceHighValue}" : player.Blackjack.AceHighValue == 21 ? $"{player.Blackjack.AceHighValue}" : $"{player.Blackjack.AceLowValue}") : player.Blackjack.GetStrValue())
                .Replace("#stand#", Config.Blackjack.DealerStandsOn.ToString())
                .Replace("#winnings#", player.Winnings.ToString("N0"))
                .Replace("#profit#", player.TotalWinnings.ToString("N0"))
                .Replace("#minbet#", Config.Blackjack.MinBet.ToString("N0"))
                .Replace("#maxbet#", Config.Blackjack.MaxBet.ToString("N0"));
        }

        private void SendMessage(string message) {
            if(Enabled) {
                Send_Message(this, new MessageEventArgs(message, MessageType.Normal));
            }
        }

        private void SendRoll() {
            if(Enabled) {
                Send_Message(this, new MessageEventArgs(Config.Blackjack.MaxRoll.ToString(), MessageType.BlackjackRoll));
            }
        }

        public void OnChatMessage(string sender, string message) {
            try {
                if((Config.RollCommand == "/dice" && sender.ToLower().Contains(Dealer.Name.ToLower()) && message.Contains("Random! (1-13)")) || (Config.RollCommand == "/random" && message.Contains("You roll a") && message.Contains("out of 13"))) {
                    if(CurrentAction == Action.DealerDraw1) {
                        CurrentAction = Action.None;

                        string cardValue = Config.RollCommand == "/dice" ? message.Replace("Random! (1-13) ", "") : Regex.Replace(message, ".*You roll a ([^\\(]+)\\(.*", "$1", RegexOptions.Singleline).Trim();
                        Dealer.Blackjack.Cards.Add(new Card(int.Parse(cardValue)));

                        SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerDraw1, Dealer)}");
                    } else if(CurrentAction == Action.DealerDraw2) {
                        CurrentAction = Action.None;

                        string cardValue = Config.RollCommand == "/dice" ? message.Replace("Random! (1-13) ", "") : Regex.Replace(message, ".*You roll a ([^\\(]+)\\(.*", "$1", RegexOptions.Singleline).Trim();
                        Dealer.Blackjack.Cards.Add(new Card(int.Parse(cardValue)));

                        int value = Dealer.Blackjack.GetIntValue();
                        if(value == 21) {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerDraw2Blackjack, Dealer)}");
                        } else if(value >= Config.Blackjack.DealerStandsOn) {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerDraw2Stand, Dealer)}");
                        } else {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerDraw2UnderStand, Dealer)}");
                        }
                    } else if(CurrentAction == Action.DealerHit) {
                        CurrentAction = Action.None;

                        string cardValue = Config.RollCommand == "/dice" ? message.Replace("Random! (1-13) ", "") : Regex.Replace(message, ".*You roll a ([^\\(]+)\\(.*", "$1", RegexOptions.Singleline).Trim();
                        Dealer.Blackjack.Cards.Add(new Card(int.Parse(cardValue)));

                        int value = Dealer.Blackjack.GetIntValue();
                        if(value == 21) {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerHit21, Dealer)}");
                        } else if(value >= Config.Blackjack.DealerStandsOn && value < 21) {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerHitStand, Dealer)}");
                        } else if(value < Config.Blackjack.DealerStandsOn) {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerHitUnderStand, Dealer)}");
                        } else {
                            SendMessage($"{FormatMessage(Config.Blackjack.Message_DealerOver21, Dealer)}");
                        }
                    } else if(CurrentAction == Action.PlayerDraw2) {
                        if(CurrentPlayer != null) {
                            string cardValue = Config.RollCommand == "/dice" ? message.Replace("Random! (1-13) ", "") : Regex.Replace(message, ".*You roll a ([^\\(]+)\\(.*", "$1", RegexOptions.Singleline).Trim();
                            CurrentPlayer.Blackjack.Cards.Add(new Card(int.Parse(cardValue)));

                            if(CurrentPlayer.Blackjack.Cards.Count == 2) {
                                CurrentAction = Action.None;

                                int value = CurrentPlayer.Blackjack.GetIntValue();
                                if(value == 21) {
                                    SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerDraw2Blackjack, CurrentPlayer)}");
                                } else {
                                    SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerDraw2, CurrentPlayer)}");
                                }

                                CurrentPlayer = null;
                            }
                        }
                    } else if(CurrentAction == Action.PlayerHit) {
                        CurrentAction = Action.None;

                        if(CurrentPlayer != null) {
                            string cardValue = Config.RollCommand == "/dice" ? message.Replace("Random! (1-13) ", "") : Regex.Replace(message, ".*You roll a ([^\\(]+)\\(.*", "$1", RegexOptions.Singleline).Trim();
                            CurrentPlayer.Blackjack.Cards.Add(new Card(int.Parse(cardValue)));

                            int value = CurrentPlayer.Blackjack.GetIntValue();
                            if(value == 21) {
                                SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerHit21, CurrentPlayer)}");
                            } else if(value < 21) {
                                if(CurrentPlayer.Blackjack.Doubled) {
                                    SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerHitUnder21Doubled, CurrentPlayer)}");
                                } else {
                                    SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerHitUnder21, CurrentPlayer)}");
                                }
                            } else {
                                SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerHitOver21, CurrentPlayer)}");
                            }

                            CurrentPlayer = null;
                        }
                    }
                }
            } catch { }
        }

        public void DrawDealer() {
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(1, 230 + 5 * ImGuiHelpers.GlobalScale);

            ImGui.Checkbox("Enabled", ref Enabled);
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Must first enable this option for the plugin to function.\nShould disable it while not doing a blackjack round to prevent unnecessary dice roll monitoring."); }
            ImGui.NextColumn();
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Dealer Name:");
            ImGui.SameLine();
            ImGui.InputText($"###dealerName", ref Dealer.Name, 255);
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("This is you! ..Or at least it should be.\nI dunno what would happen if it's not."); }

            ImGui.Columns(8);
            ImGui.SetColumnWidth(0, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(1, 80 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(2, 75 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(3, 80 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(4, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(5, 50 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(6, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(7, 80 + 5 * ImGuiHelpers.GlobalScale);

            ImGui.Separator();

            ImGui.Text("Alias");
            ImGui.NextColumn();
            ImGui.Text("Total Bet");
            ImGui.NextColumn();
            ImGui.Text("Bet Actions");
            ImGui.NextColumn();
            ImGui.Text("Card Actions");
            ImGui.NextColumn();
            ImGui.Text("Cards");
            ImGui.NextColumn();
            ImGui.Text("Value");
            ImGui.NextColumn();
            ImGui.Text("Result Actions");
            ImGui.NextColumn();
            ImGui.Text("Profit");
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.PushID($"dealer");

            //Alias
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"###dealerAlias", ref Dealer.Alias, 255);
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("A name to refer to the dealer by, does not need to be your full name."); }
            ImGui.NextColumn();

            //Bet
            ImGui.SetNextItemWidth(-1);
            int totalBet = 0;
            foreach(Player player in Players) {
                if(string.IsNullOrWhiteSpace(player.Alias)) { continue; }
                totalBet += player.Bet;
            }
            Dealer.Bet = totalBet;
            ImGuiEx.InputText("###dealerBet", ref Dealer.Bet);
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("The total bet pool for this round.\nYou don't need to touch this.\n..you can try though if you want?"); }
            ImGui.NextColumn();

            //Bet Actions
            ImGui.SetNextItemWidth(-1);
            if(!string.IsNullOrWhiteSpace(Dealer.Alias) && (CurrentEvent == Event.PlaceBets || (CurrentEvent == Event.BetsPlaced && Players.Find(x => x.Bet > 0) != null))) {
                string btnId = CurrentEvent != Event.BetsPlaced ? "B" : "F";
                string btnMsg = CurrentEvent != Event.BetsPlaced ? Config.Blackjack.Message_PlaceBets : Config.Blackjack.Message_BetsPlaced;
                string hoverMsg = CurrentEvent != Event.BetsPlaced ? "Request players to place bets." : "Announce all bets are placed.";

                if(ImGui.Button(btnId)) {
                    SendMessage($"{FormatMessage(btnMsg, Dealer)}");
                    if(CurrentEvent == Event.PlaceBets) {
                        ResetRound();
                        CurrentEvent = Event.BetsPlaced;
                    } else if(CurrentEvent == Event.BetsPlaced) {
                        CurrentEvent = Event.CardActions;
                    }
                }
                if(ImGui.IsItemHovered()) {
                    ImGui.SetTooltip(hoverMsg);
                }
            }
            ImGui.NextColumn();

            //Card Actions
            ImGui.SetNextItemWidth(-1);
            if(!string.IsNullOrWhiteSpace(Dealer.Alias) && CurrentEvent == Event.CardActions && Dealer.Blackjack.GetIntValue() < Config.Blackjack.DealerStandsOn) {
                if(Dealer.Blackjack.Cards.Count == 0 && Players.Find(x => x.Alias != "" && x.Bet != 0 && x.Blackjack.Cards.Count != 2) == null) {
                    if(ImGui.Button("1")) {
                        CurrentAction = Action.DealerDraw1;
                        SendRoll();
                    }
                    if(ImGui.IsItemHovered()) { ImGui.SetTooltip("After player initial 2 cards, draw dealer 1st card."); }
                } else if(Dealer.Blackjack.Cards.Count > 0) {
                    string btnId = Dealer.Blackjack.Cards.Count == 1 ? "2" : "H";
                    string hoverMsg = Dealer.Blackjack.Cards.Count == 1 ? "After player card actions, draw dealer 2nd card." : $"After 2nd card, hit until {Config.Blackjack.DealerStandsOn} or over.";

                    if(ImGui.Button(btnId)) {
                        CurrentAction = Dealer.Blackjack.Cards.Count == 1 ? Action.DealerDraw2 : Action.DealerHit;
                        SendRoll();
                    }
                    if(ImGui.IsItemHovered()) { ImGui.SetTooltip(hoverMsg); }
                }
            }
            ImGui.NextColumn();

            //Cards
            ImGui.SetNextItemWidth(-1);
            string cards = Dealer.Blackjack.GetCards();
            ImGui.InputText($"###dealerCards", ref cards, 255);
            ImGui.NextColumn();

            //Value
            ImGui.SetNextItemWidth(-1);
            string value = Dealer.Blackjack.GetStrValue();
            ImGui.InputText($"###dealerValue", ref value, 255);
            ImGui.NextColumn();

            //Result Actions
            ImGui.SetNextItemWidth(-1);
            int dealerValue = Dealer.Blackjack.GetIntValue();
            Player? playerValue = Players.Find(x => !string.IsNullOrWhiteSpace(x.Alias) && x.Blackjack.Cards.Count > 0 && x.Blackjack.GetIntValue() > dealerValue && x.Blackjack.GetIntValue() <= 21);
            if(!string.IsNullOrWhiteSpace(Dealer.Alias) && playerValue == null && dealerValue >= Config.Blackjack.DealerStandsOn && dealerValue <= 21) {
                if(ImGui.Button("W")) {
                    SendMessage($"{FormatMessage(Config.Blackjack.Message_NoWinners, Dealer)}");
                    EndRound();
                }
                if(ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Announce dealer wins.");
                }
            }
            ImGui.NextColumn();

            //Profit
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###dealerProfit", ref Dealer.TotalWinnings);
            string areYouOk = Dealer.TotalWinnings < 0 ? "\nAhh.. not looking good is it? ;w;" : Dealer.TotalWinnings > 0 ? "\nEmpty their pockets!! :3" : "";
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"The total profit/loss for the dealer.\n{areYouOk}"); }
            ImGui.NextColumn();
        }

        public void DrawPlayerList() {
            //ImGuiComponents.IconButton(FontAwesomeIcon.AngleDown))
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Players");

            ImGui.Columns(8);
            ImGui.SetColumnWidth(0, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(1, 80 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(2, 75 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(3, 80 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(4, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(5, 50 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(6, 90 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(7, 80 + 5 * ImGuiHelpers.GlobalScale);

            ImGui.Separator();

            ImGui.Text("Alias");
            ImGui.NextColumn();
            ImGui.Text("Bet Amount");
            ImGui.NextColumn();
            ImGui.Text("Bet Actions");
            ImGui.NextColumn();
            ImGui.Text("Card Actions");
            ImGui.NextColumn();
            ImGui.Text("Cards");
            ImGui.NextColumn();
            ImGui.Text("Value");
            ImGui.NextColumn();
            ImGui.Text("Result Actions");
            ImGui.NextColumn();
            ImGui.Text("Profit");
            ImGui.NextColumn();

            foreach(Player player in Players) {
                ImGui.Separator();
                ImGui.PushID($"player_{player.ID}");

                //Name
                ImGui.SetNextItemWidth(-1);
                ImGui.InputText($"###playerAlias", ref player.Alias, 255);
                if(ImGui.IsItemHovered()) { ImGui.SetTooltip("A name to refer to this player by, does not need to be their full name.\nLeave blank if the seat is free."); }
                ImGui.NextColumn();

                //Bet Amount
                ImGui.SetNextItemWidth(-1);
                ImGuiEx.InputText("###playerBet", ref player.Bet);
                if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Input the bet amount that the player traded.\nLeave at 0 if player is sitting out the round or the seat is free."); }
                ImGui.NextColumn();

                //Bet Actions
                ImGui.SetNextItemWidth(-1);
                if(!string.IsNullOrWhiteSpace(player.Alias) && player.Bet > 0 && player.Blackjack.Cards.Count == 0 && CurrentEvent == Event.BetsPlaced) {
                    string btnId = player.Blackjack.IsPush ? "P" : "B";
                    string btnMsg = player.Blackjack.IsPush ? Config.Blackjack.Message_PlayerBetPushed : Config.Blackjack.Message_PlayerBet;
                    string hoverMsg = player.Blackjack.IsPush && Config.Blackjack.PushAllowBet ? "After trading, announce player's bet as a pushed bet." : player.Blackjack.IsPush ? "No trading, announce player's bet as a pushed bet from previous round." : "After trading, announce player's bet.";
                    if(ImGui.Button(btnId)) {
                        SendMessage($"{FormatMessage(btnMsg, player)}");
                    }
                    if(ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(hoverMsg);
                    }
                }
                
                if(!string.IsNullOrWhiteSpace(player.Alias) && !player.Blackjack.Doubled && Dealer.Blackjack.Cards.Count == 1 && player.Blackjack.Cards.Count == 2 && player.Blackjack.GetIntValue() < 21 && (!player.Blackjack.IsPush || (player.Blackjack.IsPush && Config.Blackjack.PushAllowDouble))) {
                    ImGui.SameLine();
                    if(ImGui.Button("D###doubleBet")) {
                        if(Config.Blackjack.AutoDouble) {
                            player.Bet = player.Bet * 2;
                        }
                        player.Blackjack.Doubled = true;
                        SendMessage($"{FormatMessage(Config.Blackjack.Message_PlayerBetDouble, player)}");
                    }
                    if(ImGui.IsItemHovered()) {
                        ImGui.SetTooltip("After trading, announce player's bet as a doubled bet.");
                    }
                }
                ImGui.NextColumn();

                //Card Actions
                ImGui.SetNextItemWidth(-1);
                if(!string.IsNullOrWhiteSpace(player.Alias) && player.Bet > 0 && CurrentEvent == Event.CardActions) {
                    string btnId = player.Blackjack.Cards.Count == 0 ? "1" : player.Blackjack.Cards.Count == 1 ? "2" : "";
                    string hoverMsg = player.Blackjack.Cards.Count < 2 ? "After bets, draw initial 2 cards." : "";

                    if(btnId != "") {
                        if(ImGui.Button(btnId)) {
                            CurrentAction = Action.PlayerDraw2;
                            CurrentPlayer = player;
                            SendRoll();
                        }
                        if(ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(hoverMsg);
                        }
                    } else if(player.Blackjack.GetIntValue() < 21 && Dealer.Blackjack.Cards.Count > 0 && (!player.Blackjack.Doubled || !player.Blackjack.DoubleHit)) {
                        btnId = "?";
                        hoverMsg = "After dealer's 1st card, request player to " + (!player.Blackjack.IsPush || Config.Blackjack.PushAllowDouble ? "Stand/Hit/Double" : "Stand/Hit");

                        if(ImGui.Button(btnId)) {
                            SendMessage($"{FormatMessage((!player.Blackjack.IsPush || Config.Blackjack.PushAllowDouble ? Config.Blackjack.Message_PlayerStandHitDouble : Config.Blackjack.Message_PlayerStandHit), player)}");
                        }
                        if(ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(hoverMsg);
                        }

                        ImGui.SameLine();
                        if(ImGui.Button("H")) {
                            if(player.Blackjack.Doubled) {
                                player.Blackjack.DoubleHit = true;
                            }
                            CurrentAction = Action.PlayerHit;
                            CurrentPlayer = player;
                            SendRoll();
                        }
                        if(ImGui.IsItemHovered()) {
                            ImGui.SetTooltip("Respond to player hit request, draw additional card.");
                        }
                    }
                }
                ImGui.NextColumn();

                //Cards
                ImGui.SetNextItemWidth(-1);
                string cards = player.Blackjack.GetCards();
                ImGui.InputText($"###playerCards", ref cards, 255);
                ImGui.NextColumn();

                //Value
                ImGui.SetNextItemWidth(-1);
                string value = player.Blackjack.GetStrValue();
                ImGui.InputText($"###playerValue", ref value, 255);
                ImGui.NextColumn();

                //Result Actions
                ImGui.SetNextItemWidth(-1);
                if(!string.IsNullOrWhiteSpace(player.Alias) && player.Bet > 0 && player.Blackjack.Cards.Count >= 2 && Dealer.Blackjack.Cards.Count >= 2) {
                    int dealerValue = Dealer.Blackjack.GetIntValue();
                    if(dealerValue >= Config.Blackjack.DealerStandsOn) {
                        int playerValue = player.Blackjack.GetIntValue();

                        if((playerValue > dealerValue || dealerValue > 21) && playerValue <= 21) {
                            if(ImGui.Button("W")) {
                                double playerWinnings = playerValue == 21 ? (player.Bet * Config.Blackjack.BlackjackWinMultiplier) : player.Bet * Config.Blackjack.NormalWinMultiplier;
                                player.Winnings = (int)playerWinnings;
                                player.TotalWinnings += player.Winnings;
                                Dealer.TotalWinnings = Dealer.TotalWinnings - player.Winnings;
                                player.Bet = 0;
                                SendMessage($"{FormatMessage(Config.Blackjack.Message_Win, player)}");
                                EndRound();
                            }
                            if(ImGui.IsItemHovered()) {
                                ImGui.SetTooltip("Calculate & announce player's winnings.");
                            }
                        } else if(playerValue < dealerValue || playerValue > 21) {
                            if(ImGui.Button("L")) {
                                player.TotalWinnings -= player.Bet;
                                Dealer.TotalWinnings += player.Bet;
                                player.Bet = 0;
                                SendMessage($"{FormatMessage(Config.Blackjack.Message_Loss, player)}");
                                EndRound();
                            }
                            if(ImGui.IsItemHovered()) {
                                ImGui.SetTooltip("Calculate & optionally announce player's loss.");
                            }
                        } else if(playerValue <= 21 && playerValue == dealerValue) {
                            if(ImGui.Button("D###draw")) {
                                SendMessage($"{FormatMessage(Config.Blackjack.Message_Draw, player)}");
                                EndRound();
                            }
                            if(ImGui.IsItemHovered()) {
                                ImGui.SetTooltip("Announce player draw, offer push/refund.");
                            }
                            ImGui.SameLine();
                            ImGui.Checkbox("###playerPush", ref player.Blackjack.Pushed);
                            if(ImGui.IsItemHovered()) {
                                ImGui.SetTooltip("Push player's bet to next round.");
                            }
                        }
                    }
                }
                ImGui.NextColumn();

                //Profit
                ImGui.SetNextItemWidth(-1);
                ImGuiEx.InputText("###playerProfit", ref player.TotalWinnings);
                string areTheyOk = player.TotalWinnings < 0 ? "\nThis person is unlucky! :3" : player.TotalWinnings > 0 ? "\nThis person is too lucky! ;w;" : "";
                if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"The total profit/loss for this player.\nEmpty this field when the player stops playing.{areTheyOk}"); }
                ImGui.NextColumn();
            }
        }

        public void DrawConfigGameSetup() {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Game Rules");

            ImGui.Columns(3);
            ImGui.SetColumnWidth(0, 200 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(1, 200 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(2, 200 + 5 * ImGuiHelpers.GlobalScale);

            ImGui.Separator();

            ImGui.Text("Min Bet");
            ImGui.NextColumn();
            ImGui.Text("Max Bet");
            ImGui.NextColumn();
            ImGui.Text("Dealer Stands On");
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputInt("###minBet", Config.Blackjack, nameof(Config.Blackjack.MinBet));
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputInt("###maxBet", Config.Blackjack, nameof(Config.Blackjack.MaxBet));
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            if(ImGui.RadioButton("16", Config.Blackjack.DealerStandsOn == 16)) {
                Config.Blackjack.DealerStandsOn = 16;
            }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Why I included this as an option?\nI don't know.. I read it was an alternative rule, so.."); }
            ImGui.SameLine();
            if(ImGui.RadioButton("17", Config.Blackjack.DealerStandsOn == 17)) {
                Config.Blackjack.DealerStandsOn = 17;
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.SetNextItemWidth(-1);
            ImGuiEx.Checkbox($"Auto Double", Config.Blackjack, nameof(Config.Blackjack.AutoDouble));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Automatically double bet amount when clicking Double button.\nProbably never have to uncheck this.\n..unless you enjoy typing numbers yourself, nerd.");
            }
            ImGui.NextColumn();

            ImGui.SetNextItemWidth(-1);
            ImGuiEx.Checkbox($"Allow Bet on Push", Config.Blackjack, nameof(Config.Blackjack.PushAllowBet));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Whether a player with a pushed bet can bet again next round, adding to their pushed bet.");
            }
            ImGui.NextColumn();

            ImGui.SetNextItemWidth(-1);
            ImGuiEx.Checkbox($"Allow Double on Push", Config.Blackjack, nameof(Config.Blackjack.PushAllowDouble));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Whether a player with a pushed bet can double down next round, doubling their pushed bet.");
            }
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.SetNextItemWidth(-1);
            ImGui.Text("Normal Win Multiplier");
            ImGui.NextColumn();

            ImGui.SetNextItemWidth(-1);
            ImGui.Text("Blackjack Win Multiplier");
            ImGui.NextColumn();

            ImGui.SetNextItemWidth(-1);
            ImGuiHelpers.ScaledDummy(5);
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputFloat($"###normalWin", Config.Blackjack, nameof(Config.Blackjack.NormalWinMultiplier));
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Win multiplier for when a player wins with a hand less than 21."); }
            ImGui.NextColumn();

            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputFloat($"###blackjackWin", Config.Blackjack, nameof(Config.Blackjack.BlackjackWinMultiplier));
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Win multiplier for when a player wins with a blackjack.\n..will people even notice if we quietly reduce this to 2.0?\nI don't think lucky people deserve a bonus."); }
            ImGui.NextColumn();
        }

        public void DrawConfigMessages() {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Localization");
            ImGui.TextWrapped("Adjust text to be output for various events.\nKeywords: #player#, #dealer#, #minbet#, #maxbet#, #bet#, #cards#, #value#, #stand#, #winnings#, #profit#");

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 180 + 5 * ImGuiHelpers.GlobalScale);
            ImGui.SetColumnWidth(1, 500 + 5 * ImGuiHelpers.GlobalScale);

            ImGui.Separator();

            ImGui.Text("Descriptor");
            ImGui.NextColumn();
            ImGui.Text("Text");
            ImGui.NextColumn();

            ImGui.Separator();

            ImGui.PushID($"m_0");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlaceBets");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m0", Config.Blackjack, nameof(Config.Blackjack.Message_PlaceBets));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_00");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("BetsPlaced");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m00", Config.Blackjack, nameof(Config.Blackjack.Message_BetsPlaced));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_1");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerBet");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m1", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerBet));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_1_2");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerBetPushed");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m1_2", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerBetPushed));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_2");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerBetDouble");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m2", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerBetDouble));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_3");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerDraw2");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m3", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerDraw2));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_4");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerDraw2Blackjack");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m4", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerDraw2Blackjack));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_5");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerStandHit");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m5", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerStandHit));
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("This might look the same as PlayerHitUnder21\n..but it's not the same, trust me."); }
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_5_2");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerStandHitDouble");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m5_2", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerStandHitDouble));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_6");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerHitUnder21");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m6", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerHitUnder21));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_6_2");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerHitUnder21Doubled");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m6_2", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerHitUnder21Doubled));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_7");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerHit21");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m7", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerHit21));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_8");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("PlayerHitOver21");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m8", Config.Blackjack, nameof(Config.Blackjack.Message_PlayerHitOver21));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_9");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerDraw1");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m9", Config.Blackjack, nameof(Config.Blackjack.Message_DealerDraw1));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_10");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerDraw2UnderStand");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m10", Config.Blackjack, nameof(Config.Blackjack.Message_DealerDraw2UnderStand));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_11");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerDraw2Stand");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m11", Config.Blackjack, nameof(Config.Blackjack.Message_DealerDraw2Stand));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_12");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerDraw2Blackjack");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m12", Config.Blackjack, nameof(Config.Blackjack.Message_DealerDraw2Blackjack));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_13");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerHitUnderStand");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m13", Config.Blackjack, nameof(Config.Blackjack.Message_DealerHitUnderStand));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_14");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerHitStand");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m14", Config.Blackjack, nameof(Config.Blackjack.Message_DealerHitStand));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_15");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerHit21");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m15", Config.Blackjack, nameof(Config.Blackjack.Message_DealerHit21));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_16");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("DealerOver21");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m16", Config.Blackjack, nameof(Config.Blackjack.Message_DealerOver21));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_17");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("Win");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m17", Config.Blackjack, nameof(Config.Blackjack.Message_Win));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_17_2");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("Loss");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m17_2", Config.Blackjack, nameof(Config.Blackjack.Message_Loss));
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Should probably not let people know when they lose.\n..it might make them realize they're losing, you know?"); }
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_18");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("Draw");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m18", Config.Blackjack, nameof(Config.Blackjack.Message_Draw));
            ImGui.NextColumn(); ImGui.Separator();

            ImGui.PushID($"m_19");
            ImGui.SetNextItemWidth(-1);
            ImGui.Text("NoWinners");
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGuiEx.InputText("###m19", Config.Blackjack, nameof(Config.Blackjack.Message_NoWinners));
            ImGui.NextColumn(); ImGui.Separator();
        }

        public void DrawGuide() {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "How to be a Cute Dealer in 13 Easy Steps");
            ImGui.Separator();
            ImGui.TextWrapped("1. This plugin supports public play in /say chat as well as private /party play, this can be adjusted from the 'Main Config' tab. The default & recommended is /party play to avoid spamming public channels.");
            ImGui.Separator();
            ImGui.TextWrapped("2. On the Blackjack 'Config' tab, you can adjust game rules & customize what messages are output to chat for various events, you can also leave a message blank to output nothing for a specific event.");
            ImGui.Separator();
            ImGui.TextWrapped("3. On the Blackjack 'Game' tab, ensure the 'Enable' checkbox is checked, the Dealer Name is your own name (automatically set).\n'Alias' is a name you want the plugin to refer to you as, you will also have to manually set this for the participating players, doesn't have to be their full name.\nWhen a player is finished playing, you can simply delete their name & profit values to reset their seat.\nThe game logic also supports having players sit out a round, just keep their bet amount at 0 to ignore their seat.");
            ImGui.Separator();
            ImGui.TextWrapped("4. When everyone's name has been set, you can start a round by pressing the 'B' button in Dealer 'Bet Actions', this will send a message informing players that the round is starting & they'll need to place bets.");
            ImGui.Separator();
            ImGui.TextWrapped("5. Trade each player for their bet amount & manually add it to their 'Bet Amount' field, press the 'B' button in Player 'Bet Actions' to announce their bet amount.");
            ImGui.Separator();
            ImGui.TextWrapped("6. Press the 'F' button in Dealer 'Bet Actions' to announce all bets have been placed.");
            ImGui.Separator();
            ImGui.TextWrapped("7. For each player, press the '1' & '2' button in Player 'Card Actions' to draw 1st & 2nd card in turn.");
            ImGui.Separator();
            ImGui.TextWrapped("8. Press the '1' button in Dealer 'Card Actions' to draw 1st dealer card, do not draw 2nd dealer card yet.");
            ImGui.Separator();
            ImGui.TextWrapped("9. For each player, press the '?' button in Player 'Card Actions' to request them to Stand/Hit/Double.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Hit: Press the 'H' button in Player 'Card Actions' to draw another card, then request them to Hit/Stand if they don't bust.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Double: Trade the player for their bet amount again, then press the 'D' button in Player 'Bet Actions' to announce it & automatically update their 'Bet Amount' field, then request them to Hit/Stand.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Stand: No action required, move on to the next player.");
            ImGui.Separator();
            ImGui.TextWrapped("10. When all players have either stood or bust, press the '2' button in Dealer 'Card Actions' to draw 2nd dealer card.");
            ImGui.Separator();
            ImGui.TextWrapped("11. While dealer hand is under 17, press the 'H' button in Dealer 'Card Actions' to continue drawing.");
            ImGui.Separator();
            ImGui.TextWrapped("12. Win/Loss states are calculated automatically, press either of W/L/D buttons in Result Actions.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Win: (Dealer) - Available if all players have lost/drawn and announces as such.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Win: (Player) - Available if player beats dealer hand or dealer bust. Calculate & announce winnings which should be traded to them. This will also tally total wins/losses in the 'Profits' field for both player & dealer.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Draw: (Player) - Available if player matches dealer hand. Request Push/Refund, if they push click the checkbox to the right of the 'D' button. When a new round is started, their bet will be automatically set, press the 'P' button in 'Bet Actions' to announce their bet as a pushed bet that round.");
            ImGui.Separator();
            ImGui.TextWrapped(" - Loss: (Player) - Available if player is lower than dealer hand or bust. Tally total wins/losses in the 'Profits' field for both dealer & player, by default a loss is not announced as the loss message is blank.");
            ImGui.Separator();
            ImGui.TextWrapped("13. A new round can be started by pressing the 'B' button again in Dealer 'Bet Actions', automatically resetting cards & bet amounts (other than push).\nThe 'Reset' button can also be used to do the same thing, without announcing a new round starting.");
            ImGui.Separator();
        }
    }
}
