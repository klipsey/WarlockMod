using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using WarlockMod.Modules;
using WarlockMod.Warlock.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static RoR2.DotController;

namespace WarlockMod.Warlock.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType BloodExplosionDamage;
        public static DamageAPI.ModdedDamageType HexMask;

        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            BloodExplosionDamage = DamageAPI.ReserveDamageType();
            HexMask = DamageAPI.ReserveDamageType();
            Hook();
        }
        private static void Hook()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (!damageReport.attackerBody || !damageReport.victimBody)
            {
                return;
            }
            HealthComponent victim = damageReport.victim;
            GameObject inflictorObject = damageInfo.inflictor;
            CharacterBody victimBody = damageReport.victimBody;
            EntityStateMachine victimMachine = victimBody.GetComponent<EntityStateMachine>();
            CharacterBody attackerBody = damageReport.attackerBody;
            GameObject attackerObject = damageReport.attacker.gameObject;
            WarlockController iController = attackerBody.GetComponent<WarlockController>();
            if (NetworkServer.active)
            {
                if (iController && attackerBody.baseNameToken == "KENKO_WARLOCK_NAME")
                {
                    if(victimBody.HasBuff(WarlockBuffs.warlockHexxedEmpoweredDebuff) && !damageInfo.HasModdedDamageType(HexMask))
                    {
                        BlastAttack obj = new BlastAttack
                        {
                            radius = 15f,
                            procCoefficient = damageInfo.procCoefficient,
                            position = victimBody.corePosition,
                            attacker = attackerObject,
                            crit = damageInfo.crit,
                            baseDamage = damageInfo.damage / 2f,
                            falloffModel = BlastAttack.FalloffModel.None,
                            damageColorIndex = DamageColorIndex.Sniper,
                            baseForce = 500f
                        };
                        obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
                        obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
                        obj.AddModdedDamageType(HexMask);
                        obj.Fire();
                        EffectManager.SpawnEffect(Content.WarlockAssets.bloodExplosionEffect2, new EffectData
                        {
                            origin = victimBody.corePosition,
                            rotation = Quaternion.identity,
                            scale = 0.5f
                        }, false);
                    }
                    else if(victimBody.HasBuff(WarlockBuffs.warlockHexxedDebuff) && !damageInfo.HasModdedDamageType(HexMask))
                    {
                        DamageInfo obj2 = new DamageInfo
                        {
                            procCoefficient = damageInfo.procCoefficient,
                            position = victimBody.corePosition,
                            attacker = attackerObject,
                            crit = damageInfo.crit,
                            damage = damageInfo.damage / 2f,
                            damageColorIndex = DamageColorIndex.Sniper,
                            damageType = DamageType.Stun1s,
                        };
                        obj2.AddModdedDamageType(HexMask);
                        victimBody.healthComponent.TakeDamage(obj2);
                    }
                }
            }
        }
    }
}
