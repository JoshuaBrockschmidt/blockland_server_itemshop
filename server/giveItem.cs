$SHOP::PREF::OfferTimeout = 120000;  // 2 minutes

// Attempts to offer an item to another client.  The item equipped by the player will be offered.
// The item being offered must be up for sale.
// @param string name	Name of client to offer item to.
function GameConnection::SHOP_tryOfferItem(%this, %name)
{
  // Check if item giving is enabled.
  if (!$SHOP::PREF::CanGive) {
    %this.chatMessage("Item giving is disabled");
    return;
  }

  // Check if client is already offering an item to someone.
  if (%this.SHOP_isOffering()) {
    %this.chatMessage("You are already offering a\c3" SPC %this.SHOP_offerItem.uiName SPC "\c0to\c3" SPC %this.SHOP_offerTo.name @ "\c0. Use \c3/CancelOffer \c0to cancel this offer.");
    return;
  }

  // Get the item the player currently has selected.
  %pl = %this.player;
  if (!isObject(%pl)) {
    %this.chatMessage("No item selected");
    return;
  }
  %item = %pl.tool[%pl.currTool];
  if (!isObject(%item)) {
    %this.chatMessage("No item selected");
    return;
  }

  // Check that item is for sale.
  %price = $SHOP::DefaultShopData.getPrice(%item);
  if (%price <= 0) {
    %this.chatMessage("You can only give items which are bought");
    return;
  }

  // Check if client exists.
  %giveTo = findClientByName(%name);
  if (!isObject(%giveTo)) {
    %this.chatMessage("Player not found");
    return;
  }

  // Make sure client is not themselves.
  if (nameToID(%this) == nameToID(%giveTo)) {
    %this.chatMessage("You cannot give an item to yourself");
    return;
  }

  // Check if the client being offered already has an offer.
  if (%giveTo.SHOP_offerFrom !$= "") {
    %this.chatMessage("\c3" @ %giveTo.name SPC "\c0already has a pending offer");
    return;
  }

  // If item is buy-once, check if recipient already has that item.
  %isBuyOnce = $SHOP::DefaultShopData.getBuyOnce(%item);
  if (%isBuyOnce) {
    if (%giveTo.SHOP_inventory.hasItem(%item)) {
      %this.chatMessage("\c3" @ %giveTo.name SPC "\c0has already bought\c3" SPC %item.uiName);
      return;
    }
  }

  // Initiate offer between clients.
  %this.SHOP_offerTo = %giveTo;
  %this.SHOP_offerItem = %item;
  %this.SHOP_offerSlot = %pl.currTool;
  %giveTo.SHOP_offerFrom = %this;
  %this.chatMessage("\c4You are offering your\c3" SPC %item.uiName SPC "\c4to\c3" SPC %giveTo.name @ "\c4. Type \c3/CancelOffer\c4 to cancel.");
  %giveTo.chatMessage("\c3" @ %this.name SPC "\c4is offering you their\c3" SPC %item.uiName @ "\c4. Type \c3/AcceptItem\c4 to accept.");

  // Cancel offer after a period of time.
  %this.SHOP_offerTimer = %this.schedule($SHOP::PREF::OfferTimeout, SHOP_offerTimeout, %this, %giveTo);
}

// Checks if a client is currently offering an item, initiated by `SHOP_tryOfferItem`.
// @return boolean	True if the client is offering an item and false otherwise.
function GameConnection::SHOP_isOffering(%this)
{
  return %this.SHOP_offerTo !$= "";
}

// Checks if a client has an item being offered to them.
// @return boolean	True if the client has an offer and false otherwise.
function GameConnection::SHOP_hasOffer(%this)
{
  return %this.SHOP_offerFrom !$= "";
}

