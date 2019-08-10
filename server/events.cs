function GameConnection::SHOP_getItemsSavePath(%this)
{
  if ($Server::LAN)
    // Use LAN name as unique identifier.
    return $SHOP::ClientDataPath @ strlwr(%this.LANname) @ ".save";
  else
    // Use BLID as unique identifier.
    return $SHOP::ClientDataPath @ %this.bl_id @ ".save";
}

// Checks if an item should be saved or loaded.
// @param object item	Object to validate.
// @return boolean	True if `item` is a valid ItemData datablock, and is
//                	either free, a pickup, or a buy-once item the client has bought.
function GameConnection::SHOP_isSaveableItem(%this, %item)
{
  if (isObject(%item)) {
    if (%item.getClassName() $= "ItemData") {
      %price = $SHOP::DefaultShopData.getPrice(%item);
      %isBuyOnce = $SHOP::DefaultShopData.getBuyOnce(%item);
      %isPickup = $SHOP::DefaultShopData.isPickup(%item);
      %hasItem = %this.SHOP_inventory.hasItem(%item);
      if (%price == 0 || %isPickup || (%price > 0 && %isBuyOnce && %hasItem))
	return true;
    }
  }
  return false;
}

// Saves a player's current loadout to disk.  Only saves items that are free,
// pickups, or buy-once items the client has bought.
function Player::saveShopItems(%this)
{
  if (!isObject(%this))
    return;
  %cl = %this.client;
  if (!isObject(%cl))
    return;

  // Write each item's datablock on its own line.
  %savePath = %cl.SHOP_getItemsSavePath();
  %file = new FileObject();
  %file.openForWrite(%savePath);
  %toolCount = %this.getDatablock().maxTools;
  for (%i = 0; %i < %toolCount; %i++) {
    %tool = %this.tool[%i];
    if (isObject(%tool)) {
      %itemDb = %tool.getName();
      if (%cl.SHOP_isSaveableItem(%itemDb))
	%file.writeLine(%itemDb);
    }
  }
  %file.close();
}

// Loads a player's current loadout from disk.  Only loads items that are free,
// pickups, or buy-once items the client has bought.
function Player::loadShopItems(%this)
{
  if (!isObject(%this))
    return;
  %cl = %this.client;
  if (!isObject(%cl))
    return;

  %this.clearTools();

  // We will always load the player's save data from disk.
  // This is not the most efficient approach, but its performance is sufficient in practice.
  %savePath = %cl.SHOP_getItemsSavePath();
  if (isFile(%savePath)) {
    %file = new FileObject();
    %file.openForRead(%savePath);
    while (!%file.isEOF() && !%this.SHOP_isInventoryFull()) {
      %itemDb = trim(%file.readLine());
      if (%cl.SHOP_isSaveableItem(%itemDb))
	%this.SHOP_addItem(%itemDb);
    }
    %file.close();
  }
}

registerOutputEvent("Player", "SaveShopItems");
registerOutputEvent("Player", "LoadShopItems");
