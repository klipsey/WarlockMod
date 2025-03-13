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
        public float baseDuration = 0.5f;
        public float selfKnockbackForce = 2250f;
        public float damageCoefficient = WarlockStaticValues.crimsonSurgeDamageCoefficient;
        private float duration;
        private float fireInterval;
        private float maxShots;
        private float shotCounter;
        private float fireTimer;
        private Ray aimRay;
        private bool hasFiredFirstShot;

        private static List<CharacterBody> _enemiesHit = new List<CharacterBody>();

        public GameObject hitEffectPrefab = WarlockAssets.warlockHitImpactEffect;
        public GameObject tracerEffectPrefab = WarlockAssets.warlockTracerEffect;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            this.duration = this.baseDuration / base.attackSpeedStat;
            if (this.primaryEmpowered)
            {
                this.duration *= 0.85f;
            }
            if (this.characterBody.HasBuff(WarlockBuffs.warlockMetaMagicBuff))
            {
                this.maxShots = this.characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff) + 1;
                this.fireInterval = this.duration / this.maxShots;
                this.fireTimer = this.fireInterval;
                this.characterBody.SetBuffCount(WarlockBuffs.warlockMetaMagicBuff.buffIndex, 0);
            }
            else
            {
                fireTimer = baseDuration;
            }
            aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);
            //base.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
            Util.PlaySound("Play_imp_overlord_teleport_end", base.gameObject);
            base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);
            if (FireLaser.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireLaser.effectPrefab, base.gameObject, "EldritchMuzzle", false);
            }

            if (base.isAuthority)
            {
                this.Fire();
            }
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
            bulletAttack.procCoefficient = 1f;
            bulletAttack.damage = damageCoefficient * damageStat;
            bulletAttack.force = selfKnockbackForce;
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.tracerEffectPrefab = this.tracerEffectPrefab;
            bulletAttack.muzzleName = "EldritchMuzzle";
            bulletAttack.hitEffectPrefab = this.hitEffectPrefab;
            bulletAttack.isCrit = base.RollCrit();
            bulletAttack.HitEffectNormal = false;
            bulletAttack.stopperMask = LayerIndex.world.mask;
            bulletAttack.smartCollision = true;
            bulletAttack.maxDistance = 500f;
            bulletAttack.damageType |= DamageTypeCombo.GenericPrimary;
            _enemiesHit.Clear();
            bulletAttack.Fire();

            if (!characterMotor.isGrounded && !hasFiredFirstShot)
            {
                hasFiredFirstShot = true;
                base.characterBody.characterMotor.ApplyForce((0f - selfKnockbackForce) * aimRay.direction, true);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.fireTimer && this.fireTimer < this.duration)
            {
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