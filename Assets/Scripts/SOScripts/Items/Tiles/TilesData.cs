﻿using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Block, Gatherable, Chest, ProcessingTable, LightSource }
#region Tile hit
public class TileHit
{
    public readonly Vector2Int gridPosition;
    public readonly TileSlot tile;

    public TileHit(TileSlot tile, Vector2Int gridPosition) {
        this.tile = tile;
        this.gridPosition = gridPosition;
    }
}
#endregion
#region Main abstract classes
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
public abstract class TileAbstSO : ItemSO
{
    [SerializeField] private TileBase mainTileBase;
    [Header("Is overriden by stages if there's any.")]
    [SerializeField] private bool isSolid;
    [SerializeField] private bool isAirSource;
    public TileBase GetMainTileBase => mainTileBase;
    public bool isSpecialInteraction {
        get {
            switch (GetTileType) {
                case TileType.Block:
                case TileType.Gatherable:
                    return false;
                default:
                    return true;
            }
        }
    }
    public TileType GetTileType {
        get {
            switch (this) {
                case GatherableTileSO plant:
                    return TileType.Gatherable;
                case BlockTileSO block:
                    return TileType.Block;
                case ProcessingTableTileSO table:
                    return TileType.ProcessingTable;
                case LightSourceTileSO lightSource:
                    return TileType.LightSource;
                default:
                    throw new System.NotImplementedException();
            }
        }

    }

    public bool GetIsSolid => isSolid;
    public bool GetIsAirSource => isAirSource;
}
public interface ITileState
{
    TileBase GetMainTileBase { get; }
    TileAbstSO GetTileAbst { get; }
    TileType GetTileType { get; }
    bool GetIsSolid { get; }
    bool isSpecialInteraction { get; }
    void GatherInteraction(Vector2Int gridPosition, TileMapLayer buildingLayer);
    void SpecialInteraction(Vector2Int gridPosition, TileMapLayer buildingLayer);
    void CancelEvent(Vector2Int gridPosition, TileMapLayer tilemapLayer);
    void Init(Vector2Int gridPosition, TileMapLayer tilemapLayer, bool generation = false);
}
public class TileSlot : ITileState
{
    public ITileState tileState;
    public TileSlot(TileAbstSO tile) {
        if (tile == null)
            throw new System.NullReferenceException();
        switch (tile) {
            case GatherableTileSO plant:
                tileState = new GatherableState(plant, this);
                break;
            case BlockTileSO block:
                tileState = new BlockState(block);
                break;
            case ProcessingTableTileSO table:
                tileState = new ProcessingTableTileState(table, this);
                break;
            case LightSourceTileSO lightSource:
                tileState = new LightSourceTileState(lightSource);
                break;
            default:
                throw new System.NotImplementedException();

        }
    }
    public bool IsGatherable {
        get {
            if (tileState is GatherableState gatherable) {
                return gatherable.GetIsGatherable;
            }
            return false;
        }
    }
    public bool isSpecialInteraction => tileState.isSpecialInteraction;
    public bool GetIsDestructible {
        get {
            if(tileState is ProcessingTableTileState processing) {
                return processing.GetIsDestructible;
            }
            return true;
        }
    }
    #region Passthrough
    public virtual TileBase GetMainTileBase => tileState.GetMainTileBase;
    public virtual TileAbstSO GetTileAbst => tileState.GetTileAbst;
    public virtual TileType GetTileType => tileState.GetTileType;
    public virtual bool GetIsSolid => tileState.GetIsSolid;
    public bool GetIsAirSource => GetTileAbst.GetIsAirSource;

    public virtual void GatherInteraction(Vector2Int gridPosition, TileMapLayer buildingLayer)
        => tileState.GatherInteraction(gridPosition, buildingLayer);

    public virtual void SpecialInteraction(Vector2Int gridPosition, TileMapLayer buildingLayer)
        => tileState.SpecialInteraction(gridPosition, buildingLayer);
    public virtual void CancelEvent(Vector2Int gridPosition, TileMapLayer tilemapLayer)
        => tileState.CancelEvent(gridPosition, tilemapLayer);

    public void Init(Vector2Int gridPosition, TileMapLayer tilemapLayer, bool generation = false)
        => tileState.Init(gridPosition, tilemapLayer, generation);
    #endregion
}


#endregion












