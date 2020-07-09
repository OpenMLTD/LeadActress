using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Imas.Live {
    [Serializable]
    public sealed class ScenarioScrObj : ScriptableObject {

        [NotNull, ItemNotNull]
        public EventScenarioData[] scenario = Array.Empty<EventScenarioData>();

        [NotNull, ItemNotNull]
        public TexTargetName[] texs = Array.Empty<TexTargetName>();

        [NotNull]
        public EventScenarioData ap_st = new EventScenarioData();

        [NotNull]
        public EventScenarioData ap_pose = new EventScenarioData();

        [NotNull]
        public EventScenarioData ap_end = new EventScenarioData();

        [CanBeNull]
        public EventScenarioData ap2_st = new EventScenarioData();

        [CanBeNull]
        public EventScenarioData ap2_pose;

        [CanBeNull]
        public EventScenarioData ap2_end;

        [NotNull]
        public EventScenarioData fine_ev;

        // [NotNull, ItemNotNull]
        // public object[] EyeTexMap;

    }
}
