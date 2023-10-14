using ALE_Core.Attribute.Sorting;
using ALE_Core.Utils;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game.ModAPI;

namespace ALE_PlayerStats {

    [Category("reputation")]
    public class ReputationCommands : CommandModule {

        [Command("list player", "Lists the Reputation one Player has with every Faction on the server.")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListReputationPlayer(string playerName) {

            var identity = PlayerUtils.GetIdentityByName(playerName);

            if(identity == null) {
                Context.Respond("Player not found!");
                return;
            }

            var factions = MySession.Static.Factions;
            List<ReputationEntry> relationEntries = new List<ReputationEntry>();

            foreach (var faction in factions.Factions.Values) {

                var relation = factions.GetRelationBetweenPlayerAndFaction(identity.IdentityId, faction.FactionId);

                relationEntries.Add(new ReputationEntry(relation.Item2, faction.Tag, playerName, relation.Item1));
            }

            List<AttributeSortRule> sortRules = new List<AttributeSortRule> {
                new AttributeSortRule("reputation", AttributeSortDirection.DESCENDING),
                new AttributeSortRule("name")
            };

            AttributeSorter.Sort(relationEntries, sortRules);

            StringBuilder sb = new StringBuilder();

            foreach(ReputationEntry entry in relationEntries) 
                sb.AppendLine(entry.faction + " - " + entry.reputation.ToString("#,##0") + " - " + entry.relation);

            if (Context.Player == null) {

                Context.Respond("Reputations Player Faction");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("Reputations Player Faction", "For player "+identity.DisplayName, sb.ToString()), Context.Player.SteamUserId);
            }
        }

        [Command("list faction", "Lists the Reputation one Faction has with every Player on the server.")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListReputationFaction(string factionTag) {

            var faction = FactionUtils.GetIdentityByTag(factionTag);

            if (faction == null) {
                Context.Respond("Faction not found!");
                return;
            }

            var factions = MySession.Static.Factions;
            var players = MySession.Static.Players;
            List<ReputationEntry> relationEntries = new List<ReputationEntry>();

            foreach (var player in players.GetAllIdentities()) {

                var relation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

                relationEntries.Add(new ReputationEntry(relation.Item2, faction.Tag, player.DisplayName, relation.Item1));
            }

            List<AttributeSortRule> sortRules = new List<AttributeSortRule> {
                new AttributeSortRule("reputation", AttributeSortDirection.DESCENDING),
                new AttributeSortRule("name")
            };

            AttributeSorter.Sort(relationEntries, sortRules);

            StringBuilder sb = new StringBuilder();

            foreach (ReputationEntry entry in relationEntries)
                sb.AppendLine(entry.reputation.ToString("#,##0") + " - " + entry.name  + " - " + entry.relation);

            if (Context.Player == null) {

                Context.Respond("Reputations Faction Player");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("Reputations Faction Player", "For Faction " + factionTag, sb.ToString()), Context.Player.SteamUserId);
            }
        }

        [Command("list factions", "Lists the Reputation one Faction has with every Faction on the server.")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListReputationFactions(string factionTag) {

            var faction = FactionUtils.GetIdentityByTag(factionTag);

            if (faction == null) {
                Context.Respond("Faction not found!");
                return;
            }

            var factions = MySession.Static.Factions;
            List<ReputationEntry> relationEntries = new List<ReputationEntry>();

            foreach (var faction2 in factions.Factions.Values) {

                var relation = factions.GetRelationBetweenFactions(faction.FactionId, faction2.FactionId);

                relationEntries.Add(new ReputationEntry(relation.Item2, faction.Tag, faction2.Tag, relation.Item1));
            }

            List<AttributeSortRule> sortRules = new List<AttributeSortRule> {
                new AttributeSortRule("reputation", AttributeSortDirection.DESCENDING),
                new AttributeSortRule("name")
            };

            AttributeSorter.Sort(relationEntries, sortRules);

            StringBuilder sb = new StringBuilder();

            foreach (ReputationEntry entry in relationEntries)
                sb.AppendLine(entry.name + " - " + entry.reputation.ToString("#,##0") + " - " + entry.relation);

            if (Context.Player == null) {

                Context.Respond("Reputations Between Factions");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("Reputations Between Factions", "For Faction " + factionTag, sb.ToString()), Context.Player.SteamUserId);
            }
        }

