using System;
using Imas.Live;
using JetBrains.Annotations;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Runtime.Loaders {
    internal static class CommonAnimationControllerBuilder {

        private const string Layer0Name = "No Appeal";

        private const string Layer1Name = "Special Appeal";

        private const string Layer2Name = "Another Appeal";

        private const string Layer3Name = "Gorgeous Appeal";

        private const string DefaultStateName = "Default";

        private const string AppealStateName = "Appeal";

        private const string EnterSpecialAppeal = "EnterSpecialAppeal";

        private const string ExitSpecialAppeal = "ExitSpecialAppeal";

        private const string EnterAnotherAppeal = "EnterAnotherAppeal";

        private const string ExitAnotherAppeal = "ExitAnotherAppeal";

        private const string EnterGorgeousAppeal = "EnterGorgeousAppeal";

        private const string ExitGorgeousAppeal = "ExitGorgeousAppeal";

        // https://docs.unity3d.com/ScriptReference/Animator.PlayInFixedTime.html
        public const string SeekFrameTargetName = Layer0Name + "." + DefaultStateName;

        [NotNull]
        public static AnimatorController BuildAnimationController([NotNull] AnimationGroup group, [NotNull] string controllerName) {
            var controller = new AnimatorController();
            controller.name = controllerName;

            controller.AddLayer(Layer0Name);
            controller.AddLayer(Layer1Name);
            controller.AddLayer(Layer2Name);
            controller.AddLayer(Layer3Name);

            controller.AddParameter(EnterSpecialAppeal, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(ExitSpecialAppeal, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(EnterAnotherAppeal, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(ExitAnotherAppeal, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(EnterGorgeousAppeal, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(ExitGorgeousAppeal, AnimatorControllerParameterType.Trigger);

            var layers = controller.layers;

            {
                var states = layers[0].stateMachine;
                var s0 = states.AddState(DefaultStateName);
                states.defaultState = s0;

                s0.motion = group.mainMotion;
            }

            FillAppealStateMachine(layers[1], EnterSpecialAppeal, ExitSpecialAppeal, group.specialAppeal);
            FillAppealStateMachine(layers[2], EnterAnotherAppeal, ExitAnotherAppeal, group.anotherAppeal);
            FillAppealStateMachine(layers[3], EnterGorgeousAppeal, ExitGorgeousAppeal, group.gorgeousAppeal);

            return controller;
        }

        [NotNull]
        public static string GetEnterTriggerNameFromAppealType(AppealType appealType) {
            switch (appealType) {
                case AppealType.Special:
                    return EnterSpecialAppeal;
                case AppealType.Another:
                    return EnterAnotherAppeal;
                case AppealType.Gorgeous:
                    return EnterGorgeousAppeal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(appealType), appealType, null);
            }
        }

        [NotNull]
        public static string GetExitTriggerNameFromAppealType(AppealType appealType) {
            switch (appealType) {
                case AppealType.Special:
                    return ExitSpecialAppeal;
                case AppealType.Another:
                    return ExitAnotherAppeal;
                case AppealType.Gorgeous:
                    return ExitGorgeousAppeal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(appealType), appealType, null);
            }
        }

        private static void FillAppealStateMachine([NotNull] AnimatorControllerLayer layer, [NotNull] string enterTrigger, [NotNull] string exitTrigger, [CanBeNull] AnimationClip motion) {
            layer.blendingMode = AnimatorLayerBlendingMode.Override;

            var states = layer.stateMachine;

            var s0 = states.AddState(DefaultStateName);
            states.defaultState = s0;
            var s1 = states.AddState(AppealStateName);
            s1.motion = motion;

            var t0 = s0.AddTransition(s1, false);
            t0.hasExitTime = false;
            t0.AddCondition(AnimatorConditionMode.If, 0, enterTrigger);
            var t1 = s1.AddTransition(s0, false);
            t1.hasExitTime = false;
            t1.AddCondition(AnimatorConditionMode.If, 0, exitTrigger);
        }

    }
}
