using NLog;
using Sandbox.Game;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace ALE_PlayerStats {

    [Category("factioncleanup")]
    public class FactionCleanupCommands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public PlayerStatsPlugin Plugin => (PlayerStatsPlugin)Context.Plugin;

        [Command("tag length", "With a Tag length > than the given number.")]
        [Permission(MyPromoteLevel.Admin)]
        public void CleanupTagLength(int length) {

            List<MyFaction> factions = new List<MyFaction>(MySession.Static.Factions.GetAllFactions());

            int numberFactionsDeleted = 0;

            foreach (var faction in factions) {

                if (faction.Tag.Length <= length)
                    continue;

                DeleteFaction(faction);

                numberFactionsDeleted++;
            }

            Context.Respond(numberFactionsDeleted + " factions deleted!");
            Log.Info(numberFactionsDeleted + " factions deleted!");
        }

        [Command("name", "Removes Faction with given name, wildcards are allowed.")]
        [Permission(MyPromoteLevel.Admin)]
        public void CleanupNamePattern(string pattern) {

            List<MyFaction> factions = new List<MyFaction>(MySession.Static.Factions.GetAllFactions());

            var regex = WildCardToRegular(pattern);

            int numberFactionsDeleted = 0;

            foreach (var faction in factions) {

                if (!Regex.IsMatch(faction.Name, regex)
                    && !Regex.IsMatch(faction.Tag, regex))
                    continue;

                DeleteFaction(faction);

                numberFactionsDeleted++;
            }

            Context.Respond(numberFactionsDeleted + " factions deleted!");
            Log.Info(numberFactionsDeleted + " factions deleted!");
        }

        private static string WildCardToRegular(string value) {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private void DeleteFaction(MyFaction faction) {

            List<long> memberIdentities = new List<long>(faction.Members.Keys);

            foreach (long memberIdentitiy in memberIdentities)
                MyVisualScriptLogicProvider.KickPlayerFromFaction(memberIdentitiy);
        }
    }
}
