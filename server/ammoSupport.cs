//
// Makes several ammo types pickups by default.
// Supports the following add-ons:
//
//    * Tier+Tactical, Weapon_Package_Tier1 (2012/23/9 release)
//    * Bushido's Adventure Pack, Weapon_AdventurePack (version1.1.1)
//    * Blockality, Item_AmmoBox (2016/10/13 release)
//    * XCOM, Weapon_Package_XCOM (version 1.9.0)
//    * Complex Weapons, Weapon_Package_Complex (version 1.0.2)

$SHOP::ammoDatablocksCount = 0;

// Adds a datablock to the list of known ammo datablocks.
// @param string	Name of ammo datablock. Will not be checked for validity.
function SHOP_addAmmoDatablock(%name) {
  $SHOP::ammoDatablocks[$SHOP::ammoDatablocksCount] = %name;
  $SHOP::ammoDatablocksCount++;
}

// Makes all known ammo types that are not for sale into pickups. Ammo types with prices attached to them will keep
// their prices.
function SHOP_ShopData::makeAmmoPickups(%this)
{
  for (%i = 0; %i < $SHOP::ammoDatablocksCount; %i++) {
    %ammo = $SHOP::ammoDatablocks[%i];
    // Ignore ammo datablock that do not exist / are not loaded.
    if (isObject(%ammo)) {
      // Make sure this is a vlaid datablock.
      if (%ammo.getClassName() !$= "ItemData") {
	error("ERROR: Failed to make ammo type a pickup. Invalid ItemData datablock \"" @ %ammo @ "\"");
        continue;
      }

      // Make ammo a pickup if it is not for sale.
      if (%this.getPrice(%ammo) == -1)
	%this.makePickup(%ammo);
    }
  }

  SHOP_updateAllPriceTags();
}


// Tier+Tactical
SHOP_addAmmoDatablock("bigstaticItem");
SHOP_addAmmoDatablock("threestaticItem");
SHOP_addAmmoDatablock("fivestaticItem");
SHOP_addAmmoDatablock("sixstaticItem");
SHOP_addAmmoDatablock("sevenstaticItem");
SHOP_addAmmoDatablock("eightstaticItem");
SHOP_addAmmoDatablock("ninestaticItem");
SHOP_addAmmoDatablock("boltstaticItem");
SHOP_addAmmoDatablock("ShotgunStaticItem");
SHOP_addAmmoDatablock("grenadestaticItem");
SHOP_addAmmoDatablock("rocketstaticItem");

// Bushido's Adventure Pack
SHOP_addAmmoDatablock("advAmmoItem");
SHOP_addAmmoDatablock("advAmmoMachineRifleItem");
SHOP_addAmmoDatablock("advAmmoMachinePistolItem");
SHOP_addAmmoDatablock("advAmmoPistolItem");
SHOP_addAmmoDatablock("advAmmoRevolverItem");
SHOP_addAmmoDatablock("advAmmoRifleItem");
SHOP_addAmmoDatablock("advAmmoShotgunItem");
SHOP_addAmmoDatablock("advAmmoSniperRifleItem");

// Blockality
SHOP_addAmmoDatablock("AmmoBoxPickupItem");
// We exclude AmmoBoxDropItem since it is an equippable item.

// XCOM
SHOP_addAmmoDatablock("PowerPackItem50");
SHOP_addAmmoDatablock("PowerPackItem100");
SHOP_addAmmoDatablock("PowerPackItem300");

// Complex Weapons Package
SHOP_addAmmoDatablock("MagazineItem_3006_x8");
SHOP_addAmmoDatablock("MagazineItem_45ACP_x7");
SHOP_addAmmoDatablock("MagazineItem_M24A1");
SHOP_addAmmoDatablock("MagazineItem_45ACP_x20_SMG");
SHOP_addAmmoDatablock("MagazineItem_MicroUzi");
SHOP_addAmmoDatablock("MagazineItem_MicroUziExtended");
SHOP_addAmmoDatablock("Bullet357PackItem");
SHOP_addAmmoDatablock("BulletBuckshotPackItem");
