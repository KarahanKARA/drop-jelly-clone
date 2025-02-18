using System.Collections.Generic;
using System.Linq;
using _TheGame._Scripts.Board;
using _TheGame._Scripts.Constants;
using _TheGame._Scripts.Data;
using _TheGame._Scripts.Helpers;
using _TheGame._Scripts.Managers;
using _TheGame._Scripts.References;
using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockSystem : MonoBehaviour
    {
        private const int BoardSize = GameData.BoardSize;

        public Dictionary<Enums.ConnectionType, ChildBlockSystem> _childBlockMap
            = new Dictionary<Enums.ConnectionType, ChildBlockSystem>();

        public Vector2Int positionData = new Vector2Int();
        private BoardGrid _boardGrid;

        private void OnEnable()
        {
            _boardGrid = ComponentReferences.Instance.boardGrid;
            positionData = new Vector2Int(-10, -10);
        }

        public void CreateChildBlock(Enums.ConnectionType positionType, Enums.BlockColorType colorType,
            Enums.ConnectionType connectedWith)
        {
            var obj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var c = obj.GetComponent<ChildBlockSystem>();
            var shapeData = DataManager.Instance.blockShapeScaleAndLocalPosList
                .Find(x => x.blockPositionType == GetBlockPositionType(positionType));
            if (shapeData == null) return;

            var initPos = new Vector3(shapeData.localPos.x, shapeData.localPos.y, 0f);
            var initScale = new Vector3(shapeData.localScale.x, shapeData.localScale.y, 1f);
            c.Initialize(positionType, initPos, initScale);
            c.SetBlockColor(colorType);

            if (connectedWith != Enums.ConnectionType.None)
            {
                c.SetConnection(connectedWith);
            }

            _childBlockMap[positionType] = c;
        }

        public HashSet<ChildBlockSystem> FindBlocksToDestroy()
        {
            var set = new HashSet<ChildBlockSystem>();

            foreach (var b in _childBlockMap.Select(kvp => kvp.Value).Where(b => b != null))
            {
                if (b.isBigSquare)
                {
                    var neighbors = GetExactNeighborBlocks(b);
                    var matchingNeighbors = new List<ChildBlockSystem>();
                    var hasMatch = false;

                    foreach (var n in neighbors.Where(n => n != null && n.blockColor == b.blockColor))
                    {
                        hasMatch = true;
                        matchingNeighbors.Add(n);
                    }

                    if (hasMatch)
                    {
                        foreach (var child in _childBlockMap.Values.Where(child => child.blockColor == b.blockColor))
                        {
                            set.Add(child);
                        }

                        foreach (var child in from t in matchingNeighbors select t.transform.parent.GetComponent<BlockSystem>() into nb where nb != null && nb != this from child in nb._childBlockMap.Values where child.blockColor == b.blockColor select child)
                        {
                            set.Add(child);
                        }
                    }

                    continue;
                }

                var neigh = GetExactNeighborBlocks(b);
                foreach (var n in neigh.Where(n => n != null && n.blockColor == b.blockColor))
                {
                    set.Add(b);
                    set.Add(n);

                    var nb = n.transform.parent.GetComponent<BlockSystem>();
                    if (nb != null && nb != this)
                    {
                        foreach (var child in nb._childBlockMap.Values)
                        {
                            if (child.blockColor == b.blockColor)
                                set.Add(child);
                        }
                    }

                    FindConnectedBlocksToDestroy(b, set);
                    FindConnectedBlocksToDestroy(n, set);
                }
            }

            return set;
        }

        private void FindConnectedBlocksToDestroy(ChildBlockSystem block, HashSet<ChildBlockSystem> set)
        {
            if (block == null || !block.IsConnected) return;
            var c = GetConnectedBlock(block);
            if (c != null && c.blockColor == block.blockColor)
            {
                set.Add(c);
            }
        }

        private ChildBlockSystem GetConnectedBlock(ChildBlockSystem block)
        {
            if (block == null || !block.IsConnected) return null;
            return _childBlockMap.GetValueOrDefault(block.ConnectedWith);
        }

        public void CheckExpand()
        {
            var allPos = new[]
            {
                Enums.ConnectionType.TopLeft,
                Enums.ConnectionType.TopRight,
                Enums.ConnectionType.BottomLeft,
                Enums.ConnectionType.BottomRight
            };
            var empty = allPos.Where(p => !_childBlockMap.ContainsKey(p)).ToList();

            if (empty.Count == 0) return;

            foreach (var pos in empty)
            {
                if (TryExpandVertically(pos)) continue;
                TryExpandHorizontally(pos);
            }

            if (_boardGrid != null && _childBlockMap.Count > 0)
            {
                var col = positionData.x;
                var row = positionData.y;
                if (col >= 0 && row >= 0 && col < BoardSize && row < BoardSize)
                {
                    _boardGrid.RegisterBlockSystem(col, row, this);
                    _boardGrid.SetPositionOccupied(row, col);
                }
            }
        }

        private bool TryExpandVertically(Enums.ConnectionType emptyPos)
        {
            var list = _childBlockMap.Values.ToList();
            var source = list.FirstOrDefault(b =>
            {
                if (b.IsConnected) return false;
                if (b.position == Enums.ConnectionType.TopLeft && emptyPos == Enums.ConnectionType.BottomLeft)
                    return true;
                if (b.position == Enums.ConnectionType.TopRight && emptyPos == Enums.ConnectionType.BottomRight)
                    return true;
                if (b.position == Enums.ConnectionType.BottomLeft && emptyPos == Enums.ConnectionType.TopLeft)
                    return true;
                if (b.position == Enums.ConnectionType.BottomRight && emptyPos == Enums.ConnectionType.TopRight)
                    return true;
                return false;
            });
            if (source != null)
            {
                CloneAndMove(source, emptyPos);
                return true;
            }

            return false;
        }

        private void TryExpandHorizontally(Enums.ConnectionType emptyPos)
        {
            var list = _childBlockMap.Values.ToList();
            var source = list.FirstOrDefault(b =>
            {
                if (b.IsConnected) return false;
                if (b.position == Enums.ConnectionType.TopLeft && emptyPos == Enums.ConnectionType.TopRight)
                    return true;
                if (b.position == Enums.ConnectionType.TopRight && emptyPos == Enums.ConnectionType.TopLeft)
                    return true;
                if (b.position == Enums.ConnectionType.BottomLeft && emptyPos == Enums.ConnectionType.BottomRight)
                    return true;
                if (b.position == Enums.ConnectionType.BottomRight && emptyPos == Enums.ConnectionType.BottomLeft)
                    return true;
                return false;
            });
            if (source != null)
            {
                CloneAndMove(source, emptyPos);
            }
        }

        private void CloneAndMove(ChildBlockSystem source, Enums.ConnectionType targetPos)
        {
            var obj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var c = obj.GetComponent<ChildBlockSystem>();
            c.Initialize(targetPos, source.transform.localPosition, source.transform.localScale);
            c.SetBlockColor(source.blockColor);

            var shape = DataManager.Instance.blockShapeScaleAndLocalPosList
                .Find(x => x.blockPositionType == GetBlockPositionType(targetPos));
            if (shape != null)
            {
                var srcData = DataManager.Instance.GetConnectionData(source.position);
                var tgtData = DataManager.Instance.GetConnectionData(targetPos);
                if (srcData == null || tgtData == null)
                {
                    Debug.LogError("Connection data is null!");
                    return;
                }

                var finalPosSource = source.transform.localPosition +
                                     new Vector3(srcData.positionOffset.x, srcData.positionOffset.y, 0);
                var finalPosClone = new Vector3(shape.localPos.x, shape.localPos.y, 0) +
                                    new Vector3(tgtData.positionOffset.x, tgtData.positionOffset.y, 0);

                source.transform.DOLocalMove(finalPosSource, NumericConstants.ExpandAnimDuration);
                obj.transform.DOLocalMove(finalPosClone, NumericConstants.ExpandAnimDuration);

                var scaleSource = new Vector3(srcData.scaleAdjustment.x, srcData.scaleAdjustment.y, 0.975f);
                var scaleClone = new Vector3(tgtData.scaleAdjustment.x, tgtData.scaleAdjustment.y, 0.975f);
                source.transform.DOScale(scaleSource, NumericConstants.ExpandAnimDuration);
                obj.transform.DOScale(scaleClone, NumericConstants.ExpandAnimDuration);
            }
            else
            {
                Debug.LogError("Could not find target position data for " + targetPos);
                return;
            }

            source.SetConnection(targetPos);
            c.SetConnection(source.position);
            _childBlockMap[targetPos] = c;
        }

        public void CheckIfEmpty()
        {
            if (_childBlockMap.Count == 0)
            {
                var col = positionData.x;
                var row = positionData.y;
                if (_boardGrid != null)
                {
                    _boardGrid.UnregisterBlockSystem(col, row);
                    _boardGrid.SetPositionEmpty(row, col);
                }

                Destroy(gameObject);
                if (_boardGrid != null) _boardGrid.ApplyGravity(col);
            }
        }

        public void TryMakeBigSquareIfSingleColor()
        {
            if (_childBlockMap.Count < 2) return;
            var list = _childBlockMap.Values.ToList();
            var c = list[0].blockColor;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].blockColor != c) return;
            }

            var allPos = new[]
            {
                Enums.ConnectionType.TopLeft,
                Enums.ConnectionType.TopRight,
                Enums.ConnectionType.BottomLeft,
                Enums.ConnectionType.BottomRight
            };
            var missing = new List<Enums.ConnectionType>();
            for (int i = 0; i < allPos.Length; i++)
            {
                var p = allPos[i];
                if (!_childBlockMap.ContainsKey(p)) missing.Add(p);
            }

            if (missing.Count == 0) return;
            for (int i = 0; i < missing.Count; i++)
            {
                CloneForBigSquare(missing[i], c);
            }

            foreach (var child in _childBlockMap.Values)
            {
                child.isBigSquare = true;
            }
        }

        private void CloneForBigSquare(Enums.ConnectionType targetPos, Enums.BlockColorType color)
        {
            var obj = Instantiate(PrefabReferences.Instance.block1X1Prefab, transform);
            var c = obj.GetComponent<ChildBlockSystem>();

            var shape = DataManager.Instance.blockShapeScaleAndLocalPosList
                .Find(x => x.blockPositionType == GetBlockPositionType(targetPos));
            if (shape != null)
            {
                var conn = DataManager.Instance.GetConnectionData(targetPos);
                if (conn == null)
                {
                    Debug.LogError("Connection data is null for big square pos: " + targetPos);
                    return;
                }

                var initPos = new Vector3(shape.localPos.x, shape.localPos.y, 0f);
                var initScale = new Vector3(shape.localScale.x, shape.localScale.y, 1f);
                c.Initialize(targetPos, initPos, initScale);
                c.SetBlockColor(color);

                var finalPos = initPos + new Vector3(conn.positionOffset.x, conn.positionOffset.y, 0f);
                var finalScale = new Vector3(conn.scaleAdjustment.x, conn.scaleAdjustment.y, 0.975f);

                obj.transform.DOLocalMove(finalPos, 0.4f);
                obj.transform.DOScale(finalScale, 0.1f);
            }
            else
            {
                Debug.LogError("Could not find target position data for " + targetPos);
                return;
            }

            c.isBigSquare = true;
            _childBlockMap[targetPos] = c;
        }

        private List<ChildBlockSystem> GetExactNeighborBlocks(ChildBlockSystem b)
        {
            var list = new List<ChildBlockSystem>();
            var x = positionData.x;
            var y = positionData.y;

            if (b.position == Enums.ConnectionType.TopLeft)
            {
                if (x - 1 >= 0) list.Add(GetChildBlockAt(x - 1, y, Enums.ConnectionType.TopRight));
                if (y - 1 >= 0) list.Add(GetChildBlockAt(x, y - 1, Enums.ConnectionType.BottomLeft));
            }

            if (b.position == Enums.ConnectionType.TopRight)
            {
                if (x + 1 < BoardSize) list.Add(GetChildBlockAt(x + 1, y, Enums.ConnectionType.TopLeft));
                if (y - 1 >= 0) list.Add(GetChildBlockAt(x, y - 1, Enums.ConnectionType.BottomRight));
            }

            if (b.position == Enums.ConnectionType.BottomLeft)
            {
                if (x - 1 >= 0) list.Add(GetChildBlockAt(x - 1, y, Enums.ConnectionType.BottomRight));
                if (y + 1 < BoardSize) list.Add(GetChildBlockAt(x, y + 1, Enums.ConnectionType.TopLeft));
            }

            if (b.position == Enums.ConnectionType.BottomRight)
            {
                if (x + 1 < BoardSize) list.Add(GetChildBlockAt(x + 1, y, Enums.ConnectionType.BottomLeft));
                if (y + 1 < BoardSize) list.Add(GetChildBlockAt(x, y + 1, Enums.ConnectionType.TopRight));
            }

            return list;
        }

        private ChildBlockSystem GetChildBlockAt(int cx, int cy, Enums.ConnectionType cp)
        {
            if (_boardGrid == null) return null;
            var nb = _boardGrid.GetBlockSystemAt(cx, cy);
            if (nb == null) return null;
            if (nb._childBlockMap.TryGetValue(cp, out var child)) return child;
            return null;
        }

        private Enums.BlockPositionType GetBlockPositionType(Enums.ConnectionType ct)
        {
            if (ct == Enums.ConnectionType.TopLeft) return Enums.BlockPositionType.TopLeft;
            if (ct == Enums.ConnectionType.TopRight) return Enums.BlockPositionType.TopRight;
            if (ct == Enums.ConnectionType.BottomLeft) return Enums.BlockPositionType.BottomLeft;
            if (ct == Enums.ConnectionType.BottomRight) return Enums.BlockPositionType.BottomRight;
            return Enums.BlockPositionType.None;
        }

        private void OnDestroy()
        {
            if (_boardGrid != null) _boardGrid.UnregisterBlockSystem(positionData.x, positionData.y);
        }
    }
}