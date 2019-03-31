// Create item price data.
// @param ItemData db	Item data for item.
// @param int price     Price of item in score points. Cannot be less than 0 and will be rounded down.
// @param boolean buyOnce	True if item only needs to be bought once and false if it single use.
// @return SHOP_PriceData	Item data if price and datablock are valid, no object otherwise.
function SHOP_PriceData(%db, %price, %buyOnce) {
  %price = mFloor(%price);

  // Check that price is valid and datablock exists.
  if (%price < 0 || !isObject(%db)) {
    error("ERROR:" SPC %price SPC "is an invalid price. Must be a non-negative number.");
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
// @param int price	New price of item in score points. Cannot be less than 0 and will be rounded down.
function SHOP_PriceData::setPrice(%this, %price) {
  %price = mFloor(%price);
  if (%price < 0) {
    error("ERROR:" SPC %price SPC "is an invalid price. Must be a non-negative number.");
    return;
  }
  %this.price = %price;
}

// Marks the item as buy once or single use.
// @param boolean buyOnce	True if the item only needs to be bought once, and false if it is single use.
function SHOP_PriceData::setBuyOnce(%this, %buyOnce) {
  %this.buyOnce = %buyOnce;
}
