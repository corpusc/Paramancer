using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base class for color manipulating parameters
/// </summary>
public class MBParticleColorBase : MBParameter
{
    /// <summary>
    /// Whether particles should autofade using FadeIn and FadeOut
    /// </summary>
    public bool AutoFade;
    /// <summary>
    /// Fade-In time to first key's alpha in percent
    /// </summary>
    public float FadeIn;
    /// <summary>
    /// Fade-Out time from last key's alpha to zero in percent
    /// </summary>
    public float FadeOut;
    /// <summary>
    /// Color keys
    /// </summary>
    public List<ColorKey> Colors = new List<ColorKey>();
    /// <summary>
    /// Whether particles should die automatically when alpha falls below 0
    /// </summary>
    public bool DieOnAlpha;
    
    public override void OnBirth(MBParticle PT)
    {
        DeathReason = MBDeathReason.Color;
        PT.Color = Colors[0].Color;
        PT.mbColor = PT.Color;
    }
    
    public override bool OnLifetime(MBParticle PT)
    {
        // PT.mbColor store's the origional color!
        if (AnimatedLife) {
            float t = 1;
            if (AutoFade) {
                if (FadeIn > 0 && PT.AgePercent < FadeIn) 
                    t = PT.AgePercent / FadeIn;
                else if (FadeOut > 0 && 1 - PT.AgePercent < FadeOut) 
                    t = (1 - PT.AgePercent) / FadeOut;  
            }
            PT.Color = new Color(PT.mbColor.r, PT.mbColor.g, PT.mbColor.b, PT.mbColor.a * t);
        }
        return (!DieOnAlpha || PT.Color.a > 0);
    }
    
    /// <summary>
    /// Gets a color from the gradient
    /// </summary>
    /// <param name="t">time</param>
    /// <param name="keyonly">whether only key colors are returned</param>
    /// <returns>a Color</returns>
    public Color GetGradientColor(float t, bool keyonly)
    {
        for (int k2=1;k2<Colors.Count;k2++) {
            if (t <= Colors[k2].t) {
                int k = k2 - 1;
                if (keyonly)
                    return Colors[k].Color;
                else 
                    return Color.Lerp(Colors[k].Color, Colors[k2].Color, (t - Colors[k].t) / (Colors[k2].t - Colors[k].t));
            }
        }
        return Color.white;
    }
    /// <summary>
    /// Adds a color key
    /// </summary>
    /// <param name="t">time</param>
    /// <param name="color">the color to set at time t</param>
    /// <returns>the new Colorkey</returns>
    public ColorKey AddColorKey(float t, Color color)
    {
        ColorKey k = new ColorKey(t, color);
        Colors.Add(k);
        Colors.Sort();
        return k;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 40;
        Colors.Clear();
        AddColorKey(0,Color.white);
        AutoFade = true;
        FadeIn = 0.15f;
        FadeOut = 0.2f;
        DieOnAlpha = true;
    }

    public override void Validate()
    {
        FadeIn = Mathf.Clamp01(FadeIn);
        FadeOut = Mathf.Clamp01(FadeOut);
        if (FadeIn+FadeOut>1)
            FadeOut = 1 - FadeIn;
        
        foreach (ColorKey k in Colors)
            Mathf.Clamp01(k.t);

        Colors.Sort();
    }

}

/// <summary>
/// A color gradient key
/// </summary>
[System.Serializable]
public class ColorKey : System.IComparable
{
    /// <summary>
    /// Time in percent
    /// </summary>
    public float t;
    /// <summary>
    /// Color of this key
    /// </summary>
    public Color Color;

    public ColorKey(float age, Color color)
    {
        t=age;
        Color=color;
    }

    #region IComparable Member

    public int CompareTo(object obj)
    {
        return -((ColorKey)obj).t.CompareTo(t);
    }

    #endregion
}