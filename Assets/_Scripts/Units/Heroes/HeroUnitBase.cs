using UnityEngine;

public abstract class HeroUnitBase : UnitBase {


    public HeroUnitBase(CharacterStats stats) : base(stats) { }
    
    //private bool _canMove;

    //private void Awake() => GameManager.OnBeforeStateChanged += OnStateChanged;

    //private void OnDestroy() => GameManager.OnBeforeStateChanged -= OnStateChanged;

    //private void OnStateChanged(GameState newState) {
    //    if (newState == GameState.HeroTurn) _canMove = true;
    //}

    //private void OnMouseDown() {
    //    // Only allow interaction when it's the hero turn
    //    if (GameManager.Instance.State != GameState.HeroTurn) return;

    //    // Don't move if we've already moved
    //    if (!_canMove) return;

    //    // Show movement/attack options

    //    // Eventually either deselect or ExecuteMove(). You could split ExecuteMove into multiple functions
    //    // like Move() / Attack() / Dance()

    //    Debug.Log("Unit clicked");
    //}

    //public virtual void ExecuteMove() {
    //    // Override this to do some hero-specific logic, then call this base method to clean up the turn

    //    _canMove = false;
    //}
}