using ALE_Core.Attribute;
using System;
using VRage.Game;

namespace ALE_PlayerStats {

    public class ReputationEntry : IAttributeAware {

        public readonly int reputation;

        public readonly string faction;
        public readonly string name;

        public readonly MyRelationsBetweenFactions relation;

        public ReputationEntry(int reputation, string faction, string name, MyRelationsBetweenFactions relation) {
            this.reputation = reputation;
            this.faction = faction;
            this.name = name;
            this.relation = relation;
        }

        public IComparable GetValueOf(string attribute) {

            switch (attribute) {

                case "reputation":
                    return reputation;
                case "faction":
                    return faction;
                case "name":
                    return name;
                case "relation":
                    return relation;
                default:
                    return null;
            }
        }

        public Type GetValueType(string attribute) {

            switch (attribute) {

                case "reputation":
                    return typeof(int);
                case "faction":
                    return typeof(string);
                case "name":
                    return typeof(string);
                case "relation":
                    return typeof(MyRelationsBetweenFactions);
                default:
                    return null;
            }
        }



    }
}
