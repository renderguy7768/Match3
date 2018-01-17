using System.Collections;
using System.Collections.Generic;
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

        private void Start()
        {
            // Grab components and initialize arrays
            m_rectTransform = GetComponent<RectTransform>();
            //var tileParents = new RectTransform[m_height, m_width];
            m_cellparents = new CellParent[m_height, m_width];

            // Calculate layout values
            CellParent.ms_cellWidth = ((int)m_rectTransform.rect.width) / m_width;
            CellParent.ms_cellHeight = ((int)m_rectTransform.rect.height) / m_height;

            CellParent.ms_xPadding = (int)m_rectTransform.rect.width - (CellParent.ms_cellWidth * m_width);
            CellParent.ms_yPadding = (int)m_rectTransform.rect.height - (CellParent.ms_cellHeight * m_height);

            CellParent.ms_boxCollider2DSize = new Vector2(CellParent.ms_cellWidth - CellParent.ms_xPadding, CellParent.ms_cellHeight);
            CellParent.ms_boxCollider2DOffset = new Vector2(CellParent.ms_cellWidth, CellParent.ms_cellHeight) * 0.5f;

            // Create cells
            for (var ydx = 0; ydx < m_height; ++ydx)
            {
                for (var xdx = 0; xdx < m_width; ++xdx)
                {
                    // Creating parent gameobject
                    m_cellparents[ydx, xdx].m_rectTransform = new GameObject("cell " + xdx + ", " + ydx).AddComponent<RectTransform>();
                    m_cellparents[ydx, xdx].m_rectTransform.SetParent(m_rectTransform);
                    m_cellparents[ydx, xdx].m_rectTransform.localScale = Vector3.one;

                    /*// Adding boxcollider2d component
                    m_cellparents[ydx, xdx].m_boxCollider2D = m_cellparents[ydx, xdx].m_rectTransform.gameObject.AddComponent<BoxCollider2D>();
                    m_cellparents[ydx, xdx].m_boxCollider2D.size = CellParent.ms_boxCollider2DSize;
                    m_cellparents[ydx, xdx].m_boxCollider2D.offset = CellParent.ms_boxCollider2DOffset;

                    // Adding rigidbody2d component
                    m_cellparents[ydx, xdx].m_rigidbody2D = m_cellparents[ydx, xdx].m_rectTransform.gameObject.AddComponent<Rigidbody2D>();
                    m_cellparents[ydx, xdx].m_rigidbody2D.constraints =
                        RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                    m_cellparents[ydx, xdx].m_rigidbody2D.angularDrag = 0.0f;
                    m_cellparents[ydx, xdx].m_rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    m_cellparents[ydx, xdx].m_rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
                    m_cellparents[ydx, xdx].m_rigidbody2D.gravityScale = 10.0f;
                    m_cellparents[ydx, xdx].m_rigidbody2D.drag = 1.0f;*/

                    m_cellparents[ydx, xdx].m_rect = new Rect
                    {
                        width = CellParent.ms_cellWidth,
                        height = CellParent.ms_cellHeight,
                        x = CellParent.ms_cellWidth * xdx,
                        y = CellParent.ms_cellHeight * ydx
                    };


                    m_cellparents[ydx, xdx].m_rectTransform.pivot = new Vector2(0, 0);

                    m_cellparents[ydx, xdx].m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CellParent.ms_cellWidth);
                    m_cellparents[ydx, xdx].m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CellParent.ms_cellWidth);

                    m_cellparents[ydx, xdx].m_rectTransform.localPosition = new Vector3(
                        m_cellparents[ydx, xdx].m_rect.x - (m_rectTransform.rect.width / 2) + ((float)CellParent.ms_xPadding / 2),
                        (m_cellparents[ydx, xdx].m_rect.y - (m_rectTransform.rect.height / 2) + ((float)CellParent.ms_yPadding / 2)) /*+ m_rectTransform.rect.height*/,
                        0);

                    m_cellparents[ydx, xdx].m_cell = m_cellparents[ydx, xdx].m_rectTransform.gameObject.AddComponent<Cell>();
                    m_cellparents[ydx, xdx].m_cell.Setup(xdx, ydx, m_tileTypes);
                    m_cellparents[ydx, xdx].m_cell.Clicked += OnCellClicked;
                    m_cellparents[ydx, xdx].m_cell.Destroyed += OnCellDestroyed;
                }
            }

            // Fill in for the first time
            StartCoroutine(PopulateField());
        }

        public void SetCell(int x, int y, int tileType)
        {
            m_cellparents[y, x].m_cell.SetCell(tileType);
        }

        public IEnumerator PopulateField()
        {
            // TODO: Randomly set the cells to different tile types
            // Algorithm should ensure that there is at least one valid
            // swap and that there are existing no matches.
            for (var ydx = 0; ydx < m_height; ++ydx)
            {
                for (var xdx = 0; xdx < m_width; ++xdx)
                {
                    SetCell(xdx, ydx, (ydx * m_width + xdx) % m_tileTypes.Length);
                    yield return null;
                }
            }
        }

        public void OnCellClicked(Cell clicked)
        {
            if (m_lastClicked != null)
            {
                TrySwap(m_lastClicked, clicked);

                m_lastClicked = null;
            }
            else
            {
                m_lastClicked = clicked;
            }
        }

        public void OnCellDestroyed(Cell destroyed)
        {
            var cellsToBeMoved = new List<Cell>();
            for (var i = destroyed.Y + 1; i < m_height; i++)
            {
                cellsToBeMoved.Add(m_cellparents[i, destroyed.X].m_cell);
            }

            destroyed.MoveCells(cellsToBeMoved);
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

            SetCell(c1.X, c1.Y, t2);
            SetCell(c2.X, c2.Y, t1);
        }
    }
}
