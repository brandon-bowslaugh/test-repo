using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log( "Movement Available: " + NavigationController.MovementRemaining );
        Debug.Log( "Move " + NavigationController.MoveSpaces + " spaces." );
        NavigationController.Instance.Move();
        Debug.Log( "Moving..." );
        Debug.Log( "Moved " + NavigationController.MoveSpaces + " spaces." );
        Debug.Log( "Movement Remainging: " + NavigationController.MovementRemaining );

        Debug.Log( "" );
        Debug.Log( "Moving Again" );
        NavigationController.MoveSpaces = 3;
        Debug.Log( "Movement Available: " + NavigationController.MovementRemaining );
        Debug.Log( "Move " + NavigationController.MoveSpaces + " spaces." );
        NavigationController.Instance.Move();
        Debug.Log( "Moving..." );
        Debug.Log( "Moved " + NavigationController.MoveSpaces + " spaces." );
        Debug.Log( "Movement Remainging: " + NavigationController.MovementRemaining );

    }
}
