using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Action that swap tiles based on the last input that happened. This will change the cells of the
    /// tiles, one by another, and then execute an animation so the tiles perform a movement towards its
    /// new cell.
    /// </summary>
    public class ExecuteSwapAndMoveTilesToNewPosition : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Action that swap tiles based on the last input that happened. This will change the cells of the\n" +
            "tiles, one by another, and then execute an animation so the tiles perform a movement towards its\n" +
            "new cell.\n" +
            "\n" +
            "undoSwap: If true, the swap will be inverted, but still using the information about the last input\n" +
            "happened. So for example, you can use this same action two times in a row, one to swap and\n" +
            "other to unswap.\n" +
            "\n" +
            "animationTime: The time of the tile movement animation."
        )]
#endif
        #endregion
        /// <summary>
        /// If true, the swap will be inverted, but still using the information about the last input
        /// happened. So for example, you can use this same action two times in a row, one to swap and
        /// other to unswap.
        /// </summary>
        [SerializeField]
        private bool undoSwap;

        /// <summary>
        /// The time of the tile movement animation.
        /// </summary>
        [SerializeField]
        private float animationTime;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            Tile tileA = InputManager.Instance.LastSwapInputInfo.tileA;
            Tile tileB = InputManager.Instance.LastSwapInputInfo.tileB;

            if (!undoSwap)
            {
                Tile.SwapTiles(tileA, tileB);
            }
            else
            {
                // invert
                Tile.SwapTiles(tileB, tileA);
            }

            tileA.AnimateMovementToCellPosition(animationTime);
            tileB.AnimateMovementToCellPosition(animationTime);
        }
    }
}