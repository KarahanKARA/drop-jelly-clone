using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TheGame._Scripts.Block;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    public class BoardFlowManager : Singleton<BoardFlowManager>
    {
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

                if (ComponentReferences.Instance.boardGrid != null)
                {
                    ComponentReferences.Instance.boardGrid.ClearAllPositions();
                    allBlockSystems = FindObjectsOfType<BlockSystem>().ToList();
                    foreach (var bs in allBlockSystems)
                    {
                        var pos = bs.positionData;
                        ComponentReferences.Instance.boardGrid.RegisterBlockSystem(pos.x, pos.y, bs);
                        ComponentReferences.Instance.boardGrid.SetPositionOccupied(pos.y, pos.x);
                    }
                }

                foreach (var bs in allBlockSystems)
                {
                    bs.CheckExpand();
                    bs.TryMakeBigSquareIfSingleColor();
                    bs.CheckIfEmpty();
                }

                if (ComponentReferences.Instance.boardGrid != null)
                {
                    for (var c = 0; c < GameData.BoardSize; c++)
                    {
                        ComponentReferences.Instance.boardGrid.ApplyGravity(c);
                    }
                }

                yield return new WaitForSeconds(0.8f);
            }

            if (ComponentReferences.Instance.boardGrid != null &&
                !ComponentReferences.Instance.boardGrid.HasAnyOccupiedPosition())
            {
                UiManager.Instance.GameWin();
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator DestroyAllAtOnce(HashSet<ChildBlockSystem> blocksToDestroy)
        {
            if (blocksToDestroy == null || blocksToDestroy.Count == 0) yield break;

            var center = GetCenterPoint(blocksToDestroy);
            center.z = -5f;

            var seq = DOTween.Sequence();
            foreach (var b in blocksToDestroy)
            {
                if (b == null) continue;
                seq.Join(b.transform.DOScale(b.transform.localScale * 1.1f, 0.15f));
                seq.Join(b.transform.DOMove(center, 0.15f).SetEase(Ease.OutQuad));
            }

            foreach (var b in blocksToDestroy)
            {
                if (b == null) continue;
                seq.Join(b.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));
            }

            var done = false;
            seq.OnComplete(() => done = true);
            while (!done) yield return null;

            foreach (var b in blocksToDestroy)
            {
                if (b == null) continue;

                var color = DataManager.Instance.GetColorFromBlock(b.blockColor);
                var partObj = Instantiate(PrefabReferences.Instance.blockDestroyParticle, b.transform.position,
                    Quaternion.identity);
                var systems = partObj.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in systems)
                {
                    var main = ps.main;
                    main.startColor = color;
                }

                Destroy(partObj, 2f);

                var parent = b.transform.parent.GetComponent<BlockSystem>();
                if (parent != null) parent._childBlockMap.Remove(b.position);
                Destroy(b.gameObject);
            }
        }

        private Vector3 GetCenterPoint(HashSet<ChildBlockSystem> blocks)
        {
            var sx = 0f;
            var sy = 0f;
            var count = 0;
            foreach (var b in blocks)
            {
                if (b == null) continue;
                var p = b.transform.position;
                sx += p.x;
                sy += p.y;
                count++;
            }

            if (count == 0) return Vector3.zero;
            return new Vector3(sx / count, sy / count, 0f);
        }
    }
}