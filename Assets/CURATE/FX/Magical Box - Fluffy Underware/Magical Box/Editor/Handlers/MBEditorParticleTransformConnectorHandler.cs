// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;
using UnityEngine;

[MBParameterHandler(typeof(MBParticleTransformConnector))]
public class MBEditorParticleTransformConnectorHandler : MBEditorParameterHandler
{
    public MBEditorParticleTransformConnectorHandler()
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleTransformConnector P = Target as MBParticleTransformConnector;
        P.Source = MBGUI.DoObjectField("Source", "GameObject to instantiate", P.Source, typeof(Transform)) as Transform;
        EditorGUILayout.BeginHorizontal();
        P.SyncPosition = MBGUI.DoToggle("Sync Position", "Synchronize position?", P.SyncPosition);
        P.SyncSize = MBGUI.DoToggle("Sync Size", "Synchronize size?", P.SyncSize);
        P.SyncRotation = (MBParticleTransformConnectorRotationMode)MBGUI.DoEnumField("Sync Rotation", "Rotation synchronizing mode", P.SyncRotation);
        EditorGUILayout.EndHorizontal();
    }

}