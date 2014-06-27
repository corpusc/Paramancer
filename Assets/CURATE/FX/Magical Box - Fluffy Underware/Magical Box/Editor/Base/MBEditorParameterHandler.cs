// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
/// <summary>
/// Base MBParameter's GUI handler
/// </summary>
public class MBEditorParameterHandler
{
    public MBParameter Target { get; set; }
    public MBParameterInfo ParameterInfo { get; set; }
    /// <summary>
    /// if true, OnBirthGUI won't be called
    /// </summary>
    /// <remarks>Set this to true for Parameters without Birth settings (e.g. Acceleration)</remarks>
    public bool HideBirthGUI { get; set; }
    /// <summary>
    /// If true, the Magical Box window will be repainted
    /// </summary>
    /// <remarks>Use this to force the editor window to repaint when you need it</remarks>
    public bool NeedRepaint;
    
    /// <summary>
    /// Place parameter's initial (+AnimatedBirth) GUI code here
    /// </summary>
    public virtual void OnBirthGUI()
    {
    }
    /// <summary>
    /// Place parameter's AnimatedLifetime GUI code here (if any)
    /// </summary>
    public virtual void OnLifetimeGUI()
    {
    }

    public virtual void OnDestroy()
    {
    }

}
/// <summary>
/// Attribute class to identity a MBParameter's GUI handler
/// </summary>
public class MBParameterHandler : System.Attribute
{
    public System.Type ParameterType;

    public MBParameterHandler(System.Type parameterType)
    {
        ParameterType = parameterType;
    }
}