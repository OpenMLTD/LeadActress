using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    public enum CameraControlMode {

        /// <summary>
        /// Applies the animation curve to <see cref="Camera"/> directly.
        /// </summary>
        Direct = 0,

        /// <summary>
        /// Applies the animation curve to <see cref="IndirectCamera"/>.
        /// </summary>
        /// <remarks>
        /// Warning: using this mode may cause small time point mismatches with dance animations.
        /// </remarks>
        Indirect = 1,

    }
}
