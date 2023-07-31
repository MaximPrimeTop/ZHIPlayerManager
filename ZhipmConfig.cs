using Newtonsoft.Json;
using TShockAPI;

namespace ZHIPlayerManager
{
    public class ZhipmConfig
    {
        static string configPath = Path.Combine(TShock.SavePath + "/Zhipm", "ZhiPlayerManager.json");

        /// <summary>
        /// 从文件中导出
        /// </summary>
        /// <returns></returns>
        public static ZhipmConfig LoadConfigFile()
        {
            if (!Directory.Exists(TShock.SavePath + "/Zhipm"))
            {
                Directory.CreateDirectory(TShock.SavePath + "/Zhipm");
            }
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new ZhipmConfig(
                    true, true, true, false, true, false, true, true, 20, 5, new List<int>()
                    ), Formatting.Indented));
            }

            return JsonConvert.DeserializeObject<ZhipmConfig>(File.ReadAllText(configPath));
        }

        public ZhipmConfig() { }

        public ZhipmConfig(bool 是否启用在线时长统计, bool 是否启用死亡次数统计, bool 是否启用击杀NPC统计, bool 是否启用点数统计, bool 默认击杀字体是否对玩家显示, bool 默认点数字体是否对玩家显示, bool 是否启用击杀Boss伤害排行榜, bool 是否启用玩家自动备份, int 默认自动备份的时间_单位分钟_若为0代表关闭, int 每个玩家最多几个备份存档, List<int> 哪些生物也包含进击杀伤害排行榜)
        {
            this.WhetherToEnableOnlineTimeStatistics = 是否启用在线时长统计;
            this.WhetherToEnableDeathStatistics = 是否启用死亡次数统计;
            this.WhetherToEnableKillNPCStatistics = 是否启用击杀NPC统计;
            this.WhetherToEnablePointStatistics = 是否启用点数统计;
            this.WhetherTheDefaultKillFontIsDisplayedToPlayers = 默认击杀字体是否对玩家显示;
            this.WhetherTheDefaultPipFontIsDisplayedToThePlayer = 默认点数字体是否对玩家显示;
            this.WhetherToEnableTheKillBossDamageLeaderboard = 是否启用击杀Boss伤害排行榜;
            this.WhetherToEnableAutomaticPlayerBackup = 是否启用玩家自动备份;
            this.TheDefaultAutomaticBackupTimeInMinutes_IfIts0ItMeansOff = 默认自动备份的时间_单位分钟_若为0代表关闭;
            this.MaximumNumberOfBackupFilesPerPlayer = 每个玩家最多几个备份存档;
            this.WhichCreaturesAreAlsoIncludedInTheKillDamageLeaderboard = 哪些生物也包含进击杀伤害排行榜;
        }

        public bool WhetherToEnableOnlineTimeStatistics;
        public bool WhetherToEnableDeathStatistics;
        public bool WhetherToEnableKillNPCStatistics;
        public bool WhetherToEnablePointStatistics;
        public bool WhetherTheDefaultKillFontIsDisplayedToPlayers;
        public bool WhetherTheDefaultPipFontIsDisplayedToThePlayer;
        public bool WhetherToEnableTheKillBossDamageLeaderboard;
        public bool WhetherToEnableAutomaticPlayerBackup;
        public int TheDefaultAutomaticBackupTimeInMinutes_IfIts0ItMeansOff;
        public int MaximumNumberOfBackupFilesPerPlayer;
        public List<int> WhichCreaturesAreAlsoIncludedInTheKillDamageLeaderboard;

    }
}
