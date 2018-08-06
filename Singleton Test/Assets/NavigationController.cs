using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    #endregion

    #region Movement Variables
    private static int _movementRemaining = 6;
    private static GridLayout gridLayout;
    private static int _moveSpaces = 2;

    public static int MovementRemaining {
        get {
            return _movementRemaining;
        }
    }

    public static int MoveSpaces {
        get {
            return _moveSpaces;
        }
        set {
            _moveSpaces = value;
        }
    }
    #endregion

    #region Movement Functionality
    public void Init() {
    }
    public void Move() {
        _movementRemaining = _movementRemaining - _moveSpaces;
    }
    public void CalcMoveArea() {

    }
    #endregion
}
