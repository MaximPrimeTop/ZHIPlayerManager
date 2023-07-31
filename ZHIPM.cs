using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace ZHIPlayerManager
{
    [ApiVersion(2, 1)]
    public partial class ZHIPM : TerrariaPlugin
    {
        public override string Author => "z枳";

        public override string Description => "Player management, provide any information for modifying players, allow players to back up, roll back and other operations";

        public override string Name => "ZHIPlayerManager";

        public override Version Version => new Version(1, 0, 0, 1);

        #region 字段或属性
        /// <summary>
        /// 人物备份数据库
        /// </summary>
        public static ZplayerDB ZPDataBase { get; set; }
        /// <summary>
        /// 额外数据库
        /// </summary>
        public static ZplayerExtraDB ZPExtraDB { get; set; }
        /// <summary>
        /// 在线玩家的额外数据库的集合
        /// </summary>
        public static List<ExtraData> edPlayers { get; set; }
        /// <summary>
        /// 广播颜色
        /// </summary>
        public readonly static Color broadcastColor = new Color(0, 255, 213);
        /// <summary>
        /// 计时器，60 Timer = 1 秒
        /// </summary>
        public static long Timer
        {
            get;
            private set;
        }
        /// <summary>
        /// 清理数据的计时器
        /// </summary>
        public long cleartime = long.MaxValue;
        /// <summary>
        /// 记录需要冻结的玩家
        /// </summary>
        public static List<MessPlayer> frePlayers = new List<MessPlayer>();
        /// <summary>
        /// 需要记录的被击中的npc
        /// </summary>
        public static List<StrikeNPC> strikeNPC = new List<StrikeNPC>();

        public readonly string noplayer = "The player does not exist, please re-enter";
        public readonly string manyplayer = "This player is not unique, please re-enter";
        public readonly string offlineplayer = "The player is not online and is querying offline data";

        public static ZhipmConfig config = new ZhipmConfig();

        /// <summary>
        /// 记录世界吞噬者的数据
        /// </summary>
        public Dictionary<int, int> Eaterworld = new Dictionary<int, int>();
        /// <summary>
        /// 记录毁灭者的数据
        /// </summary>
        public Dictionary<int, int> Destroyer = new Dictionary<int, int>();
        /// <summary>
        /// 记录血肉墙的数据
        /// </summary>
        public Dictionary<int, int> FleshWall = new Dictionary<int, int>();

        #endregion

        public ZHIPM(Main game) : base(game) { }

        public override void Initialize()
        {
            Timer = 0L;
            config = ZhipmConfig.LoadConfigFile();
            ZPDataBase = new ZplayerDB(TShock.DB);
            ZPExtraDB = new ZplayerExtraDB(TShock.DB);
            edPlayers = new List<ExtraData>();

            //用来对玩家进行额外数据库更新
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            //限制玩家名字类型
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            //同步玩家的额外数据库
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            //用于统计击杀生物数，击杀别的等等
            ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
            //用于统计击杀生物数，击杀别的等等
            ServerApi.Hooks.NpcKilled.Register(this, OnNPCKilled);
            //记录死亡次数
            GetDataHandlers.PlayerSpawn.Register(OnSpawn);
            GetDataHandlers.PlayerHP.Register(OnHPChange);
            //配置文件的更新
            GeneralHooks.ReloadEvent += OnReload;


            #region 指令
            Commands.ChatCommands.Add(new Command("", Help, "zhelp")
            {
                HelpText = "Type /zhelp to view command help"
            });
            Commands.ChatCommands.Add(new Command("zhipm.save", MySSCSave, "zsave")
            {
                HelpText = "Type /zsave to back up your character save"
            });
            Commands.ChatCommands.Add(new Command("zhipm.save", MySSCSaveAuto, "zsaveauto")
            {
                HelpText = "Type /zsaveauto [minute] to automatically back up your own character archives every minute, and turn off this function when the minute is 0"
            });
            Commands.ChatCommands.Add(new Command("zhipm.save", ViewMySSCSave, "zvisa")
            {
                HelpText = "Type /zvisa [num] to view your first character backup"
            });
            Commands.ChatCommands.Add(new Command("zhipm.back", MySSCBack, "zback")
            {
                HelpText = "Type /zback [name] to read the player's character file\nType /zback [name] [num] to read the number of the player's character file"
            });
            Commands.ChatCommands.Add(new Command("zhipm.clone", SSCClone, "zclone")
            {
                HelpText = "Type /zclone [name1] [name2] to copy player 1's character data to player 2\nType /zclone [name] to copy that player's character data to yourself"
            });
            Commands.ChatCommands.Add(new Command("zhipm.modify", SSCModify, "zmodify")
            {
                HelpText = "Type /zmodify help to view the command help for modifying player data"
            });
            Commands.ChatCommands.Add(new Command("zhipm.out", ZhiExportPlayer, "zout")
            {
                HelpText = "Type /zout [name] to export that player's character saves\nType /zout all to export all character saves"
            });
            Commands.ChatCommands.Add(new Command("zhipm.sort", ZhiSortPlayer, "zsort")
            {
                HelpText = "Type /zsort help to see help for the sort series commands"
            });
            Commands.ChatCommands.Add(new Command("", HideTips, "zhide")
            {
                HelpText = "Type /zhide kill to cancel kill +1 display, use enable display again\ntype /zhide point to cancel +1 $ display, use enable display again"
            });

            Commands.ChatCommands.Add(new Command("zhipm.clear", Clear, "zclear")
            {
                HelpText = "Type /zclear useless to clear the world of dropped items, non-town or boss NPCs, and useless projectiles\nType /zclear buff [name] to clear all buffs from that player\nType /zclear buff all to clear all buffs from all players"
            });


            Commands.ChatCommands.Add(new Command("zhipm.freeze", ZFreeze, "zfre")
            {
                HelpText = "Type /zfre [name] to freeze the player"
            });
            Commands.ChatCommands.Add(new Command("zhipm.freeze", ZUnFreeze, "zunfre")
            {
                HelpText = "Type /zunfre [name] to unfreeze that player\nType /zunfre all to unfreeze all players"
            });


            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayerDB, "zresetdb")
            {
                HelpText = "Type /zresetdb [name] to clear the backup data for this player\nType /zresetdb all to clear the backup data for all players"
            });
            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayerEX, "zresetex")
            {
                HelpText = "Type /zresetex [name] to clear extra data for that player\nType /zresetex all to clear extra data for all players"
            });
            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayer, "zreset")
            {
                HelpText = "Type /zreset [name] to clear character data for that player\nType /zreset all to clear character data for all players"
            });
            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayerAll, "zresetallplayers")
            {
                HelpText = "Type /zresetallplayers to clear all data for all players"
            });


            Commands.ChatCommands.Add(new Command("zhipm.vi", ViewInvent, "vi")
            {
                HelpText = "Type /vi [name] to view the player's inventory"
            });
            Commands.ChatCommands.Add(new Command("zhipm.vi", ViewInventDisorder, "vid")
            {
                HelpText = "Type /vid [name] to see the player's inventory, not categorized"
            });
            Commands.ChatCommands.Add(new Command("zhipm.vs", ViewState, "vs")
            {
                HelpText = "Type /vs [name] to see the player's status"
            });


            Commands.ChatCommands.Add(new Command("zhipm.ban", SuperBan, "zban")
            {
                HelpText = "Type /zban add [name] [reason] to ban players whether they are online or not, reason can be left blank"
            });

            #endregion
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNPCKilled);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }


        /// <summary>
        /// 用来记录被冻结玩家数据的类
        /// </summary>
        public class MessPlayer
        {
            public int account;
            public string name;
            public string uuid;
            public Vector2 pos;
            public long clock;
            /// <summary>
            /// 只接受来自 user 的knowIPs，不是单个ip
            /// </summary>
            public string IPs;

            public MessPlayer()
            {
                account = 0;
                name = "";
                uuid = "";
                IPs = "";
                pos = new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16);
                clock = Timer;
            }

            public MessPlayer(int account, string name, string uuid, string IPs, Vector2 pos)
            {
                this.name = name;
                this.uuid = uuid;
                this.account = account;
                this.IPs = IPs;
                this.clock = Timer;
                if(pos == Vector2.Zero)
                    this.pos = new Vector2(0, 999999);
                else
                    this.pos = pos;
            }
        }


        /// <summary>
        /// 用来记录被玩家击中的npc
        /// </summary>
        public class StrikeNPC
        {
            /// <summary>
            /// 索引
            /// </summary>
            public int index;
            /// <summary>
            /// id
            /// </summary>
            public int id;
            /// <summary>
            /// 名字
            /// </summary>
            public string name = string.Empty;
            /// <summary>
            /// 是否为boss
            /// </summary>
            public bool isBoss = false;
            /// <summary>
            /// 字典，用于记录 <击中他的玩家的id -> 该玩家造成的总伤害>
            /// </summary>
            public Dictionary<int, int> playerAndDamage = new Dictionary<int, int>();
            /// <summary>
            /// 受到的总伤害
            /// </summary>
            public int AllDamage = 0;
            /// <summary>
            /// 价值
            /// </summary>
            public float value = 0f;

            public StrikeNPC() { }

            public StrikeNPC(int index, int id, string name, bool isBoss, Dictionary<int, int> playerAndDamage, int allDamage, float value)
            {
                this.index = index;
                this.id = id;
                this.name = name;
                this.isBoss = isBoss;
                this.playerAndDamage = playerAndDamage;
                AllDamage = allDamage;
                this.value = value;
            }
        }
    }
}
