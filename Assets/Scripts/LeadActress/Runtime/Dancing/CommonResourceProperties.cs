using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    public sealed class CommonResourceProperties : ScriptableObject {

        public string songResourceName {
            get => _songResourceName;
            set => _songResourceName = value;
        }

        [SerializeField]
        private string _songResourceName = "shtstr";

    }
}
