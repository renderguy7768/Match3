using UnityEngine;

namespace Assets.Scripts.Match3
{
    public struct CellParent
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
        public RectTransform m_rectTransform { get; set; }
        //public Rigidbody2D m_rigidbody2D { get; set; }
        public Rect m_rect { get; set; }
        //public BoxCollider2D m_boxCollider2D { get; set; }
        public Cell m_cell { get; set; }
        #endregion
    }
}
