// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

public class MBEditorParticleZoneBaseHandler : MBEditorParameterHandler
{
    public MBEditorParticleZoneBaseHandler()
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        MBParticleZoneBase P = Target as MBParticleZoneBase;
        P.ParticleEnteringZoneSM = MBGUI.DoEditorEvent("Particle entering", "Particle Entering Zone Event Target", P.ParticleEnteringZoneSM);
        P.ParticleInsideZoneSM = MBGUI.DoEditorEvent("Particle inside", "Particle Inside Zone Event Target", P.ParticleInsideZoneSM);
        P.ParticleLeavingZoneSM = MBGUI.DoEditorEvent("Particle leaving", "Particle Leaving Zone Event Target", P.ParticleLeavingZoneSM);
        if (P.SupportsWorldSpace) {
            EditorGUILayout.BeginHorizontal();
            P.WorldSpace = MBGUI.DoToggle("World Space", "Use world space or local space?", P.WorldSpace);
            Vector3 tempPos = MBGUI.DoVector3Field("Position", P.Transform.localPosition);
            if (MBGUI.HasChanged)
                P.Transform.localPosition = tempPos;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
            P.Mode = (MBParticleZoneMode)MBGUI.DoEnumField("Mode", "Mode", P.Mode);
            if (P.Mode==MBParticleZoneMode.Attract)
                P.Attraction = MBGUI.DoFloatField("Attraction", "", P.Attraction);
        EditorGUILayout.EndHorizontal();
    }

}

[MBParameterHandler(typeof(MBParticleRadialForce))]
public class MBEditorParticleRadialForceHandler : MBEditorParticleZoneBaseHandler
{

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleRadialForce P = Target as MBParticleRadialForce;
        EditorGUILayout.BeginHorizontal();
            P.Radius = MBGUI.DoFloatField("Radius", "0=Infinite", P.Radius);
            P.Hollow = MBGUI.DoFloatSlider("Hollow", "Cut out in percent", P.Hollow,0,1);
        EditorGUILayout.EndHorizontal();
        P.Falloff = MBGUI.DoFloatSlider("Falloff", "0=Linear", P.Falloff, 0, 1);
    }
}

[MBParameterHandler(typeof(MBParticleSphericalForce))]
public class MBEditorParticleSphericalForceHandler : MBEditorParticleRadialForceHandler
{
}

[MBParameterHandler(typeof(MBParticleRectForce))]
public class MBEditorParticleRectForceHandler : MBEditorParticleZoneBaseHandler
{
    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleRectForce P = Target as MBParticleRectForce;
        P.Scale = MBGUI.DoVector2Field("Scale", P.Scale);
    }
}