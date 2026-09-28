using System;
using EntityStates;
using RoR2;
using UnityEngine;
using WarlockMod.Warlock.SkillStates;

namespace WarlockMod.Warlock.Components
{
    public sealed class WarlockAnimationController : MonoBehaviour
    {
        private enum BookMode { Unchanged, Closed, Open, Flipping }
        private enum Gesture { None, UseMana, Hex, BlastCharge, BlastFire }

        private static readonly int BookOpen = Animator.StringToHash("bookOpen");
        private static readonly int BookFlipping = Animator.StringToHash("bookFlipping");
        private static readonly int ChargingBlast = Animator.StringToHash("chargingBlast");
        private static readonly int ChargingBlink = Animator.StringToHash("chargingBlink");
        private static readonly int IsBlasting = Animator.StringToHash("isBlasting");
        private static readonly int CreatingMetaMagic = Animator.StringToHash("creatingMetaMagic");
        private static readonly int BlastRate = Animator.StringToHash("Blast.playbackRate");
        private static readonly int HexRate = Animator.StringToHash("Hex.playbackRate");
        private static readonly int ManaRate = Animator.StringToHash("Mana.playbackRate");
        private static readonly int MetaMagicRate = Animator.StringToHash("MetaMagic.playbackRate");
        private static readonly int UseMana = Animator.StringToHash("UseMana");
        private static readonly int HexAnimation = Animator.StringToHash("Hex");
        private static readonly int BlastCharge = Animator.StringToHash("BlastCharge");
        private static readonly int BlastFire = Animator.StringToHash("BlastFire");
        private static readonly int BlastLoop = Animator.StringToHash("BlastLoop");
        private static readonly int CreateMetaMagic = Animator.StringToHash("CreateMetaMagic");
        private static readonly int CreateMetaMagicLoop = Animator.StringToHash("CreateMetaMagicLoop");
        private static readonly int ChargeBlink = Animator.StringToHash("ChargeBlink");
        private static readonly int ChargeBlinkLoop = Animator.StringToHash("ChargeBlinkLoop");
        private static readonly int IdleAnimation = Animator.StringToHash("Idle");
        private static readonly int IdleIn = Animator.StringToHash("IdleIn");
        private static readonly int IdleInSprint = Animator.StringToHash("IdleInSprint");
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int Sprint = Animator.StringToHash("Sprint");
        private static readonly int AscendDescend = Animator.StringToHash("AscendDescend");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int BookClosed = Animator.StringToHash("BookClosed");
        private static readonly int BookClose = Animator.StringToHash("BookClose");
        private static readonly int BufferEmpty = Animator.StringToHash("BufferEmpty");
        private static readonly int GestureUseMana = Animator.StringToHash("Gesture, Override.UseMana");
        private static readonly int GestureHex = Animator.StringToHash("Gesture, Override.Hex");
        private static readonly int GestureBlastCharge = Animator.StringToHash("Gesture, Override.BlastCharge");
        private static readonly int GestureBlastFire = Animator.StringToHash("Gesture, Override.BlastFire");
        private static readonly int GestureBlastLoop = Animator.StringToHash("Gesture, Override.BlastLoop");
        private static readonly int GestureEmpty = Animator.StringToHash("Gesture, Override.BufferEmpty");
        private static readonly int FullBodyMetaMagic = Animator.StringToHash("FullBody, Override.CreateMetaMagic");
        private static readonly int FullBodyMetaMagicLoop = Animator.StringToHash("FullBody, Override.CreateMetaMagicLoop");
        private static readonly int FullBodyChargeBlink = Animator.StringToHash("FullBody, Override.ChargeBlink");
        private static readonly int FullBodyChargeBlinkLoop = Animator.StringToHash("FullBody, Override.ChargeBlinkLoop");
        private static readonly int FullBodyEmpty = Animator.StringToHash("FullBody, Override.BufferEmpty");
        private static readonly int OpenBook = Animator.StringToHash("Book, Override.BookOpen");

        private CharacterBody body;
        private Animator animator;
        private EntityStateMachine primary;
        private EntityStateMachine secondary;
        private EntityStateMachine menu;
        private int bodyLayer;
        private int gestureLayer;
        private int fullBodyLayer;
        private int bookLayer;
        private float blastFireLength;
        private Gesture previousGesture;
        private EntityState previousManaState;
        private CrimsonSurgeFire previousFireState;
        private bool wasConverting;
        private bool charging;
        private bool chargingBlink;
        private bool initialized;

