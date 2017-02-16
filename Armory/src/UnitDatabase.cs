using System;
using System.Collections.Generic;
using System.Linq;
using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;
using IrisZoomDataApi.Model.Ndfbin.Types.AllTypes;
using System.Drawing;
using static Armory.Utility;

namespace Armory {
    /// <summary>
    /// Stores the handles of all units in the NDF binary. Holds the logic for searching the members of units as well as the units themselves.
    /// </summary>
    public class UnitDatabase {
        /// <summary>
        /// Unit handles are queried by unit alias.
        /// </summary>
        private Dictionary<String, Unit> aliasToUnitObject = new Dictionary<String, Unit>();

        // -------------- For getting unit name lists ------------

        // All units:
        private List<Unit> allUnits = new List<Unit>();
        // By faction:
        private List<Unit> natoUnits = new List<Unit>();
        private List<Unit> pactUnits = new List<Unit>();
        // By country:
        private Dictionary<String, List<Unit>> countryToUnitAlias = new Dictionary<String, List<Unit>>();

        //---------------- --------------------- ----------------

        private List<String> countryList;
        
        private TradManager dictionary;

        private EdataManager unitCards;
        private String pactPrefix;
        private String natoPrefix;

        private NdfObject queryTarget;
        private List<Weapon> weapons;
        private Weapon currentWeapon;
        private NdfObject currentWeaponHandle;
        private Weapon lockedWeapon;

        private const int HE_VALUE_ARME = 3;
        private const int KE_THRESHOLD_ARME = 4;
        private const int HEAT_THRESHOLD_ARME = 34;

        public UnitDatabase(List<NdfObject> unitInstances, TradManager dictionary, EdataManager iconPackage, String pactDir, String natoDir) {
            // The main job of this constructor is to
            // populate a list of all countries, of pact and nato countries, 
            // and of all units (by looking up their names in the localisation file)

            this.dictionary = dictionary;
            this.unitCards = iconPackage;
            this.pactPrefix = pactDir;
            this.natoPrefix = natoDir;

            foreach (var unitInstance in unitInstances) {
                NdfLocalisationHash hash;
                string unitName;

                if (unitInstance.TryGetValueFromQuery<NdfLocalisationHash>("NameInMenuToken", out hash)) {
                    if (dictionary.TryGetString(hash.Value, out unitName)) {
                        if (unitName == null) {
                            // discard null entry
                        }

                        else {
                            NdfPropertyValue countryName;

                            // Add unit under its name under its country
                            if (unitInstance.TryGetProperty("MotherCountry", out countryName)) {
                                String countryNameString = countryName.Value.ToString(); // TODO: error case if value is of type NdfNull

                                Unit unit = new Unit(countryNameString, unitName, unitInstance);
                                String unitNameString = unit.qualifiedName;

                                List<Unit> countryContents;
                                if (countryToUnitAlias.TryGetValue(countryNameString, out countryContents)) {

                                    // If there are duplicate units within the same country, pad the names of the duplicates
                                    while (aliasToUnitObject.ContainsKey(unitNameString)) {
                                        // TODO: Notify user when this happens
                                        Console.WriteLine("padded unit because duplicate name: " + unitNameString);
                                        unitNameString += "*";
                                    }

                                    countryContents.Add(unit);
                                    aliasToUnitObject.Add(unitNameString, unit);
                                }

                                // If country doesn't exist yet, add it
                                else {
                                    aliasToUnitObject.Add(unitNameString, unit);
                                    countryContents = new List<Unit>();
                                    countryContents.Add(unit);
                                    countryToUnitAlias.Add(countryNameString, countryContents);
                                }

                                allUnits.Add(unit);

                                // Add to pact or nato unit list; using the Nationalite field, which seems to consitently be null for blue units, but is suspicious
                                NdfPropertyValue nationalite;
                                if (unitInstance.TryGetProperty("Nationalite", out nationalite)) {
                                    if (nationalite.Value is NdfNull) {
                                        natoUnits.Add(unit);
                                    }
                                    else {
                                        pactUnits.Add(unit);
                                    }
                                }
                                else {
                                    Program.warning("No nationalite, so omitted from nato/pact list : " + unitNameString + ".");
                                }
                            }

                            else {
                                Program.warning("Skipped unit because no mother country: " + unitName + ".");
                            }
                        }
                    }
                }
            }

            // Sort all the lists alphabetically:
            foreach (var country in countryToUnitAlias) {
                country.Value.Sort();
            }
            allUnits.Sort();
            pactUnits.Sort();
            natoUnits.Sort();

            countryList = countryToUnitAlias.Keys.ToList();
            countryList.Sort();
            countryList.Insert(0, "PACT");
            countryList.Insert(0, "NATO");
            countryList.Insert(0, "All");
        }

