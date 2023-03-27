using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    public class Line
    {
        //attributes of a straight line, modelled after y=mx+c
        float gradient;
        float y_intercept;
        float x_intercept;
        bool fixedLength = false;
        bool inequality = false;
        Vector2 point1;
        Vector2 point2;

        float minX;
        float minY;
        float maxX;
        float maxY;
        
        

        public Line(Vector2 p1, Vector2 p2, bool fxdlength)
        {
            //constructor for when two points are given

            //calculate attributes based on data given
            gradient = (p2.Y - p1.Y) / (p2.X - p1.X);
            y_intercept = (gradient * -p1.X) + p1.Y;

            fixedLength = fxdlength;

            if (fixedLength)
            {
                point1 = p1;
                point2 = p2;
            }
            x_intercept = (-y_intercept) / gradient;
            if (float.IsNaN(x_intercept))
            {
                x_intercept = p1.X;
            }
        }

        public Line(float m, Vector2 p1)
        {
            //constructor for when the lines gradient is provided as well as a point
            gradient = m;

            //calculate attributes based on data given
            y_intercept = (gradient * -p1.X) + p1.Y;
            x_intercept = (-y_intercept) / gradient;
        }

        public Line(float m, float yInt)
        {
            //constructor for when the gradient and y intercept have been provided
            gradient = m;
            y_intercept = yInt;

            //calculate attributes based on data given
            x_intercept = (-y_intercept) / gradient;
        }

        public Line(float m, float yInt, float xInt, bool inequality, float minX, float maxX, float minY, float maxY)
        {
            //constructor for when attributes have been provided as well as any inequality values
            gradient = m;
            y_intercept = yInt;
            x_intercept = xInt;
            this.inequality = inequality;
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        public Line(float m, Vector2 p1, bool inequality, float minX, float maxX, float minY, float maxY)
        {
            //constructor for when gradient and a point is provided, as well as any inequality values

            //calculate attributes based on data given
            gradient = m;
            y_intercept = (gradient * -p1.X) + p1.Y;

            if (float.IsInfinity(gradient))
            {
                x_intercept = p1.X;
            }
            else
            {
                x_intercept = (-y_intercept) / gradient;
            }

            //set inequality values
            this.inequality = inequality;
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        static public Vector2 findPOI(Line l1, Line l2)
        {
            //Method to find the point of intersection between two lines
            
            //first calculates it as if both lines are simple straight lines with no inequalitys or 0/infinite gradients
            float tempY = l2.getYIntercept() - l1.getYIntercept();
            float tempM = l1.getGradient() - l2.getGradient();
            float X = tempY / tempM;
            float Y = l1.findYValue(X);

            //handles l1 being a line where y=a or x=a, not y=mx+c
            if (l1.gradient == 0)
            {
                Y = l1.getYIntercept();
                X = l2.findXValue(Y);
            }
            else if (float.IsInfinity(l1.getGradient()))
            {
                X = l1.getXIntercept();
                Y = l2.findYValue(X);
            }

            //handles l2 being a line where y=a or x=a, not y=mx+c
            if (l2.gradient == 0)
            {
                Y = l2.getYIntercept();
                X = l1.findXValue(Y);
            }
            else if (float.IsInfinity(l2.getGradient()))
            {
                X = l2.getXIntercept();
                Y = l1.findYValue(X);
            }

            //handles l1 being a line where y=a and l2 being a line where x=a
            if (l1.getGradient() == 0 && float.IsInfinity(l2.getGradient()))
            {
                X = l2.getXIntercept();
                Y = l1.getYIntercept();
            }
            if (l2.getGradient() == 0 && float.IsInfinity(l1.getGradient()))
            {
                X = l1.getXIntercept();
                Y = l2.getYIntercept();
            }


            
            if (l1.getPoint1() != null && l1.getPoint2() != null && l2.getPoint1() != null && l2.getPoint2() != null)
            {
                //handles lines of finite length and/or inequalites
                if (l1.checkPointInRange(new Vector2(X,Y)))
                {
                    if (l2.checkPointInRange(new Vector2(X,Y)))
                    {
                        return new Vector2(X, Y);
                    }
                    else
                    {
                        return new Vector2(float.NaN, float.NaN);
                    }
                }
                else
                {
                    return new Vector2(float.NaN, float.NaN);
                }
            }
            else
            {
                //if both lines are infinitely long
                return new Vector2(X, Y);
            }
            
            
        }

        public float getGradient()
        {
            return gradient;
        }

        public float getYIntercept()
        {
            return y_intercept;
        }

        public Vector2 getPoint1()
        {
            return point1;
        }

        public void setPoint1(Vector2 newPoint1)
        {
            point1 = newPoint1;
        }
        public Vector2 getPoint2()
        {
            return point2;
        }

        public void setPoint2(Vector2 newPoint2)
        {
            point2 = newPoint2;
        }

        public float findYValue(float X)
        {
            //Method that substitues an X value in y=mx+c and gives the Y value
            return gradient * X + y_intercept;
        }

        public float findXValue(float Y)
        {
            //Method that substitues a Y value in y=mx+c gives the X value

            //checks if its a line where x=a
            if (float.IsInfinity(gradient))
            {
                return x_intercept;
            }
            return (Y - y_intercept) / gradient;
        }

        public Vector2 findPointAtDistance(Vector2 point1, float distance, bool addDistance) 
        {
            //Method that finds a point at a specified distance along a line
            //addDistance is to see if we're adding the x value or taking it away

            float xValue;
            float yValue;

            double changeInX = Math.Sqrt((distance * distance) / ((gradient * gradient) + 1));
            
            if (addDistance)
            {
                xValue = point1.X + (float) changeInX;
            }
            else
            {
                xValue = point1.X - (float)changeInX;
            }

            yValue = findYValue(xValue);

            return new Vector2(xValue, yValue);
        }

        public Line findParallelLine(float displacement, bool addDisplacement)
        {
            //Method to find a line parallel to this line and a given distance away

            Line perpLine1;
            Line perpLine2;
            Vector2 parallelPoint1;
            Vector2 parallelPoint2;
            
            float perpGradient = (-1) / gradient;
            perpLine1 = new Line(perpGradient, point1);
            perpLine2 = new Line(perpGradient, point2);
            if (addDisplacement)
            {
                parallelPoint1 = perpLine1.findPointAtDistance(point1, displacement, true);
                parallelPoint2 = perpLine2.findPointAtDistance(point2, displacement, true);
            }
            else
            {
                parallelPoint1 = perpLine1.findPointAtDistance(point1, displacement, false);
                parallelPoint2 = perpLine2.findPointAtDistance(point2, displacement, false);
            }

            Line parallelLine = new Line(parallelPoint1, parallelPoint2, true);

            return parallelLine;
        }

        public void Draw(SpriteBatch spriteBatch, Color colour, float thickness)
        {
            spriteBatch.DrawLine(point1, point2, colour, thickness);
        }
        public void findRange(out float minX, out float maxX, out float minY, out float maxY)
        {
            //Method to find the range of this line based on if it has any inequalities or is of a fixed length between two points
            if (!inequality)
            {
                try
                {
                    if (point1.X <= point2.X)
                    {
                        minX = point1.X;
                        maxX = point2.X;
                    }
                    else
                    {
                        minX = point2.X;
                        maxX = point1.X;
                    }

                    if (point1.Y <= point2.Y)
                    {
                        minY = point1.Y;
                        maxY = point2.Y;
                    }
                    else
                    {
                        minY = point2.Y;
                        maxY = point1.Y;
                    }
                }
                catch (NullReferenceException)
                {

                    minX = float.NaN;
                    maxX = float.NaN;
                    minY = float.NaN;
                    maxY = float.NaN;
                }
                
            }
            else
            {
                minX = this.minX;
                minY = this.minY;
                maxX = this.maxX;
                maxY = this.maxY;
            }
            

        }

        public bool checkPointInRange(Vector2 point)
        {
            //Method to check if a point is within the range of this line
            if (checkXInRange(point.X) && checkYInRange(point.Y))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool checkXInRange(double X)
        {
            //Method to check if the given X value is within the range of this line

            float minX, maxX, minY, maxY;
            findRange(out minX, out maxX, out minY, out maxY);

            if (!float.IsNaN(minX) && !float.IsNaN(maxX))
            {
                if (minX <= X && X <= maxX)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (!float.IsNaN(minX) && float.IsNaN(maxX))
            {
                if (minX <= X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (float.IsNaN(minX) && !float.IsNaN(maxX))
            {
                if (X <= maxX)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public bool checkYInRange(double Y)
        {
            //Method to check if the given Y value is within the range of this line
            float minX, maxX, minY, maxY;
            findRange(out minX, out maxX, out minY, out maxY);

            if (!float.IsNaN(minY) && !float.IsNaN(maxY))
            {
                if (minY <= Y && Y <= maxY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (!float.IsNaN(minY) && float.IsNaN(maxY))
            {
                if (minY <= Y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (float.IsNaN(minY) && !float.IsNaN(maxY))
            {
                if (Y <= maxY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        }

        public float getXIntercept()
        {
            return x_intercept;
        }

        

    }

    
}

