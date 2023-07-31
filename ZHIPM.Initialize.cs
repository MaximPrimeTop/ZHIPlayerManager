using Microsoft.Xna.Framework;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace ZHIPlayerManager
{
    public partial class ZHIPM : TerrariaPlugin
    {
        /// <summary>
        /// 帮助指令方法指令
        /// </summary>
        /// <param name="args"></param>
        private void Help(CommandArgs args)
        {
            if (args.Parameters.Count != 0)
            {
                args.Player.SendInfoMessage("Type /zhelp to view command help");
            }
            else
            {
                args.Player.SendMessage("Type /zsave to backup your own character save\n" +
                                        "Type /zsaveauto [minute] to automatically backup your own character save every minute, and turn off this function when minute is 0\n" +
                                        "Type /zvisa [num] to see your character backup\n" +
                                        "Type /zvisa name [num] to view the player's character backup\n" +
                                        "Type /zhide kill to cancel kill + 1 display, use again to enable display\n" +
                                        "Type /zhide point to cancel + 1 $ display, use enable display again\n" +
                                        "Type /zback [name] to read the player's character file\n" +
                                        "Type /zback [name] [num] to read the player's character archive\n" +
                                        "Type /zclone [name1] [name2] to copy player 1's character data to player 2\n" +
                                        "Type /zclone [name] to copy the player's character data to yourself\n" +
                                        "Type /zmodify help to view help for modifying player data\n" +
                                        "Type /vi [name] to view the player's inventory\n" +
                                        "Type /vid [name] to view the player's inventory without categories\n" +
                                        "Type /vs [name] to view the player's status\n" +
                                        "Type /vs me to see your status\n" +
                                        "Type /zfre [name] to freeze the player\n" +
                                        "Type /zunfre [name] to unfreeze the player\n" +
                                        "Type /zunfre all to unfreeze all players\n" +
                                        "Type /zsort help to see help for the sort series commands\n" +
                                        "Type /zreset help to view the help of zreset series commands\n" +
                                        "Type /zban add [name] [reason] to ban online or offline players, reason is optional\n" +
                                        "Type /zclear useless to clear the world of dropped items, non-town or boss NPCs, and useless projectiles\n" +
                                        "Type /zclear buff [name] to clear all buffs for this player\n" +
                                        "Type /zclear buff all to clear all buffs for all players",
                                        TextColor()
                                        );
            }
        }


        /// <summary>
        /// 回档指令方法指令
        /// </summary>
        /// <param name="args"></param>
        private void MySSCBack(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                MySSCBack2(args, 1);
                return;
            }
            if (args.Parameters.Count == 2)
            {
                if (!int.TryParse(args.Parameters[1], out int num))
                {
                    args.Player.SendInfoMessage("Type /zback [name] to read the player's character file\nType /zback [name] [num] to read the number of the player's character file");
                    return;
                }
                if (num < 1 || num > config.MaximumNumberOfBackupFilesPerPlayer)
                {
                    args.Player.SendInfoMessage($"The player has a maximum of {config.MaximumNumberOfBackupFilesPerPlayer} backup files, range 1 ~ {config.MaximumNumberOfBackupFilesPerPlayer}, please re-enter");
                    return;
                }
                MySSCBack2(args, num);
            }
            else
            {
                args.Player.SendInfoMessage("Type /zback [name] to read the player's character file\nType /zback [name] [num] to read the number of the player's character file");
            }
        }


        /// <summary>
        /// 保存指令方法指令
        /// </summary>
        /// <param name="args"></param>
        private void MySSCSave(CommandArgs args)
        {
            if (args.Parameters.Count != 0)
            {
                args.Player.SendInfoMessage("Type /zsave to back up your character save");
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendInfoMessage("Incorrect object, please check your status, are you an in-game player?");
                return;
            }
            if (ZPDataBase.AddZPlayerDB(args.Player))
            {
                ExtraData? extraData = edPlayers.Find((ExtraData x) => x.Name == args.Player.Name);
                if (extraData != null)
                {
                    ZPExtraDB.WriteExtraDB(extraData);
                }
                args.Player.SendMessage("Your backup was saved successfully!", new Color(0, 255, 0));
            }
            else
            {
                args.Player.SendMessage("Your backup save failed! Please try re-entering the game", new Color(255, 0, 0));
            }
        }


        /// <summary>
        /// 自动备份指令
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MySSCSaveAuto(CommandArgs args)
        {
            if (!config.WhetherToEnableAutomaticPlayerBackup)
            {
                args.Player.SendMessage("Automatic backup is disabled, please contact administrator for details", new Color(255, 0, 0));
                return;
            }
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zsaveauto [minute] to automatically back up your own character archives every minute, and turn off this function when the minute is 0");
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendInfoMessage("Incorrect object, please check your status, are you an in-game player?");
                return;
            }
            if (int.TryParse(args.Parameters[0], out int num))
            {
                if (num < 0)
                {
                    args.Player.SendInfoMessage("The numbers are unreasonable");
                    return;
                }
                ExtraData? ex = edPlayers.Find(x => x.Name == args.Player.Name);
                if (ex == null)
                {
                    args.Player.SendInfoMessage("Modification failed, please re-enter the server and try again");
                    return;
                }
                ex.backuptime = num;
                if (num != 0)
                    args.Player.SendMessage("Modified successfully, your archive will be changed every " + num + " Automatic backup every minute, please pay attention to the archive overwriting, which may cover the part you manually backed up", new Color(0, 255, 0));
                else
                    args.Player.SendMessage("The modification is successful, your automatic backup has been turned off", new Color(0, 255, 0));
            }
            else
            {
                args.Player.SendInfoMessage("Type /zsaveauto [minute] to automatically back up your own character archives every minute, and turn off this function when the minute is 0");
            }
        }


        /// <summary>
        /// 查看我的存档方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ViewMySSCSave(CommandArgs args)
        {
            //查询本人
            if (args.Parameters.Count == 0 || args.Parameters.Count == 1 && int.TryParse(args.Parameters[0], out int num1))
            {
                if (!args.Player.IsLoggedIn)
                {
                    args.Player.SendInfoMessage("Incorrect object, please check your status, are you an in-game player?");
                    return;
                }
                int slot;
                if (args.Parameters.Count == 0)
                {
                    slot = 1;
                }
                else
                {
                    int num = int.Parse(args.Parameters[0]);
                    if (num < 1 || num > config.MaximumNumberOfBackupFilesPerPlayer)
                    {
                        args.Player.SendInfoMessage($"The player has a maximum of {config.MaximumNumberOfBackupFilesPerPlayer} backup files, range 1 ~ {config.MaximumNumberOfBackupFilesPerPlayer}, please re-enter");
                        return;
                    }
                    slot = num;
                }
                PlayerData playerData = ZPDataBase.ReadZPlayerDB(args.Player, args.Player.Account.ID, slot);
                if (playerData == null || !playerData.exists)
                {
                    args.Player.SendInfoMessage("You have not backed up");
                }
                else
                {
                    Item[] items = new Item[NetItem.MaxInventory];
                    for (int i = 0; i < NetItem.MaxInventory; i++)
                    {
                        items[i] = TShock.Utils.GetItemById(playerData.inventory[i].NetId);
                        items[i].stack = playerData.inventory[i].Stack;
                        items[i].prefix = playerData.inventory[i].PrefixId;
                    }
                    string text = GetItemsString(items, NetItem.MaxInventory, 0);
                    text = FormatArrangement(text, 30, " ");
                    string str = "Your backup [ " + args.Player.Account.ID + " - " + slot + " ]：\n" + text;
                    args.Player.SendInfoMessage(str);
                }
            }

            //查询他人
            else if (args.Parameters.Count == 1 || args.Parameters.Count == 2 && int.TryParse(args.Parameters[1], out int num2))
            {
                int slot;
                if (args.Parameters.Count == 1)
                {
                    slot = 1;
                }
                else
                {
                    int num = int.Parse(args.Parameters[1]);
                    if (num < 1 || num > config.MaximumNumberOfBackupFilesPerPlayer)
                    {
                        args.Player.SendInfoMessage($"The player has a maximum of {config.MaximumNumberOfBackupFilesPerPlayer}  backup files, range 1 ~ {config.MaximumNumberOfBackupFilesPerPlayer}, please re-enter");
                        return;
                    }
                    slot = num;
                }

                int ID = -1;
                string playerfullname = "";
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                if (list.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (users.Count == 1 || users.Count > 1 && users.Exists(x => x.Name == args.Parameters[0]))
                    {
                        if (users.Count == 1)
                        {
                            ID = users[0].ID;
                            playerfullname = users[0].Name;
                        }
                        else
                        {
                            UserAccount? temp = users.Find(x => x.Name == args.Parameters[0]);
                            ID = temp.ID;
                            playerfullname = temp.Name;
                        }
                    }
                    else if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                        return;
                    }
                    else
                    {
                        args.Player.SendInfoMessage(manyplayer);
                        return;
                    }
                }
                else
                {
                    ID = list[0].Account.ID;
                    playerfullname = list[0].Name;
                }
                PlayerData playerData = ZPDataBase.ReadZPlayerDB(new TSPlayer(-1), ID, slot);
                if (playerData == null || !playerData.exists)
                {
                    args.Player.SendInfoMessage("This player has not backed up");
                }
                else
                {
                    Item[] items = new Item[NetItem.MaxInventory];
                    for (int i = 0; i < NetItem.MaxInventory; i++)
                    {
                        items[i] = TShock.Utils.GetItemById(playerData.inventory[i].NetId);
                        items[i].stack = playerData.inventory[i].Stack;
                        items[i].prefix = playerData.inventory[i].PrefixId;
                    }
                    string text = "";
                    if (args.Player.IsLoggedIn)
                    {
                        text = GetItemsString(items, NetItem.MaxInventory, 0);
                        text = FormatArrangement(text, 30, " ");
                    }
                    else
                    {
                        text = GetItemsString(items, NetItem.MaxInventory, 1);
                    }
                    string str = "The content of player [ " + playerfullname + " ]'s backup  [ " + ID + " - " + slot + " ]：\n" + text;
                    args.Player.SendInfoMessage(str);
                }
            }

            else
            {
                args.Player.SendInfoMessage("Type /zvisa [num] to view your character backup\nType /zvisa name [num] to view the player's character backup");
            }
        }


        /// <summary>
        /// 克隆另一个人的数据的方法指令
        /// </summary>
        /// <param name="args"></param>
        private void SSCClone(CommandArgs args)
        {
            if (args.Parameters.Count == 0 || args.Parameters.Count > 2)
            {
                args.Player.SendInfoMessage("Type /zclone [name1] [name2] to copy player 1's character data to player 2\nType /zclone [name] to copy that player's character data to yourself");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0] == args.Player.Name)
                {
                    args.Player.SendMessage("Cloning failed! please don't clone yourself", new Color(255, 0, 0));
                    return;
                }
                if (!args.Player.IsLoggedIn)
                {
                    args.Player.SendInfoMessage("Incorrect object, please check your status, are you an in-game player?");
                    return;
                }
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                //找不到人，查离线
                if (list.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (user == null)
                    {
                        if (users.Count == 0)
                        {
                            args.Player.SendInfoMessage(noplayer);
                            return;
                        }
                        else if (users.Count > 1)
                        {
                            args.Player.SendInfoMessage(manyplayer);
                            return;
                        }
                        else
                            user = users[0];
                    }
                    PlayerData playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user.ID);
                    if (UpdatePlayerAll(args.Player, playerData))
                    {
                        args.Player.SendMessage("Cloned successfully! You have cloned the data of player [" + user.Name + "]", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("Cloning failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                }
                //人太多，舍弃
                else if (list.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                //一个在线
                else
                {
                    PlayerData playerData = list[0].PlayerData;
                    playerData.CopyCharacter(list[0]);
                    playerData.exists = true;
                    if (UpdatePlayerAll(args.Player, playerData))
                    {
                        args.Player.SendMessage("Cloned successfully! You have cloned the data of player [" + list[0].Name + "] onto you", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("Cloning failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                }
            }
            if (args.Parameters.Count == 2)
            {
                List<TSPlayer> player1 = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                List<TSPlayer> player2 = BestFindPlayerByNameOrIndex(args.Parameters[1]);
                if (player1.Count > 1 || player2.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                //都在线的情况
                if (player1.Count == 1 && player2.Count == 1)
                {
                    if (player1[0].Name == player2[0].Name)
                    {
                        args.Player.SendInfoMessage("Please don't clone the same person");
                        return;
                    }
                    player1[0].PlayerData.CopyCharacter(player1[0]);
                    player1[0].PlayerData.exists = true;
                    if (UpdatePlayerAll(player2[0], player1[0].PlayerData))
                    {
                        if (args.Player.Account.ID != player2[0].Account.ID)
                        {
                            args.Player.SendMessage($"Cloned successfully! You have cloned player [{player1[0].Name}] data onto [{player2[0].Name}]", new Color(0, 255, 0));
                        }
                        else
                        {
                            player2[0].SendMessage("Cloned successfully! Cloned data of player [" + player1[0].Name + "] to you", new Color(0, 255, 0));
                        }
                    }
                    else
                    {
                        args.Player.SendMessage("Cloning failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                    return;
                }
                //赋值者不在线，被赋值者在线的情况
                if (player1.Count == 0 && player2.Count == 1)
                {
                    args.Player.SendInfoMessage("Player 1 is not online and is querying offline data");
                    UserAccount user1 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    List<UserAccount> user1s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (user1 == null)
                    {
                        if (user1s.Count == 0)
                        {
                            args.Player.SendInfoMessage("Player 1 does not exist");
                            return;
                        }
                        else if (user1s.Count > 1)
                        {
                            args.Player.SendInfoMessage("Player 1 is not unique");
                            return;
                        }
                        else
                            user1 = user1s[0];
                    }
                    PlayerData playerData1 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user1.ID);
                    if (UpdatePlayerAll(player2[0], playerData1))
                    {
                        if (args.Player.Account.ID != player2[0].Account.ID)
                        {
                            args.Player.SendMessage($"Cloned successfully! Cloned player [{user1.Name}] data to player [{player2[0].Name}]", new Color(0, 255, 0));
                        }
                        else
                        {
                            player2[0].SendMessage("Cloned successfully! You have cloned player [" + user1.Name + "]", new Color(0, 255, 0));
                        }
                    }
                    else
                    {
                        args.Player.SendMessage("Cloning failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                    return;
                }
                //赋值者在线，被赋值者不在线的情况
                if (player1.Count == 1 && player2.Count == 0)
                {
                    args.Player.SendInfoMessage("Player 2 is not online and is querying offline data");
                    UserAccount user2 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                    List<UserAccount> user2s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[1], true);
                    if (user2 == null)
                    {
                        if (user2s.Count == 0)
                        {
                            args.Player.SendInfoMessage("Player 2 does not exist");
                            return;
                        }
                        else if (user2s.Count > 1)
                        {
                            args.Player.SendInfoMessage("Player 2 is not unique");
                            return;
                        }
                        else
                            user2 = user2s[0];
                    }
                    PlayerData playerData1 = player1[0].PlayerData;
                    playerData1.exists = true;
                    if (UpdateTshockDBCharac(user2.ID, playerData1))
                    {
                        args.Player.SendMessage($"Cloned successfully! Cloned player [{player1[0].Name}] data to player [{user2.Name}]", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("Cloning failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                    return;
                }
                //都不在线
                if (player1.Count == 0 && player2.Count == 0)
                {
                    args.Player.SendInfoMessage("Players are not online, offline data is being queried");
                    UserAccount user1 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    List<UserAccount> user1s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    UserAccount user2 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                    List<UserAccount> user2s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[1], true);
                    if (user1 == null)
                    {
                        if (user1s.Count == 0)
                        {
                            args.Player.SendInfoMessage("Player 1 does not exist");
                            return;
                        }
                        else if (user1s.Count > 1)
                        {
                            args.Player.SendInfoMessage("Player 1 is not the only一");
                            return;
                        }
                        else
                            user1 = user1s[0];
                    }
                    if (user2 == null)
                    {
                        if (user2s.Count == 0)
                        {
                            args.Player.SendInfoMessage("Player 2 does not exist");
                            return;
                        }
                        else if (user2s.Count > 1)
                        {
                            args.Player.SendInfoMessage("Player 2 is not unique");
                            return;
                        }
                        else
                            user2 = user2s[0];
                    }
                    PlayerData playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user1.ID);
                    if (UpdateTshockDBCharac(user2.ID, playerData))
                    {
                        args.Player.SendMessage($"Cloned successfully! Cloned player [{user1.Name}] data to player  [{user2.Name}]", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("Cloning failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                    return;
                }
            }
        }


        /// <summary>
        /// 修改人物数据方法指令
        /// </summary>
        /// <param name="args"></param>
        private void SSCModify(CommandArgs args)
        {
            if (args.Parameters.Count != 1 && args.Parameters.Count != 3)
            {
                args.Player.SendInfoMessage("Type /zmodify help to view the command help for modifying player data");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    string temp = config.WhetherToEnablePointStatistics ? "\nType /zmodify [name] point [num] to modify player points" : "";
                    args.Player.SendMessage(
                        "Type /zmodify [name] life [num] to modify the player's health\n" +
                        "Type /zmodify [name] lifemax [num] to modify the player's max health\n" +
                        "Type /zmodify [name] mana [num] to modify a player's mana\n" +
                        "Type /zmodify [name] manamax [num] to modify the player's max mana\n" +
                        "Type /zmodify [name] fish [num] to modify the player's number of angler quests\n" +
                        "Type /zmodify [name] torch [0 or 1] to disable or enable torch god favour\n" +
                        "Type /zmodify [name] demmon [0 or 1] to disable or enable the demon heart\n" +
                        "Type /zmodify [name] bread [0 or 1] to disable or enable artisan loaf buff\n" +
                        "Type /zmodify [name] heart [0 or 1] to disable or enable aegis crystal buff\n" +
                        "Type /zmodify [name] fruit [0 or 1] to disable or enable aegis fruit buff\n" +
                        "Type /zmodify [name] star [0 or 1] to disable or enable arcane crystal buffs\n" +
                        "Type /zmodify [name] pearl [0 or 1] to disable or enable the galaxy pearl buff\n" +
                        "Type /zmodify [name] worm [0 or 1] to disable or enable gummy worm buff\n" +
                        "Type /zmodify [name] ambrosia [0 or 1] to disable or enable ambrosia buff\n" +
                        "Type /zmodify [name] cart [0 or 1] to disable or enable super minecart buff\n" +
                        "Type /zmodify [name] all [0 or 1] to disable or enable all player buffs" + temp
                        , TextColor());
                }
                else
                {
                    args.Player.SendInfoMessage("Type /zmodify help to view the command help for modifying player data");
                }
                return;
            }
            if (args.Parameters.Count == 3)
            {
                //对参数3先判断是不是数据，不是数字结束
                if (!int.TryParse(args.Parameters[2], out int num))
                {
                    args.Player.SendInfoMessage("wrong format! Type /zmodify help to view the command help for modifying player data");
                    return;
                }
                //再判断能不能找到人的情况
                List<TSPlayer> players = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                if (players.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                //在线能找到
                if (players.Count == 1)
                {
                    if (args.Parameters[1].Equals("life", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statLife = num;
                        players[0].SendData(PacketTypes.PlayerHp, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Your health has been modified to: " + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("lifemax", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statLifeMax = num;
                        players[0].SendData(PacketTypes.PlayerHp, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Your max health has been modified to: " + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("mana", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statMana = num;
                        players[0].SendData(PacketTypes.PlayerMana, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Your mana has been modified to: " + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("manamax", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statManaMax = num;
                        players[0].SendData(PacketTypes.PlayerMana, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Your max mana has been modified to: " + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("fish", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.anglerQuestsFinished = num;
                        players[0].SendData(PacketTypes.NumberOfAnglerQuestsCompleted, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Your angler quest completions has been modified to: " + num, new Color(0, 255, 0));
                    }
                    else if (config.WhetherToEnablePointStatistics && args.Parameters[1].Equals("point", StringComparison.OrdinalIgnoreCase))
                    {
                        ExtraData? ex = edPlayers.Find(x => x.Name == players[0].Name);
                        if (ex != null)
                        {
                            ex.point = num;
                            players[0].SendMessage("Your points have been modified to: " + num, new Color(0, 255, 0));
                        }
                        else
                        {
                            args.Player.SendInfoMessage("Unexpected error, please try again or let the player re-enter the game");
                            return;
                        }
                    }
                    else if (num != 0 && num != 1)
                    {
                        args.Player.SendInfoMessage("wrong format! Type /zmodify help to view the command help for modifying player data");
                        return;
                    }
                    else if (args.Parameters[1].Equals("torch", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.unlockedBiomeTorches = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Torch god favor is active: " + players[0].TPlayer.unlockedBiomeTorches, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("demmon", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.extraAccessory = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Demon heart is active: " + players[0].TPlayer.extraAccessory, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("bread", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.ateArtisanBread = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Artisan loaf is active: " + players[0].TPlayer.ateArtisanBread, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("heart", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedAegisCrystal = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Aegis crystal is active:" + players[0].TPlayer.usedAegisCrystal, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("fruit", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedAegisFruit = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Aegis fruit is active: " + players[0].TPlayer.usedAegisFruit, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("star", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedArcaneCrystal = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Arcane crystal is active:" + players[0].TPlayer.usedArcaneCrystal, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("pearl", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedGalaxyPearl = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Galaxy pearl is active: " + players[0].TPlayer.usedGalaxyPearl, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("worm", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedGummyWorm = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Gummy worm is active: " + players[0].TPlayer.usedGummyWorm, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("ambrosia", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedAmbrosia = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Ambrosia is active: " + players[0].TPlayer.usedAmbrosia, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("cart", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.unlockedSuperCart = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("Super minecart is active: " + players[0].TPlayer.unlockedSuperCart, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        if (num == 1)
                        {
                            players[0].TPlayer.unlockedBiomeTorches = true;
                            players[0].TPlayer.extraAccessory = true;
                            players[0].TPlayer.ateArtisanBread = true;
                            players[0].TPlayer.usedAegisCrystal = true;
                            players[0].TPlayer.usedAegisFruit = true;
                            players[0].TPlayer.usedArcaneCrystal = true;
                            players[0].TPlayer.usedGalaxyPearl = true;
                            players[0].TPlayer.usedGummyWorm = true;
                            players[0].TPlayer.usedAmbrosia = true;
                            players[0].TPlayer.unlockedSuperCart = true;
                            players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                            players[0].SendMessage("All permanent buffs are active", new Color(0, 255, 0));
                        }
                        else if (num == 0)
                        {
                            players[0].TPlayer.unlockedBiomeTorches = false;
                            players[0].TPlayer.extraAccessory = false;
                            players[0].TPlayer.ateArtisanBread = false;
                            players[0].TPlayer.usedAegisCrystal = false;
                            players[0].TPlayer.usedAegisFruit = false;
                            players[0].TPlayer.usedArcaneCrystal = false;
                            players[0].TPlayer.usedGalaxyPearl = false;
                            players[0].TPlayer.usedGummyWorm = false;
                            players[0].TPlayer.usedAmbrosia = false;
                            players[0].TPlayer.unlockedSuperCart = false;
                            players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                            players[0].SendMessage("All permanent buffs are deactive", new Color(0, 255, 0));
                        }
                        else
                        {
                            args.Player.SendInfoMessage("Wrong format! Type /zmodify help to view the command help for modifying player data");
                        }
                    }
                    args.Player.SendMessage("Successfully modified!", new Color(0, 255, 0));
                }
                //不在线，修改离线数据
                else if (players.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    if (user == null)
                    {
                        List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                        if (users.Count == 0)
                        {
                            args.Player.SendInfoMessage(noplayer);
                            return;
                        }
                        else if (users.Count > 1)
                        {
                            args.Player.SendInfoMessage(manyplayer);
                            return;
                        }
                        else
                            user = users[0];

                    }
                    try
                    {
                        if (args.Parameters[1].Equals("life", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET Health = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("lifemax", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET MaxHealth= @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("mana", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET Mana = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("manamax", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET MaxMana = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("fish", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET questsCompleted = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (config.WhetherToEnablePointStatistics && args.Parameters[1].Equals("point", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE Zhipm_PlayerExtra SET point = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (num != 0 && num != 1)
                        {
                            args.Player.SendInfoMessage("Wrong format! Type /zmodify help to view the command help for modifying player data");
                            return;
                        }
                        else if (args.Parameters[1].Equals("torch", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET unlockedBiomeTorches = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("demmon", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET extraSlot = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("bread", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET ateArtisanBread = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("crystal", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedAegisCrystal = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("fruit", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedAegisFruit = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("arcane", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedArcaneCrystal = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("pearl", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedGalaxyPearl = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("worm", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedGummyWorm = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("ambrosia", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedAmbrosia = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("cart", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET unlockedSuperCart = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET unlockedBiomeTorches = @1, extraSlot = @2, ateArtisanBread = @3, usedAegisCrystal = @4, usedAegisFruit = @5, usedArcaneCrystal = @6, usedGalaxyPearl = @7, usedGummyWorm = @8, usedAmbrosia = @9, unlockedSuperCart = @10 WHERE Account = @0;", new object[]
                            {
                                    user.ID, num, num, num, num, num, num, num, num, num, num
                            });
                        }
                        args.Player.SendMessage("Successfully modified!", new Color(0, 255, 0));
                    }
                    catch (Exception ex)
                    {
                        args.Player.SendMessage("Failed to edit! Error: " + ex.ToString(), new Color(255, 0, 0));
                        TShock.Log.Error("Failed to edit! Error: " + ex.ToString());
                    }
                }
            }
        }


        /// <summary>
        /// 重置User备份数据库方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayerDB(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zresetdb [name] to clear the backup data for this player\nType /zresetdb all to clear the backup data for all players");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (ZPDataBase.ClearALLZPlayerDB(ZPDataBase))
                {
                    if (!args.Player.IsLoggedIn)
                    {
                        args.Player.SendMessage("All players' backup data has been reset", broadcastColor);
                        TSPlayer.All.SendMessage("All players' backup data has been reset", broadcastColor);
                    }
                    else
                    {
                        TSPlayer.All.SendMessage("All players' backup data has been reset", broadcastColor);
                    }
                }
                else
                {
                    args.Player.SendMessage("Reset failed", new Color(255, 0, 0));
                }
            }
            else
            {
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                if (list.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                if (list.Count == 1)
                {
                    if (ZPDataBase.ClearZPlayerDB(list[0].Account.ID))
                    {
                        args.Player.SendMessage($"Backup data reseted for player [ {list[0].Name} ]", new Color(0, 255, 0));
                        list[0].SendMessage("Your backup data has been reseted", broadcastColor);
                    }
                    else
                    {
                        args.Player.SendMessage("Reset failed", new Color(255, 0, 0));
                    }
                    return;
                }
                if (list.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    if (user == null)
                    {
                        args.Player.SendMessage(noplayer, new Color(255, 0, 0));
                    }
                    else
                    {
                        if (ZPDataBase.ClearZPlayerDB(user.ID))
                        {
                            args.Player.SendMessage($"Backup data reset for offline player [ {user.Name} ]", new Color(0, 255, 0));
                        }
                        else
                        {
                            args.Player.SendMessage("Reset failed", new Color(255, 0, 0));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 重置User额外数据库方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayerEX(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zresetex [name] to clear extra data for that player\nType /zresetex all to clear extra data for all players");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (ZPExtraDB.ClearALLZPlayerExtraDB(ZPExtraDB))
                {
                    edPlayers.Clear();
                    if (!args.Player.IsLoggedIn)
                    {
                        args.Player.SendMessage("Extra data for all players has been reseted", broadcastColor);
                        TSPlayer.All.SendMessage("Extra data for all players has been reseted", broadcastColor);
                    }
                    else
                    {
                        TSPlayer.All.SendMessage("Extra data for all players has been reseted", broadcastColor);
                    }
                }
                else
                {
                    args.Player.SendMessage("Reset failed", new Color(255, 0, 0));
                }
                return;
            }
            List<TSPlayer> tSPlayers = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (tSPlayers.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
                return;
            }
            if (tSPlayers.Count == 1)
            {
                if (ZPExtraDB.ClearZPlayerExtraDB(tSPlayers[0].Account.ID))
                {
                    edPlayers.RemoveAll((ExtraData x) => x.Name == tSPlayers[0].Name);
                    args.Player.SendMessage($"Reseted extra data for player [ {tSPlayers[0].Name} ]", new Color(0, 255, 0));
                    tSPlayers[0].SendMessage("Your extra data has been reset", broadcastColor);
                }
                else
                {
                    args.Player.SendMessage("Reset failed", new Color(255, 0, 0));
                }
                return;
            }
            if (tSPlayers.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user == null)
                {
                    args.Player.SendMessage(noplayer, new Color(255, 0, 0));
                }
                else
                {
                    if (ZPExtraDB.ClearZPlayerExtraDB(user.ID))
                    {
                        args.Player.SendMessage($"Extra data reset for offline player [ {user.Name} ]", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("Reset failed", new Color(255, 0, 0));
                    }
                }
            }
        }


        /// <summary>
        /// 重置玩家的人物数据方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zreset [name] to clear character data for that player\nType /zreset all to clear character data for all players");
                return;
            }
            if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                args.Player.SendInfoMessage(
                                            "Type /zresetdb [name] to clear the player's backup data\n" +
                                            "Type /zresetdb all to clear all player backup data\n" +
                                            "Type /zresetex [name] to clear extra data for this player\n" +
                                            "Type /zresetex all to clear extra data for all players\n" +
                                            "Type /zreset [name] to clear the player's character data\n" +
                                            "Type /zreset all to clear all players' character data\n" +
                                            "Type /zresetallplayers to clear all data for all players"
                                            , TextColor());
                return;
            }
            else if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                foreach (TSPlayer ts in TShock.Players)
                {
                    if (ts != null && ts.IsLoggedIn)
                    {
                        ResetPlayer(ts);
                    }
                }
                TShock.DB.Query("delete from tsCharacter");
                if (!args.Player.IsLoggedIn)
                {
                    args.Player.SendMessage("All players' character stats have been reset", broadcastColor);
                    TSPlayer.All.SendMessage("All players' character stats have been reset", broadcastColor);
                }
                else
                {
                    TSPlayer.All.SendMessage("All players' character stats have been reset", broadcastColor);
                }
                return;
            }
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (list.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
                return;
            }
            if (list.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user == null)
                {
                    args.Player.SendInfoMessage(noplayer);
                }
                else
                {
                    if (TShock.CharacterDB.RemovePlayer(user.ID))
                    {
                        args.Player.SendMessage($"Data reset for offline player [ {user.Name} ]", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("Reset failed! The player was not found in the original database, please check whether the input is correct, whether the player avoids SSC detection, and then re-enter", new Color(255, 0, 0));
                    }
                }
                return;
            }
            if (list.Count == 1)
            {
                if (ResetPlayer(list[0]) | TShock.CharacterDB.RemovePlayer(list[0].Account.ID))
                {
                    args.Player.SendMessage($"Data reset for player [ {list[0].Name} ]", new Color(0, 255, 0));
                    list[0].SendMessage("Character data has been reseted", broadcastColor);
                }
                else
                {
                    args.Player.SendInfoMessage("Reset failed");
                }
                return;
            }
        }


        /// <summary>
        /// 重置所有User所有数据方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayerAll(CommandArgs args)
        {
            if (args.Parameters.Count != 0)
            {
                args.Player.SendInfoMessage("Type /zresetallplayers to clear all data for all players");
                return;
            }
            try
            {
                foreach (TSPlayer tsplayer in TShock.Players)
                {
                    if (tsplayer != null && tsplayer.IsLoggedIn)
                    {
                        ResetPlayer(tsplayer);
                    }
                }
                TShock.DB.Query("delete from tsCharacter");
                ZPDataBase.ClearALLZPlayerDB(ZPDataBase);
                ZPExtraDB.ClearALLZPlayerExtraDB(ZPExtraDB);
                edPlayers.Clear();
            }
            catch (Exception ex)
            {
                args.Player.SendMessage("Reset failed  ZResetPlayerAll :" + ex.ToString(), new Color(255, 0, 0));
                TShock.Log.Error("Reset failed ZResetPlayerAll :" + ex.ToString());
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendMessage("Players are all initialized", new Color(0, 255, 0));
                TSPlayer.All.SendMessage("All data for all players has been fully initialized", broadcastColor);
            }
            else
            {
                TShock.Utils.Broadcast("All data for all players has been fully initialized", broadcastColor);
            }
        }


        /// <summary>
        /// 分类查阅指令
        /// </summary>
        /// <param name="args"></param>
        private void ViewInvent(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /vi [playername] to view the player's inventory");
                return;
            }
            //显示模式
            int model = args.Player.IsLoggedIn ? 0 : 1;

            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (list.Count > 0)
            {
                foreach (var li in list)
                {
                    StringBuilder sb = new StringBuilder();
                    string inventory = GetItemsString(li.TPlayer.inventory, NetItem.InventorySlots, model);

                    //装备栏一堆
                    string armor = GetItemsString(li.TPlayer.armor, NetItem.ArmorSlots, model);
                    string armor1 = GetItemsString(li.TPlayer.Loadouts[0].Armor, NetItem.ArmorSlots, model);
                    string armor2 = GetItemsString(li.TPlayer.Loadouts[1].Armor, NetItem.ArmorSlots, model);
                    string armor3 = GetItemsString(li.TPlayer.Loadouts[2].Armor, NetItem.ArmorSlots, model);

                    //染料一堆
                    string dyestuff = GetItemsString(li.TPlayer.dye, NetItem.DyeSlots, model);
                    string dyestuff1 = GetItemsString(li.TPlayer.Loadouts[0].Dye, NetItem.DyeSlots, model);
                    string dyestuff2 = GetItemsString(li.TPlayer.Loadouts[1].Dye, NetItem.DyeSlots, model);
                    string dyestuff3 = GetItemsString(li.TPlayer.Loadouts[2].Dye, NetItem.DyeSlots, model);

                    string misc = GetItemsString(li.TPlayer.miscEquips, NetItem.MiscEquipSlots, model);
                    string miscDye = GetItemsString(li.TPlayer.miscDyes, NetItem.MiscDyeSlots, model);

                    string trash = "";
                    if (model == 0 && !li.TPlayer.trashItem.IsAir)
                        trash = string.Format("【[i/s{0}:{1}]】 ", li.TPlayer.trashItem.stack, li.TPlayer.trashItem.netID);
                    else if (model == 1 && !li.TPlayer.trashItem.IsAir)
                        trash = $" [{Lang.prefix[li.TPlayer.trashItem.prefix].Value}.{li.TPlayer.trashItem.Name}:{li.TPlayer.trashItem.stack}] ";

                    string pig = GetItemsString(li.TPlayer.bank.item, NetItem.PiggySlots, model);
                    string safe = GetItemsString(li.TPlayer.bank2.item, NetItem.SafeSlots, model);
                    string forge = GetItemsString(li.TPlayer.bank3.item, NetItem.ForgeSlots, model);
                    string vault = GetItemsString(li.TPlayer.bank4.item, NetItem.VoidSlots, model);

                    if (list.Count == 1)
                        sb.AppendLine("【" + li.Name + "】 inventory:");
                    else
                        sb.AppendLine("Multiple results. 【" + li.Name + "】 inventory:");

                    if (inventory.Length > 0 && inventory != null && inventory != "")
                    {
                        sb.AppendLine("Inventory:");
                        if (model == 0)
                            sb.AppendLine(FormatArrangement(inventory, 30, " "));
                        else
                            sb.AppendLine(inventory);
                    }
                    //装备栏
                    if (armor.Length > 0 && armor != null && armor != "")
                    {
                        sb.AppendLine("Armor + Accessories + Vanity:");
                        sb.AppendLine("Current equipments: ");
                        sb.AppendLine(armor);
                    }
                    if (armor1.Length > 0 && armor1 != null && armor1 != "")
                    {
                        sb.AppendLine("Loadout 1: ");
                        sb.AppendLine(armor1);
                    }
                    if (armor2.Length > 0 && armor2 != null && armor2 != "")
                    {
                        sb.AppendLine("Loadout 2: ");
                        sb.AppendLine(armor2);
                    }
                    if (armor3.Length > 0 && armor3 != null && armor3 != "")
                    {
                        sb.AppendLine("Loadout 3: ");
                        sb.AppendLine(armor3);
                    }
                    //染料
                    if (dyestuff.Length > 0 && dyestuff != null && dyestuff != "")
                    {
                        sb.AppendLine("Current dye:");
                        sb.AppendLine(dyestuff);
                    }
                    if (dyestuff1.Length > 0 && dyestuff1 != null && dyestuff1 != "")
                    {
                        sb.AppendLine("Loadotu 1 dye:");
                        sb.AppendLine(dyestuff1);
                    }
                    if (dyestuff2.Length > 0 && dyestuff2 != null && dyestuff2 != "")
                    {
                        sb.AppendLine("Loadout 2 dye:");
                        sb.AppendLine(dyestuff2);
                    }
                    if (dyestuff3.Length > 0 && dyestuff3 != null && dyestuff3 != "")
                    {
                        sb.AppendLine("Loadout 3 dye:");
                        sb.AppendLine(dyestuff3);
                    }


                    if (misc.Length > 0 && misc != null && misc != "")
                    {
                        sb.AppendLine("Pet + Minecart + Mount + Grapple:");
                        sb.AppendLine(misc);
                    }
                    if (miscDye.Length > 0 && miscDye != null && miscDye != "")
                    {
                        sb.AppendLine("Pet Minecart Mount Hook Dye:");
                        sb.AppendLine(miscDye);
                    }
                    if (trash != "")
                    {
                        sb.AppendLine("Trash:");
                        sb.AppendLine(trash);
                    }
                    if (pig.Length > 0 && pig != null && pig != "")
                    {
                        sb.AppendLine("Piggy bank:");
                        if (model == 0)
                            sb.AppendLine(FormatArrangement(pig, 30, " "));
                        else
                            sb.AppendLine(pig);
                    }
                    if (safe.Length > 0 && safe != null && safe != "")
                    {
                        sb.AppendLine("safe:");
                        if (model == 0)
                            sb.AppendLine(FormatArrangement(safe, 30, " "));
                        else
                            sb.AppendLine(safe);
                    }
                    if (forge.Length > 0 && forge != null && forge != "")
                    {
                        sb.AppendLine("Defender's forge:");
                        if (model == 0)
                            sb.AppendLine(FormatArrangement(forge, 30, " "));
                        else
                            sb.AppendLine(forge);
                    }
                    if (vault.Length > 0 && vault != null && vault != "")
                    {
                        sb.AppendLine("Void bag:");
                        if (model == 0)
                            sb.AppendLine(FormatArrangement(vault, 30, " "));
                        else
                            sb.AppendLine(vault);
                    }
                    if (sb.Length > 0 && sb != null && !string.IsNullOrEmpty(sb.ToString()))
                        args.Player.SendMessage(sb.ToString() + "\n", TextColor());
                    else
                        args.Player.SendInfoMessage("Player 【" + li.Name + "】 is not carrying anything");
                }
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                if (temp.Count == 1 || temp.Count > 1 && temp.Exists(x => x.Name == name))
                {
                    if (temp.Count == 1)
                    {
                        PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), temp[0].ID);
                        if (temp2 != null && temp2.exists)
                        {
                            users.Add(temp[0], temp2);
                        }
                    }
                    else
                    {
                        UserAccount u = temp.Find(x => x.Name == name);
                        PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), u.ID);
                        if (temp2 != null && temp2.exists)
                        {
                            users.Add(u, temp2);
                        }
                    }
                }
                else
                {
                    foreach (var t in temp)
                    {
                        PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                        if (t != null && t2.exists)
                        {
                            users.Add(t, t2);
                        }
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendInfoMessage(noplayer);
                }
                else
                {
                    foreach (var p in users)
                    {
                        string offAll = GetItemsString(p.Value.inventory, p.Value.inventory.Length, model);
                        if (model == 0)
                            offAll = FormatArrangement(offAll, 30, " ");
                        offAll += "\n";
                        if (!string.IsNullOrEmpty(offAll))
                        {
                            if (users.Count > 1)
                                args.Player.SendMessage("Multiple results. Inventory of player 【" + p.Key.Name + "】:" + "\n" + offAll, TextColor());
                            else
                                args.Player.SendMessage("Inventory of player 【" + p.Key.Name + "】:" + "\n" + offAll, TextColor());
                        }
                        else
                        {
                            args.Player.SendInfoMessage("Player 【" + p.Key.Name + "】 is not carrying anything\n");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 不分类查阅指令
        /// </summary>
        /// <param name="args"></param>
        private void ViewInventDisorder(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /vid [playername] to see player's inventory without sorting");
                return;
            }
            int model = args.Player.IsLoggedIn ? 0 : 1;

            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (list.Count > 0)
            {
                foreach (var li in list)
                {
                    string inventory = GetItemsString(li.TPlayer.inventory, NetItem.InventorySlots, model);

                    string armor = GetItemsString(li.TPlayer.armor, NetItem.ArmorSlots, model);
                    string armor1 = GetItemsString(li.TPlayer.Loadouts[0].Armor, NetItem.ArmorSlots, model);
                    string armor2 = GetItemsString(li.TPlayer.Loadouts[1].Armor, NetItem.ArmorSlots, model);
                    string armor3 = GetItemsString(li.TPlayer.Loadouts[2].Armor, NetItem.ArmorSlots, model);


                    string dyestuff = GetItemsString(li.TPlayer.dye, NetItem.DyeSlots, model);
                    string dyestuff1 = GetItemsString(li.TPlayer.Loadouts[0].Dye, NetItem.DyeSlots, model);
                    string dyestuff2 = GetItemsString(li.TPlayer.Loadouts[1].Dye, NetItem.DyeSlots, model);
                    string dyestuff3 = GetItemsString(li.TPlayer.Loadouts[2].Dye, NetItem.DyeSlots, model);


                    string misc = GetItemsString(li.TPlayer.miscEquips, NetItem.MiscEquipSlots, model);
                    string miscDye = GetItemsString(li.TPlayer.miscDyes, NetItem.MiscDyeSlots, model);

                    string trash = "";
                    if (model == 0 && !li.TPlayer.trashItem.IsAir)
                        trash = string.Format("【[i/s{0}:{1}]】 ", li.TPlayer.trashItem.stack, li.TPlayer.trashItem.netID);
                    else if (model == 1 && !li.TPlayer.trashItem.IsAir)
                        trash = $"[{Lang.prefix[li.TPlayer.trashItem.prefix].Value}.{li.TPlayer.trashItem.Name}]";

                    string pig = GetItemsString(li.TPlayer.bank.item, NetItem.PiggySlots, model);
                    string safe = GetItemsString(li.TPlayer.bank2.item, NetItem.SafeSlots, model);
                    string forge = GetItemsString(li.TPlayer.bank3.item, NetItem.ForgeSlots, model);
                    string vault = GetItemsString(li.TPlayer.bank4.item, NetItem.VoidSlots, model);

                    string all = inventory + armor + armor1 + armor2 + armor3 + dyestuff + dyestuff1 + dyestuff2 + dyestuff3 + misc + misc + miscDye + trash + pig + safe + forge + vault;
                    if (model == 0)
                        all = FormatArrangement(all, 30, " ");

                    if (!string.IsNullOrWhiteSpace(all))
                    {
                        if (list.Count == 1)
                            args.Player.SendMessage("Inventory of player 【" + li.Name + "】:\n" + all + "\n", TextColor());
                        else
                            args.Player.SendMessage("Multiple results. Inventory of player 【" + li.Name + "】:\n" + all + "\n", TextColor());
                    }
                    else
                        args.Player.SendInfoMessage("Player 【" + li.Name + "】s not carrying anything\n");
                }
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                if (temp.Count == 1 || temp.Count > 1 && temp.Exists(x => x.Name == name))
                {
                    if (temp.Count == 1)
                    {
                        PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), temp[0].ID);
                        if (temp2 != null && temp2.exists)
                        {
                            users.Add(temp[0], temp2);
                        }
                    }
                    else
                    {
                        UserAccount u = temp.Find(x => x.Name == name);
                        PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), u.ID);
                        if (temp2 != null && temp2.exists)
                        {
                            users.Add(u, temp2);
                        }
                    }
                }
                else
                {
                    foreach (var t in temp)
                    {
                        PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                        if (t != null && t2.exists)
                        {
                            users.Add(t, t2);
                        }
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendInfoMessage(noplayer);
                }
                else
                {
                    foreach (var p in users)
                    {
                        string offAll = GetItemsString(p.Value.inventory, p.Value.inventory.Length, model);
                        if (model == 0)
                            offAll = FormatArrangement(offAll, 30, " ");

                        if (!string.IsNullOrWhiteSpace(offAll))
                        {
                            if (users.Count > 1)
                                args.Player.SendMessage("Multiple results. Inventor of player 【" + p.Key.Name + "】:" + "\n" + offAll + "\n", TextColor());
                            else
                                args.Player.SendMessage("Inventories of player 【" + p.Key.Name + "】:" + "\n" + offAll + "\n", TextColor());
                        }
                        else
                        {
                            args.Player.SendInfoMessage("Player 【" + p.Key.Name + "】 is not carrying anything\n");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 查询玩家的状态
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ViewState(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /vs [playername] to see the player's stats");
                return;
            }
            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (name.Equals("me", StringComparison.OrdinalIgnoreCase) && args.Player.IsLoggedIn)
            {
                list.Clear();
                list.Add(args.Player);
            }

            if (args.Player.IsLoggedIn)
            {
                if (list.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    ExtraData? ex = edPlayers.Find(x => x.Name == list[0].Name);
                    sb.AppendLine("【" + list[0].Name + "】 stats:");
                    sb.AppendLine("Max health[i:29]: " + list[0].TPlayer.statLifeMax + "   Current health[i:58]:" + list[0].TPlayer.statLife);
                    sb.AppendLine("Max mana[i:109]: " + list[0].TPlayer.statManaMax2 + "   Current mana[i:184]: " + list[0].TPlayer.statMana);
                    sb.AppendLine("Angler quests completed[i:3120]:" + list[0].TPlayer.anglerQuestsFinished);
                    sb.AppendLine("Coins [i:855]: " + cointostring(getPlayerCoin(list[0].Name)));
                    sb.Append("Buffs[i:678]: ");
                    int flag = 0;
                    foreach (int buff in list[0].TPlayer.buffType)
                    {
                        if (buff != 0)
                        {
                            flag++;
                            sb.Append(Lang.GetBuffName(buff) + "  ");
                            if (flag == 12)
                                sb.AppendLine();
                        }
                    }
                    if (flag == 0)
                    {
                        sb.Append("None");
                    }
                    sb.AppendLine();

                    sb.Append("Permanent buffs: ");
                    flag = 0;
                    if (list[0].TPlayer.extraAccessory)
                    {
                        flag++;
                        sb.Append("[i:3335]  ");
                    }
                    if (list[0].TPlayer.unlockedBiomeTorches)
                    {
                        flag++;
                        sb.Append("[i:5043]  ");
                    }
                    if (list[0].TPlayer.ateArtisanBread)
                    {
                        flag++;
                        sb.Append("[i:5326]  ");
                    }
                    if (list[0].TPlayer.usedAegisCrystal)
                    {
                        flag++;
                        sb.Append("[i:5337]  ");
                    }
                    if (list[0].TPlayer.usedAegisFruit)
                    {
                        flag++;
                        sb.Append("[i:5338]  ");
                    }
                    if (list[0].TPlayer.usedArcaneCrystal)
                    {
                        flag++;
                        sb.Append("[i:5339]  ");
                    }
                    if (list[0].TPlayer.usedGalaxyPearl)
                    {
                        flag++;
                        sb.Append("[i:5340]  ");
                    }
                    if (list[0].TPlayer.usedGummyWorm)
                    {
                        flag++;
                        sb.Append("[i:5341]  ");
                    }
                    if (list[0].TPlayer.usedAmbrosia)
                    {
                        flag++;
                        sb.Append("[i:5342]  ");
                    }
                    if (list[0].TPlayer.unlockedSuperCart)
                    {
                        flag++;
                        sb.Append("[i:5289]");
                    }
                    if (flag == 0)
                    {
                        sb.Append("None");
                    }
                    sb.AppendLine();
                    if (ex != null)
                    {
                        if (config.WhetherToEnableOnlineTimeStatistics)
                            sb.AppendLine("Playtime[i:3099]: " + timetostring(ex.time));
                        if (config.WhetherToEnableDeathStatistics)
                            sb.AppendLine("Deaths[i:321]: " + ex.deathCount);
                        if (config.WhetherToEnableKillNPCStatistics)
                        {
                            sb.AppendLine("Mobs killed[i:3095]:" + ex.killNPCnum + " ");
                            sb.AppendLine("Bosses killed[i:3868]: " + DictionaryToVSString(ex.killBossID));
                            sb.AppendLine("Rare mobs killed[i:4274]: " + DictionaryToVSString(ex.killRareNPCID));
                        }
                        if (config.WhetherToEnablePointStatistics && config.WhetherToEnableKillNPCStatistics)
                            sb.AppendLine("Points[i:575]: " + ex.point);
                    }

                    args.Player.SendMessage(sb.ToString(), TextColor());
                }
                else
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                    List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                    if (temp.Count == 1 || temp.Count > 1 && temp.Exists(x => x.Name == name))
                    {
                        if (temp.Count == 1)
                        {
                            PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), temp[0].ID);
                            if (temp2 != null && temp2.exists)
                            {
                                users.Add(temp[0], temp2);
                            }
                        }
                        else
                        {
                            UserAccount u = temp.Find(x => x.Name == name);
                            PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), u.ID);
                            if (temp2 != null && temp2.exists)
                            {
                                users.Add(u, temp2);
                            }
                        }
                    }
                    else
                    {
                        foreach (var t in temp)
                        {
                            PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                            if (t != null && t2.exists)
                            {
                                users.Add(t, t2);
                            }
                        }
                    }
                    if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                    }
                    else
                    {
                        foreach (var p in users)
                        {
                            StringBuilder sb = new StringBuilder();
                            ExtraData? ex = ZPExtraDB.ReadExtraDB(p.Key.ID);
                            if (users.Count == 1)
                                sb.AppendLine("【" + p.Key.Name + "】stats:");
                            else
                                sb.AppendLine("【" + p.Key.Name + "】 stats:");

                            sb.AppendLine("Max health[i:29]: " + p.Value.maxHealth + "   Curret health[i:58]: " + p.Value.health);
                            sb.AppendLine("Max mana[i:109]: " + p.Value.maxMana + "   Current mana[i:184]: " + p.Value.mana);
                            sb.AppendLine("Angler quests completed[i:3120]:" + p.Value.questsCompleted);
                            sb.AppendLine("Coins[i:855]: " + cointostring(getPlayerCoin(p.Key.Name)));
                            sb.Append("Permant buffs: ");
                            int flag = 0;
                            if (p.Value.extraSlot != null && p.Value.extraSlot.GetValueOrDefault() == 1)
                            {
                                flag++;
                                sb.Append("[i:3335]  ");
                            }
                            if (p.Value.unlockedBiomeTorches == 1)
                            {
                                flag++;
                                sb.Append("[i:5043]  ");
                            }
                            if (p.Value.ateArtisanBread == 1)
                            {
                                flag++;
                                sb.Append("[i:5326]  ");
                            }
                            if (p.Value.usedAegisCrystal == 1)
                            {
                                flag++;
                                sb.Append("[i:5337]  ");
                            }
                            if (p.Value.usedAegisFruit == 1)
                            {
                                flag++;
                                sb.Append("[i:5338]  ");
                            }
                            if (p.Value.usedArcaneCrystal == 1)
                            {
                                flag++;
                                sb.Append("[i:5339]  ");
                            }
                            if (p.Value.usedGalaxyPearl == 1)
                            {
                                flag++;
                                sb.Append("[i:5340]  ");
                            }
                            if (p.Value.usedGummyWorm == 1)
                            {
                                flag++;
                                sb.Append("[i:5341]  ");
                            }
                            if (p.Value.usedAmbrosia == 1)
                            {
                                flag++;
                                sb.Append("[i:5342]  ");
                            }
                            if (p.Value.unlockedSuperCart == 1)
                            {
                                flag++;
                                sb.Append("[i:5289]");
                            }
                            if (flag == 0)
                            {
                                sb.Append("None");
                            }
                            sb.AppendLine();
                            if (ex != null)
                            {
                                if (config.WhetherToEnableOnlineTimeStatistics)
                                    sb.AppendLine("Playtime[i:3099]: " + timetostring(ex.time));
                                if (config.WhetherToEnableDeathStatistics)
                                    sb.AppendLine("Deaths[i:321]: " + ex.deathCount);
                                if (config.WhetherToEnableKillNPCStatistics)
                                {
                                    sb.AppendLine("Mobs killed[i:3095]: " + ex.killNPCnum + " ");
                                    sb.AppendLine("Bosses killed[i:3868]: " + DictionaryToVSString(ex.killBossID));
                                    sb.AppendLine("Rare mobs killed[i:4274]: " + DictionaryToVSString(ex.killRareNPCID));
                                }
                                if (config.WhetherToEnablePointStatistics && config.WhetherToEnableKillNPCStatistics)
                                    sb.AppendLine("Points[i:575]: " + ex.point);
                            }

                            args.Player.SendMessage(sb.ToString(), TextColor());
                        }
                    }
                }
            }
            else
            {
                if (list.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    ExtraData? ex = edPlayers.Find(x => x.Name == list[0].Name);
                    sb.AppendLine("【" + list[0].Name + "】 stats:");
                    sb.AppendLine("Max health: " + list[0].TPlayer.statLifeMax + "   Current health:" + list[0].TPlayer.statLife);
                    sb.AppendLine("Max mana: " + list[0].TPlayer.statManaMax2 + "   Current mana: " + list[0].TPlayer.statMana);
                    sb.AppendLine("Angler quests completed: " + list[0].TPlayer.anglerQuestsFinished);
                    sb.AppendLine("Coins: " + cointostring(getPlayerCoin(list[0].Name), 1));
                    sb.Append("Buff: ");
                    int flag = 0;
                    foreach (int buff in list[0].TPlayer.buffType)
                    {
                        if (buff != 0)
                        {
                            flag++;
                            sb.Append(Lang.GetBuffName(buff) + "  ");
                        }
                    }
                    if (flag == 0)
                    {
                        sb.Append("None");
                    }
                    sb.AppendLine();

                    sb.Append("Permanent buffs:");
                    flag = 0;
                    if (list[0].TPlayer.extraAccessory)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(3335) + "  ");
                    }
                    if (list[0].TPlayer.unlockedBiomeTorches)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5043) + "  ");
                    }
                    if (list[0].TPlayer.ateArtisanBread)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5326) + "  ");
                    }
                    if (list[0].TPlayer.usedAegisCrystal)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5337) + "  ");
                    }
                    if (list[0].TPlayer.usedAegisFruit)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5338) + "  ");
                    }
                    if (list[0].TPlayer.usedArcaneCrystal)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5339) + "  ");
                    }
                    if (list[0].TPlayer.usedGalaxyPearl)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5340) + "  ");
                    }
                    if (list[0].TPlayer.usedGummyWorm)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5341) + "  ");
                    }
                    if (list[0].TPlayer.usedAmbrosia)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5342) + "  ");
                    }
                    if (list[0].TPlayer.unlockedSuperCart)
                    {
                        flag++;
                        sb.Append(Lang.GetItemNameValue(5289));
                    }
                    if (flag == 0)
                    {
                        sb.Append("None");
                    }
                    sb.AppendLine();
                    if (ex != null)
                    {
                        if (config.WhetherToEnableOnlineTimeStatistics)
                            sb.AppendLine("Playtime: " + timetostring(ex.time));
                        if (config.WhetherToEnableDeathStatistics)
                            sb.AppendLine("Detahs: " + ex.deathCount);
                        if (config.WhetherToEnableKillNPCStatistics)
                        {
                            sb.AppendLine("Mobs killed: " + ex.killNPCnum + " ");
                            sb.AppendLine("Bosses killed: " + DictionaryToVSString(ex.killBossID, false));
                            sb.AppendLine("Rare mobs killed: " + DictionaryToVSString(ex.killRareNPCID, false));
                        }
                        if (config.WhetherToEnableKillNPCStatistics && config.WhetherToEnablePointStatistics)
                            sb.AppendLine("Points: " + ex.point);
                    }

                    args.Player.SendMessage(sb.ToString(), TextColor());
                }
                else
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                    List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                    if (temp.Count == 1 || temp.Count > 1 && temp.Exists(x => x.Name == name))
                    {
                        if (temp.Count == 1)
                        {
                            PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), temp[0].ID);
                            if (temp2 != null && temp2.exists)
                            {
                                users.Add(temp[0], temp2);
                            }
                        }
                        else
                        {
                            UserAccount u = temp.Find(x => x.Name == name);
                            PlayerData temp2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), u.ID);
                            if (temp2 != null && temp2.exists)
                            {
                                users.Add(u, temp2);
                            }
                        }
                    }
                    else
                    {
                        foreach (var t in temp)
                        {
                            PlayerData t2;
                            if (t != null)
                            {
                                t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                                if (t2.exists)
                                {
                                    users.Add(t, t2);
                                }
                            }
                        }
                    }
                    if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                    }
                    else
                    {
                        foreach (var p in users)
                        {
                            StringBuilder sb = new StringBuilder();
                            ExtraData? ex = ZPExtraDB.ReadExtraDB(p.Key.ID);
                            if (users.Count == 1)
                                sb.AppendLine("【" + p.Key.Name + "】Stats:");
                            else
                                sb.AppendLine("Multiple results. 【" + p.Key.Name + "】 stats:");
                            sb.AppendLine("Max health: " + p.Value.maxHealth + "   Current health: " + p.Value.health);
                            sb.AppendLine("Max mana: " + p.Value.maxMana + "   Current mana: " + p.Value.mana);
                            sb.AppendLine("Angler quests completed: " + p.Value.questsCompleted);
                            sb.AppendLine("Coins: " + cointostring(getPlayerCoin(p.Key.Name), 1));
                            sb.Append("Permanent buffs: ");
                            int flag = 0;
                            if (p.Value.extraSlot != null && p.Value.extraSlot.GetValueOrDefault() == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(3335) + "  ");
                            }
                            if (p.Value.unlockedBiomeTorches == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5043) + "  ");
                            }
                            if (p.Value.ateArtisanBread == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5326) + "  ");
                            }
                            if (p.Value.usedAegisCrystal == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5337) + "  ");
                            }
                            if (p.Value.usedAegisFruit == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5338) + "  ");
                            }
                            if (p.Value.usedArcaneCrystal == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5339) + "  ");
                            }
                            if (p.Value.usedGalaxyPearl == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5340) + "  ");
                            }
                            if (p.Value.usedGummyWorm == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5341) + "  ");
                            }
                            if (p.Value.usedAmbrosia == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5342) + "  ");
                            }
                            if (p.Value.unlockedSuperCart == 1)
                            {
                                flag++;
                                sb.Append(Lang.GetItemNameValue(5289));
                            }
                            if (flag == 0)
                            {
                                sb.Append("None");
                            }
                            sb.AppendLine();
                            if (ex != null)
                            {
                                if (config.WhetherToEnableOnlineTimeStatistics)
                                    sb.AppendLine("Playtime: " + timetostring(ex.time));
                                if (config.WhetherToEnableDeathStatistics)
                                    sb.AppendLine("Deaths: " + ex.deathCount);
                                if (config.WhetherToEnableKillNPCStatistics)
                                {
                                    sb.AppendLine("Mobs killed: " + ex.killNPCnum + " ");
                                    sb.AppendLine("Bosses killed: " + DictionaryToVSString(ex.killBossID, false));
                                    sb.AppendLine("Rare mobs kille: " + DictionaryToVSString(ex.killRareNPCID, false));
                                }
                                if (config.WhetherToEnablePointStatistics && config.WhetherToEnableKillNPCStatistics)
                                    sb.AppendLine("Points: " + ex.point);
                            }

                            args.Player.SendMessage(sb.ToString(), TextColor());
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 清理
        /// </summary>
        /// <param name="args"></param>
        private void Clear(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendInfoMessage("Type /zclear useless to clear the world of dropped items, non-town or boss NPCs, and useless projectiles\nType /zclear buff [name] to clear all buffs for that player\nType /zclear buff all to clear all buffs for all players");
                return;
            }
            if (args.Parameters.Count == 1 && args.Parameters[0].Equals("useless", StringComparison.OrdinalIgnoreCase))
            {
                cleartime = Timer + 1200L;
                if (!args.Player.IsLoggedIn)
                    args.Player.SendMessage("The server will clear useless all NPCs, projectiles and items in the world after 20 seconds", new Color(255, 0, 0));
                TSPlayer.All.SendMessage("The server will clear all useless NPCs, projectiles and items in the world after 20 seconds", new Color(255, 0, 0));
            }
            else if (args.Parameters.Count == 2 && args.Parameters[0].Equals("buff", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (TSPlayer tSPlayer in TShock.Players)
                    {
                        if (tSPlayer != null && tSPlayer.IsLoggedIn)
                        {
                            clearAllBuffFromPlayer(tSPlayer);
                        }
                    }
                    args.Player.SendMessage($"All Buffs have been removed from all players", new Color(0, 255, 0));
                    return;
                }
                List<TSPlayer> ts = BestFindPlayerByNameOrIndex(args.Parameters[1]);
                if (ts.Count == 0)
                {
                    args.Player.SendInfoMessage("The player is offline or does not exist");
                }
                else if (ts.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                }
                else
                {
                    clearAllBuffFromPlayer(ts[0]);
                    args.Player.SendMessage($"[ {ts[0].Name} ] buffs removed", new Color(0, 255, 0));
                }
            }
            else
            {
                args.Player.SendInfoMessage("Type /zclear useless to clear the world of dropped items, non-town or boss NPCs, and useless projectiles\nType /zclear buff [name] to clear all buffs for that player\nType /zclear buff all to clear all buffs for all players");
            }
        }


        /// <summary>
        /// 游戏更新，用来实现人物额外数据如时间的同步增加，这里实现对进服人员的添加和time自增
        /// </summary>
        /// <param name="args"></param>
        private void OnGameUpdate(EventArgs args)
        {
            //自制计时器，60 Timer = 1 秒
            Timer++;
            //以秒为单位处理，降低计算机计算量
            if (Timer % 60L == 0L)
            {
                //在线时长 +1 的部分，遍历在线玩家，对time进行自增
                TSPlayer[] players = TShock.Players;
                for (int i = 0; i < players.Length; i++)
                {
                    TSPlayer tsp = players[i];
                    if (tsp != null && tsp.IsLoggedIn)
                    {
                        //如果当前玩家已存在，那么更新额外数据
                        ExtraData? extraData = edPlayers.Find((ExtraData x) => x.Name == tsp.Name);
                        if (extraData != null)
                        {
                            if (config.WhetherToEnableOnlineTimeStatistics)
                            {
                                extraData.time += 1L;
                                if (extraData.time % 1800L == 0L)
                                {
                                    if (config.WhetherToEnablePointStatistics)
                                    {
                                        extraData.point += 1000;
                                        SendText(tsp, "Bonus points + 1000", broadcastColor, tsp.TPlayer.Center);
                                    }
                                    tsp.SendMessage("You are already online " + timetostring(extraData.time), broadcastColor);
                                    TShock.Log.Info("Player " + extraData.Name + " already online " + timetostring(extraData.time));
                                    NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(tsp.TPlayer.Center, 4), tsp.Index, -1);
                                    Projectile.NewProjectile(null, tsp.TPlayer.Center, -Vector2.UnitY * 4f, Main.rand.Next(415, 419), 0, 0f, -1, 0f, 0f, 0f);
                                }
                            }
                        }
                        //否则查找是否已注册过
                        else
                        {
                            //注册过了，那么读取
                            ExtraData? extraData2 = ZPExtraDB.ReadExtraDB(tsp.Account.ID);
                            if (extraData2 != null)
                            {
                                edPlayers.Add(extraData2);
                            }
                            //否则创建一个新的
                            else
                            {
                                ExtraData ex = new ExtraData(tsp.Account.ID, tsp.Name, 0L, config.TheDefaultAutomaticBackupTimeInMinutes_IfIts0ItMeansOff, 0, 0L, !config.WhetherTheDefaultKillFontIsDisplayedToPlayers, !config.WhetherTheDefaultPipFontIsDisplayedToThePlayer);
                                ZPExtraDB.WriteExtraDB(ex);
                                edPlayers.Add(ex);
                            }
                        }
                    }
                }


                //自动备份的处理部分，这里以分钟为单位 3600L = 1 分钟，默认用全局计时器进行备份的部分
                if (config.WhetherToEnableAutomaticPlayerBackup && Timer % 3600L == 0L)
                {
                    foreach (var ex in edPlayers)
                    {//到达备份间隔时长，备份一次
                        foreach (TSPlayer ts in TShock.Players)
                        {
                            if (ts != null && ts.IsLoggedIn && ts.Name == ex.Name && ex.backuptime != 0L && Timer % (3600L * ex.backuptime) == 0L)
                            {
                                ZPExtraDB.WriteExtraDB(ex);
                                ZPDataBase.AddZPlayerDB(ts);
                                ts.SendMessage("Characters has been automatically backed up", new Color(0, 255, 0));
                                TShock.Log.Info($"【{ts.Name}】's character archived and backed up");
                            }
                        }
                    }
                }


                //清理世界无效数据处理
                if (Timer > cleartime)
                {
                    foreach (var v in Main.npc)
                    {
                        if (v.active && !v.boss && !v.townNPC)
                        {
                            v.active = false;
                            NetMessage.SendData(23, -1, -1, null, v.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                    foreach (var v in Main.projectile)
                    {
                        if (v.active)
                        {
                            v.active = false;
                            TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", v.identity, v.owner);
                        }
                    }
                    foreach (var v in Main.item)
                    {
                        if (v.active)
                        {
                            v.active = false;
                            TSPlayer.All.SendData(PacketTypes.ItemDrop, "", v.whoAmI, 0f, 0f, 0f, 0);
                        }
                    }
                    cleartime = long.MaxValue;
                    TSPlayer.All.SendMessage("Cleaned up all projectiles, items, useless NPCs", new Color(65, 165, 238));
                }
            }

            //冻结处理
            if (frePlayers.Count != 0)
            {
                foreach (var v in frePlayers)
                {
                    TShock.Players.ForEach(x =>
                    {
                        if (x != null && x.IsLoggedIn && (x.UUID.Equals(v.uuid) || x.Name.Equals(v.name) || !string.IsNullOrEmpty(v.IPs) && !string.IsNullOrEmpty(x.IP) && IPStostringIPs(v.IPs).Contains(x.IP)))
                        {
                            for (int i = 0; i < 22; i++)
                            {
                                switch (x.TPlayer.buffType[i])
                                {
                                    case 149:
                                    case 156:
                                    case 47:
                                    case 23:
                                    case 31:
                                    case 80:
                                    case 88:
                                    case 120:
                                    case 145:
                                    case 163:
                                    case 199:
                                    case 160:
                                    case 197:
                                        break;
                                    default:
                                        x.TPlayer.buffType[i] = 0;
                                        break;
                                }
                            }
                            x.SendData(PacketTypes.PlayerBuff, "", x.Index, 0f, 0f, 0f, 0);
                            x.SetBuff(149, 720);//网住
                            x.SetBuff(156, 720);//石化
                            x.SetBuff(47, 300); //冰冻
                            x.SetBuff(23, 300); //诅咒
                            x.SetBuff(31, 300); //困惑
                            x.SetBuff(80, 300); //灯火管制
                            x.SetBuff(88, 300); //混沌
                            x.SetBuff(120, 300);//臭气
                            x.SetBuff(145, 300);//月食
                            x.SetBuff(163, 300);//阻塞
                            x.SetBuff(199, 300);//创意震撼
                            x.SetBuff(160, 300);//眩晕
                            x.SetBuff(197, 300);//粘液
                            if (Timer % 240L == 0)
                            {
                                x.SendInfoMessage("You have been frozen, please ask an administrator for details");
                                SendText(x, "you are frozen", Color.Red, x.TPlayer.Center);
                            }
                            x.Teleport(v.pos.X, v.pos.Y);
                            if (Timer > v.clock + 60)
                            {
                                bool flag = false;
                                foreach (var v in x.TPlayer.buffType)
                                {
                                    if (v == 149)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    NetMessage.SendPlayerDeath(x.Index, PlayerDeathReason.ByCustomReason(""), int.MaxValue, new Random().Next(-1, 1), false, -1, -1);
                                    if (Timer % 240L == 0)
                                        x.SendInfoMessage("don't be smart");
                                }
                            }
                        }
                    });
                }
            }
        }


        /// <summary>
        /// 对进入服务器的玩家进行一些限制
        /// </summary>
        /// <param name="args"></param>
        private void OnServerJoin(JoinEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
            {
                return;
            }
            TSPlayer tsplayer = TShock.Players[args.Who];
            if (int.TryParse(tsplayer.Name, out int num) || double.TryParse(tsplayer.Name, out double num2))
            {
                tsplayer.Kick("Characters cannot only have numbers in name", true);
            }
            else if ((tsplayer.Name[0] >= ' ' && tsplayer.Name[0] <= '/') || (tsplayer.Name[0] >= ':' && tsplayer.Name[0] <= '@') || (tsplayer.Name[0] > '[' && tsplayer.Name[0] <= '`') || (tsplayer.Name[0] >= '{' && tsplayer.Name[0] <= '~'))
            {
                tsplayer.Kick("Characters cannot have special symbols in name", true);
            }
            else if (tsplayer.Name.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("Your name contains command keywords: all ", true);
            }
            else if (tsplayer.Name.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("Your name contains command keywords: time ", true);
            }
            else if (tsplayer.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("Your name contains command keywords: help ", true);
            }
            else if (tsplayer.Name.Equals("me", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("Your name contains command keywords: me ", true);
            }
        }


        /// <summary>
        /// 对离开服务区的玩家的额外数据库，进行保存
        /// </summary>
        /// <param name="args"></param>
        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
            {
                return;
            }
            //清理掉这个离开服务器的玩家的额外数据内存
            foreach (var v in edPlayers)
            {
                if (v.Name == TShock.Players[args.Who].Name)
                {
                    ZPExtraDB.WriteExtraDB(v);
                    edPlayers.RemoveAll(x => x.Account == v.Account || x.Name == v.Name);
                    break;
                }
            }
            //顺便遍历下整个edplayers，移除所有和tsplayers不同步的元素，免得越堆越多
            for (int i = 0; i < edPlayers.Count; i++)
            {
                bool flag = false;
                foreach (TSPlayer p in TShock.Players)
                {
                    if (p != null && p.IsLoggedIn && (p.Name == edPlayers[i].Name || p.Account.ID == edPlayers[i].Account))
                    {
                        flag = true; break;
                    }
                }
                if (!flag)
                {
                    ZPExtraDB.WriteExtraDB(edPlayers[i]);
                    edPlayers.RemoveAt(i);
                    i--;
                }
            }
        }


        /// <summary>
        /// 对提示隐藏的指令
        /// </summary>
        /// <param name="args"></param>
        private void HideTips(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zhide kill to hide/show the kill +1 display, use enable display again\nType /zhide point to hide/show the +1 $ display, use enable display again");
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendInfoMessage("Incorrect object, please check your status, are you an in-game player?");
                return;
            }

            if (config.WhetherToEnableKillNPCStatistics && args.Parameters[0].Equals("kill", StringComparison.OrdinalIgnoreCase))
            {
                edPlayers.ForEach(x =>
                {
                    if (x.Name == args.Player.Name)
                    {
                        x.hideKillTips = !x.hideKillTips;
                        args.Player.SendMessage($"{(x.hideKillTips ? "Hiding" : "Showing")} kill counter", new Color(0, 255, 0));
                    }
                });
            }
            else if (config.WhetherToEnablePointStatistics && args.Parameters[0].Equals("point", StringComparison.OrdinalIgnoreCase))
            {
                edPlayers.ForEach(x =>
                {
                    if (x.Name == args.Player.Name)
                    {
                        x.hidePointTips = !x.hidePointTips;
                        args.Player.SendMessage($"{(x.hidePointTips ? "Hiding" : "Showing")} point counter", new Color(0, 255, 0));
                    }
                });
            }
            else if (!config.WhetherToEnableKillNPCStatistics && args.Parameters[0].Equals("kill", StringComparison.OrdinalIgnoreCase))
                args.Player.SendInfoMessage("Killed NPC stats are not enabled, this function is not available");
            else if (!config.WhetherToEnablePointStatistics && args.Parameters[0].Equals("point", StringComparison.OrdinalIgnoreCase))
                args.Player.SendInfoMessage("Point statis are not enabled, this function is not available");
            else
                args.Player.SendInfoMessage("Type /zhide kill to cancel kill +1 display, use enable display again\nType /zhide point to cancel +1 $ display, use enable display again");
        }


        /// <summary>
        /// 导出这个玩家的人物存档
        /// </summary>
        /// <param name="args"></param>
        private void ZhiExportPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zout [name] to export that player's character saves\nType /zout all to export all character saves");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Dictionary<UserAccount, PlayerData> players = new Dictionary<UserAccount, PlayerData>();
                    using (QueryResult queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter"))
                    {
                        while (queryResult.Read())
                        {
                            int num = queryResult.Get<int>("Account");
                            UserAccount user = TShock.UserAccounts.GetUserAccountByID(num);
                            players.Add(user, TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), num));
                        }
                    }
                    args.Player.SendMessage("Estimated number of exported user archives: " + players.Count, new Color(100, 233, 255));
                    TShock.Log.Info("Estimated number of exported user archives: " + players.Count.ToString());
                    StringBuilder sb = new StringBuilder();
                    int failedcount = 0;
                    foreach (var one in players)
                    {
                        Player? player = CreateAPlayer(one.Key.Name, one.Value);
                        if (ExportPlayer(player, ZPExtraDB.getPlayerExtraDBTime(one.Key.ID)))
                        {
                            if (args.Player.IsLoggedIn)
                            {
                                args.Player.SendMessage($"User [{player!.name}] exported in directory: tshock / Zhipm / {Main.worldName} / {player!.name}.plr", new Color(0, 255, 0));
                            }
                            else
                            {
                                sb.AppendLine($"User [{player!.name}] exported in directory: tshock / Zhipm / {Main.worldName} / {player!.name}.plr");
                            }
                            TShock.Log.Info($"User [{player!.name}] exported in directory: tshock / Zhipm / {Main.worldName} / {player!.name}.plr");
                        }
                        else
                        {
                            if (args.Player.IsLoggedIn)
                            {
                                args.Player.SendInfoMessage("Failed to export user [" + one.Key + "] due to missing data");
                            }
                            else
                            {
                                sb.AppendLine($"Failed to export user [{one.Key.Name}] due to missing data");
                            }
                            failedcount++;
                            TShock.Log.Info($"Failed to export user [{one.Key.Name}] due to missing data");
                        }
                    }
                    sb.AppendLine($"Export failed for {failedcount} users due to missing data");
                    if (!args.Player.IsLoggedIn)
                    {
                        args.Player.SendInfoMessage(sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("Error in ZhiExportPlayer : " + ex.ToString());
                    args.Player.SendErrorMessage("Error in ZhiExportPlayer : " + ex.ToString());
                    Console.WriteLine("Error in ZhiExportPlayer : " + ex.ToString());
                }
                return;
            }

            List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (list.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                if (users.Count == 1 || users.Count > 1 && users.Exists(x => x.Name == args.Parameters[0]))
                {
                    if (users.Count > 1)
                    {
                        users[0] = users.Find(x => x.Name == args.Parameters[0]);
                    }
                    PlayerData playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), users[0].ID);
                    Player? player = CreateAPlayer(args.Parameters[0], playerData);
                    if (ExportPlayer(player, ZPExtraDB.getPlayerExtraDBTime(users[0].ID)))
                    {
                        args.Player.SendMessage($"Export succeeded! Directory: tshock / Zhipm / {Main.worldName} / {args.Parameters[0]}.plr", new Color(0, 255, 0));
                        TShock.Log.Info($"Export succeeded! Directory: tshock / Zhipm / {Main.worldName} / {args.Parameters[0]}.plr");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Export failed due to missing data");
                        TShock.Log.Info("Export failed due to missing data");
                    }
                }
                else if (users.Count == 0)
                {
                    args.Player.SendInfoMessage(noplayer);
                    return;
                }
                else if (users.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
            }
            else if (list.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else if (ExportPlayer(list[0].TPlayer, ZPExtraDB.getPlayerExtraDBTime(list[0].Account.ID)))
            {
                args.Player.SendMessage($"Export succeeded! Directory: tshock / Zhipm / {Main.worldName} / {list[0].Name}.plr", new Color(0, 255, 0));
                TShock.Log.Info($"Export succeeded! Directory: tshock / Zhipm / {Main.worldName} / {list[0].Name}.plr");
            }
            else
            {
                args.Player.SendErrorMessage("Export failed due to missing data");
                TShock.Log.Info("Export failed due to missing data");
            }
        }


        /// <summary>
        /// 对玩家在线时常进行排序
        /// </summary>
        /// <param name="args"></param>
        private void ZhiSortPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 1 && args.Parameters.Count != 2)
            {
                args.Player.SendInfoMessage("Type /zsort help to see help for the sort series commands");
                return;
            }
            //帮助指令
            if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                string temp1 = config.WhetherToEnableOnlineTimeStatistics ? ("Type /zsort time to see the top ten characters online time leaderboard\n" +
                                                            "Type /zsort time [num] to view the current online time leaderboard of [num] characters\n" +
                                                            "Type /zsort time all to view the online time leaderboard of all players\n") : "";
                string temp2 = config.WhetherToEnableKillNPCStatistics ? ("\nType /zsort kill [num] to view the current leaderboard of the number of creatures killed by [num] characters\n" +
                                                           "Type /zsort kill to see the top 10 list of creatures killed by characters\n" +
                                                           "Type /zsort kill all to see a leaderboard of all player kills\n" +
                                                           "Type /zsort boss [num] to view the leaderboard of the total number of bosses killed by [num] characters\n" +
                                                           "Type /zsort boss to view the top ten leaderboards of the total number of characters killed by Boss\n" +
                                                           "Type /zsort boss all to view the leaderboard of the total number of bosses killed by all players\n" +
                                                           "Type /zsort rarenpc [num] to view the total number of rare creatures killed by [num] characters\n" +
                                                           "Type /zsort rarenpc to view the top ten total number of rare creatures killed by characters\n" +
                                                           "Type /zsort rarenpc all to see the total number of rare creatures killed by all players") : "";
                string temp3 = config.WhetherToEnablePointStatistics ? ("\nType /zsort point [num] to view the current [num] character point leaderboard\n" +
                                                        "Type /zsort point to see the top ten character points leaderboard\n" +
                                                        "Type /zsort point all to see all player point leaderboards") : "";
                string temp4 = config.WhetherToEnableDeathStatistics ? ("\nType /zsort death [num] to view the current [num] death count list\n" +
                                                        "Type /zsort death to see the top ten character deaths\n" +
                                                        "Type /zsort death all to see a leaderboard of all player deaths") : "";
                string temp5 = config.WhetherToEnableDeathStatistics && config.WhetherToEnableOnlineTimeStatistics ?
                                                       ("\nType /zsort clumsy to see the top 10 clumsy characters\n" +
                                                        "Type /zsort clumsy [num] to view the current list of [num] handicapped characters\n" +
                                                        "Type /zsort clumsy all to view all clumsy rankings") : "";

                args.Player.SendMessage(
                    temp1 +
                    "Type /zsort coin to see the top 10 characters by coin count\n" +
                    "Type /zsort coin [num] to view the current [num] character coin count leaderboard\n" +
                    "Type /zsort coin all to see a leaderboard of all player coin counts\n" +
                    "Type /zsort fish to see the top 10 list of character quest fish\n" +
                    "Type /zsort fish [num] to view the current [num] character task fish list\n" +
                    "Type /zsort fish all to see a leaderboard of all player quest fish counts" +
                    temp4 + temp2 + temp3 + temp5
                    , TextColor());
                return;
            }
            //时间排序
            else if (config.WhetherToEnableOnlineTimeStatistics && args.Parameters[0].Equals("time", StringComparison.OrdinalIgnoreCase))
            {
                // time 排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB(ExtraDataDate.time, false);
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"【{list[i].Name}】 playtime {timetostring(list[i].time)}");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most" + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 playtime {timetostring(list[i].time)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 playtime {timetostring(list[i].time)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort time [num] to view the current online time leaderboard of [num] characters\nType /zsort time to view the top ten online time leaderboards\nType /zsort time all to view the online time rankings of all players");
                    }
                }
            }
            //钱币排序
            else if (args.Parameters[0].Equals("coin", StringComparison.OrdinalIgnoreCase))
            {
                List<UserAccount> list = new List<UserAccount>();
                using (QueryResult queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter"))
                {
                    while (queryResult.Read())
                    {
                        int num = queryResult.Get<int>("Account");
                        list.Add(TShock.UserAccounts.GetUserAccountByID(num));
                    }
                }

                list.Sort((p1, p2) => getPlayerCoin(p2.Name).CompareTo(getPlayerCoin(p1.Name)));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        if (args.Player.IsLoggedIn)
                            sb.AppendLine($"【{list[i].Name}】 total coins: {cointostring(getPlayerCoin(list[i].Name))}");
                        else
                            sb.AppendLine($"【{list[i].Name}】 total coins: {cointostring(getPlayerCoin(list[i].Name), 1)}");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            if (args.Player.IsLoggedIn)
                                sb.AppendLine($"【{list[i].Name}】 total coins: {cointostring(getPlayerCoin(list[i].Name))}");
                            else
                                sb.AppendLine($"【{list[i].Name}】 total coins: {cointostring(getPlayerCoin(list[i].Name), 1)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (args.Player.IsLoggedIn)
                                sb.AppendLine($"【{list[i].Name}】 total coins: {cointostring(getPlayerCoin(list[i].Name))}");
                            else
                                sb.AppendLine($"【{list[i].Name}】 total coins: {cointostring(getPlayerCoin(list[i].Name), 1)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort coin to see top 10 coin count leaderboards\nType /zsort coin [num] to see coin count leaderboards for current [num] characters\nType /zsort coin all to see coin count leaderboards for all players");
                    }
                }
            }
            //钓鱼任务排序
            else if (args.Parameters[0].Equals("fish", StringComparison.OrdinalIgnoreCase))
            {
                List<UserAccount> list = new List<UserAccount>();
                using (QueryResult queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter ORDER BY questsCompleted DESC"))
                {
                    while (queryResult.Read())
                    {
                        int num = queryResult.Get<int>("Account");
                        list.Add(TShock.UserAccounts.GetUserAccountByID(num));
                    }
                }
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"【{list[i].Name}】completed {TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), list[i].ID).questsCompleted} angler quests");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 completed {TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), list[i].ID).questsCompleted} angler quests");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 completed {TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), list[i].ID).questsCompleted} angler quests");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort fish to view the top ten character task fish list\nType /zsort fish [num] to view the current [num] personal task fish list\nType /zsort fish all to view all player task fish Quantity leaderboard");
                    }
                }
            }
            //斩杀数排序
            else if (config.WhetherToEnableKillNPCStatistics && args.Parameters[0].Equals("kill", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB(ExtraDataDate.killNPCnum, false);
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"【{list[i].Name}】 killed {list[i].killNPCnum} mobs");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 killed {list[i].killNPCnum} mobs");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 killed {list[i].killNPCnum} mobs");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort kill [num] to view the current leaderboard of [num] the number of creatures killed by a character\nType /zsort kill to view the top ten list of the number of creatures killed by a character\nType /zsort kill all to view all players Killed Creatures Leaderboard");
                    }
                }
            }
            //斩杀Boss排序
            else if (config.WhetherToEnableKillNPCStatistics && args.Parameters[0].Equals("boss", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();
                list.Sort((p1, p2) => getKillNumFromDictionary(p2.killBossID).CompareTo(getKillNumFromDictionary(p1.killBossID)));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"【{list[i].Name}】 killed {getKillNumFromDictionary(list[i].killBossID)} bosses");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 killed {getKillNumFromDictionary(list[i].killBossID)} bosses");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 killed {getKillNumFromDictionary(list[i].killBossID)} bosses");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort boss [num] to view the leaderboard of the total number of bosses killed by [num] characters\nType /zsort boss to view the top ten characters killed by the boss\nType /zsort boss all to view the total number of bosses killed by all players leaderboard");
                    }
                }
            }
            //斩杀罕见生物排序
            else if (config.WhetherToEnableKillNPCStatistics && args.Parameters[0].Equals("rarenpc", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();
                list.Sort((p1, p2) => getKillNumFromDictionary(p2.killRareNPCID).CompareTo(getKillNumFromDictionary(p1.killRareNPCID)));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"【{list[i].Name}】 killed {getKillNumFromDictionary(list[i].killRareNPCID)} rare mobs");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 killed {getKillNumFromDictionary(list[i].killRareNPCID)} rare mobs");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 killed {getKillNumFromDictionary(list[i].killRareNPCID)} rare mobs");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort rarenpc [num] to view the current list of the total number of rare creatures killed by [num] characters\nType /zsort rarenpc to view the top ten of the total number of rare creatures killed by characters\nType /zsort rarenpc all to view Leaderboard of the total number of rare creatures killed by all players");
                    }
                }
            }
            //点数排行
            else if (config.WhetherToEnablePointStatistics && args.Parameters[0].Equals("point", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB(ExtraDataDate.point, false);
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"Name {i + 1}:【{list[i].Name}】 points {list[i].point} ");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("Invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"Name {i + 1}:【{list[i].Name}】 Points {list[i].point}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"Name {i + 1}:【{list[i].Name}】 Points {list[i].point}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort point [num] to view the current [num] character point leaderboard\nType /zsort point to see the top ten character point leaderboards\ntype /zsort point all to see all player point leaderboards");
                    }
                }
            }
            //死亡次数排行
            else if (config.WhetherToEnableDeathStatistics && args.Parameters[0].Equals("death", StringComparison.OrdinalIgnoreCase))
            {//排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB(ExtraDataDate.deathCount, false);
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount} ");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("Invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort death [num] to view the current [num] death count leaderboard\nType /zsort death to see the top ten character death counts\nType /zsort death all to see the death count rankings for all players");
                    }
                }
            }
            //菜鸡榜
            else if (config.WhetherToEnableDeathStatistics && config.WhetherToEnableOnlineTimeStatistics && args.Parameters[0].Equals("clumsy", StringComparison.OrdinalIgnoreCase))
            {//排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();

                list.Sort((p1, p2) =>
                {
                    double k1 = 0.0, k2 = 0.0;
                    if (p1.time > 0L)
                    {
                        k1 = p1.deathCount * 1000.0 / p1.time;
                    }
                    if (p2.time > 0L)
                    {
                        k2 = p2.deathCount * 1000.0 / p2.time;
                    }
                    return k2.CompareTo(k1);
                });
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        if (args.Player.IsLoggedIn)
                            sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount * 1000.0 / list[i].time:0.00}");
                        else
                            sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount * 1000.0 / list[i].time:0.00}");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("invalid number");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("Current most " + list.Count + " people");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            if (args.Player.IsLoggedIn)
                                sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount * 1000.0 / list[i].time:0.00}");
                            else
                                sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount * 1000.0 / list[i].time:0.00}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (args.Player.IsLoggedIn)
                                sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount * 1000.0 / list[i].time:0.00}");
                            else
                                sb.AppendLine($"【{list[i].Name}】 deaths: {list[i].deathCount * 1000.0 / list[i].time:0.00}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("Type /zsort clumsy to view the top ten handicapped characters\nType /zsort clumsy [num] to view the current [num] handicapped rankings\nType /zsort clumsy all to view the list of all handicapped players");
                    }
                }
            }

            else
            {
                args.Player.SendInfoMessage("Type /zsort help to see help for the sort series commands");
            }
        }


        /// <summary>
        /// 办掉离线或在线的玩家，超级ban指令
        /// </summary>
        /// <param name="args"></param>
        private void SuperBan(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendInfoMessage("Type /zban add [name] [reason] to ban players whether they are online or not, reason can be left blank");
                return;
            }
            if (args.Parameters[0].Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[1]);
                //封禁原因，可不填
                string reason;
                if (args.Parameters.Count == 3)
                    reason = args.Parameters[2];
                else
                    reason = "Reflect on what you have done!";

                if (list.Count == 1)
                {
                    if (list[0].Ban(reason, "ZHIPlayerManager by " + args.Player.Name))
                    {
                        args.Player.SendMessage($"User {list[0].Name} has been banned by {args.Player.Name}", broadcastColor);
                        TShock.Log.Info($"User {list[0].Name} has been banned by  {args.Player.Name}");
                    }
                    else
                    {   //实际上这个情况永远不会发生，因为ban方法的返回值就没返回false过
                        args.Player.SendInfoMessage($"The ban of user {list[0].Name} failed, maybe the player has already been banned or the group he belongs to is prohibited from banning");
                        TShock.Log.Info($" The ban of user {list[0].Name} failed, maybe the player has already been banned or the group he belongs to is prohibited from banning");
                    }
                }
                else if (list.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                }
                //离线查找
                else
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount? user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                    if (user == null)
                    {
                        args.Player.SendInfoMessage("Exact search not found, trying fuzzy search");
                        List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[1], true);
                        if (users.Count == 1)
                        {
                            user = users[0];
                        }
                        else if (users.Count > 1)
                        {
                            args.Player.SendInfoMessage("Found more than one player. Make sure you type the full name, and use quotations if the name has space in it.");
                            return;
                        }
                        else
                        {
                            args.Player.SendInfoMessage(noplayer + "Use quotations if the name has space in it.");
                            return;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(user.Name))
                        TShock.Bans.InsertBan("acc:" + user.Name, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                    if (!string.IsNullOrWhiteSpace(user.UUID))
                        TShock.Bans.InsertBan("uuid:" + user.UUID, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                    if (!string.IsNullOrWhiteSpace(user.KnownIps))
                    {
                        string[] ips = IPStostringIPs(user.KnownIps);
                        foreach (string str in ips)
                        {
                            if (!string.IsNullOrWhiteSpace(str))
                                TShock.Bans.InsertBan("ip:" + str, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                        }
                    }
                    if (!args.Player.IsLoggedIn)
                        args.Player.SendMessage($"User {user.Name} has been banned by {args.Player.Name}", broadcastColor);
                    TSPlayer.All.SendMessage($"User {user.Name} has been banned by {args.Player.Name}", broadcastColor);
                    TShock.Log.Info($"User {user.Name} has been banned by {args.Player.Name}");
                }
            }
            else
            {
                args.Player.SendInfoMessage("Type /zban add [name] [reason] to ban players whether they are online or not, reason can be left blank");
            }
        }


        /// <summary>
        /// 冻结该玩家，禁止他做出任何操作
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ZFreeze(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zfre [name] to freeze the player");
                return;
            }
            List<TSPlayer> ts = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (ts.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user != null)
                {
                    if (frePlayers.Exists(x => x.name == user.Name && x.uuid == user.UUID && (x.IPs == null ? true : x.IPs.Equals(user.KnownIps))))
                    {
                        args.Player.SendMessage($"Player [{user.Name}] has been frozen", new Color(0, 255, 0));
                    }
                    else
                    {
                        frePlayers.Add(new MessPlayer(user.ID, user.Name, user.UUID, user.KnownIps, Vector2.Zero));
                        args.Player.SendMessage($"Player [{user.Name}] frozen successfully", new Color(0, 255, 0));
                    }
                }
                else
                {
                    args.Player.SendInfoMessage("Exact search not found, trying fuzzy search");
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (users.Count == 1)
                    {
                        if (frePlayers.Exists(x => x.name == users[0].Name && x.uuid == users[0].UUID && (x.IPs == null ? true : x.IPs.Equals(users[0].KnownIps))))
                        {
                            args.Player.SendMessage($"Player [{users[0].Name}] has been frozen!", new Color(0, 255, 0));
                        }
                        else
                        {
                            frePlayers.Add(new MessPlayer(users[0].ID, users[0].Name, users[0].UUID, users[0].KnownIps, Vector2.Zero));
                            args.Player.SendMessage($"Player [{users[0].Name}] frozen successfully!", new Color(0, 255, 0));
                        }
                    }
                    else if (users.Count > 1)
                    {
                        args.Player.SendInfoMessage(manyplayer);
                        return;
                    }
                    else
                    {
                        args.Player.SendInfoMessage(noplayer);
                        return;
                    }
                }
            }
            else if (ts.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else
            {
                if (frePlayers.Exists(x => x.name == ts[0].Name && x.uuid == ts[0].UUID && (x.IPs == null ? true : x.IPs.Equals(ts[0].Account.KnownIps))))
                {
                    args.Player.SendMessage($"Player [{ts[0].Name}] has been frozen!", new Color(0, 255, 0));
                }
                else
                {
                    clearAllBuffFromPlayer(ts[0]);
                    frePlayers.Add(new MessPlayer(ts[0].Account.ID, ts[0].Name, ts[0].UUID, ts[0].Account.KnownIps, ts[0].TPlayer.Center));
                    args.Player.SendMessage($"Player [{ts[0].Name}] frozen successfully!", new Color(0, 255, 0));
                }
            }
        }


        /// <summary>
        /// 取消冻结该玩家
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ZUnFreeze(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("Type /zunfre [name] to unfreeze that player\nType /zunfre all to unfreeze all players");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                frePlayers.ForEach(x =>
                {
                    List<TSPlayer> ts = BestFindPlayerByNameOrIndex(x.name);
                    if (ts.Count > 0 && ts[0].Name == x.name)
                    {
                        clearAllBuffFromPlayer(ts[0]);
                    }
                });
                frePlayers.Clear();
                args.Player.SendMessage("All players have been unfrozen", new Color(0, 255, 0));
                return;
            }
            List<TSPlayer> ts = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (ts.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user != null)
                {
                    int c = frePlayers.RemoveAll(x => x.uuid == user.UUID || x.name == user.Name || !string.IsNullOrEmpty(x.IPs) && !string.IsNullOrEmpty(user.KnownIps) && IPStostringIPs(x.IPs).Any(y => IPStostringIPs(user.KnownIps).Contains(y)) || string.IsNullOrEmpty(x.IPs) && string.IsNullOrEmpty(user.KnownIps));
                    if (c > 0)
                        args.Player.SendMessage($"Player [{user.Name}] has been unfrozen", new Color(0, 255, 0));
                    else
                        args.Player.SendMessage($"Player [{user.Name}] is not frozen!", new Color(0, 255, 0));
                }
                else
                {
                    args.Player.SendInfoMessage("Exact search not found, trying fuzzy search");
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (users.Count > 1)
                    {
                        args.Player.SendInfoMessage(manyplayer);
                        return;
                    }
                    else if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                        return;
                    }
                    else
                    {
                        int c = frePlayers.RemoveAll(x => x.uuid == users[0].UUID || x.name == users[0].Name || !string.IsNullOrEmpty(x.IPs) && !string.IsNullOrEmpty(users[0].KnownIps) && IPStostringIPs(x.IPs).Any(y => IPStostringIPs(users[0].KnownIps).Contains(y)) || string.IsNullOrEmpty(x.IPs) && string.IsNullOrEmpty(users[0].KnownIps));
                        if (c > 0)
                            args.Player.SendMessage($"Player [{users[0].Name}] has been unfrozen", new Color(0, 255, 0));
                        else
                            args.Player.SendMessage($"Player [{users[0].Name}] is not fozen", new Color(0, 255, 0));
                    }
                }
            }
            else if (ts.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else
            {
                int c = frePlayers.RemoveAll(x => x.uuid == ts[0].UUID || x.name == ts[0].Name || !string.IsNullOrEmpty(x.IPs) && !string.IsNullOrEmpty(ts[0].IP) && IPStostringIPs(x.IPs).Any(x => ts[0].IP == x));
                if (c > 0)
                {
                    clearAllBuffFromPlayer(ts[0]);
                    args.Player.SendMessage($"Player [{ts[0].Name}] has been unfozen", new Color(0, 255, 0));
                    ts[0].SendMessage("You have been unfrozen", new Color(0, 255, 0));
                }
                else
                {
                    args.Player.SendMessage($"Player [{ts[0].Name}] is not frozen", new Color(0, 255, 0));
                }
            }
        }


        /// <summary>
        /// 击中npc时进行标记
        /// </summary>
        /// <param name="args"></param>
        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            //如果 击中的玩家是空的，或npc是傀儡，或npc是飞弹，或npc是城镇npc，或者他是雕像怪，结束
            if (!config.WhetherToEnableKillNPCStatistics || args.Player == null || args.Npc.netID == 488 || args.Npc.lifeMax == 1 || args.Npc.townNPC || args.Npc.SpawnedFromStatue)
            {
                return;
            }

            List<TSPlayer> players = BestFindPlayerByNameOrIndex(args.Player.name);
            if (players.Count == 0)
                return;
            //这个生物是否以前被击中过
            StrikeNPC? strike = strikeNPC.Find(x => x.index == args.Npc.whoAmI && x.id == args.Npc.netID);
            if (strike != null)
            {   //如果击中过，寻找击中他的玩家是否被记录
                if (strike.playerAndDamage.ContainsKey(players[0].Account.ID))
                {//已被记录，那么伤害记录加数值
                    if (args.Damage > 3000)//不正常的伤害
                    {
                        strike.playerAndDamage[players[0].Account.ID] += 2500;
                        strike.AllDamage += 2500;
                    }
                    else
                    {
                        strike.playerAndDamage[players[0].Account.ID] += args.Damage;
                        strike.AllDamage += args.Damage;
                    }
                }
                else//否则，创建新的 player->damage
                {
                    strike.playerAndDamage.Add(players[0].Account.ID, args.Damage);
                    strike.AllDamage += args.Damage;
                }
            }
            else//如果没有击中过，创建新的 npc
            {
                StrikeNPC snpc = new StrikeNPC();
                snpc.id = args.Npc.netID;
                snpc.index = args.Npc.whoAmI;
                snpc.name = args.Npc.FullName;

                if (config.WhetherToEnablePointStatistics)
                {
                    //处理特殊生物的价值
                    switch (snpc.id)
                    {
                        case 667://金色史莱姆-250
                            snpc.value = args.Npc.value / 15; break;
                        case 125://双子
                        case 126://双子
                        case 196://宁芙-250
                            snpc.value = args.Npc.value / 2; break;
                        case 87://飞龙-50
                        case 88:
                        case 89:
                        case 90:
                        case 91:
                        case 92:
                        case -4://粉史莱姆-50
                        case 85://宝箱怪-500
                        case 629://冰雪宝箱怪-500
                        case 216://海盗船长-100
                            snpc.value = args.Npc.value / 5; break;
                        case 662://海盗诅咒-400
                            snpc.value = 40000f; break;
                        case 393://火星飞碟四个肢体-300
                        case 394:
                            snpc.value = 30000f; break;
                        case 395://火星飞蝶本体-2500
                            snpc.value = 250000f; break;
                        case 618://恐惧鹦鹉螺 1000
                            snpc.value = 100000f; break;
                        case 346://圣诞坦克-500
                        case 621://血鳗鱼-374
                        case 622://血鳗鱼
                        case 623://血鳗鱼
                        case 580://蚁狮马
                        case 581://蚁狮蜂
                        case 508://巨型蚁狮马
                        case 509://巨型蚁狮蜂
                        case 60://地狱蝙蝠
                        case 59://熔岩史莱姆
                        case 62://恶魔
                        case 66://巫毒恶魔
                            snpc.value = args.Npc.value * 2; break;
                        case 564://黑暗魔法师-200
                            snpc.value = 20000f; break;
                        case 565://T3黑暗魔法师-500
                            snpc.value = 50000f; break;
                        case 576://T2食人魔-800
                            snpc.value = 80000f; break;
                        case 577://T3食人魔-1500
                            snpc.value = 150000f; break;
                        case 551://双足翼龙-6250
                            snpc.value = 625000f; break;
                        case 657://史莱姆皇后-2000
                            snpc.value = 200000f; break;
                        case 294://肉前地牢怪 + 3
                        case 295:
                        case 296:
                        case -14:
                        case -13:
                        case 31:
                        case 32:
                        case 34:
                        case 71:
                            snpc.value = args.Npc.value + 300; break;
                        default:
                            snpc.value = args.Npc.value; break;
                    }

                    if (!Main.hardMode)
                    {
                        switch (snpc.id)
                        {//肉前真菌敌怪价格降低 2/3
                            case 254:
                            case 255:
                            case 257:
                            case 258:
                            case 259:
                            case 260:
                            case 261:
                            case 634:
                            case 635:
                                snpc.value /= 3; break;
                            default: break;
                        }
                    }

                    //天顶世界对宝箱怪的价值进行限定
                    if ((Main.remixWorld || Main.zenithWorld) && (args.Npc.netID == 85 || args.Npc.netID == 629))
                    {
                        snpc.value = args.Npc.value / 17;
                    }
                }
                else
                    snpc.value = 0f;

                //处理特殊生物是否为boss
                switch (snpc.id)
                {
                    case 13://世界吞噬者
                    case 14:
                    case 15:
                    case 325://哀木
                    case 327://南瓜王
                    case 564://T1黑暗魔法师
                    case 565://T3黑暗魔法师
                    case 576://T2食人魔
                    case 577://T3食人魔
                    case 551://双足翼龙
                             //case 491://荷兰飞船 492大炮
                    case 344://常绿尖叫怪
                    case 345://冰雪女皇
                    case 346://圣诞坦克
                    case 517://日耀柱
                    case 422://星璇柱
                    case 493://星尘柱
                    case 507://星云柱
                        snpc.isBoss = true; break;
                    default:
                        snpc.isBoss = args.Npc.boss; break;
                }

                snpc.playerAndDamage.Add(players[0].Account.ID, args.Damage);
                snpc.AllDamage += args.Damage;
                strikeNPC.Add(snpc);
            }
        }


        /// <summary>
        /// 杀死npc时计数
        /// </summary>
        /// <param name="args"></param>
        private void OnNPCKilled(NpcKilledEventArgs args)
        {
            if (!config.WhetherToEnableKillNPCStatistics)
            {
                if (strikeNPC.Count > 0)
                    strikeNPC.Clear();
                return;
            }

            //毁灭者的处理，这个npc的死亡钩子只记录他的头，所以这里没有写135和136，并且没有放在 for (int i = 0; i < strikeNPC.Count; i++) 里，因为这个boss可能没有被击中过头部
            //为什么移到外面，因为用于判断毁灭者死亡条件的头部可能一直不会被击中
            if (args.npc.netID == 134)
            {//遍历所有被击中过的strikeNPC，记录 135 和 136 的击中情况
                foreach (var sss in strikeNPC)
                {
                    if (sss.id == 134 || sss.id == 135 || sss.id == 136)
                    {
                        foreach (var ss in sss.playerAndDamage)
                        {
                            if (Destroyer.ContainsKey(ss.Key))
                            {
                                Destroyer[ss.Key] += ss.Value;
                            }
                            else
                            {
                                Destroyer.Add(ss.Key, ss.Value);
                            }
                        }
                    }
                }
                int sum = 0;
                foreach (var des in Destroyer)
                {
                    sum += des.Value;
                }
                edPlayers.ForEach((Action<ExtraData>)(x =>
                {
                    if (Destroyer.TryGetValue(x.Account, out int value))
                    {
                        x.killNPCnum++;

                        int point = 0;
                        if (config.WhetherToEnablePointStatistics)
                        {
                            point = (int)(2000f * value / sum);
                            x.point += point;
                        }

                        if (x.killBossID.ContainsKey(134))
                            x.killBossID[134]++;
                        else
                            x.killBossID.Add(134, 1);
                        List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                        if (temp.Count != 0)
                        {
                            if (!x.hideKillTips)
                                SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                            NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                            if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                        }
                    }
                }));
                if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                    SendKillBossMessage(args.npc.FullName, Destroyer, sum);
                Destroyer.Clear();
                strikeNPC.RemoveAll(x => x.id == 134 || x.id == 136 || x.id == 135 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                return;
            }
            //肉山同理，肉山嘴巴
            else if (args.npc.netID == 113)
            {//遍历所有被击中过的strikeNPC，记录 113 和 114 的击中情况
                foreach (var sss in strikeNPC)
                {
                    if (sss.id == 113 || sss.id == 114)
                    {
                        foreach (var ss in sss.playerAndDamage)
                        {
                            if (FleshWall.ContainsKey(ss.Key))
                            {
                                FleshWall[ss.Key] += ss.Value;
                            }
                            else
                            {
                                FleshWall.Add(ss.Key, ss.Value);
                            }
                        }
                    }
                }
                int sum = 0;
                foreach (var fw in FleshWall)
                {
                    sum += fw.Value;
                }
                edPlayers.ForEach((Action<ExtraData>)(x =>
                {
                    if (FleshWall.TryGetValue(x.Account, out int value))
                    {
                        x.killNPCnum++;

                        int point = 0;
                        if (config.WhetherToEnablePointStatistics)
                        {
                            point = (int)(2000f * value / sum);
                            x.point += point;
                        }

                        if (x.killBossID.ContainsKey(113))
                            x.killBossID[113]++;
                        else
                            x.killBossID.Add(113, 1);
                        List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                        if (temp.Count != 0)
                        {
                            if (!x.hideKillTips)
                                SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                            NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                            if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                        }
                    }
                }));
                if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                    SendKillBossMessage("Wall of flesh", FleshWall, sum);
                FleshWall.Clear();
                strikeNPC.RemoveAll(x => x.id == 113 || x.id == 114 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                return;
            }

            //其他生物，对被击杀的生物进行计数
            for (int i = 0; i < strikeNPC.Count; i++)
            {
                if (strikeNPC[i].index == args.npc.whoAmI && strikeNPC[i].id == args.npc.netID)
                {
                    switch (strikeNPC[i].id)
                    {
                        case 13://世界吞噬者特殊处理，特殊点：可以有多个头，只有最后一个头死亡时计入击杀
                        case 14:
                        case 15:
                            {
                                bool flag = true;
                                foreach (var n in Main.npc)
                                {
                                    if (n.whoAmI != args.npc.whoAmI && (n.type == 13 || n.type == 14 || n.type == 15) && n.active)
                                    {
                                        flag = false; break;
                                    }
                                }
                                if (flag)
                                {
                                    foreach (var sss in strikeNPC)
                                    {
                                        foreach (var ss in sss.playerAndDamage)
                                        {
                                            if (Eaterworld.ContainsKey(ss.Key))
                                            {
                                                Eaterworld[ss.Key] += ss.Value;
                                            }
                                            else
                                            {
                                                Eaterworld.Add(ss.Key, ss.Value);
                                            }
                                        }
                                    }
                                    int sum = 0;
                                    foreach (var eater in Eaterworld)
                                    {
                                        sum += eater.Value;
                                    }
                                    edPlayers.ForEach((Action<ExtraData>)(x =>
                                    {
                                        if (Eaterworld.TryGetValue(x.Account, out int value))
                                        {
                                            x.killNPCnum++;

                                            int point = 0;
                                            if (config.WhetherToEnablePointStatistics)
                                            {
                                                point = (int)(1250f * value / sum);
                                                x.point += point;
                                            }

                                            if (x.killBossID.ContainsKey(13))
                                                x.killBossID[13]++;
                                            else
                                                x.killBossID.Add(13, 1);
                                            List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                            if (temp.Count != 0)
                                            {
                                                if (!x.hideKillTips)
                                                    SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                                if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                                    SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                                            }
                                        }
                                    }));
                                    if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                                        SendKillBossMessage(args.npc.FullName, Eaterworld, sum);
                                    strikeNPC.RemoveAll(x => x.id == 13 || x.id == 14 || x.id == 15 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                    Eaterworld.Clear();
                                    return;
                                }
                            }
                            break;
                        case 492://荷兰飞船的处理，特殊点：本体不可被击中，在其他炮塔全死亡后计入击杀
                            {
                                bool flag = true;
                                int index = -1;
                                foreach (var n in Main.npc)
                                {
                                    if (n.whoAmI != args.npc.whoAmI && n.type == 492 && n.active)
                                    {
                                        flag = false;
                                    }
                                    if (n.netID == 491)
                                    {
                                        index = n.whoAmI;
                                    }
                                }
                                if (index >= 0)
                                {
                                    StrikeNPC? st = strikeNPC.Find(x => x.id == 491);
                                    if (st == null)
                                    {
                                        strikeNPC.Add(new StrikeNPC(index, 491, Main.npc[index].FullName, true, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage, 80000f));
                                    }
                                    else
                                    {
                                        strikeNPC[i].playerAndDamage.ForEach(y =>
                                        {
                                            if (st.playerAndDamage.ContainsKey(y.Key))
                                            {
                                                st.playerAndDamage[y.Key] += y.Value;
                                                st.AllDamage += y.Value;
                                            }
                                            else
                                            {
                                                st.playerAndDamage.Add(y.Key, y.Value);
                                                st.AllDamage += y.Value;
                                            }
                                        });
                                    }
                                }
                                if (flag)
                                {
                                    StrikeNPC? airship = strikeNPC.Find(x => x.id == 491);
                                    if (airship == null)
                                    {
                                        strikeNPC.RemoveAll(x => x.id == 491 || x.id == 492 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                        return;
                                    }
                                    edPlayers.ForEach((Action<ExtraData>)(x =>
                                    {
                                        if (airship.playerAndDamage.TryGetValue(x.Account, out int value))
                                        {
                                            x.killNPCnum += 2;

                                            int point = 0;
                                            if (config.WhetherToEnablePointStatistics)
                                            {
                                                point = (int)(airship.value * value / airship.AllDamage / 100);
                                                x.point += point;
                                            }

                                            if (x.killBossID.ContainsKey(491))
                                                x.killBossID[491]++;
                                            else
                                                x.killBossID.Add(491, 1);
                                            List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                            if (temp.Count != 0)
                                            {
                                                if (!x.hideKillTips)
                                                    SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                                if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                                    SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                                            }
                                        }
                                    }));
                                    if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                                        SendKillBossMessage(args.npc.FullName, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage);
                                    strikeNPC.RemoveAll(x => x.id == 491 || x.id == 492 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                    return;
                                }
                            }
                            break;
                        case 398://月球领主的处理，特殊点，本体可被击中，但肢体会假死，击中肢体也应该算入本体中
                            {
                                List<StrikeNPC> strikenpcs = strikeNPC.FindAll(x => x.id == 397 || x.id == 396);
                                if (strikenpcs.Count > 0)
                                {
                                    foreach (var v in strikenpcs)
                                    {
                                        foreach (var vv in v.playerAndDamage)
                                        {
                                            if (strikeNPC[i].playerAndDamage.ContainsKey(vv.Key))
                                            {
                                                strikeNPC[i].playerAndDamage[vv.Key] += vv.Value;
                                                strikeNPC[i].AllDamage += vv.Value;
                                            }
                                            else
                                            {
                                                strikeNPC[i].playerAndDamage.Add(vv.Key, vv.Value);
                                                strikeNPC[i].AllDamage += vv.Value;
                                            }
                                        }
                                    }
                                }
                                edPlayers.ForEach((Action<ExtraData>)(x =>
                                {
                                    if (strikeNPC[i].playerAndDamage.TryGetValue(x.Account, out int value))
                                    {
                                        x.killNPCnum++;

                                        int point = 0;
                                        if (config.WhetherToEnablePointStatistics)
                                        {
                                            point = (int)(value * 1f / strikeNPC[i].AllDamage * strikeNPC[i].value / 100);
                                            x.point += point;
                                        }

                                        if (x.killBossID.ContainsKey(398))
                                            x.killBossID[398]++;
                                        else
                                            x.killBossID.Add(398, 1);
                                        List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                        if (temp.Count != 0)
                                        {
                                            if (!x.hideKillTips)
                                                SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                                            NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                            if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                                SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                                        }
                                    }
                                }));
                                if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                                    SendKillBossMessage("Moon Lord", strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage);
                                strikeNPC.RemoveAll(x => x.id == 398 || x.id == 397 || x.id == 396 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                return;
                            }
                            break;
                        case 127://机械骷髅王的处理，特殊点，本体可能被击中，其他肢体可能会死
                        case 128:
                        case 129:
                        case 130:
                        case 131:
                            {
                                StrikeNPC? strike = strikeNPC.Find(x => x.id == 127);
                                if (strike == null)
                                {
                                    int index = -1;
                                    foreach (var n in Main.npc)
                                    {
                                        if (n.netID == 127)
                                        {
                                            index = n.whoAmI;
                                        }
                                    }
                                    if (index == -1)//我认为不可能发生这种情况
                                    {
                                        strikeNPC.RemoveAll(x => x.id == 127 || x.id == 128 || x.id == 129 || x.id == 130 || x.id == 131 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                        return;
                                    }
                                    strike = new StrikeNPC(index, 127, Main.npc[index].FullName, true, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage, 300000);
                                    strikeNPC.Add(strike);
                                }
                                else if (strikeNPC[i].id != 127)//把肢体受伤计算加入到本体头部中
                                {
                                    foreach (var v in strikeNPC[i].playerAndDamage)
                                    {
                                        if (strike.playerAndDamage.ContainsKey(v.Key))
                                        {
                                            strike.playerAndDamage[v.Key] += v.Value;
                                            strike.AllDamage += v.Value;
                                        }
                                        else
                                        {
                                            strike.playerAndDamage.Add(v.Key, v.Value);
                                            strike.AllDamage += v.Value;
                                        }
                                    }
                                }
                                if (strikeNPC[i].id == 127)
                                {
                                    edPlayers.ForEach((Action<ExtraData>)(x =>
                                    {
                                        if (strikeNPC[i].playerAndDamage.TryGetValue(x.Account, out int value))
                                        {
                                            x.killNPCnum += 1;

                                            int point = 0;
                                            if (config.WhetherToEnablePointStatistics)
                                            {
                                                point = (int)(value * 1.0f / strikeNPC[i].AllDamage * strikeNPC[i].value / 100);
                                                x.point += point;
                                            }

                                            if (x.killBossID.ContainsKey(127))
                                                x.killBossID[127]++;
                                            else
                                                x.killBossID.Add(127, 1);
                                            List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                            if (temp.Count != 0)
                                            {
                                                if (!x.hideKillTips)
                                                    SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                                if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                                    SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                                            }
                                        }
                                    }));
                                    if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                                        SendKillBossMessage(args.npc.FullName, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage);
                                    strikeNPC.RemoveAll(x => x.id == 127 || x.id == 128 || x.id == 129 || x.id == 130 || x.id == 131 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                    return;
                                }
                            }
                            break;
                        case 245://石巨人的特殊处理
                        case 246:
                        case 247:
                        case 248:
                            {
                                StrikeNPC? strike = strikeNPC.Find(x => x.id == 245);
                                if (strike == null)
                                {
                                    int index = -1;
                                    foreach (var n in Main.npc)
                                    {
                                        if (n.netID == 245)
                                        {
                                            index = n.whoAmI;
                                        }
                                    }
                                    if (index == -1)//不可能发生这种情况
                                    {
                                        strikeNPC.RemoveAll(x => x.id == 245 || x.id == 246 || x.id == 247 || x.id == 248 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                        return;
                                    }
                                    strike = new StrikeNPC(index, 245, Main.npc[index].FullName, true, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage, 400000);
                                    strikeNPC.Add(strike);
                                }
                                else if (strikeNPC[i].id != 245)//把除了本体以外的肢体的伤害计算加到本体上
                                {
                                    foreach (var v in strikeNPC[i].playerAndDamage)
                                    {
                                        if (strike.playerAndDamage.ContainsKey(v.Key))
                                        {
                                            strike.playerAndDamage[v.Key] += v.Value;
                                            strike.AllDamage += v.Value;
                                        }
                                        else
                                        {
                                            strike.playerAndDamage.Add(v.Key, v.Value);
                                            strike.AllDamage += v.Value;
                                        }
                                    }
                                }
                                if (strikeNPC[i].id == 245)
                                {
                                    edPlayers.ForEach((Action<ExtraData>)(x =>
                                    {
                                        if (strikeNPC[i].playerAndDamage.TryGetValue(x.Account, out int value))
                                        {
                                            x.killNPCnum += 1;

                                            int point = 0;
                                            if (config.WhetherToEnablePointStatistics)
                                            {
                                                point = (int)(value * 1.0f / strikeNPC[i].AllDamage * strikeNPC[i].value / 100);
                                                x.point += point;
                                            }

                                            if (x.killBossID.ContainsKey(245))
                                                x.killBossID[245]++;
                                            else
                                                x.killBossID.Add(245, 1);
                                            List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                            if (temp.Count != 0)
                                            {
                                                if (!x.hideKillTips)
                                                    SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                                if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                                    SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                                            }
                                        }
                                    }));
                                    if (config.WhetherToEnableTheKillBossDamageLeaderboard)
                                        SendKillBossMessage(args.npc.FullName, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage);
                                    strikeNPC.RemoveAll(x => x.id == 245 || x.id == 246 || x.id == 247 || x.id == 248 || x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                    return;
                                }
                            }
                            break;
                        default:
                            {
                                edPlayers.ForEach((Action<ExtraData>)(x =>
                                {
                                    if (strikeNPC[i].playerAndDamage.TryGetValue(x.Account, out int value))
                                    {
                                        x.killNPCnum++;

                                        int point = 0;
                                        if (config.WhetherToEnablePointStatistics)
                                        {
                                            point = (int)(value * 1.0f / strikeNPC[i].AllDamage * strikeNPC[i].value / 100);
                                            if (point == 0 && args.npc.CanBeChasedBy())
                                                point = 1;
                                            x.point += point;
                                        }

                                        if (strikeNPC[i].isBoss)
                                        {
                                            if (x.killBossID.ContainsKey(strikeNPC[i].id))
                                                x.killBossID[strikeNPC[i].id]++;
                                            else
                                                x.killBossID.Add(strikeNPC[i].id, 1);
                                        }
                                        if (args.npc.rarity > 0)
                                        {
                                            if (x.killRareNPCID.ContainsKey(strikeNPC[i].id))
                                                x.killRareNPCID[strikeNPC[i].id]++;
                                            else
                                                x.killRareNPCID.Add(strikeNPC[i].id, 1);
                                        }
                                        List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                        if (temp.Count != 0)
                                        {
                                            if (args.npc.rarity > 0)
                                            {
                                                if (!x.hideKillTips)
                                                    SendAllText(temp[0], "rare kill + 1", new Color(0, 150, 255), new Color(0, 95, 160), args.npc.Center - Vector2.UnitY * 10);
                                            }
                                            else
                                            {
                                                if (!x.hideKillTips)
                                                    SendAllText(temp[0], "kill + 1", Color.White, Color.Gray, args.npc.Center - Vector2.UnitY * 10);
                                            }
                                            if (strikeNPC[i].isBoss)
                                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                            if (!x.hidePointTips && config.WhetherToEnablePointStatistics)
                                                SendAllText(temp[0], $"+ {point} $", new Color(255, 100, 255), new Color(150, 75, 150), temp[0].TPlayer.Center);
                                        }
                                    }
                                }));
                                if (config.WhetherToEnableTheKillBossDamageLeaderboard && (args.npc.boss || args.npc.netID == 551 || args.npc.netID == 125 || args.npc.netID == 126 || config.WhichCreaturesAreAlsoIncludedInTheKillDamageLeaderboard.Contains(args.npc.netID)))
                                {
                                    SendKillBossMessage(args.npc.FullName, strikeNPC[i].playerAndDamage, strikeNPC[i].AllDamage);
                                }
                                strikeNPC.RemoveAt(i);
                                strikeNPC.RemoveAll(x => x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
                                return;
                            }
                            break;
                    }
                }
                //清理因为意外导致的不正确的数据
                if (i >= 0 && (strikeNPC[i].id != Main.npc[strikeNPC[i].index].netID || !Main.npc[strikeNPC[i].index].active))
                {
                    strikeNPC.RemoveAt(i);
                    i--;
                }
            }
        }


        /// <summary>
        /// 重置死亡状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpawn(object? sender, GetDataHandlers.SpawnEventArgs e)
        {
            if (!config.WhetherToEnableDeathStatistics)
            {
                return;
            }
            edPlayers.ForEach(x =>
            {
                if (x.Name == e.Player.Name && x.dead)
                {
                    x.dead = false;
                }
            });
        }


        /// <summary>
        /// 记录死亡次数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHPChange(object? sender, GetDataHandlers.PlayerHPEventArgs e)
        {
            if (e.Current == 0 && config.WhetherToEnableDeathStatistics)
            {
                edPlayers.ForEach(x =>
                {
                    if (x.Name == e.Player.Name && !x.dead)
                    {
                        x.dead = true;
                        x.deathCount++;
                    }
                });
            }
        }


        /// <summary>
        /// 重新加载配置
        /// </summary>
        /// <param name="e"></param>
        private void OnReload(ReloadEventArgs e)
        {
            config = ZhipmConfig.LoadConfigFile();
            if (config.MaximumNumberOfBackupFilesPerPlayer < 1)
            {
                config.MaximumNumberOfBackupFilesPerPlayer = 5;
                e.Player.SendMessage("The minimum number of backup archives is 1, please do not enter an invalid value, it has been modified to the default 5", new Color(255, 0, 0));
            }
            if (!config.WhetherToEnableDeathStatistics)
            {
                edPlayers.ForEach(x =>
                {
                    if (x.dead)
                    {
                        x.dead = false;
                    }
                });
            }
        }
    }
}