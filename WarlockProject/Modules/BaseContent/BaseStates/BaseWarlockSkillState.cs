using EntityStates;
using RoR2;
using WarlockMod.Warlock.Components;
using WarlockMod.Warlock.Content;
using WarlockMod.Warlock.SkillStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace WarlockMod.Modules.BaseStates
{
    public abstract class BaseWarlockSkillState : BaseSkillState
    {
        protected WarlockController warlockController;
        protected Animator modelAnimator;

        protected bool primaryEmpowered;
        protected bool secondaryEmpowered;
        protected bool utilityEmpowered;
        public virtual void AddRecoil2(float x1, float x2, float y1, float y2)
        {
            this.AddRecoil(x1, x2, y1, y2);
        }
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            modelAnimator = GetModelAnimator();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected bool CanPlayGestureAnimation(bool primary)
        {
            var menu = FindSiblingStateMachine("MetaMenu")?.state;
            if (menu is Empower1 || menu is Empower2 || menu is Empower3) return false;
            return !primary || !(FindSiblingStateMachine("Weapon2")?.state is Hex);
        }

        protected int GetAnimationStateHash(string layerName)
        {
            int layer = modelAnimator.GetLayerIndex(layerName);
            return modelAnimator.IsInTransition(layer)
                ? modelAnimator.GetNextAnimatorStateInfo(layer).shortNameHash
                : modelAnimator.GetCurrentAnimatorStateInfo(layer).shortNameHash;
        }

        protected void RefreshState()
        {
            if (!warlockController)
            {
                warlockController = base.GetComponent<WarlockController>();
                primaryEmpowered = warlockController.primaryEmpowered;
                secondaryEmpowered = warlockController.secondaryEmpowered;
                utilityEmpowered = warlockController.utilityEmpowered;
            }
        }
    }
}
