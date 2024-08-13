using System.Collections;
using UnityEngine;

namespace Events.AbilitySystem
{
    public class AttributeManagerEvents
    {
        public delegate void PreAttributeDelegate(Attribute attribute, ref float newValue);
        public delegate void PostAttributeDelegate(Attribute attribute);
        public static PreAttributeDelegate OnPreAttributeChange;
        public static PostAttributeDelegate OnPostAttributeChange;
    }
}