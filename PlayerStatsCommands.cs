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

            string factionTag = null;
            bool online = false;
            bool playerOnly = false;
            string orderby = "name";

            for (int i = 0; i < args.Count; i++) {

                if (args[i] == "-online")
                    online = true;

                if (args[i] == "-players")
                    playerOnly = true;

                if (args[i].StartsWith("-faction="))
                    factionTag = args[i].Replace("-faction=", "");

                if (args[i].StartsWith("-orderby=")) {

                    orderby = args[i].Replace("-orderby=", "");

                    if (orderby != "name"
                        && orderby != "date"
                        && orderby != "faction") {

                        Context.Respond("You can only order by 'name' 'date' or 'faction'!");
                        return;
                    }
                }
            }

            ListPlayers(factionTag, online, playerOnly, orderby);
        }

        private void ListPlayers(string factionTag, bool online, bool playerOnly, string orderby) {

            var playerList = MySession.Static.Players;
            var factionList = MySession.Static.Factions;

            var identities = MySession.Static.Players.GetAllIdentities();

            List<PlayerEntry> entries = new List<PlayerEntry>();

            foreach (var identity in identities) {

                long id = identity.IdentityId;

                /* filter NPCs if demanded */
                if (playerOnly && playerList.IdentityIsNpc(id))
                    continue;

                /* Filter offline if demanded */
                if (online && !playerList.IsPlayerOnline(id))
                    continue;

                var faction = factionList.GetPlayerFaction(id);

                /* Filter faction if player has one */
                if (factionTag != null && factionTag != "" && (faction == null || faction.Tag != factionTag))
                    continue;

                /* Filter factionless people if demanded */
                if (factionTag == "" && faction != null)
                    continue;

                /* can be 0 for NPCs */
                ulong steamId = playerList.TryGetSteamId(id);
                var lastLogoutTime = identity.LastLogoutTime;
                var lastLoginTime = identity.LastLoginTime;

                var date = lastLogoutTime;
                if (lastLoginTime > lastLogoutTime)
                    date = lastLoginTime;

                var tag = faction == null ? null : faction.Tag;

                entries.Add(new PlayerEntry(identity.DisplayName, id, steamId, tag, date));
            }

            if(orderby != null) {

                entries.Sort(delegate (PlayerEntry entry1, PlayerEntry entry2) {

                    /* Nullsafety on entry */
                    if (entry1 == null) {

                        if (entry2 == null)
                            return 0;
                        else
                            return 1;

                    } else if (entry2 == null)
                        return -1;

                    if (orderby == "date") {

                        if (entry1.date == null) {

                            if (entry2.date == null)
                                return 0;
                            else
                                return 1;

                        } else if (entry2.date == null)
                            return -1;

                        int result = entry1.date.CompareTo(entry2.date);

                        if (result != 0)
                            return result;
                    }

                    if (orderby == "faction") {

                        if (entry1.factionTag == null) {

                            if (entry2.factionTag == null)
                                return 0;
                            else
                                return 1;

                        } else if (entry2.factionTag == null)
                            return -1;

                        int result = entry1.factionTag.CompareTo(entry2.factionTag);

                        if (result != 0)
                            return result;
                    }

                    /* Fallback for name */
                    if (entry1.name == null) {

                        if (entry2.name == null)
                            return 0;
                        else
                            return 1;

                    } else if (entry2.name == null)
                        return -1;

                    return entry1.name.CompareTo(entry2.name);
                });
            }

            StringBuilder sb = new StringBuilder();

            foreach(PlayerEntry entry in entries) {

                string factionName = "";

                if (entry.factionTag != null)
                    factionName = " [" + entry.factionTag + "]";

                sb.AppendLine((entry.name + factionName).PadRight(20) + " - Date: " + entry.date);
                sb.AppendLine("    EntityId: " + entry.id + ", SteamId: " + entry.steamId);
            }

            if (Context.Player == null) {

                Context.Respond("Players");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("Players", "", sb.ToString()), Context.Player.SteamUserId);
            }
        }

        private class PlayerEntry {

            public readonly string name;
            public readonly long id;
            public readonly ulong steamId;
            public readonly string factionTag;
            public readonly DateTime date;

            public PlayerEntry(string name, long id, ulong steamId, string factionTag, DateTime date) {
                this.name = name;
                this.id = id;
                this.steamId = steamId;
                this.factionTag = factionTag;
                this.date = date;
            }
        }
    }
}
