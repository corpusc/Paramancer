// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleOrientation))]
public class MBEditorParticleOrientationHandler : MBEditorParameterHandler
{

    public override void OnBirthGUI()
    {
        base.OnBirthGUI();
        MBParticleOrientation P = Target as MBParticleOrientation;
        EditorGUILayout.BeginHorizontal();
            P.Offset = MBGUI.DoFloatSlider("Offset", "", P.Offset, -180,180);
            P.Base = MBGUI.DoFloatSlider("Base", "Base Orientation", P.Base, -180, 180);
            P.ArcRange = MBGUI.DoFloatSlider("Random Arc", "Random Offset Range", P.ArcRange, 0, 360);
        EditorGUILayout.EndHorizontal();
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleOrientation P = Target as MBParticleOrientation;
        EditorGUILayout.BeginHorizontal();
            P.Mode = (MBOrientationMode)MBGUI.DoEnumField("Mode", "", P.Mode);
            switch (P.Mode) {
                case MBOrientationMode.Turn:
                    P.FixedTurnSpeed = MBGUI.DoFloatField("Speed", "Turnspeed in angles/second", P.FixedTurnSpeed);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    P.FixedRandomPercent = MBGUI.DoFloatSlider("Random %", "Random deviation from Speed", P.FixedRandomPercent, 0, 1);
                    P.FixedRandomSign = MBGUI.DoToggle("Random Dir", "", P.FixedRandomSign);
                    break;
            }
        EditorGUILayout.EndHorizontal();
    }

}