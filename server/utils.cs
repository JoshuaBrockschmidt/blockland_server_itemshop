//
// General-purpose utility functions.
//

// Gets the item object the client's player is looking at.
// @return ItemData	Item object if the client's player is looking at an item and -1 otherwise.
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
