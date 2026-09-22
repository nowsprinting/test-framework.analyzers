using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4005
{
    [AddComponentMenu("/")]
    public abstract class HiddenBaseOfDerived : MonoBehaviour
    {
    }

    public class DerivedFromHiddenBase : HiddenBaseOfDerived // UTF4005
    {
    }
}
