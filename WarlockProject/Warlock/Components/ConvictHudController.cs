/*
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using InterrogatorMod.Interrogator.Components;
using InterrogatorMod.Interrogator.Content;

namespace InterrogatorMod.Interrogator.Components
{
    public class ConvictHudController : MonoBehaviour
    {
        public HUD targetHUD;
        public InterrogatorController interrogatorController;

        public LanguageTextMeshController targetText;
        public GameObject durationDisplay;
        public Image durationBar;
        public Image durationBarColor;

        private void Start()
        {
            this.interrogatorController = this.targetHUD?.targetBodyObject?.GetComponent<InterrogatorController>();
            this.interrogatorController.onConvictDurationChange += SetDisplay;

            this.durationDisplay.SetActive(false);
            SetDisplay();
        }

        private void OnDestroy()
        {
            if (this.interrogatorController) this.interrogatorController.onConvictDurationChange -= SetDisplay;

            this.targetText.token = string.Empty;
            this.durationDisplay.SetActive(false);
            GameObject.Destroy(this.durationDisplay);
        }

        private void Update()
        {
            if(targetText.token != string.Empty) { targetText.token = string.Empty; }

            if(this.interrogatorController && this.interrogatorController.convictTimer > 0f)
            {
                float fill = this.interrogatorController.convictTimer;

                if (this.durationBarColor)
                {
                    if (fill >= 1f) this.durationBarColor.fillAmount = 1f;
                    this.durationBarColor.fillAmount = Mathf.Lerp(this.durationBarColor.fillAmount, fill, Time.deltaTime * 2f);
                }

                this.durationBar.fillAmount = fill;
            }
            else if(this.durationDisplay.activeSelf == true && this.interrogatorController.convictTimer <= 0f)
            {
                this.durationDisplay.SetActive(false);
            }
        }

        private void SetDisplay()
        {
            if (this.interrogatorController)
            {
                this.durationDisplay.SetActive(true);
                this.targetText.token = string.Empty;

                this.durationBar.color = InterrogatorAssets.interrogatorColor;
            }
            else
            {
                this.durationDisplay.SetActive(false);
            }
        }
    }
}
*/