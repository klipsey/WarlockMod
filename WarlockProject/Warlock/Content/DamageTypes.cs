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
        public static DamageAPI.ModdedDamageType WarlockBleed;

        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            BloodExplosionDamage = DamageAPI.ReserveDamageType();
            HexMask = DamageAPI.ReserveDamageType();
            WarlockBleed = DamageAPI.ReserveDamageType();
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
                    if(victimBody.HasBuff(WarlockBuffs.warlockHexxedEmpoweredDebuff) && !damageInfo.HasModdedDamageType(HexMask) && damageInfo.dotIndex == DotIndex.None)
                    {
                        Util.PlaySound("Play_bleedOnCritAndExplode_explode", victimBody.gameObject);
                        GameObject obj8 = UnityEngine.Object.Instantiate(WarlockAssets.warlockHexExplodeEffect, victimBody.corePosition, Quaternion.identity);
                        DelayBlastWarlock obj = obj8.GetComponent<DelayBlastWarlock>();
                        obj.position = victimBody.corePosition;
                        obj.baseDamage = (damageInfo.damage / 2f) * victimBody.GetBuffCount(WarlockBuffs.warlockHexxedEmpoweredDebuff);
                        obj.baseForce = 0f;
                        obj.radius = 16f;
                        obj.attacker = damageInfo.attacker;
                        obj.inflictor = victim.gameObject;
                        obj.crit = damageInfo.crit;
                        obj.maxTimer = 0f;
                        obj.damageColorIndex = DamageColorIndex.Sniper;
                        obj.falloffModel = BlastAttack.FalloffModel.None;
                        obj.moddedDamageTypeHolder.Add(HexMask);
                        if(victimBody.HasBuff(WarlockBuffs.warlockHexxedMetaMagicDebuff))
                        {
                            obj.moddedDamageTypeHolder.Add(WarlockBleed);
                        }
                        obj8.GetComponent<TeamFilter>().teamIndex = damageReport.attackerTeamIndex;
                        NetworkServer.Spawn(obj8);
                    }
                    else if(victimBody.HasBuff(WarlockBuffs.warlockHexxedDebuff) && !damageInfo.HasModdedDamageType(HexMask) && damageInfo.dotIndex == DotIndex.None)
                    {
                        DamageInfo obj2 = new DamageInfo
                        {
                            procCoefficient = damageInfo.procCoefficient,
                            position = victimBody.corePosition,
                            attacker = attackerObject,
                            inflictor = victim.gameObject,
                            crit = damageInfo.crit,
                            damage = (damageInfo.damage / 2f) * victimBody.GetBuffCount(WarlockBuffs.warlockHexxedDebuff),
                            damageColorIndex = DamageColorIndex.Sniper,
                            damageType = DamageType.Stun1s,
                        };
                        obj2.AddModdedDamageType(HexMask);
                        if (victimBody.HasBuff(WarlockBuffs.warlockHexxedMetaMagicDebuff))
                        {
                            obj2.AddModdedDamageType(WarlockBleed);
                        }
                        victimBody.healthComponent.TakeDamage(obj2);
                    }

                    if(damageInfo.HasModdedDamageType(WarlockBleed) && damageInfo.inflictor)
                    {
                        for(int i = 0; i < damageInfo.inflictor.GetComponent<CharacterBody>().GetBuffCount(WarlockBuffs.warlockHexxedMetaMagicDebuff);  i++) 
                        {
                            DotController.InflictDot(victimBody.gameObject, attackerBody.gameObject, RoR2.DotController.DotIndex.Bleed, WarlockStaticValues.bleedDuration, damageInfo.procCoefficient * 0.2f);
                        }
                    }
                }
            }
        }
    }
}
