﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is responsible for controlling the currently selected ability. 
// Works in conjunction with ReticleController for casting abilities
public class AbilityController : MonoBehaviour {

    #region Create Singleton
    private static AbilityController _instance;

    public static AbilityController Instance {
        get {
            if (_instance == null) {
                GameObject go = new GameObject( "AbilityController" );
                go.AddComponent<AbilityController>();
            }
            return _instance;
        }
    }

    void Awake() {
        _instance = this;
    }
    #endregion

    // Public Variables
    public static int AbilitiesUsed = 0; // Counter variable
    public enum ReticleType { None, Diamond, Square, Cone };
    public enum AbilityType { Damage, Healing, Mobility, Cleanse };
    public enum TargetType { Any, Enemy, Ally, Self };
    public static int CastRange { get; private set; }
    public static AbilityType Type { get; private set; } // 0 is damage, 1 is healing, 2 is mobility
    public static ReticleType Reticle { get; set; } // 0 is none, 1 is diamond, 2 is square
    public static int XAxis { get; private set; }
    public static int YAxis { get; private set; } // unused at this time, will be required for some abilities later

    // Local Variables
    float abilityValue; // damage or healing number
    TargetType targetType; // 0 all, 1 is enemy, 2 is ally, 3 is self
    private Status status;

    // For initializing the selected ability
    public void Init(BattleAbility ability) {
        Debug.Log( JsonUtility.ToJson( ability ) );
        SetTargetType( ability.targetType );
        SetAbilityType( ability.abilityType );
        CastRange = ability.range;
        SetAbilityDamage( ability.value );
        XAxis = ability.xAxis;
        SetReticleType( ability.reticleType );
        if (ability.status != null) {
            status = ability.status;
        } else {
            status = null;
        }
        
        TurnController.State = TurnController.TurnState.Attack;

    }

    // Initializes enum ReticleType Reticle
    private void SetReticleType( int type ) {
        if(AbilitiesUsed <= 0) {
            switch (type) {
                case 0:
                    Reticle = ReticleType.None;
                    break;
                case 1:
                    Reticle = ReticleType.Diamond;
                    break;
                case 2:
                    Reticle = ReticleType.Square;
                    break;
            }
        } else {
            Reticle = ReticleType.None;
        }
        
    }

    // Initializes enum TargetType targetType, used for deciding affected targets
    private void SetTargetType( int type ) {
        switch (type) {
            case 0:
                targetType = TargetType.Any;
                break;
            case 1:
                targetType = TargetType.Ally;
                break;
            case 2:
                targetType = TargetType.Enemy;
                break;
            case 3:
                targetType = TargetType.Self;
                break;
        }
    }

    // Initializes enum AbilityType Type, used for Ability Functionality
    private void SetAbilityType( int type ) {
        switch (type) {
            case 0:
                Type = AbilityType.Damage;
                break;
            case 1:
                Type = AbilityType.Healing;
                break;
            case 2:
                Type = AbilityType.Mobility;
                break;
            case 3:
                Type = AbilityType.Cleanse;
                break;
        }
    }

    // Initializes abilityValue, used for Healing or Damage calculations
    private void SetAbilityDamage( float damage ) {
        abilityValue = damage;
    }

    // Method responsible for the cast animation, calls Cast() after completion
    public IEnumerator Animation( Color32 color1, Color32 color2, Color32 color3 ) {

        // Initialize sequence for the animation order
        TileController.sequence = 0;

        // Animation sequence 0
        yield return new WaitForSeconds( 0.3f );
        TileController.Instance.ColorTiles( ReticleController.CastArea, TileController.ColorMethod.Cast );

        // Animation sequence 1
        yield return new WaitForSeconds( 0.1f );
        TileController.Instance.ColorTiles( ReticleController.CastArea, TileController.ColorMethod.Cast );

        // Animation sequence 2
        yield return new WaitForSeconds( 0.1f );
        TileController.Instance.ColorTiles( ReticleController.CastArea, TileController.ColorMethod.Cast );

        // Animation sequence 3
        yield return new WaitForSeconds( 0.1f );
        TileController.Instance.ColorTiles( ReticleController.CastArea, TileController.ColorMethod.Cast );

        // Animation sequence 4
        yield return new WaitForSeconds( 0.1f );
        TileController.Instance.ColorTiles( ReticleController.CastArea, TileController.ColorMethod.Cast );

        // Cast ability
        Cast();
    }

