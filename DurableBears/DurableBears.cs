using System;
using BepInEx;
using RoR2;
using R2API;
using R2API.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace DurableBears
{

    [BepInPlugin("mod.orare.durablebears", "Durable Bears", modVersion)]
    [R2APISubmoduleDependency(nameof(R2API.LanguageAPI))]
    [BepInDependency(itemStatsModName, BepInDependency.DependencyFlags.SoftDependency)]

    public class DurableBears : BaseUnityPlugin
    {
        public const string modVersion = "1.0.1";
        public const string itemStatsModName = "dev.ontrigger.itemstats";

        public static ConfigEntry<int> configInitialArmor;
        public static ConfigEntry<int> configAdditionalArmor;

        public void Awake()
        {
            configInitialArmor = Config.Bind("General Settings", "Initial Armor", 15, "The amount of armor the inital item gives.");
            configAdditionalArmor = Config.Bind("General Settings", "Additional Armor", 15, "The amount of armor subsequent stacks give.");

            RemoveBearDodge();
            AddBearArmor();
            ReplaceBearToolTip();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(itemStatsModName))
                ItemStatsModCustomDefs.AddItemStatsModDef();
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
                if (self.inventory)
                {
                    var itemCount = self.inventory.GetItemCount(ItemIndex.Bear);
                    if (itemCount > 0)
                        self.InvokeMethod("set_armor", self.armor + configInitialArmor.Value + (itemCount - 1) * configAdditionalArmor.Value);
                }
            };
        }

        public void ReplaceBearToolTip()
        {
            R2API.LanguageAPI.Add("ITEM_BEAR_PICKUP", "Reduce incoming damage.");
            R2API.LanguageAPI.Add("ITEM_BEAR_DESC", $"<style=cIsHealing>Increase armor</style> by <style=cIsHealing>{configInitialArmor.Value }</style> <style=cStack>(+{configAdditionalArmor.Value} per stack)</style>.");
        }   
    }
}