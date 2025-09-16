**Enchant Tools** allows to give enchanted tools for mining, woodcuting and skining to player.

 * If player use enchanted tool for woodcuting, player will recieve charcoal instead wood.
 * If player use enchanted tool for skinnig, player will recieve cooked meat instead raw.
 * If player use enchanted tool for mining, player will recieve Metal Fragments instead Metal Ore, Sulfur instead Sulfur Ore and High Quality Metal instead High Quality Metal Ore.
 
To give an enchanted tool to player, you need to define and customize it in config file in section Tools. Plugin will generate a default config included few items, you can customise them or add new ones.

## Permissions

* `enchanttools.admin` - Allows players to give enchanted tools to other players
* rest of the permissions of using the tools can be set/added in the cfg  
## Chat Commands

* `/et <item shortname> <player name or id>`
  
## Console Commands

* et <item shortname> <player id>`
  
**Note:** Console command is available only from server console or RCON, but an API is provided for remote handouts

## Configuration

```json
{
  "Command": "et",
  "Tools": [
    {
      "shortname": "hatchet",
      "skinId": 3554084772,
      "customName": "Enchanted Hatchet",
      "CanRepair": true,
      "Permission": "enchanttools.hatchet"
    },
    {
      "shortname": "axe.salvaged",
      "skinId": 0,
      "customName": "Enchanted Salvaged Axe",
      "CanRepair": true,
      "Permission": "enchanttools.axe.salvaged"
    },
    {
      "shortname": "pickaxe",
      "skinId": 3116120925,
      "customName": "Enchanted Pickaxe",
      "CanRepair": false,
      "Permission": "enchanttools.pickaxe"
    },
    {
      "shortname": "icepick.salvaged",
      "skinId": 0,
      "customName": "Enchanted Salvaged Icepick",
      "CanRepair": true,
      "Permission": "enchanttools.icepick.salvaged"
    }
  ]
}
```

## Localization

```json
{
  "PermissionAdmin": "You don't have permission to use this command.",
  "MultiplePlayer": "Multiple players found: {0}",
  "PlayerIsNotFound": "The player with the name or SteamID {0} was not found.",
  "UsageSyntax": "Usage command syntax: \n<color=#FF99CC>{0} <tool_name> <playerName or Id></color>\nAvailable tools names:\n{1}",
  "ToolGiven": "{0} received enchanted tool: {1}.",
  "CantRepair": "You can't repair an enchanted tools.",
  "ConsoleNotAvailable": "This command available only from server console or rcon.",
  "ConsoleNoPlayerFound": "No player with the specified SteamID was found.",
  "ConsoleNoPlayerAlive": "The player with the specified ID was not found among active or sleeping players.",
  "ConsoleToolGiven": "{0} received enchanted tool {1}.",
  "ConsoleUsageSyntax": "Usage command syntax: \n<color=#FF99CC>{0} <tool_name> <steamId></color>\nAvailable tools names:\n{1}"
}
```

## API
```
IsEnchanted(Item item)
```
```cs
bool isEnchanted = EnchantTools.IsEnchanted(item);
```
or
```cs
Item someItem = player.ToPlayer().GetActiveItem();
if (EnchantTools.IsEnchanted(someItem))
{
    Puts("This item is an enchanted tool!");
}
```

```
AddTool(string toolShortname)
```
```cs
Item enchantedTool = EnchantTools.AddTool("pickaxe");
```
or
```cs
Item enchantedPickaxe = EnchantTools.AddTool("pickaxe");
if (enchantedPickaxe != null)
{
    player.ToPlayer().GiveItem(enchantedPickaxe);
}

```

```
AddRandomTool()
```
```cs
Item randomTool = EnchantTools.AddRandomTool();
```
or
```cs
Item randomEnchantedTool = EnchantTools.AddRandomTool();
if (randomEnchantedTool != null)
{
    player.ToPlayer().GiveItem(randomEnchantedTool);
}
```
## Credits

* Default : Original plugin Developer/Maintainer
* Krungh Crow : Current maintainer
