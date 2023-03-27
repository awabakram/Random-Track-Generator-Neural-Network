using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    class ToggleButton : Button
    {
        bool toggled;
        string textUntoggled;
        string textToggled;
        Color colourUntoggled;
        Color colourToggled;

        public ToggleButton(int newHeight, int newWidth, string textUntoggled, string textToggled, Vector2 position, SpriteFont newfont, MouseState mstate, Color colourUntoggled, Color colourToggled) : base(newHeight, newWidth, textUntoggled, position, newfont, mstate, colourUntoggled)
        {
            this.textUntoggled = textUntoggled;
            this.textToggled = textToggled;
            this.colourUntoggled = colourUntoggled;
            this.colourToggled = colourToggled;

            isClicked = false;
            toggled = false;
        }

        public void Update(GameTime gameTime, MouseState mState)
        {
            UpdateClicked(gameTime, mState);

            //when clicked, the button changes to the opposite state
            if (isClicked)
            {
                isClicked = false;

                if (toggled)
                {
                    toggled = false;
                }
                else
                {
                    toggled = true;
                }
                
            }

            if (toggled)
            {
                currentButtonColor = colourToggled;
                buttonText = textToggled;
            }
            else
            {
                currentButtonColor = colourUntoggled;
                buttonText = textUntoggled;
            }

        }

        public bool getToggled()
        {
            return toggled;
        }

        public void setToggled(bool newVal)
        {
            toggled = newVal;
        }
    }
}
