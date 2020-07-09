using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LeadActress.Runtime.Sway {
    internal static class SwayControllerExtensions {

        [CanBeNull]
        public static SwayBone FindBoneByPath([NotNull] this SwayController controller, [NotNull] string path) {
            foreach (var bone in controller.SwayBones) {
                if (bone.Path == path) {
                    return bone;
                }
            }

            return null;
        }

        [NotNull]
        public static SwayManager FindManagerByPath([NotNull] this SwayController controller, [NotNull] string path) {
            var results = new List<SwayManager>();

            foreach (var manager in controller.Managers) {
                if (path.StartsWith(manager.Path)) {
                    results.Add(manager);
                }
            }

            if (results.Count == 0) {
                throw new ArgumentException("The given path is not in this sway controller.");
            }

            if (results.Count > 0) {
                results.Sort(CompareByPathLengthReversed);
            }

            return results[0];
        }

        private static int CompareByPathLengthReversed([NotNull] SwayManager v1, [NotNull] SwayManager v2) {
            return -v1.Path.Length.CompareTo(v2.Path.Length);
        }

    }
}
