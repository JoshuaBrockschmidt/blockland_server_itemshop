// The admin level needed to set prices and buy-once/single-use status.
// 0 for admin, 1 for super admin, 2 for host.
$SHOP::PREF::AdminLevel = 1;

// Whether items should be buy once by default, the alternative being singlu use.
$SHOP::PREF::BuyOnceByDefault = true;

// Whether players can sell items.
$SHOP::PREF::CanSell = true;

// Whether players can give items.
$SHOP::PREF::CanGive = true;

// Whether players can throw items. If false, items dropped from a player's inventory will simply be deleted.
$SHOP::PREF::CanThrow = false;

// Whether players can pickup non-static items (not items on bricks).
$SHOP::PREF::CanPickUpDropped = false;

// Distance within which players can view price tags.
$SHOP::PREF::PriceTagDist = 30;

// Price tag color for items not for sale.
$SHOP::PREF::NotForSaleColor = "1 0 0 1";

// Price tag for free items.
$SHOP::PREF::FreeColor = "0 1 0 1";

// Price tag color for buy once items.
$SHOP::PREF::BuyOnceColor = "0 1 1 1";

// Price tag color for single use items.
$SHOP::PREF::SingleUseColor = "0 0 1 1";

// Whether to display a message to admins every time an item's price or status is updated.
$SHOP::PREF::DisplayUpdates = true;
// TODO: Use Blockland Glass preferences.
