using IrisZoomDataApi.Model.Ndfbin;
using System;

namespace Armory {
    class Unit {
        private String _qualifiedName;
        private String _name;
        private String _factory;
        private NdfObject _ndfHandle;

        public Unit(String country, String name, String factory, NdfObject ndfHandle) {
            _qualifiedName = country + " - " + name;
            _name = name;
            _factory = factory;
            _ndfHandle = ndfHandle;
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
    }
}
