using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Sway {
    public sealed class SwayManager : SwayObjectBase {

        internal SwayManager() {
            RootBones = Array.Empty<SwayBone>();
            _thresholds = new Dictionary<int, float>();
            _spreadZRotations = new Dictionary<int, float>();
            _pushRatios = new Dictionary<int, IReadOnlyDictionary<int, float>>();
        }

        [NotNull, ItemNotNull]
        public SwayBone[] RootBones { get; internal set; }

        public Vector3 Gravity { get; internal set; }

        public float StiffnessForce { get; internal set; }

        public float DragForce { get; internal set; }

        public float FollowForce { get; internal set; }

        public float LineMoveLimit { get; internal set; }

        public float SideLineMoveLimit { get; internal set; }

        public bool IsSpread { get; internal set; }

        [NotNull]
        public IReadOnlyDictionary<int, float> Thresholds {
            [DebuggerStepThrough]
            get => _thresholds;
        }

        [NotNull]
        public IReadOnlyDictionary<int, float> SpreadZRotations {
            [DebuggerStepThrough]
            get => _spreadZRotations;
        }

        [NotNull]
        public IReadOnlyDictionary<int, IReadOnlyDictionary<int, float>> PushRatios {
            [DebuggerStepThrough]
            get => _pushRatios;
        }

        public override string ToString() {
            return $"SwayManager \"{Path}\"";
        }

        internal void AddThreshold(int key, float value) {
            _thresholds.Add(key, value);
        }

        internal void AddSpreadZRotation(int key, float value) {
            _spreadZRotations.Add(key, value);
        }

        internal void AddPushRatio(int key, int index, float value) {
            Dictionary<int, float> d;

            if (_pushRatios.ContainsKey(key)) {
                d = (Dictionary<int, float>)_pushRatios[key];
            } else {
                d = new Dictionary<int, float>();
                _pushRatios.Add(key, d);
            }

            d.Add(index, value);
        }

        [NotNull]
        internal static readonly SwayManager Dummy = new SwayManager();

        [NotNull]
        private readonly Dictionary<int, float> _thresholds;

        [NotNull]
        private readonly Dictionary<int, float> _spreadZRotations;

        [NotNull]
        private readonly Dictionary<int, IReadOnlyDictionary<int, float>> _pushRatios;

    }
}
