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
        internal static GameObject warlockSurgeMuzzleEffect;
        internal static GameObject telekinesisTracker;
        internal static GameObject dashDestinationPreview;
        internal static GameObject dashBlinkEffect;
        internal static GameObject dashBlinkDestinationEffect;
        internal static Material destealthMaterial;
        internal static readonly Color warlockColor = new Color(155f / 255f, 55f / 255f, 55f / 255f);
        internal static readonly Color warlockSpecialRed = new Color(36f / 255f, 22f / 255f, 22f / 255f);

        public static void Init(AssetBundle assetBundle) => mainAssetBundle = assetBundle;

        internal static Sprite CreateUnlockIcon()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return null;

            var portrait = mainAssetBundle.LoadAsset<Texture>("texWarlockIcon");
            if (!portrait) throw new InvalidOperationException("Missing required Warlock portrait 'texWarlockIcon'.");
            var background = Load<Texture2D>("RoR2/Base/Common/texSurvivorBGIcon.png");
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "texWarlockUnlockIcon",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var previousTarget = RenderTexture.active;
            bool previousSrgbWrite = GL.sRGBWrite;
            var target = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            bool completed = false;
            try
            {
                GL.sRGBWrite = false;
                Graphics.Blit(background, target);
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                var backgroundPixels = texture.GetPixels();

                Graphics.Blit(portrait, target);
                texture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                var portraitPixels = texture.GetPixels();
                for (int i = 0; i < backgroundPixels.Length; i++)
                {
                    var color = Color.Lerp(backgroundPixels[i], portraitPixels[i], portraitPixels[i].a);
                    color.a = portraitPixels[i].a + backgroundPixels[i].a * (1f - portraitPixels[i].a);
                    backgroundPixels[i] = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.gamma : color;
                }
                texture.SetPixels(backgroundPixels);
                texture.Apply(false, true);
                var icon = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                    100f, 0, SpriteMeshType.FullRect);
                icon.name = texture.name;
                completed = true;
                return icon;
            }
            finally
            {
                RenderTexture.active = previousTarget;
                GL.sRGBWrite = previousSrgbWrite;
                RenderTexture.ReleaseTemporary(target);
                if (!completed) Object.Destroy(texture);
            }
        }

        public static void InitAssets()
        {
            destealthMaterial = Load<Material>("RoR2/Base/Imp/matImpBossDissolve.mat");
            CreateEffects();
            CreateTracker();
            CreateHexBlast();
            CreateHexDebuffEffects();
            CreateDashPreview();
        }

        private static void CreateHexDebuffEffects()
        {
            CreateHexDebuffEffect("WarlockHexDebuffEffect", 1f, Color.black, Color.black,
                body => body.HasBuff(WarlockBuffs.warlockHexxedDebuff));
            CreateHexDebuffEffect("WarlockEmpoweredHexDebuffEffect", 1.2f, Color.black, warlockColor,
                body => body.HasBuff(WarlockBuffs.warlockHexxedEmpoweredDebuff));
            CreateHexDebuffEffect("WarlockMetaMagicHexDebuffEffect", 1.4f, warlockColor, warlockColor,
                body => body.HasBuff(WarlockBuffs.warlockHexxedMetaMagicDebuff));
        }

        private static void CreateHexDebuffEffect(string name, float radiusMultiplier, Color shadowColor,
            Color highlightColor, TempVisualEffectAPI.EffectCondition condition)
        {
            var effect = CloneEffect("RoR2/Base/DeathMark/DeathMarkEffect.prefab", name);
            foreach (var sound in effect.GetComponentsInChildren<AkEvent>(true))
                Object.DestroyImmediate(sound);

            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
            {
                var sourceRamp = Load<Material>("RoR2/Base/DeathMark/matDeathMarkFire.mat").GetTexture("_RemapTex");
                var ramp = CreateHexDebuffRamp(sourceRamp, name + "Ramp", shadowColor, highlightColor);
                foreach (var renderer in effect.GetComponentsInChildren<Renderer>(true))
                {
                    var material = Object.Instantiate(renderer.sharedMaterial);
                    material.SetTexture("_RemapTex", ramp);
                    renderer.sharedMaterial = material;
                }
            }

            if (!TempVisualEffectAPI.AddTemporaryVisualEffect(effect,
                body => body.radius * radiusMultiplier,
                body => body.healthComponent && body.healthComponent.alive && condition(body)))
                throw new InvalidOperationException($"Could not register Warlock's Hex visual effect '{name}'.");
        }

        private static Texture2D CreateHexDebuffRamp(Texture source, string name, Color shadowColor, Color highlightColor)
        {
            if (!source) throw new InvalidOperationException("Death Mark's color ramp is missing.");
            var ramp = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var previousTarget = RenderTexture.active;
            bool previousSrgbWrite = GL.sRGBWrite;
            var target = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            bool completed = false;
            try
            {
                GL.sRGBWrite = false;
                Graphics.Blit(source, target);
                RenderTexture.active = target;
                ramp.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                {
                    shadowColor = shadowColor.linear;
                    highlightColor = highlightColor.linear;
                }
                var pixels = ramp.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    var color = Color.Lerp(shadowColor, highlightColor, pixels[i].maxColorComponent);
                    color.a = pixels[i].a;
                    pixels[i] = color;
                }
                ramp.SetPixels(pixels);
                ramp.Apply(false, true);
                completed = true;
                return ramp;
            }
            finally
            {
                RenderTexture.active = previousTarget;
                GL.sRGBWrite = previousSrgbWrite;
                RenderTexture.ReleaseTemporary(target);
                if (!completed) Object.Destroy(ramp);
            }
        }

        private static void CreateDashPreview()
        {
            dashBlinkEffect = Load<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab");
            dashBlinkDestinationEffect = Load<GameObject>("RoR2/Base/Imp/ImpBossBlinkDestination.prefab");
            dashDestinationPreview = CloneEffect("RoR2/Base/Common/TeamAreaIndicator, FullSphere.prefab", "WarlockDashPreview");
            Object.DestroyImmediate(dashDestinationPreview.GetComponent<TeamAreaIndicator>());
            var material = Load<Material>("RoR2/Base/Common/matTeamAreaIndicatorFullMonster.mat");
            foreach (var renderer in dashDestinationPreview.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = material;
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
            warlockTracerEffect = CreateSurgeEffect("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamTracer.prefab", "WarlockTracer", impRamp);
            warlockSurgeMuzzleEffect = CreateSurgeEffect("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab", "WarlockSurgeMuzzle", impRamp);
            Object.DestroyImmediate(warlockSurgeMuzzleEffect.transform.Find("Ring").gameObject);
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
            warlockHitImpactEffect.GetComponent<EffectComponent>().soundName = "";
            foreach (var shake in warlockHitImpactEffect.GetComponentsInChildren<ShakeEmitter>(true))
                Object.DestroyImmediate(shake);
            warlockHitImpactEffect.GetComponent<OmniEffect>().enabled = false;
            SetImpactMaterial("Scaled Hitspark 3, Radial (Random Color)", Load<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat"));
            SetImpactMaterial("Flash, Hard", Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterFireCorrupted.mat"));
            SetImpactMaterial("Impact Slash", Load<Material>("RoR2/Base/Imp/matImpSlashImpact.mat"));
            SetImpactMaterial("ScaledSmokeRing, Mesh", Load<Material>("RoR2/Base/Imp/matImpDust.mat"));
            SetImpactMaterial("Scaled Hitspark 2 (Random Color)/Scaled Hitspark 4, Directional (Random Color) (1)", Load<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat"));
            SetImpactMaterial("Scaled Hitspark 2 (Random Color)", Load<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat"));
            foreach (var particles in warlockHitImpactEffect.GetComponentsInChildren<ParticleSystem>(true))
            {
                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                var material = Object.Instantiate(renderer.sharedMaterial);
                material.SetTexture("_RemapTex", impRamp);
                material.SetColor("_TintColor", Color.white);
                renderer.sharedMaterial = material;
                SetParticleTint(particles, Color.white);
            }
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

        private static GameObject CreateSurgeEffect(string key, string name, Texture ramp)
        {
            const float visualScale = 3f;
            var effect = CloneEffect(key, name);
            effect.GetComponent<EffectComponent>().soundName = "";
            foreach (var renderer in effect.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!materials[i]) continue;
                    var material = Object.Instantiate(materials[i]);
                    material.SetTexture("_RemapTex", ramp);
                    material.SetColor("_TintColor", Color.white);
                    material.SetFloat("_Boost", 1f);
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_InternalSimpleBlendMode", 1f);
                    material.SetInt("_ZWrite", 0);
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
            // Scale thickness without moving the tracer's world-space endpoints.
            foreach (var line in effect.GetComponentsInChildren<LineRenderer>(true))
                line.widthMultiplier *= visualScale;
            foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                if (main.startSize3D)
                {
                    main.startSizeXMultiplier *= visualScale;
                    main.startSizeYMultiplier *= visualScale;
                    main.startSizeZMultiplier *= visualScale;
                }
                else
                {
                    main.startSizeMultiplier *= visualScale;
                }
                SetParticleTint(particles, Color.white);
            }
            foreach (var light in effect.GetComponentsInChildren<Light>(true))
                light.color = warlockColor;
            Modules.Content.CreateAndAddEffectDef(effect);
            return effect;
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
            Object.DestroyImmediate(original);
            Object.DestroyImmediate(warlockHexExplodeEffect.GetComponent<NetworkIdentity>());
        }
    }
}
