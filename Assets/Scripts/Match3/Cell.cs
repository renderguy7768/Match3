using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 *************************** DO NOT FORGET TO CLEAN UP BEFORE SUBMITTING*************************
 **/

namespace Assets.Scripts.Match3
{
    public class Cell : Graphic, IPointerDownHandler, IPointerUpHandler
    {
        private const float MinMoveSpeed = 350.0f;
        private const float MaxMoveSpeed = MinMoveSpeed + 100.0f;

        private int _r;
        public int R { get { return _r; } }
        private int _c;
        public int C { get { return _c; } }
        private Vector3 _targetPosition;
        public Vector3 TargetPosition { get { return _targetPosition; } }

        // Treating CellType as Bit Flag
        private uint _cellType;
        public uint CellType
        {
            get { return _cellType; }
            private set { _cellType = 1u << (int)value; }
        }

        public int TileIndex { get; private set; }

        public event Action<Cell, Vector2> Clicked;
        public event Action<Vector2> Released;

        private RectTransform m_rectTransform;

        private static GameObject[] ms_tileTypes;
        private static float ms_boardMinY;

        public void Setup(int r, int c, float targetX, float targetY)
        {
            _r = r;
            _c = c;

            _cellType = 0;
            TileIndex = -1;

            m_rectTransform = GetComponent<RectTransform>();

            _targetPosition = new Vector3(targetX, targetY, 0);
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

        private void ClearCell()
        {
            if (m_rectTransform.childCount == 1)
            {
                Destroy(m_rectTransform.GetChild(0).gameObject);
            }

            _cellType = 0;
            TileIndex = -1;
        }

        public void SetCell(int tileType)
        {
            ClearCell();

            var tile = Instantiate(ms_tileTypes[tileType], m_rectTransform);

            var tileRect = tile.GetComponent<RectTransform>();

            tileRect.localScale = Vector3.one;
            tileRect.sizeDelta = Vector2.zero;
            tileRect.anchoredPosition = Vector2.zero;

            CellType = (uint)tileType;
            TileIndex = tileType;
        }

        public static void SetupStaticVariables(GameObject[] tileTypes, float boardHeight)
        {
            ms_tileTypes = tileTypes;
            ms_boardMinY = boardHeight;
        }

        private void Update()
        {
            var t = Mathf.Abs((rectTransform.localPosition.y - ms_boardMinY) / ms_boardMinY);
            var currentSpeed = Mathf.Lerp(MinMoveSpeed, MaxMoveSpeed, t);
            rectTransform.localPosition = Vector3.MoveTowards(rectTransform.localPosition, TargetPosition,
                currentSpeed * Time.deltaTime);
        }

        public void Swap(Cell otherCell)
        {
            Utility.Swap(ref _r, ref otherCell._r);
            Utility.Swap(ref _c, ref otherCell._c);
            Utility.Swap(ref _targetPosition, ref otherCell._targetPosition);

            gameObject.name = "cell " + R + ", " + C;
            otherCell.gameObject.name = "cell " + otherCell.R + ", " + otherCell.C;
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
