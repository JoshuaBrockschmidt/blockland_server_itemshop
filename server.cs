$SHOP::FilePath = filePath($Con::File) @ "/";
$SHOP::ServerPath = $SHOP::FilePath @ "server/";
$SHOP::CommonPath = $SHOP::FilePath @ "common/";
$SHOP::DataPath = "config/server/ItemShop/";

if (isObject(SHOP_ServerGroup)) {
  SHOP_ServerGroup.chainDeleteAll();
  SHOP_ServerGroup.delete();
}
new ScriptGroup(SHOP_ServerGroup);

exec($SHOP::ServerPath @ "buyItems.cs");
exec($SHOP::ServerPath @ "commands.cs");
exec($SHOP::ServerPath @ "prefs.cs");
exec($SHOP::ServerPath @ "priceTags.cs");

exec($SHOP::ServerPath @ "InvData.cs");
exec($SHOP::ServerPath @ "PriceData.cs");
exec($SHOP::ServerPath @ "ShopData.cs");

exec($SHOP::CommonPath @ "CSVReader.cs");

$SHOP::DefaultShopData = SHOP_ShopData();
SHOP_ServerGroup.add($SHOP::DefaultShopData);

deactivatePackage(ItemShopPackage); // DEBUG
activatePackage(ItemShopPackage);

// Load item price data.
$SHOP::PriceSaveFileName = $SHOP::DataPath @ "itemshop.csv";
if (isFile($SHOP::PriceSaveFileName)) {
  echo("Loading price data...");
  $SHOP::DefaultShopData.loadData($SHOP::PriceSaveFileName);
}
