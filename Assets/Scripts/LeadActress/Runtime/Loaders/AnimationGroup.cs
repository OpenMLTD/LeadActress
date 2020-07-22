using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    internal sealed class AnimationGroup {

        public AnimationGroup([NotNull] AnimationClip mainMotion, [CanBeNull] AnimationClip specialAppeal, [CanBeNull] AnimationClip anotherAppeal, [CanBeNull] AnimationClip gorgeousAppeal) {
            this.mainMotion = mainMotion;
            this.specialAppeal = specialAppeal;
            this.anotherAppeal = anotherAppeal;
            this.gorgeousAppeal = gorgeousAppeal;
        }

        [NotNull]
        public readonly AnimationClip mainMotion;

        [CanBeNull]
        public readonly AnimationClip specialAppeal;

        [CanBeNull]
        public readonly AnimationClip anotherAppeal;

        [CanBeNull]
        public readonly AnimationClip gorgeousAppeal;

    }
}
