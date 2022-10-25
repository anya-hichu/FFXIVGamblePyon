using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using GamblePyon.Games;
using Dalamud.Interface.Colors;
using GamblePyon.Extensions;

namespace GamblePyon {
    public class MainWindow : Window {
        private readonly GamblePyon plugin;
        public static Config Config { get; set; }

        private MainTab CurrentMainTab = MainTab.Blackjack;
        private enum MainTab { Blackjack, Config }

        private SubTab CurrentSubTab = SubTab.BlackjackGame;
        private enum SubTab { BlackjackGame, BlackjackConfig, BlackjackGuide }

        private string Messages = "";

        private Blackjack Blackjack;

        public MainWindow(GamblePyon plugin) : base("GamblePyon") {
            this.SizeCondition = ImGuiCond.Appearing;
            this.Size = new Vector2(680, 500) * ImGuiHelpers.GlobalScale;
            this.plugin = plugin;
        }

        public override void OnOpen() {
            if(Blackjack == null) {
                Blackjack = new Blackjack();
                Blackjack.Config = Config;
                Blackjack.Send_Message += Blackjack_SendMessage;
            }

            base.OnOpen();
        }

        private void Blackjack_SendMessage(object? sender, MessageEventArgs e) => SendMessage(e.Message, e.MessageType);

        private List<string> QueuedMessages = new List<string>();
        private void SendMessage(string message, MessageType messageType) {
            if(!string.IsNullOrWhiteSpace(message)) {
                if(messageType == MessageType.Normal) {
                    QueuedMessages.Add($"{(Config.Debug ? "/echo" : Config.ChatChannel)} {message}");
                } else if(messageType == MessageType.BlackjackRoll) {
                    int.TryParse(message, out int num);

                    if(Config.Debug) {
                        string n = (new System.Random().Next(num == 0 ? Config.Blackjack.MaxRoll : num) + 1).ToString();
                        string s = Config.ChatChannel == "/p" ? $"Random! (1-{num}) {n}" : $"Random! You roll a {n} (out of {num}).";
                        ReceivedMessage(Blackjack.Dealer.Name, s);
                    } else {
                        QueuedMessages.Add($"{Config.RollCommand} {(num == 0 ? Config.Blackjack.MaxRoll : num)}");
                    }
                }
            }
        }

        public void OnChatMessage(XivChatType type, uint senderId, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled) {
            if(isHandled) { return; }

            ReceivedMessage(sender.TextValue, message.TextValue);

            //Messages += $"[{type}] [{senderId}] [{sender.TextValue}] [{message.TextValue}]\n";
        }

        private void ReceivedMessage(string sender, string message) {
            if(Blackjack != null && Blackjack.Enabled) {
                Blackjack.OnChatMessage(sender, message);
            }
        }

        public override void Draw() {
            DrawMainTabs();

            switch(CurrentMainTab) {
                case MainTab.Blackjack: {
                        DrawBlackjackSubTabs();
                        break;
                    }
                case MainTab.Config: {
                        DrawMainConfig();
                        break;
                    }
                default:
                    DrawBlackjackSubTabs();
                    break;
            }

            if(QueuedMessages.Count > 0) {
                GamblePyon.XIVCommon.Functions.Chat.SendMessage(QueuedMessages[0]);
                QueuedMessages.RemoveAt(0);
            }
        }

        private void DrawMainTabs() {
            if(ImGui.BeginTabBar("GamblePyonMainTabBar", ImGuiTabBarFlags.NoTooltip)) {
                if(ImGui.BeginTabItem("Blackjack###GamblePyon_Blackjack_MainTab")) {
                    CurrentMainTab = MainTab.Blackjack;
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Main Config###GamblePyon_Config_MainTab")) {
                    CurrentMainTab = MainTab.Config;
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
                ImGui.Spacing();
            }
        }

        private void DrawMainConfig() {
            ImGui.Text("Channel");

            if(ImGui.RadioButton("Party", Config.ChatChannel == "/p")) {
                Config.ChatChannel = "/p";
                Config.RollCommand = "/dice";
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play in private /p chat using /dice roll.");
            }
            ImGui.SameLine();
            if(ImGui.RadioButton("Say", Config.ChatChannel == "/s")) {
                Config.ChatChannel = "/s";
                Config.RollCommand = "/random";
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Play in public /s chat using /random roll.");
            }

            ImGui.Separator();
            ImGuiEx.Checkbox("Debug", Config, nameof(Config.Debug));
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Output messages to /echo chat & roll internally.\nThis is just here so I can test things without spamming chat!");
            }

            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            ConfigButtons();
        }

        private void DrawBlackjackSubTabs() {
            if(ImGui.BeginTabBar("BlackjackSubTabBar", ImGuiTabBarFlags.NoTooltip)) {
                if(ImGui.BeginTabItem("Game###GamblePyon_BlackjackGame_SubTab")) {
                    CurrentSubTab = SubTab.BlackjackGame;
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Config###GamblePyon_BlackjackConfig_SubTab")) {
                    CurrentSubTab = SubTab.BlackjackConfig;
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Guide###GamblePyon_BlackjackGuide_SubTab")) {
                    CurrentSubTab = SubTab.BlackjackGuide;
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
                ImGui.Spacing();
            }

            switch(CurrentSubTab) {
                case SubTab.BlackjackGame: {
                        DrawBlackjackGame();
                        break;
                    }
                case SubTab.BlackjackConfig: {
                        DrawBlackjackConfig();
                        break;
                    }
                case SubTab.BlackjackGuide: {
                        DrawBlackjackGuide();
                        break;
                    }
                default:
                    DrawBlackjackConfig();
                    break;
            }
        }

        private void DrawBlackjackGame() {
            //ImGui.InputTextMultiline("###input", ref Messages, 1000, new Vector2(500, 300));
            //ImGui.Separator();
            //ImGuiHelpers.ScaledDummy(5);

            Blackjack.DrawDealer();

            ImGui.Separator();
            ImGui.PopID();
            ImGui.Columns(1);
            ImGui.Separator();

            Blackjack.DrawPlayerList();

            ImGui.Separator();
            ImGui.PopID();
            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if(ImGui.Button("Reset")) {
                Blackjack.ResetRound();
            }
            ImGui.SameLine();
            if(ImGui.Button("Close")) {
                IsOpen = false;
            }
        }

        private void DrawBlackjackConfig() {
            Blackjack.DrawConfigGameSetup();

            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            ConfigButtons();

            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            Blackjack.DrawConfigMessages();

            ImGui.PopID();
            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            ConfigButtons();
        }

        private void DrawBlackjackGuide() {
            Blackjack.DrawGuide();

            ImGui.Columns(1);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if(ImGui.Button("Close")) {
                IsOpen = false;
            }
        }

        private void ConfigButtons() {
            if(ImGui.Button("Save")) {
                Config.Save();
            }

            ImGui.SameLine();

            if(ImGui.Button("Close")) {
                IsOpen = false;
            }
        }
    }
}
