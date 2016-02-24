using System.Collections.Generic;
using UnityEngine;

namespace StateMachineBehaviours
{
    public class ClearMatchedTiles : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            List<Tile> matchedTiles = TileManager.Instance.GetCacheOfMatchedTiles();

            for (int i = 0; i < matchedTiles.Count; i++)
            {
                matchedTiles[i].Cell.DetachTile();
                matchedTiles[i].Cell = null;
                matchedTiles[i].Clear();
            }

            TileManager.Instance.CleanCacheOfMatchedTiles();
        }
    }
}