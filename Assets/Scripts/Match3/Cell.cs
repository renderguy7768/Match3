using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Match3
{
    public class Cell : Graphic, IPointerDownHandler, IPointerUpHandler
    {
        private const float InitMinMoveSpeed = 350.0f;
        private const float InitMaxMoveSpeed = InitMinMoveSpeed + 100.0f;

        private const float GameMinMoveSpeed = InitMinMoveSpeed * 2.0f;
        private const float GameMaxMoveSpeed = GameMinMoveSpeed + 100.0f;

        public static bool IsBoardPopulated;

        [Serializable]
        public struct CellInfo
        {
            public int R;
            public int C;

            public Vector3 TargetPosition;
        }
        public CellInfo ThisCellInfo;

        public Image ChildImage { get; private set; }
        public Color MatchColor { get; private set; }
        public bool IsMatched { get; set; }

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
            ResetCell();

            ThisCellInfo.R = r;
            ThisCellInfo.C = c;

            ThisCellInfo.TargetPosition = new Vector3(targetX, targetY, 0);
        }

        private void ResetCell()
        {
            if (transform.childCount == 1)
            {
                Destroy(transform.GetChild(0).gameObject);
            }
            _cellType = 0;
            TileIndex = -1;
            ChildImage = null;
            MatchColor = Color.clear;
            IsMatched = false;
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
            ResetCell();

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
            if (IsMatched) return;
            var t = Mathf.Abs((rectTransform.localPosition.y - ms_boardMinY) / ms_boardMinY);
            var currentSpeed = IsBoardPopulated
                ? Mathf.Lerp(GameMinMoveSpeed, GameMaxMoveSpeed, t)
                : Mathf.Lerp(InitMinMoveSpeed, InitMaxMoveSpeed, t);
            rectTransform.localPosition =
                Vector3.MoveTowards(
                    rectTransform.localPosition,
                    ThisCellInfo.TargetPosition,
                    currentSpeed * Time.deltaTime);
        }

        public void Swap(Cell otherCell)
        {
            Utility.Swap(ref ThisCellInfo, ref otherCell.ThisCellInfo);
#if UNITY_EDITOR
            gameObject.name = "cell " + ThisCellInfo.R + ", " + ThisCellInfo.C;
            otherCell.gameObject.name = "cell " + otherCell.ThisCellInfo.R + ", " + otherCell.ThisCellInfo.C;
#endif
        }
    }
}
