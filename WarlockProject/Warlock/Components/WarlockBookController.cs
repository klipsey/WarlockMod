using RoR2;
using UnityEngine;
using WarlockMod.Warlock.SkillStates;

namespace WarlockMod.Warlock.Components
{
    public sealed class WarlockBookController : MonoBehaviour
    {
        private enum BookMode { Unchanged, Closed, Open, Flipping }

        private static readonly int BookOpen = Animator.StringToHash("bookOpen");
        private static readonly int BookFlipping = Animator.StringToHash("bookFlipping");
        private static readonly int UseMana = Animator.StringToHash("UseMana");
        private static readonly int Hex = Animator.StringToHash("Hex");
        private static readonly int BlastCharge = Animator.StringToHash("BlastCharge");
        private static readonly int BlastFire = Animator.StringToHash("BlastFire");
        private static readonly int BlastLoop = Animator.StringToHash("BlastLoop");
        private static readonly int CreateMetaMagic = Animator.StringToHash("CreateMetaMagic");
        private static readonly int CreateMetaMagicLoop = Animator.StringToHash("CreateMetaMagicLoop");
        private static readonly int ChargeBlink = Animator.StringToHash("ChargeBlink");
        private static readonly int ChargeBlinkLoop = Animator.StringToHash("ChargeBlinkLoop");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int IdleIn = Animator.StringToHash("IdleIn");
        private static readonly int IdleInSprint = Animator.StringToHash("IdleInSprint");
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int Sprint = Animator.StringToHash("Sprint");
        private static readonly int AscendDescend = Animator.StringToHash("AscendDescend");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int BookClosed = Animator.StringToHash("BookClosed");
        private static readonly int BookClose = Animator.StringToHash("BookClose");
        private static readonly int BufferEmpty = Animator.StringToHash("BufferEmpty");
        private static readonly int OpenBook = Animator.StringToHash("Book, Override.BookOpen");

        private CharacterBody body;
        private Animator animator;
        private EntityStateMachine menu;
        private int bodyLayer;
        private int gestureLayer;
        private int fullBodyLayer;
        private int bookLayer;

        private void Start()
        {
            body = GetComponent<CharacterBody>();
            var locator = GetComponent<ModelLocator>();
            animator = locator && locator.modelTransform ? locator.modelTransform.GetComponent<Animator>() : null;
            menu = EntityStateMachine.FindByCustomName(gameObject, "MetaMenu");
            if (!body || !animator || !menu)
            {
                Log.Error("Warlock book controller requires its CharacterBody, model Animator and MetaMenu state machine.");
                enabled = false;
                return;
            }
            bodyLayer = animator.GetLayerIndex("Body");
            gestureLayer = animator.GetLayerIndex("Gesture, Override");
            fullBodyLayer = animator.GetLayerIndex("FullBody, Override");
            bookLayer = animator.GetLayerIndex("Book, Override");
            if (bodyLayer < 0 || gestureLayer < 0 || fullBodyLayer < 0 || bookLayer < 0 ||
                !animator.HasState(bookLayer, OpenBook))
            {
                Log.Error("Warlock's animator is missing its configured book layers or BookOpen state.");
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!animator || !body || !menu) return;
            if (!animator.isActiveAndEnabled || !body.healthComponent || !body.healthComponent.alive)
            {
                SetBool(BookFlipping, false);
                return;
            }

            int fullBodyState = EffectiveState(fullBodyLayer);
            BookMode mode = GetBookMode(fullBodyState);
            if (mode == BookMode.Unchanged) mode = GetBookMode(EffectiveState(gestureLayer));
            if (mode == BookMode.Unchanged) mode = GetBookMode(EffectiveState(bodyLayer));
            bool open = menu.state is RitualPrep || menu.state is Empower ||
                (mode == BookMode.Unchanged ? animator.GetBool(BookOpen) : mode != BookMode.Closed);
            bool flipping = mode == BookMode.Flipping;
            SetBool(BookOpen, open);
            SetBool(BookFlipping, flipping);
            int bookState = EffectiveState(bookLayer);
            // Closed states cannot open while flipping, so enter BookOpen explicitly.
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
            if (state == Hex || state == BlastFire || state == BlastLoop || state == Idle || state == IdleIn || state == IdleInSprint)
                return BookMode.Open;
            if (state == Run || state == Sprint || state == AscendDescend || state == Jump) return BookMode.Closed;
            return BookMode.Unchanged;
        }

        private void SetBool(int hash, bool value)
        {
            if (animator.GetBool(hash) != value) animator.SetBool(hash, value);
        }

        private void OnDisable()
        {
            if (animator) SetBool(BookFlipping, false);
        }
    }
}
