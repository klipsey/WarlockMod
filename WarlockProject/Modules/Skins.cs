using RoR2;
using System;
using UnityEngine;

namespace WarlockMod.Modules
{
    internal static class Skins
    {
        internal static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] defaultRendererInfos, GameObject root, UnlockableDef unlockableDef = null)
        {
            var parameters = ScriptableObject.CreateInstance<SkinDefParams>();
            parameters.name = "Warlock" + skinName + "Params";
            parameters.rendererInfos = (CharacterModel.RendererInfo[])defaultRendererInfos.Clone();
            var skin = ScriptableObject.CreateInstance<SkinDef>();
            skin.name = "Warlock" + skinName;
            skin.nameToken = skinName;
            skin.icon = skinIcon;
            skin.rootObject = root;
            skin.unlockableDef = unlockableDef;
            skin.baseSkins = Array.Empty<SkinDef>();
            skin.skinDefParams = parameters;
            return skin;
        }
    }
}
