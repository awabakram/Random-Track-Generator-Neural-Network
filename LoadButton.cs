using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class LoadButton : Button
    {
        Track LoadedTrack;

        public LoadButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate,  Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            isClicked = false;
        }

        public void Update(GameTime gameTime, MouseState mState, ref Track currentTrack, ref Car currentCar, ref Car[] trainingCars, string filename, ref string statusString)
        {
            UpdateClicked(gameTime, mState);
            if (isClicked)
            {
                isClicked = false;
                if (filename == "")
                {
                    statusString = "Please enter the track's\nname that you wish to load";
                    return;
                }
                LoadedTrack = new Track(filename, ref statusString);
                if (LoadedTrack.getTrackPossible() == false)
                {
                    return;
                }
                currentTrack = LoadedTrack;
                
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
