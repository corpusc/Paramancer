// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleRotation))]
public class MBEditorParticleRotationHandler : MBEditorParameterHandler
{

    public MBEditorParticleRotationHandler()
    {
        HideBirthGUI = true;
    }

    public override void  OnLifetimeGUI()
    {
 	    base.OnLifetimeGUI();
        MBParticleRotation P = Target as MBParticleRotation;
        EditorGUILayout.BeginHorizontal();
            P.Base = MBGUI.DoVector3Field("Base", P.Base);
            P.RandomPercent = MBGUI.DoFloatSlider("Random %", "Random deviation from Base", P.RandomPercent, 0, 1);
        EditorGUILayout.EndHorizontal();
    }

}