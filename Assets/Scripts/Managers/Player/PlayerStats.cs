﻿using System;
using System.Collections.Generic;
using UnityEngine;
public enum StatType
{
    None,
    HP,
    Food,
    Water,
    Air,
    Sleep,
    Temperature,

    MaxHP,
    MaxFood,
    MaxWater,
    MaxAir,
    MaxSleep,

    EXP,
    EXPtoNextLevel,
    Level,
    MoveSpeed,
    AttackDMG,
    GatheringSpeed
}
public class PlayerStats : MonoSingleton<PlayerStats>
{
    private Dictionary<StatType, Stat> StatsDict;
    public override void Init() {
        StatsDict = new Dictionary<StatType, Stat>();
        FillDictionary();
        AddReactions();
        ResetStats();
    }

    private void ResetStats() {
        foreach (Stat stat in StatsDict.Values)
            stat.Reset();
    }

    private void FillDictionary() {
        Stat maxStat;
        //HP
        maxStat = AddToDict(StatType.MaxHP, 100);
        AddToDict(StatType.HP, 100, maxStat);
        //Food
        maxStat = AddToDict(StatType.MaxFood, 100);
        AddToDict(StatType.Food, 0, maxStat);
        //Water
        maxStat = AddToDict(StatType.MaxWater, 100);
        AddToDict(StatType.Water, 100, maxStat);
        //Air
        maxStat = AddToDict(StatType.MaxAir, 100);
        AddToDict(StatType.Air, 100, maxStat);
        //Sleep
        maxStat = AddToDict(StatType.MaxSleep, 100);
        AddToDict(StatType.Sleep, 100, maxStat);

        //Other
        AddToDict(StatType.Temperature, 100);
        AddToDict(StatType.EXPtoNextLevel, 100);
        AddToDict(StatType.Level, 1);
        AddToDict(StatType.MoveSpeed, 1);
        AddToDict(StatType.GatheringSpeed, 1);
        AddToDict(StatType.AttackDMG, 100);

        ExpStat stat = new ExpStat(
            StatType.EXP,
            0,
            maxStat
        );
        StatsDict.Add(StatType.EXP, stat);
    }
    private Stat AddToDict(StatType statType, float defaultValue, Stat maxStat = null) {
        Stat stat = new Stat(
                statType,
                defaultValue,
                maxStat
            );
        StatsDict.Add(statType, stat);
        return stat;
    }
    private void AddReactions() {
        EffectData hpLoseEffect = new EffectData() {
            effectStatType = StatType.HP,
            effectType = EffectType.OverTimeSmallPortion,
            isPrecentage = false,
            isRelative = false,
            amount = -1f,
            tickTime = 1f,
            duration = Mathf.Infinity
        };
        EffectData hpRegenEffect = new EffectData() {
            effectStatType = StatType.HP,
            effectType = EffectType.OverTimeSmallPortion,
            isPrecentage = false,
            isRelative = false,
            amount = 1f,
            tickTime = 1f,
            duration = Mathf.Infinity
        };
        AddReaction(StatType.Food, false, true, 0, new EffectData[1] { hpLoseEffect });
        AddReaction(StatType.Food, true, false, 95, new EffectData[1] { hpRegenEffect });
    }
    private void AddReaction(StatType statType, bool isPrecentage, bool checkSmaller, float reactionStartValue, EffectData[] effectsData) {
        Reaction reaction = new Reaction(isPrecentage, checkSmaller, reactionStartValue, effectsData);
        GetStat(statType).AddReaction(reaction);
    }
    public Stat GetStat(StatType statType) => StatsDict[statType];
    public Stat GetMaxStat(StatType statType)
        => StatsDict[statType].maxStat;
}
[System.Serializable]
public class Stat
{
    public StatType statType;
    [HideInInspector] public Stat maxStat;
    private List<Reaction> reactions;
    public float defaultValue;
    private protected float value;
    public bool GetIsCapped => maxStat != null;

    public Stat(StatType statType, float defaultValue, Stat maxStat) {
        this.statType = statType;
        this.maxStat = maxStat;
        this.maxStat = maxStat;
        this.defaultValue = defaultValue;
    }
    public virtual float GetSetValue {
        get => value;
        set {

            this.value = Mathf.Max(value, 0);
            if (maxStat != null)
                this.value = Mathf.Min(this.value, maxStat.GetSetValue);
            UIManager._instance.UpdateSurvivalBar(this, value);
            if (reactions != null)
                foreach (Reaction reaction in reactions)
                    reaction.CheckIfReactionEligible(this);

        }
    }
    public void Reset() {
        GetSetValue = defaultValue;
    }
    public void AddReaction(Reaction reaction) {
        if (reactions == null)
            reactions = new List<Reaction>();
        reactions.Add(reaction);
    }
}







[System.Serializable]
public class ExpStat : Stat
{
    public ExpStat(StatType statType, float defaultValue, Stat maxStat) : base(statType, defaultValue, maxStat) {
    }

    public override float GetSetValue {
        get => value;
        set {
            PlayerStats playerStats = PlayerStats._instance;
            this.value = Mathf.Max(value, 0);
            float XPtoLevel = playerStats.GetStat(StatType.EXPtoNextLevel).GetSetValue;
            if (this.value >= XPtoLevel) {
                playerStats.GetStat(StatType.Level).GetSetValue += 1;
                //Needs to be recursive
                playerStats.GetStat(StatType.EXPtoNextLevel).GetSetValue *= 1.5f;
                GetSetValue -= XPtoLevel;
            }
            else {
                UIManager._instance.UpdateExpAndLvlBar();
            }
        }
    }
}

public class Reaction
{
    private bool isPercentage, checkSmaller;
    private float reactionStartValue;
    private EffectData[] effectsData;
    private EffectController[] effectsCont;
    private bool effectsRunning;

    public Reaction(bool isPercentage, bool checkSmaller, float reactionStartValue, EffectData[] effectsData) {
        this.isPercentage = isPercentage;
        this.checkSmaller = checkSmaller;
        this.reactionStartValue = reactionStartValue;
        this.effectsData = effectsData;
    }

    private EffectController[] GetEffectsCont {
        get {
            if (effectsCont == null) {
                effectsCont = EffectHandler._instance.CreateControllers(effectsData, new float[effectsData.Length]);
            }
            return effectsCont;
        }
    }
    public bool CheckIfReactionEligible(Stat stat) {
        float tempValueCheck = reactionStartValue;
        if (isPercentage && stat.GetIsCapped) {
            tempValueCheck = stat.maxStat.GetSetValue * (reactionStartValue / 100);
        }
        if ((checkSmaller && stat.GetSetValue <= tempValueCheck) || (!checkSmaller && stat.GetSetValue >= tempValueCheck)) {
            if (!effectsRunning)
                StartReaction();
        }
        else {
            if (effectsRunning)
                StopReaction();
        }
        return false;
    }
    private void StartReaction() {
        EffectHandler._instance.BeginAllEffects(effectsData, GetEffectsCont);
        effectsRunning = true;
    }
    private void StopReaction() {
        EffectHandler._instance.StopAllEffects(GetEffectsCont);
        effectsRunning = false;

    }
}