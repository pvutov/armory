using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IrisZoomDataApi;
using IrisZoomDataApi.Model.Ndfbin;
using System.Text.RegularExpressions;

namespace Armory
{
    public partial class Form1 : Form {
        private UnitDatabase unitDatabase;
        private List<String> currentUnits;


        public Form1(UnitDatabase unitDatabase) {
            this.unitDatabase = unitDatabase;
            InitializeComponent();

            listBox1.DataSource = unitDatabase.getAllCountries();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            List<String> filteredUnits = currentUnits.FindAll(delegate (string s) {
                try {
                    return Regex.IsMatch(s, textBox1.Text, RegexOptions.IgnoreCase);
                }
                catch (System.ArgumentException) {
                   return Regex.IsMatch(s, Regex.Escape(textBox1.Text), RegexOptions.IgnoreCase);
                }
            });
            listBox2.DataSource = filteredUnits;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            String selectedFaction = listBox1.GetItemText(listBox1.SelectedItem);

            currentUnits = unitDatabase.getUnitList(selectedFaction);
            listBox2.DataSource = currentUnits;

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) {
            weaponDropdown.SelectedItem = null;
            String selectedUnit = listBox2.GetItemText(listBox2.SelectedItem);
            if (unitDatabase.setQueryTarget(selectedUnit)) {
                
                weaponDropdown.DataSource = unitDatabase.getWeapons();
                weaponDropdownSimple.DataSource = weaponDropdown.DataSource;

                Weapon lockedWeapon = unitDatabase.tryGetLockIndexedWeapon();
                if (lockedWeapon != null) {
                    weaponDropdown.SelectedItem = lockedWeapon;
                }

                // Controls common to all tabs ---
                #region
                priceFieldSimple.Text = unitDatabase.getPrice();
                priceFieldFirepower.Text = unitDatabase.getPrice();
                priceFieldRecon.Text = unitDatabase.getPrice();
                priceFieldMobility.Text = unitDatabase.getPrice();
                priceFieldSurvivability.Text = unitDatabase.getPrice();

                nameFieldSimple.Text = unitDatabase.getUnitName();
                nameFieldFirepower.Text = unitDatabase.getUnitName();
                nameFieldRecon.Text = unitDatabase.getUnitName();
                nameFieldMobility.Text = unitDatabase.getUnitName();
                nameFieldSurvivability.Text = unitDatabase.getUnitName();

                if (unitDatabase.isPrototype()) {
                    prototypeLabelSimple.Show();
                    prototypeLabelFirepower.Show();
                    prototypeLabelRecon.Show();
                    prototypeLabelMobility.Show();
                    prototypeLabelSurvivability.Show();
                }
                else {
                    prototypeLabelSimple.Hide();
                    prototypeLabelFirepower.Hide();
                    prototypeLabelRecon.Hide();
                    prototypeLabelMobility.Hide();
                    prototypeLabelSurvivability.Hide();
                }

                unitCardField.Image = unitDatabase.getUnitCard();
                unitCardFieldFirepower.Image = unitCardField.Image;
                unitCardFieldRecon.Image = unitCardField.Image;
                unitCardFieldMobility.Image = unitCardField.Image;
                unitCardFieldSurvivability.Image = unitCardField.Image;

                // END common controls --------------
                #endregion

                // Controls from Survivability tab ------
                #region
                topArmorField.Text = unitDatabase.getTopArmor();
                sideArmorField.Text = unitDatabase.getSideArmor();
                rearArmorField.Text = unitDatabase.getRearArmor();
                frontArmorField.Text = unitDatabase.getFrontArmor();
                healthField.Text = unitDatabase.getHealth();
                ecmField.Text = unitDatabase.getECM();
                sizeField.Text = unitDatabase.getSize();
                maxSuppressionDamagesField.Text = unitDatabase.getMaxSuppressionDamages();
                stunDamagesRegenField.Text = unitDatabase.getStunDamagesRegen();
                stunDamagesToGetStunnedField.Text = unitDatabase.getStunDamagesToGetStunned();
                suppressDamagesRegenRatioField.Text = unitDatabase.getSuppressDamagesRegenRatio();
                suppressDamagesRegenRatioOutOfRangeField.Text = unitDatabase.getSuppressDamagesRegenRatioOutOfRange();
                paliersSuppressDamagesField.Text = unitDatabase.getPaliersSuppressDamages();
                paliersPhysicalDamagesField.Text = unitDatabase.getPaliersPhysicalDamages();
                physicalDamagesEffectsField.Text = unitDatabase.getPhysicalDamagesEffects();
                suppressDamagesEffectsField.Text = unitDatabase.getSuppressDamagesEffects();

                // END firepower controls -----------
                #endregion

                // Controls from Recon tab ----------
                #region
                groundOpticsField.Text = unitDatabase.getGroundOptics();
                airOpticsField.Text = unitDatabase.getAirOptics();
                timeBetweenEachIdentifyRollField.Text = unitDatabase.getTimeBetweenEachIdentifyRoll();
                identifyBaseProbabilityField.Text = unitDatabase.getIdentifyBaseProbability();
                stealthField.Text = unitDatabase.getStealth();
                detectionTbaField.Text = unitDatabase.getAntiheloSpottingCap();
                porteeVisionField.Text = unitDatabase.getAntigroundSpottingCap();
                porteeVisionTbaField.Text = unitDatabase.getAntigroundSpottingCapWhileFlying();
                opticalStrengthAntiRadarField.Text = unitDatabase.getOpticalStrengthAntiRadar();
                unitTypeField.Text = unitDatabase.getUnitType();
                killExperienceBonusField.Text = unitDatabase.getKillExperienceBonus();
                // END recon controls ---------------
                #endregion

                // Controls from mobility tab -------
                #region
                speedField.Text = unitDatabase.getSpeed();
                fuelField.Text = unitDatabase.getFuel();
                autonomyField.Text = unitDatabase.getAutonomy();
                tempsDemiTourField.Text = unitDatabase.getTempsDemiTour();
                maxAccelerationField.Text = unitDatabase.getMaxAcceleration();
                maxDecelerationField.Text = unitDatabase.getMaxDeceleration();
                unitMovingTypeField.Text = unitDatabase.getUnitMovingType();
                vehicleSubTypeField.Text = unitDatabase.getVehicleSubType();
                suppressDamageRatioIfTransporterKilledField.Text = unitDatabase.getSuppressDamageRatioIfTransporterKilled();
                wreckUnloadPhysicalDamageBonusField.Text = unitDatabase.getWreckUnloadPhysicalDamageBonus();
                wreckUnloadStunDamageBonusField.Text = unitDatabase.getWreckUnloadStunDamageBonus();
                wreckUnloadSuppressDamageBonusField.Text = unitDatabase.getWreckUnloadSuppressDamageBonus();
                flyingAltitudeField.Text = unitDatabase.getFlyingAltitude();
                minimalAltitudeField.Text = unitDatabase.getMinimalAltitude();
                supplyCapacityField.Text = unitDatabase.getSupplyCapacity();
                lowAltitudeFlyingAltitudeField.Text = unitDatabase.getLowAltitudeFlyingAltitude();
                nearGroundFlyingAltitudeField.Text = unitDatabase.getNearGroundFlyingAltitude();
                // END mobility controls ------------
                #endregion


                // Controls from simple tab ---------
                #region
                healthFieldSimple.Text = healthField.Text;
                groundOpticsFieldSimple.Text = groundOpticsField.Text;
                airOpticsFieldSimple.Text = airOpticsField.Text;
                stealthFieldSimple.Text = stealthField.Text;
                ecmFieldSimple.Text = ecmField.Text;
                sizeFieldSimple.Text = sizeField.Text;
                speedFieldSimple.Text = speedField.Text;
                autonomyFieldSimple.Text = autonomyField.Text;
                frontArmorFieldSimple.Text = frontArmorField.Text;
                sideArmorFieldSimple.Text = sideArmorField.Text;
                rearArmorFieldSimple.Text = rearArmorField.Text;
                topArmorFieldSimple.Text = topArmorField.Text;
                altitudeFieldSimple.Text = flyingAltitudeField.Text;
                supplyCapacityFieldSimple.Text = supplyCapacityField.Text;
                // END simple controls --------------
                #endregion
            }

            else {
                Program.warning("The unit you selected was not found. This should never happen.");
            }
        }

