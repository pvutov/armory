using IrisZoomDataApi.Model.Ndfbin;
using System;

namespace Armory {
    public class Unit : IComparable<Unit> {
        private String _qualifiedName;
        private String _name;
        private String _factory;
        private NdfObject _ndfHandle;

        public Unit(String country, String name, NdfObject ndfHandle) {
            _qualifiedName = country + " - " + name;
            _name = name;
            _ndfHandle = ndfHandle;

            // Get which tab the unit is in
            NdfPropertyValue factory;
            if (ndfHandle.TryGetProperty("Factory", out factory)) {
                String factoryString = factory.Value.ToString();
                switch (factoryString) {
                    case "3": _factory = "Logistic"; break;
                    case "6": _factory = "Infantry"; break;
                    case "7": _factory = "Plane"; break;
                    case "8": _factory = "Vehicle"; break;
                    case "9": _factory = "Tank"; break;
                    case "10": _factory = "Recon"; break;
                    case "11": _factory = "Helo"; break;
                    case "12": _factory = "Wasted dev time"; break;
                    case "13": _factory = "Support"; break;
                    default:  _factory = factoryString; break;
                }
            }
        }

        /// <summary>
        /// The short name is the name as displayed to the player.
        /// </summary>
        /// <returns></returns>
        public String shortName {
            get { return _name; }
        }

        /// <summary>
        /// The unit name prefixed with its country 
        /// [as displayed in the search box]. 
        /// </summary>
        /// <returns></returns>
        public String qualifiedName {
            get { return _qualifiedName; }
            set { _qualifiedName = value; }
        }

        /// <summary>
        /// Get the deck category the unit is under.
        /// </summary>
        /// <returns></returns>
        public String category {
            get { return _factory; }
        }

        public NdfObject handle {
            get { return _ndfHandle; }
        }

        public override int GetHashCode() {
            return qualifiedName.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is Unit) {
                return qualifiedName.Equals( ((Unit) obj).qualifiedName);
            } else {
                return false;
            }
        }

        public override string ToString() {
            return qualifiedName;
        }

        public int CompareTo(Unit other) {
            return qualifiedName.CompareTo(other.qualifiedName);
        }
    }
}
