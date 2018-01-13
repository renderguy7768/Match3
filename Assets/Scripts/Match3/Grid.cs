using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Grid : MonoBehaviour
{
    public GameObject[] m_tileTypes;
    public int m_width;
    public int m_height;

    private RectTransform m_rectTransform;
    private Cell[,] m_cells;

    private Cell m_lastClicked = null;

    void Start()
    {
        // Grab components and initialize arrays
        m_rectTransform = GetComponent<RectTransform>();
        RectTransform[,] tileParents = new RectTransform[m_height, m_width];
        m_cells = new Cell[m_height, m_width];

        // Calculate layout values
        int cellWidth = ((int)m_rectTransform.rect.width) / m_width;
        int cellHeight = ((int)m_rectTransform.rect.height) / m_height;

        int xPadding = (int)m_rectTransform.rect.width - (cellWidth * m_width);
        int yPadding = (int)m_rectTransform.rect.height - (cellHeight * m_height);

        // Create cells
        for ( int ydx = 0; ydx < m_height; ++ydx)
        {
            for ( int xdx = 0; xdx < m_width; ++xdx)
            {
                tileParents[ydx, xdx] = new GameObject("cell " + xdx + ", " + ydx).AddComponent<RectTransform>();
                tileParents[ydx, xdx].SetParent(m_rectTransform);
                tileParents[ydx, xdx].localScale = Vector3.one;

                Rect r = new Rect();
                r.width = cellWidth;
                r.height = cellHeight;

                r.x = cellWidth * xdx;
                r.y = cellHeight * ydx;

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
        for ( int ydx = 0; ydx < m_height; ++ydx )
        {
            for ( int xdx = 0; xdx < m_width; ++xdx)
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
        
        int t1 = c1.CellType;
        int t2 = c2.CellType;

        SetCell(c1.X, c1.Y, t2);
        SetCell(c2.X, c2.Y, t1);
    }
}
