$SHOP::FilePath = filePath($Con::File) @ "/";
$SHOP::ServerPath = $SHOP::FilePath @ "server/";
$SHOP::CommonPath = $SHOP::FilePath @ "common/";

// Directories where price and inventory data is stored.
$SHOP::DataPath = "config/server/ItemShop/";
if ($Server::LAN)
  $SHOP::DataPath = $SHOP::DataPath @ "lan/";
else
  $SHOP::DataPath = $SHOP::DataPath @ "net/";
$SHOP::DataPath = $SHOP::DataPath @ strlwr($GamemodeDisplayName) @ "/";
$SHOP::GamemodePath = "Add-Ons/Gamemode_" @ $GamemodeDisplayName @ "/";
$SHOP::ClientDataPath = $SHOP::DataPath @ "clients/";

if (isObject(SHOP_ServerGroup)) {
  SHOP_ServerGroup.chainDeleteAll();
  SHOP_ServerGroup.delete();
}
new ScriptGroup(SHOP_ServerGroup);

exec($SHOP::ServerPath @ "ammoSupport.cs");
exec($SHOP::ServerPath @ "buyItems.cs");
exec($SHOP::ServerPath @ "commands.cs");
exec($SHOP::ServerPath @ "events.cs");
exec($SHOP::ServerPath @ "giveItem.cs");
exec($SHOP::ServerPath @ "players.cs");
exec($SHOP::ServerPath @ "prefs.cs");
exec($SHOP::ServerPath @ "priceTags.cs");
exec($SHOP::ServerPath @ "utils.cs");

exec($SHOP::ServerPath @ "InvData.cs");
exec($SHOP::ServerPath @ "PriceData.cs");
exec($SHOP::ServerPath @ "ShopData.cs");

exec($SHOP::CommonPath @ "CSVReader.cs");

$SHOP::DefaultShopData = SHOP_ShopData();
SHOP_ServerGroup.add($SHOP::DefaultShopData);

deactivatePackage(ItemShopPackage); // DEBUG
activatePackage(ItemShopPackage);

$SHOP::PriceSaveFileName = $SHOP::DataPath @ "itemshop.csv";
$SHOP::DefaultPriceSaveFileName = $SHOP::GamemodePath @ "itemshop.csv";

// We do not want to load shop data until all item add-ons have been loaded.
package ItemShopLoadAfterPackage
{
  function GameConnection::onClientEnterGame(%this)
  {
    // A client cannot enter the game until all add-ons have been loaded
    // so this is a good place to load item data.

    // Load item data.
    if (isFile($SHOP::PriceSaveFileName))
      %loadFrom = $SHOP::PriceSaveFileName;
    else if (isFile($SHOP::DefaultPriceSaveFileName))
      %loadFrom = $SHOP::DefaultPriceSaveFileName;

    if (%loadFrom !$= "") {
      echo("Loading price data from \"" @ %loadFrom @ "\"");
      $SHOP::DefaultShopData.loadData(%loadFrom);
    }

    // Make ammo pickups.
    $SHOP::DefaultShopData.makeAmmoPickups();
    Parent::onClientEnterGame(%this);

    // Only need to run this script once so deactivate the package to prevent
    // running it again.  Please note that this must be called after
    // Parent::onClientEnterGame or other packages that wrap this function will
    // not execute.
    deactivatePackage(ItemShopLoadAfterPackage);
  }
};
activatePackage(ItemShopLoadAfterPackage);
