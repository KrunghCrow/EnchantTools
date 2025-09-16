using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Enchant Tools" , "Krungh Crow" , "2.0.0")]
    [Description("Adds enchanted tools for mining melted resources")]
    public class EnchantTools : RustPlugin
    {
        private ConfigData configData;

        #region Oxide Hooks
        private void Init()
        {
            LoadVariables();
            cmd.AddChatCommand(configData.Command , this , "CmdEnchant");
            cmd.AddConsoleCommand(configData.Command , this , "CcmdEnchant");
            permission.RegisterPermission("enchanttools.admin" , this);

            foreach (var tool in configData.Tools)
            {
                if (!string.IsNullOrEmpty(tool.Permission))
                {
                    permission.RegisterPermission(tool.Permission , this);
                }
            }
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string , string>
            {
                ["PermissionAdmin"] = "You don't have permission to use this command." ,
                ["MultiplePlayer"] = "Multiple players found: {0}" ,
                ["PlayerIsNotFound"] = "The player with the name {0} was not found." ,
                ["UsageSyntax"] = "Usage command syntax: \n<color=#FF99CC>{0} <tool_name> <playerName or Id></color>\nAvailable tools names:\n{1}" ,
                ["ToolGiven"] = "{0} received enchanted tool: {1}." ,
                ["CantRepair"] = "You can't repair an enchanted tools." ,
                ["ConsoleNotAvailable"] = "This command available only from server console or rcon." ,
                ["ConsoleNoPlayerFound"] = "No player with the specified SteamID was found." ,
                ["ConsoleNoPlayerAlive"] = "The player with the specified ID was not found among active or sleeping players." ,
                ["ConsoleToolGiven"] = "{0} received enchanted tool {1}." ,
                ["ConsoleUsageSyntax"] = "Usage command syntax: \n<color=#FF99CC>{0} <tool_name> <steamId></color>\nAvailable tools names:\n{1}"
            } , this);
        }
        #endregion

        #region Config
        private class ConfigData
        {
            public string Command;
            public List<Tool> Tools;
        }

        private void LoadVariables()
        {
            configData = Config.ReadObject<ConfigData>();
            if (configData == null || configData.Tools == null || string.IsNullOrEmpty(configData.Command))
            {
                LoadDefaultConfig();
            }
            else
            {
                bool configChanged = false;
                foreach (var tool in configData.Tools)
                {
                    if (string.IsNullOrEmpty(tool.Permission))
                    {
                        tool.Permission = $"enchanttools.{tool.shortname}";
                        configChanged = true;
                    }
                    if (tool.CanRepair == null)
                    {
                        tool.CanRepair = false;
                        configChanged = true;
                    }
                }
                if (configChanged)
                {
                    SaveConfig();
                }
            }
        }

        protected override void LoadDefaultConfig()
        {
            configData = new ConfigData
            {
                Command = "et" ,
                Tools = new List<Tool>()
                {
                    new Tool()
                    {
                        shortname = "hatchet",
                        skinId = 3554084772,
                        customName = "Enchanted Hatchet",
                        CanRepair = false,
                        Permission = "enchanttools.hatchet"
                    },
                    new Tool()
                    {
                        shortname = "axe.salvaged",
                        skinId = 0,
                        customName = "Enchanted Salvaged Axe",
                        CanRepair = false,
                        Permission = "enchanttools.axesalvaged"
                    },
                    new Tool()
                    {
                        shortname = "pickaxe",
                        skinId = 3116120925,
                        customName = "Enchanted Pickaxe",
                        CanRepair = false,
                        Permission = "enchanttools.pickaxe"
                    },
                    new Tool()
                    {
                        shortname = "icepick.salvaged",
                        skinId = 0,
                        customName = "Enchanted Salvaged Icepick",
                        CanRepair = false,
                        Permission = "enchanttools.icepicksalvaged"
                    },
                }
            };
            SaveConfig();
        }

        private class Tool
        {
            public string shortname;
            public ulong skinId;
            public string customName;
            public bool? CanRepair;
            public string Permission;
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(configData);
        }
        #endregion

        #region Hook Triggers
        private void OnDispenserBonus(ResourceDispenser dispenser , BaseEntity entity , Item item)
        {
            BasePlayer player = entity.ToPlayer();
            if (player == null) return;

            var activeItem = player.GetActiveItem();
            var toolInConfig = configData.Tools.FirstOrDefault(tool => tool.shortname == activeItem.info.shortname && activeItem.skin == tool.skinId);

            if (toolInConfig != null && permission.UserHasPermission(player.UserIDString , toolInConfig.Permission))
            {
                HandleItemReplacement(player , item);
            }
        }

        private void OnDispenserGather(ResourceDispenser dispenser , BaseEntity entity , Item item)
        {
            BasePlayer player = entity.ToPlayer();
            if (player == null) return;

            var activeItem = player.GetActiveItem();
            var toolInConfig = configData.Tools.FirstOrDefault(tool => tool.shortname == activeItem.info.shortname && activeItem.skin == tool.skinId);

            if (toolInConfig != null && permission.UserHasPermission(player.UserIDString , toolInConfig.Permission))
            {
                HandleItemReplacement(player , item);
            }
        }

        private object OnItemRepair(BasePlayer player , Item item)
        {
            var toolInConfig = configData.Tools.FirstOrDefault(tool => tool.shortname == item.info.shortname && item.skin == tool.skinId);

            if (toolInConfig != null && toolInConfig.CanRepair.HasValue && !toolInConfig.CanRepair.Value)
            {
                SendReply(player , lang.GetMessage("CantRepair" , this , player.UserIDString));
                return false;
            }
            return null;
        }
        #endregion

        #region Chat command
        private void CmdEnchant(BasePlayer player , string command , string[] args)
        {
            if (!player.IsAdmin && !permission.UserHasPermission(player.UserIDString , "enchanttools.admin"))
            {
                SendReply(player , lang.GetMessage("PermissionAdmin" , this , player.UserIDString));
                return;
            }

            List<string> shortnames = configData.Tools.Select(t => t.shortname).ToList();

            if (args.Length == 2 && shortnames.Contains(args[0]))
            {
                List<BasePlayer> foundPlayers = new List<BasePlayer>();
                string searchNameOrId = args[1];

                foreach (BasePlayer p in BasePlayer.activePlayerList)
                {
                    if (p.displayName.Contains(searchNameOrId) || p.UserIDString == searchNameOrId)
                    {
                        foundPlayers.Add(p);
                    }
                }

                if (foundPlayers.Count > 1)
                {
                    SendReply(player , lang.GetMessage("MultiplePlayer" , this , player.UserIDString) , string.Join(", " , foundPlayers.Select(p => p.displayName).ToArray()));
                    return;
                }
                else if (foundPlayers.Count == 0)
                {
                    SendReply(player , lang.GetMessage("PlayerIsNotFound" , this , player.UserIDString) , args[1]);
                    return;
                }
                else
                {
                    Tool tool = configData.Tools.Find(t => t.shortname == args[0]);
                    BasePlayer receiver = foundPlayers.First();
                    if (receiver.IsValid())
                    {
                        GiveEnchantTool(receiver , tool);
                        SendReply(player , lang.GetMessage("ToolGiven" , this , player.UserIDString) , receiver.displayName , tool.customName ?? args[0]);
                        return;
                    }
                }
            }
            SendReply(player , lang.GetMessage("UsageSyntax" , this , player.UserIDString) , configData.Command , string.Join(", " , shortnames.ToArray()));
        }
        #endregion

        #region Console command
        private void CcmdEnchant(ConsoleSystem.Arg arg)
        {
            if (!arg.IsServerside && !arg.IsRcon)
            {
                BasePlayer player = arg.Player();
                if (player != null)
                {
                    player.ConsoleMessage(lang.GetMessage("ConsoleNotAvailable" , this , player.UserIDString));
                }
                return;
            }

            List<string> shortnames = configData.Tools.Select(t => t.shortname).ToList();

            string[] args = arg.Args;
            if (args != null && args.Length == 2 && shortnames.Contains(args[0]))
            {
                string steamId = args[1];
                IPlayer iplayer = covalence.Players.FindPlayerById(steamId);
                BasePlayer receiver = BasePlayer.FindAwakeOrSleeping(steamId);

                if (iplayer == null)
                {
                    SendReply(arg , lang.GetMessage("ConsoleNoPlayerFound" , this));
                }
                else if (receiver == null)
                {
                    SendReply(arg , lang.GetMessage("ConsoleNoPlayerAlive" , this));
                }
                else
                {
                    Tool tool = configData.Tools.Find(t => t.shortname == args[0]);
                    GiveEnchantTool(receiver , tool);
                    SendReply(arg , lang.GetMessage("ConsoleToolGiven" , this) , receiver.displayName , tool.customName ?? args[0]);
                }
                return;
            }
            SendReply(arg , lang.GetMessage("ConsoleUsageSyntax" , this) , configData.Command , string.Join(", " , shortnames.ToArray()));
        }
        #endregion

        #region Helpers
        private void ReplaceAndGiveItem(BasePlayer player , Item item , int newItemId)
        {
            if (item != null && player != null)
            {
                int amount = item.amount;
                item.Remove();
                for (int i = 0; i < amount; i++)
                {
                    Item newItem = ItemManager.CreateByItemID(newItemId , 1);
                    if (newItem != null)
                    {
                        player.GiveItem(newItem);
                    }
                }
            }
        }

        private void HandleItemReplacement(BasePlayer player , Item item)
        {
            switch (item.info.shortname)
            {
                case "sulfur.ore":
                    ReplaceAndGiveItem(player , item , -1581843485);
                    break;
                case "metal.ore":
                    ReplaceAndGiveItem(player , item , 69511070);
                    break;
                case "hq.metal.ore":
                    ReplaceAndGiveItem(player , item , 317398316);
                    break;
                case "wood":
                    ReplaceAndGiveItem(player , item , -1938052175);
                    break;
                case "wolfmeat.raw":
                    ReplaceAndGiveItem(player , item , 813023040);
                    break;
                case "meat.boar":
                    ReplaceAndGiveItem(player , item , -242084766);
                    break;
                case "chicken.raw":
                    ReplaceAndGiveItem(player , item , -1848736516);
                    break;
                case "bearmeat":
                    ReplaceAndGiveItem(player , item , 1873897110);
                    break;
                case "deermeat.raw":
                    ReplaceAndGiveItem(player , item , -1509851560);
                    break;
                case "bigcatmeat":
                    ReplaceAndGiveItem(player , item , -1318837358);
                    break;
                case "snakemeat":
                    ReplaceAndGiveItem(player , item , -170436364);
                    break;
                case "crocodilemeat":
                    ReplaceAndGiveItem(player , item , 392828520);
                    break;
                case "fish.raw":
                    ReplaceAndGiveItem(player , item , 1668129151);
                    break;
                case "horsemeat.raw":
                    ReplaceAndGiveItem(player , item , -1162759543);
                    break;
                case "humanmeat.raw":
                    ReplaceAndGiveItem(player , item , 1536610005);
                    break;
            }
        }

        private void GiveEnchantTool(BasePlayer player , Tool tool)
        {
            Item item = CreateEnchantedTool(tool);
            if (item != null)
            {
                player.GiveItem(item);
            }
        }

        private Item CreateEnchantedTool(Tool tool)
        {
            Item item = ItemManager.CreateByName(tool.shortname , 1 , tool.skinId);
            if (item != null)
            {
                item.name = tool.customName;
            }
            return item;
        }
        #endregion

        #region API
        public bool IsEnchanted(Item item)
        {
            return configData.Tools.Any(tool => tool.shortname == item.info.shortname && item.skin == tool.skinId && item.name == tool.customName);
        }

        public Item AddTool(string toolShortname)
        {
            Tool tool = configData.Tools.FirstOrDefault(t => t.shortname == toolShortname);
            if (tool == null) return null;
            return CreateEnchantedTool(tool);
        }

        public Item AddRandomTool()
        {
            if (configData.Tools.Count == 0) return null;
            System.Random random = new System.Random();
            Tool randomTool = configData.Tools[random.Next(configData.Tools.Count)];
            return CreateEnchantedTool(randomTool);
        }
        #endregion
    }
}
