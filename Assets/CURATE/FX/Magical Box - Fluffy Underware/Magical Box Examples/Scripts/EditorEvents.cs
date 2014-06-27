using UnityEngine;
using System.Collections;
/*
 * Some example callbacks that can be used for particle events
 * 
 */

public class EditorEvents : MonoBehaviour {

    public void ColorParticleRed(MBEvent e)
    {
        e.Particle.Color = Color.red;
    }

    public void ColorParticleWhite(MBEvent e)
    {
        e.Particle.Color = Color.white;
    }

    public void Grow(MBEvent e)
    {
        float f=2*e.Particle.ParticleSystem.DeltaTime;
        e.Particle.Scale += new Vector3(f, f, f);
    }
    
}
