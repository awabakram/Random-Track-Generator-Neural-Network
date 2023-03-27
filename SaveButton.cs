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
    class SaveButton : Button
    {
        public SaveButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Color newColor) : base(newHeight, newWidth, text, position,  newfont,  mstate,  newColor)
        {
            isClicked = false;
        }

        public void Update(GameTime gameTime, MouseState mState ,ref Track currentTrack, string filename, ref string statusString)
        {
            

            UpdateClicked(gameTime, mState);

            if (isClicked)
            {
                isClicked = false;

                //check if the a track already exists with the given name
                if (filename == "")
                {
                    statusString = "Please Enter a name for the \ntrack. Then try again";
                    return;
                }
                if (Directory.Exists(filename))
                {
                    statusString = "A File already exists with this name";
                    return;
                }

                //Code to save track
                string filepath;
                StreamWriter writer;
                Directory.CreateDirectory(filename);

                //save Startpoint and startpoint edges
                filepath = $@"{filename}/StartPoints.txt";
                writer = new StreamWriter(filepath);
                writer.WriteLine($"{currentTrack.getStartPoint().X}, {currentTrack.getStartPoint().Y}");
                writer.WriteLine($"{currentTrack.getStartPointEdges()[0].getPosition().X}, {currentTrack.getStartPointEdges()[0].getPosition().Y}");
                writer.WriteLine($"{currentTrack.getStartPointEdges()[1].getPosition().X}, {currentTrack.getStartPointEdges()[1].getPosition().Y}");
                writer.Close();

                //save the final points
                List<TrackPoint> finalPoints = currentTrack.getFinalPoints();
                filepath = $@"{filename}/Finalpoints.txt";
                writer = new StreamWriter(filepath);
                for (int i = 0; i < finalPoints.Count; i++)
                {
                    writer.WriteLine($"{finalPoints[i].getPosition().X}, {finalPoints[i].getPosition().Y}");
                }
                writer.Close();

                statusString = "Successfully Saved Track";
            }

        }
    }
}
