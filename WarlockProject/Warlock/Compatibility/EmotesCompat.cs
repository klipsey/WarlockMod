using System;
using System.Runtime.CompilerServices;
using EmotesAPI;
using RoR2;
using UnityEngine;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.Compatibility
{
    internal static class EmotesCompat
    {
        private static bool initialized;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Initialize()
        {
            if (initialized) return;
            GameObject skeleton = null;
            try
            {
                var source = WarlockAssets.mainAssetBundle.LoadAsset<GameObject>("warlock_emoteskeleton");
                if (!source) throw new InvalidOperationException("Missing warlock_emoteskeleton in the Warlock bundle.");
                var animator = source.GetComponent<Animator>();
                if (!animator || !animator.avatar || !animator.avatar.isValid || !animator.avatar.isHuman)
                    throw new InvalidOperationException("Warlock emote skeleton needs a valid humanoid Avatar and root Animator.");

                skeleton = UnityEngine.Object.Instantiate(source);
                skeleton.name = "WarlockEmoteSkeleton";
                foreach (var renderer in skeleton.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = false;
                foreach (var bone in skeleton.GetComponentsInChildren<Transform>(true))
                    bone.gameObject.SetActive(true);
                var childLocator = WarlockSurvivor.characterPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>();
                var bodyRenderer = childLocator.FindChild("Model").GetComponent<SkinnedMeshRenderer>();
                var emoteRenderers = skeleton.GetComponentsInChildren<SkinnedMeshRenderer>();
                int bodyMeshIndex = Array.FindIndex(emoteRenderers, renderer =>
                    bodyRenderer && bodyRenderer.sharedMesh && renderer.sharedMesh &&
                    renderer.name == bodyRenderer.name);
                if (bodyMeshIndex < 0)
                    throw new InvalidOperationException(
                        $"Warlock emote body renderer did not match. Body: {(bodyRenderer ? bodyRenderer.name : "missing")}, " +
                        $"mesh: {(bodyRenderer && bodyRenderer.sharedMesh ? bodyRenderer.sharedMesh.name : "missing")}. Skeleton: " +
                        string.Join(", ", Array.ConvertAll(emoteRenderers, renderer =>
                            $"{renderer.name}/{(renderer.sharedMesh ? renderer.sharedMesh.name : "no mesh")}/{renderer.bones.Length} bones")));
                var emoteBodyRenderer = emoteRenderers[bodyMeshIndex];
                if (Array.Exists(emoteBodyRenderer.bones, bone => !bone))
                    throw new InvalidOperationException("Warlock emote bone mapping is stale. Refresh the emote skeleton in Unity and rebuild the bundle.");
                var daggerRoot = Array.Find(skeleton.GetComponentsInChildren<Transform>(true), bone => bone.name == "dagger.x");
                if (!daggerRoot)
                    throw new InvalidOperationException("Warlock emote skeleton is missing its dagger root.");
                emoteBodyRenderer.bones = Array.FindAll(emoteBodyRenderer.bones,
                    bone => bone != daggerRoot && !bone.IsChildOf(daggerRoot));
                CustomEmotesAPI.ImportArmature(WarlockSurvivor.characterPrefab, skeleton, bodyMeshIndex);
                var mapper = skeleton.GetComponent<BoneMapper>();
                if (!mapper || !mapper.smr1 || !mapper.smr2 || !mapper.a2)
                    throw new InvalidOperationException("CustomEmotesAPI did not initialize Warlock's bone mapper.");
                mapper.a2.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                mapper.revertTransform = true;
                mapper.smr1 = emoteBodyRenderer;
                mapper.smr2 = bodyRenderer;
                var visibility = skeleton.AddComponent<EmotePropVisibility>();
                visibility.book = childLocator.FindChild("BookModel").GetComponent<Renderer>();
                visibility.dagger = childLocator.FindChild("DaggerModel").GetComponent<Renderer>();
                CustomEmotesAPI.animChanged += OnAnimationChanged;
                initialized = true;
            }
            catch (Exception exception)
            {
                if (skeleton) UnityEngine.Object.Destroy(skeleton);
                Log.Error($"Warlock emote integration failed; gameplay remains available. {exception}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void OnAnimationChanged(string animation, BoneMapper mapper)
        {
            if (!mapper) return;
            var visibility = mapper.GetComponent<EmotePropVisibility>();
            if (visibility) visibility.SetEmoting(!string.IsNullOrEmpty(animation) && animation != "none");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Shutdown()
        {
            if (!initialized) return;
            CustomEmotesAPI.animChanged -= OnAnimationChanged;
            foreach (var visibility in UnityEngine.Object.FindObjectsOfType<EmotePropVisibility>())
                visibility.SetEmoting(false);
            initialized = false;
        }
    }
}
