using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Logging/LoggerAsset", menuName = "LoggerData", order = 0)]
public class Logger : SingletonScriptableObject<Logger>
{
    public LogModeOptions logMode;

    public static void Log(LogModeOptions _mode, string _msg)
    {
        if (Instance.logMode >= _mode)
            Debug.Log(_msg);
    }

}

#if UNITY_EDITOR
public class LoggerEditor
{

    /// <summary>
    /// will add a reference to the logger into this scene
    /// </summary>
    [MenuItem("Tools/Create Logger Reference")]
    public static void CreateloggerRefferenceInScene()
    {
        GameObject loggerRef = Object.Instantiate(Resources.Load("LoggerRef") as GameObject);
    }
}
#endif


public enum LogModeOptions
{
    Minimal,
    Partial,
    Full
}