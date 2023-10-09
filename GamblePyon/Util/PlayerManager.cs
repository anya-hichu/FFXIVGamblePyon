using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using GamblePyon.Models;

using DalamudPartyMember = Dalamud.Game.ClientState.Party.PartyMember;
using StructsPartyMember = FFXIVClientStructs.FFXIV.Client.Game.Group.PartyMember;

namespace GamblePyon {
    public unsafe class PlayerManager /* : IDisposable */ {
        private const int GroupMemberOffset = 0x0CC8;
        private const int AllianceMemberOffset = 0x0E14;
        private const int AllianceSizeOffset = 0x0EB4;
        private const int GroupMemberSize = 0x20;
        private const int GroupMemberIdOffset = 0x18;

        public PlayerManager() {

        }


        public void UpdateParty(ref List<Player> players, string dealerName, NameMode nameMode) {

            List<Player> partyMembers = new List<Player>();

            int groupMemberCount = GroupManager.Instance()->MemberCount;
            for (int i = 0; i < groupMemberCount; i++)
            {
                StructsPartyMember* memberStruct = GroupManager.Instance()->GetPartyMemberByIndex(i);
                DalamudPartyMember? partyMember = GamblePyon.PartyList.CreatePartyMemberReference(new IntPtr(memberStruct));

                if (partyMember == null) { continue; }
                if (partyMember.Name.TextValue == dealerName) { continue; }
                Player newPlayer = new Player((int)partyMember.ObjectId, partyMember.Name.TextValue);
                newPlayer.Alias = newPlayer.GetAlias(nameMode);
                partyMembers.Add(newPlayer);
            }

            foreach(Player player in players) {
                if(partyMembers.Find(x => x.ID == player.ID) == null) {
                    player.Name = "";
                }
            }

            players.RemoveAll(player => player.Name == "");

            foreach(Player partyMember in partyMembers) {
                if(players.Find(x => x.ID == partyMember.ID) == null) {
                    players.Add(partyMember);
                }
            }
        }        
    }
}
