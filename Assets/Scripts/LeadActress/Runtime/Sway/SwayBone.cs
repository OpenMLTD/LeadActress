﻿using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Sway {
    public sealed class SwayBone : SwayObjectBase {

        internal SwayBone() {
            Manager = SwayManager.Dummy;
            Children = Array.Empty<SwayBone>();
            Colliders = Array.Empty<SwayCollider>();
        }

        [NotNull]
        public SwayManager Manager { get; internal set; }

        [CanBeNull]
        public SwayBone Parent { get; internal set; }

        [NotNull, ItemNotNull]
        public SwayBone[] Children { get; internal set; }

        public float Radius { get; internal set; }

        public ColliderType Type { get; internal set; }

        public bool IsSkirt { get; internal set; }

        public bool HitMuteForce { get; internal set; }

        public float HitMuteRate { get; internal set; }

        public bool IsZConstraint { get; internal set; }

        public bool HasAngleLimit { get; internal set; }

        public bool UseBindingDirection { get; internal set; }

        /// <summary>
        /// Angle in degrees.
        /// </summary>
        [CanBeNull]
        public float? MinYAngle { get; internal set; }

        /// <summary>
        /// Angle in degrees.
        /// </summary>
        [CanBeNull]
        public float? MaxYAngle { get; internal set; }

        /// <summary>
        /// Angle in degrees.
        /// </summary>
        [CanBeNull]
        public float? MinZAngle { get; internal set; }

        /// <summary>
        /// Angle in degrees.
        /// </summary>
        [CanBeNull]
        public float? MaxZAngle { get; internal set; }

        public float FollowForce { get; internal set; }

        public RefParam RefParam { get; internal set; }

        public float StiffnessForce { get; internal set; }

        public float DragForce { get; internal set; }

        public bool HitTwice { get; internal set; }

        public bool UseLinkHit { get; internal set; }

        public Vector3 Gravity { get; internal set; }

        public Vector3 ColliderOffset { get; internal set; }

        public float SideSpringTolerance { get; internal set; }

        public float SideSpringForce { get; internal set; }

        public float SideAntiSpringTolerance { get; internal set; }

        public float SideAntiSpringForce { get; internal set; }

        public float AntiFoldForceK { get; internal set; }

        public float AntiFoldAngle { get; internal set; }

        [NotNull, ItemNotNull]
        public SwayCollider[] Colliders { get; internal set; }

        [CanBeNull]
        public SwayBone SideLink { get; internal set; }

        public override string ToString() {
            return $"SwayBone \"{Path}\"";
        }

        [CanBeNull, ItemNotNull]
        internal string[] ColliderPaths { get; set; }

        [CanBeNull]
        internal string SideLinkPath { get; set; }

    }
}
