using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using GamblePyon;
using GlamblePyon.model;

public class PlayerManager
{
    private const int GroupMemberOffset = 3272;

    private const int AllianceMemberOffset = 3604;

    private const int AllianceSizeOffset = 3764;

    private const int GroupMemberSize = 32;

    private const int GroupMemberIdOffset = 24;

    public unsafe void UpdateParty(ref List<Player> players, string dealerName, NameMode nameMode)
    {
        List<Player> partyMembers = [];
        int groupMemberCount = GroupManager.Instance()->MainGroup.MemberCount;
        for (int i = 0; i < groupMemberCount; i++)
        {
            PartyMember* memberStruct = GroupManager.Instance()->MainGroup.GetPartyMemberByIndex(i);
            IPartyMember partyMember = Plugin.PartyList.CreatePartyMemberReference(new IntPtr(memberStruct))!;
            if (partyMember != null && !(partyMember.Name.TextValue == dealerName))
            {
                Player newPlayer = new Player((int)partyMember.ObjectId, partyMember.Name.TextValue);
                newPlayer.Alias = newPlayer.GetAlias(nameMode);
                partyMembers.Add(newPlayer);
            }
        }
        foreach (Player player2 in players)
        {
            if (partyMembers.Find((Player x) => x.ID == player2.ID) == null)
            {
                player2.Name = "";
            }
        }
        players.RemoveAll((Player player) => player.Name == "");
        foreach (Player partyMember2 in partyMembers)
        {
            if (players.Find((Player x) => x.ID == partyMember2.ID) == null)
            {
                players.Add(partyMember2);
            }
        }
    }
}
