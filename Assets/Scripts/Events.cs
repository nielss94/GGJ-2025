using System;
using UnityEngine;

public class Events : MonoBehaviour
{
    public static event Action OnRebirth;
    public static event Action<float> OnEggSpeed;

    public static void Rebirth()
    {
        OnRebirth?.Invoke();
    }

    public static void EggSpeed(float speed)
    {
        OnEggSpeed?.Invoke(speed);
    }

}
