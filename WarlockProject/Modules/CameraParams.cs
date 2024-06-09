﻿using RoR2;
using UnityEngine;

internal enum InterrogatorCameraParams
{
    DEFAULT,
    AIM_PISTOL,
    AIM_SNIPER,
    EMOTE
}

namespace WarlockMod.Modules
{
    internal static class CameraParams
    {
        internal static CharacterCameraParamsData defaultCameraParams;
        internal static CharacterCameraParamsData aimCameraParams;
        internal static CharacterCameraParamsData sniperAimCameraParams;
        internal static CharacterCameraParamsData emoteCameraParams;

        internal static void InitializeParams()
        {
            defaultCameraParams = NewCameraParams("ccpRobDriver", 70f, 1.37f, new Vector3(0f, 0f, -8.1f));
            aimCameraParams = NewCameraParams("ccpRobDriverAim", 70f, 0.8f, new Vector3(1f, 0f, -5f));
            sniperAimCameraParams = NewCameraParams("ccpRobDriverSniperAim", 70f, 0.8f, new Vector3(0f, 0f, 0.75f));
            emoteCameraParams = NewCameraParams("ccpRobDriverEmote", 70f, 0.4f, new Vector3(0f, 0f, -6f));
        }

        private static CharacterCameraParamsData NewCameraParams(string name, float pitch, float pivotVerticalOffset, Vector3 standardPosition)
        {
            return NewCameraParams(name, pitch, pivotVerticalOffset, standardPosition, 0.1f);
        }

        private static CharacterCameraParamsData NewCameraParams(string name, float pitch, float pivotVerticalOffset, Vector3 idealPosition, float wallCushion)
        {
            CharacterCameraParamsData newParams = new CharacterCameraParamsData();

            newParams.maxPitch = pitch;
            newParams.minPitch = -pitch;
            newParams.pivotVerticalOffset = pivotVerticalOffset;
            newParams.idealLocalCameraPos = idealPosition;
            newParams.wallCushion = wallCushion;

            return newParams;
        }

        internal static CameraTargetParams.CameraParamsOverrideHandle OverrideCameraParams(CameraTargetParams camParams, InterrogatorCameraParams camera, float transitionDuration = 0.5f)
        {
            CharacterCameraParamsData paramsData = GetNewParams(camera);

            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = paramsData,
                priority = 0,
            };

            return camParams.AddParamsOverride(request, transitionDuration);
        }

        internal static CharacterCameraParams CreateCameraParamsWithData(InterrogatorCameraParams camera)
        {

            CharacterCameraParams newPaladinCameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();

            newPaladinCameraParams.name = camera.ToString().ToLower() + "Params";

            newPaladinCameraParams.data = GetNewParams(camera);

            return newPaladinCameraParams;
        }

        internal static CharacterCameraParamsData GetNewParams(InterrogatorCameraParams camera)
        {
            CharacterCameraParamsData paramsData = defaultCameraParams;

            switch (camera)
            {

                default:
                case InterrogatorCameraParams.DEFAULT:
                    paramsData = defaultCameraParams;
                    break;
                case InterrogatorCameraParams.AIM_PISTOL:
                    paramsData = aimCameraParams;
                    break;
                case InterrogatorCameraParams.AIM_SNIPER:
                    paramsData = sniperAimCameraParams;
                    break;
                case InterrogatorCameraParams.EMOTE:
                    paramsData = emoteCameraParams;
                    break;
            }

            return paramsData;
        }
    }
}