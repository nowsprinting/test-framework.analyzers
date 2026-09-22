using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4005
{
    public enum HelperEnum
    {
        None,
    }

    public class NameMatchesFileAmongOthers : MonoBehaviour // UTF4005
    {
    }

    public class OtherComponentInSameFile : MonoBehaviour
    {
    }
}