        /// <summary>
        /// UnitDatabase can only be used by one window because of stateful fields like currentWeapon.
        /// <para/>Yet, the dictionaries and data objects in it are too big to be directly copied.
        /// <para/>This constructor is meant to replicate the provided UnitDatabase, copying all references except for stateful, window-specific variables.
        /// </summary>
        /// <param name="father"></param>
        private UnitDatabase(UnitDatabase father) {
            aliasToUnitObject = father.aliasToUnitObject;
            allUnits = father.allUnits;
            countryList = father.countryList;
            countryToUnitAlias = father.countryToUnitAlias;
            dictionary = father.dictionary;
            natoPrefix = father.natoPrefix;
            natoUnits = father.natoUnits;
            pactPrefix = father.pactPrefix;
            pactUnits = father.pactUnits;
            unitCards = father.unitCards;
        }

        public UnitDatabase clone() {
            return new UnitDatabase(this);
        }

        /// <summary>
        /// Get all existing countries.
        /// </summary>
        /// <returns>A list of countries.</returns>
        public List<String> getAllCountries() {
            return countryList;
        }

        /// <summary>
        /// Get a list of all units in some faction.
        /// </summary>
        /// <param name="faction">Some faction, either a country name or one of the PACT/NATO/All constants.</param>
        /// <returns>A list of units.</returns>
        public List<Unit> getUnitList(String faction) {
            List<Unit> units;

            switch (faction) {
                case "All": return allUnits;
                case "NATO": return natoUnits;
                case "PACT": return pactUnits;
                default:
                    if (countryToUnitAlias.TryGetValue(faction, out units)) {
                        return units;
                    }
                    // Should never be triggered
                    else {
                        Program.warning(faction + " Faction not found.");
                        return allUnits;
                    }
            }
        }

        /// <summary>
        /// For grouping queries to the same unit. Yes I know that premature optimization is the root of all evil..
        /// </summary>
        /// <param name="name"> The unit the database will be answering questions about. </param>
        /// <returns> True if succeeded, false if target not found. </returns>
        public bool setQueryTarget(String name) {
            Unit tmpUnit;
            bool result = aliasToUnitObject.TryGetValue(name, out tmpUnit);
            if (result) {
                queryTarget = tmpUnit.handle;
            }

            // Create a list of all weapons and the turrets they belong to
            weapons = new List<Weapon>();
            NdfValueWrapper weapon = null;

            for (int i = 0; ; i++) {
                // Queries that ask for a list element fail, so we have to dig as deep as Turrets[0].Weapons[0].Ammunition to find out of Turrets[i] even exists
                if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.WeaponManager.Default.TurretDescriptorList[" + i.ToString() + "].MountedWeaponDescriptorList[0].Ammunition", out weapon)) {
                    weapons.Add(new Weapon(dictionary, ((NdfObjectReference)weapon).Instance, i, 0));
                }
                else {
                    break;
                }

                // Get all weapons under this turret
                int j = 1;
                while (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.WeaponManager.Default.TurretDescriptorList[" + i.ToString() + "].MountedWeaponDescriptorList[" + j + "].Ammunition", out weapon)) {
                    weapons.Add(new Weapon(dictionary, ((NdfObjectReference)weapon).Instance, i, j));
                    j++; ;
                }
            }

            return result;
        }

        public void setCurrentWeapon(Weapon weap) {
            currentWeapon = weap;

            if (weap != null) {
                currentWeaponHandle = weap.getHandle();
            }
            else {
                currentWeaponHandle = null;
            }
        }

        /// <summary>
        /// Try to get the weapon in the position indexed by the lock slot checkbox.
        /// </summary>
        public Weapon tryGetLockIndexedWeapon() {
            if (lockedWeapon != null) {
                int turret = lockedWeapon.getTurretIndex();
                int slot = lockedWeapon.getWeaponIndex();

                foreach (Weapon w in getWeapons()) {
                    if (w.getTurretIndex() == turret
                        && w.getWeaponIndex() == slot) {

                        return w;
                    }
                }
            }

            return lockedWeapon;
        }

