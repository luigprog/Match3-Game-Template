using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Action that Spawn(crude) tiles in order to fill all the empty spaces of the grid.
    /// </summary>
    public class FillGridWithTiles : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Action that Spawn(crude) tiles in order to fill all the empty spaces of the grid.\n" +
            "\n" +
            "avoidMatches: If true, the tiles will be mutated in order to generate no matches."
        )]
#endif
        #endregion
        /// <summary>
        /// If true, the tiles will be mutated in order to generate no matches.
        /// </summary>
        [SerializeField]
        private bool avoidMatches;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            TileManager.Instance.FillGridWithTiles();

            if (avoidMatches)
            {
                while (TileManager.Instance.GetLineMatchesInTheWholeGrid().Count > 0)
                {
                    // Mutate the notFullyCreated tiles until there is no matches
                    TileManager.Instance.RandomlyMutateTiles();
                }
            }

            TileManager.Instance.FinalizeSpawn();
        }
    }
}