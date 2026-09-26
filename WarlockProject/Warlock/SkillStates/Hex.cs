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
            if (tracker && isAuthority)
            {
                victim = tracker.GetTrackingTarget();
            }
                if (victim && victim.healthComponent && victim.healthComponent.alive)
                {
                    victimBody = victim.healthComponent.body;
                    if (!victimBody) return;
                    if (NetworkServer.active && (victimBody.teamComponent.teamIndex == characterBody.teamComponent.teamIndex ||
                        Vector3.Distance(inputBank.aimOrigin, victimBody.corePosition) > tracker.maxTrackingDistance + victimBody.radius))
                        return;
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
                    }, transmit: false);

                    if (NetworkServer.active)
                    {
                        if(this.characterBody.HasBuff(WarlockBuffs.warlockMetaMagicBuff))
                        {
                            for(int i = 0; i < this.characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff); i++) 
                            {
                                this.victimBody.AddTimedBuff(WarlockBuffs.warlockHexxedMetaMagicDebuff, WarlockConfig.HexDuration);
                            }
                            this.characterBody.SetBuffCount(WarlockBuffs.warlockMetaMagicBuff.buffIndex, 0);
                        }

                        if (!this.characterBody.HasBuff(WarlockBuffs.warlockEmpoweredM2Buff))
                        {
                            victimBody.AddTimedBuff(WarlockBuffs.warlockHexxedDebuff, WarlockConfig.HexDuration);
                        }
                        else
                        {
                            victimBody.AddTimedBuff(WarlockBuffs.warlockHexxedEmpoweredDebuff, WarlockConfig.HexDuration);
                            this.characterBody.RemoveBuff(WarlockBuffs.warlockEmpoweredM2Buff);
                        }
                    }
                }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(HurtBoxReference.FromHurtBox(victim));
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            victim = reader.ReadHurtBoxReference().ResolveHurtBox();
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