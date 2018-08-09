﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is responsible for handling player movement.
// Works in conjunction with ReticleController for determining movement area.
public class NavigationController : MonoBehaviour {

    #region Create Singleton
    private static NavigationController _instance;

    public static NavigationController Instance {
        get {
            if (_instance == null) {
                GameObject go = new GameObject( "NavigationController" );
                go.AddComponent<NavigationController>();
            }
            return _instance;
        }
    }

    void Awake() {
        _instance = this;
    }

    public void Setup() {
        Debug.Log( "NavigationController.cs -> Setup()" );
    }

    #endregion
    #region Movement Variables
    // Public Variables
    public static int MovementRemaining { get; set; }   // total movement remaining for the entity

    // Local Variables
    private GameObject entity;
    private Vector3 cellPosition;
    public static List<Vector3Int> navTiles;
    private float speed = 3f;
    private float xDiff;
    private float yDiff;
    private float step;
    #endregion

    #region Movement Functionality

    // This method Initializes the movement variables when it becomes a new players turn
    public void Init( GameObject entity ) {
        navTiles = new List<Vector3Int>();
        this.entity = entity;
        MovementRemaining = entity.GetComponent<Character>().movementStat;
        cellPosition = entity.transform.position;
    }

    // This method Re-Initializes the movement variables
    public void ReInit() {
        TileController.Instance.ClearTiles( navTiles );
        navTiles.Clear();
        cellPosition = entity.transform.position;
    }

    // This method decides which direction to move first for Character movement
    // TODO remove this after A* Algorithm Implementation
    private void FindPath(Vector3 destination) {

        // Initialize the current cellPosition
        cellPosition = entity.transform.position;

        // Determine movement direction 
        if (cellPosition.x < destination.x) {
            xDiff = destination.x - cellPosition.x;
        } else {
            xDiff = cellPosition.x - destination.x;
        }

        if (cellPosition.y < destination.y) {
            yDiff = destination.y - cellPosition.y;
        } else {
            yDiff = cellPosition.y - destination.y;
        }

    }

    
    // This method is responsible for moving the Character to the targeted location
    public void Move(Vector3 destination) {

        FindPath( destination );
        StartCoroutine( Step( destination ) ); // calls step to make sure that the movement is animated rather than a teleport
        MovementRemaining = Mathf.FloorToInt( MovementRemaining - Mathf.Abs( xDiff ) - Mathf.Abs( yDiff ) );
        // Re-Initialize movement variables
        ReInit();
        TurnController.State = TurnController.TurnState.Standby;

    }

    // This method animates the Character movement and respositions the Character
    private IEnumerator Step(Vector3 destination) {
        while (entity.transform.position != destination) {

            // set the movement direction TODO change this after A* Algorithm Implementation
            if (xDiff > yDiff && entity.transform.position.x != destination.x ||
                xDiff < yDiff && entity.transform.position.y == destination.y ||
                xDiff == yDiff && entity.transform.position.x != destination.x) {

                // perform the movement
                entity.transform.position = Vector3.MoveTowards( entity.transform.position, new Vector3( destination.x, entity.transform.position.y, entity.transform.position.z ), speed * Time.deltaTime );

            }
            else {

                // perform the movement
                entity.transform.position = Vector3.MoveTowards( entity.transform.position, new Vector3( entity.transform.position.x, destination.y, entity.transform.position.z ), speed * Time.deltaTime );

            }
            yield return null;
        }

        // Decides the direction to move next
        FindPath( destination );
    }


    // Displays the movement area when the 'Move' button is pressed
    public void CalcMoveArea() {
        // will probably need to replace this with A* Algorithm TODO
        for (int x = (MovementRemaining * -1); x <= MovementRemaining; x++) {
            for (int y = (MovementRemaining * -1); y <= MovementRemaining; y++) {
                // Ensures diamond shape
                if (Mathf.Abs( x ) + Mathf.Abs( y ) <= MovementRemaining) { 
                    Vector3Int tile = new Vector3Int( Mathf.FloorToInt(cellPosition.x) + x, Mathf.FloorToInt(cellPosition.y) + y, Mathf.FloorToInt(cellPosition.z) );
                    // Ensures that there is not a water tile that exists at this tiles position
                    if (TileController.Instance.ColorTile( tile, TileController.ColorMethod.Movement )) { 
                        navTiles.Add( tile );
                    }
                }
            }
        }
    }

    #endregion
}
