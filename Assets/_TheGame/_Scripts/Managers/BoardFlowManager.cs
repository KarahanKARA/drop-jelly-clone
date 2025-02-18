using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TheGame._Scripts.Block;
using _TheGame._Scripts.Board;
using _TheGame._Scripts.Constants;
using _TheGame._Scripts.Data;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    public class BoardFlowManager : MonoBehaviour
    {
        public static BoardFlowManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void StartFullFlowWithCallback(Action onComplete)
        {
            StartCoroutine(FullFlowRoutine(onComplete));
        }

        private IEnumerator FullFlowRoutine(Action onComplete)
        {
            while (true)
            {
                var allBlockSystems = FindObjectsOfType<BlockSystem>().ToList();
                var allBlocksToDestroy = new HashSet<ChildBlockSystem>();

                foreach (var bs in allBlockSystems)
                {
                    var blocks = bs.FindBlocksToDestroy();
                    foreach (var b in blocks) allBlocksToDestroy.Add(b);
                }

                if (allBlocksToDestroy.Count == 0) break;

                yield return StartCoroutine(DestroyAllAtOnce(allBlocksToDestroy));
        
                var grid = FindObjectOfType<BoardGrid>();
                if (grid != null)
                {
                    grid.ClearAllPositions();
            
                    allBlockSystems = FindObjectsOfType<BlockSystem>().ToList();
                    foreach (var bs in allBlockSystems)
                    {
                        var pos = bs.positionData;
                        grid.RegisterBlockSystem(pos.x, pos.y, bs);
                        grid.SetPositionOccupied(pos.y, pos.x);
                    }
                }

                foreach (var bs in allBlockSystems)
                {
                    bs.CheckExpand();
                    bs.TryMakeBigSquareIfSingleColor();
                    bs.CheckIfEmpty();
                }

                if (grid != null)
                {
                    for (var c = 0; c < GameData.BoardSize; c++) 
                    {
                        grid.ApplyGravity(c);
                    }
                    yield return new WaitForSeconds(NumericConstants.DelayBetweenAnimations);
                }

                yield return new WaitForSeconds(NumericConstants.DelayBetweenAnimations);
            }
            onComplete?.Invoke();
        }

        private IEnumerator DestroyAllAtOnce(HashSet<ChildBlockSystem> blocksToDestroy)
        {
            var seq = DG.Tweening.DOTween.Sequence();
            foreach (var b in blocksToDestroy)
            {
                if (b == null) continue;
                seq.Join(b.transform.DOScale(Vector3.zero, 0.3f));
            }
            var done = false;
            seq.OnComplete(() => done = true);
            while (!done) yield return null;

            foreach (var b in blocksToDestroy)
            {
                if (b == null) continue;
                var parent = b.transform.parent.GetComponent<BlockSystem>();
                if (parent != null) parent._childBlockMap.Remove(b.position);
                Destroy(b.gameObject);
            }
        }

    }
}
