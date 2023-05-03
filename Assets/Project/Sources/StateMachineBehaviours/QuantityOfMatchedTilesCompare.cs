using System.Collections.Generic;
using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Perform a test of quantity of matched tiles, using the TileManager cached information about matches. 
    /// </summary>
    public class QuantityOfMatchedTilesCompare : StateMachineBehaviour
    {
        private enum TestTypeEnum { GreaterThan }

        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Perform a test of quantity of matched tiles, using the TileManager cached information about matches."
        )]
#endif
        #endregion
        [SerializeField]
        private TestTypeEnum testType;

        [SerializeField]
        private float targetValue;

        [SerializeField]
        private string trueTrigger;

        [SerializeField]
        private string falseTrigger;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            List<Tile> matchedTiles = TileManager.Instance.GetCacheOfMatchedTiles();

            switch (testType)
            {
                case TestTypeEnum.GreaterThan:
                    if (matchedTiles != null && matchedTiles.Count > targetValue)
                    {
                        fsm.SetTrigger(trueTrigger);
                    }
                    else
                    {
                        fsm.SetTrigger(falseTrigger);
                    }
                    break;

            }
        }
    }
}
