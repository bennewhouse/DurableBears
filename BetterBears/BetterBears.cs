using System;
using BepInEx;
using RoR2;
using R2API;
using R2API.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;


namespace BetterBears
{

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("mod.orare.betterbears", "Better Bears", modVersion)]

    public class BetterBears : BaseUnityPlugin
    {
        public const string modVersion = "0.1.5";
        public void Awake()
        {
            RemoveBearDodge();
            AddBearArmor();
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

    }
}
