using FMODUnity;
using UnityEngine;

public class GGJ2025EventEmitter : StudioEventEmitter
{
    public void Trigger() {
        Play();
    }

    public void StopTrigger() {
        Stop();
    }

    public void SetTraveling(float value) {
        SetParameter("travelling", value);
    }
}
