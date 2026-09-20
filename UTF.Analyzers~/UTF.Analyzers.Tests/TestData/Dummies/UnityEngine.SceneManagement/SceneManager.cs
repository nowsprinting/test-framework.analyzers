// Dummy of UnityEngine.SceneManagement.SceneManager. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine.SceneManagement
{
    public static class SceneManager
    {
        public static AsyncOperation UnloadSceneAsync(string sceneName)
        {
            throw new NotImplementedException();
        }
    }
}
