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
        private Cell[,] m_cells;

        private Cell m_lastClicked;

        private void Start()
        {
            // Grab components and initialize arrays
            m_rectTransform = GetComponent<RectTransform>();
            var tileParents = new RectTransform[m_height, m_width];
            m_cells = new Cell[m_height, m_width];

            // Calculate layout values
            var cellWidth = ((int)m_rectTransform.rect.width) / m_width;
            var cellHeight = ((int)m_rectTransform.rect.height) / m_height;

            var xPadding = (int)m_rectTransform.rect.width - (cellWidth * m_width);
            var yPadding = (int)m_rectTransform.rect.height - (cellHeight * m_height);

            var boxCollider2DSize = new Vector2(cellWidth - xPadding, cellHeight);
            var boxCollider2DOffset = new Vector2(cellWidth, cellHeight) * 0.5f;

            // Create cells
            for (var ydx = 0; ydx < m_height; ++ydx)
            {
                for (var xdx = 0; xdx < m_width; ++xdx)
                {
                    // Creating parent gameobject
                    tileParents[ydx, xdx] = new GameObject("cell " + xdx + ", " + ydx).AddComponent<RectTransform>();
                    tileParents[ydx, xdx].SetParent(m_rectTransform);
                    tileParents[ydx, xdx].localScale = Vector3.one;

                    // Adding boxcollider2d component
                    var boxCollider2D = tileParents[ydx, xdx].gameObject.AddComponent<BoxCollider2D>();
                    boxCollider2D.size = boxCollider2DSize;
                    boxCollider2D.offset = boxCollider2DOffset;

                    // Adding rigidbody2d component
                    var rigidBody2D = tileParents[ydx, xdx].gameObject.AddComponent<Rigidbody2D>();
                    rigidBody2D.constraints =
                        RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                    rigidBody2D.angularDrag = 0.0f;
                    rigidBody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    rigidBody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
                    rigidBody2D.gravityScale = 10.0f;

                    var r = new Rect
                    {
                        width = cellWidth,
                        height = cellHeight,
                        x = cellWidth * xdx,
                        y = cellHeight * ydx
                    };


                    tileParents[ydx, xdx].pivot = new Vector2(0, 0);

                    tileParents[ydx, xdx].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellWidth);
                    tileParents[ydx, xdx].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellWidth);

                    tileParents[ydx, xdx].localPosition = new Vector3(
                        r.x - (m_rectTransform.rect.width / 2) + ((float)xPadding / 2),
                        r.y - (m_rectTransform.rect.height / 2) + ((float)yPadding / 2),
                        0);

                    m_cells[ydx, xdx] = tileParents[ydx, xdx].gameObject.AddComponent<Cell>();
                    m_cells[ydx, xdx].Setup(xdx, ydx, m_tileTypes);
                    m_cells[ydx, xdx].Clicked += OnCellClicked;
                }
            }

            // Fill in for the first time
            StartCoroutine(PopulateField());
        }



        public void SetCell(int x, int y, int tileType)
        {
            m_cells[y, x].SetCell(tileType);
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