    // ValidateTargets methods ensure that only Characters inside the cast area will be affected
    private void ValidateTargets(GameObject[] targets, float val) {
        foreach (GameObject target in targets) {
            ValidateTarget( target, val );
        }
    }

    // ValidateTarget methods ensure that only the Character inside the cast area will be affected
    private void ValidateTarget( GameObject target, float val ) {
        if (ReticleController.CastArea.Contains( TileController.GridLayout.WorldToCell( target.transform.position ) )) {
            target.GetComponent<Character>().TakeDamage( val );

            if(status != null) {
                Status curStatus = new Status();
                curStatus = status;
                target.GetComponent<Character>().ApplyStatusEffect( curStatus );
            }
        }
    }

    private void CleanseTargets(GameObject[] targets) {
        foreach (GameObject target in targets) {
            CleanseTarget( target );
        }
    }

    private void CleanseTarget(GameObject target) {
        if (ReticleController.CastArea.Contains( TileController.GridLayout.WorldToCell( target.transform.position ) ) && target.tag != "Player") { // need to rework this logic later with teams implemented TODO
            target.GetComponent<Character>().ClearAllBuffs();
            target.GetComponent<Character>().ClearAllDebuffs(); // Temporary until teams TODO
            target.GetComponent<Character>().RevealThisCharacter();
        } else if(ReticleController.CastArea.Contains( TileController.GridLayout.WorldToCell( target.transform.position ) ) && target.tag == "Player") {
            target.GetComponent<Character>().ClearAllDebuffs();
        }
    }

    // Casts the spell, and deals damage to affected targets using abilityValue. Healing is done with -abilityValue
    private void Cast() {

        // Increment the amount of times a player has casted an ability. This will be used more later with talents
        AbilitiesUsed += 1;

        // Determine TargetType then AbilityType { possibly switch statement later TODO }
        if (targetType == TargetType.Any) { // Target Any

            if (Type == AbilityType.Damage) { // Damage Any
                ValidateTargets( GameObject.FindGameObjectsWithTag( "Enemy" ), abilityValue );
                ValidateTargets( GameObject.FindGameObjectsWithTag( "Ally" ), abilityValue );
            } else if (Type == AbilityType.Healing) { // Heal Any
                ValidateTargets( GameObject.FindGameObjectsWithTag( "Enemy" ), -abilityValue );
                ValidateTargets( GameObject.FindGameObjectsWithTag( "Ally" ), -abilityValue );
                ValidateTarget( GameObject.FindWithTag( "Player" ), -abilityValue );
            } else if (Type == AbilityType.Cleanse) { // Cleanse Any
                CleanseTargets( GameObject.FindGameObjectsWithTag( "Enemy" ) );
                CleanseTargets( GameObject.FindGameObjectsWithTag( "Ally" ) );
                CleanseTarget( GameObject.FindWithTag( "Player" ) );
            }

        } else if (targetType == TargetType.Enemy) { // Target Enemies

            if (Type == AbilityType.Damage) { // Damage Enemies
                ValidateTargets( GameObject.FindGameObjectsWithTag( "Enemy" ), abilityValue );
            }

        } else if (targetType == TargetType.Ally) { // Target Allies

            if (Type == AbilityType.Healing) { // Heal Allies
                ValidateTargets( GameObject.FindGameObjectsWithTag( "Ally" ), -abilityValue );
            }

        } else if (targetType == TargetType.Self) { // Target Self

            if (Type == AbilityType.Mobility) { // Move Self
                GameObject.FindWithTag( "Player" ).transform.position = new Vector3( ReticleController.CastArea[0].x + 0.5f, ReticleController.CastArea[0].y + 0.5f, ReticleController.CastArea[0].z );
                NavigationController.Instance.ReInit();
            }

        }
    }
}
