using Imas.Live;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    public sealed class CommonResourceProperties : ScriptableObject {

        public string songResourceName {
            get => _songResourceName;
            set => _songResourceName = value;
        }

        public AppealType appealType {
            get => _appealType;
            set => _appealType = value;
        }

        [SerializeField]
        private string _songResourceName = "shtstr";

        [SerializeField]
        private AppealType _appealType = AppealType.None;

    }
}
