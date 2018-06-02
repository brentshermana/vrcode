using UnityEngine;

using TMPro;

namespace vrcode.vr.vrui.panel
{
    public class VRUI_TextPanel : VRUI_Panel
    {
        // replaces 'Start'
        protected override void Init()
        {
            base.Init();
            OnResize += InitText;
        }

        
        protected void InitText(PanelShape shape)
        {
            // instantiate the canvas holding the text
            GameObject textCanvas = Instantiate(Resources.Load<GameObject>("Vrui/VRUI_Canvas"));
            textCanvas.transform.parent = transform;
            
            // other instantiation:
            VRUI_CurvedText curvedText = textCanvas.GetComponent<VRUI_CurvedText>();
            curvedText.Init();
            curvedText.MatchShape(shape);
        }
    }
}