using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Sway {
    public sealed class SwayCollider : SwayObjectBase {

        internal SwayCollider() {
        }

        public ColliderType Type { get; internal set; }

        public CapsuleAxis Axis { get; internal set; }

        public float Radius { get; internal set; }

        public float Distance { get; internal set; }

        public Vector3 Offset { get; internal set; }

        public PlaneAxis PlaneAxis { get; internal set; }

        public bool CapsuleSphere1 { get; internal set; }

        public bool CapsuleSphere2 { get; internal set; }

        // This should be an enum, but only one value ("All") is observed
        public string ClipPlane { get; internal set; }

        public bool UseSkirtBarrier { get; internal set; }

        public bool PlaneRotationEnabled { get; internal set; }

        /// <summary>
        /// Euler angles, in degrees.
        /// </summary>
        public Vector3 PlaneRotationEuler { get; internal set; }

        [CanBeNull]
        public SwayCollider BridgeTarget { get; internal set; }

        public override string ToString() {
            return $"SwayCollider \"{Path}\"";
        }

        [CanBeNull]
        internal string BridgeTargetPath { get; set; }

    }
}
