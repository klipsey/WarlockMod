using EntityStates;
using RoR2;
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
        private bool hasPlayedSpawnSound;

        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            if (NetworkServer.active)
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            if (modelAnimator)
            {
                modelAnimator.SetFloat("MetaMagic.playbackRate", Mathf.Max(0.01f, animationPlaybackRate));
                modelAnimator.SetBool("creatingMetaMagic", true);
                PlayCrossfade("FullBody, Override", "CreateMetaMagic", 0.05f);
            }
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
            if (!hasPlayedSpawnSound && fixedAge > 0f)
            {
                hasPlayedSpawnSound = true;
                if (NetworkClient.active && !WwiseIntegrationManager.noAudio &&
                    Util.PlaySound(spawnSoundString, gameObject) == 0)
                    Log.Warning($"Warlock spawn sound '{spawnSoundString}' failed to post.");
            }
            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            if (modelAnimator) modelAnimator.SetBool("creatingMetaMagic", false);
            base.OnExit();
            if (NetworkServer.active && characterBody)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                if (!outer.destroying)
                    characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
    }
}