        private void customQueryInput_TextChanged(object sender, EventArgs e) {
            customQueryOutputField.Text = unitDatabase.doCustomQuery(customQueryInput.Text);
        }

        private void weaponDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            unitDatabase.setCurrentWeapon((Weapon)weaponDropdown.SelectedItem);
            customQueryOutputField.Text = unitDatabase.doCustomQuery(customQueryInput.Text);
            
            // avoid callback                        
            weaponDropdownSimple.SelectedIndexChanged -= weaponDropdownSimple_SelectedIndexChanged;
            weaponDropdownSimple.SelectedItem = weaponDropdown.SelectedItem;
            weaponDropdownSimple.SelectedIndexChanged += weaponDropdownSimple_SelectedIndexChanged;

            // Controls from Firepower tab ------
            #region
            //weaponPictureField.Image = unitDatabase.getWeaponPicture();
            turretField.Text = unitDatabase.getWeaponTurret();
            aimTimeField.Text = unitDatabase.getAimTime();
            accuracyField.Text = unitDatabase.getAccuracy();
            minAccuracyField.Text = unitDatabase.getMinAccuracy();
            minCritChanceField.Text = unitDatabase.getMinCritChance();
            stabilizerField.Text = unitDatabase.getStabilizer();
            supplyCostField.Text = unitDatabase.getSupplyCost();
            suppressionField.Text = unitDatabase.getSuppression();
            suppressionSplashRadius.Text = unitDatabase.getSuppressionSplash();
            heField.Text = unitDatabase.getHE();
            apField.Text = unitDatabase.getAP();
            heSplashField.Text = unitDatabase.getHeSplash();
            tagsLabel.Text = unitDatabase.getTags();
            groundRangeField.Text = unitDatabase.getGroundRange();
            heloRangeField.Text = unitDatabase.getHeloRange();
            planeRangeField.Text = unitDatabase.getPlaneRange();
            noiseField.Text = unitDatabase.getNoise();
            fireChanceField.Text = unitDatabase.getFireChance();
            salvoLengthField.Text = unitDatabase.getSalvoLength();
            shotReloadField.Text = unitDatabase.getShotReloadPostprocessed();
            salvoReloadField.Text = unitDatabase.getSalvoReload();
            rofField.Text = unitDatabase.getROF();
            maxDispersionField.Text = unitDatabase.getMaxDispersion();
            minDispersionField.Text = unitDatabase.getMinDispersion();
            angleDispersionField.Text = unitDatabase.getAngleDispersion();
            tirReflexeField.Text = unitDatabase.getTirReflexe();
            randomDispersionField.Text = unitDatabase.getRandomDispersion();
            missileTimeBetweenCorrectionsField.Text = unitDatabase.getMissileTimeBetweenCorrections();
            puissanceField.Text = unitDatabase.getPuissance();
            missileMaxSpeedField.Text = unitDatabase.getMissileMaxSpeed();
            ammoField.Text = unitDatabase.getAmmo();
            missileMaxAccelField.Text = unitDatabase.getMissileMaxAcceleration();
            // END firepower controls -----------
            #endregion

