using UnityEngine;

namespace WarlockMod.Warlock.Compatibility
{
    public sealed class EmotePropVisibility : MonoBehaviour
    {
        public Renderer book;
        public Renderer dagger;
        private bool hidden;
        private bool bookWasHidden;
        private bool daggerWasHidden;

        internal void SetEmoting(bool emoting)
        {
            if (hidden == emoting) return;
            if (emoting)
            {
                bookWasHidden = book && book.forceRenderingOff;
                daggerWasHidden = dagger && dagger.forceRenderingOff;
                if (book) book.forceRenderingOff = true;
                if (dagger) dagger.forceRenderingOff = true;
            }
            else
            {
                if (book) book.forceRenderingOff = bookWasHidden;
                if (dagger) dagger.forceRenderingOff = daggerWasHidden;
            }
            hidden = emoting;
        }

        private void OnDisable() => SetEmoting(false);
        private void OnDestroy() => SetEmoting(false);
    }
}
