﻿
using System;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public ISingleton[] singletons;
    public static event Action DeathEvent;
    // The original start that controls all other Inits
    void Start()
    {
        Init();

    }
    public override void Init()
    {
        singletons = new ISingleton[12] {
            CameraController._instance,  // alon
             GridManager._instance, // alon
             UIManager._instance, // -----
             PlayerStats._instance, // alon
             InputManager._instance, // rei - V
             PlayerManager._instance, // rei - V
             GodmodeScript._instance, //-----
             CraftingManager._instance, // elor
             InventoryUIManager._instance, // elor
             EffectHandler._instance, // alon
             ConsumeablesHandler._instance, //
             SoundManager._instance

        };

        foreach (ISingleton singleton in singletons)
        {
            if (singleton != null)
            {
                singleton.Init();
            }
        }
    }
    public static void OnDeath()
    {
        DeathEvent?.Invoke();
        Debug.Log("Died!");
    }
}
