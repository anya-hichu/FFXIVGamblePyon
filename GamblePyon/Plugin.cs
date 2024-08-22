using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace GamblePyon
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "GamblePyon";
        private const string CommandName = "/pyon";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IObjectTable Objects { get; private set; } = null!;
        [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static IPartyList PartyList { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;

        private WindowSystem Windows { get; init; }
        private static MainWindow MainWindow = null!;

        public static Chat Chat = null!;

        public Plugin()
        {
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Blackjack Interface."
            });

            PluginInterface.UiBuilder.OpenMainUi += OpenMainWindow;
            PluginInterface.UiBuilder.OpenConfigUi += OpenMainWindow;

            Chat = new Chat(SigScanner);
            Windows = new WindowSystem(Name);
            MainWindow = new MainWindow(this) { IsOpen = false };
            MainWindow.Config = PluginInterface.GetPluginConfig() as Config ?? new Config();
            MainWindow.Config.Initialize(PluginInterface);
            Windows.AddWindow(MainWindow);
            MainWindow.Initialize();

            PluginInterface.UiBuilder.Draw += Windows.Draw;
            ChatGui.ChatMessage += MainWindow.OnChatMessage;
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.Draw -= Windows.Draw;
            ChatGui.ChatMessage -= MainWindow.OnChatMessage;
            MainWindow.Dispose();
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            MainWindow.IsOpen = true;
        }
    }
}
