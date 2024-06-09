using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using WarlockMod.Warlock.Content;
using System;

namespace WarlockMod.Warlock.Components
{
    public class WarlockController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        private Material[] swordMat;
        private Material[] batMat;
        public string currentSkinNameToken => this.skinController.skins[this.skinController.currentSkinIndex].nameToken;
        public string altSkinNameToken => WarlockSurvivor.INTERROGATOR_PREFIX + "MASTERY_SKIN_NAME";

        public bool primaryEmpowered => this.characterBody.HasBuff(WarlockBuffs.warlockEmpoweredM1Buff);
        public bool secondaryEmpowered => this.characterBody.HasBuff(WarlockBuffs.warlockEmpoweredM2Buff);
        public bool utilityEmpowered => this.characterBody.HasBuff(WarlockBuffs.warlockEmpoweredUtilityBuff);

        public int maxSecondaryStock = 1;
        public int currentSecondaryStock = 1;

        public int maxUtilityStock = 1;
        public int currentUtilityStock = 1;

        public float jamTimer;
        public float soundTimer = 3f;

        public float convictDurationMax;

        public uint soundID1;
        private bool hasStoppedSound;
        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();

            Hook();

            this.Invoke("ApplySkin", 0.3f);
        }
        private void Start()
        {
        }
        #region tooMuchCrap
        private void Hook()
        {
        }
        public void ApplySkin()
        {
            if (this.skinController)
            {
            }
        }

        #endregion
        private void FixedUpdate()
        {
            if(jamTimer > 0f) jamTimer -= Time.fixedDeltaTime;

            if (soundTimer > 0f) soundTimer -= Time.fixedDeltaTime;
            else if (!hasStoppedSound)
            {
                hasStoppedSound = true;
                AkSoundEngine.StopPlayingID(soundID1);
            }

        }
        public void SetupStockSecondary()
        {
            currentSecondaryStock = this.skillLocator.secondary.stock;
            maxSecondaryStock = this.skillLocator.secondary.maxStock;
        }
        public void SetupStockUtility()
        {
            currentUtilityStock = this.skillLocator.utility.stock;
            maxUtilityStock = this.skillLocator.utility.maxStock;
        }
        public void PlaySound()
        {
            soundID1 = Util.PlaySound("Play_imp_overlord_teleport_start", this.gameObject);
            hasStoppedSound = false;
            soundTimer = 3f;
        }
        public void ReturnSavedStocks()
        {
            this.skillLocator.secondary.RemoveAllStocks();
            this.skillLocator.utility.RemoveAllStocks();
            for (int i = 0; i < this.currentSecondaryStock; i++) this.skillLocator.secondary.AddOneStock();
            for (int i = 0; i < this.currentUtilityStock; i++) this.skillLocator.utility.AddOneStock();

        }
        private void OnDestroy()
        {
            AkSoundEngine.StopPlayingID(soundID1);
        }
    }
}
