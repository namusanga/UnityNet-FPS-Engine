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
}

#if UNITY_EDITOR
public class GameSettingsEditor
{
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
}
#endif
