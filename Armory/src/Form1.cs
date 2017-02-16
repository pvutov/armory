using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Armory
{
    public partial class Form1 : Form {
        private UnitDatabase unitDatabase;
        private List<String> currentUnits;


        public Form1(UnitDatabase unitDatabase) {
            this.unitDatabase = unitDatabase;
            InitializeComponent();

            countrySelect.DataSource = unitDatabase.getAllCountries();

            categorySelect.DataSource = new List<String>(new String[] { "6", "13", "14" });
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void unitSearch_TextChanged(object sender, EventArgs e) {
            filterUnitList();
        }

        private void countrySelect_SelectedIndexChanged(object sender, EventArgs e) {
            filterUnitList();
        }

        private void categorySelect_SelectedIndexChanged(object sender, EventArgs e) {
            filterUnitList();
        }

        private void filterUnitList() {
            String selectedFaction = countrySelect.GetItemText(countrySelect.SelectedItem);
            currentUnits = unitDatabase.getUnitList(selectedFaction);



            List<String> filteredUnits = currentUnits.FindAll(s => {
                try {
                    return Regex.IsMatch(s, unitSearch.Text, RegexOptions.IgnoreCase);
                }
                catch (ArgumentException) {
                    return Regex.IsMatch(s, Regex.Escape(unitSearch.Text), RegexOptions.IgnoreCase);
                }
            });
            unitList.DataSource = filteredUnits;
        }

        private void unitList_SelectedIndexChanged(object sender, EventArgs e) {
            weaponDropdown.SelectedItem = null;
            String selectedUnit = unitList.GetItemText(unitList.SelectedItem);
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

                // Flag:
                #region
                Bitmap img = null;
                switch (selectedUnit.Substring(0,3)) {
                    case "ANZ": img = Properties.Resources.ANZAC; break;
                    case "CAN": img = Properties.Resources.CAN; break;
                    case "CHI": img = Properties.Resources.PRC; break;
                    case "DAN": img = Properties.Resources.DEN; break;
                    case "FIN": img = Properties.Resources.FIN; break;
                    case "FR ": img = Properties.Resources.FRA; break;
                    case "HOL": img = Properties.Resources.NED; break;
                    case "ISR": img = Properties.Resources.ISR; break;
                    case "JAP": img = Properties.Resources.JAP; break;
                    case "NK ": img = Properties.Resources.DPRK; break;
                    case "NOR": img = Properties.Resources.NOR; break;
                    case "POL": img = Properties.Resources.POL; break;
                    case "RDA": img = Properties.Resources.DDR; break;
                    case "RFA": img = Properties.Resources.BRD; break;
                    case "ROK": img = Properties.Resources.ROK; break;
                    case "SWE": img = Properties.Resources.SWE; break;
                    case "TCH": img = Properties.Resources.CZS; break;
                    case "UK ": img = Properties.Resources.UK; break;
                    case "URS": img = Properties.Resources.USSR; break;
                    case "US ": img = Properties.Resources.USA; break;
                    case "YUG": img = Properties.Resources.YU; break;
                    default: break;
                }
                flagSimple.Image = img;
                flagFirepower.Image = flagSimple.Image;
                flagRecon.Image = flagSimple.Image;
                flagMobility.Image = flagSimple.Image;
                flagSurvivability.Image = flagSimple.Image;
                #endregion

                // Availability:
                #region
                String[] availability = unitDatabase.getMaxDeployableAmount();
                bool hasRookie = availability[0] != "0";
                bool hasTrained = availability[1] != "0";
                bool hasHardened = availability[2] != "0";
                bool hasVeteran = availability[3] != "0";
                bool hasElite = availability[4] != "0";
                
                if (hasRookie) {
                    noRookieImageFirepower.Hide();
                    noRookieImageSimple.Hide();
                    noRookieImageRecon.Hide();
                    noRookieImageMobility.Hide();
                    noRookieImageSurvivability.Hide();

                    yesRookieImageFirepower.Show();
                    yesRookieImageSimple.Show();
                    yesRookieImageRecon.Show();
                    yesRookieImageMobility.Show();
                    yesRookieImageSurvivability.Show();

                    rookieFieldFirepower.Text = availability[0];
                    rookieFieldSimple.Text = rookieFieldFirepower.Text;
                    rookieFieldRecon.Text = rookieFieldFirepower.Text;
                    rookieFieldMobility.Text = rookieFieldFirepower.Text;
                    rookieFieldSurvivability.Text = rookieFieldFirepower.Text;
                }
                else {
                    noRookieImageFirepower.Show();
                    noRookieImageSimple.Show();
                    noRookieImageRecon.Show();
                    noRookieImageMobility.Show();
                    noRookieImageSurvivability.Show();

                    yesRookieImageFirepower.Hide();
                    yesRookieImageSimple.Hide();
                    yesRookieImageRecon.Hide();
                    yesRookieImageMobility.Hide();
                    yesRookieImageSurvivability.Hide();

                    rookieFieldFirepower.Text = "";
                    rookieFieldSimple.Text = rookieFieldFirepower.Text;
                    rookieFieldRecon.Text = rookieFieldFirepower.Text;
                    rookieFieldMobility.Text = rookieFieldFirepower.Text;
                    rookieFieldSurvivability.Text = rookieFieldFirepower.Text;
                }
                if (hasTrained) {
                    noTrainedImageFirepower.Hide();
                    noTrainedImageSimple.Hide();
                    noTrainedImageRecon.Hide();
                    noTrainedImageMobility.Hide();
                    noTrainedImageSurvivability.Hide();

                    yesTrainedImageFirepower.Show();
                    yesTrainedImageSimple.Show();
                    yesTrainedImageRecon.Show();
                    yesTrainedImageMobility.Show();
                    yesTrainedImageSurvivability.Show();

                    trainedFieldFirepower.Text = availability[1];
                    trainedFieldSimple.Text = trainedFieldFirepower.Text;
                    trainedFieldRecon.Text = trainedFieldFirepower.Text;
                    trainedFieldMobility.Text = trainedFieldFirepower.Text;
                    trainedFieldSurvivability.Text = trainedFieldFirepower.Text;
                }
                else {
                    noTrainedImageFirepower.Show();
                    noTrainedImageSimple.Show();
                    noTrainedImageRecon.Show();
                    noTrainedImageMobility.Show();
                    noTrainedImageSurvivability.Show();

                    yesTrainedImageFirepower.Hide();
                    yesTrainedImageSimple.Hide();
                    yesTrainedImageRecon.Hide();
                    yesTrainedImageMobility.Hide();
                    yesTrainedImageSurvivability.Hide();

                    trainedFieldFirepower.Text = "";
                    trainedFieldSimple.Text = trainedFieldFirepower.Text;
                    trainedFieldRecon.Text = trainedFieldFirepower.Text;
                    trainedFieldMobility.Text = trainedFieldFirepower.Text;
                    trainedFieldSurvivability.Text = trainedFieldFirepower.Text;

                }
                if (hasHardened) {
                    noHardenedImageFirepower.Hide();
                    noHardenedImageSimple.Hide();
                    noHardenedImageRecon.Hide();
                    noHardenedImageMobility.Hide();
                    noHardenedImageSurvivability.Hide();

                    yesHardenedImageFirepower.Show();
                    yesHardenedImageSimple.Show();
                    yesHardenedImageRecon.Show();
                    yesHardenedImageMobility.Show();
                    yesHardenedImageSurvivability.Show();

                    hardenedFieldFirepower.Text = availability[2];
                    hardenedFieldSimple.Text = hardenedFieldFirepower.Text;
                    hardenedFieldRecon.Text = hardenedFieldFirepower.Text;
                    hardenedFieldMobility.Text = hardenedFieldFirepower.Text;
                    hardenedFieldSurvivability.Text = hardenedFieldFirepower.Text;

                }
                else {
                    noHardenedImageFirepower.Show();
                    noHardenedImageSimple.Show();
                    noHardenedImageRecon.Show();
                    noHardenedImageMobility.Show();
                    noHardenedImageSurvivability.Show();

                    yesHardenedImageFirepower.Hide();
                    yesHardenedImageSimple.Hide();
                    yesHardenedImageRecon.Hide();
                    yesHardenedImageMobility.Hide();
                    yesHardenedImageSurvivability.Hide();

                    hardenedFieldFirepower.Text = "";
                    hardenedFieldSimple.Text = hardenedFieldFirepower.Text;
                    hardenedFieldRecon.Text = hardenedFieldFirepower.Text;
                    hardenedFieldMobility.Text = hardenedFieldFirepower.Text;
                    hardenedFieldSurvivability.Text = hardenedFieldFirepower.Text;

                }
                if (hasVeteran) {
                    noVeteranImageFirepower.Hide();
                    noVeteranImageSimple.Hide();
                    noVeteranImageRecon.Hide();
                    noVeteranImageMobility.Hide();
                    noVeteranImageSurvivability.Hide();

                    yesVeteranImageFirepower.Show();
                    yesVeteranImageSimple.Show();
                    yesVeteranImageRecon.Show();
                    yesVeteranImageMobility.Show();
                    yesVeteranImageSurvivability.Show();

                    veteranFieldFirepower.Text = availability[3];
                    veteranFieldSimple.Text = veteranFieldFirepower.Text;
                    veteranFieldRecon.Text = veteranFieldFirepower.Text;
                    veteranFieldMobility.Text = veteranFieldFirepower.Text;
                    veteranFieldSurvivability.Text = veteranFieldFirepower.Text;

                }
                else {
                    noVeteranImageFirepower.Show();
                    noVeteranImageSimple.Show();
                    noVeteranImageRecon.Show();
                    noVeteranImageMobility.Show();
                    noVeteranImageSurvivability.Show();

                    yesVeteranImageFirepower.Hide();
                    yesVeteranImageSimple.Hide();
                    yesVeteranImageRecon.Hide();
                    yesVeteranImageMobility.Hide();
                    yesVeteranImageSurvivability.Hide();

                    veteranFieldFirepower.Text = "";
                    veteranFieldSimple.Text = veteranFieldFirepower.Text;
                    veteranFieldRecon.Text = veteranFieldFirepower.Text;
                    veteranFieldMobility.Text = veteranFieldFirepower.Text;
                    veteranFieldSurvivability.Text = veteranFieldFirepower.Text;

                }
                if (hasElite) {
                    noEliteImageFirepower.Hide();
                    noEliteImageSimple.Hide();
                    noEliteImageRecon.Hide();
                    noEliteImageMobility.Hide();
                    noEliteImageSurvivability.Hide();

                    yesEliteImageFirepower.Show();
                    yesEliteImageSimple.Show();
                    yesEliteImageRecon.Show();
                    yesEliteImageMobility.Show();
                    yesEliteImageSurvivability.Show();

                    eliteFieldFirepower.Text = availability[4];
                    eliteFieldSimple.Text = eliteFieldFirepower.Text;
                    eliteFieldRecon.Text = eliteFieldFirepower.Text;
                    eliteFieldMobility.Text = eliteFieldFirepower.Text;
                    eliteFieldSurvivability.Text = eliteFieldFirepower.Text;

                }
                else {
                    noEliteImageFirepower.Show();
                    noEliteImageSimple.Show();
                    noEliteImageRecon.Show();
                    noEliteImageMobility.Show();
                    noEliteImageSurvivability.Show();

                    yesEliteImageFirepower.Hide();
                    yesEliteImageSimple.Hide();
                    yesEliteImageRecon.Hide();
                    yesEliteImageMobility.Hide();
                    yesEliteImageSurvivability.Hide();

                    eliteFieldFirepower.Text = "";
                    eliteFieldSimple.Text = eliteFieldFirepower.Text;
                    eliteFieldRecon.Text = eliteFieldFirepower.Text;
                    eliteFieldMobility.Text = eliteFieldFirepower.Text;
                    eliteFieldSurvivability.Text = eliteFieldFirepower.Text;

                }
                #endregion

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

                // suppression effects
                paliersSuppressDamagesField.Text = unitDatabase.getPaliersSuppressDamages();
                infAndCanonSpeedModifierField.Text = unitDatabase.getSuppressDamagesInfAndCanonSpeedModifier();
                infAndCanonDispersionModifierField.Text = unitDatabase.getSuppressDamagesInfAndCanonDispersionModifier();
                infDamagesMultiplierField.Text = unitDatabase.getSuppressDamagesInfDamagesMultiplier();
                infFiringRateMultiplierField.Text = unitDatabase.getSuppressDamagesInfFiringRateMultiplier();
                canonFiringRateMultiplierField.Text = unitDatabase.getSuppressDamagesCanonFiringRateMultiplier();
                vehiculeFiringRateMultiplierField.Text = unitDatabase.getSuppressDamagesVehiculeFiringRateMultiplier();
                vehiculeDispersionMultiplierField.Text = unitDatabase.getSuppressDamagesVehiculeDispersionMultiplier();
                artilleryDispersionMultiplierField.Text = unitDatabase.getSuppressDamagesArtilleryDispersionMultiplier();
                hitModifierField.Text = unitDatabase.getSuppressDamagesHitModifier();

                // physical damage effects
                paliersPhysicalDamagesField.Text = unitDatabase.getPaliersPhysicalDamages();
                cannonFiringRateMultiplierField.Text = unitDatabase.getPhysicalDamagesCannonFiringRateMultiplier();
                vehiculeSpeedField.Text = unitDatabase.getPhysicalDamagesVehiculeSpeedModifier();
                vehiculeFiringRateField.Text = unitDatabase.getPhysicalDamagesVehiculeFiringRateMultiplier();
                chassisRotationSpeedField.Text = unitDatabase.getPhysicalDamagesVehiculeChassisRotationSpeedModifier();
                turretRotationSpeedField.Text = unitDatabase.getPhysicalDamagesVehiculeTurretRotationSpeedModifier();
                // END Survivability controls -----------
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
                specializedDetection4Field.Text = unitDatabase.getAntiplaneSpottingCap();
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

        private void checkForUpdatesButton_Click(object sender, EventArgs e) {
            Updater updater = new Updater();

            if (updater.updateAvailable()) {
                // update
                ProgressBar progressBar = new ProgressBar();
                progressBar.Name = "downloadProgressBar";
                progressBar.Maximum = 100;
                progressBar.Scale(new SizeF(0.7f, 1f));

                Panel container = new Panel();
                container.Location = checkForUpdatesButton.Location;
                container.Controls.Add(progressBar);

                panelTopRight.Controls.Remove(checkForUpdatesButton);
                panelTopRight.Controls.Add(container);

                updater.applyUpdate((int val) => { progressBar.Value = val; });

                // remove progress bar when it is done
                var t = new Timer();
                t.Interval = 10000;
                t.Tick += (s, _) => {
                    if (progressBar.Value == progressBar.Maximum) {
                        panelTopRight.Controls.Remove(container);
                        panelTopRight.Controls.Add(checkForUpdatesButton);
                        t.Stop();
                    }
                };
                t.Start();
            }
            else {
                // remove button, show and disappear text
                Label updateMessageLabel = new Label();
                updateMessageLabel.Name = "updateMessageLabel";
                updateMessageLabel.Text = "at newest version";
                updateMessageLabel.Location = checkForUpdatesButton.Location;
                updateMessageLabel.MaximumSize = new Size(checkForUpdatesButton.Size.Width, 0);
                updateMessageLabel.AutoSize = true;

                panelTopRight.Controls.Remove(checkForUpdatesButton);
                panelTopRight.Controls.Add(updateMessageLabel);

                var t = new Timer();
                t.Interval = 4000; 
                t.Tick += (s, _) => {
                    panelTopRight.Controls.Remove(updateMessageLabel);
                    t.Stop();
                };
                t.Start();
            }


        }

        private void cloneButton_Click(object sender, EventArgs e) {
            Form1 f = new Form1(unitDatabase.clone());
            f.checkForUpdatesButton.Hide();
            f.Show();
        }
    }
}
