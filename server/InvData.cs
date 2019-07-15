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
function SHOP_InvData::saveData(%this, %fn)
{
  if (!isWriteableFileName(%fn)) {
    error("ERROR: \"" @ %fn @ "\" is not writable");
    return false;
  }

  // Write each item's datablock on its own line.
  %file = new FileObject();
  %file.openForWrite(%fn);
  %itemCnt = %this.items.getCount();
  for (%i = 0; %i < %itemCnt; %i++) {
    %item = %this.items.getObject(%i).getName();
    %file.writeLine(%item);
  }
  %file.close();

  return true;
}

// Loads inventory data from a file.
// @param string fn     Path of file relative to the Blockland folder.
// @return boolean	True if data was successfully loaded and false otherwise.
function SHOP_InvData::loadData(%this, %fn)
{
  if (!isFile(%fn)) {
    error("ERROR: \"" @ %fn @ "\" is not a file");
    return false;
  }

  %this.items.clear();

  %file = new FileObject();
  %file.openForRead(%fn);

  // Read item datablocks off each line.
  %lineNum = 0;
  while (!%file.isEOF()) {
    %item = trim(%file.readLine());
    if (%item !$= "") {
      if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
	error("ERROR: Invalid ItemData \"" @ %item @ "\" on line" SPC %lineNum @ ". Ignoring.");
	%lineNum++;
	continue;
      }
      %this.items.add(%item);
    }
    %lineNum++;
  }

  %file.close();

  return true;
}

// Gets the path to a player's inventory data file.
function GameConnection::SHOP_getInvPath(%this)
{
  if ($Server::LAN)
    // Use LAN name as unique identifier.
    return $SHOP::ClientDataPath @ strlwr(%this.LANname) @ ".itm";
  else
    // Use BLID as unique identifier.
    return $SHOP::ClientDataPath @ %this.bl_id @ ".itm";
}

// Saves a player's inventory data.
// @return boolean	True if data was successfully saved and false otherwise.
function GameConnection::SHOP_saveInvData(%this)
{
  %invPath = %this.SHOP_getInvPath();
  return %this.SHOP_inventory.saveData(%invPath);
}

// Loads a player's inventory data if it is saved.
// @return boolean	True if data was successfully loaded or there was no
//			data to load and false if something went wrong.
function GameConnection::SHOP_loadInvData(%this)
{
  %invPath = %this.SHOP_getInvPath();
  if (isFile(%invPath))
    return %this.SHOP_inventory.loadData(%invPath);
  else
    return true;
}

package ItemShopPackage
{
  function GameConnection::onClientEnterGame(%this)
  {
    Parent::onClientEnterGame(%this);
    %this.SHOP_inventory = SHOP_InvData();
    if (!%this.SHOP_loadInvData())
      error("ERROR: Failed to load inventory data for \"" @ %this.getName() @ "\"");
  }

  function GameConnection::onClientLeaveGame(%this)
  {
    if (!%this.SHOP_saveInvData())
      error("ERROR: Failed to write inventory data for \"" @ %this.getName() @ "\"");
    Parent::onClientEnterGame(%this);
  }
};
