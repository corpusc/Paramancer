// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;
using UnityEngine;

[MBParameterHandler(typeof(MBParticleMathOperator))]
public class MBEditorParticleMathOperatorHandler : MBEditorParameterHandler
{
    public MBEditorParticleMathOperatorHandler()
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleMathOperator P = Target as MBParticleMathOperator;
        EditorGUILayout.BeginHorizontal();
        P.Delay = MBGUI.DoFloatField("Delay", "Delay in seconds", P.Delay);
        P.DelayRandomPercent = MBGUI.DoFloatSlider("Random %", "Random deviation from delay", P.DelayRandomPercent,0f,1f);
        P.DelaySync = MBGUI.DoToggle("Synchronize", "Synchronize particles?", P.DelaySync);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        P.Target = (MBParticleMathOperatorTarget)MBGUI.DoEnumField("Target", "Target to apply value to", P.Target);
        P.Operator = (MBParticleMathOperatorOp)MBGUI.DoEnumField("Operator", "How to apply value", P.Operator);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        switch (P.Target) {
            case MBParticleMathOperatorTarget.Size:
                P.Value = MBGUI.DoVector2Field("Value", P.Value);
                break;
            case MBParticleMathOperatorTarget.Acceleration:
            case MBParticleMathOperatorTarget.Friction:
            case MBParticleMathOperatorTarget.Mass:
                P.Value.x=MBGUI.DoFloatField("Value","",P.Value.x);
                break;            
            default:
                P.Value = MBGUI.DoVector3Field("Value", P.Value);
                break;
        }
        
        P.RandomPercent = MBGUI.DoFloatSlider("Random %", "Random deviation from value", P.RandomPercent, 0f, 1f);
        P.RandomSign = MBGUI.DoToggle("Random Sign", "Randomize sign?", P.RandomSign);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (P.Operator == MBParticleMathOperatorOp.SineWave) {
            P.Speed = MBGUI.DoFloatField("Speed", "SinWave speed", P.Speed);
        }
        P.UseDeltaTime = MBGUI.DoToggle("Use DeltaTime", "multiply with deltaTime?", P.UseDeltaTime);
        EditorGUILayout.EndHorizontal();

    }
}