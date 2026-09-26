using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace WarlockMod.Warlock.Content
{
    internal static class Dots
    {
        private static readonly DotController.DotDef procBleedDef = new DotController.DotDef();
        private static DotController.DotIndex procBleed;
        private static bool initialized;

        internal static void Init()
        {
            if (initialized) return;
            procBleed = DotAPI.RegisterDotDef(procBleedDef, RefreshBleed, null, DealProcBleed);
            if (procBleed == DotController.DotIndex.None)
                throw new System.InvalidOperationException("Warlock metamagic bleed registration failed.");
            On.RoR2.DotController.InitDotCatalog += InitializeBleedDefinition;
            initialized = true;
        }

        private static void InitializeBleedDefinition(On.RoR2.DotController.orig_InitDotCatalog orig)
        {
            orig();
            var vanilla = DotController.GetDotDef(DotController.DotIndex.Bleed);
            procBleedDef.interval = vanilla.interval;
            procBleedDef.damageCoefficient = vanilla.damageCoefficient;
            procBleedDef.damageColorIndex = vanilla.damageColorIndex;
            procBleedDef.associatedBuff = vanilla.associatedBuff;
        }

        internal static void Unhook()
        {
            if (initialized) On.RoR2.DotController.InitDotCatalog -= InitializeBleedDefinition;
        }

        internal static void InflictBleed(GameObject victim, GameObject attacker, int stacks, float sourceProc)
        {
            if (!NetworkServer.active || !victim || !attacker || stacks <= 0 || sourceProc <= 0f ||
                WarlockConfig.BleedDamageMultiplier <= 0f) return;
            var index = WarlockConfig.BleedProc > 0f ? procBleed : DotController.DotIndex.Bleed;
            for (int i = 0; i < stacks; i++)
                DotController.InflictDot(victim, attacker, null, index, WarlockConfig.BleedDuration,
                    sourceProc * WarlockConfig.BleedDamageMultiplier);
        }

        private static void RefreshBleed(DotController controller, DotController.DotStack added)
        {
            foreach (var stack in controller.dotStackList)
            {
                if (stack.dotIndex == procBleed && stack.timer < added.timer)
                {
                    stack.timer = added.timer;
                    stack.totalDuration = added.totalDuration;
                }
            }
        }

        private static void DealProcBleed(DotController controller, DotController.PendingDamage pending)
        {
            if (!NetworkServer.active || !controller.victimHealthComponent || !controller.victimBody) return;
            var damage = new DamageInfo
            {
                attacker = pending.attackerObject,
                damage = pending.totalDamage,
                inflictor = controller.gameObject,
                position = controller.victimBody.corePosition,
                procCoefficient = WarlockConfig.BleedProc,
                damageColorIndex = procBleedDef.damageColorIndex,
                damageType = pending.damageType | DamageType.DoT,
                dotIndex = procBleed,
                inflictedHurtbox = pending.hitHurtBox
            };
            damage.procChainMask.AddProc(ProcType.BleedOnHit);
            damage.AddModdedDamageType(DamageTypes.HexMask);
            controller.victimHealthComponent.TakeDamage(damage);
            if (!damage.rejected && damage.procCoefficient > 0f)
            {
                GlobalEventManager.instance.OnHitEnemy(damage, controller.victimObject);
                GlobalEventManager.instance.OnHitAll(damage, controller.victimObject);
            }
        }
    }
}