// Cancels an item offer initiated by `GameConnection::SHOP_tryOfferItem`.
function GameConnection::SHOP_endOffer(%this)
{
  %giveTo = %this.SHOP_offerTo;
  %this.SHOP_offerTo = "";
  %this.SHOP_offerItem = "";
  %this.SHOP_offerSlot = "";
  if (isObject(%giveTo))
    %giveTo.SHOP_offerFrom = "";
  if (isEventPending(%this.SHOP_offerTimer))
    cancel(%this.SHOP_offerTimer);
}

// Cancels a item offer initiated by `GameConnection::SHOP_tryOfferItem` after a timeout.
// Messages the client and would-be recipient about the timeout.
function GameConnection::SHOP_offerTimeout(%this)
{
  %this.chatMessage("\c4Your offer of\c3" SPC %this.SHOP_offerItem.uiName SPC "\c4has timed out");
  %giveTo = %this.SHOP_offerTo;
  if (isObject(%giveTo))
    %giveTo.chatMessage("\c3" @ %this.name @ "\c4's offer to you has timed out");
  %this.SHOP_endOffer();
}

// Attempts to cancel an offer made by `GameConnection::SHOP_tryOfferItem`.
// Checks that an offer has been made.
function GameConnection::SHOP_tryCancelOffer(%this)
{
  if (!%this.SHOP_isOffering()) {
    %this.chatMessage("You have no pending offer");
    return;
  }
  %giveTo = %this.SHOP_offerTo;
  %this.chatMessage("\c4You have canceled your offer of\c3" SPC %this.SHOP_offerItem.uiName SPC "\c4to\c3" SPC %giveTo.name);
  if (isObject(%giveTo))
    %giveTo.chatMessage("\c3" @ %this.name SPC "\c4has canceled their offer");
  %this.SHOP_endOffer();
}

