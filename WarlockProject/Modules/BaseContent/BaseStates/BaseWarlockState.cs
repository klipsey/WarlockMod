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
        protected WarlockController interrogatorController;

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RefreshState();
        }
        protected void RefreshState()
        {
            if (!interrogatorController)
            {
                interrogatorController = base.GetComponent<WarlockController>();
            }
        }
    }
}
