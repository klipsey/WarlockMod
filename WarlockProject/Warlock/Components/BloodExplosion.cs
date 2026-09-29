using UnityEngine;
using RoR2;

namespace WarlockMod.Warlock.Components
{
    public class BloodExplosion : MonoBehaviour
    {
        private CharacterModel model;

        private void Awake()
        {
            var body = GetComponent<CharacterBody>();
            if (body && body.modelLocator && body.modelLocator.modelTransform)
            {
                model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();
                if (model) model.invisibilityCount++;
            }
        }

        private void OnDestroy()
        {
            if (model) model.invisibilityCount--;
        }
    }
}
