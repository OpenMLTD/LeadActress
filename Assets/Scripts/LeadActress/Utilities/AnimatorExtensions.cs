using System;
using JetBrains.Annotations;
using UnityEditor.Animations;
using UnityEngine;

namespace LeadActress.Utilities {
    public static class AnimatorExtensions {

        public static void ResetAllParameters([NotNull] this Animator animator) {
            var controller = animator.runtimeAnimatorController as AnimatorController;

            if (controller == null) {
                return;
            }

            var parameters = controller.parameters;

            foreach (var parameter in parameters) {
                switch (parameter.type) {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameter.name, parameter.defaultFloat);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameter.name, parameter.defaultInt);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameter.name, parameter.defaultBool);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        animator.ResetTrigger(parameter.name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

    }
}
