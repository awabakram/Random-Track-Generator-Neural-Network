using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Random_Track_Generation
{
    public class TrackPoint
    {
        //x and y coordinates
        Vector2 position;

        //angle between p0's positive x-axis and the point
        double polarAngle;

        //distance from p0
        double distance; 

        public TrackPoint(Vector2 newPos)
        {
            position = newPos;
        }

        public TrackPoint(float newX, float newY)
        {
            position.X = newX;
            position.Y = newY;
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public double getPolarAngle()
        {
            return polarAngle;
        }

        public void setPolarAngle(double angle)
        {
            polarAngle = angle;
        }

        public double getDistance()
        {
            return distance;
        }

        public void setDistance(double newDistance)
        {
            distance = newDistance;
        }
    }
}
