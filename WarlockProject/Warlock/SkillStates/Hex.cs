using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using WarlockMod.Modules.BaseStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.GlobalSkills.LunarNeedle;
using UnityEngine.Networking;
using WarlockMod.Warlock.Content;
using WarlockMod.Warlock.Components;

namespace WarlockMod.Warlock.SkillStates
{
    public class Hex : BaseWarlockSkillState
    {
        private float baseDuration = 0.5f;

        private float duration;

        private WarlockTracker tracker;

        private HurtBox victim;

        private CharacterBody victimBody;

        private CameraTargetParams.AimRequest aimRequest;

        public GameObject markedPrefab = WarlockAssets.warlockHexConsume;
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            tracker = this.GetComponent<WarlockTracker>();
            if (tracker)
            {
                victim = tracker.GetTrackingTarget();
                if (victim)
                {
                    victimBody = victim.healthComponent.body;
                    if (base.cameraTargetParams)
                    {
                        aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
                    }
                    StartAimMode(duration);
                    PlayAnimation("Gesture, Override", "Point", "Swing.playbackRate", duration * 1.5f);
                    EffectManager.SpawnEffect(markedPrefab, new EffectData
                    {
                        origin = victimBody.corePosition,
                        scale = 1.5f
                    }, transmit: true);

                    if (NetworkServer.active)
                    {
                        if (!this.characterBody.HasBuff(WarlockBuffs.warlockEmpoweredM2Buff)) victimBody.AddTimedBuff(WarlockBuffs.warlockHexxedDebuff, WarlockStaticValues.hexDuration);
                        else
                        {
                            victimBody.AddTimedBuff(WarlockBuffs.warlockHexxedEmpoweredDebuff, WarlockStaticValues.hexDuration);
                            this.characterBody.RemoveBuff(WarlockBuffs.warlockEmpoweredM2Buff);
                        }
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            aimRequest?.Dispose();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}