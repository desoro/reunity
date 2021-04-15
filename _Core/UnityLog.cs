using UnityEngine;

namespace Phuntasia
{
    public class UnityLog
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            SetLogger();
        }

        public static void SetLogger()
        {
            LogManager.Verbose = Debug.Log;
            LogManager.Info = Debug.Log;
            LogManager.Warn = Debug.LogWarning;
            LogManager.Error = Debug.LogError;
            LogManager.Exception = Debug.LogException;
        }
    }
}