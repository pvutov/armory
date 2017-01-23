using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;
using IrisZoomDataApi.Model.Ndfbin.Types.AllTypes;

namespace Armory {

    /// <summary>
    /// Technically TAmmunition
    /// </summary>
    public class Weapon {
        private int turretIndex, weaponIndex;
        private NdfObject weaponHandle;
        private string name;

        public Weapon(TradManager dictionary, NdfObject weapon, int turretIndex, int weaponIndex) {
            weaponHandle = weapon;
            this.turretIndex = turretIndex;
            this.weaponIndex = weaponIndex;

            NdfLocalisationHash hash;
            if (weapon.TryGetValueFromQuery<NdfLocalisationHash>("Name", out hash)) {
                dictionary.TryGetString(hash.Value, out name);
            }
        }

        public int getTurretIndex() {
            return turretIndex;
        }

        public int getWeaponIndex() {
            return weaponIndex;
        }

        public NdfObject getHandle() {
            return weaponHandle;
        }

        public string getName() {
            return name;
        }

        override public string ToString() {
            return name;
        }
    }
}
