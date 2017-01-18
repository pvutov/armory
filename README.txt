This program is an honest attempt to expose the "hidden" WRD armory values in a user-friendly way.

Instead of showing all of the data that the modding suite provides, I've chosen to highlight 
the parts that are relevant to the player. There are bound to be errors. I may forget to include
an important stat. I may present an NDF variable as something it is not, because I do not understand
the entire of the NDF binary. If you catch any of these errors or omissions, please inform me.


The following is a list of fields that the program modifies instead of displaying verbatim:

Field | NDF Variable name                              | Transformations
-----------------------------------------------------------------------------
Prototype | IsPrototype                                | True => Yes, Null => No
Speed | Modules.MouvementHandler.Default.Maxspeed      | divided by 52, see http://eightgold.com/RedDragon/GameScales.pdf
FlyingAltitude | Modules.MouvementHandler.Default.FlyingAltitude | divided by 52
MinimalAltitude | Modules.MouvementHandler.Default.MinimalAltitude | divided by 52
MaxAcceleration | Modules.MouvementHandler.Default.MaxAcceleration | divided by 52
MaxDeceleration | Modules.MouvementHandler.Default.MaxDeceleration | divided by 52
LowAltitudeFlyingAltitude | Modules.Position.Default.LowAltitudeFlyingAltitude | divided by 52
NearGroundFlyingAltitude | Modules.Position.Default.NearGroundFlyingAltitude | divided by 52
Front/etc Armor | Modules.Damage.Default.CommonDamageDescriptor.BlindageProperties.ArmorDescriptorFront.BaseBlindage| all values subtracted by 4 with minimum 0; for values formerly in the 1-4 range, appended "splash resist type " + value before subtraction
AP     | Modules.WeaponManager.Default.TurretDescriptorList[m].MountedWeaponDescriptorList[n].Ammunition.Arme | if between 5 and 34, subtract 4; if above 34, subtract 34, if equal to 3, return '-' (arme 3 => HE)
Tags | Composite of TAmmunition.Guidance and WeaponDescriptor.TirEnMouvement | [STAT] intentionally omitted
All weapon ranges and dispersions | Modules.WeaponManager.Default.TurretDescriptorList[0].MountedWeaponDescriptorList[0].Ammunition.PorteeMaximale | multiplied by 175, divided by 13000
Physical/suppression splash | Modules.WeaponManager.Default.TurretDescriptorList[0].MountedWeaponDescriptorList[0].Ammunition.RadiusSplashSuppressDamages | divided by 52
spotting caps aka DetectionTBA, PorteeVisionTBA, PorteeVision | Modules.ScannerConfiguration.Default.DetectionTBA | multiplied by 175 and divided by 13000
missile speed | Modules.WeaponManager.Default.TurretDescriptorList[0].MountedWeaponDescriptorList[0].Ammunition.MissileDescriptor.Modules.MouvementHandler.Maxspeed | divided by 52 
stabilizer | ...HitRollRule.MinimalHitProbability | if TirEnMouvement is not True, then ignore stabilizer and output "-"
accuracies | --------------------- | converted from fractionals to percentages (0.1 -> 10% and so on)
shot reload | ...TempsEntreDeuxTirs | if salvo length = 1, output '-' instead of real value


If curious about the source of any other field, look in UnitDatabase.cs.



-------------------

Want to help? I'd love input from modders and designers:
- Are there any gameplay-significant stats that I missed? Where can I find them? 
- Can you help me make the program look better / increase its usability? Give me a mockup and 
I'll implement it if it's good. Even better, you can implement it yourself with no programming experience:
	1. download this project  
	2. download visual studio community edition (google it), press file->open->project, navigate


-------------------

Mayb I'll implement these one day:
-availability display thing
-country flag infront of unit name
-pinning
-android
-filter by more than 1 faction
-better supply stat, rearm time instead of supply cost for planes, autonomy + supply capacity merged