using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Identify matches and cache it, for further usage.
    /// </summary>
    public class MatchAndCacheTiles : StateMachineBehaviour
    {
        private enum TestContextEnum { UsingCachedInputTiles, UsingWholeGrid }

        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Identify matches and cache it, for further usage."
        )]
#endif
        #endregion
        [SerializeField]
        private TestContextEnum testContext;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            switch (testContext)
            {
                case TestContextEnum.UsingCachedInputTiles:
                    TileManager.Instance.MatchLinesFromTiles(InputManager.Instance.LastSwapInputInfo.tileA,
                                                                 InputManager.Instance.LastSwapInputInfo.tileB);
                    break;
                case TestContextEnum.UsingWholeGrid:
                    TileManager.Instance.MatchLinesInTheWholeGrid();
                    break;
            }
        }
    }
}