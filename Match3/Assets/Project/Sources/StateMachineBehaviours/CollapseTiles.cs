using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Call TileManager.CollapseTiles()
    /// Doc:
    /// Iterate through all tiles and find new valid cell to the ones that are "flying"(has
    /// empty cell below), resulted of a match and clearing session. This arrangement will free
    /// cells in the top of the grid, so new tiles can be spawned and placed in these cells.
    /// Note: This method just arrange the cells of the tiles, a gravity effect must still be called
    ///       later so the tiles can actually fall towards its respective cells.
    /// </summary>
    public class CollapseTiles : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);
            TileManager.Instance.CollapseTiles();
        }
    }
}