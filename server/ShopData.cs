// Create shop object for storing item data.
// @return SHOP_ShopData	New shop object.
function SHOP_ShopData() {
  %this = new ScriptGroup() {
    class = SHOP_ShopData;
  };
  return %this;
}

// Sets the price of an item.
// @param ItemData item      Item datablock of item.
// @param int price	New price of item in score points. Cannot be less than 0 and will be rounded down.
function SHOP_ShopData::setPrice(%this, %item, %price) {
  if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }
  %item = %item.getName();

  // Search for existing price data.
  %found = %this.prices[%item];

  if (isObject(%found)) {
    %found.setPrice(%price);
  } else {
    // Create a new price data object.
    %newPriceData = SHOP_PriceData(%item, %price, $SHOP::PREF::BuyOnceByDefault);
    if (isObject(%newPriceData)) {
      %this.add(%newPriceData);
      %this.prices[%item] = %newPriceData;
    } else {
      error("ERROR: Failed to create SHOP_PriceData");
      return;
    }
  }
}

// Gets the price of the item data.
// @param ItemData item     Item datablock of item.
// @return int	Price of item in score points. -1 if the item is not for sale.
function SHOP_ShopData::getPrice(%this, %item) {
  if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }
  %item = %item.getName();

  %found = %this.prices[%item];
  if (isObject(%found))
    return %found.price;
  else
    return -1;
}

// Makes an item unbuyable / not for sale.
// @param ItemData item      Item data for item.
function SHOP_ShopData::makeUnbuyable(%this, %item) {
  %found = %this.prices[%item.getName()];
  if (isObject(%found)) {
    %found.delete();
    %this.remove(%found);
  }
}

// Makes an item buy once or single use.
// @param ItemData item      Item data for item that is free or for sale.
// @param boolean buyOnce	True to make item buy once and false to make it single use.
function SHOP_ShopData::setBuyOnce(%this, %item, %buyOnce) {
  if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }
  %item = %item.getName();

  %priceData = %this.prices[%item];
  if (isObject(%priceData))
    %priceData.setBuyOnce(%buyOnce);
  else
    error("ERROR: Item is not for sale");
}

// Gets whether an item is buy once.
// @param ItemData item 	Item data for item that is free or for sale.
// @return boolean	True if the item is buy once and false if it is single use.
function SHOP_ShopData::getBuyOnce(%this, %item) {
  if (!isObject(%item) || %item.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }
  %item = %item.getName();

  %priceData = %this.prices[%item];
  if (isObject(%priceData))
    return %priceData.buyOnce;
  else
    error("ERROR: Item is not for sale");
}

// Clears all item prices making all items unbuyable.
function SHOP_ShopData::clearItems(%this) {
  %this.deleteAll();
  %this.clear();
}

// Saves item price data to a file. Will overwrite an existing file of the same name.
// @param string filename	Path of file relative to the Blockland folder.
// @return boolean	True if save was successful and false otherwise.
function SHOP_ShopData::saveData(%this, %filename) {
  if (!isWriteableFileName(%filename)) {
    error("ERROR: File \"" @ %filename @ "\" is not writable");
    return false;
  }

  %file = new FileObject();
  %file.openForWrite(%filename);

  // Write each item's price and buy once status in a CSV format, where each line is formatted as
  //     <string: ITEM DATABLOCK>,<int: PRICE>,<boolean: BUY ONCE>
  %priceCnt = %this.getCount();
  for (%i = 0; %i < %priceCnt; %i++) {
    %priceData = %this.getObject(%i);
    %buyOnce = %priceData.buyOnce ? 1 : 0;
    %file.writeLine("\"" @ %priceData.itemDb.getName() @ "\"," @ %priceData.price @ "," @ %buyOnce);
  }

  %file.close();

  return true;
}

// Loads item price data from a file. Prices for the shop will be cleared regardless
// of whether data loads successfully.
// @param string filename	Path of file relative to the Blockland folder.
// @return boolean	True if load was successful and false otherwise.
function SHOP_ShopData::loadData(%this, %filename) {
  %this.clearItems();

  if (!isFile(%filename)) {
    error("ERROR: File \"" @ %filename @ "\" is not a file");
    return false;
  }

  %file = new FileObject();
  %file.openForRead(%filename);
  %csvReader = SHOP_CSVReader(",");

  %lineNum = 1;
  while (!%file.isEOF()) {
    %line = trim(%file.readLine());
    if (%line !$= "") {
      %csvReader.setDataString(%line);

      if (!%csvReader.hasNextValue()) {
	error("ERROR: Truncated item datablock on line" SPC %lineNum);
	%file.close();
	return false;
      }

      // Get the item datablock.
      %item = %csvReader.readNextValue();
      if (%item $= "" || !isObject(%item) || %item.getClassName() !$= "ItemData") {
	error("ERROR: Invalid ItemData \"" @ %item @ "\" on line" SPC %lineNum @ ". Ignoring.");
	%lineNum++;
	continue;
      }

      if (!%csvReader.hasNextValue()) {
	error("ERROR: Truncated price on line" SPC %lineNum);
	%file.close();
	return false;
      }

      // Get the item's price.
      %price = %csvReader.readNextValue();
      if (%price $= "") {
	error("ERROR: Invalid integer price on line" SPC %lineNum);
	%file.close();
	return false;
      }
      %price = mFloor(%price);
      // TODO: Check if price is numeric

      if (!%csvReader.hasNextValue()) {
	error("ERROR: Truncated buy once status on line" SPC %lineNum);
	%file.close();
	return false;
      }

      // Get the item's buy once status.
      %buyOnce = %csvReader.readNextValue();
      if (%buyOnce $= "") {
	error("ERROR: Invalid buy once boolean on line" SPC %lineNum);
	%file.close();
	return false;
      }

      %this.setPrice(%item, %price);
      %this.setBuyOnce(%item, %buyOnce);
    }
    %lineNum++;
  }

  %file.close();

  return true;
}

// Sets a directory for automatically saving price data to.
// @param string dir	Path of base directory to automatically save price data within.
function SHOP_ShopData::setAutoSaveDir(%this, %dir) {
  %this.autoSaveDir = %dir;
}
