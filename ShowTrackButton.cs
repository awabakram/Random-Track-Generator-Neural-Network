using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Random_Track_Generation
{
    class ShowTrackButton : Button
    {
        
        public ShowTrackButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            isClicked = false;
        }

        public void Update(GameTime gameTime, MouseState mState, ref Track currentTrack, ref Car currentCar, ref Car[] trainingCars, Track trackToBeLoaded, ref string statusString)
        {
            UpdateClicked(gameTime, mState);
            if (isClicked)
            {
                isClicked = false;
                if (trackToBeLoaded == null)
                {
                    statusString = "Track Not Loaded";
                    return;
                }
                currentTrack = trackToBeLoaded;

                double carRotation = Math.Atan(currentTrack.getLastLine().getGradient());
                currentCar.reset(currentTrack.getStartPoint(), (float)carRotation, currentTrack.getCheckpoints());

                for (int i = 0; i < trainingCars.Length; i++)
                {
                    trainingCars[i].reset(currentTrack.getStartPoint(), (float)carRotation, currentTrack.getCheckpoints());
                }

                statusString = "Track Loaded";
            }
            
        }

        

    }
}
