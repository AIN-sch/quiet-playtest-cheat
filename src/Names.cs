using Common.Client;
using DataTable;
using System.Collections.Generic;

namespace QUIETCheat
{
    /// <summary>枚举 → 中文名 对照表（ESP 用）。查不到回退枚举名。</summary>
    public static class Names
    {
        private static readonly Dictionary<MonsterType, string> _monsters = new Dictionary<MonsterType, string>
        {
            { MonsterType.GrandMa, "奶奶" },
            { MonsterType.GrandMa_Guard, "守卫奶奶" },
            { MonsterType.GrandMa_Raptor, "迅猛龙奶奶" },
            { MonsterType.T_Rex, "霸王龙" },
            { MonsterType.Puppy, "小狗" },
        };

        private static readonly Dictionary<StealableType, string> _items = new Dictionary<StealableType, string>
        {
            { StealableType.TV, "电视" },
            { StealableType.PC, "电脑" },
            { StealableType.SAFE, "保险箱" },
            { StealableType.STATUE, "雕像" },
            { StealableType.FRAME, "相框" },
            { StealableType.LAPTOP, "笔记本电脑" },
            { StealableType.USB, "U盘" },
            { StealableType.CALCULATOR, "计算器" },
            { StealableType.RADIO, "收音机" },
            { StealableType.BOOKS, "书籍" },
            { StealableType.ARMOR, "盔甲" },
            { StealableType.HELMET, "头盔" },
            { StealableType.SWORD, "剑" },
            { StealableType.CAN, "罐头" },
            { StealableType.BOTTLE, "瓶子" },
            { StealableType.FRYINGPAN, "平底锅" },
            { StealableType.PLATE, "盘子" },
            { StealableType.GOLD, "黄金" },
            { StealableType.SILVERGLASS, "银酒杯" },
            { StealableType.DENTURES, "假牙" },
            { StealableType.STICK, "拐杖" },
            { StealableType.TEDDYBEAR, "泰迪熊" },
            { StealableType.TOYCUBE, "玩具积木" },
            { StealableType.RIFLE, "步枪" },
            { StealableType.SMARTPHONE, "智能手机" },
            { StealableType.POWERBANK, "充电宝" },
            { StealableType.CHICKEN, "鸡肉" },
            { StealableType.ALARM, "闹钟" },
            { StealableType.COUSHION, "抱枕" },
            { StealableType.PLANT, "盆栽" },
            { StealableType.POKETWATCH, "怀表" },
            { StealableType.NECKLACE, "项链" },
            { StealableType.CAMERA, "相机" },
            { StealableType.RUBBERBUCK, "橡皮鸭" },
            { StealableType.OUIJABOARD, "通灵板" },
            { StealableType.BIGTEDDYBEAR, "大泰迪熊" },
            { StealableType.GRANDMASEWINGKIT, "奶奶的缝纫包" },
            { StealableType.DETERGENT, "清洁剂" },
            { StealableType.STAGHEAD, "鹿头标本" },
            { StealableType.SEASONING, "调味料" },
            { StealableType.RING, "戒指" },
            { StealableType.WALLET, "钱包" },
        };

        /// <summary>怪物类型 → 中文名；查不到回退枚举名。</summary>
        public static string MonsterName(MonsterType type)
        {
            return _monsters.TryGetValue(type, out var s) ? s : type.ToString();
        }

        /// <summary>可偷物品类型 → 中文名；查不到回退枚举名。</summary>
        public static string ItemName(StealableType type)
        {
            return _items.TryGetValue(type, out var s) ? s : type.ToString();
        }
    }
}
