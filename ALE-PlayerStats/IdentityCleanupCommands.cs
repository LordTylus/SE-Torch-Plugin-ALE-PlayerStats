using ALE_Core.Utils;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace ALE_PlayerStats {

    [Category("identitycleanup")]
    public class IdentityCleanupCommands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public PlayerStatsPlugin Plugin => (PlayerStatsPlugin)Context.Plugin;

        [Command("name", "Removes Identities with given name, wildcards are allowed.")]
        [Permission(MyPromoteLevel.Admin)]
        public void CleanupNamePattern(string pattern) {

            List<MyIdentity> identities = new List<MyIdentity>(MySession.Static.Players.GetAllIdentities());

            var regex = WildCardToRegular(pattern);

            int numberOfIdentitiesDeleted = 0;
            int deletedGrids = 0;

            Dictionary<long, List<MyCubeGrid>> grids = GetGridsPerOwner();
            HashSet<long> removedPlayers = new HashSet<long>();

            foreach (var identity in identities) {

                if (!Regex.IsMatch(PlayerUtils.GetDisplayNameWithoutIcon(identity), regex))
                    continue;

                long IdentityId = identity.IdentityId;

                MyVisualScriptLogicProvider.KickPlayerFromFaction(IdentityId);
                MySession.Static.Players.RemoveIdentity(IdentityId);

                if (grids.ContainsKey(IdentityId)) {

                    List<MyCubeGrid> gridsForOwner = grids[IdentityId];

                    foreach (var grid in gridsForOwner) {

                        if (grid.BigOwners.Contains(IdentityId)) {
                            grid.Close();
                            deletedGrids++;
                        }
                    }
                }

                removedPlayers.Add(identity.IdentityId);

                numberOfIdentitiesDeleted++;
            }

            GridUtils.TransferBlocksToBigOwner(removedPlayers);

            Context.Respond($"Deleted {removedPlayers.Count} players and {deletedGrids} grids!");
            Log.Info($"Deleted {removedPlayers.Count} players and {deletedGrids} grids!");
        }

        private static Dictionary<long, List<MyCubeGrid>> GetGridsPerOwner() {

            List<MyCubeGrid> grids = MyEntities.GetEntities().OfType<MyCubeGrid>().ToList();

            Dictionary<long, List<MyCubeGrid>> dictionary = new Dictionary<long, List<MyCubeGrid>>();

            foreach (var grid in grids) {

                var owners = grid.BigOwners;

                foreach (var owner in owners) {

                    if (!dictionary.ContainsKey(owner)) 
                        dictionary[owner] = new List<MyCubeGrid>(); ;

                    dictionary[owner].Add(grid);
                }
            }

            return dictionary;
        }

        private static string WildCardToRegular(string value) {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
