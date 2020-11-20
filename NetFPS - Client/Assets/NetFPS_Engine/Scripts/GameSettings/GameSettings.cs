using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameSettings", menuName = "GameSettingsData")]
public class GameSettings : SingletonScriptableObject<GameSettings>
{
    /// <summary>
    /// these layers allow players to stand on them
    /// </summary>
    public LayerMask groundingLayers;
    /// <summary>
    /// how far from player center to check for the ground
    /// </summary>
    public float groundCheckDistance;

    public float gravity;

    /// <summary>
    /// should we start the game in offline mode
    /// </summary>
    [Tooltip("Should the game start in offline mode")]
    public bool offline = false;

    [Tooltip("Tell game manager not to instantiate a locap player")]
    public bool instantiateLocalPlayer;
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    /// <summary>
    /// will add a reference to the logger into this scene
    /// </summary>
    [MenuItem("Tools/Create GameSettings Reference")]
    public static void CreateloggerRefferenceInScene()
    {
        //create the holder if none
        if (GameObject.Find("SinglatonsHolder") == null)
            new GameObject("SinglatonsHolder");

        GameObject singlatonHolders = GameObject.Find("SinglatonsHolder");
        GameObject loggerRef = Object.Instantiate(Resources.Load("GameSettingsRef") as GameObject, singlatonHolders.transform);
        loggerRef.name = "GameSettingsRef";
    }

    /// <summary>
    /// will select the main game settings object
    /// </summary>
    [MenuItem("Tools/Select GameSettings Reference")]
    public static void SelectGameSettingsRef()
    {
        Selection.activeObject = GameSettings.Instance;
    }
}
#endif
