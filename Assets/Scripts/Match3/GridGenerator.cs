using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Match3
{
    [RequireComponent(typeof(RectTransform))]
    public class GridGenerator : MonoBehaviour
    {
        public GameObject[] m_tileTypes;
        public int m_width;
        public int m_height;

        private RectTransform m_rectTransform;
        private CellParent[,] m_cellparents;

        private Cell m_lastClicked;
        private Vector2 m_initialPressPosition;
        private Vector2 m_finalPressPosition;

        private enum Direction { Invalid, Left, Right, Up, Down }

        private void Start()
        {
            // Grab components and initialize arrays
            m_rectTransform = GetComponent<RectTransform>();
            //var tileParents = new RectTransform[m_height, m_width];
            m_cellparents = new CellParent[m_width, m_height];

            // Calculate layout values
            CellParent.ms_cellWidth = ((int)m_rectTransform.rect.width) / m_width;
            CellParent.ms_cellHeight = ((int)m_rectTransform.rect.height) / m_height;

            CellParent.ms_xPadding = (int)m_rectTransform.rect.width - (CellParent.ms_cellWidth * m_width);
            CellParent.ms_yPadding = (int)m_rectTransform.rect.height - (CellParent.ms_cellHeight * m_height);

            CellParent.ms_boxCollider2DSize = new Vector2(CellParent.ms_cellWidth - CellParent.ms_xPadding, CellParent.ms_cellHeight);
            CellParent.ms_boxCollider2DOffset = new Vector2(CellParent.ms_cellWidth, CellParent.ms_cellHeight) * 0.5f;

            // Create cells
            for (var row = 0; row < m_width; ++row)
            {
                for (var column = 0; column < m_height; ++column)
                {
                    // Creating parent gameobject
                    m_cellparents[row, column].m_rectTransform = new GameObject("cell " + row + ", " + column).AddComponent<RectTransform>();
                    m_cellparents[row, column].m_rectTransform.SetParent(m_rectTransform);
                    m_cellparents[row, column].m_rectTransform.localScale = Vector3.one;

                    // Adding boxcollider2d component
                    m_cellparents[row, column].m_boxCollider2D = m_cellparents[row, column].m_rectTransform.gameObject.AddComponent<BoxCollider2D>();
                    m_cellparents[row, column].m_boxCollider2D.size = CellParent.ms_boxCollider2DSize;
                    m_cellparents[row, column].m_boxCollider2D.offset = CellParent.ms_boxCollider2DOffset;

                    // Adding rigidbody2d component
                    m_cellparents[row, column].m_rigidbody2D = m_cellparents[row, column].m_rectTransform.gameObject.AddComponent<Rigidbody2D>();
                    m_cellparents[row, column].m_rigidbody2D.constraints =
                        RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                    m_cellparents[row, column].m_rigidbody2D.angularDrag = 0.0f;
                    m_cellparents[row, column].m_rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    m_cellparents[row, column].m_rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
                    m_cellparents[row, column].m_rigidbody2D.gravityScale = 10.0f;
                    m_cellparents[row, column].m_rigidbody2D.drag = 1.0f;

                    m_cellparents[row, column].m_rect = new Rect
                    {
                        width = CellParent.ms_cellWidth,
                        height = CellParent.ms_cellHeight,
                        x = CellParent.ms_cellWidth * row,
                        y = CellParent.ms_cellHeight * column
                    };


                    m_cellparents[row, column].m_rectTransform.pivot = new Vector2(0, 0);

                    m_cellparents[row, column].m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CellParent.ms_cellWidth);
                    m_cellparents[row, column].m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CellParent.ms_cellWidth);

                    m_cellparents[row, column].m_rectTransform.localPosition = new Vector3(
                        m_cellparents[row, column].m_rect.x - (m_rectTransform.rect.width / 2) + ((float)CellParent.ms_xPadding / 2),
                        (m_cellparents[row, column].m_rect.y - (m_rectTransform.rect.height / 2) + ((float)CellParent.ms_yPadding / 2)) + m_rectTransform.rect.height,
                        0);

                    m_cellparents[row, column].m_cell = m_cellparents[row, column].m_rectTransform.gameObject.AddComponent<Cell>();
                    m_cellparents[row, column].m_cell.Setup(row, column, m_tileTypes);
                    m_cellparents[row, column].m_cell.Clicked += OnCellClicked;
                    m_cellparents[row, column].m_cell.Released += OnCellReleased;
                }
            }

            // Fill in for the first time
            StartCoroutine(PopulateField());
        }



        public void SetCell(int r, int c, int tileType)
        {
            m_cellparents[r, c].m_cell.SetCell(tileType);
        }

        public IEnumerator PopulateField()
        {
            // TODO: Randomly set the cells to different tile types
            // Algorithm should ensure that there is at least one valid
            // swap and that there are existing no matches.
            for (var row = 0; row < m_width; ++row)
            {
                for (var column = 0; column < m_height; ++column)
                {
                    SetCell(row, column, (column * m_width + row) % m_tileTypes.Length);
                    yield return null;
                }
            }
        }

        public void OnCellClicked(Cell clicked, Vector2 currentPointerPosition)
        {
            if (m_lastClicked != null) return;
            m_lastClicked = clicked;
            m_initialPressPosition = currentPointerPosition;
        }

        public void OnCellReleased(Vector2 currentPointerPosition)
        {
            if (m_lastClicked == null) return;
            m_finalPressPosition = currentPointerPosition;
            print(GetDirection(m_lastClicked));
            //TrySwap(m_lastClicked, clicked);
            m_lastClicked = null;
        }

        private Direction GetDirection(Cell clicked)
        {
            var deltaPressPosition = m_finalPressPosition - m_initialPressPosition;
            var swipeAngle = Mathf.Atan2(deltaPressPosition.y, deltaPressPosition.x) * Mathf.Rad2Deg;

            if (swipeAngle > -45.0f && swipeAngle <= 45.0f && clicked.R < m_width - 1)
            {
                return Direction.Right;
            }
            if (swipeAngle > 45.0f && swipeAngle <= 135.0f && clicked.C < m_height - 1)
            {
                return Direction.Up;
            }
            if ((swipeAngle > 135.0f || swipeAngle <= -135.0f) && clicked.R > 0)
            {
                return Direction.Left;
            }
            if (swipeAngle > -135.0f && swipeAngle <= -45.0f && clicked.C > 0)
            {
                return Direction.Down;
            }

            return Direction.Invalid;
        }

        private void TrySwap(Cell c1, Cell c2)
        {
            // TODO: Swapping should only occur if the swap
            // will result in a match.
            // If the swap is valid then the Grid should
            // Find all matches, clear matches, fill in empty
            // cells and repeat until there are no matches.

            var t1 = c1.CellType;
            var t2 = c2.CellType;

            SetCell(c1.R, c1.C, t2);
            SetCell(c2.R, c2.C, t1);
        }
    }
}
