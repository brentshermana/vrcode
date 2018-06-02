using System;

namespace vrcode.vr.vrui.panel
{
    public class PanelShape
    {
        private float width;
        private float height;
        private float radius;
        private float depth;
        private float squaresPerUnit;

        public PanelShape(float w, float h, float r, float d, float squaresPerUnit)
        {
            width = w;
            height = h;
            radius = r;
            depth = d;
            this.squaresPerUnit = squaresPerUnit;
        }
        
        public float Width
        {
            get { return width;{} }
        }

        public float Height
        {
            get { return height; }
        }

        public float Radius
        {
            get { return radius; }
        }

        public float Depth
        {
            get { return depth; }
        }

        public float SquaresPerUnit
        {
            get { return squaresPerUnit; }
        }
    }
}