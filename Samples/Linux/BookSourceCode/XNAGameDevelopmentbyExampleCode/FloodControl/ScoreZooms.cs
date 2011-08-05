using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class ScoreZoom
    {
        public string Text;
        public Color DrawColor;
        public int displayCounter;
        public int MaxDisplayCount = 30;
        private float scale = 0.4f;
        private float lastScaleAmount = 0.0f;
        private float scaleAmount = 0.4f;

        public float Scale
        {
            get { return scaleAmount * displayCounter; }
        }

        public bool IsCompleted
        {
            get { return (displayCounter > MaxDisplayCount); }
        }

        public ScoreZoom(string displayText, Color fontColor)
        {
            Text = displayText;
            DrawColor = fontColor;
            displayCounter = 0;
        }

        public void Update()
        {
            scale += lastScaleAmount + scaleAmount;
            lastScaleAmount += scaleAmount;
            displayCounter++;
        }
    }
}
