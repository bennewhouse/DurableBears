using System;
using BepInEx;
using RoR2;
using R2API;
using R2API.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ItemStats.Stat;
using ItemStats;
using System.Collections.Generic;
using ItemStats.ValueFormatters;

namespace DurableBears
{

    [BepInPlugin("mod.orare.durablebears", "Durable Bears", modVersion)]
    [R2APISubmoduleDependency(nameof(R2API.LanguageAPI))]
    [BepInDependency(itemStatsModName, BepInDependency.DependencyFlags.SoftDependency)]

    public class DurableBears : BaseUnityPlugin
    {
        public const string modVersion = "0.1.5";
        public const string itemStatsModName = "dev.ontrigger.itemstats";
        public void Awake()
        {
            RemoveBearDodge();
            AddBearArmor();
            ReplaceBearToolTip();
        }

        public void RemoveBearDodge()
        {
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                //RoR2.HealthComponent - line 447.
                ILCursor cursor = new ILCursor(il);
                cursor.GotoNext(
                    x => x.MatchLdcI4(0),
                    x => x.Match(OpCodes.Ble_S),
                    x => x.MatchLdcR4(15)
                   );
                //Bear count needed for trigger is now set at >Int32.Maxvalue instead of >0
                cursor.Remove();
                cursor.Emit(OpCodes.Ldc_I4, Int32.MaxValue);
            };
        }
        public void AddBearArmor()
        {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                if(self.inventory)
                {
                    var itemCount = self.inventory.GetItemCount(ItemIndex.Bear);
                    self.InvokeMethod("set_armor", self.armor + itemCount * 15);
                }
            };
        }

        public void ReplaceBearToolTip()
        {
            R2API.LanguageAPI.Add("ITEM_BEAR_PICKUP", "Reduce incoming damage.");
            R2API.LanguageAPI.Add("ITEM_BEAR_DESC", "<style=cIsHealing>Increase armor</style> by <style=cIsHealing>15</style> <style=cStack>(+15 per stack)</style>.");

            if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(itemStatsModName))
            {
                ItemStatDef statDef = ItemStatsMod.GetItemStatDef(ItemIndex.Bear);
                statDef.Stats = new List<ItemStat>
                {
                    new ItemStat(
                    (itemCount, ctx) => 15f * itemCount,
                    (value, ctx) => $"Bonus Armor: {value.FormatInt()}"
                    )
                };
            }
        }

    }
}
