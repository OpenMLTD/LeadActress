using System;
using JetBrains.Annotations;

namespace LeadActress.Runtime.Sway {
    public sealed class SwayController {

        internal SwayController() {
        }

        [NotNull]
        public string Top { get; internal set; } = string.Empty;

        [NotNull, ItemNotNull]
        public SwayManager[] Managers { get; internal set; } = Array.Empty<SwayManager>();

        [NotNull, ItemNotNull]
        public SwayCollider[] Colliders { get; internal set; } = Array.Empty<SwayCollider>();

        [NotNull, ItemNotNull]
        public SwayBone[] SwayBones { get; internal set; } = Array.Empty<SwayBone>();

    }
}
