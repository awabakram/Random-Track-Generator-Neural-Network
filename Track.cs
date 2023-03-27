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
    public class Track
    {
        Random rand = new Random();

        //Attributes relating to the game border
        Vector2 gameBorderTL;
        Vector2 gameBorderBR;
        const int SpaceOfPointsFromEdge = 120;

        //Attributes relating to track points which can be discarded at the end
        int numberOfPoints;
        TrackPoint[] trackPoints;
        TrackPoint point0; //the point with the lowest Y value
        TrackPoint[] orderedTrackPoints;
        TrackPoint[] convexHullPoints;
        

        bool trackPossible = true;

        //attributes relating to the track that are needed for the final track
        List<TrackPoint> finalPoints = new List<TrackPoint>();
        Line lastLine;
        TrackPoint startPoint;
        TrackPoint[] startPointEdges;
        List<Line> insideLineBorder = new List<Line>();
        List<Line> outsideLineBorder = new List<Line>();
        List<TrackPoint> checkpoints = new List<TrackPoint>();

        //Attributes relating to drawing
        SpriteFont font;
        const float trackWidth = 100f;


        public Track(Vector2 newGameBorderTL, Vector2 newGameBorderBR, SpriteFont newfont)
        {
            //Constructor to generate a normal track with curved lines
            font = newfont;
            gameBorderTL = newGameBorderTL;
            gameBorderBR = newGameBorderBR;
            GenerateTrack();
        }

        public Track(string filename, ref string statustring) 
        {
            //Constructor to load a track from a file
            StreamReader reader;
            string filepath;
            string line;
            string[] splitline;
            double X;
            double Y;

            //handling the event that the file that is inputted doesnt exist
            try
            {
                filepath = $@"{filename}/StartPoints.txt";
                reader = new StreamReader(filepath);
            }
            catch (Exception)
            {
                trackPossible = false;
                statustring = "File Not Found";
                return;
            }

            //load startpoint
            line = reader.ReadLine();
            splitline = line.Split(',');
            X = Convert.ToDouble(splitline[0]);
            Y = Convert.ToDouble(splitline[1]);
            startPoint = new TrackPoint(new Vector2((float)X, (float)Y));

            //load startPointEdges
            startPointEdges = new TrackPoint[2];
            line = reader.ReadLine();
            splitline = line.Split(',');
            X = Convert.ToDouble(splitline[0]);
            Y = Convert.ToDouble(splitline[1]);
            startPointEdges[0] = new TrackPoint(new Vector2((float)X, (float)Y));

            line = reader.ReadLine();
            splitline = line.Split(',');
            X = Convert.ToDouble(splitline[0]);
            Y = Convert.ToDouble(splitline[1]);
            startPointEdges[1] = new TrackPoint(new Vector2((float)X, (float)Y));
            reader.Close();

            //loading the FinalPoints
            try
            {
                filepath = $@"{filename}/Finalpoints.txt";
                reader = new StreamReader(filepath);
            }
            catch (Exception)
            {
                trackPossible = false;
                statustring = "File Incomplete";
                return;
            }

            finalPoints = new List<TrackPoint>();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                splitline = line.Split(',');
                X = Convert.ToDouble(splitline[0]);
                Y = Convert.ToDouble(splitline[1]);
                finalPoints.Add(new TrackPoint(new Vector2((float)X, (float)Y)));
            }
            reader.Close();

            findLastLine();
            findTrackBorders();
            eliminateOverlapLines();
            connectLineBorders();
            defineCheckpoints();
            statustring = "Successfully Loaded Track";
        }

        public Track(Vector2 newGameBorderTL, Vector2 newGameBorderBR, SpriteFont newfont, bool DefaultTrack)
        {
            //Constructor to generate the default track (a straight line)
            font = newfont;
            gameBorderTL = newGameBorderTL;
            gameBorderBR = newGameBorderBR;

            generateDefaultTrack();
        }

        public Track(Vector2 newGameBorderTL, Vector2 newGameBorderBR, SpriteFont newfont, bool DefaultTrack, bool straightLineTrack)
        {
            //Constructor to generate a track with straight lines
            font = newfont;
            gameBorderTL = newGameBorderTL;
            gameBorderBR = newGameBorderBR;

            GenerateStraightLineTrack();
        }

        void generateDefaultTrack()
        {
            //Method to initialise all nesicary attributes for a straight line track
            finalPoints = new List<TrackPoint>();
            finalPoints.Add(new TrackPoint(new Vector2(400, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(600, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(800, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(1000, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(1200, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(1400, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(1600, 540)));
            finalPoints.Add(new TrackPoint(new Vector2(1800, 540)));
            startPoint = new TrackPoint(new Vector2(500, 540));

            startPointEdges = new TrackPoint[2];
            startPointEdges[0] = new TrackPoint(new Vector2(500, 540 - trackWidth / 2));
            startPointEdges[1] = new TrackPoint(new Vector2(500, 540 + trackWidth / 2));

            lastLine = new Line(finalPoints[finalPoints.Count - 1].getPosition(), finalPoints[0].getPosition(), true);

            insideLineBorder.Add(new Line(new Vector2(400, 540 - (trackWidth / 2)), new Vector2(1800, 540 - (trackWidth / 2)), true));
            outsideLineBorder.Add(new Line(new Vector2(400, 540 + (trackWidth / 2)), new Vector2(1800, 540 + (trackWidth / 2)), true));
            outsideLineBorder.Add(new Line(new Vector2(400, 540 - (trackWidth / 2)), new Vector2(400, 540 + (trackWidth / 2)), true));
            outsideLineBorder.Add(new Line(new Vector2(1800, 540 + (trackWidth / 2)), new Vector2(1800, 540 - (trackWidth / 2)), true));

            for (int i = 0; i < finalPoints.Count; i++)
            {
                checkpoints.Add(finalPoints[i]);
            }
            checkpoints[0] = startPoint;


        }

        public void GenerateTrack()
        {
            //Method to generate a track (with curves)

            InitialisePoints(gameBorderTL, gameBorderBR);
            orderTrackpoints();
            trackPossible = checkTrackPossible();
            if (trackPossible == false)
            {
                return;
            }

            grahamScan();
            findFinalTrackPoints();
            trackPossible = checkForNullPoints();
            if (trackPossible == false)
            {
                return;
            }

            excludeOutOfRangePoints();

            findStartPoint();
            //findTrackBorders();
            findTrackBorders();
            eliminateOverlapLines();
            connectLineBorders();
            defineCheckpoints();
            
        }

        public void GenerateStraightLineTrack()
        {
            //Method to generate a track (straight lines)
            InitialisePoints(gameBorderTL, gameBorderBR);
            orderTrackpoints();
            trackPossible = checkTrackPossible();
            if (trackPossible == false)
            {
                return;
            }

            grahamScan();
            for (int i = 0; i < convexHullPoints.Length; i++)
            {
                finalPoints.Add(convexHullPoints[i]);
            }

            findStartPoint();
            findTrackBorders();
            eliminateOverlapLines();
            connectLineBorders();
            defineCheckpoints();
           
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (trackPossible == false)
            {
                spriteBatch.DrawString(font, "Track Generation Failed", new Vector2((gameBorderTL.X + gameBorderBR.X) / 2, (gameBorderTL.Y + gameBorderBR.Y) / 2), Color.Black);
                return;
            }

            //Draw lines between the points
            for (int i = 0; i < finalPoints.Count - 1; i++)
            {
                spriteBatch.DrawLine(finalPoints[i].getPosition(), finalPoints[i + 1].getPosition(), Color.Black, trackWidth);
            }
            spriteBatch.DrawLine(finalPoints[finalPoints.Count - 1].getPosition(), finalPoints[0].getPosition(), Color.Black, trackWidth);

            for (int i = 0; i < finalPoints.Count; i++)
            {
                spriteBatch.DrawCircle(finalPoints[i].getPosition(), trackWidth / 2, 32, Color.Black, trackWidth / 2);
                //spriteBatch.DrawRectangle(new RectangleF(finalPoints[i].getPosition(), new Size2(trackWidth / 2, trackWidth/2)), Color.Black, trackWidth / 2);
                
            }

            //Draw the Start Line
            spriteBatch.DrawLine(startPointEdges[0].getPosition(), startPointEdges[1].getPosition(), Color.White, 5f);

            


            //for (int i = 0; i < insideLineBorder.Count; i++)
            //{
            //    insideLineBorder[i].Draw(spriteBatch, Color.White, 5);
            //}
            //for (int i = 0; i < outsideLineBorder.Count; i++)
            //{
            //    outsideLineBorder[i].Draw(spriteBatch, Color.Red, 5);
            //}



        }

        public bool getTrackPossible()
        {
            return trackPossible;
        }

        void InitialisePoints(Vector2 gameBorderTL, Vector2 gameBorderBR)
        {
            //Method to generate a set of random points,
            //find the lowest point
            //and set their polar angles and distances


            //Decide how many points you want to generate - decided randomly
            numberOfPoints = rand.Next(5, 25);
            trackPoints = new TrackPoint[numberOfPoints];

            //generate the random points
            for (int i = 0; i < trackPoints.Length; i++)
            {
                float tempX = rand.Next(Convert.ToInt32(gameBorderTL.X + SpaceOfPointsFromEdge), Convert.ToInt32(gameBorderBR.X - SpaceOfPointsFromEdge));
                float tempY = rand.Next(Convert.ToInt32(gameBorderTL.Y + SpaceOfPointsFromEdge), Convert.ToInt32(gameBorderBR.Y - SpaceOfPointsFromEdge));

                trackPoints[i] = new TrackPoint(tempX, tempY);
            }

            //set point0
            point0 = findLowestPoint();

            //Find the polar angles
            for (int i = 0; i < trackPoints.Length; i++)
            {
                trackPoints[i].setPolarAngle(findPolarAngle(trackPoints[i]));
            }

            //Find Distances
            for (int i = 0; i < trackPoints.Length; i++)
            {
                trackPoints[i].setDistance(findDistance(point0,trackPoints[i]));
            }


        }

        TrackPoint findLowestPoint()
        {
            //Method to find the lowest point in trackpoints
            //since y=0 is at the top of the screen in monogame, 
            //it looks for the point with the HIGHEST y value.

            TrackPoint lowestPoint = trackPoints[0];
            for (int i = 1; i < trackPoints.Length; i++)
            {
                if (trackPoints[i].getPosition().Y > lowestPoint.getPosition().Y)
                {
                    lowestPoint = trackPoints[i];
                }
                else if (trackPoints[i].getPosition().Y == lowestPoint.getPosition().Y)
                {
                    if (trackPoints[i].getPosition().X < lowestPoint.getPosition().X)
                    {
                        lowestPoint = trackPoints[i];
                    }
                }
            }

            return lowestPoint;
        }

        double findPolarAngle(TrackPoint point)
        {
            //Method to find the polar angle between point0 and the given point, in radians

            float yDifference = point0.getPosition().Y - point.getPosition().Y;
            float xDifference = point.getPosition().X - point0.getPosition().X;

            double angle = Math.Atan(yDifference / xDifference);

            if (xDifference < 0)
            {
                angle = angle + Math.PI;
            }

            return angle;
        }

        static public double findDistance(TrackPoint p0, TrackPoint p1)
        {
            //Method to find the distance between two points 
            //using pythagoras' theorem a^2 + b^2 = c^2

            float yDifference = p0.getPosition().Y - p1.getPosition().Y;
            float xDifference = p1.getPosition().X - p0.getPosition().X;

            return Math.Sqrt((xDifference * xDifference) + (yDifference * yDifference)); 
        }

        void orderTrackpoints()
        {
            //Method to order trackpoints so that they can be used in the graham scan
            
            List<TrackPoint> orderedTrackPointsList = new List<TrackPoint>();
            orderedTrackPointsList.Add(point0);

            TrackPoint[] sanitisedTrackPoints = removePoint(trackPoints, point0); //return an array of trackPoints with point0 removed from it

            //they are ordered by polar angle, lowest to highest,
            TrackPoint[] tempSortedPoints = mergeSort(sanitisedTrackPoints);

            //if two points have the same polar angle, then the furthest one is kept
            tempSortedPoints = removePointsWithSamePolarAngle(tempSortedPoints);

            for (int i = 0; i < tempSortedPoints.Length; i++)
            {
                orderedTrackPointsList.Add(tempSortedPoints[i]);
            }

            orderedTrackPoints = orderedTrackPointsList.ToArray();
        }

        TrackPoint[] mergeSort(TrackPoint[] items)
        {
            //This method sorts the Trackpoints by polar angle, lowest to highest

            TrackPoint[] left_half;
            TrackPoint[] right_half;

            //Base case for recursion
            if (items.Length < 2)
            {
                return items;
            }

            int midpoint = items.Length / 2;

            //Do the left half
            left_half = new TrackPoint[midpoint];
            for (int i = 0; i < midpoint; i++)
            {
                left_half[i] = items[i];
            }

            //figure out how big the right half should be
            if (items.Length % 2 == 0)
            {
                right_half = new TrackPoint[midpoint];
            }
            else
            {
                right_half = new TrackPoint[midpoint + 1];
            }

            //fill in hte right half
            int rightIndex = 0;
            for (int i = midpoint; i < items.Length; i++)
            {
                right_half[rightIndex] = items[i];
                rightIndex++;
            }

            //recursion bit
            left_half = mergeSort(left_half);
            right_half = mergeSort(right_half);

            items = merge(left_half, right_half);

            return items;
        }

        TrackPoint[] merge(TrackPoint[] list1, TrackPoint[] list2)
        {
            TrackPoint[] merged = new TrackPoint[list1.Length + list2.Length];

            int index1 = 0;
            int index2 = 0;
            int indexMerged = 0;

            while (index1 < list1.Length && index2 < list2.Length)
            {
                if (list1[index1].getPolarAngle() < list2[index2].getPolarAngle())
                {
                    merged[indexMerged] = list1[index1];
                    index1++;
                }
                else if (list1[index1].getPolarAngle() == list2[index2].getPolarAngle())
                {
                    if (list1[index1].getDistance() < list2[index2].getDistance())
                    {
                        merged[indexMerged] = list1[index1];
                        index1++;
                    }
                    else
                    {
                        merged[indexMerged] = list2[index2];
                        index2++;
                    }
                }
                else
                {
                    merged[indexMerged] = list2[index2];
                    index2++;
                }
                indexMerged++;
            }

            while (index1 < list1.Length)
            {
                merged[indexMerged] = list1[index1];
                index1++;
                indexMerged++;
            }

            while (index2 < list2.Length)
            {
                merged[indexMerged] = list2[index2];
                index2++;
                indexMerged++;
            }

            return merged;
        }

        TrackPoint[] removePoint(TrackPoint[] inputPoints, TrackPoint pointToBeRemoved)
        {
            //Method that removes the specified trackpoint from the specified array

            TrackPoint[] points = new TrackPoint[inputPoints.Length - 1];
            
            int pointIndex = 0;
            
            for (int i = 0; i < inputPoints.Length; i++)
            {
                if (trackPoints[i] != pointToBeRemoved)
                {
                    points[pointIndex] = inputPoints[i];
                    pointIndex++;
                }
            }

            return points;
        }

        TrackPoint[] removePointsWithSamePolarAngle(TrackPoint[] inputSortedPoints)
        {
            //Method that goes through the inputted arrays and
            //checks if any of the points have the same polar angle,
            //if they do then the one with the greatest distance is kept
            TrackPoint[] currentSortedPoints = inputSortedPoints;
            int roundingValue = 5; //checks them rounded to 5 d.p.

            for (int i = 0; i < inputSortedPoints.Length - 1; i++)
            {
                if (Math.Round(inputSortedPoints[i].getPolarAngle(), roundingValue) == Math.Round(inputSortedPoints[i + 1].getPolarAngle(), roundingValue))
                {
                    if (inputSortedPoints[i].getDistance() > inputSortedPoints[i + 1].getDistance())
                    {
                        currentSortedPoints = removePoint(currentSortedPoints, inputSortedPoints[i + 1]);
                    }
                    else
                    {
                        currentSortedPoints = removePoint(currentSortedPoints, inputSortedPoints[i]);
                    }
                }
            }

            return currentSortedPoints;
        }

        bool checkTrackPossible()
        {
            //Method that checks if there are still enough points to make a complete track
            if (orderedTrackPoints.Length < 3)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        void grahamScan()
        {
            //Method for the graham scan algorithm
            Stack pointsStack = new Stack(new TrackPoint[] {orderedTrackPoints[0], orderedTrackPoints[1], orderedTrackPoints[2]});

            for (int i = 3; i < orderedTrackPoints.Length; i++)
            {
                while (checkLeft(pointsStack.getSTLPoint(), pointsStack.getLastPoint(), orderedTrackPoints[i]) == false)
                {
                    pointsStack.pop();
                }
                pointsStack.push(orderedTrackPoints[i]);
            }

            convexHullPoints = pointsStack.getStack().ToArray();

        }

        bool checkLeft(TrackPoint A, TrackPoint B, TrackPoint C) 
        {
            //Checks if point C is to the left of the line between points A and B

            // Cross-product of lines AB and AC
            double result = ((B.getPosition().X - A.getPosition().X) * (A.getPosition().Y - C.getPosition().Y)) - ((A.getPosition().Y - B.getPosition().Y) * (C.getPosition().X - A.getPosition().X)); 

            if (result < 0)
            {
                return false;
            }

            return true;
        }

        void findFinalTrackPoints()
        {
            //Method that takes the straight line track and adds curves between the points

            for (int i = 0; i < convexHullPoints.Length - 1; i++)
            {

                TrackPoint[] tempPoints = findBezierCurve(convexHullPoints[i], findCurvePoint(convexHullPoints[i], convexHullPoints[i + 1]), convexHullPoints[i + 1]);

                for (int j = 0; j < tempPoints.Length; j++)
                {
                    finalPoints.Add(tempPoints[j]);
                }
            }

        }

        TrackPoint findCurvePoint(TrackPoint point1, TrackPoint point2)
        {
            //Method that finds a random point along the line between the two given points,
            //then offsets it a random distance perpendicularly from the line

            TrackPoint randomPoint = findRandomPointAlongLine(point1, point2);
            Line line = new Line(point1.getPosition(), point2.getPosition(), true);

            float perpGradient = -1 / line.getGradient();
            Line perpLine = new Line(perpGradient, randomPoint.getPosition());

            float xOffset;
            float curvePointX;
            float curvePointY;

            do
            {
                xOffset = rand.Next(-20, 20);

                curvePointX = randomPoint.getPosition().X + xOffset;
                curvePointY = perpLine.findYValue(curvePointX); 
            } while (curvePointY < gameBorderTL.Y || curvePointY > gameBorderBR.Y || curvePointX < gameBorderTL.X || curvePointX > gameBorderBR.X);

            return new TrackPoint(curvePointX, curvePointY);
        }

        TrackPoint findMidpoint(TrackPoint point1, TrackPoint point2)
        {
            //Method that find the midpoint between the two given points
            return new TrackPoint(new Vector2((point1.getPosition().X + point2.getPosition().X) / 2, (point1.getPosition().Y + point2.getPosition().Y) / 2));
        }

        TrackPoint findRandomPointAlongLine(TrackPoint point1, TrackPoint point2)
        {
            //Method that generated a random point along a line between the two given points

            float X;

            //threshold is so that the generated point isnt too close to the given points
            float threshold = 0.1f; //between 0 and 1
            Line line = new Line(point1.getPosition(), point2.getPosition(), true);
            float xDifference = point2.getPosition().X - point1.getPosition().X;
            
            float minX = xDifference * threshold + point1.getPosition().X;
            float maxX = xDifference * (1 - threshold) + point1.getPosition().X;

            if (minX < maxX)
            {
                X = rand.Next(Convert.ToInt32(minX), Convert.ToInt32(maxX));
            }
            else if (minX > maxX)
            {
                X = rand.Next(Convert.ToInt32(maxX), Convert.ToInt32(minX));
            }
            else
            {
                X = findMidpoint(point1, point2).getPosition().X;
            }
            
            float Y = line.findYValue(X);

            return new TrackPoint(X, Y);
        }

        TrackPoint[] findBezierCurve(TrackPoint p0, TrackPoint p1, TrackPoint p2)
        {
            //Method that takes 3 points and creates a curved line based on those three points

            List<Line> curveLines = new List<Line>();
            List<Vector2> POIs = new List<Vector2>();

            float tIncrement = 0.01f;

            for (float t = tIncrement; t < 1; t += tIncrement)
            {
                float pA_X = ((p1.getPosition().X - p0.getPosition().X) * t) + p0.getPosition().X;
                float pA_Y = ((p1.getPosition().Y - p0.getPosition().Y) * t) + p0.getPosition().Y;
                TrackPoint pA = new TrackPoint(new Vector2(pA_X, pA_Y));

                float pB_X = ((p2.getPosition().X - p1.getPosition().X) * t) + p1.getPosition().X;
                float pB_Y = ((p2.getPosition().Y - p1.getPosition().Y) * t) + p1.getPosition().Y;
                TrackPoint pB = new TrackPoint(new Vector2(pB_X, pB_Y));

                curveLines.Add(new Line(pA.getPosition(), pB.getPosition(), true));
            }

            for (int i = 0; i < curveLines.Count - 1; i++)
            {
                POIs.Add(Line.findPOI(curveLines[i], curveLines[i + 1])); 
            }

            TrackPoint[] returnArray = new TrackPoint[POIs.Count];
            for (int i = 0; i < returnArray.Length; i++)
            {
                returnArray[i] = new TrackPoint(POIs[i]);
            }

            return returnArray;

        }

        void  findStartPoint()
        {
            //Method that generates the start-line of the generated track
            startPoint = findRandomPointAlongLine(finalPoints[finalPoints.Count - 1], finalPoints[0]);
            findLastLine();

            float perpendicularGradient = (-1) / lastLine.getGradient();
            Line perpendicularLine = new Line(perpendicularGradient, startPoint.getPosition());

            Vector2 edge1 = perpendicularLine.findPointAtDistance(startPoint.getPosition(), trackWidth / 2, true);
            Vector2 edge2 = perpendicularLine.findPointAtDistance(startPoint.getPosition(), trackWidth / 2, false);

            startPointEdges = new TrackPoint[] { new TrackPoint(edge1), new TrackPoint(edge2) };

        }

        void findLastLine()
        {
            //Method that specifies the lastline of the track
            lastLine = new Line(finalPoints[finalPoints.Count - 1].getPosition(), finalPoints[0].getPosition(), true);
        }

        bool checkForNullPoints() 
        {
            //Method that checks if any of the points are null
            //returns false if there are null values and therefore the track is not possible
            bool returnVal = true;
            for (int i = 0; i < finalPoints.Count; i++)
            {
                if (finalPoints[i].getPosition() == null)
                {
                    returnVal = false;
                    break;
                }
                
            }

            return returnVal;
        }

        public Vector2 getStartPoint()
        {
            return startPoint.getPosition();
        }

        public TrackPoint[] getStartPointEdges()
        {
            return startPointEdges;
        }

        public Line getLastLine()
        {
            return lastLine;
        }

        void excludeOutOfRangePoints()
        {
            //Method that gets rid of any points that are outside of the gamescreen borders

            List<TrackPoint> tempFinalPonts = new List<TrackPoint>();
            for (int i = 0; i < finalPoints.Count; i++)
            {
                bool faulty = false;
                if (finalPoints[i].getPosition().X < gameBorderTL.X)
                {
                    faulty = true;
                }
                else if (finalPoints[i].getPosition().X > gameBorderBR.X)
                {
                    faulty = true;
                }
                else if (finalPoints[i].getPosition().Y < gameBorderTL.Y)
                {
                    faulty = true;
                }
                else if (finalPoints[i].getPosition().Y > gameBorderBR.Y)
                {
                    faulty = true;
                }
                else if (float.IsNaN(finalPoints[i].getPosition().X) || float.IsNaN(finalPoints[i].getPosition().Y))
                {
                    faulty = true;
                }

                if (!faulty)
                {
                    tempFinalPonts.Add(finalPoints[i]);
                }
            }

            finalPoints = tempFinalPonts;
        }

        public List<TrackPoint> getFinalPoints()
        {
            return finalPoints;
        }

        void findTrackBorders()
        {
            //Method that find the borders of the track

            Line line;
            Line parallelLine1;
            Line parallelLine2;
            bool Line1Left;

            for (int i = 0; i < finalPoints.Count - 1; i++)
            {
                line = new Line(finalPoints[i].getPosition(), finalPoints[i + 1].getPosition(), true);

                parallelLine1 = line.findParallelLine((trackWidth / 2), true);
                parallelLine2 = line.findParallelLine((trackWidth / 2), false);
                Line1Left = checkLeft(new TrackPoint(line.getPoint1()), new TrackPoint(line.getPoint2()), new TrackPoint(parallelLine1.getPoint2()));
                if (Line1Left)
                {
                    insideLineBorder.Add(parallelLine1);
                    outsideLineBorder.Add(parallelLine2);
                }
                else
                {
                    insideLineBorder.Add(parallelLine2);
                    outsideLineBorder.Add(parallelLine1);
                }
            }
            line = new Line(finalPoints[finalPoints.Count - 1].getPosition(), finalPoints[0].getPosition(), true);
            parallelLine1 = line.findParallelLine(trackWidth / 2, true);
            parallelLine2 = line.findParallelLine(trackWidth / 2, false);
            Line1Left = checkLeft(new TrackPoint(line.getPoint1()), new TrackPoint(line.getPoint2()), new TrackPoint(parallelLine1.getPoint2()));
            if (Line1Left)
            {
                insideLineBorder.Add(parallelLine1);
                outsideLineBorder.Add(parallelLine2);
            }
            else
            {
                insideLineBorder.Add(parallelLine2);
                outsideLineBorder.Add(parallelLine1);
            }

        }

        public List<Line> getInsideLineBorders()
        {
            return insideLineBorder;
        }

        public List<Line> getOutsideLineBorders()
        {
            return outsideLineBorder;
        }

        void defineCheckpoints()
        {
            //Method that defines the list of checkpoints associated with the track
            checkpoints.Add(startPoint);
            for (int i = 0; i < finalPoints.Count; i++)
            {
                checkpoints.Add(finalPoints[i]);
            }
        }

        public List<TrackPoint> getCheckpoints()
        {
            return checkpoints;
        }

        public float getTrackWidth()
        {
            return trackWidth;
        }

        void eliminateOverlapLines()
        {
            //this is the code to check for POIs between each line and its subsequent line and set their lengths so that they dont overlap anymore
            Vector2 POI;
            for (int i = 0; i < insideLineBorder.Count - 1; i++)
            {
                POI = Line.findPOI(insideLineBorder[i], insideLineBorder[i + 1]);
                if (!float.IsNaN(POI.X) && !float.IsNaN(POI.X))
                {
                    insideLineBorder[i].setPoint2(POI);
                    insideLineBorder[i + 1].setPoint1(POI);
                }
            }
            POI = Line.findPOI(insideLineBorder[insideLineBorder.Count - 1], insideLineBorder[0]);
            if (!float.IsNaN(POI.X) && !float.IsNaN(POI.X))
            {
                insideLineBorder[insideLineBorder.Count - 1].setPoint2(POI);
                insideLineBorder[0].setPoint1(POI);
            }


            for (int i = 0; i < outsideLineBorder.Count - 1; i++)
            {
                POI = Line.findPOI(outsideLineBorder[i], outsideLineBorder[i + 1]);
                if (!float.IsNaN(POI.X) && !float.IsNaN(POI.X))
                {
                    outsideLineBorder[i].setPoint2(POI);
                    outsideLineBorder[i + 1].setPoint1(POI);
                }
            }
            POI = Line.findPOI(outsideLineBorder[outsideLineBorder.Count - 1], outsideLineBorder[0]);
            if (!float.IsNaN(POI.X) && !float.IsNaN(POI.X))
            {
                outsideLineBorder[outsideLineBorder.Count - 1].setPoint2(POI);
                outsideLineBorder[0].setPoint1(POI);
            }
        }

        void connectLineBorders()
        {
            //Method that connects the point2 of each line with the point1 of the subsequent line so that there are no gaps between the track's borders
            List<Line> newInsideLineBorder = new List<Line>();
            for (int i = 0; i < insideLineBorder.Count - 1; i++)
            {
                newInsideLineBorder.Add(insideLineBorder[i]);
                newInsideLineBorder.Add( new Line(insideLineBorder[i].getPoint2(), insideLineBorder[i + 1].getPoint1(), true));
            }
            newInsideLineBorder.Add(insideLineBorder[insideLineBorder.Count - 1]);
            newInsideLineBorder.Add(new Line(insideLineBorder[insideLineBorder.Count - 1].getPoint2(), insideLineBorder[0].getPoint1(), true));

            List<Line> newOutsideLineBorder = new List<Line>();
            for (int i = 0; i < outsideLineBorder.Count - 1; i++)
            {
                newOutsideLineBorder.Add(outsideLineBorder[i]);
                newOutsideLineBorder.Add(new Line(outsideLineBorder[i].getPoint2(), outsideLineBorder[i + 1].getPoint1(), true));
            }
            newOutsideLineBorder.Add(outsideLineBorder[outsideLineBorder.Count - 1]);
            newOutsideLineBorder.Add(new Line(outsideLineBorder[outsideLineBorder.Count - 1].getPoint2(), outsideLineBorder[0].getPoint1(), true));

            insideLineBorder = newInsideLineBorder;
            outsideLineBorder = newOutsideLineBorder;
        }

    }
}
