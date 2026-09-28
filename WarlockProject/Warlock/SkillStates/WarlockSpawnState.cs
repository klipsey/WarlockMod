using EntityStates;
using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;
using ImpBossSpawnState = EntityStates.ImpBossMonster.SpawnState;

namespace WarlockMod.Warlock.SkillStates
{
    public class WarlockSpawnState : BaseState
    {
        public static float duration = 2f;
        public static float animationPlaybackRate = 1f;
        public static string spawnSoundString = "Play_imp_overlord_spawn";

        private Animator modelAnimator;

        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            if (NetworkServer.active)
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            if (modelAnimator)
            {
                modelAnimator.SetFloat("MetaMagic.playbackRate", animationPlaybackRate);
                modelAnimator.SetBool("creatingMetaMagic", true);
                PlayCrossfade("FullBody, Override", "CreateMetaMagic", 0.05f);
            }
            if (NetworkClient.active && !WwiseIntegrationManager.noAudio &&
                PointSoundManager.EmitSoundLocal(spawnSoundString, characterBody.corePosition) == 0)
                Log.Warning($"Warlock spawn sound '{spawnSoundString}' failed to post.");
            if (ImpBossSpawnState.spawnEffectPrefab)
            {
                EffectManager.SpawnEffect(ImpBossSpawnState.spawnEffectPrefab, new EffectData
                {
                    origin = transform.position
                }, transmit: false);
            }

            var modelTransform = GetModelTransform();
            if (modelTransform && ImpBossSpawnState.destealthMaterial)
            {
                var overlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                overlay.duration = 1f;
                overlay.destroyComponentOnEnd = true;
                overlay.originalMaterial = ImpBossSpawnState.destealthMaterial;
                overlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlay.animateShaderAlpha = true;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            if (modelAnimator) modelAnimator.SetBool("creatingMetaMagic", false);
            base.OnExit();
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
    }
}
