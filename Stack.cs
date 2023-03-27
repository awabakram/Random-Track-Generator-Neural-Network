using System;
using System.Collections.Generic;
using System.Text;

namespace Random_Track_Generation
{
    class Stack
    {
        List<TrackPoint> stack = new List<TrackPoint>();

        public Stack(TrackPoint[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                stack.Add(array[i]);
            }
        }
        
        public void push(TrackPoint addPoint)
        {
            //Method to push the given value to the top of the stack
            stack.Add(addPoint);
        }

        public TrackPoint pop()
        {
            //Method to remove the value at the top of the stack and return it
            try
            {
                TrackPoint tempPoint = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                return tempPoint;
            }
            catch (ArgumentOutOfRangeException)
            {

                return null;
            }
        }

        public TrackPoint popSTL() //pop Second to Last
        {
            //Method to remove the value that is 2nd to top of the stack and return it
            try
            {
                TrackPoint tempPoint = stack[stack.Count - 2];
                stack.RemoveAt(stack.Count - 2);
                return tempPoint;
            }
            catch (ArgumentOutOfRangeException)
            {

                return null;
            }
        }


        public List<TrackPoint> getStack()
        {
            return stack;
        }

        public TrackPoint getLastPoint()
        {
            //Method to return the value at the top of the stack
            try
            {
                return stack[stack.Count - 1];
            }
            catch (ArgumentOutOfRangeException)
            {

                return null;
            }
        }

        public TrackPoint getSTLPoint() //Get second to last point
        {
            //Method to return the value that is 2nd to the top of the stack
            try
            {
                return stack[stack.Count - 2];
            }
            catch (ArgumentOutOfRangeException)
            {

                return null;
            }
        }
    }
}
