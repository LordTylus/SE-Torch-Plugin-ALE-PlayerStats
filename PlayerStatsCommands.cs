using ALE_Core.Attribute;
using ALE_Core.Attribute.Filter;
using ALE_Core.Attribute.Sorting;
using NLog;
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

    public class PlayerStatsCommands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public PlayerStatsPlugin Plugin => (PlayerStatsPlugin)Context.Plugin;

        [Command("listplayers", "Lists all Players in your world and allows for some filtering and sorting.")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListPlayers() {

            List<string> args = Context.Args;

            List<AttributeSortRule> sortRules = new List<AttributeSortRule>();
            List<AbstractAttributeFilterRule> filterRules = new List<AbstractAttributeFilterRule>();

            for (int i = 0; i < args.Count; i++) {

                if (args[i] == "-online")
                    filterRules.Add(new StandardAttributeFilterRule("online", AttributeRelation.EQUALS, true));

                if (args[i] == "-players")
                    filterRules.Add(new StandardAttributeFilterRule("real", AttributeRelation.EQUALS, true));

                if (args[i] == "-npcs")
                    filterRules.Add(new StandardAttributeFilterRule("real", AttributeRelation.EQUALS, false));

                if (args[i].StartsWith("-faction="))
                    filterRules.Add(new StandardAttributeFilterRule("faction", AttributeRelation.EQUALS, args[i].Replace("-faction=", "")));

                if (args[i].StartsWith("-filter=")) {

                    string filter = args[i].Replace("-filter=", "").Trim();

                    var definiton = AttributeFilterFactory.GetFilterDefinition(filter);

                    if(definiton == null) {
                        Context.Respond("Could not parse '"+filter+"'. Ignoring!");
                        continue;
                    }

                    string attribute = definiton.Attribute;

                    if (attribute != "name"
                        && attribute != "date"
                        && attribute != "faction"
                        && attribute != "blocks"
                        && attribute != "pcu") {

                        Context.Respond("You can only filter 'name', 'date', 'faction', 'pcu' or blocks! Combinations are possible. Ignoring!");
                        continue;
                    }

                    if(attribute == "name" || attribute == "faction")
                        filterRules.Add(new StandardAttributeFilterRule(attribute, definiton.Relation, definiton.Value));

                    if (attribute == "blocks" || attribute == "pcu") {

                        if (int.TryParse(definiton.Value, out int intValue)) {

                            filterRules.Add(new StandardAttributeFilterRule(attribute, definiton.Relation, intValue));

                        } else {

                            Context.Respond("Could not parse '" + definiton.Value + "'. Ignoring!");
                            continue;
                        }
                    }

                    if (attribute == "date") {

                        if (int.TryParse(definiton.Value, out int intValue)) {

                            DateTime now = DateTime.Now;
                            now = now.AddDays(-intValue);
                                
                            filterRules.Add(new StandardAttributeFilterRule(attribute, definiton.Relation, now));

                        } else {

                            Context.Respond("Could not parse '" + definiton.Value + "' only an integer amount of days is allowed. Ignoring!");
                            continue;
                        }
                    }
                }

                if (args[i].StartsWith("-orderby=")) {

                    string orderby = args[i].Replace("-orderby=", "");

                    if (orderby != "name"
                        && orderby != "date"
                        && orderby != "faction"
                        && orderby != "blocks"
                        && orderby != "pcu") {

                        Context.Respond("You can only order by 'name', 'date', 'faction', 'pcu' or blocks! Combinations are possible");
                        return;
                    }

                    sortRules.Add(new AttributeSortRule(orderby));
                }
            }

            ListPlayers(sortRules, filterRules);
        }

        private void ListPlayers(List<AttributeSortRule> sortRules, List<AbstractAttributeFilterRule> filterRules) {

            var playerList = MySession.Static.Players;
            var factionList = MySession.Static.Factions;

            var identities = MySession.Static.Players.GetAllIdentities();

            List<PlayerEntry> entries = new List<PlayerEntry>();

            foreach (var identity in identities) {

                long id = identity.IdentityId;

                bool isReal = !playerList.IdentityIsNpc(id);
                bool isOnline = playerList.IsPlayerOnline(id);
                
                var faction = factionList.GetPlayerFaction(id);
                string tag = faction != null ? faction.Tag : "";

                /* can be 0 for NPCs */
                ulong steamId = playerList.TryGetSteamId(id);
                var lastLogoutTime = identity.LastLogoutTime;
                var lastLoginTime = identity.LastLoginTime;

                var date = lastLogoutTime;
                if (lastLoginTime > lastLogoutTime)
                    date = lastLoginTime;

                var blockLimits = identity.BlockLimits;
                
                entries.Add(new PlayerEntry(isReal, isOnline, identity.DisplayName, id, steamId, tag, date, blockLimits.BlocksBuilt, blockLimits.PCUBuilt));
            }

            AttributeFilterer.Filter(entries, filterRules);
            AttributeSorter.Sort(entries, sortRules);

            StringBuilder sb = new StringBuilder();

            foreach(PlayerEntry entry in entries) {

                string factionName = "";

                if (entry.factionTag != null)
                    factionName = " [" + entry.factionTag + "]";

                sb.AppendLine((entry.name + factionName).PadRight(20) + " - Date: " + entry.date);
                sb.AppendLine("    Block: " + entry.blocks + ", PCU: " + entry.pcu);
                sb.AppendLine("    EntityId: " + entry.id + ", SteamId: " + entry.steamId);
            }

            if (Context.Player == null) {

                Context.Respond("Players");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("Players", "", sb.ToString()), Context.Player.SteamUserId);
            }
        }
    }
}
