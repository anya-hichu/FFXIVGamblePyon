using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace GamblePyon {
    [Serializable]
    public class Config : IPluginConfiguration {
        public int Version { get; set; } = 0;
        public bool Debug { get; set; } = false;
        public string RollCommand { get; set; } = "/dice"; // /random
        public string ChatChannel { get; set; } = "/p"; // /s

        public BlackjackConfig Blackjack { get; set; } = new BlackjackConfig();

        [NonSerialized] private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            PluginInterface = pluginInterface;
        }

        public void Save() {
            PluginInterface!.SavePluginConfig(this);
        }
    }
}
