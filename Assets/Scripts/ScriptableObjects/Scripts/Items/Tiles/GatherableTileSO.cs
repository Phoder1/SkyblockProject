﻿using Assets.TimeEvents;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Gatherable Tile", menuName = "SO/" + "Tiles/" + "Gatehrable", order = 1)]
public class GatherableTileSO : TileAbstSO
{
    [SerializeField] private GrowthStage[] stages;
    public GrowthStage[] GetStages => stages;
    [SerializeField] private ToolType toolType;
    public ToolType GetToolType => toolType;


    [SerializeField] private float minGrowTime;
    [SerializeField] private float maxGrowTime;
    [SerializeField] private float GatheringTime;
    public float GetMinGrowTime => minGrowTime;
    public float GetMaxGrowTime => maxGrowTime;
    public float GetGatheringTime => GatheringTime;
}
[System.Serializable]
public class GrowthStage
{
    [SerializeField] private TileBase stageTile;
    [SerializeField] private bool isGatherable;
    [SerializeField] private Drop[] drops;

    public TileBase GetStageTile => stageTile;
    public bool GetIsGatherable => isGatherable;
    public Drop[] GetDrops => drops;
}
[System.Serializable]
public class Drop
{
    [SerializeField] ItemSO item;
    [SerializeField] float Chance;
    [SerializeField] int minAmount;
    [SerializeField] int maxAmount;

    public ItemSO GetItem => item;
    public float GetChance => Chance;
    public int GetMinAmount => minAmount;
    public int GetMaxAmount => maxAmount;
}
public class GatherableState : ITileState
{
    public TileSlot tileSlot;
    public TimeEvent eventInstance;
    public GatherableTileSO tile;
    public int currentStage = 0;
    public int StagesCount => tile.GetStages.Length;
    public bool reachedMaxStage => currentStage >= StagesCount - 1;

    public GatherableState(GatherableTileSO tile, TileSlot tileSlot) {
        currentStage = 0;
        this.tile = tile;
        this.tileSlot = tileSlot;
    }
    public TileBase GetMainTileBase {
        get {
            if (tile.GetStages != null)
                return tile.GetStages[currentStage].GetStageTile;
            return tile.GetMainTileBase;
        }

    }
    public TileAbstSO GetTileAbst => tile;

    public ToolType GetToolType => tile.GetToolType;

    public TileType GetTileType => tile.GetTileType;

    public bool GetIsSolid => tile.GetIsSolid;
    public float GetGatherTime => GetGatherTime;

    public bool isSpecialInteraction => tile.isSpecialInteraction;
    public bool GetIsGatherable => tile.GetStages[currentStage].GetIsGatherable;

    public void GatherInteraction(Vector2Int gridPosition, TileMapLayer tileMapLayer) {
        if (reachedMaxStage) {
            Debug.Log("Tried gathering");
            GridManager._instance.SetTile(null, gridPosition, tileMapLayer, true);
            Inventory inventory = Inventory.GetInstance;
            foreach (Drop drop in tile.GetStages[currentStage].GetDrops) {
                if (Random.value <= drop.GetChance) {
                    inventory.AddToInventory(0, new ItemSlot(drop.GetItem, Random.Range(drop.GetMinAmount, drop.GetMaxAmount)));
                }
            }
            inventory.PrintInventory(0);
        }
    }

    public void CancelEvent(Vector2Int gridPosition, TileMapLayer tilemapLayer) {
        if (eventInstance != null)
            eventInstance.Cancel();
    }

    public void SpecialInteraction(Vector2Int gridPosition, TileMapLayer tileMapLayer) {
    }
    public void Grow(Vector2Int gridPosition, TileMapLayer tileMapLayer) {
        currentStage++;
        GridManager._instance.SetTile(tileSlot, gridPosition, tileMapLayer, false);
        if (!reachedMaxStage) {
            InitEvent(gridPosition, tileMapLayer);
        }
    }
    public void Init(Vector2Int gridPosition, TileMapLayer tilemapLayer, bool generation = false) {
        if (generation)
            currentStage = StagesCount - 1;
        if (eventInstance == null && tile.GetStages.Length > 1 && !reachedMaxStage)
            InitEvent(gridPosition, tilemapLayer);
    }
    private void InitEvent(Vector2Int gridPosition, TileMapLayer tileMapLayer) {
        eventInstance = new TileGrowEvent(Time.time + Random.Range(tile.GetMinGrowTime, tile.GetMaxGrowTime), tileSlot, gridPosition, tileMapLayer);
    }


}
public class TileGrowEvent : TimeEvent
{
    protected TileSlot triggeringTile;
    protected readonly Vector2Int eventPosition;
    protected readonly TileMapLayer tileMapLayer;
    public TileGrowEvent(float triggerTime, TileSlot triggeringTile, Vector2Int eventPosition, TileMapLayer tileMapLayer) : base(triggerTime) {
        this.triggeringTile = triggeringTile;
        this.eventPosition = eventPosition;
        this.tileMapLayer = tileMapLayer;
    }

    public override void Trigger() {
        eventTriggered = true;
        ((GatherableState)triggeringTile.tileState).Grow(eventPosition, tileMapLayer);

    }
}

