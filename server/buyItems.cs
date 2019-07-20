// Gives a client a prompt to buy an item. Handles score transaction.
// @param GameConnection this	Client who is buying item.
// @param ItemData item		Item that client is trying to buy.
function GameConnection::SHOP_tryBuyItem(%this, %item)
{
  if ($SHOP::DefaultShopData.isPickup(%item))
    return;

  // TODO: Check if player has room in their tool inventory.

  %price = $SHOP::DefaultShopData.getPrice(%item);
  %newScore = %this.score - %price;
  %name = %item.uiName;
  if (%price $= "" || %price < 0) {
    %this.centerPrint("\c6This item is not for sale", 4);
  }
  else if (%price == 0 || ($SHOP::DefaultShopData.getBuyOnce(%item) && %this.SHOP_inventory.hasItem(%item))) {
    if (isObject(%this.player))
      %this.player.SHOP_addItem(%item);
  }
  else if (%newScore < 0) {
    // TODO: plural and singular form for "points"
    %msg = "\c6You need\c2" SPC (-%newScore) SPC "\c6more points to purchase a\c2" SPC %name;
    %this.centerPrint(%msg, 4);
  }
  else {
    %this.SHOP_pendingItem = %item;
    // TODO: plural and singular form for "points"
    %msg = "Would you like to buy a" SPC %name SPC "for" SPC %price SPC "points?"
       NL "You will have" SPC %newScore SPC "points after purchasing.";
    %title = "Confirm Purchase";
    if ($SHOP::DefaultShopData.getBuyOnce(%item))
      %title = %title SPC "(buy once)";
    else
      %title = %title SPC "(single use)";
    commandToClient(%this, 'MessageBoxYesNo', %title, %msg,
		    'SHOP_confirmPurchase', 'SHOP_declinePurchase');
  }
}

// Adds an item to a player's inventory if there is an available slot.
// @param ItemData 	item	Item to add.
function Player::SHOP_addItem(%this, %item)
{
  %item = nameToID(%item);
  // Code derived from Destiny's Event_addItem.
  %toolCount = %this.getDatablock().maxTools;
  for (%i = 0; %i < %toolCount; %i++) {
    %tool = %this.tool[%i];
    if (%tool == 0) {
      // Slot is available, so add item.
      %this.tool[%i] = %item;
      %this.weaponCount++;
      // Use delay to prevent firing on add.
      schedule(100, 0, messageClient, %this.client, 'MsgItemPickup', '', %i, %item);
      break;
    }
  }
}

// Confirms the purchase of an item.
// @param GameConnection cl	Client who is confirming their purchase request.
function serverCmdSHOP_confirmPurchase(%cl)
{
  // Ensure there is a purchase queued up.
  %item = nameToID(%cl.SHOP_pendingItem);
  if (!isObject(%item)) {
    error("ERROR: No purchase to confirm");
    return;
  }

  %price = $SHOP::DefaultShopData.getPrice(%item);
  %newScore = %cl.score - %price;

  // Check that item is still for sale.
  if (%price < 0) {
    %msg = "This item is no longer for sale.";
    commandToClient(%cl, 'MessageBoxOK', "Purchase Error", %msg);
    return;
  } // Check that client still has enough points.
  else if (%newScore < 0) {
    %msg = "You can no longer afford this item.";
    commandToClient(%cl, 'MessageBoxOK', "Purchase Error", %msg);
  }
  // TODO: Check if player has room in their tool inventory.
  else {
    // If this item is buy once make the item free for future purchases.
    if ($SHOP::DefaultShopData.getBuyOnce(%item)) {
      %cl.SHOP_inventory.addItem(%item);
      if (!%cl.SHOP_saveInvData())
	error("ERROR: Failed to save inventory data for \"" @ %cl.getName() @ "\"");
    }

    %cl.setScore(%newScore);

    // Add item to player's inventory if there is space.
    if (isObject(%cl.player))
      %cl.player.SHOP_addItem(%item);
  }
  
  %cl.SHOP_pendingItem = "";
}

package ItemShopPackage
{
  function serverCmdMessageBoxNo(%cl)
  {
    %cl.SHOP_pendingPrice = "";
    %cl.SHOP_pendingItem = "";

    return Parent::serverCmdMessageBoxNo(%cl);
  }

  // Disallows item pickup on collision.
  // Code derivative of Chrono's Script_ClickToPickup
  function Armor::onCollision(%this, %obj, %col, %vec, %vel)
  {
    // TODO: allow players to pickup ammo

    // Prevent a player from picking up an item if they are in a minigame and the item is not pickup-able.
    %cl = %obj.client;
    if (isObject(%cl) && isObject(%cl.minigame) && %col.getClassName() $= "Item") {
      %db = %col.getDatablock();
      if (!$SHOP::DefaultShopData.isPickup(%db))
	return 0;
    }

    return Parent::onCollision(%this, %obj, %col, %vec, %vel);
  }

  // Players can buy items by clicking on them.
  // Players cannot pickup items off the ground.
  // Code derivative of Chrono's Script_ClickToPickup
  function Armor::onTrigger(%this, %obj, %slot, %state)
  {
    %ret = Parent::onTrigger(%this, %obj, %slot, %state);

    // Only attempt a transaction if player is inside a minigame.
    %cl = %obj.client;
    if (isObject(%cl) && isObject(%cl.minigame)) {
      // If player is clicking without an item in their hand, attempt a transaction.
      if (%slot == 0 && %state == 1 && %obj.getMountedImage(0) == 0) {
	// Find object player is looking at.
	%eye = %obj.getEyePoint();
	%end = vectorScale(%obj.getEyeVector(), 10);
	%mask = $TypeMasks::FxBrickObjectType
	   | $TypeMasks::InteriorObjectType
	   | $TypeMasks::TerrainObjectType
	   | $TypeMasks::ItemObjectType;
	%raycast = containerRayCast(%eye, vectorAdd(%eye, %end), %mask, %obj);
	%target = firstWord(%raycast);

	// If target is an item, give client a transaction prompt.
	if (isObject(%target)
	    && %target.getClassName() $= "Item"
	    && miniGameCanUse(%obj, %target)
	    && %target.canPickup
	    && isObject(%target.spawnBrick))
	  %obj.client.SHOP_tryBuyItem(%target.getDatablock());
      }
    }

    return %ret;
  }

  function serverCmdDropTool(%cl, %slot)
  {
    // Only delete item if client is in a minigame
    %pl = %cl.player;
    if (isObject(%cl.minigame) && %pl.tool[%slot] != 0) {
      // Delete item without dropping it.
      %pl.tool[%slot] = 0;
      if (%this.weaponCount > 0)
	%this.weaponCount--;
      messageClient(%cl, 'MsgItemPickup', '', %slot, 0);
      if (%slot == %pl.currTool)
	%pl.unmountImage(0);
    } else {
      Parent::serverCmdDropTool(%cl, %slot);
    }
  }
};
