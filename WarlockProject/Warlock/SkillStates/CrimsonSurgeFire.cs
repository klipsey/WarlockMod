using EntityStates;
using EntityStates.Treebot.Weapon;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Linq;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Components;
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
        public int maxShots = 1;
        private int shotCounter;
        private bool empoweredShot;
        public Ray initialAimRay;
        private float fireTimer;
        private Ray aimRay;
        private bool animationStarted;
        private bool IsBlasting => maxShots > 1 && shotCounter < maxShots;

        public GameObject hitEffectPrefab = WarlockAssets.warlockHitImpactEffect;
        public GameObject tracerEffectPrefab = WarlockAssets.warlockTracerEffect;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            this.duration = this.baseDuration / base.attackSpeedStat;
            empoweredShot = warlockController.TryConsumeEmpowerment(WarlockBuffs.warlockEmpoweredM1Buff);
            fireInterval = duration / maxShots;
            fireTimer = fireInterval;
            shotCounter = 1;
            UpdateAnimation();
            aimRay = initialAimRay.direction == Vector3.zero ? GetAimRay() : initialAimRay;
            base.StartAimMode(aimRay, 2f, false);
            Util.PlaySound("Play_imp_overlord_teleport_end", base.gameObject);
            Fire();
        }

        public override void Update()
        {
            base.Update();
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (!modelAnimator || !modelAnimator.isActiveAndEnabled)
            {
                animationStarted = false;
                return;
            }
            modelAnimator.SetBool("isBlasting", IsBlasting);
            if (!CanPlayGestureAnimation(true))
            {
                animationStarted = false;
                return;
            }
            if (animationStarted) return;
            PlayCrossfade("Gesture, Override", "BlastFire", "Blast.playbackRate",
                Mathf.Max(0.01f, fireInterval), 0.05f);
            animationStarted = true;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(maxShots);
            writer.Write(aimRay.origin);
            writer.Write(aimRay.direction);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            maxShots = Mathf.Max(1, reader.ReadInt32());
            initialAimRay = new Ray(reader.ReadVector3(), reader.ReadVector3());
        }

        private void Fire()
        {
            EffectManager.SimpleMuzzleFlash(WarlockAssets.warlockSurgeMuzzleEffect, gameObject, "Muzzle", false);
            if (NetworkServer.active)
            {
                DamageTypeCombo damageType = empoweredShot ? DamageType.WeakOnHit : DamageType.Generic;
                damageType.damageSource = DamageSource.Primary;

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
                bulletAttack.force = 0f;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.tracerEffectPrefab = this.tracerEffectPrefab;
                bulletAttack.muzzleName = "Muzzle";
                bulletAttack.hitEffectPrefab = this.hitEffectPrefab;
                bulletAttack.isCrit = base.RollCrit();
                bulletAttack.HitEffectNormal = false;
                bulletAttack.stopperMask = LayerIndex.world.mask;
                bulletAttack.smartCollision = true;
                bulletAttack.maxDistance = 500f;
                bulletAttack.damageType = damageType;
                if (empoweredShot) bulletAttack.hitCallback = EmpoweredHit;
                bulletAttack.Fire();
            }

            if (isAuthority && !characterMotor.isGrounded)
            {
                float shotKnockbackForce = selfKnockbackForce * Mathf.Pow(0.5f, shotCounter - 1);
                base.characterBody.characterMotor.ApplyForce(-shotKnockbackForce * aimRay.direction, true);
            }
        }

        private static bool EmpoweredHit(BulletAttack attack, ref BulletAttack.BulletHit hit)
        {
            bool pierce = BulletAttack.defaultHitCallback(attack, ref hit);
            var health = hit.hitHurtBox ? hit.hitHurtBox.healthComponent : null;
            if (NetworkServer.active && health && health.alive && health.body &&
                FriendlyFireManager.ShouldDirectHitProceed(health, TeamComponent.GetObjectTeam(attack.owner)))
            {
                var body = health.body;
                var rigidbody = health.GetComponent<Rigidbody>();
                float mass = body.characterMotor ? body.characterMotor.mass : rigidbody ? rigidbody.mass : 1f;
                float suitability = FireSonicBoom.shoveSuitabilityCurve.Evaluate(mass);
                body.RecalculateStats();
                // Use REX's lift and mass resistance, but never pull distant targets inward.
                Vector3 direction = Vector3.ProjectOnPlane(hit.direction, Vector3.up).normalized;
                float speed = Trajectory.CalculateInitialYSpeedForHeight(30f, -body.acceleration);
                Vector3 velocity = direction * speed + Vector3.up * 3f;
                health.TakeDamageForce(velocity * (mass * suitability), alwaysApply: true, disableAirControlUntilCollision: true);
            }
            return pierce;
        }

        public override void OnExit()
        {
            if (modelAnimator) modelAnimator.SetBool("isBlasting", false);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            while (shotCounter < maxShots && base.fixedAge >= fireTimer)
            {
                shotCounter++;
                if (modelAnimator) modelAnimator.SetBool("isBlasting", IsBlasting);
                this.fireTimer += this.fireInterval;
                aimRay = base.GetAimRay();
                base.StartAimMode(aimRay, 2f, false);
                Util.PlaySound("Play_imp_overlord_teleport_end", base.gameObject);
                Fire();
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