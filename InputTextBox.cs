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
    class InputTextBox : Button
    {
        //this need attributes to store the keys that re being pressed as well as if shift is being held
        Keys[] keys;
        bool shift;

        public InputTextBox(int newHeight, int newWidth, string text, Vector2 position, SpriteFont newfont, MouseState mstate, Color newColor) : base(newHeight, newWidth, text, position, newfont, mstate, newColor)
        {
            isClicked = false;
            buttonText = "";
        }

        public void Update(GameTime gameTime, MouseState mState, KeyboardState kstate, KeyboardState previouskstate)
        {
            UpdateClicked(gameTime, mState);
            if (isClicked)
            {
                currentButtonColor = Color.LightBlue;
                keys = kstate.GetPressedKeys();

                //only exit typing mode when enter is pressed
                if (kstate.IsKeyDown(Keys.Enter))
                {
                    isClicked = false;
                    return;
                }

                //check for shift being pressed
                if (kstate.IsKeyDown(Keys.LeftShift) || kstate.IsKeyDown(Keys.RightShift))
                {
                    shift = true;
                }
                else
                {
                    shift = false;
                }

                //delete a letter when backspace is pressed
                if (buttonText.Length>0 && kstate.IsKeyDown(Keys.Back) && previouskstate.IsKeyDown(Keys.Back) == false)
                {
                    buttonText = buttonText.Substring(0, buttonText.Length - 1);
                }

                //check for vaid inputs and add them to the box's text
                if (keys.Length > 0 && previouskstate.IsKeyDown(keys[0]) == false)
                {
                    switch (keys[0])
                    {
                        case Keys.Space:
                            buttonText += " ";
                            break;
                        case Keys.D0:
                            if (shift)
                            {
                                buttonText += ")";
                            }
                            else
                            {
                                buttonText += "0";
                            }
                            break;
                        case Keys.D1:
                            if (shift)
                            {
                                buttonText += "!";
                            }
                            else
                            {
                                buttonText += "1";
                            }
                            break;
                        case Keys.D2:
                            buttonText += "2";
                            break;
                        case Keys.D3:
                            if (shift)
                            {
                                buttonText += "£";
                            }
                            else
                            {
                                buttonText += "3";
                            }
                            break;
                        case Keys.D4:
                            if (shift)
                            {
                                buttonText += "$";
                            }
                            else
                            {
                                buttonText += "4";
                            }
                            break;
                        case Keys.D5:
                            if (shift)
                            {
                                buttonText += "%";
                            }
                            else
                            {
                                buttonText += "5";
                            }
                            break;
                        case Keys.D6:
                            if (shift)
                            {
                                buttonText += "^";
                            }
                            else
                            {
                                buttonText += "6";
                            }
                            break;
                        case Keys.D7:
                            if (shift)
                            {
                                buttonText += "&";
                            }
                            else
                            {
                                buttonText += "7";
                            }
                            break;
                        case Keys.D8:
                            buttonText += "8";
                            break;
                        case Keys.D9:
                            if (shift)
                            {
                                buttonText += "(";
                            }
                            else
                            {
                                buttonText += "9";
                            }
                            break;
                        case Keys.A:
                            if (shift)
                            {
                                buttonText += "A";
                            }
                            else
                            {
                                buttonText += "a";
                            }
                            break;
                        case Keys.B:
                            if (shift)
                            {
                                buttonText += "B";
                            }
                            else
                            {
                                buttonText += "b";
                            }
                            break;
                        case Keys.C:
                            if (shift)
                            {
                                buttonText += "C";
                            }
                            else
                            {
                                buttonText += "c";
                            }
                            break;
                        case Keys.D:
                            if (shift)
                            {
                                buttonText += "D";
                            }
                            else
                            {
                                buttonText += "d";
                            }
                            break;
                        case Keys.E:
                            if (shift)
                            {
                                buttonText += "E";
                            }
                            else
                            {
                                buttonText += "e";
                            }
                            break;
                        case Keys.F:
                            if (shift)
                            {
                                buttonText += "F";
                            }
                            else
                            {
                                buttonText += "f";
                            }
                            break;
                        case Keys.G:
                            if (shift)
                            {
                                buttonText += "G";
                            }
                            else
                            {
                                buttonText += "g";
                            }
                            break;
                        case Keys.H:
                            if (shift)
                            {
                                buttonText += "H";
                            }
                            else
                            {
                                buttonText += "h";
                            }
                            break;
                        case Keys.I:
                            if (shift)
                            {
                                buttonText += "I";
                            }
                            else
                            {
                                buttonText += "i";
                            }
                            break;
                        case Keys.J:
                            if (shift)
                            {
                                buttonText += "J";
                            }
                            else
                            {
                                buttonText += "j";
                            }
                            break;
                        case Keys.K:
                            if (shift)
                            {
                                buttonText += "K";
                            }
                            else
                            {
                                buttonText += "k";
                            }
                            break;
                        case Keys.L:
                            if (shift)
                            {
                                buttonText += "L";
                            }
                            else
                            {
                                buttonText += "l";
                            }
                            break;
                        case Keys.M:
                            if (shift)
                            {
                                buttonText += "M";
                            }
                            else
                            {
                                buttonText += "m";
                            }
                            break;
                        case Keys.N:
                            if (shift)
                            {
                                buttonText += "N";
                            }
                            else
                            {
                                buttonText += "n";
                            }
                            break;
                        case Keys.O:
                            if (shift)
                            {
                                buttonText += "O";
                            }
                            else
                            {
                                buttonText += "o";
                            }
                            break;
                        case Keys.P:
                            if (shift)
                            {
                                buttonText += "P";
                            }
                            else
                            {
                                buttonText += "p";
                            }
                            break;
                        case Keys.Q:
                            if (shift)
                            {
                                buttonText += "Q";
                            }
                            else
                            {
                                buttonText += "q";
                            }
                            break;
                        case Keys.R:
                            if (shift)
                            {
                                buttonText += "R";
                            }
                            else
                            {
                                buttonText += "r";
                            }
                            break;
                        case Keys.S:
                            if (shift)
                            {
                                buttonText += "S";
                            }
                            else
                            {
                                buttonText += "s";
                            }
                            break;
                        case Keys.T:
                            if (shift)
                            {
                                buttonText += "T";
                            }
                            else
                            {
                                buttonText += "t";
                            }
                            break;
                        case Keys.U:
                            if (shift)
                            {
                                buttonText += "U";
                            }
                            else
                            {
                                buttonText += "u";
                            }
                            break;
                        case Keys.V:
                            if (shift)
                            {
                                buttonText += "V";
                            }
                            else
                            {
                                buttonText += "v";
                            }
                            break;
                        case Keys.W:
                            if (shift)
                            {
                                buttonText += "W";
                            }
                            else
                            {
                                buttonText += "w";
                            }
                            break;
                        case Keys.X:
                            if (shift)
                            {
                                buttonText += "X";
                            }
                            else
                            {
                                buttonText += "x";
                            }
                            break;
                        case Keys.Y:
                            if (shift)
                            {
                                buttonText += "Y";
                            }
                            else
                            {
                                buttonText += "y";
                            }
                            break;
                        case Keys.Z:
                            if (shift)
                            {
                                buttonText += "Z";
                            }
                            else
                            {
                                buttonText += "z";
                            }
                            break;
                        case Keys.NumPad0:
                            buttonText += "0";
                            break;
                        case Keys.NumPad1:
                            buttonText += "1";
                            break;
                        case Keys.NumPad2:
                            buttonText += "2";
                            break;
                        case Keys.NumPad3:
                            buttonText += "3";
                            break;
                        case Keys.NumPad4:
                            buttonText += "4";
                            break;
                        case Keys.NumPad5:
                            buttonText += "5";
                            break;
                        case Keys.NumPad6:
                            buttonText += "6";
                            break;
                        case Keys.NumPad7:
                            buttonText += "7";
                            break;
                        case Keys.NumPad8:
                            buttonText += "8";
                            break;
                        case Keys.NumPad9:
                            buttonText += "9";
                            break;
                        case Keys.Add:
                            buttonText += "+";
                            break;
                        case Keys.Subtract:
                            buttonText += "-";
                            break;
                        case Keys.OemPeriod:
                            buttonText += ".";
                            break;
                        case Keys.OemComma:
                            buttonText += ",";
                            break;
                        default:
                            break;
                    } 
                }


            }
        }

        public string getText()
        {
            return buttonText;
        }

        public bool getIsClicked()
        {
            return isClicked;
        }
    }
}
