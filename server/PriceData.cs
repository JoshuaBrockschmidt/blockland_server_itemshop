// Create item price data.
// @param ItemData db	Item data for item.
// @param int price     Price of item in score points. Will be rounded down.
//                 	A negative price will be interpreted as a pickup-able item.
// @param boolean buyOnce	True if item only needs to be bought once and false if it single use.
// @return SHOP_PriceData	Item data if price and datablock are valid, no object otherwise.
function SHOP_PriceData(%db, %price, %buyOnce)
{
  %price = mFloor(%price);

  // Check that datablock exists.
  if (!isObject(%db)) {
    error("ERROR: The datablock \"" @ %db @ "\" does not exist.");
    return;
  }

  %db = %db.getName();
  if (%db $= "" || %db.getClassName() !$= "ItemData") {
    error("ERROR: Invalid ItemData");
    return;
  }

  %this = new ScriptObject() {
    class = SHOP_PriceData;
    itemDb = %db;
    price = %price;
    buyOnce = %buyOnce;
  };

  return %this;
}

// Sets the price of an item.
// @param int price	New price of item in score points. Will be rounded down.
//                 	A negative price will be interpreted as a pickup-able item.
function SHOP_PriceData::setPrice(%this, %price)
{
  %price = mFloor(%price);
  %this.price = %price;
}

// Marks the item as buy once or single use.
// @param boolean buyOnce	True if the item only needs to be bought once, and false if it is single use.
function SHOP_PriceData::setBuyOnce(%this, %buyOnce)
{
  %this.buyOnce = %buyOnce;
}
