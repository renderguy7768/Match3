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

        public struct CellIndex
        {
            public int R;
            public int C;
        }
        public CellIndex ThisCellIndex;

        private Vector3 _targetPosition;
        public Vector3 TargetPosition { get { return _targetPosition; } }

        public Image ChildImage { get; private set; }
        public Color MatchColor { get; private set; }
        public bool IsMatched;

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

        private static GameObject[] ms_tileTypes;
        private static float ms_boardMinY;

        public void Setup(int r, int c, float targetX, float targetY)
        {
            ThisCellIndex.R = r;
            ThisCellIndex.C = c;

            _cellType = 0;
            TileIndex = -1;
            ChildImage = null;
            MatchColor = Color.clear;
            IsMatched = false;

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

        public void SetCell(int tileType)
        {
            if (transform.childCount == 1)
            {
                Destroy(transform.GetChild(0).gameObject);
            }

            var tile = Instantiate(ms_tileTypes[tileType], transform);

            var tileRect = tile.GetComponent<RectTransform>();

            tileRect.localScale = Vector3.one;
            tileRect.sizeDelta = Vector2.zero;
            tileRect.anchoredPosition = Vector2.zero;

            CellType = (uint)tileType;
            TileIndex = tileType;
            ChildImage = tile.GetComponent<Image>();
            var childColor = ChildImage.color;
            MatchColor = new Color(childColor.r, childColor.g, childColor.b,
                childColor.a * 0.5f);
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
            Utility.Swap(ref ThisCellIndex, ref otherCell.ThisCellIndex);
            Utility.Swap(ref _targetPosition, ref otherCell._targetPosition);
#if UNITY_EDITOR
            gameObject.name = "cell " + ThisCellIndex.R + ", " + ThisCellIndex.C;
            otherCell.gameObject.name = "cell " + otherCell.ThisCellIndex.R + ", " + otherCell.ThisCellIndex.C;
#endif
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
