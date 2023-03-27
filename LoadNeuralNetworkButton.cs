using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class LoadNeuralNetworkButton : Button
    {
        public LoadNeuralNetworkButton(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            isClicked = false;

        }

        public void Update(GameTime gameTime, MouseState mState, ref Car currentCar, string filename, ref string statusString)
        {
            UpdateClicked(gameTime, mState);
            if (isClicked)
            {
                isClicked = false;
                string filePath = filename + ".txt";
                NeuralNetwork newNet = new NeuralNetwork(filePath, ref statusString);
                if (newNet.getBiases() != null)
                {
                    currentCar.setNeuralNetwork(newNet);
                }
                

            }
        }
    }
}
