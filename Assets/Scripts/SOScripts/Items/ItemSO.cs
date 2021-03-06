﻿using UnityEngine;


public enum ItemName
{
    //generic
    OakLog,
    WoodenStick,
    Stone,
    Sugar,
    UncookedMeat,
    //Consumable
    CookedMeat,
    SweetCookedMeat,
    Apple,
    OxygenBubble,
    // Building
    Flower,
    CraftingTable,
    WoodBlock,
    StoneBlock,
    //Tools
    WoodenSword,
    WoodenHoe
    //Gear

}



public enum ItemType
{
    Generic,
    Tool,
    Consumable,
    Building,
    Gear

}





[CreateAssetMenu(fileName = "New Generic", menuName = "Items/" + "Generic")]
public class ItemSO : ScriptableObject
{
    [HideInInspector]
    public int itemID;

    [SerializeField]
    private string itemName;
    public string getItemName => itemName;

    [SerializeField]
    private int maxStackSize;
    public int getmaxStackSize => maxStackSize;

    [SerializeField]
    [TextArea]
    private string description;
    public string getDescription => description;

 


    //[SerializeField]
    //private ItemName itemEnum;
    //public ItemName getItemEnum => itemEnum;

    [SerializeField]
    private Sprite sprite;
    public Sprite getsprite { get => sprite; }



    public ItemType GetItemType
    {
        get
        {
            switch (this)
            {
                case ConsumableItemSO consumable:
                    return ItemType.Consumable;
                case GearItemSO Gear:
                    return ItemType.Gear;
                case TileAbstSO Tile:
                    return ItemType.Building;
                case ToolItemSO Tool:
                    return ItemType.Tool;
                default:
                    return ItemType.Generic;
            }
        }
    }
}


