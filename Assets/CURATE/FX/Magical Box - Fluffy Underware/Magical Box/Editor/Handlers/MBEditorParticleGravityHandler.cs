// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================

[MBParameterHandler(typeof(MBParticleGravity))]
public class MBEditorParticleGravityHandler : MBEditorParameterHandler
{
    public MBEditorParticleGravityHandler()
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        MBParticleGravity P = Target as MBParticleGravity;
        P.Base = MBGUI.DoVector3Field("Direction", P.Base);
    }
}


