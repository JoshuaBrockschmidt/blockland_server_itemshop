//
// General-purpose utility functions.
//

// Gets the item object the client's player is looking at.
// @return Item	Item object if the client's player is looking at an item and -1 otherwise.
function GameConnection::SHOP_findItemFromEye(%this)
{
  %pl = %this.player;
  if (!isObject(%pl)) {
    error("ERROR: Client has no player.");
    return -1;
  }

  %eye = %pl.getEyePoint();
  %end = vectorScale(%pl.getEyeVector(), 10);
  %mask = $TypeMasks::FxBrickObjectType
     | $TypeMasks::InteriorObjectType
     | $TypeMasks::TerrainObjectType
     | $TypeMasks::ItemObjectType;
  %raycast = containerRayCast(%eye, vectorAdd(%eye, %end), %mask, %obj);
  %target = firstWord(%raycast);

  if (!isObject(%target) || %target.getClassName() !$= "Item") {
    return -1;
  } else {
    return %target;
  }
}

// Checks if a player's inventory is full.
function Player::SHOP_isInventoryFull(%this)
{
  %toolCount = %this.getDatablock().maxTools;
  for (%i = 0; %i < %toolCount; %i++) {
    if (%this.tool[%i] == 0)
      return false;
  }
  return true;
}

// Deletes a tool from a client's player's inventory. Item will not be thrown. If slot is empty, nothing will happen.
// @param integer slot	Tool slot containing item to delete.
function GameConnection::SHOP_deleteTool(%this, %slot)
{
  %pl = %this.player;
  if (isObject(%pl)) {
    // Check if selected tool slot has a tool in it.
    %tool = %pl.tool[%slot];
    if (%tool != 0) {
      if (%tool.canDrop) {
	%pl.tool[%slot] = 0;
	messageClient(%this, 'MsgItemPickup', '', %slot, 0);
	if (%slot == %pl.currTool)
	  %pl.unmountImage(0);
      }
    }
  }
}

function Player::_SHOP_addItem(%this, %slot, %item) {
  %this.tool[%slot] = %item;
  messageClient(%this.client, 'MsgItemPickup', '', %slot, %item);
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
      // Use delay to prevent firing on add.
      %this.schedule(100, _SHOP_addItem, %i, %item);
      break;
    }
  }
}

// Displays a chat message to all admins.
// @param string msg	Message to display.
function SHOP_chatMessageAllAdmins(%msg)
{
  %msg = "\c2Item Shop\c6:" SPC %msg;
  %clientCount = ClientGroup.getCount();
  for (%i = 0; %i < %clientCount; %i++) {
    %cl = ClientGroup.getObject(%i);
    if (%cl.isAdmin)
      %cl.chatMessage(%msg);
  }
}
