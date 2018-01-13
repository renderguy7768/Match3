using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : Graphic, IPointerClickHandler
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public int CellType { get; private set; }
    public event Action<Cell> Clicked;

    private RectTransform m_rectTransform;
    private GameObject[] m_tileTypes;
    
    public void Setup(int x, int y, GameObject[] tileTypes)
    {
        m_tileTypes = tileTypes;

        X = x;
        Y = y;

        CellType = -1;

        m_rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(Clicked != null)
        {
            Clicked(this);
        }
    }

    public void SetCellType(int to)
    {
        CellType = to;
    }

    public void ClearCell()
    {
        if (m_rectTransform.childCount == 1)
        {
            Destroy(m_rectTransform.GetChild(0).gameObject);
        }

        CellType = -1;
    }
    
    public void SetCell(int tileType)
    {
        ClearCell();

        GameObject tile = Instantiate(m_tileTypes[tileType], m_rectTransform);

        RectTransform tileRect = tile.GetComponent<RectTransform>();
        RectTransform prefabRect = m_tileTypes[tileType].GetComponent<RectTransform>();

        tileRect.localScale = prefabRect.localScale;

        tileRect.sizeDelta = prefabRect.sizeDelta;
        tileRect.anchoredPosition = prefabRect.anchoredPosition;

        CellType = tileType;
    }

    ////////////////////////////////////////////////////////
    // Overrides for Graphic in order to have an invisible
    // UI Collider
    ////////////////////////////////////////////////////////
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        return;
    }

    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        return true;
    }
}
