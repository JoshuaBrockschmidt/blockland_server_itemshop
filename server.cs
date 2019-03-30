$SHOP::FilePath = filePath($Con::File) @ "/";
$SHOP::ServerPath = $SHOP::FilePath @ "server/";
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

$SHOP::DefaultShopData = SHOP_ShopData();
//$SHOP::DefaultShopData.load($SHOP::DataPath @ "itemshop.csv"); // TODO
SHOP_ServerGroup.add($SHOP::DefaultShopData);

deactivatePackage(ItemShopPackage); // DEBUG
activatePackage(ItemShopPackage);