        [Command("change player", "Adds the given reputation between the given player and faction. It can be negative to remove reputation.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void ChangeReputationPlayer(string playerName, string factionTag, int reputationDelta) {

            var faction = FactionUtils.GetIdentityByTag(factionTag);

            if (faction == null) {
                Context.Respond("Faction not found!");
                return;
            }

            var player = PlayerUtils.GetIdentityByName(playerName);

            if (player == null) {
                Context.Respond("Player not found!");
                return;
            }

            var factions = MySession.Static.Factions;

            var currentReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

            factions.AddFactionPlayerReputation(player.IdentityId, faction.FactionId, reputationDelta, false, true);

            var newReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

            Context.Respond("Changed reputation between " + faction.Tag + " and " + player.DisplayName + " from " + currentReputation.Item2.ToString("#,##0") + " to " + newReputation.Item2.ToString("#,##0"));
        }

        [Command("change faction", "Adds the given reputation of all players from faction 1 to faction 2. It can be negative to remove reputation.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void ChangeReputationPlayers(string factionTag1, string factionTag2, int reputationDelta) {

            var faction1 = FactionUtils.GetIdentityByTag(factionTag1);

            if (faction1 == null) {
                Context.Respond("Faction1 not found!");
                return;
            }

            var faction2 = FactionUtils.GetIdentityByTag(factionTag2);

            if (faction2 == null) {
                Context.Respond("Faction2 not found!");
                return;
            }

            var factions = MySession.Static.Factions;

            foreach (long playerId in faction1.Members.Keys) 
                factions.AddFactionPlayerReputation(playerId, faction2.FactionId, reputationDelta, false, true);

            Context.Respond("Added reputation " + reputationDelta.ToString("#,##0") + " to all Players of Faction " + faction1.Tag + " to " + faction2.Tag);
        }

        [Command("change debugfaction", "Adds the given reputation between the given factions. It can be negative to remove reputation. (Use with caution)")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void ChangeReputationFaction(string factionTag1, string factionTag2, int reputationDelta) {

            var faction1 = FactionUtils.GetIdentityByTag(factionTag1);

            if (faction1 == null) {
                Context.Respond("Faction1 not found!");
                return;
            }

            var faction2 = FactionUtils.GetIdentityByTag(factionTag2);

            if (faction2 == null) {
                Context.Respond("Faction2 not found!");
                return;
            }

            var factions = MySession.Static.Factions;

            var currentReputation = factions.GetRelationBetweenFactions(faction1.FactionId, faction2.FactionId);

            factions.SetReputationBetweenFactions(faction1.FactionId, faction2.FactionId, currentReputation.Item2 + reputationDelta);

            var newReputation = factions.GetRelationBetweenFactions(faction1.FactionId, faction2.FactionId);

            Context.Respond("Changed reputation between " + faction1.Tag + " and " + faction2.Tag + " from " + currentReputation.Item2.ToString("#,##0") + " to " + newReputation.Item2.ToString("#,##0"));
        }

        [Command("set player", "Sets the given reputation between the given player and faction.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void SetReputationPlayer(string playerName, string factionTag, int reputation) {

            var faction = FactionUtils.GetIdentityByTag(factionTag);

            if (faction == null) {
                Context.Respond("Faction not found!");
                return;
            }

            var player = PlayerUtils.GetIdentityByName(playerName);

            if (player == null) {
                Context.Respond("Player not found!");
                return;
            }

            var factions = MySession.Static.Factions;

            var currentReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

            factions.AddFactionPlayerReputation(player.IdentityId, faction.FactionId, (reputation - currentReputation.Item2), false, true);

            var newReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

            Context.Respond("Changed reputation between " + faction.Tag + " and " + player.DisplayName + " from " + currentReputation.Item2.ToString("#,##0") + " to " + newReputation.Item2.ToString("#,##0"));
        }

        [Command("set playerallfactions", "Sets the given reputation between the given player and all factions.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void SetReputationPlayerAllFactions(string playerName, int reputation) {

            var player = PlayerUtils.GetIdentityByName(playerName);

            if (player == null) {
                Context.Respond("Player not found!");
                return;
            }

            var factions = MySession.Static.Factions;
            var newReputation = 0;

            foreach (var localFaction in factions.Factions.Values) {

                var currentReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, localFaction.FactionId);

                factions.AddFactionPlayerReputation(player.IdentityId, localFaction.FactionId, (reputation - currentReputation.Item2), false, true);

                newReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, localFaction.FactionId).Item2;
            }

            Context.Respond("Changed reputation of player " + player.DisplayName + " with all factions to " + newReputation.ToString("#,##0"));
        }

        [Command("set faction", "Sets the given reputation of all players from faction 1 to faction 2.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void SetReputationPlayers(string factionTag1, string factionTag2, int reputation) {

            var faction1 = FactionUtils.GetIdentityByTag(factionTag1);

            if (faction1 == null) {
                Context.Respond("Faction1 not found!");
                return;
            }

            var faction2 = FactionUtils.GetIdentityByTag(factionTag2);

            if (faction2 == null) {
                Context.Respond("Faction2 not found!");
                return;
            }

            var factions = MySession.Static.Factions;
            var newReputation = 0;

            foreach (long playerId in faction1.Members.Keys) {

                var currentReputation = factions.GetRelationBetweenPlayerAndFaction(playerId, faction2.FactionId);

                factions.AddFactionPlayerReputation(playerId, faction2.FactionId, (reputation - currentReputation.Item2), false, true);

                newReputation = factions.GetRelationBetweenPlayerAndFaction(playerId, faction2.FactionId).Item2;
            }

            Context.Respond("Set reputation of all player of faction " + faction1.Tag + " with " + faction2.Tag + " to " + newReputation.ToString("#,##0"));
        }

        [Command("set factionallplayers", "Sets the given reputation of all players with passed on faction.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void SetReputationFactionAllPlayers(string factionTag, int reputation) {

            var faction = FactionUtils.GetIdentityByTag(factionTag);

            if (faction == null) {
                Context.Respond("Faction not found!");
                return;
            }

            var players = MySession.Static.Players;
            var factions = MySession.Static.Factions;
            
            foreach (var player in players.GetAllIdentities()) {

                var currentReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

                factions.AddFactionPlayerReputation(player.IdentityId, faction.FactionId, (reputation - currentReputation.Item2), false, true);
            }

            Context.Respond("Set reputation of faction " + faction.Tag + " with all players to "+ reputation.ToString("#,##0"));
        }

        [Command("set allreputations", "Sets the given reputation of all players with all factions.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void SetAllReputations(int reputation) {

            var players = MySession.Static.Players;
            var factions = MySession.Static.Factions;

            foreach (var player in players.GetAllIdentities()) {

                foreach (var faction in factions.GetAllFactions()) {

                    var currentReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

                    factions.AddFactionPlayerReputation(player.IdentityId, faction.FactionId, (reputation - currentReputation.Item2), false, true);
                }
            }

            Context.Respond("Set reputation of all factions with all players to " + reputation.ToString("#,##0"));
        }

        [Command("reset", "Resets all reputations back to default.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void reset() {

            var players = MySession.Static.Players;
            var factions = MySession.Static.Factions;

            foreach (var faction in factions.GetAllFactions()) {

                int reputation = -1000;

                if (faction.IsEveryoneNpc() && faction.Tag != "SPRT" && faction.Tag != "SPID")
                    reputation = 0;

                foreach (var player in players.GetAllIdentities()) {

                    var currentReputation = factions.GetRelationBetweenPlayerAndFaction(player.IdentityId, faction.FactionId);

                    factions.AddFactionPlayerReputation(player.IdentityId, faction.FactionId, (reputation - currentReputation.Item2), false, true);
                }
            }

            Context.Respond("All Reputations reset!");
        }

        [Command("set debugfaction", "Sets the given reputation between the given factions.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void SetReputationFaction(string factionTag1, string factionTag2, int reputation) {

            var faction1 = FactionUtils.GetIdentityByTag(factionTag1);

            if (faction1 == null) {
                Context.Respond("Faction1 not found!");
                return;
            }

            var faction2 = FactionUtils.GetIdentityByTag(factionTag2);

            if (faction2 == null) {
                Context.Respond("Faction2 not found!");
                return;
            }

            var factions = MySession.Static.Factions;

            var currentReputation = factions.GetRelationBetweenFactions(faction1.FactionId, faction2.FactionId);

            factions.SetReputationBetweenFactions(faction1.FactionId, faction2.FactionId, reputation);

            var newReputation = factions.GetRelationBetweenFactions(faction1.FactionId, faction2.FactionId);

            Context.Respond("Changed reputation between " + faction1.Tag + " and " + faction2.Tag + " from " + currentReputation.Item2.ToString("#,##0") + " to " + newReputation.Item2.ToString("#,##0"));
        }
    }
}
