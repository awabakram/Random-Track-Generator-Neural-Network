using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class SaveNeuralNetworkButton : Button
    {
        public SaveNeuralNetworkButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            isClicked = false;
        }
        public bool getClicked()
        {
            return isClicked;
        }

        public void setClicked(bool clicked)
        {
            isClicked = clicked;
        }
    }
}
