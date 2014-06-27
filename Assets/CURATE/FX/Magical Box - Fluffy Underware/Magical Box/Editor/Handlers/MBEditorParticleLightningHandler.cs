// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleLightning))]
public class MBEditorParticleLightningHandler : MBEditorParameterHandler
{
    public MBEditorParticleLightningHandler()
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleLightning P = Target as MBParticleLightning;

        EditorGUILayout.BeginHorizontal();
        P.Speed = MBGUI.DoFloatField("Speed", "Animation Speed", P.Speed);
        P.Amplitude = MBGUI.DoFloatField("Amplitude", "Amplitude Scale", P.Amplitude);
        EditorGUILayout.EndHorizontal();
    }

}