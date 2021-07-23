using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace DurableBears
{
    public class ItemStatsModCustomDefs
    {
        public static void AddItemStatsModDef()
        {
            ItemStats.ItemStatDef statDef = ItemStats.ItemStatsMod.GetItemStatDef(RoR2Content.Items.Bear.itemIndex);
            statDef.Stats = new List<ItemStats.Stat.ItemStat>
                {
                    new ItemStats.Stat.ItemStat(
                    (itemCount, ctx) => DurableBears.configInitialArmor.Value + (itemCount-1) * DurableBears.configAdditionalArmor.Value,
                    (value, ctx) => $"Bonus Armor: {ItemStats.ValueFormatters.Extensions.FormatInt(value)}"
                    )
                };
        }
    }
}
