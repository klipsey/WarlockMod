using EntityStates;
using RoR2;
using WarlockMod.Warlock.Components;
using WarlockMod.Warlock.Content;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace WarlockMod.Modules.BaseStates
{
    public abstract class BaseWarlockState : BaseState
    {
        protected WarlockController warlockController;

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
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
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
