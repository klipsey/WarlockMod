using EntityStates;
using RoR2;
using UnityEngine;
using EntityStates.GolemMonster;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Linq;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class CrimsonSurgeFire : BaseWarlockSkillState
    {
        public float baseDuration = 1f;
        public float selfKnockbackForce = 2250f;
        public float damageCoefficient = WarlockConfig.CrimsonSurgeDamage;
        private float duration;
        private float fireInterval;
        private int maxShots = 1;
        private int shotCounter;
        private bool empoweredShot;
        private float fireTimer;
        private Ray aimRay;

        public GameObject hitEffectPrefab = WarlockAssets.warlockHitImpactEffect;
        public GameObject tracerEffectPrefab = WarlockAssets.warlockTracerEffect;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            if (isAuthority)
            {
                maxShots = characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff) + 1;
                empoweredShot = primaryEmpowered;
            }
            this.duration = this.baseDuration / base.attackSpeedStat;
            if (empoweredShot)
            {
                this.duration *= 0.85f;
            }
            if (NetworkServer.active && maxShots > 1)
            {
                characterBody.SetBuffCount(WarlockBuffs.warlockMetaMagicBuff.buffIndex,
                    Mathf.Max(0, characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff) - (maxShots - 1)));
            }
            fireInterval = duration / maxShots;
            fireTimer = fireInterval;
            shotCounter = 1;
            aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);
            //base.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
            Util.PlaySound("Play_imp_overlord_teleport_end", base.gameObject);
            base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);
            if (FireLaser.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireLaser.effectPrefab, base.gameObject, "Muzzle", false);
            }

            if (base.isAuthority)
            {
                this.Fire();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(maxShots);
            writer.Write(empoweredShot);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            maxShots = Mathf.Max(1, reader.ReadInt32());
            empoweredShot = reader.ReadBoolean();
        }

        private void Fire()
        {
            BulletAttack bulletAttack = new BulletAttack();
            bulletAttack.owner = base.gameObject;
            bulletAttack.weapon = base.gameObject;
            bulletAttack.origin = aimRay.origin;
            bulletAttack.aimVector = aimRay.direction;
            bulletAttack.minSpread = 0f;
            bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
            bulletAttack.radius = 2f;
            bulletAttack.bulletCount = 1;
            bulletAttack.procCoefficient = WarlockConfig.CrimsonSurgeProc;
            bulletAttack.damage = damageCoefficient * damageStat;
            bulletAttack.force = selfKnockbackForce;
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.tracerEffectPrefab = this.tracerEffectPrefab;
            bulletAttack.muzzleName = "Muzzle";
            bulletAttack.hitEffectPrefab = this.hitEffectPrefab;
            bulletAttack.isCrit = base.RollCrit();
            bulletAttack.HitEffectNormal = false;
            bulletAttack.stopperMask = LayerIndex.world.mask;
            bulletAttack.smartCollision = true;
            bulletAttack.maxDistance = 500f;
            bulletAttack.damageType |= DamageType.Generic;
            bulletAttack.Fire();

            if (!characterMotor.isGrounded)
            {
                float shotKnockbackForce = selfKnockbackForce * Mathf.Pow(0.5f, shotCounter - 1);
                base.characterBody.characterMotor.ApplyForce(-shotKnockbackForce * aimRay.direction, true);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            while (shotCounter < maxShots && base.fixedAge >= fireTimer)
            {
                shotCounter++;
                this.fireTimer += this.fireInterval;
                aimRay = base.GetAimRay();
                base.StartAimMode(aimRay, 2f, false);
                Util.PlaySound("Play_imp_overlord_teleport_end", base.gameObject);
                base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);
                if (base.isAuthority)
                {
                    this.Fire();
                }
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}