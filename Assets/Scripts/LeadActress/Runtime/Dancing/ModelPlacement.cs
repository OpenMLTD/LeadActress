using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    [AddComponentMenu("MLTD/Model Placement")]
    public class ModelPlacement : MonoBehaviour {

        public int motionNumber {
            get => _motionNumber;
            set => _motionNumber = value;
        }

        public int formationNumber {
            get => _formationNumber;
            set => _formationNumber = value;
        }

        [Tooltip("Which dance animation does this idol use.")]
        [SerializeField]
        [Range(MltdSimulationConstants.MinDanceMotion, MltdSimulationConstants.MaxDanceMotion)]
        private int _motionNumber = MltdSimulationConstants.MinDanceMotion;

        [Tooltip("Which position does this idol stand.")]
        [SerializeField]
        [Range(MltdSimulationConstants.MinDanceFormation, MltdSimulationConstants.MaxDanceFormation)]
        private int _formationNumber = MltdSimulationConstants.MinDanceFormation;

    }
}
