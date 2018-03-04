using UnityEngine;
using CurvedUI;
using TMPro;

namespace vrcode.vr.vrui.panel
{
    public class VRUI_CurvedText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private CurvedUISettings curvedUiSettings;

        private Vector2 canvasDim;

        void Update()
        {
            UpdateTmpRectSize();
        }

        // called once after instantiation
        public void Init()
        {
            // center on parent, which should be a panel
            transform.localPosition = Vector3.zero;
            // due to how UI works, must turn around
            transform.localRotation = Quaternion.AngleAxis(180, transform.parent.up);
        }

        // called when panel's shape changes
        public void MatchShape(PanelShape shape)
        {
            SetCurvature(shape);

            canvasDim = CanvasDimensions(shape);
            GetComponent<RectTransform>().sizeDelta = canvasDim;
            
        }

        // the TMP object is masked, so it can have a size larger than the rest of the canvas
        protected void UpdateTmpRectSize()
        {
            Vector2 tmpExtents = text.bounds.extents * 2f;
            
            // height should scale with dimensions of the text
            float height = Mathf.Max(tmpExtents.y, canvasDim.y);
            // width shouldn't scale
            float width = canvasDim.x;
            text.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(height, width);
        }

        // the canvas's scale may not be the same as the panel's,
        // so a conversion must be made to get the canvas size
        // which equals the panel's
        protected Vector2 CanvasDimensions(PanelShape shape)
        {
            Vector3 scale = GetComponent<RectTransform>().localScale;
            return new Vector2(shape.Width / scale.x, shape.Height / scale.y);
        }
        
        // sets curvature for curvedUi which matches panel's curvature
        protected void SetCurvature(PanelShape shape)
        {
            float angleDegrees = 360f * shape.Width / (2f * Mathf.PI * shape.Radius);
            // use ceil because if the text has less curvature than the screen, the
            // edges of the text could be inside the screen
            curvedUiSettings.Angle = Mathf.CeilToInt(angleDegrees);
        }
    }
}