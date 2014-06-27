// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

/// <summary>
/// Inspector class of MBAnchor
/// </summary>
[CustomEditor(typeof(MBLayer))]
public class MBLayerInspector : Editor
{
    void OnEnable()
    {
        MBEditor.Select(Selection.activeGameObject);
    }

    void OnDisable()
    {
        if (Selection.activeGameObject)
            MBEditor.Select(Selection.activeGameObject);
    }

    void OnSceneGUI()
    {
        MBEditorUtility.OnEditorSceneGUI();
    }

}
