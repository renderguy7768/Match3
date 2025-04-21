using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Match3
{
    [RequireComponent(typeof(RectTransform))]
    public class GridGenerator : MonoBehaviour
    {
        [Tooltip("Fill all the tile names from Resources/TilePrefabs here")]
        public string[] m_tileNames;

        [Tooltip("Grid Width (Number of Columns)")]
        public int m_width;

        [Tooltip("Grid Height (Number of Rows)")]
        public int m_height;

        [Tooltip("Increase this if tiles are being created on screen instead of Load Area")]
        [Range(1.0f, 20.0f)]
        public float m_startHeightOffsetMultiplier;

        [Tooltip("Increase or decrease this to adjust the tile removal delay")]
        [Range(0.5f, 5.0f)]
        public float RemoveTilesDelay;

        private GameObject[] m_tileTypePrefabs;

        private RectTransform m_rectTransform;
        private Cell[,] m_cells;

        private Cell m_firstClickedCell;
        private Vector2 m_initialPressPosition;
        private Vector2 m_finalPressPosition;

        private const float m_minSqrSwipeThreshold = 10.0f * 10.0f;
        private const float m_epsison = float.Epsilon * 10.0f;

        private Cell[,] m_cellsUnUsed;
        private List<Cell.CellInfo> m_matchedCellInfo;
        private float m_startHeightOffset;
        private List<Cell> m_recheckList;
        private uint _score = 0;

        private enum Direction : uint
        {
            Invalid = 0u,
            Left = 1u,
            Right = ~Left,
            Up = 2u,
            Down = ~Up,
            HorizontalAdjacent = 3u,
            VerticleAdjacent = ~HorizontalAdjacent
        }

        private enum GameState { Move, Wait }

        private GameState _gameState;

        private uint m_validIndexMask;

        private void Awake()
        {
            // Loading Tile Prefabs
            m_tileTypePrefabs = new GameObject[m_tileNames.Length];
            for (var i = 0; i < m_tileNames.Length; i++)
            {
                m_tileTypePrefabs[i] = Resources.Load<GameObject>(@"TilePrefabs\" + m_tileNames[i]);
            }

            // Setting initial Gamestate
            _gameState = GameState.Wait;
            Cell.IsBoardPopulated = false;
        }

        private void Start()
        {
            // Creating valid index mask
            var tileArrayLength = m_tileTypePrefabs.Length - 1;
            while (tileArrayLength >= 0)
            {
                m_validIndexMask |= 1u << tileArrayLength;
                tileArrayLength--;
            }

            // Grab components and initialize arrays
            m_rectTransform = GetComponent<RectTransform>();
            m_cells = new Cell[m_height, m_width];
            m_cellsUnUsed = new Cell[m_height, m_width];
            m_matchedCellInfo = new List<Cell.CellInfo>(m_height * m_width);
            m_recheckList = new List<Cell>(m_height * m_width);

            // Calculate layout values
            var cellWidth = ((int)m_rectTransform.rect.width) / m_width;
            var cellHeight = ((int)m_rectTransform.rect.height) / m_height;

            var xPadding = (int)m_rectTransform.rect.width - (cellWidth * m_width);
            var yPadding = (int)m_rectTransform.rect.height - (cellHeight * m_height);

            var xOffsetPerCell = (m_rectTransform.rect.width + xPadding) * 0.5f;
            var yOffsetPerCell = (m_rectTransform.rect.height + yPadding) * 0.5f;

            m_startHeightOffset = m_rectTransform.rect.height * m_startHeightOffsetMultiplier;

            Cell.SetupStaticVariables(m_tileTypePrefabs, m_rectTransform.rect.yMin);
            // Create cells
            for (var row = 0; row < m_height; ++row)
            {
                for (var column = 0; column < m_width; ++column)
                {
                    // Creating parent gameobject with all required components

                    var newCell = new GameObject("cell " + row + ", " + column);

                    newCell.AddComponent<CanvasRenderer>();
                    var rectTransform = newCell.AddComponent<RectTransform>();
                    rectTransform.SetParent(m_rectTransform);
                    rectTransform.localScale = Vector3.one;
                    rectTransform.pivot = Vector2.zero;
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellWidth);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellHeight);

                    var rect = new Rect
                    {
                        width = cellWidth,
                        height = cellHeight,
                        x = cellWidth * column,
                        y = cellHeight * row
                    };
                    var xPos = rect.x - xOffsetPerCell;
                    var yPos = rect.y - yOffsetPerCell;
                    rectTransform.localPosition = new Vector3(xPos, yPos + m_startHeightOffset, 0);

                    m_cells[row, column] = newCell.AddComponent<Cell>();
                    m_cells[row, column].Setup(row, column, xPos, yPos);
                    m_cells[row, column].Clicked += OnCellClicked;
                    m_cells[row, column].Released += OnCellReleased;
                    m_cells[row, column].color = Color.clear;
                }
            }

            // Fill in for the first time
            StartCoroutine(PopulateField());
        }

        private IEnumerator PopulateField()
        {
            for (var row = 0; row < m_height; ++row)
            {
                for (var column = 0; column < m_width; ++column)
                {
                    var tileType = Random.Range(0, m_tileTypePrefabs.Length);
                    uint validIndex;
                    if (CheckForMatchesDuringGeneration(row, column, tileType, out validIndex))
                    {
                        // Making a valid tile index list using the bits of valid Index
                        validIndex = ~validIndex;
                        validIndex &= m_validIndexMask;
                        tileType = GenerateAValidTileIndexFromValidIndexBits(validIndex);
                    }
                    m_cells[row, column].SetCell(tileType);

                    yield return null;
                }
            }

            var lastCell = m_cells[m_height - 1, m_width - 1];
            yield return new WaitWhile(() => Vector3.Distance(lastCell.ThisCellInfo.TargetPosition, lastCell.rectTransform.localPosition) > m_epsison);
            _gameState = GameState.Move;
            Cell.IsBoardPopulated = true;
        }

        private int GenerateAValidTileIndexFromValidIndexBits(uint validIndex)
        {
            var validTileIndex = new List<int>(m_tileTypePrefabs.Length);
            for (var i = 0; i < m_tileTypePrefabs.Length; i++)
            {
                if ((validIndex & (1 << i)) != 0)
                {
                    validTileIndex.Add(i);
                }
            }
            var validTileType = Random.Range(0, validTileIndex.Count);
            return validTileIndex[validTileType];
        }

        private bool CheckForMatchesDuringGeneration(int row, int column, int tileType, out uint validIndex)
        {
            var isMatchLeft = false;
            var isMatchDown = false;
            var cellType = 1u << tileType;
            validIndex = 1u << m_tileTypePrefabs.Length;

            if (row > 1 && column > 1)
            {
                // Check left and down
                isMatchLeft = CheckMatch3(row, column, cellType, ref validIndex, Direction.Left, Direction.Down);
                isMatchDown = CheckMatch3(row, column, cellType, ref validIndex, Direction.Down, Direction.Left);
            }
            else if (row <= 1 && column > 1)
            {
                // Check left
                isMatchLeft = CheckMatch3(row, column, cellType, ref validIndex, Direction.Left);
            }
            else if (row > 1 && column <= 1)
            {
                // Check down
                isMatchDown = CheckMatch3(row, column, cellType, ref validIndex, Direction.Down);
            }

            return isMatchLeft || isMatchDown;
        }

        private bool CheckMatch3(int row, int column, uint cellType, ref uint validIndex, Direction primaryDirection, Direction secondaryDirection = Direction.Invalid)
        {
            var r1 = -1;
            var c1 = -1;
            var r2 = -1;
            var c2 = -1;
            switch (primaryDirection)
            {
                case Direction.Left:
                    r1 = r2 = row;
                    c1 = column - 1;
                    c2 = column - 2;
                    break;
                case Direction.Down:
                    c1 = c2 = column;
                    r1 = row - 1;
                    r2 = row - 2;
                    break;
                default:
                    Debug.Assert(false, "This should not happen. Invalid Direction");
                    return false;
            }

            if ((m_cells[r1, c1].CellType &
                 m_cells[r2, c2].CellType &
                 cellType) == 0) return false;
            validIndex |= cellType;

            if (secondaryDirection == Direction.Invalid) return true;
            switch (secondaryDirection)
            {
                case Direction.Left:
                    r1 = r2 = row;
                    c1 = column - 1;
                    c2 = column - 2;
                    break;
                case Direction.Down:
                    c1 = c2 = column;
                    r1 = row - 1;
                    r2 = row - 2;
                    break;
                default:
                    Debug.Assert(false, "This should not happen.");
                    return false;
            }

            var secondaryCellType = m_cells[r1, c1].CellType &
                                    m_cells[r2, c2].CellType;
            if (secondaryCellType != 0)
            {
                validIndex |= secondaryCellType;
            }
            return true;
        }

        private void OnCellClicked(Cell clicked, Vector2 currentPointerPosition)
        {
            if (m_firstClickedCell != null || _gameState == GameState.Wait) return;
            m_firstClickedCell = clicked;
            m_initialPressPosition = currentPointerPosition;
        }

        private void OnCellReleased(Vector2 currentPointerPosition)
        {
            if (m_firstClickedCell == null) return;
            m_finalPressPosition = currentPointerPosition;
            Cell otherCell = null;
            var direction = GetDirection(m_firstClickedCell);

            switch (direction)
            {
                case Direction.Left:
                    otherCell = m_cells[m_firstClickedCell.ThisCellInfo.R, m_firstClickedCell.ThisCellInfo.C - 1];
                    break;
                case Direction.Right:
                    otherCell = m_cells[m_firstClickedCell.ThisCellInfo.R, m_firstClickedCell.ThisCellInfo.C + 1];
                    break;
                case Direction.Up:
                    otherCell = m_cells[m_firstClickedCell.ThisCellInfo.R + 1, m_firstClickedCell.ThisCellInfo.C];
                    break;
                case Direction.Down:
                    otherCell = m_cells[m_firstClickedCell.ThisCellInfo.R - 1, m_firstClickedCell.ThisCellInfo.C];
                    break;
            }

            if (otherCell != null)
            {
                var otherDirection = ~direction;
                StartCoroutine(TrySwap(m_firstClickedCell, otherCell, direction, otherDirection));
            }

            m_firstClickedCell = null;
        }

        private Direction GetDirection(Cell clicked)
        {
            var deltaPressPosition = m_finalPressPosition - m_initialPressPosition;

            if (deltaPressPosition.sqrMagnitude <= m_minSqrSwipeThreshold) return Direction.Invalid;

            var swipeAngle = Mathf.Atan2(deltaPressPosition.y, deltaPressPosition.x) * Mathf.Rad2Deg;

            if (swipeAngle > -45.0f && swipeAngle <= 45.0f && clicked.ThisCellInfo.C < m_width - 1)
            {
                return Direction.Right;
            }

            if (swipeAngle > 45.0f && swipeAngle <= 135.0f && clicked.ThisCellInfo.R < m_height - 1)
            {
                return Direction.Up;
            }

            if ((swipeAngle > 135.0f || swipeAngle <= -135.0f) && clicked.ThisCellInfo.C > 0)
            {
                return Direction.Left;
            }

            if (swipeAngle > -135.0f && swipeAngle <= -45.0f && clicked.ThisCellInfo.R > 0)
            {
                return Direction.Down;
            }

            return Direction.Invalid;
        }

        private IEnumerator TrySwap(Cell c1, Cell c2, Direction d1, Direction d2)
        {
            _gameState = GameState.Wait;
            ActualSwap(c1, c2);
            var checkCell1 = CheckForMatchesDuringGame(c1, d2);
            var checkCell2 = CheckForMatchesDuringGame(c2, d1);
            yield return new WaitForSeconds(RemoveTilesDelay);
            if (!checkCell1 && !checkCell2)
            {
                ActualSwap(c2, c1);
                _gameState = GameState.Move;
            }
            else
            {
                yield return new WaitUntil(RemoveMatches);
                StartCoroutine(CollapseColumns());
            }
        }

        private bool RemoveMatches()
        {
            m_matchedCellInfo = m_matchedCellInfo.Distinct().ToList();
            m_matchedCellInfo.Sort((info1, info2) =>
            (info1.R * m_height + info1.C).CompareTo(info2.R * m_height + info2.C));
            foreach (var index in m_matchedCellInfo)
            {
                if (m_cells[index.R, index.C] == null) continue;

                m_cells[index.R, index.C].IsMatched = true;

                m_cellsUnUsed[index.R, index.C] = m_cells[index.R, index.C];

                var previousPosition = m_cells[index.R, index.C].rectTransform.localPosition;
                m_cells[index.R, index.C].rectTransform.localPosition = new Vector3(previousPosition.x, previousPosition.y + m_startHeightOffset, 0);

                m_cells[index.R, index.C] = null;
            }

            return true;
        }

        private IEnumerator CollapseColumns()
        {
            foreach (var cellinfo in m_matchedCellInfo)
            {
                var row = cellinfo.R;
                var col = cellinfo.C;
                for (int i = row + 1, j = row; i < m_height; i++, j++)
                {
                    if (m_cellsUnUsed[j, col] == null) break;
                    m_cells[j, col] = m_cellsUnUsed[j, col];
                    m_cellsUnUsed[j, col] = null;

                    while (i < m_height && m_cells[i, col] == null)
                    {
                        i++;
                    }

                    if (i < m_height)
                    {
                        ActualSwap(m_cells[i, col], m_cells[j, col]);
                        if (!m_cells[j, col].IsMatched)
                        {
                            m_recheckList.Add(m_cells[j, col]);
                        }
                        m_cellsUnUsed[i, col] = m_cells[i, col];
                        m_cells[i, col] = null;
                    }

                    yield return null;
                }
            }
            m_matchedCellInfo.Clear();
            if (m_recheckList.Count > 0)
            {
                StartCoroutine(ReCheck());
            }
            else
            {
                StartCoroutine(Refill());
                _gameState = GameState.Move;
            }
        }

        private IEnumerator ReCheck()
        {
            var checkCell = false;
            foreach (var cell in m_recheckList)
            {
                var tempCell = cell;
                yield return new WaitWhile(() => Vector3.Distance(tempCell.ThisCellInfo.TargetPosition, tempCell.rectTransform.localPosition) > m_epsison);
                checkCell |= CheckForMatchesDuringGame(cell, Direction.Invalid);
            }
            m_recheckList.Clear();
            if (checkCell)
            {
                yield return new WaitUntil(RemoveMatches);
                StartCoroutine(CollapseColumns());
            }
            else
            {
                StartCoroutine(Refill());
            }
        }

        private IEnumerator Refill()
        {
            for (var row = 0; row < m_height; ++row)
            {
                for (var column = 0; column < m_width; ++column)
                {
                    if (m_cells[row, column] != null && m_cells[row, column].IsMatched)
                    {
                        m_recheckList.Add(m_cells[row, column]);
                        m_cells[row, column].SetCell(Random.Range(0, m_tileTypePrefabs.Length));
                    }

                    if (m_cellsUnUsed[row, column] != null && m_cellsUnUsed[row, column].IsMatched)
                    {
                        m_recheckList.Add(m_cellsUnUsed[row, column]);
                        m_cellsUnUsed[row, column].SetCell(Random.Range(0, m_tileTypePrefabs.Length));
                        m_cells[row, column] = m_cellsUnUsed[row, column];
                        m_cellsUnUsed[row, column] = null;
                    }

                    yield return null;
                }
            }
            if (m_recheckList.Count > 0)
            {
                StartCoroutine(ReCheck());
            }
            else
            {
                _gameState = GameState.Move;
            }
        }

        private bool CheckForMatchesDuringGame(Cell cellUnderCheck, Direction ignoreDirection)
        {
            var row = cellUnderCheck.ThisCellInfo.R;
            var column = cellUnderCheck.ThisCellInfo.C;

            var isMatchLeft = false;
            var isMatchRight = false;
            var isMatchUp = false;
            var isMatchDown = false;
            var isMatchHorizontalAdjacent = false;
            var isMatchVerticleAdjacent = false;

            if (row > 1 && row < m_height - 2)
            {
                // check up & down if it does not match ignore dir
                if (ignoreDirection != Direction.Up)
                {
                    isMatchUp = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Up);
                }
                if (ignoreDirection != Direction.Down)
                {
                    isMatchDown = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Down);
                }
            }
            else if (row <= 1 && ignoreDirection != Direction.Up)
            {
                // check up 
                isMatchUp = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Up);
            }
            else if (row >= m_height - 2 && ignoreDirection != Direction.Down)
            {
                // check down
                isMatchDown = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Down);
            }

            if (column > 1 && column < m_width - 2)
            {
                // check left & right if it does not match ignore dir
                if (ignoreDirection != Direction.Left)
                {
                    isMatchLeft = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Left);
                }
                if (ignoreDirection != Direction.Right)
                {
                    isMatchRight = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Right);
                }
            }
            else if (column <= 1 && ignoreDirection != Direction.Right)
            {
                // check right 
                isMatchRight = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Right);
            }
            else if (column >= m_height - 2 && ignoreDirection != Direction.Left)
            {
                // check left
                isMatchLeft = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.Left);
            }

            if (row > 0 && row < m_height - 1)
            {
                isMatchVerticleAdjacent = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.VerticleAdjacent);
            }

            if (column > 0 && column < m_width - 1)
            {
                isMatchHorizontalAdjacent = CheckMatch3DuringGame(row, column, cellUnderCheck.CellType, Direction.HorizontalAdjacent);
            }

            return isMatchLeft || isMatchRight || isMatchUp || isMatchDown || isMatchHorizontalAdjacent ||
                   isMatchVerticleAdjacent;
        }

        private bool CheckMatch3DuringGame(int row, int column, uint cellType, Direction direction)
        {
            int r1, c1, r2, c2;
            switch (direction)
            {
                case Direction.Left:
                    r1 = r2 = row;
                    c1 = column - 1;
                    c2 = column - 2;
                    break;
                case Direction.Right:
                    r1 = r2 = row;
                    c1 = column + 1;
                    c2 = column + 2;
                    break;
                case Direction.Up:
                    c1 = c2 = column;
                    r1 = row + 1;
                    r2 = row + 2;
                    break;
                case Direction.Down:
                    c1 = c2 = column;
                    r1 = row - 1;
                    r2 = row - 2;
                    break;
                case Direction.HorizontalAdjacent:
                    r1 = r2 = row;
                    c1 = column - 1;
                    c2 = column + 1;
                    break;
                case Direction.VerticleAdjacent:
                    c1 = c2 = column;
                    r1 = row - 1;
                    r2 = row + 1;
                    break;
                default:
                    Debug.Assert(false, "This should not happen. Invalid Direction");
                    return false;
            }

            if (m_cells[r1, c1] == null || m_cells[r2, c2] == null) return false;

            if ((m_cells[r1, c1].CellType &
                 m_cells[r2, c2].CellType &
                 cellType) == 0) return false;

            m_cells[r1, c1].ChildImage.color = m_cells[r2, c2].ChildImage.color =
                m_cells[row, column].ChildImage.color = m_cells[row, column].MatchColor;

            m_matchedCellInfo.Add(m_cells[r1, c1].ThisCellInfo);
            m_matchedCellInfo.Add(m_cells[r2, c2].ThisCellInfo);
            m_matchedCellInfo.Add(m_cells[row, column].ThisCellInfo);

            _score += 3;
            Score.OnScoreChanged.Invoke(_score);

            return true;
        }

        private void ActualSwap(Cell c1, Cell c2)
        {
#if UNITY_EDITOR
            var tempSiblingIndex = c1.transform.GetSiblingIndex();
            c1.transform.SetSiblingIndex(c2.transform.GetSiblingIndex());
            c2.transform.SetSiblingIndex(tempSiblingIndex);
#endif

            Utility.Swap(ref m_cells[c1.ThisCellInfo.R, c1.ThisCellInfo.C], ref m_cells[c2.ThisCellInfo.R, c2.ThisCellInfo.C]);
            c1.Swap(c2);
        }
    }
}
