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
  function Armor::onCollision(%this, %armor, %col, %vec, %vel)
  {
    // TODO: allow players to pickup ammo

    // Prevent a player from picking up an item if they are in a minigame and the item is not pickup-able.
    %cl = %armor.client;
    if (isObject(%cl) && %col.getClassName() $= "Item") {
      %db = %col.getDatablock();
      %isStatic = %col.isStatic();

      // Lobby player types cannot pick up items.
      // True if the player is a lobby player type.
      %isLobby = %armor.getDatablock().SHOP_isLobby;

      // True if item is marked as pickup-able
      %pickup = $SHOP::DefaultShopData.isPickup(%db);

      // Lobby players can only pick up pickup-able items from spawn bricks.
      if (%isLobby && (!%isStatic || !%pickup))
	return 0;

      // True if item is a non-static item and picking up non-static item is enabled.
      %droppedPickup = !%isStatic && $SHOP::PREF::CanPickUpDropped;

      // Pick up behavior in minigames.
      if (isObject(%cl.minigame) && !%pickup && !%droppedPickup)
	return 0;
    }

    return Parent::onCollision(%this, %armor, %col, %vec, %vel);
  }

  // Players can buy items by clicking on them.
  // Players cannot pickup items off the ground.
  // Code derivative of Chrono's Script_ClickToPickup
  function Armor::onTrigger(%this, %armor, %slot, %state)
  {
    %ret = Parent::onTrigger(%this, %armor, %slot, %state);

    // Only attempt a transaction if player is inside a minigame.
    %cl = %armor.client;
    if (isObject(%cl) && isObject(%cl.minigame)) {
      // If player is clicking without an item in their hand, attempt a transaction.
      if (%slot == 0 && %state == 1 && %armor.getMountedImage(0) == 0) {
	%item = %cl.SHOP_findItemFromEye();
	if (isObject(%item)
	    && miniGameCanUse(%armor, %item)
	    && %item.canPickup
	    && %item.isStatic())
	  %armor.client.SHOP_tryBuyItem(%item.getDatablock());
      }
    }

    return %ret;
  }

  function serverCmdDropTool(%cl, %slot)
  {
    %pl = %cl.player;
    if (isObject(%pl)) {
      // True if client is in a minigame and throwing item fromone's inventory is disallowed.
      %miniDisallow = isObject(%cl.minigame) && !$SHOP::PREF::CanThrow;

      // True if client has a lobby player type.
      %isLobby = %pl.getDatablock().SHOP_isLobby;

      // True if selected tool slot has a tool in it.
      %hasTool = %pl.tool[%slot] != 0;

      if (%hasTool && (%miniDisallow || %isLobby)) {
	if (%pl.tool[%slot].canDrop) {
	  // Delete item without dropping it.
	  %pl.tool[%slot] = 0;
	  if (%this.weaponCount > 0)
	    %this.weaponCount--;
	  messageClient(%cl, 'MsgItemPickup', '', %slot, 0);
	  if (%slot == %pl.currTool)
	    %pl.unmountImage(0);
	}
      }
    }

    Parent::serverCmdDropTool(%cl, %slot);
  }
};
