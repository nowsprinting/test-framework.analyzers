// Dummy of UnityEngine.AddComponentMenu. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AddComponentMenu : Attribute
    {
        public AddComponentMenu(string menuName)
        {
        }

        public AddComponentMenu(string menuName, int order)
        {
        }
    }
}
