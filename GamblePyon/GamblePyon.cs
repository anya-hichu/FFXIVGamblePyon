using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using XivCommon;
using Dalamud.Game.ClientState;

namespace GamblePyon {
    public sealed class GamblePyon : IDalamudPlugin {
        public string Name => "GamblePyon";
        private const string CommandName = "/pyon";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static CommandManager CommandManager { get; private set; }
        [PluginService] public static ChatGui ChatGui { get; private set; }
        [PluginService] public static ClientState ClientState { get; private set; }

        private WindowSystem Windows;
        private static MainWindow MainWindow;

        public static XivCommonBase XIVCommon;

        public GamblePyon() {
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open Blackjack Interface."
            });

            PluginInterface.UiBuilder.OpenConfigUi += () => {
                MainWindow.IsOpen = true;
            };

            XIVCommon = new XivCommonBase();
            Windows = new WindowSystem(Name);
            MainWindow = new MainWindow(this) { IsOpen = false };
            MainWindow.Config = PluginInterface.GetPluginConfig() as Config ?? new Config();
            MainWindow.Config.Initialize(PluginInterface);
            Windows.AddWindow(MainWindow);

            PluginInterface.UiBuilder.Draw += Windows.Draw;
            ChatGui.ChatMessage += MainWindow.OnChatMessage;
        }

        public void Dispose() {
            PluginInterface.UiBuilder.Draw -= Windows.Draw;
            ChatGui.ChatMessage -= MainWindow.OnChatMessage;
            CommandManager.RemoveHandler(CommandName);
            XIVCommon.Dispose();
        }

        private void OnCommand(string command, string args) {
            MainWindow.IsOpen = true;
        }
    }
}