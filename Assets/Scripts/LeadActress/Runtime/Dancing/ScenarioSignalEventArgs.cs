using System;
using Imas.Live;
using JetBrains.Annotations;

namespace LeadActress.Runtime.Dancing {
    public sealed class ScenarioSignalEventArgs : EventArgs {

        internal ScenarioSignalEventArgs([NotNull] EventScenarioData data, ScenarioVariation variation) {
            Data = data;
            Variation = variation;
        }

        [NotNull]
        public EventScenarioData Data { get; }

        public ScenarioVariation Variation { get; }

    }
}
