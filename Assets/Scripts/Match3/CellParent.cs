using UnityEngine;

namespace Assets.Scripts.Match3
{
    public class CellParent
    {
        #region Static Properties
        public static int ms_cellWidth { get; set; }
        public static int ms_cellHeight { get; set; }
        public static int ms_xPadding { get; set; }
        public static int ms_yPadding { get; set; }
        public static Vector2 ms_boxCollider2DSize { get; set; }
        public static Vector2 ms_boxCollider2DOffset { get; set; }
        #endregion

        #region Instance Properties
        public GameObject m_gameObject { get; private set; }
        public RectTransform m_rectTransform { get; private set; }
        public Rigidbody2D m_rigidbody2D { get; private set; }
        public Rect m_rect { get; private set; }
        public BoxCollider2D m_boxCollider2D { get; private set; }
        public Cell m_cell { get; private set; }
        #endregion

        public CellParent(int row, int column, RectTransform parent)
        {
            m_gameObject = new GameObject("cell " + row + ", " + column);

            m_rectTransform = m_gameObject.AddComponent<RectTransform>();
            m_rectTransform.SetParent(parent);
            m_rectTransform.localScale = Vector3.one;
            m_rectTransform.pivot = new Vector2(0, 0);
            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ms_cellWidth);
            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ms_cellWidth);

            m_rect = new Rect
            {
                width = ms_cellWidth,
                height = ms_cellHeight,
                x = ms_cellWidth * column,
                y = ms_cellHeight * row
            };
            m_rectTransform.localPosition = new Vector3(
                m_rect.x - (parent.rect.width / 2) + ((float)ms_xPadding / 2),
                (m_rect.y - (parent.rect.height / 2) + ((float)ms_yPadding / 2)) + parent.rect.height,
                0);

            m_rigidbody2D = m_gameObject.AddComponent<Rigidbody2D>();
            m_rigidbody2D.constraints =
                RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            m_rigidbody2D.angularDrag = 0.0f;
            m_rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            m_rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            m_rigidbody2D.gravityScale = 10.0f;
            m_rigidbody2D.drag = 1.0f;

            m_boxCollider2D = m_gameObject.AddComponent<BoxCollider2D>();
            m_boxCollider2D.size = ms_boxCollider2DSize;
            m_boxCollider2D.offset = ms_boxCollider2DOffset;

            m_cell = m_gameObject.AddComponent<Cell>();
        }
    }
}

