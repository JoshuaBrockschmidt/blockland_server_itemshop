// Creates an empty virtual inventory of items.
function SHOP_InvData()
{
  %this = new ScriptObject() {
    class = SHOP_InvData;
  };
  %this.items = new SimSet();
  return %this;
}

// Adds an item to the inventory.
// @param ItemData item	Item to add.
function SHOP_InvData::addItem(%this, %item)
{
  if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }

  if (!%this.hasItem(%item))
    %this.items.add(%item);
}

// Remove an item from the inventory.
// @param ItemData item	Item to remove.
function SHOP_InvData::removeItem(%this, %item)
{
  if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }

  if (%this.hasItem(%item))
    %this.items.remove(%item);
}

// Checks if inventory already contains an item.
// @param ItemData item	Item to search for.
// @return boolean	True if inventory has the item and false otherwise.
function SHOP_InvData::hasItem(%this, %item)
{
  return %this.items.isMember(%item);
}

// Save inventory data to a file.
// @param string fn     Path of file relative to the Blockland folder.
// @return boolean	True if data was successfully saved and false otherwise.
function SHOP_InvData::saveData(%this, %fn) {
  error("ERROR: Not implemented yet");
  return false;
}

// Loads inventory data from a file.
// @param string fn     Path of file relative to the Blockland folder.
// @return boolean	True if data was successfully loaded and false otherwise.
function SHOP_InvData::loadData(%this, %fn) {
  error("ERROR: Not implemented yet");
  return false;
}

package ItemShopPackage {
  function GameConnection::onClientEnterGame(%this) {
    Parent::onClientEnterGame(%this);
    %this.SHOP_inventory = SHOP_InvData();
    // TODO: Load inventory data.
  }

  function GameConnection::onClientLeaveGame(%this) {
    // TODO: Save inventory data.
    Parent::onClientEnterGame(%this);
  }
};
