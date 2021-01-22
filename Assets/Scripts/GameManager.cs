﻿public class GameManager : MonoSingleton<GameManager>
{
    public ISingleton[] singletons;

    // The original start that controls all other Inits
    void Start() {
        Init();

    }
    public override void Init() {
        singletons = new ISingleton[12] {
            CameraController._instance,
            PlayerMovementHandler._instance,
             GridManager._instance,
             UIManager._instance,
             PlayerStats._instance,
             InputManager._instance,
             PlayerManager._instance,
             GodmodeScript._instance,
             CraftingManager._instance,
             InventoryUIManager._instance,
             EffectHandler._instance,
             ConsumeablesHandler._instance

        };

        foreach (ISingleton singleton in singletons) {
            if (singleton != null) {
                singleton.Init();
            }
        }
    }
}