            // Controls from simple tab ---------
            #region
            groundRangeFieldSimple.Text = groundRangeField.Text;
            planeRangeFieldSimple.Text = planeRangeField.Text;
            heloRangeFieldSimple.Text = heloRangeField.Text;
            tagsLabelSimple.Text = tagsLabel.Text;
            accuracyFieldSimple.Text = accuracyField.Text;
            stabilizerFieldSimple.Text = stabilizerField.Text;
            apFieldSimple.Text = apField.Text;
            heFieldSimple.Text = heField.Text;
            rofFieldSimple.Text = rofField.Text;
            shotReloadFieldSimple.Text = shotReloadField.Text;
            salvoLengthFieldSimple.Text = salvoLengthField.Text;
            salvoReloadFieldSimple.Text = salvoReloadField.Text;
            aimTimeFieldSimple.Text = aimTimeField.Text;
            ammoFieldSimple.Text = ammoField.Text;
            maxDispersionFieldSimple.Text = maxDispersionField.Text;
            minDispersionFieldSimple.Text = minDispersionField.Text;
            missileSpeedFieldSimple.Text = missileMaxSpeedField.Text;
            supplyCostFieldSimple.Text = supplyCostField.Text;
            turretFieldSimple.Text = turretField.Text;
            tagsLabelSimple.Text = tagsLabel.Text;
            suppressionFieldSimple.Text = suppressionField.Text;
            // END simple controls --------------
            #endregion
        }

        private void weaponDropdownSimple_SelectedIndexChanged(object sender, EventArgs e) {
            weaponDropdown.SelectedItem = weaponDropdownSimple.SelectedItem;
        }

        private void lockWeaponCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (lockWeaponCheckbox.Checked) {
                string weaponPosition = unitDatabase.lockWeapon();
                lockWeaponCheckbox.Text = "locked " + weaponPosition;
                lockWeaponCheckboxSimple.Text = lockWeaponCheckbox.Text;
                
                // avoid callback                        
                lockWeaponCheckboxSimple.CheckedChanged -= lockWeaponCheckboxSimple_CheckedChanged;
                lockWeaponCheckboxSimple.Checked = true;
                lockWeaponCheckboxSimple.CheckedChanged += lockWeaponCheckboxSimple_CheckedChanged;
            }
            else {
                lockWeaponCheckbox.Text = "lock slot";
                lockWeaponCheckboxSimple.Text = lockWeaponCheckbox.Text;
                unitDatabase.unlockWeapon();

                // avoid callback                        
                lockWeaponCheckboxSimple.CheckedChanged -= lockWeaponCheckboxSimple_CheckedChanged;
                lockWeaponCheckboxSimple.Checked = false;
                lockWeaponCheckboxSimple.CheckedChanged += lockWeaponCheckboxSimple_CheckedChanged;
            }
        }

        private void lockWeaponCheckboxSimple_CheckedChanged(object sender, EventArgs e) {
            lockWeaponCheckbox.Checked = lockWeaponCheckboxSimple.Checked;
        }

        private void button1_Click(object sender, EventArgs e) {
            WinSparkleWrapper.win_sparkle_check_update_with_ui();
        }
    }
}
