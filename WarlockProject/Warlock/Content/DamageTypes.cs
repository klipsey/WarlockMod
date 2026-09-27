using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WarlockMod.Warlock.Components;

namespace WarlockMod.Warlock.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType HexMask;
        private static bool hooked;
        private static int damageDepth;
        private static bool applyingHexDamage;
        private static readonly Queue<PendingHexDamage> pendingHexDamage = new Queue<PendingHexDamage>();

        private struct PendingHexDamage
        {
            public HealthComponent victim;
            public DamageInfo damage;
            public int bleedStacks;
        }

        internal static void Init()
        {
            if (hooked) return;
            HexMask = DamageAPI.ReserveDamageType();
            On.RoR2.HealthComponent.TakeDamageProcess += TakeDamageProcess;
            GlobalEventManager.onServerDamageDealt += OnDamageDealt;
            hooked = true;
        }

        internal static void Unhook()
        {
            if (!hooked) return;
            GlobalEventManager.onServerDamageDealt -= OnDamageDealt;
            On.RoR2.HealthComponent.TakeDamageProcess -= TakeDamageProcess;
            pendingHexDamage.Clear();
            hooked = false;
        }

        private static void TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damage)
        {
            damageDepth++;
            bool completed = false;
            try
            {
                orig(self, damage);
                completed = true;
            }
            finally
            {
                damageDepth--;
                if (!completed) pendingHexDamage.Clear();
            }
            if (damageDepth == 0) ApplyPendingHexDamage();
        }

        private static void ApplyPendingHexDamage()
        {
            if (applyingHexDamage || pendingHexDamage.Count == 0) return;
            applyingHexDamage = true;
            try
            {
                while (pendingHexDamage.Count > 0)
                {
                    var pending = pendingHexDamage.Dequeue();
                    if (!pending.victim || !pending.victim.alive) continue;
                    pending.victim.TakeDamage(pending.damage);
                    if (pending.victim && !pending.damage.rejected)
                        Dots.InflictBleed(pending.victim.gameObject, pending.damage.attacker, pending.bleedStacks, pending.damage.procCoefficient);
                }
            }
            finally
            {
                applyingHexDamage = false;
                pendingHexDamage.Clear();
            }
        }

        private static void OnDamageDealt(DamageReport report)
        {
            if (!NetworkServer.active || !report.attackerBody || !report.victimBody ||
                !report.attackerBody.GetComponent<WarlockController>()) return;
            var damage = report.damageInfo;
            if (damage.rejected || damage.damage <= 0f || damage.dotIndex != DotController.DotIndex.None) return;
            if (damage.HasModdedDamageType(HexMask))
            {
                var blast = damage.inflictor ? damage.inflictor.GetComponent<DelayBlastWarlock>() : null;
                if (blast) Dots.InflictBleed(report.victim.gameObject, damage.attacker, blast.bleedStacks, damage.procCoefficient);
                return;
            }

            var victim = report.victimBody;
            int stacks = victim.GetBuffCount(WarlockBuffs.warlockHexxedDebuff);
            int empoweredStacks = victim.GetBuffCount(WarlockBuffs.warlockHexxedEmpoweredDebuff);
            int bleedStacks = victim.GetBuffCount(WarlockBuffs.warlockHexxedMetaMagicDebuff);
            if (WarlockConfig.BleedProc > 0f && damage.procChainMask.HasProc(ProcType.BleedOnHit))
                bleedStacks = 0;

            if (empoweredStacks > 0)
            {
                DamageTypeCombo damageType = DamageType.Stun1s;
                damageType.damageSource = DamageSource.Secondary;

                var blastObject = Object.Instantiate(WarlockAssets.warlockHexExplodeEffect, victim.corePosition, Quaternion.identity);
                var blast = blastObject.GetComponent<DelayBlastWarlock>();
                blast.position = victim.corePosition;
                blast.baseDamage = damage.damage * WarlockConfig.EmpoweredHexDamage * empoweredStacks;
                blast.radius = 16f;
                blast.attacker = damage.attacker;
                blast.inflictor = blastObject;
                blast.crit = damage.crit;
                blast.maxTimer = 0f;
                blast.damageColorIndex = DamageColorIndex.Sniper;
                blast.falloffModel = BlastAttack.FalloffModel.None;
                blast.procChainMask = damage.procChainMask;
                blast.bleedStacks = bleedStacks;
                blast.damageType = damageType;
                blast.moddedDamageTypeHolder.Add(HexMask);
                blastObject.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;
            }
            
            if(stacks > 0)
            {
                DamageTypeCombo damageType = DamageType.Stun1s;
                damageType.damageSource = DamageSource.Secondary;

                var bonus = new DamageInfo
                {
                    procCoefficient = damage.procCoefficient * WarlockConfig.HexProcMultiplier,
                    procChainMask = damage.procChainMask,
                    position = victim.corePosition,
                    attacker = damage.attacker,
                    inflictor = victim.gameObject,
                    crit = damage.crit,
                    damage = damage.damage * WarlockConfig.HexDamage * stacks,
                    damageColorIndex = DamageColorIndex.Sniper,
                    damageType = damageType
                };
                bonus.AddModdedDamageType(HexMask);
                
                pendingHexDamage.Enqueue(new PendingHexDamage
                {
                    victim = victim.healthComponent,
                    damage = bonus,
                    bleedStacks = bleedStacks
                });
            }
        }
    }
}
