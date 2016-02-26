using System.Collections.Generic;
using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Check for matches and cache the result using the TileManager, for further usage.
    /// </summary>
    public class CheckAndCacheMatches : StateMachineBehaviour
    {
        private enum TestContextEnum { UsingCachedInputTiles, UsingWholeGrid }

        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Check for matches and cache the result using the TileManager, for further usage."
        )]
#endif
        #endregion
        [SerializeField]
        private TestContextEnum testContext;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            List<Tile> matchedTiles = null;

            switch (testContext)
            {
                case TestContextEnum.UsingCachedInputTiles:
                    matchedTiles = TileManager.Instance.GetLineMatchesFromTiles(InputManager.Instance.LastSwapInputInfo.tileA,
                                                                              InputManager.Instance.LastSwapInputInfo.tileB);
                    break;
                case TestContextEnum.UsingWholeGrid:
                    matchedTiles = TileManager.Instance.GetLineMatchesInTheWholeGrid();
                    break;
            }

            if (matchedTiles != null && matchedTiles.Count > 0)
            {
                TileManager.Instance.AddToCacheOfMatchedTiles(matchedTiles);
            }
        }
    }
}