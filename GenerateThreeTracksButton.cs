using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class GenerateThreeTracksButton : Button
    {
        Track generatedTrack;
        Vector2 gameBorderTL; //Top Left Corner of game section
        Vector2 gameBorderBR; //Bottom Right Corner of game section
        SpriteFont font;

        public GenerateThreeTracksButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Vector2 topLeftBorder, Vector2 bottomRightBorder, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            font = newfont;
            gameBorderTL = topLeftBorder;
            gameBorderBR = bottomRightBorder;

            isClicked = false;
        }

        public void Update(GameTime gameTime, MouseState mState,ref Track currentTrack1, ref Track currentTrack2, ref Track currentTrack3, ref string statusString)
        {
            UpdateClicked(gameTime, mState);
            if (isClicked)
            {
                isClicked = false;

                generatedTrack = new Track(gameBorderTL, gameBorderBR, font);
                currentTrack1 = generatedTrack;

                generatedTrack = new Track(gameBorderTL, gameBorderBR, font);
                currentTrack2 = generatedTrack;

                generatedTrack = new Track(gameBorderTL, gameBorderBR, font);
                currentTrack3 = generatedTrack;

                statusString = "3 Tracks Generated";
            }
        }
    }
}
