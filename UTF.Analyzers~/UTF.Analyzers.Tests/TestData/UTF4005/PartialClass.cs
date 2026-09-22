using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4005
{
    public partial class PartialClass : MonoBehaviour // UTF4005
    {
    }

    public partial class PartialClass
    {
        private void Awake()
        {
        }
    }
}