        /// <summary>
        /// The database will remember the weapon's position and
        /// try to default to it when unit selection changes.
        /// </summary>
        /// <returns>A string characterizing the weapon's position.</returns>
        public string lockWeapon() {
            lockedWeapon = currentWeapon;

            return lockedWeapon.getTurretIndex().ToString()
                + ":" + lockedWeapon.getWeaponIndex().ToString();
        }

        public void unlockWeapon() {
            lockedWeapon = null;
        }

        // Non-modules --------------------
        #region
        public Bitmap getUnitCard() {
            Bitmap result = null;

            NdfPropertyValue val;
            if (queryTarget.TryGetProperty("ClassNameForDebug", out val)) {
                String debugName = val.Value.ToString();
                // Omit "Unit_" prefix and match to commoninterface.ppk naming format
                debugName = debugName.Substring(5).ToLower();

                // Icons have different dir depending on faction
                // Instead of looking up faction which may fail etc, just try both
                if (unitCards.TryToLoadTgv(pactPrefix + debugName + ".tgv", out result)) {
                    return result;
                }
                else {
                    unitCards.TryToLoadTgv(natoPrefix + debugName + ".tgv", out result);
                    return result;
                }
            }

            return result;
        }

        public String getUnitName() {

            NdfLocalisationHash hash;
            String result;
            if (queryTarget.TryGetValueFromQuery<NdfLocalisationHash>("NameInMenuToken", out hash)) {
                if (dictionary.TryGetString(hash.Value, out result)) {
                    return result;
                }
            }

            return "idk";
        }


        public bool isPrototype() {

            NdfPropertyValue prototype;
            if (queryTarget.TryGetProperty("IsPrototype", out prototype)) {
                String value = prototype.Value.ToString();

                if (value == "True") {
                    return true;
                }
                else if (value == "null") {
                    return false;
                }

            }

            Program.warning("Prototype status could not be parsed.");
            return false;
        }

        public String getPrice() {
            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("ProductionPrice[0]", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        /// <summary>
        /// Extract the values from an ndf list into an "|"-delimited string
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private String linearizeList(NdfCollection list) {
            String result = "";

            foreach (CollectionItemValueHolder item in list.InnerList) {
                if (result != "") {
                    result += "|";
                }

                result += item.Value.ToString();
            }

            return result;
        }

        public String[] getMaxDeployableAmount() {

            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("MaxDeployableAmount", out val)) {

                return linearizeList(val).Split('|');
            }

            return null;
        }

        public String getSize() {
            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("HitRollSizeModifier", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public String getECM() {
            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("HitRollECMModifier", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public String doCustomQuery(String query) {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>(query, out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        // END Non-modules ----------------
        #endregion

        // Position module ----------------
        #region
        public String getNearGroundFlyingAltitude() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Position.Default.NearGroundFlyingAltitude", out val)) {

                result = divideBy52AndAppendMeters(val.ToString());
            }

            return result;
        }

        public String getLowAltitudeFlyingAltitude() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Position.Default.LowAltitudeFlyingAltitude", out val)) {

                result = divideBy52AndAppendMeters(val.ToString());
            }

            return result;
        }
        // END Position module --------
        #endregion

        // WeaponManager module -----------
        #region
        public List<Weapon> getWeapons() {
            return weapons;
        }

        public Bitmap getWeaponPicture() {
            Bitmap result = null;

            NdfValueWrapper val;
            if (currentWeaponHandle != null) { 
                if (currentWeaponHandle.TryGetValueFromQuery("InterfaceWeaponTexture.FileName", out val)) {
                    String resourcePath = val.ToString();
                    // Omit "GameData:" prefix and match to commoninterface.ppk naming format
                    resourcePath = @"pc\texture" + resourcePath.Substring("GameData:".Length).ToLower().Replace("/", @"\").Replace(".png", ".tgv");
                    
                    // Icons have different dir depending on faction
                    // Instead of looking up faction which may fail etc, just try both
                    if (unitCards.TryToLoadTgv(resourcePath, out result)) {
                        return result;
                    }
                }
            }
            return result;
        }


        public String getAmmo() {
            if (currentWeapon != null && currentWeaponHandle != null) {
                NdfValueWrapper val;
                string salvesIndex;
                if (queryTarget.TryGetValueFromQuery("Modules.WeaponManager.Default.TurretDescriptorList[" + currentWeapon.getTurretIndex().ToString() + "].MountedWeaponDescriptorList[" + currentWeapon.getWeaponIndex() + "].SalvoStockIndex", out val)) {
                    salvesIndex = val.ToString();
                }
                else {
                    salvesIndex = "0";
                }

                if (queryTarget.TryGetValueFromQuery("Modules.WeaponManager.Default.Salves[" + salvesIndex + "]", out val)) {
                    string salvos = val.ToString();

                    return multiplyStrings(salvos, getSalvoLength()) + " shots";
                }
            }

            return "idk";
        }

        public String getWeaponTurret() {
            if (currentWeapon != null)
                return currentWeapon.getTurretIndex().ToString();
            else
                return "idk";
        }

        public String getSupplyCost() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("SupplyCost", out val)) {
                    String result = val.ToString();

                    return divideStrings(result, getSalvoLength()) + "/shot";
                }

            return "idk";
        }

        public String getAccuracy() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("HitRollRule.HitProbability", out val)) {
                    String result = val.ToString();

                    return fractionToPercent(result);
                }

            return "idk";
        }

