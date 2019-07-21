//
// Player types that cannot pick up items or drop them on the ground. Dropped items will simply be deleted from the
// player's inventory.
//

if(ForceRequiredAddOn("Player_Quake") == $Error::AddOn_NotFound) {
	error("Player_Quake failed ot load! Item shop's lobby player types cannot be loaded");
	return $Error::AddOn_NotFound;
}

if(ForceRequiredAddOn("Player_No_Jet") == $Error::AddOn_NotFound) {
	error("Player_No_Jet failed to load! Item shop's lobby player types cannot be loaded");
	return $Error::AddOn_NotFound;
}

// A standard player who cannot pick up items or drop them on the ground.
datablock PlayerData(PlayerLobbyStandard : PlayerStandardArmor)
{
  SHOP_isLobby = true;
  uiName = "Lobby Player";
};

// A lobby player type without jets.
datablock PlayerData(PlayerLobbyNoJet : PlayerNoJet)
{
  SHOP_isLobby = true;
  uiName = "Lobby No-Jet Player";
};

// A quake-like lobby player.
datablock PlayerData(PlayerLobbyQuake : PlayerQuakeArmor)
{
  SHOP_isLobby = true;
  uiName = "Lobby Quake-Like Player";
};
