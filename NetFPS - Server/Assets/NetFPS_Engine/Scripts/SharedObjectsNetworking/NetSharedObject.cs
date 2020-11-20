using System;
using UnityEditor;
using UnityEngine;

public class NetSharedObject : MonoBehaviour
{
    [HideInInspector] public string guid;
    
    //tell the Spawner that you have been enabled
    private void OnEnable()
    {
        NetSharedObjectSpawner.Active.ObjectEnabled(this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NetSharedObject))]
public class NetSharedObject_Editor : Editor
{
    private NetSharedObject Target
    {
        get
        {
            if (_target == null)
                _target = target as NetSharedObject;
            return _target;
        }
    }
    private NetSharedObject _target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //DISPLAY GUID OR MAKE A NEW ONE
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(Target.guid.ToString());
        if (GUILayout.Button("Generate"))
        {
            // Target.guid = Guid.NewGuid().ToString();
            serializedObject.FindProperty("guid").stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
        EditorGUILayout.EndHorizontal();
    }

}
#endif
