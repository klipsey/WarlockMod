using System;
using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using WarlockMod.Warlock.Components;
using Object = UnityEngine.Object;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockAssets
    {
        internal static AssetBundle mainAssetBundle;
        internal static GameObject spawnPrefab;
        internal static GameObject hexChargeEffect;
        internal static GameObject consumeOrb;
        internal static GameObject warlockHitImpactEffect;
        internal static GameObject warlockHexExplodeEffect;
        internal static GameObject bloodExplosionEffect;
        internal static GameObject warlockHexConsume;
        internal static GameObject warlockTracerEffect;
        internal static GameObject telekinesisTracker;
        internal static Material destealthMaterial;
        internal static readonly Color warlockColor = new Color(155f / 255f, 55f / 255f, 55f / 255f);
        internal static readonly Color warlockSpecialRed = new Color(36f / 255f, 22f / 255f, 22f / 255f);

        public static void Init(AssetBundle assetBundle) => mainAssetBundle = assetBundle;

        public static void InitAssets()
        {
            destealthMaterial = Load<Material>("RoR2/Base/Imp/matImpBossDissolve.mat");
            CreateEffects();
            CreateTracker();
            CreateHexBlast();
        }

        private static T Load<T>(string key) where T : Object
        {
            try
            {
                var asset = Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
                if (!asset) throw new InvalidOperationException("Load returned no asset.");
                return asset;
            }
            catch (Exception exception)
            {
                Log.Error($"Required Warlock asset '{key}' ({typeof(T).Name}) could not be loaded: {exception}");
                throw;
            }
        }

        private static GameObject CloneEffect(string key, string name) =>
            Load<GameObject>(key).InstantiateClone(name, false);

        private static void CreateEffects()
        {
            var exposedMaterial = Object.Instantiate(Load<Material>("RoR2/Base/Merc/matMercExposed.mat"));
            exposedMaterial.SetColor("_TintColor", warlockColor);
            warlockHexConsume = CloneEffect("RoR2/Base/Merc/MercExposeConsumeEffect.prefab", "WarlockHexConsume");
            warlockHexConsume.transform.Find("Visual, Consumed/PulseEffect, Ring (1)").GetComponent<ParticleSystemRenderer>().sharedMaterial = exposedMaterial;
            warlockHexConsume.GetComponent<EffectComponent>().soundName = Modules.SoundBanks.HexEvent;
            Object.DestroyImmediate(warlockHexConsume.transform.Find("Visual, Consumed/PulseEffect, Slash").gameObject);
            Modules.Content.CreateAndAddEffectDef(warlockHexConsume);

            warlockTracerEffect = CloneEffect("RoR2/Base/Golem/TracerGolem.prefab", "WarlockTracer");
            var beam = warlockTracerEffect.transform.Find("SmokeBeam");
            var color = beam.GetComponent<ParticleSystem>().colorOverLifetime;
            var gradient = new Gradient();
            gradient.SetKeys(new[]
            {
                new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.black, 0.1f),
                new GradientColorKey(warlockColor, 0.34f), new GradientColorKey(Color.black, 1f)
            }, new[]
            {
                new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0.75f, 1f)
            });
            color.color = gradient;
            beam.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", null);
            Modules.Content.CreateAndAddEffectDef(warlockTracerEffect);

            spawnPrefab = CloneEffect("RoR2/Base/ImpBoss/ImpBossDeathEffect.prefab", "WarlockChargeEffect");
            var effect = spawnPrefab.GetComponent<EffectComponent>();
            effect.applyScale = true;
            effect.positionAtReferencedTransform = true;
            effect.parentToReferencedTransform = true;
            effect.soundName = "";
            spawnPrefab.transform.Find("DashRings").localScale *= 0.75f;
            Object.DestroyImmediate(spawnPrefab.transform.Find("PP").gameObject);
            MakeContinuous(spawnPrefab);
            foreach (var shake in spawnPrefab.GetComponentsInChildren<ShakeEmitter>(true))
                Object.DestroyImmediate(shake);
            Modules.Content.CreateAndAddEffectDef(spawnPrefab);

            var impDustMaterial = Load<Material>("RoR2/Base/Imp/matImpDust.mat");
            var impRamp = impDustMaterial.GetTexture("_RemapTex");
            bloodExplosionEffect = CloneEffect("RoR2/Base/ImpBoss/ImpBossBlink.prefab", "WarlockBloodExplosion");
            foreach (var child in new[] { "LongLifeNoiseTrails, Bright", "Dash, Bright", "Flash, Red" })
                SetParticleTint(bloodExplosionEffect.transform.Find("Particles/" + child).GetComponent<ParticleSystem>(), warlockColor);
            var explosionSphereMaterial = bloodExplosionEffect.transform.Find("Particles/Sphere").GetComponent<ParticleSystemRenderer>().material;
            explosionSphereMaterial.SetTexture("_RemapTex", impRamp);
            explosionSphereMaterial.SetColor("_RimColor", warlockColor);
            Object.DestroyImmediate(bloodExplosionEffect.transform.Find("Particles/Flash, White").gameObject);
            bloodExplosionEffect.GetComponentInChildren<Light>().gameObject.SetActive(false);
            Object.DestroyImmediate(bloodExplosionEffect.transform.Find("PP").gameObject);
            effect = bloodExplosionEffect.GetComponent<EffectComponent>();
            effect.applyScale = true;
            effect.soundName = Modules.SoundBanks.BloodExplosionEvent;
            Modules.Content.CreateAndAddEffectDef(bloodExplosionEffect);

            warlockHitImpactEffect = CloneEffect("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab", "WarlockHitImpact");
            foreach (var shake in warlockHitImpactEffect.GetComponentsInChildren<ShakeEmitter>(true))
                Object.DestroyImmediate(shake);
            warlockHitImpactEffect.GetComponent<OmniEffect>().enabled = false;
            var impactMaterial = Object.Instantiate(Load<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat"));
            impactMaterial.SetColor("_TintColor", warlockColor);
            SetImpactMaterial("Scaled Hitspark 3, Radial (Random Color)", impactMaterial);
            SetImpactMaterial("Flash, Hard", Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterFireCorrupted.mat"));
            SetImpactMaterial("Impact Slash", Load<Material>("RoR2/Base/Imp/matImpSlashImpact.mat"));
            SetImpactMaterial("ScaledSmokeRing, Mesh", Load<Material>("RoR2/Base/Imp/matImpDust.mat"));
            SetImpactMaterial("Scaled Hitspark 2 (Random Color)/Scaled Hitspark 4, Directional (Random Color) (1)", Load<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat"));
            SetImpactMaterial("Scaled Hitspark 2 (Random Color)", Load<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat"));
            warlockHitImpactEffect.transform.Find("Scaled Hitspark 3, Radial (Random Color)").localScale = Vector3.one * 1.5f;
            warlockHitImpactEffect.transform.Find("Flash, Hard").localScale = Vector3.one * 1.5f;
            warlockHitImpactEffect.transform.Find("ScaledSmokeRing, Mesh").localScale = Vector3.one * 3f;
            warlockHitImpactEffect.transform.Find("Scaled Hitspark 2 (Random Color)").localScale = new Vector3(1f, 1f, 3f);
            foreach (Transform child in warlockHitImpactEffect.transform)
                if (child.GetSiblingIndex() > 0) child.gameObject.SetActive(true);
            warlockHitImpactEffect.transform.localScale = Vector3.one * 1.5f;
            Modules.Content.CreateAndAddEffectDef(warlockHitImpactEffect);

            consumeOrb = CloneEffect("RoR2/Base/Infusion/InfusionOrbEffect.prefab", "WarlockConsumeOrb");
            var trail = consumeOrb.transform.Find("TrailParent/Trail").GetComponent<TrailRenderer>();
            trail.widthMultiplier = 0.35f;
            var orbTrailMaterial = Object.Instantiate(Load<Material>("RoR2/Base/Infusion/matInfusionTrail.mat"));
            orbTrailMaterial.SetTexture("_RemapTex", impRamp);
            orbTrailMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            orbTrailMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            orbTrailMaterial.SetFloat("_InternalSimpleBlendMode", 1f);
            orbTrailMaterial.SetInt("_ZWrite", 0);
            trail.sharedMaterial = orbTrailMaterial;
            var core = consumeOrb.transform.Find("VFX/Core").GetComponent<ParticleSystem>();
            core.GetComponent<ParticleSystemRenderer>().sharedMaterial = Load<Material>("RoR2/Base/Imp/matImpPortalEffect.mat");
            SetParticleTint(core, Color.white);
            consumeOrb.transform.Find("VFX").localScale = Vector3.one * 0.5f;
            consumeOrb.transform.Find("VFX/Core").localScale = Vector3.one * 4.5f;
            var pulse = consumeOrb.transform.Find("VFX/PulseGlow").GetComponent<ParticleSystem>();
            var pulseMaterial = Object.Instantiate(Load<Material>("RoR2/Base/Common/VFX/matOmniRing2Generic.mat"));
            pulseMaterial.SetTexture("_RemapTex", impRamp);
            pulse.GetComponent<ParticleSystemRenderer>().sharedMaterial = pulseMaterial;
            SetParticleTint(pulse, Color.white);
            hexChargeEffect = consumeOrb.transform.Find("VFX").gameObject.InstantiateClone("WarlockHexCharge", false);
            foreach (var scaleCurve in hexChargeEffect.GetComponentsInChildren<ObjectScaleCurve>(true))
                Object.DestroyImmediate(scaleCurve);
            MakeContinuous(hexChargeEffect);

            var arrivalEffect = CloneEffect("RoR2/Base/Infusion/InfusionOrbFlash.prefab", "WarlockConsumeOrbArrival");
            foreach (var particles in arrivalEffect.GetComponentsInChildren<ParticleSystem>(true))
            {
                particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = impDustMaterial;
                SetParticleTint(particles, Color.white);
            }
            Modules.Content.CreateAndAddEffectDef(arrivalEffect);
            var orb = consumeOrb.GetComponent<OrbEffect>();
            if (orb.onArrival.GetPersistentMethodName(0) != nameof(OrbEffect.InstantiateEffectCopyRotation))
                throw new InvalidOperationException("Infusion orb arrival callback changed; cannot replace its blood flash safely.");
            orb.onArrival.SetPersistentListenerState(0, UnityEventCallState.Off);
            orb.endEffect = arrivalEffect;
            orb.endEffectCopiesRotation = true;
            Modules.Content.CreateAndAddEffectDef(consumeOrb);
        }

        private static void MakeContinuous(GameObject effect)
        {
            foreach (var timer in effect.GetComponentsInChildren<DestroyOnTimer>(true)) Object.DestroyImmediate(timer);
            foreach (var end in effect.GetComponentsInChildren<DestroyOnParticleEnd>(true)) Object.DestroyImmediate(end);
            foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                main.loop = true;
                main.stopAction = ParticleSystemStopAction.None;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }
        }

        internal static GameObject CreateChargeEffect(GameObject prefab, Transform muzzle, Vector3 aimDirection)
        {
            if (!muzzle)
            {
                Log.Error("Cannot attach Warlock charge effect: Muzzle is missing.");
                return null;
            }
            var effect = Object.Instantiate(prefab, muzzle.position, Util.QuaternionSafeLookRotation(aimDirection));
            var component = effect.GetComponent<EffectComponent>();
            if (component) component.noEffectData = true;
            effect.transform.localScale = Vector3.one * 0.2f;
            effect.transform.SetParent(muzzle, true);
            return effect;
        }

        internal static void StopChargeEffect(GameObject effect)
        {
            if (!effect) return;
            foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>(true))
                particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Object.Destroy(effect, 0.15f);
        }

        private static void SetParticleTint(ParticleSystem particles, Color tint)
        {
            var main = particles.main;
            tint.a = main.startColor.color.a;
            main.startColor = tint;
            var colorOverLifetime = particles.colorOverLifetime;
            if (!colorOverLifetime.enabled) return;
            var gradient = colorOverLifetime.color.gradient;
            var keys = gradient.colorKeys;
            for (int i = 0; i < keys.Length; i++)
                keys[i].color = Color.white * keys[i].color.maxColorComponent;
            gradient.SetKeys(keys, gradient.alphaKeys);
            colorOverLifetime.color = gradient;
        }

        private static void SetImpactMaterial(string child, Material material)
        {
            var transform = warlockHitImpactEffect.transform.Find(child);
            transform.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            transform.gameObject.SetActive(true);
        }

        private static void CreateTracker()
        {
            telekinesisTracker = CloneEffect("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab", "WarlockTracker");
            var core = telekinesisTracker.transform.Find("Core Pip").GetComponent<SpriteRenderer>();
            core.sharedMaterial = Load<Material>("RoR2/Base/UI/SpecialUIMaterials/matUIOverbrighten2x.mat");
            core.sprite = mainAssetBundle.LoadAsset<Sprite>("Grab");
            if (!core.sprite) throw new InvalidOperationException("Missing required Warlock tracker sprite 'Grab'.");
            telekinesisTracker.transform.Find("Holder").gameObject.SetActive(false);
            var darkCore = telekinesisTracker.transform.Find("Core, Dark").GetComponent<SpriteRenderer>();
            darkCore.sprite = Load<Sprite>("RoR2/Base/UI/texCrosshair2.png");
            darkCore.color = warlockColor;
        }

        private static void CreateHexBlast()
        {
            warlockHexExplodeEffect = CloneEffect("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplodeDelay.prefab", "WarlockHexBlast");
            var original = warlockHexExplodeEffect.GetComponent<DelayBlast>();
            var blast = warlockHexExplodeEffect.AddComponent<DelayBlastWarlock>();
            blast.explosionEffect = original.explosionEffect;
            blast.timerStagger = original.timerStagger;
            blast.procCoefficient = WarlockConfig.EmpoweredHexProc;
            Object.DestroyImmediate(original);
            Object.DestroyImmediate(warlockHexExplodeEffect.GetComponent<NetworkIdentity>());
        }
    }
}