// Attempts to accept an offer from another client initiated by `GameConnection::SHOP_tryOfferItem`.
// Nothing will happen if no offer is currently active.  If an offer is active, the item will be
// given only if the offerer still has it.
function GameConnection::SHOP_tryAcceptItem(%this) {
  // Check that client has an active offer to them.
  if (!%this.SHOP_hasOffer()) {
    %this.chatMessage("Nobody is offering you an item");
    return;
  }

  // Ensure offerer still exists.
  %offerer = %this.SHOP_offerFrom;
  if (!isObject(%offerer)) {
    %this.chatMessage("Nobody is offering you an item");
    %this.SHOP_offerFrom = "";
    return;
  }

  // Ensure item is still give-able.
  %item = %this.SHOP_offerFrom.SHOP_offerItem;
  %price = $SHOP::DefaultShopData.getPrice(%item);
  if (%price <= 0) {
    %this.chatMessage("\c4The\c3" SPC %item.uiName SPC "\c4that\c3" SPC %offerer.name SPC "\c4offered you is no longer for sale. Offer canceled.");
    %offerer.chatMessage("\c4The item you offered\c3" SPC %this.name SPC "\c4is no longer for sale. Offer canceled.");
    %offerer.SHOP_endOffer();
    return;
  }

  %isBuyOnce = $SHOP::DefaultShopData.getBuyOnce(%item);
  if (%isBuyOnce) {
    // Item being offered is buy-once.
    // Make sure the offerer still owns the item.
    if (!%offerer.SHOP_inventory.hasItem(%item)) {
      %this.chatMessage("\c3" @ %offerer.name SPC "\c4no longer owns a\c3" SPC %item.uiName @ "\c4. Offer canceled.");
      %offerer.chatMessage("\c4You no longer own a\c3" SPC %item.uiName @ "\c4. Offer canceled.");
      %offerer.SHOP_endOffer();
      return;
    }

    // Make sure the client does not already own the item.
    if (%this.SHOP_inventory.hasItem(%item)) {
      %this.chatMessage("\c4You have already bought a\c3" SPC %item.uiName @ "\c4. Offer canceled.");
      %offerer.chatMessage("\c3" @ %this.name SPC "\c4has already bought a\c3" SPC %item.uiName @ "\c4. Offer canceled.");
      %offerer.SHOP_endOffer();
      return;
    }

    // Take item from offerer and give it to the client.
    %offerer.SHOP_inventory.removeItem(%item);
    if (!%offerer.SHOP_saveInvData())
      error("ERROR: Failed to save inventory data for" SPC %offerer.name SPC "after giving an item to" SPC %this.name);
    %this.SHOP_inventory.addItem(%item);
    if (!%this.SHOP_saveInvData())
      error("ERROR: Failed to save inventory data for" SPC %this.name SPC "after receiving a gift from" SPC %offerer.name);

    // Take item from offerer's inventory and give it to the client (if they have room).
    %offerPl = %offerer.player;
    if (isObject(%offerPl)) {
      %toolCount = %offerPl.getDatablock().maxTools;
      for (%i = 0; %i < %toolCount; %i++) {
	%tool = %offerPl.tool[%i];
	if (nameToID(%tool) == nameToID(%item))
	  %offerer.SHOP_deleteTool(%i);
      }
    }
    %pl = %this.player;
    if (isObject(%pl))
      %pl.SHOP_addItem(%item);

    // Inform both clients of success.
    %this.chatMessage("\c4You accepted a\c3" SPC %item.uiName SPC "\c4from\c3" SPC %offerer.name @ "\c4. You now own this item.");
    %offerer.chatMessage("\c3" @ %this.name SPC "\c4accepted your\c3" SPC %item.uiName @ "\c4. You no longer own this item.");
    %offerer.SHOP_endOffer();

  } else {
    // Item being offered is single-use.
    // Make sure the offerer still has the item in their inventory.
    %offerPl = %offerer.player;
    if (isObject(%offerPl))
      %hasItem = nameToID(%offerPl.tool[%offerer.SHOP_offerSlot]) == nameToID(%offerer.SHOP_offerItem);
    if (!isObject(%offerPl) || !%hasItem) {
      %this.chatMessage("\c3" @ %offerer.name SPC "\c4no longer has the\c3" SPC %item.uiName SPC "\c4they offered you. Offer canceled.");
      %offerer.chatMessage("\c4You no longer have the\c3" SPC %item.uiName SPC "\c4you offered\c3" SPC %this.name @ "\c4. Offer canceled.");
      %offerer.SHOP_endOffer();
      return;
    }

    // Make sure the client is alive.  Allow them to retry if not.
    %pl = %this.player;
    if (!isObject(%pl)) {
      %this.chatMessage("\c4You must be alive to accept an single use item. Please respawn and try again.");
      return;
    }

    // Make sure the client has room in their inventory.  Allow them to retry if not.
    if (%pl.SHOP_isInventoryFull()) {
      %this.chatMessage("\c4Your inventory is full. Please drop an item try again.");
      return;
    }

    // Take item from offerer and give it to the client.
    %offerer.SHOP_deleteTool(%offerer.SHOP_offerSlot);
    %pl.SHOP_addItem(%item);

    // Inform both clients of success.
    %this.chatMessage("\c4You accepted a\c3" SPC %item.uiName SPC "\c4from\c3" SPC %offerer.name);
    %offerer.chatMessage("\c3" @ %this.name SPC "\c4accepted your\c3" SPC %item.uiName);
    %offerer.SHOP_endOffer();
  }
}

package ItemShopGivePackage
{
  function GameConnection::onClientLeaveGame(%this)
  {
    // Cancel a client's offer when they leave the game.
    if (%this.SHOP_isOffering()) {
      %giveTo = %this.SHOP_offerTo;
      if (isObject(%giveTo))
	%giveTo.chatMessage("\c3" @ %this.name @ "\c4's offer to you has been canceled");
      %this.SHOP_endOffer();
    }

    Parent::onClientLeaveGame(%this);
  }
};

activatePackage(ItemShopGivePackage);
