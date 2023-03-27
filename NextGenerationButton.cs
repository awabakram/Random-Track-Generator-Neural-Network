using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class NextGenerationButton : Button
    {
        Vector2 gameBorderTL; //Top Left Corner of game section
        Vector2 gameBorderBR; //Bottom Right Corner of game section
        SpriteFont font;

        public NextGenerationButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Vector2 topLeftBorder, Vector2 bottomRightBorder, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            font = newfont;
            gameBorderTL = topLeftBorder;
            gameBorderBR = bottomRightBorder;

            isClicked = false;
        }

        public bool Update(GameTime gameTime, MouseState mState)
        {
            UpdateClicked(gameTime, mState);

            if (isClicked)
            {
                isClicked = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
