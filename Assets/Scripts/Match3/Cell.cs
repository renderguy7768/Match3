using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Match3
{
    public class Cell : Graphic, IPointerDownHandler, IPointerUpHandler
    {
        public int R; //{ get; private set; }
        public int C; //{ get; private set; }

        public int CellType { get; private set; }
        public event Action<Cell, Vector2> Clicked;
        public event Action<Vector2> Released;

        private RectTransform m_rectTransform;
        private GameObject[] m_tileTypes;

        public void Setup(int x, int y, GameObject[] tileTypes)
        {
            m_tileTypes = tileTypes;

            R = x;
            C = y;

            CellType = -1;

            m_rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Clicked != null && eventData.button == PointerEventData.InputButton.Left)
            {
                Clicked(this, eventData.position);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Released != null && eventData.button == PointerEventData.InputButton.Left)
            {
                Released(eventData.position);
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

            var tile = Instantiate(m_tileTypes[tileType], m_rectTransform);

            var tileRect = tile.GetComponent<RectTransform>();
            var prefabRect = m_tileTypes[tileType].GetComponent<RectTransform>();

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
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return true;
        }
    }
}
