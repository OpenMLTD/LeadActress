using JetBrains.Annotations;
using UnityEditor.Animations;

namespace LeadActress.Runtime.Loaders {
    internal static class CommonAnimationControllerBuilder {

        [NotNull]
        public static AnimatorController BuildAnimationController([NotNull] AnimationGroup group, [NotNull] string controllerName) {
            var controller = new AnimatorController();
            controller.name = controllerName;

            controller.AddLayer("No Appeal");
            controller.AddLayer("Special Appeal");
            controller.AddLayer("Another Appeal");
            controller.AddLayer("Gorgeous Appeal");

            var layers = controller.layers;

            {
                var layer = layers[0];
                var states = layer.stateMachine;
                var s0 = states.AddState("Default");
                states.defaultState = s0;

                s0.motion = group.mainMotion;
            }

            return controller;
        }

    }
}