        public String getMinAccuracy() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) {
                if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("HitRollRule.MinimalHitProbability", out val)) {
                    String result = val.ToString();

                    return fractionToPercent(result);
                }
            }

            return "idk";
        }

        public String getStabilizer() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null && currentWeapon != null) {

                // Ignore stabilizer and return '-' if TirEnMouvement = false
                if (queryTarget.TryGetValueFromQuery("Modules.WeaponManager.Default.TurretDescriptorList[" + currentWeapon.getTurretIndex().ToString() + "].MountedWeaponDescriptorList[" + currentWeapon.getWeaponIndex() + "].TirEnMouvement", out val)) {
                    if (val.ToString() == "True") {
                        if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("HitRollRule.HitProbabilityWhileMoving", out val)) {
                            String result = val.ToString();

                            return fractionToPercent(result);
                        }
                        // if we're not finding the stabilizer, dunno is more appropriate than '-'
                        else {
                            return "idk";
                        }
                    }
                }

                return "-";
            }

            return "idk";
        }
        public String getMinCritChance() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) {
                if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("HitRollRule.MinimalCritProbability", out val)) {
                    String result = val.ToString();

                    return fractionToPercent(result);
                }
            }

            return "idk";
        }

        public String getMaxDispersion() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) {
                if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("DispersionAtMaxRange", out val)) {
                    
                    return divideBy52(val.ToString());
                }
            }

            return "idk";
        }

        public String getMinDispersion() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("DispersionAtMinRange", out val)) {
                    
                    return divideBy52(val.ToString());
                }

            return "idk";
        }

        public String getSuppression() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("SuppressDamages", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getSuppressionSplash() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("RadiusSplashSuppressDamages", out val)) {
                    String result = val.ToString();

                    return divideBy52(result);
                }

            return "idk";
        }

        public String getHeSplash() {

            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("RadiusSplashPhysicalDamages", out val)) {
                    String result = val.ToString();

                    return divideBy52(result);
                }

            return "idk";
        }

        public String getHE() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PhysicalDamages", out val)) {
                    String result = val.ToString();

                    string ap = getAP();
                    if (result == "1" && ap != "idk" && ap != "-") {
                        result = "-";
                    }
                    return result;
                }

            return "idk";
        }

        public String getFireChance() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("FireTriggeringProbability", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getAP() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("Arme", out val)) {
                    String result = val.ToString();

                    // Arme = 3 => HE
                    if (result == HE_VALUE_ARME.ToString()) {
                        return "-";
                    }

                    int transform;
                    if (int.TryParse(result, out transform)) {
                        if (transform > HEAT_THRESHOLD_ARME) {
                            transform -= HEAT_THRESHOLD_ARME;
                        }
                        else if (transform > KE_THRESHOLD_ARME) {
                            transform -= KE_THRESHOLD_ARME;
                        }                        
                    }

                    result = transform.ToString();
                    return result;
                }

            return "idk";
        }

        public String getTags() {
            String result = "";
            NdfValueWrapper val;

            // SEAD/RAD tags are in Guidance
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("Guidance", out val)) {
                    switch (val.ToString()) {
                        case "1": result += "[RAD] "; break;
                        case "2": result += "[SEAD] "; break;
                        default: break;
                    }
                }

            // SA/F&F/GUID
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("IsFireAndForget", out val)) {
                    if (val.ToString() == "True") {
                        result += "[F&F] ";
                    }
                    else if (val.ToString() == "False" && queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.WeaponManager.Default.TurretDescriptorList[" + currentWeapon.getTurretIndex().ToString() + "].MountedWeaponDescriptorList[" + currentWeapon.getWeaponIndex().ToString() + "].TirEnMouvement", out val)) {
                        if (val.ToString() == "True") {
                            result += "[SA] ";
                        }
                        else {
                            result += "[GUID] ";
                        }
                    }
                }

            // AoE
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("Arme", out val)) {
                    if (val.ToString() == "3") {
                        result += "[AoE] ";
                    }
                }

            // KE/HEAT
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("Arme", out val)) {
                    int intVal;
                    if (int.TryParse(val.ToString(), out intVal)) {
                        if (intVal > HEAT_THRESHOLD_ARME) {
                            result += "[HEAT] ";
                        }
                        else if (intVal > KE_THRESHOLD_ARME) {
                            result += "[KE] ";
                        }
                    }
                }

            // CQC
            if (getGroundRange() == "1") {
                result += "[CQC] ";
            }

            // INDIR
            if (getTirIndirect() == "True") {
                result += "[INDIR]";
            }

            return result;
        }

        public String getGroundRange() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PorteeMaximale", out val)) {
                    String result = val.ToString();
                    int maxRange, minRange;
                    if (int.TryParse(result, out maxRange)) {
                        maxRange = (maxRange * 175) / 13000;
                        result = maxRange.ToString();
                    }

                    // prepend min range
                    if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PorteeMinimale", out val)) {

                            if (int.TryParse(val.ToString(), out minRange)) {
                                minRange = (minRange * 175) / 13000;
                                result = minRange.ToString() + "m" + " to " + maxRange.ToString();
                            }

                        }
                    return result + "m";
                }

            return "idk";
        }

        public String getHeloRange() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PorteeMaximaleTBA", out val)) {
                    String result = val.ToString();
                    int maxRange, minRange;
                    if (int.TryParse(result, out maxRange)) {
                        maxRange = (maxRange * 175) / 13000;
                        result = maxRange.ToString();
                    }

                    // prepend min range
                    if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PorteeMinimaleTBA", out val)) {

                            if (int.TryParse(val.ToString(), out minRange)) {
                                minRange = (minRange * 175) / 13000;
                                result = minRange.ToString() + "m" + " to " + maxRange.ToString();
                            }

                        }
                    return result + "m";
                }

            return "idk";
        }

        public String getPlaneRange() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PorteeMaximaleHA", out val)) {
                    String result = val.ToString();
                    int maxRange, minRange;
                    if (int.TryParse(result, out maxRange)) {
                        maxRange = (maxRange * 175) / 13000;
                        result = maxRange.ToString();
                    }

                    // prepend min range
                    if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("PorteeMinimaleHA", out val)) {

                            if (int.TryParse(val.ToString(), out minRange)) {
                                minRange = (minRange * 175) / 13000;
                                result = minRange.ToString() + "m" + " to " + maxRange.ToString();
                            }

                        }
                    return result + "m";
                }

            return "idk";
        }

        public String getNoise() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("NoiseDissimulationMalus", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getSalvoLength() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("NbTirParSalves", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getShotReload() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("TempsEntreDeuxTirs", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getShotReloadPostprocessed() {
            if (getSalvoLength() == "1") {
                return "-";
            }
            else {

                NdfValueWrapper val;
                if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("TempsEntreDeuxTirs", out val)) {
                        String result = val.ToString();

                        return result;
                    }

                return "idk";
            }
        }

        public String getSalvoReload() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("TempsEntreDeuxSalves", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getROF() {
            string salvoLength = getSalvoLength();
            string salvoReload = getSalvoReload();
            string shotReload = getShotReload();

            double salvoLengthDbl, salvoReloadDbl, shotReloadDbl;

            if (double.TryParse(salvoLength, out salvoLengthDbl) &&
                double.TryParse(salvoReload, out salvoReloadDbl) &&
                double.TryParse(shotReload, out shotReloadDbl)) {

                double result = (60 * salvoLengthDbl) / (salvoReloadDbl + (salvoLengthDbl - 1) * shotReloadDbl);
                return result.ToString("0.##/min");
            }

            return "idk";
        }

        public String getRandomDispersion() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("RandomDispersion", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getMissileTimeBetweenCorrections() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("MissileTimeBetweenCorrections", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getAngleDispersion() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("AngleDispersion", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getTirReflexe() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("TirReflexe", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getTirIndirect() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("TirIndirect", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getPuissance() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("Puissance", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getAimTime() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("TempsDeVisee", out val)) {
                    String result = val.ToString();

                    return result;
                }

            return "idk";
        }

        public String getMissileMaxSpeed() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("MissileDescriptor.Modules.MouvementHandler.Maxspeed", out val)) {

                    return divideBy52(val.ToString());
                }

            return "idk";
        }

        public String getMissileMaxAcceleration() {
            NdfValueWrapper val;
            if (currentWeaponHandle != null) if (currentWeaponHandle.TryGetValueFromQuery<NdfValueWrapper>("MissileDescriptor.Modules.MouvementHandler.MaxAcceleration", out val)) {

                    return divideBy52(val.ToString());
                }

            return "idk";
        }

        // END WeaponManager module -------
        #endregion

        // Experience module --------------
        #region
        public String getKillExperienceBonus() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Experience.Default.KillExperienceBonus", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        // END Experience module ----------
        #endregion

        // Visibility module --------------
        #region
        public String getStealth() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Visibility.Default.UnitStealthBonus", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        // END Visibility module ----------
        #endregion

        // ScannerConfiguration module ----
        #region
        public String getGroundOptics() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.ScannerConfiguration.Default.OpticalStrength", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public String getAirOptics() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.ScannerConfiguration.Default.OpticalStrengthAltitude", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public String getAntiheloSpottingCap() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.ScannerConfiguration.Default.DetectionTBA", out val)) {
                String result = val.ToString();
                int resultInt;
                if (int.TryParse(result, out resultInt)) {
                    resultInt = (resultInt * 175) / 13000;
                    result = resultInt.ToString() + "m";
                }

                return result;
            }

            return "idk";
        }

        public String getAntigroundSpottingCap() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.ScannerConfiguration.Default.PorteeVision", out val)) {
                String result = val.ToString();
                int resultInt;
                if (int.TryParse(result, out resultInt)) {
                    resultInt = (resultInt * 175) / 13000;
                    result = resultInt.ToString() + "m";
                }

                return result;
            }

            return "idk";
        }

        public String getAntiplaneSpottingCap() {

            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.ScannerConfiguration.Default.SpecializedDetections", out val)) {
                String result = val.ToString();
                NdfMap w;
                foreach (CollectionItemValueHolder h in val.InnerList) {
                    w = (NdfMap) h.Value;
                    if (w.Key.Value.ToString() == "4") {
                        result = ((MapValueHolder)w.Value).Value.ToString();
                    }
                }

                int resultInt;
                if (int.TryParse(result, out resultInt)) {
                    resultInt = (resultInt * 175) / 13000;
                    result = resultInt.ToString() + "m";
                }

                return result;
            }

            return "idk";
        }

        public String getAntigroundSpottingCapWhileFlying() {

            NdfMapList val;
            if (queryTarget.TryGetValueFromQuery<NdfMapList>("Modules.ScannerConfiguration.Default.PorteeVisionTBA", out val)) {
                //val.GetMap("4")
                String result = val.ToString();
                int resultInt;
                if (int.TryParse(result, out resultInt)) {
                    resultInt = (resultInt * 175) / 13000;
                    result = resultInt.ToString() + "m";
                }

                return result;
            }

            return "idk";
        }

        public String getOpticalStrengthAntiRadar() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.ScannerConfiguration.Default.OpticalStrengthAntiradar", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public String getUnitType() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.ScannerConfiguration.Default.UnitType", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        // END ScannerConfiguration moduke ------
        #endregion

        // Scanner module -----------------
        #region

        public String getTimeBetweenEachIdentifyRoll() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.TimeBetweenEachIdentifyRoll", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public String getIdentifyBaseProbability() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.IdentifyBaseProbability", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        // END Scanner module -------------
        #endregion

        // MouvementHandler module --------
        #region
        public String getSpeed() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.Maxspeed", out val)) {

                result = divideBy52(val.ToString());
            }

            return result;
        }

        public string getTempsDemiTour() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.TempsDemiTour", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public string getMaxAcceleration() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.MaxAcceleration", out val)) {

                result = divideBy52AndAppendMeters(val.ToString());
            }

            return result;
        }

        public string getMaxDeceleration() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.MaxDeceleration", out val)) {

                result = divideBy52AndAppendMeters(val.ToString());
            }

            return result;
        }

        public string getUnitMovingType() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.UnitMovingType", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public string getVehicleSubType() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.VehicleSubType", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }


        public string getFlyingAltitude() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.FlyingAltitude", out val)) {

                result = divideBy52AndAppendMeters(val.ToString());
            }

            return result;
        }


        public string getMinimalAltitude() {
            String result = "idk";

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.MouvementHandler.Default.MinimalAltitude", out val)) {

                result = divideBy52AndAppendMeters(val.ToString());
            }

            return result;
        }
        // END MouvementHandler module -----
        #endregion

        // Damage module ------------------
        #region
        public string getHealth() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.MaxDamages", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        public string transformArmor(string armor) {
            int val;
            if (int.TryParse(armor, out val)) {
                string suffix = "";
                if (val > 4) {
                    val -= 4;
                }
                else if (val > 0) {
                    val = 0;
                    suffix = " Splash Resist type " + armor;
                }
                armor = val.ToString() + suffix;

            }
            return armor;
        }
        public string getFrontArmor() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.BlindageProperties.ArmorDescriptorFront.BaseBlindage", out val)) {
                String result = transformArmor(val.ToString());

                return result;
            }

            return "idk";
        }
        public string getSideArmor() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.BlindageProperties.ArmorDescriptorSides.BaseBlindage", out val)) {
                String result = transformArmor(val.ToString());

                return result;
            }

            return "idk";
        }
        public string getRearArmor() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.BlindageProperties.ArmorDescriptorRear.BaseBlindage", out val)) {
                String result = transformArmor(val.ToString());

                return result;
            }

            return "idk";
        }
        public string getTopArmor() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.BlindageProperties.ArmorDescriptorTop.BaseBlindage", out val)) {
                String result = transformArmor(val.ToString());

                return result;
            }

            return "idk";
        }
        public string getMaxSuppressionDamages() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.MaxSuppressionDamages", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        public string getStunDamagesRegen() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.StunDamagesRegen", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        public string getStunDamagesToGetStunned() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.StunDamagesToGetStunned", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesRegenRatioOutOfRange() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesRegenRatioOutOfRange", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesRegenRatio() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesRegenRatio", out val)) {
                String result = "soon!";
                //foreach (CollectionItemValueHolder item in val.InnerList) {
                //    NDF
                //    result += ((NdfFloat_2)item.Value).Value.ToString() + "|";
                //}

                //String result = linearizeList(val);

                return result;
            }

            return "idk";
        }
        public string getPaliersSuppressDamages() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PaliersSuppressDamages", out val)) {
                String result = linearizeList(val);

                return result;
            }

            return "idk";
        }
        public string getPaliersPhysicalDamages() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PaliersPhysicalDamages", out val)) {
                String result = linearizeList(val);

                return result;
            }

            return "idk";
        }        
        public string getPhysicalDamagesVehiculeChassisRotationSpeedModifier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PhysicalDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("VehiculeChassisRotationSpeedModifier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getPhysicalDamagesVehiculeTurretRotationSpeedModifier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PhysicalDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("VehiculeTurretRotationSpeedModifier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getPhysicalDamagesVehiculeFiringRateMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PhysicalDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("VehiculeFiringRateMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getPhysicalDamagesVehiculeSpeedModifier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PhysicalDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("VehiculeSpeedModifier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getPhysicalDamagesCannonFiringRateMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.PhysicalDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("CanonFiringRateMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }

        public string getSuppressDamagesInfAndCanonSpeedModifier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("InfAndCanonSpeedModifier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesInfAndCanonDispersionModifier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    String s = "idk";
                    NdfPropertyValue v = null;
                    if (((NdfObjectReference)item.Value).Instance.TryGetProperty("InfAndCanonDispersionMultiplier", out v)) {
                        s = v.Value.ToString();
                    } 

                    result += s;
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesInfDamagesMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("InfDamagesMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesInfFiringRateMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("InfFiringRateMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesCanonFiringRateMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("CanonFiringRateMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesVehiculeFiringRateMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("VehiculeFiringRateMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesVehiculeDispersionMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("VehiculeDispersionMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesArtilleryDispersionMultiplier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("ArtilleryDispersionMultiplier").ToString();
                }

                return result;
            }

            return "idk";
        }
        public string getSuppressDamagesHitModifier() {
            NdfCollection val;
            if (queryTarget.TryGetValueFromQuery<NdfCollection>("Modules.Damage.Default.CommonDamageDescriptor.SuppressDamagesEffects", out val)) {
                String result = "";
                foreach (CollectionItemValueHolder item in val.InnerList) {
                    if (result != "") {
                        result += "|";
                    }

                    result += ((NdfObjectReference)item.Value).Instance.GetValueOfProperty("HitModifier").ToString();
                }

                return result;
            }

            return "idk";
        }

        // END Damage module
        #endregion

        // Fuel module --------------------
        #region
        public string getFuel() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Fuel.Default.FuelCapacity", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public string getAutonomy() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Fuel.Default.FuelMoveDuration", out val)) {
                String result = val.ToString();

                return result + "s";
            }

            return "idk";
        }
        // END Fuel module --------
        #endregion

        // Transportable module -----------
        #region
        public string getSuppressDamageRatioIfTransporterKilled() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Transportable.SuppressDamageRatioIfTransporterKilled", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        // END Transportable module -------
        #endregion

        // Transporter module -------------
        #region
        public string getWreckUnloadPhysicalDamageBonus() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Transporter.Default.WreckUnloadPhysicalDamageBonus", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public string getWreckUnloadSuppressDamageBonus() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Transporter.Default.WreckUnloadSuppressDamageBonus", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }

        public string getWreckUnloadStunDamageBonus() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Transporter.Default.WreckUnloadStunDamageBonus", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        // END Transporter module ---------
        #endregion

        // Supply module ------------------
        #region
        public string getSupplyCapacity() {

            NdfValueWrapper val;
            if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Supply.Default.SupplyCapacity", out val)) {
                String result = val.ToString();

                return result;
            }

            return "idk";
        }
        // END Supply module --------------
        #endregion



        // Retired, nande said that changing these does nothing:
        #region
        //public string getMultiplierAtMaxRange() {
        //    NdfValueWrapper val;
        //    if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.DistanceMultiplierRule.MultiplierAtMaxRange", out val)) {
        //        String result = val.ToString();

        //        return result;
        //    }

        //    return "idk";
        //}

        //public string getMultiplierAtMinRange() {
        //    NdfValueWrapper val;
        //    if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.DistanceMultiplierRule.MultiplierAtMinRange", out val)) {
        //        String result = val.ToString();

        //        return result;
        //    }

        //    return "idk";
        //}

        //public string getProbabilityExponent() {
        //    NdfValueWrapper val;
        //    if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.DistanceMultiplierRule.ProbabilityExponent", out val)) {
        //        String result = val.ToString();

        //        return result;
        //    }

        //    return "idk";
        //}

        //public string getMultiplierAtMaxRangeWhileMoving() {
        //    NdfValueWrapper val;
        //    if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.DistanceMultiplierRule.MultiplierAtMaxRangeWhileMoving", out val)) {
        //        String result = val.ToString();

        //        return result;
        //    }

        //    return "idk";
        //}

        //public string getMultiplierAtMinRangeWhileMoving() {
        //    NdfValueWrapper val;
        //    if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.DistanceMultiplierRule.MultiplierAtMinRangeWhileMoving", out val)) {
        //        String result = val.ToString();

        //        return result;
        //    }

        //    return "idk";
        //}

        //public string getProbabilityExponentWhileMoving() {
        //    NdfValueWrapper val;
        //    if (queryTarget.TryGetValueFromQuery<NdfValueWrapper>("Modules.Scanner.Default.VisibilityRollRule.DistanceMultiplierRule.ProbabilityExponentWhileMoving", out val)) {
        //        String result = val.ToString();

        //        return result;
        //    }

        //    return "idk";
        //}
        #endregion
        // END RETIRED --------------------
    }
}
