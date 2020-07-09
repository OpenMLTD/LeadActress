using System;
using Imas.Live;
using LeadActress.Runtime.Loaders;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/Scenario Event Signal")]
    public class ScenarioEventSignal : MonoBehaviour {

        public ScenarioLoader scenarioLoader;

        public PlayerControl playerControl;

        public event EventHandler<ScenarioSignalEventArgs> EventEmitted;

        private async void Start() {
            var scenarioData = await scenarioLoader.LoadAsync();
            _events = scenarioData.scenario;
            _variation = scenarioLoader.variation;
        }

        private void Update() {
            if (playerControl.isOnPlaying) {
                InitStates();
            } else if (playerControl.isOnStopping) {
                ResetStates();
            }

            if (!playerControl.isPlaying) {
                return;
            }

            var currentTime = playerControl.relativeTime;

            while (_eventIndex < _events.Length) {
                var ev = _events[_eventIndex];

                if (currentTime >= ev.absTime) {
                    EventEmitted?.Invoke(this, new ScenarioSignalEventArgs(ev, _variation));
                    _eventIndex += 1;
                } else {
                    break;
                }
            }
        }

        private void InitStates() {
            _eventIndex = 0;
        }

        private void ResetStates() {
            _eventIndex = 0;
        }

        private EventScenarioData[] _events;

        private int _eventIndex;

        private ScenarioVariation _variation;

    }
}
