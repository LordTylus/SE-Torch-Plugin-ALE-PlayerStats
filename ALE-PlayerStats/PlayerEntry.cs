using ALE_Core.Attribute;
using System;

namespace ALE_PlayerStats {

    public class PlayerEntry : IAttributeAware {

        public readonly bool isReal;
        public readonly bool isOnline;
        public readonly string name;
        public readonly long id;
        public readonly ulong steamId;
        public readonly string factionTag;
        public readonly DateTime date;
        public readonly int blocks;
        public readonly int pcu;

        public PlayerEntry(bool isReal, bool isOnline, string name, long id, ulong steamId, string factionTag, DateTime date, int blocks, int pcu) {
            this.isReal = isReal;
            this.isOnline = isOnline;
            this.name = name;
            this.id = id;
            this.steamId = steamId;
            this.factionTag = factionTag;
            this.date = date;
            this.blocks = blocks;
            this.pcu = pcu;
        }

        public IComparable GetValueOf(string attribute) {

            switch(attribute) {

                case "real":
                    return isReal;
                case "online":
                    return isOnline;
                case "name":
                    return name;
                case "id":
                    return id;
                case "steamId":
                    return steamId;
                case "faction":
                    return factionTag;
                case "date":
                    return date;
                case "blocks":
                    return blocks;
                case "pcu":
                    return pcu;
                default:
                    return null;
            }
        }

        public Type GetValueType(string attribute) {

            switch (attribute) {

                case "real":
                    return typeof(bool);
                case "online":
                    return typeof(bool);
                case "name":
                    return typeof(string);
                case "id":
                    return typeof(long);
                case "steamId":
                    return typeof(ulong);
                case "faction":
                    return typeof(string);
                case "date":
                    return typeof(DateTime);
                case "blocks":
                    return typeof(int);
                case "pcu":
                    return typeof(int);
                default:
                    return null;
            }
        }
    }
}
