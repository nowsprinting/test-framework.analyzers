using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4005
{
    public partial class HiddenOnOtherPartialDeclaration : MonoBehaviour
    {
    }

    [AddComponentMenu("/")]
    public partial class HiddenOnOtherPartialDeclaration
    {
    }
}
