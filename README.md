# ZHIPlayerManager
Versatile management player plugin
- originally made by [skywhale-zhi](https://github.com/skywhale-zhi)
- translated to English by [TheTrueMomo](https://github.com/TheTrueMomo)
- final patches/translations by Maxthegreat99
- my dum dum doesn't know how to do stuff so i decided to fork to delete 2 lines of code

# Translated README
## Features:

### 1. View player inventory and status
Check the function of the backpack, you can check 1.44 Updated extra backpack slot, offline or not.The function of checking the status, currently can display the HP, Mana, number of finished fishing quests, permanent buffs，the number of buffs, the number of coins, additional data and： Online time, the number of creatures killed, the situation of killing the Boss, the number of rare creatures killed, the number of deaths, etc., support offline

### 2. Modify player information
Allows modification of almost any player information，Including HP, Max HP, Mana, Max Mana, number of fishering tasks, torch god, demon heart activation, Artisan Loaf, life crystal, Aegis Fruit, Arcane Crystal, galaxy pearl, Gummy worm, delicacy, super minecart modification, support modification of online and offline players


### 3. Allow player backups and auto-backups
Players sometimes lose save files in the server,Especially when the game freezes,This plugin allows players to manually back up their archives or automatically back up,Up to five archive columns, more than five oldest will be automatically deleted (default 5, can be adjusted)， **Do not give the player permission to rollback**, If so players can freely brush things,It is recommended that the administrator hold the rollback permission， **The data is recorded in the Zhipm_PlayerBackUp table of tshock.sqlite**. Regardless of whether the player is online or not, whether the player's default equipment bar is consistent with the backup default equipment bar, it can be successful.The same is true for the copy character archive command below, which is quite convenient

### 4. Clean up player data
The plug-in has multiple types of clearing instructions, enter `/zreset help` for details. And enter the command `/zresetallplayers` when opening up wasteland on the server, you can directly clear all the data of all players

### 5. Copy player save
You can copy one person's save file to another at any time, whether they are online or not

### 6. Record player play time
This plug-in will record the player's playing time and will not disappear with the closing of the server. **The data is recorded in the Zhipm_PlayerExtra table of tshock.sqlite**, and you can choose whether to enable it

### 7. Data Leaderboard
Currently there are rankings for playing time, total money, number of completed missions, number of creatures killed, number of rare creatures killed, and bosses killed. You can view them in the `/zsort` series of commands

### 8. More friendly ban command
This ban command `/zban` supports offline ban, and bans acc, ip, uuid at the same time. It supports fuzzy search for names, but it will only ban when it finds only one player.

### 9. Allows player character saves to be exported
The `/zout` series commands allow character data to be exported, and are packaged in folders according to the current map name, except that the missing character data will cause the export to fail, and others can be exported

### 10. freeze player
The `/zfre name` command can directly freeze the player, compare the three data of acc, ip, and uuid, and freeze it directly if they match. Can freeze offline players when they re-enter the server. Note that this function will be invalid after the server is restarted, it is only used for temporary freezing, if you want to use `/zban` for a long time

### 11. Clean invalid server data
This feature is going to be removed from this plugin in the future, because it is a bit off topic. Using the `/zclear` command will clear all useless NPCs in the world that are not Boss and non-town NPCs after 20 seconds, clean up all items that fall on the ground, clean up all projectiles, and reduce invalid data in the server.

### 12. Bossfight player stats
When the player kills the Boss, the plugin can count the damage output of each player and broadcast the output. Players can see their contributions. Can be set to include damage dealt to mobs

## instruction
- Help series, quickly view all instructions of the plugin
- Permission 1: [None, anyone can use]
- Command 1: `/zhelp`
- Function 1: View all command help under this plugin
-
- save series, for saving archive backups, for viewing backups
- Permission 2: `zhipm.save`
- Command 2-1: `/zsave`
- Function 2-1: Backup your character archive
- Command 2-2: `/zvisa [num]`
- Function 2-2: View your own backup inventory, num ranges from 1 to 5 (default 5, can be changed) **num can be left blank**, the smaller the backup, the newer, the default is to automatically view the latest backup when num is not entered 1
- Command 2-3: `/zvisa [name] [num]`
- Function 2-3: View someone’s backup inventory, num ranges from 1 to 5 (default 5, can be changed) **num can be left blank**, the smaller the backup, the newer, the default is to automatically view the latest backup when num is not entered 1
- Commands 2-4: `/zsaveauto [num]`
- Function 2-4: Allow the user to automatically back up himself, fill in the number to automatically back up every num minutes, fill in 0 to disable this function
-
- Rollback series, separate permissions to prevent players from freely swiping items
- Permission 3: `zhipm.back`
- Command 3: `/zback [name] [num]`
- Function 3: Let the player go back to the num number backup, **num can be left blank**, if not filled in, it will go back to the latest save 1 by default, this command should not be given to ordinary players, otherwise they can use the backup and back up to brush items, it is only designed for the convenience of management and use
-
- Duplicate series, clone any player's save
- Permission 4: `zhipm.clone`
- Command 4: `/zclone [name1] [name2]`
- Function 4: Copy the character data of name1 to player name2, **name2 can be left blank**, if not filled, the data of player name1 will be copied to me by default
-
- Modify the series, simply modify some attributes
- Permission 5: `zhipm.modify`
- Command 5-1: `/zmodify help`
- Function 5-1: View the help of zmodify series commands, zmodify series commands can modify online or offline players
- Command 5-2: `/zmodify [name] life [num]`
- Function 5-2: Modify the player's life value to num
- Command 5-3: `/zmodify [name] lifemax [num]`
- Function 5-3: Modify the player's life limit to num
- Command 5-4: `/zmodify [name] mana [num]`
- Function 5-4: Modify the player's mana value to num
- Command 5-5: `/zmodify [name] manamax [num]`
- Function 5-5: Modify the player's maximum mana value to num
- Commands 5-6: `/zmodify [name] fish [num]`
- Function 5-6: Modify the number of tasks completed by the player's fishing to num
- Instructions 5-7: `/zmodify [name] torch [0 or 1]`
- Functions 5-7: Turn off or on the player's torch god or buff
- Commands 5-8: `/zmodify [name] demmon [0 or 1]`
- Function 5-8: Turn off or on the player's demon heart or buff
- Commands 5-9: `/zmodify [name] bread [0 or 1]`
- Features 5-9: Turn off or on player's or artisan bread buffs
- Commands 5-10: `/zmodify [name] heart [0 or 1]`
- Functions 5-10: Turn off or on player's or Life Crystal (Aegis Crystal) buffs
- Commands 5-11: `/zmodify [name] fruit [0 or 1]`
- Functions 5-11: Turn off or on player's or Aegis fruit buffs
- Commands 5-12: `/zmodify [name] star [0 or 1]`
- Functions 5-12: Turn off or on player's or arcane crystal buffs
- Commands 5-13: `/zmodify [name] pearl [0 or 1]`
- Function 5-13: Turn off or on player's or Galactic Pearl buffs
- Commands 5-14: `/zmodify [name] worm [0 or 1]`
- Function 5-14: Turn player's or sticky worm buffs off or on
- Commands 5-15: `/zmodify [name] ambrosia [0 or 1]`
- Function 5-15: Turn off or on the player's or delicacy buff
- Commands 5-16: `/zmodify [name] cart [0 or 1]`
- Function 5-16: Turn off or on player's or super minecart buffs
- Commands 5-17: `/zmodify [name] all [0 or 1]`
- Function 5-17: Turn off or on a player's or all buffs
- Command 5-18: `/zmodify [name] point [num]`
- Function 5-18: Modify player points to num
-
- Freeze series, temporarily freeze disobedient players
- Permission 6: `zhipm.freeze`
- Command 6-1: `/zfre [name]`
- Function 6-1: Freeze the player, compare and freeze the three data from name, uuid, ip, it can be processed even if the player is offline, but it will fail after the server restarts
- Command 6-2: `/zunfre [name]`
- Function 6-2: Unfreeze this player
- Command 6-3: `/zunfre all`
- Function 6-3: Unfreeze all players
-
- Reset series, dangerous instructions, often used to clean up the database with one click or punish players
- Permission 7: `zhipm.reset`
- Command 7-1: `/zresetdb [name]`
- Function 7-1: Reset the backup data of this player, delete the data in the backup database
- Command 7-2: `/zresetdb all`
- Function 7-2: Reset all players' backup data
- Command 7-3: `/zresetex [name]`
- Function 7-3: Reset extra data for this player, delete data in extra database
- Command 7-4: `/zresetex all`
- Function 7-4: Reset extra data for all players
- Command 7-5: `/zreset [name]`
- Function 7-5: Reset the character data of this player, reset according to the configuration of tshock's ssconfig.json
- Command 7-6: `/zreset all`
- Function 7-6: Reset the character data of all players, reset according to the configuration of tshock's ssconfig.json
- Command 7-7: `/zresetallplayers`
- Function 7-7: Reset all data of all players, suitable for new land reclamation, **This command will delete all data in the original tsCharacter table and plug-in Zhipm_PlayerBackUp and Zhipm_PlayerExtra tables! **
-
- Check backpack series, the previous check backpack plug-in is integrated, so there is no z at the beginning
- Permission 8: `zhipm.vi`
- Command 8-1: `/vi [name]`
- Function 8-1: Query all inventories of this player, arrange them in order, and return icon or text according to the situation
- Command 8-2: `/vid [name]`
- Function 8-2: Query all inventories of this player, not in order, and return icon or text according to the situation
-
- Check status information series, same as above
- Permission 9: `zhipm.vs`
- Command 9-1: `/vs [name]`
- Function 9-1: Query all status data of this player, including life value, mana value, number of coins, number of completed mission fish, online time, number of creatures killed, number of bosses, number of rare creatures, permanent gain and buff gain
- Command 9-2: `/vs me`
- Function 9-2: Query self
-
- Ranking series, the collected information ranking, entertainment can also be supervised, such as using the coin ranking to directly find cheating players
- Permission 10: `zhipm.sort`
- Command 10-1: `/zsort help`
- Function 10-1: Check the help of zsort commands
- Command 10-2: `/zsort time [num/all]`
- Function 10-2: Sort the online time of all players in the current server in descending order, **If you don’t fill in num, only the top 10 will be displayed by default**, fill in num to display the top num players, fill in all to display all rankings
- Command 10-3: `/zsort coin [num/all]`
- Function 10-3: Arrange the coins of all players in the current server in descending order, **If you don’t fill in num, only the top 10 will be displayed by default**, fill in num to display the top num names, fill in all to display all rankings
- Command 10-4: `/zsort fish [num/all]`
- Function 10-4: Arrange the number of fishing tasks completed by all players in the current server in descending order. **If you do not fill in num, only the top 10 will be displayed by default**. Fill in num to display the top num names, and fill in all to display all rankings
- Command 10-5: `/zsort boss [num/all]`
- Function 10-5: Arrange the number of bosses killed by all players in the current server in descending order, **If you don’t fill in num, only the top 10 will be displayed by default**, fill in num to display the top num names, fill in all to display all rankings- 指令10-6： `/zsort kill [num/all]`
- Command 10-6: /zsort kill [num/all]
- Function 10-6: Arrange the total number of creatures killed by all players in the current server in descending order. If you do not fill in num, only the top 10 will be displayed by default. Fill in num to display the top num names, and fill in all to display all rankings
- Command 10-7: `/zsort rarenpc [num/all]`
- Function 10-7: Arrange the number of rare creatures killed by all players in the current server in descending order, **If you don’t fill in num, only the top 10 will be displayed by default**, fill in num to display the top num names, fill in all to display all rankings
- Command 10-8: `/zsort point [num/all]`
- Function 10-8: Arrange the points of all players in the current server in descending order, **If you don’t fill in num, only the top 10 will be displayed by default**, fill in num to display the top num players, fill in all to display all rankings
- Command 10-9: `/zsort death [num/all]`
- Function 10-9: Arrange the number of deaths of all players in the current server in descending order, **If you don’t fill in num, only the top 10 will be displayed by default**, fill in num to display the top num players, fill in all to display all rankings
- Commands 10-10: `/zsort clumsy [num/all]`
- Function 10-10: Arrange the game chicken values ​​​​of all players in the current server in descending order. **If you don’t fill in num, only the top 10 will be displayed by default**. Fill in num to display the top num names, and fill in all to display all rankings. **This function is only for entertainment**.
-
- Export data series to meet the needs of some players who want game archives
- Permission 11: `zhipm.out`
- Command 11: `/zout [name/all]`
- Function 11: Export the archive of the character named name, fill in all to export the archive of all characters
-
- Super ban series
- Permission 12: `zhipm.ban`
- Command 12: `/zban add [name] [reason, optional]`
- Function 12: The optimized ban command can remove offline users and support fuzzy search. When the search result is not unique, the ban will fail to avoid false bans. This command will ban acc, ip, uuid at the same time and record it into the log and send out a broadcast after the command is successful. If the player name has a space, you can use English quotation marks to enclose it, such as `/zban add "Super Bear Kid" fried picture`
-
- Player's own game experience settings
- Permission 13: [None, anyone can use]
- Command 13: `/zhide kill or point`
- Function 13: It is used to hide the white floating font of kill + 1 or the pink floating font of + num $ when the player kills the creature, use this command again to unhide
-
- A function that is going to be removed from this plugin, it feels inappropriate to put it here but I don't want to write other plugins for now
- Permission 14: `zhipm.clear`
- Directive 14-1: `/zclear useless`
- Feature 14-1: Cleans up the world of dropped items, non-town NPCs and non-Boss NPCs, and useless projectiles
- Command 14-2: `/zclear buff [name]`
- Function 14-2: Clear all buffs from a player


## Configuration file ZhiPlayerManager.json
```
{
  "Whether to enable online time statistics": true,		//Enabling this feature will record the player's online time
  "是否启用死亡次数统计": true        //同上
  "Whether to enable kill NPC statistics": true,		//ditto
  "Whether to enable point statistics": false,			//Kill monsters to get points, currently in testing, the default is off, need "whether to enable kill NPC statistics" to open
  "Whether the default kill font is shown to players": true,	//Whether to enable kill + 1 monster kill font, need "Enable kill NPC statistics" to open
  "Whether the default pip font is displayed to the player": true,	//The font corresponding to the point is currently in testing, and it is disabled by default. It needs to be enabled in "Enable point statistics"
  "Whether to enable the kill boss damage leaderboard": true, 	//Count and send the player's damage contribution when killing the Boss, need "Enable kill NPC statistics" to open
  "Whether to enable automatic player backup": false,		//Automatic backup, different from manual backup
  "The default automatic backup time_in minutes_if it is 0, it means off": 20,  //Backup online players in the server every 20 minutes
  "The maximum number of backup files for each player": 5, 	//The maximum number of backup files for each player
  "Which creatures are also included in the kill damage ranking list":[] 	//The killing creature damage ranking list corresponding to the killing boss damage ranking list, you can fill in the creature ID here, and you need to enable "whether to enable kill NPC statistics"
}
```

## other
-  **Do not give the backup permission and rollback permission to players at the same time**, so they can freely farm items
- The plug-in is essentially a collection of all-round management of players. It is large in size because commands account for a large proportion and does not take up too much server computing power.
- The plugin adds two tables in tshock.sqlite, Zhipm_PlayerBackUp and Zhipm_PlayerExtra. The former table is a backup of the tsCharater table. The primary key is AccAndSlot, which is a string of xxx-x composed of the player account ID and the backup slot ID. The backup slot ID is 1 ~ 5, which can be modified in the configuration file
- The latter table records the statistical information of this plugin: time online time in seconds, backuptime automatic backup time in minutes, killNPCnum the number of NPCs killed, killBossID the combination of BossIDs and numbers killed, such as 4~10 means killing the Eye of Cthulhu 10 times, killRareNPCID the combination of the ID and number of rare NPCs killed, point points, hideKillTips whether to hide the white floating word of kill+1, hidePointTips Whether to hide the +1$ pink floating word
- Points are a test function, which is equivalent to currency. The advantage is that it avoids the bug of Terra's built-in network card swiping money and the bug of monsters picking up money (making money depreciate rapidly). Points can be obtained by killing monsters. The purpose is to use this plug-in as a pre-plug-in in the future to use statistical information such as points to realize functions such as commodity purchases. Currently, it is disabled by default. You can enable it
- Seeing that there are quite a lot of command permissions, in fact, it is only recommended to give the default players `zhipm.save`, `zhipm.sort`, `zhipm.vi`, `zhipm.vs` these permissions are enough, and other commands can be automatically obtained and used by super management.
- **This plugin has some simple restrictions on player names: the name cannot be pure numbers, the name cannot be completely equal to some instructions of the server, and the first character of the name cannot be a special symbol except [**


## Statistical data
- If you want to use the data organized by this plug-in, please refer to it as a front-end plug-in. I suggest you go to the source code. Here is a brief introduction
```
long Timer;  Timer, recording the running time of the server, unit 1/60 second
List<MessPlayer> frePlayers;  Collection of frozen players
List<ExtraData> edPlayers { get; set; }  The part where all player data is integrated
public class ExtraData
{
    /// Account ID
    int Account;
    /// name
    string Name;
    /// Total online time, in seconds
    long time;
    /// Backup interval, in minutes
    int backuptime;
    /// Number of creatures killed
    int killNPCnum;
    /// The id statistics of killing the boss, id -> number of kills
    Dictionary<int,int> killBossID;
    /// The id statistics of killing rare creatures, id -> number of kills
    Dictionary<int,int> killRareNPCID;
    /// points (a test function, equivalent to currency)
    long point;
    /// Whether to hide kill + 1 word
    bool hideKillTips;
    /// Whether to hide the word with points + 1 $
    bool hidePointTips;
    ///Number of deaths
    int deathCount;
}
```
## Forked repository
https://github.com/skywhale-zhi/ZHIPlayerManager
