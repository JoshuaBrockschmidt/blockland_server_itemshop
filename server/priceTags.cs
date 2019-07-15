// A collection of all items with price tags.
// This is not added to SHOP_ServerGroup because the items stored inside it are not part of this add-on.
$SHOP::DisplayItems = new ScriptGroup();

// Used for displaying names on top of items.
datablock StaticShapeData(SHOPItemTextShape)
{
  shapefile = "base/data/shapes/empty.dts";
};

// Update the price tag displayed above an item, which includes the name and price.
function Item::SHOP_updatePriceTag(%this)
{
  %db = %this.getDatablock();
  %price = $SHOP::DefaultShopData.getPrice(%db);
  if (%price < 0) {
    %price = "not for sale";
    %this.setShapeNameColor($SHOP::PREF::NotForSaleColor);
  } else if (%price == 0) {
    %price = "free";
    %this.setShapeNameColor($SHOP::PREF::FreeColor);
  } else {
    %price = %price SPC "points";
    if ($SHOP::DefaultShopData.getBuyOnce(%db))
      %this.setShapeNameColor($SHOP::PREF::BuyOnceColor);
    else
      %this.setShapeNameColor($SHOP::PREF::SingleUseColor);
  }
  %name = %db.uiName SPC "(" @ %price @ ")";
  %this.setShapeName(%name);
  %this.setShapeNameDistance($SHOP::PREF::PriceTagDist);
}

function SHOP_updateAllPriceTags()
{
  %itemCnt = $SHOP::DisplayItems.getCount();
  for (%i = 0; %i < %itemCnt; %i++) {
    %item = $SHOP::DisplayItems.getObject(%i);
    %item.SHOP_updatePriceTag();
  }
}

package ItemShopPackage {
  function FxDTSBrick::setItem(%this, %db)
  {
    Parent::setItem(%this, %db);

    %item = %this.item;
    if (isObject(%item)) {
      $SHOP::DisplayItems.add(%item);
      %item.SHOP_updatePriceTag();
    }
  }
};
