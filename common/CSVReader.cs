// Original code by Greek2me. Code is not functionally identical to original.

// Creates a new CSVReader object.
// @param string delimiter	The delimiter to use. "\" is the escape character.
// @param string dataString	The string to parse. Not required in constructor.
// @return SHOP_CSVReader
function SHOP_CSVReader(%delimiter, %dataString)
{
  if (!strLen(%delimiter))
    %delimiter = ",";
  %reader = new ScriptObject() {
    class = SHOP_CSVReader;
    currentPosition = 0;
    delimiter = %delimiter;
    dataString = %dataString;
  };
  return %reader;
}

// Gives the CSVReader a new string to parse.
// @param string dataString	String to parse.
function SHOP_CSVReader::setDataString(%this, %dataString)
{
  %this.currentPosition = 0;
  %this.dataString = %dataString;
}

// Reads data from a comma-separated-value string.
// @return string	The next value found.
function SHOP_CSVReader::readNextValue(%this)
{
  %inQuotes = false;
  %quoteStart = -1;
  %quoteEnd = -1;
  %length = strLen(%this.dataString);
  for (%i = %this.currentPosition; %i <= %length; %i++) {
    %char = getSubStr(%this.dataString, %i, 1);
    if (%char $= "\\") {
      continue;
    }
    else if (%char $= "\"")  {
      if (%inQuotes)
	%quoteEnd = %i;
      else if (%quoteStart < 0)
	%quoteStart = %i + 1;
      else
	break;
      %inQuotes = !%inQuotes;
    }
    else if ((!%inQuotes && %char $= %this.delimiter) || %i >= %length) {
      if(%quoteStart < 0)
	%value = getSubStr(%this.dataString, %this.currentPosition, %i - %this.currentPosition);
      else if (%quoteEnd > %quoteStart)
	%value = getSubStr(%this.dataString, %quoteStart, %quoteEnd - %quoteStart);
      %this.currentPosition = %i + 1;
      break;
    }
  }
  return %value;
}

// Checks whether another value exists.
// @return boolean	True if there is another value to parse and false otherwise.
function SHOP_CSVReader::hasNextValue(%this)
{
  return %this.currentPosition < strLen(%this.dataString);
}
