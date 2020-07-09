using JetBrains.Annotations;

namespace LeadActress.Runtime.Sway {
    internal static class SwayBoneExtensions {

        public static bool IsSemiRootBone([NotNull] this SwayBone bone) {
            return bone.Parent == null;
        }

    }
}