        private void Start()
        {
            body = GetComponent<CharacterBody>();
            var locator = GetComponent<ModelLocator>();
            animator = locator && locator.modelTransform ? locator.modelTransform.GetComponent<Animator>() : null;
            primary = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
            secondary = EntityStateMachine.FindByCustomName(gameObject, "Weapon2");
            menu = EntityStateMachine.FindByCustomName(gameObject, "MetaMenu");
            if (!body || !animator || !primary || !secondary || !menu)
            {
                Log.Error("Warlock animation driver requires its model Animator and Weapon, Weapon2 and MetaMenu state machines.");
                enabled = false;
                return;
            }
            bodyLayer = animator.GetLayerIndex("Body");
            gestureLayer = animator.GetLayerIndex("Gesture, Override");
            fullBodyLayer = animator.GetLayerIndex("FullBody, Override");
            bookLayer = animator.GetLayerIndex("Book, Override");
            if (bodyLayer < 0 || gestureLayer < 0 || fullBodyLayer < 0 || bookLayer < 0 ||
                !HasParameter(BookOpen, AnimatorControllerParameterType.Bool) ||
                !HasParameter(BookFlipping, AnimatorControllerParameterType.Bool) ||
                !HasParameter(ChargingBlast, AnimatorControllerParameterType.Bool) ||
                !HasParameter(ChargingBlink, AnimatorControllerParameterType.Bool) ||
                !HasParameter(IsBlasting, AnimatorControllerParameterType.Bool) ||
                !HasParameter(CreatingMetaMagic, AnimatorControllerParameterType.Bool) ||
                !HasParameter(BlastRate, AnimatorControllerParameterType.Float) ||
                !HasParameter(HexRate, AnimatorControllerParameterType.Float) ||
                !HasParameter(ManaRate, AnimatorControllerParameterType.Float) ||
                !HasParameter(MetaMagicRate, AnimatorControllerParameterType.Float) ||
                !animator.HasState(gestureLayer, GestureUseMana) || !animator.HasState(gestureLayer, GestureHex) ||
                !animator.HasState(gestureLayer, GestureBlastCharge) || !animator.HasState(gestureLayer, GestureBlastFire) ||
                !animator.HasState(gestureLayer, GestureBlastLoop) || !animator.HasState(fullBodyLayer, FullBodyMetaMagicLoop) ||
                !animator.HasState(gestureLayer, GestureEmpty) || !animator.HasState(fullBodyLayer, FullBodyMetaMagic) ||
                !animator.HasState(fullBodyLayer, FullBodyChargeBlink) || !animator.HasState(fullBodyLayer, FullBodyChargeBlinkLoop) ||
                !animator.HasState(fullBodyLayer, FullBodyEmpty) ||
                !animator.HasState(bookLayer, OpenBook))
            {
                Log.Error("Warlock's animator is missing the configured book/skill parameters or animation states. Rebuild the Unity bundle and DLL.");
                enabled = false;
                return;
            }
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name == "root|BlastFire") blastFireLength = clip.length;
            if (blastFireLength <= 0f)
            {
                Log.Error("Warlock's animator is missing the root|BlastFire clip.");
                enabled = false;
                return;
            }
            initialized = true;
            ResetControls();
        }

        private bool HasParameter(int hash, AnimatorControllerParameterType type)
        {
            foreach (var parameter in animator.parameters)
                if (parameter.nameHash == hash && parameter.type == type) return true;
            return false;
        }

