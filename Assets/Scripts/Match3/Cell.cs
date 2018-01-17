using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Match3
{
    public class Cell : Graphic, IPointerClickHandler
    {
        public int X;//{ get; private set; }
        public int Y; //{ get; private set; }

        public int CellType { get; private set; }
        public event Action<Cell> Clicked;
        public event Action<Cell> Destroyed;

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
            if (Clicked != null && eventData.button == PointerEventData.InputButton.Left)
            {
                Clicked(this);
            }
            else if (Destroyed != null && eventData.button == PointerEventData.InputButton.Right)
            {
                Destroyed(this);
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

        public void MoveCells(List<Cell> cellsToBeMoved)
        {
            ClearCell();
            var nextParent = m_rectTransform;
            for (var i = 0; i < cellsToBeMoved.Count; i++)
            {
                var previousChild = cellsToBeMoved[i].m_rectTransform.GetChild(0).GetComponent<RectTransform>();
                previousChild.SetParent(nextParent);
                previousChild.anchoredPosition = Vector2.zero;
                nextParent = cellsToBeMoved[i].m_rectTransform;
            }

            
            //var previousTileType = CellType;
            /*
            var previousTileRect = previousRectTranform.GetChild(0).GetComponent<RectTransform>();
            var previousLocalScale = previousTileRect.localScale;
            var previousSizeDelta = previousTileRect.sizeDelta;
            var previousAnchoredPosition = previousTileRect.anchoredPosition;

            

            foreach (var cell in cellsToBeMoved)
            {
                previousTileRect.SetParent(previousRectTranform);
                previousTileRect.localScale = previousLocalScale;
                previousTileRect.sizeDelta = previousSizeDelta;
                previousTileRect.anchoredPosition = previousAnchoredPosition;
                previousRectTranform = cell.m_rectTransform;
                previousTileRect = previousRectTranform.GetChild(0).GetComponent<RectTransform>();
                previousLocalScale = previousTileRect.localScale;
                previousSizeDelta = previousTileRect.sizeDelta;
                previousAnchoredPosition = previousTileRect.anchoredPosition;
            }*/

            //var tile = Instantiate(m_tileTypes[tileType], m_rectTransform);

            //var tileRect = tile.GetComponent<RectTransform>();
            /*var prefabRect = m_tileTypes[tileType].GetComponent<RectTransform>();

            tileRect.localScale = prefabRect.localScale;

            tileRect.sizeDelta = prefabRect.sizeDelta;
            tileRect.anchoredPosition = prefabRect.anchoredPosition;

            CellType = tileType;*/


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
