using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class ResetCarsButton : Button
    {
        Vector2 gameBorderTL; //Top Left Corner of game section
        Vector2 gameBorderBR; //Bottom Right Corner of game section
        SpriteFont font;


        public ResetCarsButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Vector2 topLeftBorder, Vector2 bottomRightBorder, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            font = newfont;
            gameBorderTL = topLeftBorder;
            gameBorderBR = bottomRightBorder;

            isClicked = false;
        }

        public void Update(GameTime gameTime, MouseState mState, ref Track currentTrack, ref Car currentCar, ref Car[] trainingCars)
        {
            UpdateClicked(gameTime, mState);

            if (isClicked)
            {
                isClicked = false;

                double carRotation = Math.Atan(currentTrack.getLastLine().getGradient());
                currentCar.reset(currentTrack.getStartPoint(), (float)carRotation, currentTrack.getCheckpoints());

                for (int i = 0; i < trainingCars.Length; i++)
                {
                    trainingCars[i].reset(currentTrack.getStartPoint(), (float)carRotation, currentTrack.getCheckpoints());
                }
            }
        }
    }
}
