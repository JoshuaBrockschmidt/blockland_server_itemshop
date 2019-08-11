// Check if a client meets the admin level necessary to use admin commands.
// @param GameConnection cl	Client to check admin level of.
function SHOP_checkAdminLevel(%cl)
{
  if (($SHOP::PREF::AdminLevel == 0 && %cl.isAdmin) ||
      ($SHOP::PREF::AdminLevel == 1 && %cl.isSuperAdmin)) {
    return true;
  }

  %isHost = %cl.isLocalConnection() || %cl.bl_id == getNumKeyID();
  if ($SHOP::PREF::AdminLevel == 2 && %isHost)
    return true;

  return false;
}

package ItemShopPackage
{
  // Sets the price of an item.
  // @param int price   Price of item in score points. Must be greater than or equal to 0 and will
  //                 	be rounded down. Set price to 0 for free. Provide a negative price to make
  //                    item unbuyable.
  function serverCmdSetPrice(%cl, %price)
  {
    // Check if client has the correct admin level.
    if (!SHOP_checkAdminLevel(%cl)) {
      %cl.chatMessage("You do not have permission to use this command");
      return;
    }

    if (!isObject(%cl.player)) {
      %cl.chatMessage("You must be spawned to use this command");
      return;
    }

    if (trim(%price) $= "") {
      %cl.chatMessage("Please enter a price");
      return;
    }

    // TODO: Check if %price is numeric

    %price = mFloor(%price);

    // Find item the client's player is looking at if there is one.
    %item = %cl.SHOP_findItemFromEye();
    if (!isObject(%item)) {
      %cl.centerPrint("\c5No item found", 4);
      return;
    }
    %db = %item.getDatablock();
    %curPrice = $SHOP::DefaultShopData.getPrice(%db, %price);
    if (%price >= 0) {
      if (%price == %curPrice) {
	if (%curPrice == 0)
	  %msg = "\c6" @ %db.uiName SPC "\c5is already free";
	else
	  %msg = "\c5Price for\c6" SPC %db.uiName SPC "\c5is already\c6" SPC %price;
	%cl.centerPrint(%msg, 4);
	return;
      }

      $SHOP::DefaultShopData.setPrice(%db, %price);
      if (%price == 0)
	%msg = "\c6" @ %db.uiName SPC "\c2is now free";
      else
	%msg = "\c2Price for\c6" SPC %db.uiName SPC "\c2set to\c6" SPC %price;
      %cl.centerPrint(%msg, 4);
      if ($SHOP::PREF::DisplayUpdates) {
	if (%price == 0)
	  %msg = "\c3" @ %cl.name SPC "\c6has made\c3" SPC %db.uiName SPC "\c6free";
	else
	  %msg = "\c3" @ %cl.name SPC "\c6has changed the price of\c3" SPC %db.uiName SPC "\c6to\c3" SPC %price;
	SHOP_chatMessageAllAdmins(%msg);
      }
    } else {
      if (%curPrice == -1) {
	%cl.centerPrint("\c6" @ %db.uiName SPC "\c5is already not for sale", 4);
	return;
      }
      $SHOP::DefaultShopData.makeUnbuyable(%db);
      %cl.centerPrint("\c6" @ %db.uiName SPC "\c2is no longer for sale", 4);
      if ($SHOP::PREF::DisplayUpdates) {
	%msg = "\c3" @ %cl.name SPC "\c6has made\c3" SPC %db.uiName SPC "\c6not for sale";
	SHOP_chatMessageAllAdmins(%msg);
      }
    }

    $SHOP::DefaultShopData.saveData($SHOP::PriceSaveFileName);
    SHOP_updateAllPriceTags();
  }

  function serverCmdSP(%cl, %price)
  {
    serverCmdSetPrice(%cl, %price);
  }

  // Makes an item free after the first purchase.
  function serverCmdBuyOnce(%cl)
  {
    // Check if client has the correct admin level.
    if (!SHOP_checkAdminLevel(%cl)) {
      %cl.chatMessage("You do not have permission to use this command");
      return;
    }

    // Get item the client's player is looking at.
    %item = %cl.SHOP_findItemFromEye();
    if (!isObject(%item)) {
      %cl.centerPrint("\c5No item found", 4);
      return;
    }
    %db = %item.getDatablock();

    if ($SHOP::DefaultShopData.isPickup(%db)) {
      %cl.centerPrint("\c5Pick-ups cannot be bought", 4);
      return;
    }

    if ($SHOP::DefaultShopData.getPrice(%db) == -1) {
      %cl.centerPrint("\c6" @ %db.uiName SPC "\c5is not for sale", 4);
      return;
    }

    if ($SHOP::DefaultShopData.getPrice(%db) == 0) {
      %cl.centerPrint("\c5Free items cannot be bought", 4);
      return;
    }

    if ($SHOP::DefaultShopData.getBuyOnce(%db)) {
      %cl.centerPrint("\c6" @ %db.uiName SPC "\c5is already buy once", 4);
      return;
    }

    $SHOP::DefaultShopData.setBuyOnce(%db, true);

    $SHOP::DefaultShopData.saveData($SHOP::PriceSaveFileName);
    SHOP_updateAllPriceTags();

    %cl.centerPrint("\c6" @ %db.uiName SPC "\c2is now buy once", 4);
    if ($SHOP::PREF::DisplayUpdates) {
      %msg = "\c3" @ %cl.name SPC "\c6has made\c3" SPC %db.uiName SPC "\c6buy once";
      SHOP_chatMessageAllAdmins(%msg);
    }
  }

  // Makes an item a single use, wherein client must repurchase the item after each use.
  function serverCmdSingleUse(%cl)
  {
    // Check if client has the correct admin level.
    if (!SHOP_checkAdminLevel(%cl)) {
      %cl.chatMessage("You do not have permission to use this command");
      return;
    }

    // Get item the client's player is looking at.
    %item = %cl.SHOP_findItemFromEye();
    if (!isObject(%item)) {
      %cl.centerPrint("\c5No item found", 4);
      return;
    }
    %db = %item.getDatablock();

    if ($SHOP::DefaultShopData.isPickup(%db)) {
      %cl.centerPrint("\c5Pick-ups cannot be bought", 4);
      return;
    }

    if ($SHOP::DefaultShopData.getPrice(%db) == -1) {
      %cl.centerPrint("\c6" @ %db.uiName SPC "\c5is not for sale", 4);
      return;
    }

    if ($SHOP::DefaultShopData.getPrice(%db) == 0) {
      %cl.centerPrint("\c5Free items cannot be bought", 4);
      return;
    }

    if (!$SHOP::DefaultShopData.getBuyOnce(%db)) {
      %cl.centerPrint("\c6" @ %db.uiName SPC "\c5is already single use", 4);
      return;
    }

    $SHOP::DefaultShopData.setBuyOnce(%db, false);

    $SHOP::DefaultShopData.saveData($SHOP::PriceSaveFileName);
    SHOP_updateAllPriceTags();

    %cl.centerPrint("\c6" @ %db.uiName SPC "\c2is now single use", 4);
    if ($SHOP::PREF::DisplayUpdates) {
      %msg = "\c3" @ %cl.name SPC "\c6has made\c3" SPC %db.uiName SPC "\c6single use";
      SHOP_chatMessageAllAdmins(%msg);
    }
  }

  // Makes the item a player is looking at pick-up, wherein it can be picked up on touch like a normal item.
  // Helpful for making ammo drops usable.
  function serverCmdPickup(%cl)
  {
    // Check if client has the correct admin level.
    if (!SHOP_checkAdminLevel(%cl)) {
      %cl.chatMessage("You do not have permission to use this command");
      return;
    }

    if (!isObject(%cl.player)) {
      %cl.chatMessage("You must be spawned to use this command");
      return;
    }

    // Find item the client's player is looking at if there is one.
    %item = %cl.SHOP_findItemFromEye();
    if (!isObject(%item)) {
      %cl.centerPrint("\c5No item found", 4);
      return;
    }
    %db = %item.getDatablock();

    if ($SHOP::DefaultShopData.isPickup(%db)) {
      %cl.centerPrint("\c6" @ %db.uiName SPC "\c5is already a pick-up", 4);
      return;
    }

    $SHOP::DefaultShopData.makePickup(%db);
    $SHOP::DefaultShopData.saveData($SHOP::PriceSaveFileName);
    SHOP_updateAllPriceTags();

    %cl.centerPrint("\c6" @ %db.uiName SPC "\c2is now a pick-up", 4);
    if ($SHOP::PREF::DisplayUpdates) {
      %msg = "\c3" @ %cl.name SPC "\c6has made\c3" SPC %db.uiName SPC "\c6a pick-up";
      SHOP_chatMessageAllAdmins(%msg);
    }
  }

  // Sells the client's player's equipped item. The client will no longer own this item after selling.
  function serverCmdSellItem(%cl)
  {
    if (!isObject(%cl))
      return;

    if (!isObject(%cl.minigame)) {
      %cl.centerPrint("\c5You are not in a minigame", 4);
      return;
    }

    if (!$SHOP::PREF::CanSell) {
      %cl.centerPrint("\c5Selling is disabled", 4);
      return;
    }

    %pl = %cl.player;
    if (!isObject(%pl)) {
      %cl.centerPrint("\c5You must be alive to sell", 4);
      return;
    }

    %item = %pl.tool[%pl.currTool];
    if (!isObject(%item)) {
      %cl.centerPrint("\c5Please select an item", 4);
      return;
    }

    %cl.SHOP_trySellItem(%item);
  }

  // Transfer an item from one client to another. The client will no longer own this item after giving it.
  // The transfer will not happen if the would-be recipient already owns that item. Single use items can
  // only be given if the recipient has an empty inventory slot. Buy once items can be given regardless.
  // Finally, the recipient must accept the item offer.
  // @param string a0 ... a10      Name of client to give the item to.
  function serverCmdGiveItem(%cl, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
  {
    if (!isObject(%cl))
      return;

    %name = trim(%a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5 SPC %a6 SPC %a7 SPC %a8 SPC %a9 SPC %a10);
    %cl.SHOP_tryOfferItem(%name);
  }

  // Cancels an offer to give an item to another player.
  function serverCmdCancelOffer(%cl)
  {
    if (!isObject(%cl))
      return;

    %cl.SHOP_tryCancelOffer();
  }

  // Accepts another client's item offer.
  function serverCmdAcceptItem(%cl)
  {
    if (!isObject(%cl))
      return;

    %cl.SHOP_tryAcceptItem();
  }

  // Prompts a player confirming if they to sell all their items back for points.
  function serverCmdSellAllItems(%cl)
  {
    if (!isObject(%cl))
      return;

    if (!$SHOP::PREF::CanSell) {
      %cl.centerPrint("\c5Selling is disabled", 4);
      return;
    }

    %cl.SHOP_trySellAllItems();
  }

  // Prompts a client confirming if they to reset their shop inventory.
  function serverCmdResetItemData(%cl)
  {
    if (!isObject(%cl))
      return;

    %cl.SHOP_tryResetItemData();
  }

  function serverCmdShopHelp(%cl)
  {
    %bullet = "<font:palatino linotype:24>\c9  \x95";

    %cl.chatMessage("<font:palatino linotype:35><shadow:2:2>\c6Item Shop Help");
    %cl.chatMessage("<font:impact:20><shadow:2:2>\c6------------------------------------------");

    %cl.chatMessage("<font:palatino linotype:25>\c6Admin Commands");
    %cl.chatMessage(%bullet SPC "\c4/SetPrice <price> \c6OR \c4/sp <price>\c6 - Set the price of an item you are looking at.");
    %cl.chatMessage(%bullet SPC "\c4/BuyOnce\c6 - Make an item free after the first purchase.");
    %cl.chatMessage(%bullet SPC "\c4/SingleUse\c6 - Make an item single use after purchase.");
    %cl.chatMessage(%bullet SPC "\c4/Pickup\c6 - Make an item a pickup. Behaves like a normal item when touched. Use \c4/SetPrice \c6to undo.");

    %cl.chatMessage("<font:palatino linotype:25>\c6Events");
    %cl.chatMessage(%bullet SPC "\c4Player -> SaveShopItems\c6 - Save the player's loadout, excluding items which are single use or not for sale.");
    %cl.chatMessage(%bullet SPC "\c4Player -> LoadShopItems\c6 - Load the player's saved loadout, excluding items which are single use or not for sale.");

    %cl.chatMessage("<font:palatino linotype:25>\c6General Commands");
    %cl.chatMessage(%bullet SPC "\c4/SellItem\c6 - Sell an item for its current market price.");
    %cl.chatMessage(%bullet SPC "\c4/GiveItem <player name>\c6 - Offers another player your equipped item.");
    %cl.chatMessage(%bullet SPC "\c4/CancelOffer\c6 - Cancels an offer initiated by \c4/GiveItem\c6.");
    %cl.chatMessage(%bullet SPC "\c4/AcceptItem\c6 - Accepts an item gifting offer from another player initiated by \c4/GiveItem\c6.");
    %cl.chatMessage(%bullet SPC "\c4/SellAllItems\c6 - Sell every item you have bought.");
    %cl.chatMessage(%bullet SPC "\c4/ResetItemData\c6 - Un-buy every item you have bought. Does not return score points.");
    %cl.chatMessage(%bullet SPC "\c4/ShopHelp\c6 - Show this help");

    %cl.chatMessage("<font:palatino linotype:20>\c6(Scroll up for more)");
  }
};
