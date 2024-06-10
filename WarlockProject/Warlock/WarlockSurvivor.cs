using BepInEx.Configuration;
using WarlockMod.Modules;
using WarlockMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RoR2.UI;
using R2API;
using R2API.Networking;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using WarlockMod.Warlock.Components;
using WarlockMod.Warlock.Content;
using WarlockMod.Warlock.SkillStates;
using HG;
using EntityStates;
using R2API.Networking.Interfaces;
using EmotesAPI;
using System.Runtime.CompilerServices;
using static RoR2.TeleporterInteraction;

namespace WarlockMod.Warlock
{
    public class WarlockSurvivor : SurvivorBase<WarlockSurvivor>
    {
        public override string assetBundleName => "warlock";
        public override string bodyName => "InterrogatorBody";
        public override string masterName => "InterrogatorMonsterMaster";
        public override string modelPrefabName => "mdlInterrogator";
        public override string displayPrefabName => "InterrogatorDisplay";

        public const string INTERROGATOR_PREFIX = WarlockPlugin.DEVELOPER_PREFIX + "_WARLOCK_";
        public override string survivorTokenPrefix => INTERROGATOR_PREFIX;

        internal static GameObject characterPrefab;

        public static SkillDef ritualScepterSkillDef;
        public static WarlockSkillDef m1EmpowerSkillDef;
        public static WarlockSkillDef m2EmpowerSkillDef;
        public static WarlockSkillDef utilityEmpowerSkillDef;
        public static SkillDef empowerSkillDef;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = INTERROGATOR_PREFIX + "NAME",
            subtitleNameToken = INTERROGATOR_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texInterrogatorIcon"),
            bodyColor = WarlockAssets.warlockColor,
            sortPosition = 99f,

            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 100f,
            armor = 0f,
            damage = 12f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                    dontHotpoo = true,
                },
                new CustomRendererInfo
                {
                    childName = "MeleeModel",
                    dontHotpoo = true,
                },
                new CustomRendererInfo
                {
                    childName = "CleaverModel",
                    dontHotpoo = true,
                },
                new CustomRendererInfo
                {
                    childName = "JacketModel",
                    dontHotpoo = true,
                },
                new CustomRendererInfo
                {
                    childName = "VisorModel",
                    dontHotpoo = true,
                },
                new CustomRendererInfo
                {
                    childName = "ExtraModel",
                    dontHotpoo = true,
                },
        };

        public override UnlockableDef characterUnlockableDef => WarlockUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new WarlockItemDisplays();
        public override AssetBundle assetBundle { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }
        public override void Initialize()
        {

            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            //need the character unlockable before you initialize the survivordef

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            WarlockConfig.Init();

            WarlockUnlockables.Init();

            base.InitializeCharacter();

            CameraParams.InitializeParams();

            ChildLocator childLocator = bodyPrefab.GetComponentInChildren<ChildLocator>();

            DamageTypes.Init();

            WarlockStates.Init();
            WarlockTokens.Init();

            WarlockAssets.InitAssets();

            WarlockBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            characterPrefab = bodyPrefab;

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<WarlockController>();
            bodyPrefab.AddComponent<WarlockTracker>();
        }
        public void AddHitboxes()
        {
            Prefabs.SetupHitBoxGroup(characterModelObject, "MeleeHitbox", "MeleeHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SkillStates.MainState), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
            Prefabs.AddEntityStateMachine(bodyPrefab, "MetaMenu");
        }

        #region skills
        public override void InitializeSkills()
        {
            bodyPrefab.AddComponent<WarlockPassive>();
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPassiveSkills();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
            if (WarlockPlugin.scepterInstalled) InitializeScepter();
        }

        private void AddPassiveSkills()
        {
            WarlockPassive passive = bodyPrefab.GetComponent<WarlockPassive>();

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = false;

            passive.interrogatorPassive = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = INTERROGATOR_PREFIX + "PASSIVE_NAME",
                skillNameToken = INTERROGATOR_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "PASSIVE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texInterrogatorPassive"),
                keywordTokens = new string[] { },
                activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle)),
                activationStateMachineName = "",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 2,
                stockToConsume = 1
            });

            Skills.AddPassiveSkills(passive.passiveSkillSlot.skillFamily, passive.interrogatorPassive);

            m1EmpowerSkillDef = Skills.CreateSkillDef<WarlockSkillDef>(new SkillDefInfo
            {
                skillName = "Empower1",
                skillNameToken = INTERROGATOR_PREFIX + "PRIMARY_EMPOWER1_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "PRIMARY_EMPOWER1_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictScepter"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Empower1)),
                activationStateMachineName = "MetaMenu",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0
            });

            m2EmpowerSkillDef = Skills.CreateSkillDef<WarlockSkillDef>(new SkillDefInfo
            {
                skillName = "Empower2",
                skillNameToken = INTERROGATOR_PREFIX + "SECONDARY_EMPOWER_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "SECONDARY_EMPOWER_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictScepter"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Empower2)),
                activationStateMachineName = "MetaMenu",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0
            });

            utilityEmpowerSkillDef = Skills.CreateSkillDef<WarlockSkillDef>(new SkillDefInfo
            {
                skillName = "Empower3",
                skillNameToken = INTERROGATOR_PREFIX + "UTILITY_EMPOWER_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "UTILITY_EMPOWER_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictScepter"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Empower3)),
                activationStateMachineName = "MetaMenu",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0
            });

            empowerSkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Empower",
                skillNameToken = INTERROGATOR_PREFIX + "SPECIAL_EMPOWER_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "SPECIAL_EMPOWER_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictScepter"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(Empower)),
                activationStateMachineName = "MetaMenu",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0
            });
        }

        private void AddPrimarySkills()
        {
            SkillDef crimsonSurge = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Crimson Surge",
                skillNameToken = INTERROGATOR_PREFIX + "PRIMARY_SURGE_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "PRIMARY_SURGE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texInterrogatorCleaverIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(CrimsonSurgePrep)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 0
            });

            Skills.AddPrimarySkills(bodyPrefab, crimsonSurge);

        }

        private void AddSecondarySkills()
        {
            SkillDef hex = Skills.CreateSkillDef<WarlockTrackerSkillDef>(new SkillDefInfo
            {
                skillName = "Hex",
                skillNameToken = INTERROGATOR_PREFIX + "SECONDARY_HEX_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "SECONDARY_HEX_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSwingIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(Hex)),
                activationStateMachineName = "Weapon2",
                baseMaxStock = 2,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { Tokens.agileKeyword }
            });

            Skills.AddSecondarySkills(bodyPrefab, hex);
        }

        private void AddUtilitySkills()
        {
            SkillDef dash = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Blood Dash",
                skillNameToken = INTERROGATOR_PREFIX + "UTILITY_BLOOD_DASH_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "UTILITY_BLOOD_DASH_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texFalsifyIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(BloodDash)),
                activationStateMachineName = "Weapon2",
                baseMaxStock = 2,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 0,
                requiredStock = 1,
                stockToConsume = 1

            });

            Skills.AddUtilitySkills(bodyPrefab, dash);
        }

        private void AddSpecialSkills()
        {
            WarlockSkillDef invoke = Skills.CreateSkillDef<WarlockSkillDef>(new SkillDefInfo
            {
                skillName = "Ritual",
                skillNameToken = INTERROGATOR_PREFIX + "SPECIAL_RITUAL_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "SPECIAL_RITUAL_DESCRIPTION",
                keywordTokens = new string[] { Tokens.metaMagicKeyword },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(RitualPrep)),
                activationStateMachineName = "MetaMenu",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = true,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Skills.AddSpecialSkills(bodyPrefab, invoke);
        }

        private void InitializeScepter()
        {
            ritualScepterSkillDef = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Ritual Scepter",
                skillNameToken = INTERROGATOR_PREFIX + "SPECIAL_SCEPTER_RITUAL_NAME",
                skillDescriptionToken = INTERROGATOR_PREFIX + "SPECIAL_SCEPTER_RITUAL_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictScepter"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(RitualPrep)),
                activationStateMachineName = "MetaMenu",
                baseMaxStock = 1,
                baseRechargeInterval = 5f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(ritualScepterSkillDef, bodyName, SkillSlot.Special, 0);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshInterrogator",
                "meshBat",
                "meshCleaver",
                "meshJacket",
                "meshVisor",
                "meshExtra");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            /*
            defaultSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = true,
                }
            };
            */
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            /*
            #region MasterySkin

            ////creating a new skindef as we did before
            SkinDef masterySkin = Modules.Skins.CreateSkinDef(INTERROGATOR_PREFIX + "MASTERY_SKIN_NAME",
                assetBundle.LoadAsset<Sprite>("texMonsoonSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                InterrogatorUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshSpyAlt",
                "meshRevolverAlt",//no gun mesh replacement. use same gun mesh
                "meshKnifeAlt",
                "meshWatchAlt",
                null,
                "meshVisorAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            masterySkin.rendererInfos[0].defaultMaterial = InterrogatorAssets.spyMonsoonMat;
            masterySkin.rendererInfos[1].defaultMaterial = InterrogatorAssets.spyMonsoonMat;
            masterySkin.rendererInfos[2].defaultMaterial = InterrogatorAssets.spyMonsoonMat;
            masterySkin.rendererInfos[3].defaultMaterial = InterrogatorAssets.spyMonsoonMat;
            masterySkin.rendererInfos[5].defaultMaterial = InterrogatorAssets.spyVisorMonsoonMat;

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = false,
                }
            };
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            skins.Add(masterySkin);

            #endregion
            */
            skinController.skins = skins.ToArray();
        }
        #endregion skins


        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            WarlockAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            //HUD.onHudTargetChangedGlobal += HUDSetup;
            On.RoR2.UI.LoadoutPanelController.Rebuild += LoadoutPanelController_Rebuild;
            //On.RoR2.HealthComponent.TakeDamage += new On.RoR2.HealthComponent.hook_TakeDamage(HealthComponent_TakeDamage);
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            if(WarlockPlugin.emotesInstalled) Emotes();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = WarlockAssets.mainAssetBundle.LoadAsset<GameObject>("interrogator_emoteskeleton");
                CustomEmotesAPI.ImportArmature(WarlockSurvivor.characterPrefab, skele);
            };
        }


        private static void LoadoutPanelController_Rebuild(On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig, LoadoutPanelController self)
        {
            orig(self);

            if (self.currentDisplayData.bodyIndex == BodyCatalog.FindBodyIndex("InterrogatorBody"))
            {
                foreach (LanguageTextMeshController i in self.gameObject.GetComponentsInChildren<LanguageTextMeshController>())
                {
                    if (i && i.token == "LOADOUT_SKILL_MISC") i.token = "Passive";
                }
            }
        }
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                if(self.HasBuff(WarlockBuffs.warlockHexxedDebuff) || self.HasBuff(WarlockBuffs.warlockHexxedEmpoweredDebuff))
                {
                    self.moveSpeed *= 0.85f;
                }
                if(self.baseNameToken == "KENKO_WARLOCK_NAME")
                {
                    WarlockController iController = self.gameObject.GetComponent<WarlockController>();
                    if(iController)
                    {

                    }
                }
            }
        }
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            CharacterBody attackerBody = damageReport.attackerBody;
            if (attackerBody && damageReport.attackerMaster && damageReport.victim)
            {
                if(attackerBody.baseNameToken == "KENKO_WARLOCK_NAME")
                {
                    if(NetworkServer.active)
                    {
                        if (attackerBody.GetBuffCount(WarlockBuffs.warlockCrimsonManaStack) < WarlockStaticValues.requiredCrimsonMana - 1)
                        {
                            attackerBody.AddBuff(WarlockBuffs.warlockCrimsonManaStack);
                        }
                        else
                        {
                            attackerBody.SetBuffCount(WarlockBuffs.warlockCrimsonManaStack.buffIndex, 0);

                            ConsumeOrb orb = new ConsumeOrb();
                            orb.origin = damageReport.victim.transform.position;
                            orb.target = Util.FindBodyMainHurtBox(attackerBody);
                            RoR2.Orbs.OrbManager.instance.AddOrb(orb);

                            if (damageReport.victim.gameObject.TryGetComponent<NetworkIdentity>(out var identity))
                            {
                                new SyncBloodExplosion(identity.netId, damageReport.victim.gameObject).Send(NetworkDestination.Clients);
                            }
                        }
                    }
                }
            }
        }
        internal static void HUDSetup(HUD hud)
        {
            /*
            if (hud.targetBodyObject && hud.targetMaster && hud.targetMaster.bodyPrefab == InterrogatorSurvivor.characterPrefab)
            {
                if (!hud.targetMaster.hasAuthority) return;
                Transform skillsContainer = hud.equipmentIcons[0].gameObject.transform.parent;

                // ammo display for atomic
                Transform healthbarContainer = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("BarRoots").Find("LevelDisplayCluster");

                GameObject stealthTracker = GameObject.Instantiate(healthbarContainer.gameObject, hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster"));
                stealthTracker.name = "AmmoTracker";
                stealthTracker.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                GameObject.DestroyImmediate(stealthTracker.transform.GetChild(0).gameObject);
                MonoBehaviour.Destroy(stealthTracker.GetComponentInChildren<LevelText>());
                MonoBehaviour.Destroy(stealthTracker.GetComponentInChildren<ExpBar>());

                stealthTracker.transform.Find("LevelDisplayRoot").Find("ValueText").gameObject.SetActive(false);
                GameObject.DestroyImmediate(stealthTracker.transform.Find("ExpBarRoot").gameObject);

                stealthTracker.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

                RectTransform rect = stealthTracker.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.8f, 0.8f, 1f);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.offsetMin = new Vector2(120f, -40f);
                rect.offsetMax = new Vector2(120f, -40f);
                rect.pivot = new Vector2(0.5f, 0f);
                //positional data doesnt get sent to clients? Manually making offsets works..
                rect.anchoredPosition = new Vector2(50f, 0f);
                rect.localPosition = new Vector3(120f, -40f, 0f);

                GameObject chargeBarAmmo = GameObject.Instantiate(InterrogatorAssets.mainAssetBundle.LoadAsset<GameObject>("WeaponChargeBar"));
                chargeBarAmmo.name = "StealthMeter";
                chargeBarAmmo.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                rect = chargeBarAmmo.GetComponent<RectTransform>();

                rect.localScale = new Vector3(0.75f, 0.1f, 1f);
                rect.anchorMin = new Vector2(100f, 2f);
                rect.anchorMax = new Vector2(100f, 2f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(100f, 2f);
                rect.localPosition = new Vector3(100f, 2f, 0f);
                rect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));

                ConvictHudController stealthComponent = stealthTracker.AddComponent<ConvictHudController>();

                stealthComponent.targetHUD = hud;
                stealthComponent.targetText = stealthTracker.transform.Find("LevelDisplayRoot").Find("PrefixText").gameObject.GetComponent<LanguageTextMeshController>();
                stealthComponent.durationDisplay = chargeBarAmmo;
                stealthComponent.durationBar = chargeBarAmmo.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                stealthComponent.durationBarColor = chargeBarAmmo.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>();
            }
            */
        }
    }
}