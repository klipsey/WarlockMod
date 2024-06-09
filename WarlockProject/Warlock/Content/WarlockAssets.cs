using RoR2;
using UnityEngine;
using WarlockMod.Modules;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using ThreeEyedGames;
using WarlockMod.Warlock.Components;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockAssets
    {
        //AssetBundle
        internal static AssetBundle mainAssetBundle;

        //Shader
        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        //Effects

        internal static GameObject spawnPrefab;
        internal static GameObject spawnPrefab2;

        internal static GameObject parrySlashEffect;
        internal static GameObject parrySlashEffect2;

        internal static GameObject wideSlashEffect;
        internal static GameObject wideSlashEffect2;

        internal static GameObject uppercutEffect;
        internal static GameObject uppercutEffect2;

        internal static GameObject clipSlashEffect;
        internal static GameObject clipSlashEffect2;

        internal static GameObject pickupScissorEffect;
        internal static GameObject pickupScissorEffect2;

        internal static GameObject scissorsSlashEffect;
        internal static GameObject scissorsSlashEffect2;

        internal static GameObject scissorsSlashComboEffect;
        internal static GameObject scissorsSlashComboEffect2;

        internal static GameObject clawSlashEffect;
        internal static GameObject clawSlashEffect2;
        internal static GameObject clawSlashComboEffect;
        internal static GameObject clawSlashComboEffect2;

        internal static GameObject blinkEffect;
        internal static GameObject blinkEffect2;

        internal static GameObject impDashEffect;
        internal static GameObject impDashEffect2;

        internal static GameObject smallBlinkEffect;
        internal static GameObject smallBlinkEffect2;

        internal static GameObject insatiableEndEffect;
        internal static GameObject insatiableEndEffect2;

        internal static GameObject scissorsHitImpactEffect;
        internal static GameObject scissorsHitImpactEffect2;

        internal static GameObject slamEffect;
        internal static GameObject slamEffect2;

        internal static GameObject genericImpactExplosionEffect;
        internal static GameObject genericImpactExplosionEffect2;

        internal static GameObject bloodSplatterEffect;
        internal static GameObject bloodSplatterEffect2;

        internal static GameObject sewnCdEffect;
        internal static GameObject sewnEffect;
        internal static GameObject sewnEffect2;

        internal static GameObject trailEffect;
        internal static GameObject trailEffectHands;
        internal static GameObject trailEffect2;
        internal static GameObject trailEffectHands2;

        internal static GameObject bloodExplosionEffect;
        internal static GameObject bloodExplosionEffect2;
        internal static GameObject bloodSpurtEffect;
        internal static GameObject bloodSpurtEffect2;

        internal static GameObject warlockHexConsume;

        internal static GameObject warlockTracerEffect;

        //Misc Prefabs
        internal static TeamAreaIndicator WarlockTeamAreaIndicator;
        internal static TeamAreaIndicator WarlockTeamAreaIndicator2;

        internal static GameObject telekinesisTracker;
        internal static GameObject telekinesisCdTracker;

        internal static GameObject chainToHeart;
        internal static GameObject heartPrefab;

        internal static GameObject chainToHeart2;
        internal static GameObject heartPrefab2;
        //Overlay Effects
        internal static GameObject stitchEffect;

        //Materials
        internal static Material destealthMaterial;
        internal static Material destealthMaterial2;
        internal static Material insatiableOverlayMat;
        internal static Material insatiableOverlayMat2;
        internal static Material parryMat;
        internal static Material commandoMat;
        internal static Material mercMat;

        //Networked Hit Sounds
        internal static NetworkSoundEventDef scissorsHitSoundEvent;
        internal static NetworkSoundEventDef parrySuccessSoundEvent;

        //Projectiles
        internal static GameObject needlePrefab;
        internal static GameObject needlePrefab2;

        internal static GameObject needleButcheredPrefab;
        internal static GameObject needleButcheredPrefab2;

        internal static GameObject scissorRPrefab;
        internal static GameObject scissorLPrefab;

        internal static GameObject scissorRPrefab2;
        internal static GameObject scissorLPrefab2;

        //Colors
        internal static Color warlockColor = new Color(84f / 255f, 0f / 255f, 11f / 255f);
        internal static Color warlockSpecialRed = new Color(36f / 255f, 22f / 255f, 22f / 255f);
        internal static Color warlockSecondaryColor = Color.black;

        //Crosshair
        public static void Init(AssetBundle assetBundle)
        {
            mainAssetBundle = assetBundle;
        }
        public static void InitAssets()
        {
            CreateMaterials();

            CreateModels();

            CreateEffects();

            CreateSounds();

            CreateProjectiles();

            CreateUI();
        }

        private static void CleanChildren(Transform startingTrans)
        {
            for (int num = startingTrans.childCount - 1; num >= 0; num--)
            {
                if (startingTrans.GetChild(num).childCount > 0)
                {
                    CleanChildren(startingTrans.GetChild(num));
                }
                Object.DestroyImmediate(startingTrans.GetChild(num).gameObject);
            }
        }

        private static void CreateMaterials()
        {
            insatiableOverlayMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorCorruptOverlay.mat").WaitForCompletion());

            insatiableOverlayMat2 = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorCorruptOverlay.mat").WaitForCompletion());
            insatiableOverlayMat2.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampBombOrb.png").WaitForCompletion());

            destealthMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpBossDissolve.mat").WaitForCompletion();

            destealthMaterial2 = Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercEvisTarget.mat").WaitForCompletion();

            parryMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/CritOnUse/matFullCrit.mat").WaitForCompletion();

            mercMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercSwipe1.mat").WaitForCompletion());
        }

        private static void CreateModels()
        {
        }
        #region effects
        private static void CreateEffects()
        {
            Material fakeMerc = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercExposed.mat").WaitForCompletion());
            fakeMerc.SetColor("_TintColor", Color.red);
            warlockHexConsume = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab").WaitForCompletion().InstantiateClone("WarlockHexed", true);
            warlockHexConsume.AddComponent<NetworkIdentity>();
            warlockHexConsume.transform.Find("Visual, Consumed").Find("PulseEffect, Ring (1)").gameObject.GetComponent<ParticleSystemRenderer>().material = fakeMerc;
            warlockHexConsume.gameObject.GetComponent<EffectComponent>().soundName = "sfx_interrogator_point";
            Object.Destroy(warlockHexConsume.transform.Find("Visual, Consumed").Find("PulseEffect, Slash").gameObject);

            Modules.Content.CreateAndAddEffectDef(warlockHexConsume);

            warlockTracerEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/TracerGolem.prefab").WaitForCompletion().InstantiateClone("WarlockTracer", true);
            warlockTracerEffect.AddComponent<NetworkIdentity>();
            var tracerCOL = warlockTracerEffect.transform.Find("SmokeBeam").gameObject.GetComponent<ParticleSystem>().colorOverLifetime;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.black, 0.1f), new GradientColorKey(Color.red, 0.34f), new GradientColorKey(Color.black, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.75f, 1.0f) });
            tracerCOL.color = grad;
            warlockTracerEffect.transform.Find("SmokeBeam").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", null);

            Modules.Content.CreateAndAddEffectDef(warlockTracerEffect);

            spawnPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossDeathEffect.prefab").WaitForCompletion().InstantiateClone("SpawnEffect");
            spawnPrefab.AddComponent<NetworkIdentity>();
            spawnPrefab.GetComponent<EffectComponent>().applyScale = true;
            spawnPrefab.transform.Find("DashRings").localScale *= 0.75f;
            Modules.Content.CreateAndAddEffectDef(spawnPrefab);

            spawnPrefab2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossDeathEffect.prefab").WaitForCompletion().InstantiateClone("SpawnEffect2");
            spawnPrefab2.AddComponent<NetworkIdentity>();
            spawnPrefab2.GetComponent<EffectComponent>().applyScale = true;
            spawnPrefab2.transform.Find("DashRings").localScale *= 0.75f;
            spawnPrefab2.transform.Find("NoiseTrails").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());
            spawnPrefab2.transform.Find("Dash").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());
            spawnPrefab2.transform.Find("DashRings").gameObject.GetComponent<ParticleSystemRenderer>().material = mercMat;
            spawnPrefab2.transform.Find("Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());
            spawnPrefab2.transform.Find("Point Light").gameObject.GetComponent<Light>().color = Color.cyan;

            GameObject.Destroy(spawnPrefab2.transform.Find("Flash, Red").gameObject);
            GameObject.Destroy(spawnPrefab2.transform.Find("PP").gameObject);
            Modules.Content.CreateAndAddEffectDef(spawnPrefab2);

            bloodExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("WarlockBloodExplosion", false);

            Material bloodMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion());
            Material bloodMat2 = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion());

            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodExplosionEffect.GetComponentInChildren<Light>().gameObject.SetActive(false);

            bloodExplosionEffect.GetComponentInChildren<PostProcessVolume>().sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/ppLocalGold.asset").WaitForCompletion();

            bloodExplosionEffect.GetComponent<EffectComponent>().applyScale = true;
            bloodExplosionEffect.GetComponent<EffectComponent>().soundName = "sfx_blood_gurgle";
            Modules.Content.CreateAndAddEffectDef(bloodExplosionEffect);

            bloodSpurtEffect = mainAssetBundle.LoadAsset<GameObject>("BloodSpurtEffect");

            bloodSpurtEffect.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodSpurtEffect.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial = bloodMat2;

            bloodExplosionEffect2 = PrefabAPI.InstantiateClone(bloodExplosionEffect, "WarlockBloodExplosionNoSound");

            bloodExplosionEffect2.GetComponent<EffectComponent>().soundName = "Play_magmaWorm_M1_explo";

            Modules.Content.CreateAndAddEffectDef(bloodExplosionEffect2);

            Material bloodMat3 = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion());
            Material bloodMat4 = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion());

            bloodSpurtEffect2 = PrefabAPI.InstantiateClone(bloodSpurtEffect, "BloodSpurtEffect2");

            bloodSpurtEffect2.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material = bloodMat4;
            bloodSpurtEffect2.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);
            bloodSpurtEffect2.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial = bloodMat4;
            bloodSpurtEffect2.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial.SetColor("_TintColor", Color.cyan);

            stitchEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BleedEffect.prefab").WaitForCompletion().InstantiateClone("StitchEffect");
            stitchEffect.AddComponent<NetworkIdentity>();
            stitchEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", warlockColor);

            sewnCdEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifyStack3Effect.prefab").WaitForCompletion().InstantiateClone("SewnNo", false);
            sewnCdEffect.AddComponent<NetworkIdentity>();
            sewnCdEffect.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.clear);
            sewnCdEffect.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.clear);
            sewnCdEffect.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.clear);

            sewnEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifyStack3Effect.prefab").WaitForCompletion().InstantiateClone("SewnYes");
            sewnEffect.AddComponent<NetworkIdentity>();
            sewnEffect.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", warlockColor);
            sewnEffect.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", warlockColor);
            sewnEffect.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", warlockColor);

            sewnEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifyStack3Effect.prefab").WaitForCompletion().InstantiateClone("SewnYes2");
            sewnEffect2.AddComponent<NetworkIdentity>();
            sewnEffect2.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.cyan);
            sewnEffect2.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.cyan);
            sewnEffect2.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.cyan);

            parrySlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureTracer.prefab").WaitForCompletion().InstantiateClone("ParrySlashEffect");
            parrySlashEffect.AddComponent<NetworkIdentity>();
            parrySlashEffect.transform.gameObject.GetComponent<EffectComponent>().soundName = "Play_huntress_R_snipe_shoot";
            parrySlashEffect.transform.GetChild(2).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", warlockColor);
            parrySlashEffect.transform.GetChild(3).gameObject.GetComponent<LineRenderer>().material.SetColor("_TintColor", warlockColor);
            Modules.Content.CreateAndAddEffectDef(parrySlashEffect);

            parrySlashEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureTracer.prefab").WaitForCompletion().InstantiateClone("ParrySlashEffect2");
            parrySlashEffect2.AddComponent<NetworkIdentity>();
            parrySlashEffect2.transform.gameObject.GetComponent<EffectComponent>().soundName = "Play_huntress_R_snipe_shoot";
            parrySlashEffect2.transform.GetChild(2).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);
            parrySlashEffect2.transform.GetChild(3).gameObject.GetComponent<LineRenderer>().material.SetColor("_TintColor", Color.cyan);
            Modules.Content.CreateAndAddEffectDef(parrySlashEffect2);

            telekinesisTracker = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion().InstantiateClone("WarlockTracker", false);
            Material component = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/UI/matUIOverbrighten2x.mat").WaitForCompletion());
            Object.DestroyImmediate(telekinesisTracker.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>());
            SpriteRenderer balls = telekinesisTracker.transform.GetChild(0).gameObject.AddComponent<SpriteRenderer>();
            balls.SetMaterial(component);
            balls.sprite = mainAssetBundle.LoadAsset<Sprite>("Grab");
            telekinesisTracker.transform.GetChild(1).gameObject.SetActive(false);
            Sprite sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texCrosshair2.png").WaitForCompletion();
            Material component2 = telekinesisTracker.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().material;
            Object.DestroyImmediate(telekinesisTracker.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>());
            SpriteRenderer balls2 = telekinesisTracker.transform.GetChild(2).gameObject.AddComponent<SpriteRenderer>();
            balls2.SetMaterial(component2);
            balls2.sprite = sprite;
            balls2.color = warlockColor;

            telekinesisCdTracker = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion().InstantiateClone("NotWarlockTracker", false);
            component = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/UI/matUIOverbrighten2x.mat").WaitForCompletion());
            Object.DestroyImmediate(telekinesisCdTracker.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>());
            balls = telekinesisCdTracker.transform.GetChild(0).gameObject.AddComponent<SpriteRenderer>();
            balls.SetMaterial(component);
            balls.sprite = mainAssetBundle.LoadAsset<Sprite>("NoGrab");
            telekinesisCdTracker.transform.GetChild(1).gameObject.SetActive(false);
            sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texCrosshair2.png").WaitForCompletion();
            component2 = telekinesisCdTracker.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().material;
            Object.DestroyImmediate(telekinesisCdTracker.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>());
            balls2 = telekinesisCdTracker.transform.GetChild(2).gameObject.AddComponent<SpriteRenderer>();
            balls2.SetMaterial(component2);
            balls2.sprite = sprite;
            balls2.color = warlockColor;

            blinkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("BlinkStart");
            blinkEffect.AddComponent<NetworkIdentity>();
            blinkEffect.GetComponent<EffectComponent>().applyScale = true;
            blinkEffect.transform.GetChild(0).localScale = Vector3.one * 0.5f;
            blinkEffect.transform.GetChild(1).localScale = Vector3.one * 0.5f;
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("Point light").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("Flash, Red").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("Flash, White").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("Distortion").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("Dash").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("Dash, Bright").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("LongLifeNoiseTrails").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("Particles").Find("LongLifeNoiseTrails, Bright").gameObject);
            GameObject.DestroyImmediate(blinkEffect.transform.Find("PP").gameObject);

            Modules.Content.CreateAndAddEffectDef(blinkEffect);

            blinkEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("BlinkStart2");
            blinkEffect2.AddComponent<NetworkIdentity>();
            blinkEffect2.GetComponent<EffectComponent>().applyScale = true;
            blinkEffect2.transform.GetChild(0).localScale = Vector3.one * 0.5f;
            blinkEffect2.transform.GetChild(1).localScale = Vector3.one * 0.5f;
            blinkEffect2.transform.Find("Particles").Find("DashRings").gameObject.GetComponent<ParticleSystemRenderer>().material = mercMat;
            blinkEffect2.transform.Find("Particles").Find("Sphere").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLunarElectric.png").WaitForCompletion());
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("Point light").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("Flash, Red").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("Flash, White").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("Distortion").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("Dash").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("Dash, Bright").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("LongLifeNoiseTrails").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("Particles").Find("LongLifeNoiseTrails, Bright").gameObject);
            GameObject.DestroyImmediate(blinkEffect2.transform.Find("PP").gameObject);

            Modules.Content.CreateAndAddEffectDef(blinkEffect2);

            smallBlinkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpBlinkEffect.prefab").WaitForCompletion().InstantiateClone("BlinkSmall");
            smallBlinkEffect.AddComponent<NetworkIdentity>();
            Modules.Content.CreateAndAddEffectDef(smallBlinkEffect);

            smallBlinkEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpBlinkEffect.prefab").WaitForCompletion().InstantiateClone("BlinkSmall2");
            smallBlinkEffect2.AddComponent<NetworkIdentity>();
            smallBlinkEffect2.transform.Find("Particles").Find("NoiseTrails").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarGolem/matLunarGolemBlastDustLG.mat").WaitForCompletion());
            smallBlinkEffect2.transform.Find("Particles").Find("Flash, White").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/bazaar/matSeerPortalEffectEdge.mat").WaitForCompletion());
            smallBlinkEffect2.transform.Find("Particles").Find("Point light").gameObject.GetComponent<Light>().color = Color.cyan;
            smallBlinkEffect2.transform.Find("Particles").Find("Dash").gameObject.GetComponent<ParticleSystemRenderer>().material = mercMat;
            Modules.Content.CreateAndAddEffectDef(smallBlinkEffect2);
            /*
            clawSlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSlash.prefab").WaitForCompletion().InstantiateClone("WarlockSlash");
            clawSlashEffect.AddComponent<NetworkIdentity>();
            clawSlashEffect.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
            clawSlashEffect.GetComponent<ScaleParticleSystemDuration>().initialDuration = 0.5f;
            clawSlashEffect.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Croco/matCrocoSlash.mat").WaitForCompletion());
            clawSlashEffect.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", mainAssetBundle.LoadAsset<Texture>("texRampWarlock"));

            clawSlashEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSlash.prefab").WaitForCompletion().InstantiateClone("WarlockSlash2");
            clawSlashEffect2.AddComponent<NetworkIdentity>();
            clawSlashEffect2.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
            clawSlashEffect2.GetComponent<ScaleParticleSystemDuration>().initialDuration = 0.5f;
            clawSlashEffect2.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Croco/matCrocoSlash.mat").WaitForCompletion());
            clawSlashEffect2.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft2.png").WaitForCompletion());

            clawSlashComboEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoComboFinisherSlash.prefab").WaitForCompletion().InstantiateClone("WarlockComboSlash");
            clawSlashComboEffect.AddComponent<NetworkIdentity>();
            clawSlashComboEffect.transform.GetChild(0).localScale = new Vector3(1.25f, 1.25f, 1.25f);
            clawSlashComboEffect.GetComponent<ScaleParticleSystemDuration>().initialDuration = 0.5f;
            clawSlashComboEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Croco/matCrocoSlash.mat").WaitForCompletion());
            clawSlashComboEffect.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", mainAssetBundle.LoadAsset<Texture>("texRampWarlock"));
            clawSlashComboEffect.transform.GetChild(1).gameObject.SetActive(false);

            clawSlashComboEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoComboFinisherSlash.prefab").WaitForCompletion().InstantiateClone("WarlockComboSlash2");
            clawSlashComboEffect2.AddComponent<NetworkIdentity>();
            clawSlashComboEffect2.transform.GetChild(0).localScale = new Vector3(1.25f, 1.25f, 1.25f);
            clawSlashComboEffect2.GetComponent<ScaleParticleSystemDuration>().initialDuration = 0.5f;
            clawSlashComboEffect2.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Croco/matCrocoSlash.mat").WaitForCompletion());
            clawSlashComboEffect2.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft2.png").WaitForCompletion());
            clawSlashComboEffect2.transform.GetChild(1).gameObject.SetActive(false);
            */
            scissorsSlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("ScissorSwing");
            scissorsSlashEffect.AddComponent<NetworkIdentity>();
            scissorsSlashEffect.transform.GetChild(0).gameObject.SetActive(false);
            scissorsSlashEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());//Assets.LoadEffect("HenrySwordSwingEffect", true);
            var fard = scissorsSlashEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fard.startLifetimeMultiplier = 2f;

            scissorsSlashEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("ScissorSwingBlue");
            scissorsSlashEffect2.AddComponent<NetworkIdentity>();
            scissorsSlashEffect2.transform.GetChild(0).gameObject.SetActive(false);
            fard = scissorsSlashEffect2.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fard.startLifetimeMultiplier = 2f;

            scissorsSlashComboEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("ScissorSwing3");
            scissorsSlashComboEffect.AddComponent<NetworkIdentity>();
            scissorsSlashComboEffect.transform.GetChild(0).gameObject.SetActive(false);
            scissorsSlashComboEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());
            scissorsSlashComboEffect.transform.GetChild(1).localScale = new Vector3(1f, 1.5f, 1.5f);

            scissorsSlashComboEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("ScissorSwing3Blue");
            scissorsSlashComboEffect2.AddComponent<NetworkIdentity>();
            scissorsSlashComboEffect2.transform.GetChild(0).gameObject.SetActive(false);
            scissorsSlashComboEffect2.transform.GetChild(1).localScale = new Vector3(1f, 1.5f, 1.5f);

            clipSlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("ClipSwing");
            clipSlashEffect.AddComponent<NetworkIdentity>();
            clipSlashEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());
            clipSlashEffect.transform.GetChild(1).localScale = new Vector3(0.5f, 0.75f, 0.5f);
            fard = clipSlashEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fard.startLifetimeMultiplier = 0.6f;

            clipSlashEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("ClipSwing2");
            clipSlashEffect2.AddComponent<NetworkIdentity>();
            clipSlashEffect2.transform.GetChild(1).localScale = new Vector3(0.5f, 0.75f, 0.5f);
            fard = clipSlashEffect2.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fard.startLifetimeMultiplier = 0.6f;

            pickupScissorEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("ScissorPickupSwing");
            pickupScissorEffect.AddComponent<NetworkIdentity>();
            pickupScissorEffect.transform.GetChild(0).localScale *= 1.5f;
            pickupScissorEffect.transform.GetChild(0).rotation = Quaternion.AngleAxis(90f, Vector3.left);
            pickupScissorEffect.transform.GetChild(1).rotation = Quaternion.AngleAxis(90f, Vector3.left);
            pickupScissorEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());
            fard = pickupScissorEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fard.startLifetimeMultiplier = 2f;
            Modules.Content.CreateAndAddEffectDef(pickupScissorEffect);

            pickupScissorEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("ScissorPickupSwing2");
            pickupScissorEffect2.AddComponent<NetworkIdentity>();
            pickupScissorEffect2.transform.GetChild(0).localScale *= 1.5f;
            pickupScissorEffect2.transform.GetChild(0).rotation = Quaternion.AngleAxis(90f, Vector3.left);
            pickupScissorEffect2.transform.GetChild(1).rotation = Quaternion.AngleAxis(90f, Vector3.left);
            fard = pickupScissorEffect2.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fard.startLifetimeMultiplier = 2f;
            Modules.Content.CreateAndAddEffectDef(pickupScissorEffect2);

            wideSlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("WarlockWideSwordSwing");
            wideSlashEffect.AddComponent<NetworkIdentity>();
            wideSlashEffect.transform.GetChild(0).localScale *= 1.5f;
            wideSlashEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());
            var sex = wideSlashEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.6f;
            sex.startRotation3D = false;
            Object.Destroy(wideSlashEffect.GetComponent<EffectComponent>());

            wideSlashEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("WarlockWideSwordSwing");
            wideSlashEffect2.AddComponent<NetworkIdentity>();
            wideSlashEffect2.transform.GetChild(0).localScale *= 1.5f;
            sex = wideSlashEffect2.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.6f;
            sex.startRotation3D = false;
            Object.Destroy(wideSlashEffect2.GetComponent<EffectComponent>());

            uppercutEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("WarlockUppercut");
            uppercutEffect.AddComponent<NetworkIdentity>();
            uppercutEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());
            sex = uppercutEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.6f;
            sex.startRotation3D = false;
            Object.Destroy(uppercutEffect.GetComponent<EffectComponent>());

            uppercutEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("WarlockUppercut2");
            uppercutEffect2.AddComponent<NetworkIdentity>();
            sex = uppercutEffect2.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            sex.startLifetimeMultiplier = 0.6f;
            sex.startRotation3D = false;
            Object.Destroy(uppercutEffect2.GetComponent<EffectComponent>());


            scissorsHitImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("ScissorImpact", false);
            scissorsHitImpactEffect.AddComponent<NetworkIdentity>();
            scissorsHitImpactEffect.GetComponent<OmniEffect>().enabled = false;
            Material material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat").WaitForCompletion());
            material.SetColor("_TintColor", Color.red);
            scissorsHitImpactEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            scissorsHitImpactEffect.transform.GetChild(2).localScale = Vector3.one * 1.5f;
            scissorsHitImpactEffect.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterFireCorrupted.mat").WaitForCompletion());
            material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSlashImpact.mat").WaitForCompletion());
            scissorsHitImpactEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            scissorsHitImpactEffect.transform.GetChild(4).localScale = Vector3.one * 3f;
            scissorsHitImpactEffect.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDust.mat").WaitForCompletion());
            scissorsHitImpactEffect.transform.GetChild(6).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat").WaitForCompletion());
            scissorsHitImpactEffect.transform.GetChild(6).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat").WaitForCompletion());
            scissorsHitImpactEffect.transform.GetChild(1).localScale = Vector3.one * 1.5f;
            scissorsHitImpactEffect.transform.GetChild(1).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(2).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(3).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(4).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(5).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(6).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
            scissorsHitImpactEffect.transform.GetChild(6).transform.localScale = new Vector3(1f, 1f, 3f);
            scissorsHitImpactEffect.transform.localScale = Vector3.one * 1.5f;
            Modules.Content.CreateAndAddEffectDef(scissorsHitImpactEffect);

            scissorsHitImpactEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("ScissorImpact2", false);
            scissorsHitImpactEffect2.AddComponent<NetworkIdentity>();
            scissorsHitImpactEffect2.GetComponent<OmniEffect>().enabled = false;
            scissorsHitImpactEffect2.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat").WaitForCompletion());
            scissorsHitImpactEffect2.transform.GetChild(2).localScale = Vector3.one * 1.5f;
            scissorsHitImpactEffect2.transform.GetChild(4).localScale = Vector3.one * 3f;
            scissorsHitImpactEffect2.transform.GetChild(1).localScale = Vector3.one * 1.5f;
            scissorsHitImpactEffect2.transform.GetChild(1).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(2).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(3).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(4).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(5).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(6).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
            scissorsHitImpactEffect2.transform.GetChild(6).transform.localScale = new Vector3(1f, 1f, 3f);
            scissorsHitImpactEffect2.transform.localScale = Vector3.one * 1.5f;
            Modules.Content.CreateAndAddEffectDef(scissorsHitImpactEffect2);

            impDashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressBlinkEffect.prefab").WaitForCompletion().InstantiateClone("ImpDash");
            impDashEffect.AddComponent<NetworkIdentity>();
            material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());
            impDashEffect.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            impDashEffect.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            impDashEffect.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
            impDashEffect.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
            impDashEffect.transform.GetChild(0).GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            impDashEffect.transform.GetChild(0).GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            WarlockMod.Modules.Content.CreateAndAddEffectDef(impDashEffect);

            impDashEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressBlinkEffect.prefab").WaitForCompletion().InstantiateClone("ImpDash2");
            impDashEffect2.AddComponent<NetworkIdentity>();
            impDashEffect2.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            impDashEffect2.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            impDashEffect2.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
            impDashEffect2.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
            WarlockMod.Modules.Content.CreateAndAddEffectDef(impDashEffect2);

            insatiableEndEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarSkillReplacements/LunarDetonatorConsume.prefab").WaitForCompletion().InstantiateClone("InsatiableEndEffect");
            insatiableEndEffect.AddComponent<NetworkIdentity>();
            var fart = insatiableEndEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fart.startColor = Color.black;
            fart = insatiableEndEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().main;
            fart.startColor = Color.red;
            insatiableEndEffect.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            insatiableEndEffect.transform.GetChild(3).gameObject.SetActive(false);
            insatiableEndEffect.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarSkillReplacements/matLunarNeedleImpactEffect.mat").WaitForCompletion());
            material.SetColor("_TintColor", Color.red);
            insatiableEndEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            insatiableEndEffect.transform.GetChild(6).gameObject.SetActive(false);
            Object.Destroy(insatiableEndEffect.GetComponent<EffectComponent>());

            insatiableEndEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarSkillReplacements/LunarDetonatorConsume.prefab").WaitForCompletion().InstantiateClone("InsatiableEndEffect2");
            insatiableEndEffect2.AddComponent<NetworkIdentity>();
            fart = insatiableEndEffect2.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            fart.startColor = Color.black;
            fart = insatiableEndEffect2.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().main;
            fart.startColor = Color.cyan;
            insatiableEndEffect2.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);
            insatiableEndEffect2.transform.GetChild(3).gameObject.SetActive(false);
            insatiableEndEffect2.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);
            material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarSkillReplacements/matLunarNeedleImpactEffect.mat").WaitForCompletion());
            material.SetColor("_TintColor", Color.cyan);
            insatiableEndEffect2.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            insatiableEndEffect2.transform.GetChild(6).gameObject.SetActive(false);
            Object.Destroy(insatiableEndEffect2.GetComponent<EffectComponent>());

            genericImpactExplosionEffect = CreateImpactExplosionEffect("WarlockScissorImpact", Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodGeneric.mat").WaitForCompletion(),
                Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDecal.mat").WaitForCompletion(), false, 2);
            Material blueMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDecal.mat").WaitForCompletion());
            blueMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());
            genericImpactExplosionEffect2 = CreateImpactExplosionEffect("WarlockScissorImpact2", Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodGeneric.mat").WaitForCompletion(),
                blueMat, true, 2);

            bloodSplatterEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("Splat", true);
            bloodSplatterEffect.AddComponent<NetworkIdentity>();
            bloodSplatterEffect.transform.GetChild(0).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(1).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(2).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(3).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(4).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(5).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(6).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(7).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(8).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(9).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(10).gameObject.SetActive(false);
            bloodSplatterEffect.transform.Find("Decal").GetComponent<Decal>().Material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDecal.mat").WaitForCompletion());
            bloodSplatterEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;
            bloodSplatterEffect.transform.GetChild(12).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(13).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(14).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(15).gameObject.SetActive(false);
            bloodSplatterEffect.transform.localScale = Vector3.one;
            WarlockMod.Modules.Content.CreateAndAddEffectDef(bloodSplatterEffect);

            bloodSplatterEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("Splat", true);
            bloodSplatterEffect2.AddComponent<NetworkIdentity>();
            bloodSplatterEffect2.transform.GetChild(0).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(1).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(2).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(3).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(4).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(5).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(6).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(7).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(8).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(9).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(10).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.Find("Decal").GetComponent<Decal>().Material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/CrippleWard/matLunarWardDecal.mat").WaitForCompletion());
            bloodSplatterEffect2.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;
            bloodSplatterEffect2.transform.GetChild(12).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(13).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(14).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.GetChild(15).gameObject.SetActive(false);
            bloodSplatterEffect2.transform.localScale = Vector3.one;
            WarlockMod.Modules.Content.CreateAndAddEffectDef(bloodSplatterEffect2);

            slamEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossGroundSlam.prefab").WaitForCompletion().InstantiateClone("WarlockSlamEffect");
            slamEffect.AddComponent<NetworkIdentity>();
            WarlockMod.Modules.Content.CreateAndAddEffectDef(slamEffect);
            slamEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossGroundSlam.prefab").WaitForCompletion().InstantiateClone("WarlockSlamEffect2");
            slamEffect2.AddComponent<NetworkIdentity>();
            slamEffect2.transform.Find("Particles").Find("ClawMesh").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());
            var main = slamEffect2.transform.Find("Particles").Find("Dust, Directional").gameObject.GetComponent<ParticleSystem>().main;
            main.startColor = Color.cyan;
            main = slamEffect2.transform.Find("Particles").Find("Dust, Billboard").gameObject.GetComponent<ParticleSystem>().main;
            main.startColor = Color.cyan;
            slamEffect2.transform.Find("Particles").Find("Dash").gameObject.GetComponent<ParticleSystemRenderer>().material = mercMat;
            slamEffect2.transform.Find("Particles").Find("DashRings").gameObject.GetComponent<ParticleSystemRenderer>().material = mercMat;
            slamEffect2.transform.Find("Particles").Find("Sphere").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLunarElectric.png").WaitForCompletion());
            GameObject.DestroyImmediate(slamEffect2.transform.Find("PP").gameObject);
            GameObject.DestroyImmediate(slamEffect2.transform.Find("Particles").Find("Dash, Bright").gameObject);
            GameObject.DestroyImmediate(slamEffect2.transform.Find("Particles").Find("Point light").gameObject);
            GameObject.DestroyImmediate(slamEffect2.transform.Find("Particles").Find("Flash, White").gameObject);
            GameObject.DestroyImmediate(slamEffect2.transform.Find("Particles").Find("Flash, Red").gameObject);
            WarlockMod.Modules.Content.CreateAndAddEffectDef(slamEffect2);

            GameObject impThing = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ImpVoidspikeProjectile");

            TeamAreaIndicator teamArea = PrefabAPI.InstantiateClone(impThing.transform.Find("ImpactEffect/TeamAreaIndicator, FullSphere").gameObject, "WarlockTeamIndicator", false).GetComponent<TeamAreaIndicator>();

            teamArea.teamMaterialPairs[1].sharedMaterial = new Material(teamArea.teamMaterialPairs[1].sharedMaterial);
            teamArea.teamMaterialPairs[1].sharedMaterial.SetColor("_TintColor", Color.red);

            WarlockTeamAreaIndicator = teamArea;

            GameObject impThing2 = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ImpVoidspikeProjectile");

            TeamAreaIndicator teamArea2 = PrefabAPI.InstantiateClone(impThing2.transform.Find("ImpactEffect/TeamAreaIndicator, FullSphere").gameObject, "WarlockTeamIndicator", false).GetComponent<TeamAreaIndicator>();

            teamArea2.teamMaterialPairs[1].sharedMaterial = new Material(teamArea2.teamMaterialPairs[1].sharedMaterial);
            teamArea2.teamMaterialPairs[1].sharedMaterial.SetColor("_TintColor", Color.cyan);
            WarlockTeamAreaIndicator2 = teamArea2;

            //Add this to her hands during insatiable?
            GameObject obj = new GameObject();
            trailEffect = obj.InstantiateClone("ScissorTrail", false);
            TrailRenderer trail = trailEffect.AddComponent<TrailRenderer>();
            trail.startWidth = 1f;
            trail.endWidth = 0f;
            trail.time = 0.5f;
            trail.emitting = true;
            trail.numCornerVertices = 0;
            trail.numCapVertices = 0;
            trail.material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matSmokeTrail.mat").WaitForCompletion());
            trail.material.SetTexture("_RemapTex", mainAssetBundle.LoadAsset<Texture>("texRampWarlockTrail"));
            trail.alignment = LineAlignment.TransformZ;

            GameObject obj2 = new GameObject();
            trailEffectHands = obj2.InstantiateClone("WarlockTrail", false);
            TrailRenderer trail2 = trailEffectHands.AddComponent<TrailRenderer>();
            trail2.startWidth = 0.3f;
            trail2.endWidth = 0f;
            trail2.time = 0.5f;
            trail2.emitting = true;
            trail2.numCornerVertices = 0;
            trail2.numCapVertices = 0;
            trail2.material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matSmokeTrail.mat").WaitForCompletion());
            trail2.material.SetTexture("_RemapTex", mainAssetBundle.LoadAsset<Texture>("texRampWarlockTrail"));

            GameObject obj3 = new GameObject();
            trailEffect2 = obj3.InstantiateClone("ScissorTrail2", false);
            TrailRenderer trail3 = trailEffect2.AddComponent<TrailRenderer>();
            trail3.startWidth = 1f;
            trail3.endWidth = 0f;
            trail3.time = 0.5f;
            trail3.emitting = true;
            trail3.numCornerVertices = 0;
            trail3.numCapVertices = 0;
            trail3.material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matSmokeTrail.mat").WaitForCompletion());
            trail3.material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());
            trail3.alignment = LineAlignment.TransformZ;

            GameObject obj4 = new GameObject();
            trailEffectHands2 = obj4.InstantiateClone("WarlockTrail2", false);
            TrailRenderer trail4 = trailEffectHands2.AddComponent<TrailRenderer>();
            trail4.startWidth = 0.3f;
            trail4.endWidth = 0f;
            trail4.time = 0.5f;
            trail4.emitting = true;
            trail4.numCornerVertices = 0;
            trail4.numCapVertices = 0;
            trail4.material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matSmokeTrail.mat").WaitForCompletion());
            trail4.material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampLightning.png").WaitForCompletion());

        }

        #endregion

        #region projectiles
        private static void CreateProjectiles()
        {

        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
            scissorsHitSoundEvent = WarlockMod.Modules.Content.CreateAndAddNetworkSoundEventDef("Play_merc_sword_impact");
        }
        #endregion

        private static void CreateUI()
        {
        }

        #region helpers
        private static GameObject CreateImpactExplosionEffect(string effectName, Material bloodMat, Material colorMat, bool blue, float scale = 1f)
        {
            GameObject newEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone(effectName, true);

            newEffect.transform.Find("Spikes, Small").gameObject.SetActive(false);

            newEffect.transform.Find("PP").gameObject.SetActive(false);
            newEffect.transform.Find("Point light").gameObject.SetActive(false);
            newEffect.transform.Find("Flash Lines").GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat").WaitForCompletion());
            if (blue) newEffect.transform.Find("Flash Lines").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);

            newEffect.transform.GetChild(3).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.GetChild(6).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;

            var boom = newEffect.transform.Find("Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.5f;
            boom = newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.3f;
            boom = newEffect.transform.GetChild(6).GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.4f;

            newEffect.transform.Find("Physics").GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matFracturedGround.mat").WaitForCompletion());

            newEffect.transform.Find("Decal").GetComponent<Decal>().Material = colorMat;
            newEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;

            newEffect.transform.Find("FoamSplash").gameObject.SetActive(false);
            newEffect.transform.Find("FoamBilllboard").gameObject.SetActive(false);
            newEffect.transform.Find("Dust").gameObject.SetActive(false);
            newEffect.transform.Find("Dust, Directional").gameObject.SetActive(false);

            newEffect.transform.localScale = Vector3.one * scale;

            newEffect.AddComponent<NetworkIdentity>();

            ParticleSystemColorFromEffectData PSCFED = newEffect.AddComponent<ParticleSystemColorFromEffectData>();
            PSCFED.particleSystems = new ParticleSystem[]
            {
                newEffect.transform.Find("Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(6).GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(3).GetComponent<ParticleSystem>()
            };
            if (blue)
            {
                newEffect.transform.Find("Fire").gameObject.SetActive(false);
                newEffect.transform.Find("Flash Lines, Fire").gameObject.SetActive(false);
                newEffect.transform.GetChild(6).gameObject.SetActive(false);
                newEffect.transform.GetChild(3).gameObject.SetActive(false);
            }
            PSCFED.effectComponent = newEffect.GetComponent<EffectComponent>();

            WarlockMod.Modules.Content.CreateAndAddEffectDef(newEffect);

            return newEffect;
        }
        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = mainAssetBundle.LoadAsset<Material>(materialName);

            if (!tempMat) return commandoMat;

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0f);
        }
        #endregion
    }
}