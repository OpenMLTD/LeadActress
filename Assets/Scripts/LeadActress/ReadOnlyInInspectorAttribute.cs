using System;
using UnityEngine;

namespace LeadActress {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ReadOnlyInInspectorAttribute : PropertyAttribute {

    }
}