        private void Update()
        {
            if (!initialized) return;
            if (!animator.isActiveAndEnabled || !body.healthComponent || !body.healthComponent.alive)
            {
                ResetControls();
                return;
            }

            bool nextCharging = primary.state is CrimsonSurgePrep;
            SetBool(ChargingBlast, nextCharging);
            if (charging && !nextCharging && !(primary.state is CrimsonSurgeFire) &&
                EffectiveState(gestureLayer) == BlastCharge)
                animator.CrossFadeInFixedTime(GestureEmpty, 0.05f, gestureLayer);
            charging = nextCharging;

            bool nextChargingBlink = secondary.state is BloodDashPrep;
            SetBool(ChargingBlink, nextChargingBlink);
            if (nextChargingBlink && !chargingBlink)
                animator.CrossFadeInFixedTime(FullBodyChargeBlink, 0.05f, fullBodyLayer, 0f);
            else if (chargingBlink && !nextChargingBlink && EffectiveState(fullBodyLayer) == ChargeBlink)
                animator.CrossFadeInFixedTime(FullBodyEmpty, 0.05f, fullBodyLayer);
            chargingBlink = nextChargingBlink;

            var empower = menu.state as Empower;
            bool nextCreating = !nextChargingBlink && empower != null && empower.IsCreatingMetaMagic;
            SetBool(CreatingMetaMagic, nextCreating);
            if (!nextChargingBlink && empower != null && !wasConverting)
                Play(FullBodyMetaMagic, fullBodyLayer, MetaMagicRate, body.attackSpeed);
            wasConverting = !nextChargingBlink && empower != null;

            var fire = primary.state as CrimsonSurgeFire;
            SetBlasting((primary.state is CrimsonSurgePrep prep && prep.HasAppliedMetamagic) ||
                (fire != null && fire.IsBlasting));
            bool usingMana = menu.state is Empower1 || menu.state is Empower2 || menu.state is Empower3;
            Gesture gesture = usingMana ? Gesture.UseMana : secondary.state is Hex ? Gesture.Hex :
                nextCharging ? Gesture.BlastCharge : fire != null ? Gesture.BlastFire : Gesture.None;
            switch (gesture)
            {
                case Gesture.UseMana:
                    if (previousGesture != gesture || previousManaState != menu.state)
                        Play(GestureUseMana, gestureLayer, ManaRate, body.attackSpeed);
                    break;
                case Gesture.Hex:
                    if (previousGesture != gesture || EffectiveState(gestureLayer) != HexAnimation)
                        Play(GestureHex, gestureLayer, HexRate, body.attackSpeed);
                    break;
                case Gesture.BlastCharge:
                    if (previousGesture != gesture || EffectiveState(gestureLayer) != BlastCharge)
                        Play(GestureBlastCharge, gestureLayer, BlastRate, body.attackSpeed);
                    break;
                case Gesture.BlastFire:
                    if (previousGesture != gesture || previousFireState != fire)
                        Play(GestureBlastFire, gestureLayer, BlastRate, blastFireLength / Mathf.Max(0.01f, fire.ShotInterval));
                    break;
            }
            previousGesture = gesture;
            previousManaState = menu.state;
            previousFireState = fire;

            BookMode mode;
            int fullBodyState = EffectiveState(fullBodyLayer);
            if (nextChargingBlink || nextCreating || fullBodyState == CreateMetaMagic || fullBodyState == CreateMetaMagicLoop)
                mode = BookMode.Flipping;
            else if (gesture != Gesture.None)
                mode = gesture == Gesture.UseMana || gesture == Gesture.BlastCharge ? BookMode.Flipping : BookMode.Open;
            else
            {
                mode = GetBookMode(EffectiveState(gestureLayer));
                if (mode == BookMode.Unchanged) mode = GetBookMode(EffectiveState(bodyLayer));
            }
            bool open = menu.state is RitualPrep || empower != null ||
                (mode == BookMode.Unchanged ? animator.GetBool(BookOpen) : mode != BookMode.Closed);
            bool flipping = mode == BookMode.Flipping;
            SetBool(BookOpen, open);
            SetBool(BookFlipping, flipping);
            int bookState = EffectiveState(bookLayer);
            // The authored closed states only open when flipping is false, so enter BookOpen before flipping.
            if (flipping && (bookState == BookClosed || bookState == BookClose || bookState == BufferEmpty))
                animator.CrossFadeInFixedTime(OpenBook, 0.05f, bookLayer);
        }

        private int EffectiveState(int layer) => animator.IsInTransition(layer)
            ? animator.GetNextAnimatorStateInfo(layer).shortNameHash
            : animator.GetCurrentAnimatorStateInfo(layer).shortNameHash;

        private static BookMode GetBookMode(int state)
        {
            if (state == UseMana || state == BlastCharge || state == ChargeBlink || state == ChargeBlinkLoop ||
                state == CreateMetaMagic || state == CreateMetaMagicLoop)
                return BookMode.Flipping;
            if (state == HexAnimation || state == BlastFire || state == BlastLoop || state == IdleAnimation || state == IdleIn || state == IdleInSprint)
                return BookMode.Open;
            if (state == Run || state == Sprint || state == AscendDescend || state == Jump) return BookMode.Closed;
            return BookMode.Unchanged;
        }

        private void Play(int state, int layer, int rateParameter, float rate)
        {
            animator.SetFloat(rateParameter, Mathf.Max(0.01f, rate));
            animator.CrossFadeInFixedTime(state, 0.05f, layer, 0f);
        }

        private void SetBool(int hash, bool value)
        {
            if (animator.GetBool(hash) != value) animator.SetBool(hash, value);
        }

        internal void SetBlasting(bool value)
        {
            if (initialized && isActiveAndEnabled && animator) SetBool(IsBlasting, value);
        }

        private void ResetControls()
        {
            SetBool(ChargingBlast, false);
            SetBool(ChargingBlink, false);
            SetBool(IsBlasting, false);
            SetBool(CreatingMetaMagic, false);
            SetBool(BookFlipping, false);
            previousGesture = Gesture.None;
            previousManaState = null;
            previousFireState = null;
            wasConverting = charging = chargingBlink = false;
        }

        private void OnDisable()
        {
            if (initialized && animator) ResetControls();
        }
    }
}
