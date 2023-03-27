using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

using System;
using System.Collections.Generic;
using System.Text;

namespace Random_Track_Generation
{
    class Car
    {
        //attributes of the car
        Texture2D texture;
        Vector2 position;
        float rotation;
        double speed;
        
        KeyboardState previousKState;

        const double acceleration = 2;
        const double anglularVelocity = 0.04 * (180/Math.PI);
        bool collided;
        
        //the radar lines and the positions they originate from
        Line[] Lines = new Line[8];
        Vector2[] EdgePositions = new Vector2[8];

        //checkpoints on the track that they must pass through
        List<TrackPoint> checkpoints = new List<TrackPoint>();
        float trackwidth;

        //Used for the AI driving and training
        NeuralNetwork neuralNet;
        double totalTimeAlive;
        int[] passedCheckpointNumbers;
        float fitness;
        float distanceToNextCheckPoint;

        public Car(Vector2 position, float rotation, Texture2D texture, List<TrackPoint> checkpoints, float trackwidth, int[] layers)
        {
            //constructor for car when given the layer coposition of the neural network
            this.position = position;
            this.rotation = rotation;
            this.texture = texture;

            previousKState = Keyboard.GetState();
            speed = 0;
            collided = false;

            updateLines();
            this.checkpoints = checkpoints;
            this.trackwidth = trackwidth;
            
            totalTimeAlive = 0;

            passedCheckpointNumbers = new int[checkpoints.Count];
            for (int i = 0; i < passedCheckpointNumbers.Length; i++)
            {
                passedCheckpointNumbers[i] = 0;
            }

            neuralNet = new NeuralNetwork(layers);
        }
        public Car(Vector2 position, float rotation, Texture2D texture, List<TrackPoint> checkpoints, float trackwidth)
        {
            //constructor for when layer composition of the ent isnt given, so a default composition has been set for the neural net
            this.position = position;
            this.rotation = rotation;
            this.texture = texture;

            previousKState = Keyboard.GetState();
            speed = 0;
            collided = false;

            updateLines();
            this.checkpoints = checkpoints;
            this.trackwidth = trackwidth;
            
            totalTimeAlive = 0;

            passedCheckpointNumbers = new int[checkpoints.Count];
            for (int i = 0; i < passedCheckpointNumbers.Length; i++)
            {
                passedCheckpointNumbers[i] = 0;
            }

            neuralNet = new NeuralNetwork(new int[] { 9, 7, 7, 4 });
        }
        public Car(Vector2 position, float rotation, Texture2D texture, List<TrackPoint> checkpoints, float trackwidth, NeuralNetwork neuralNet)
        {
            //Constructor for when a car is to be made with a premade neural network
            this.position = position;
            this.rotation = rotation;
            this.texture = texture;

            previousKState = Keyboard.GetState();
            speed = 0;
            collided = false;

            updateLines();
            this.checkpoints = checkpoints;
            this.trackwidth = trackwidth;
            
            totalTimeAlive = 0;

            passedCheckpointNumbers = new int[checkpoints.Count];
            for (int i = 0; i < passedCheckpointNumbers.Length; i++)
            {
                passedCheckpointNumbers[i] = 0;
            }

            this.neuralNet = neuralNet;
        }


        public void reset(Vector2 position, float rotation)
        {
            //reset method to reset the car to its original position on a track
            //and to reset all other relevant attributes so they dont interfere with the next generation
            this.position = position;
            this.rotation = rotation;
            collided = false;
            speed = 0;
            updateLines();
            totalTimeAlive = 0;

            passedCheckpointNumbers = new int[checkpoints.Count];
            for (int i = 0; i < passedCheckpointNumbers.Length; i++)
            {
                passedCheckpointNumbers[i] = 0;
            }
        }

        public void reset(Vector2 position, float rotation, List<TrackPoint> checkpoints)
        {
            //Reset method for reseting to a new track, so the checkpoints are different
            this.position = position;
            this.rotation = rotation;
            this.checkpoints = checkpoints;
            collided = false;
            speed = 0;
            updateLines();
            totalTimeAlive = 0;

            passedCheckpointNumbers = new int[checkpoints.Count];
            for (int i = 0; i < passedCheckpointNumbers.Length; i++)
            {
                passedCheckpointNumbers[i] = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Drawing ther car

            //Draw the car a different colour if it has already collided with the wall
            if (!collided)
            {
                spriteBatch.Draw(texture, position, null, Color.White, rotation, new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, position, null, Color.Blue, rotation, new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 0f);
            }
        }

        public void Update(GameTime gameTime, GraphicsDevice GraphicsDevice)
        {
            //This update method is for manual driving of the car
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!collided)
            {
                //Values decided based on car
                int excessWidth = texture.Width / 2;
                int hitBoxSize = texture.Width + 2 * excessWidth;
                Color carColour = new Color(250, 0, 0);

                checkPixelCollision(GraphicsDevice, new Rectangle((int)position.X - excessWidth, (int)position.Y - excessWidth, hitBoxSize, hitBoxSize), hitBoxSize, carColour);
                if (collided)
                {
                    speed = 0;
                    return;
                }

                move(delta);
            }
            else if (collided)
            {
                //if the car has hit the wall, we want it to stop
                speed = 0;

            }

        }

        public void Update(GameTime gameTime, GraphicsDevice GraphicsDevice, float[] distancesToWall, bool TrainingMode, bool autoNextGen)
        {
            //This Update is for the neural network to drive the car
            float delta = (float) gameTime.ElapsedGameTime.TotalSeconds;

            //in training mode, we may want it to end the generation after a set amount of time
            //since there are some cars that may just go in circles and never collide
            if (TrainingMode && autoNextGen && totalTimeAlive > 60) 
            {
                collided = true;
            }

            if (!collided)
            {
                //Values decided based on car
                int excessWidth = 5;
                int hitBoxSize = texture.Width + 2 * excessWidth;
                Color carColour = new Color(250, 0, 0);

                checkPixelCollision(GraphicsDevice, new Rectangle((int)position.X - excessWidth, (int)position.Y - excessWidth, hitBoxSize, hitBoxSize), hitBoxSize, carColour);
                if (collided)
                {
                    speed = 0;
                    
                    fitness = calculateFitness();
                    distanceToNextCheckPoint = findDistanceToNextCheckpoint();
                    return;
                }

                totalTimeAlive += gameTime.ElapsedGameTime.TotalSeconds;

                float[] inputs = new float[9];
                for (int i = 0; i < distancesToWall.Length; i++)
                {
                    inputs[i] = distancesToWall[i];
                }
                inputs[8] = (float) speed;

                aiMove(delta, inputs);
                
                updateLines();
                checkCheckPointPassed();

                //move(delta);
            }
            else if (collided)
            {
                //if the car has hit the wall, we want it to stop and then evaluate how well it did
                speed = 0;
                if (TrainingMode)
                {
                    fitness = calculateFitness();
                    distanceToNextCheckPoint = findDistanceToNextCheckpoint();
                }
                
            }
            
        }
        
        void move(float delta)
        {
            //This is the move method to drive the car manually using WASD

            KeyboardState currentKState = Keyboard.GetState();

            if (currentKState.IsKeyDown(Keys.W))
            {
                speed += acceleration;
            }
            if (currentKState.IsKeyDown(Keys.S))
            {
                speed -= acceleration;
            }
            if (currentKState.IsKeyDown(Keys.A))
            {
                rotation -= (float) anglularVelocity * delta;
                if (rotation <= 2 * Math.PI)
                {
                    rotation += (float) (2 * Math.PI);
                }
            }
            if (currentKState.IsKeyDown(Keys.D))
            {
                rotation += (float)anglularVelocity * delta;
                if (rotation >= 2 * Math.PI)
                {
                    rotation -= (float) (2 * Math.PI);
                }
            }

            speed = speed * 0.995;
            

            position.X += (float) (Math.Cos(rotation) * speed * delta);
            position.Y += (float)(Math.Sin(rotation) * speed * delta);
        }
        
        public double getSpeed()
        {
            return speed;
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public float getRotation()
        {
            return rotation;
        }

        void checkPixelCollision(GraphicsDevice GraphicsDevice, Rectangle HitBox, int HitBoxSize, Color CarColor)
        {
            //this method is to check the pixels around the car to see if it has hit (collided with) any green pixels

            //get all of the pixel data around the car
            Color[] colourData = new Color[HitBoxSize * HitBoxSize];
            GraphicsDevice.GetBackBufferData<Color>(HitBox, colourData, 0, colourData.Length);
            Color[,] newColourData = new Color[HitBoxSize, HitBoxSize];
            int tempx = 0;
            int tempy = 0;

            //converts the 1D array to a 2D array so that it is easier to work with
            for (int i = 0; i < colourData.Length; i++)
            {
                if (tempx == HitBoxSize)
                {
                    tempx -= HitBoxSize;
                    tempy++;
                }
                newColourData[tempx, tempy] = colourData[i];
                tempx++;
            }

            //check for collision
            for (int x = 1; x < newColourData.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < newColourData.GetLength(1) - 1; y++)
                {
                    if (newColourData[x,y] == CarColor) 
                    {
                        if (newColourData[x + 1, y] == Color.Green)
                        {
                            collided = true;
                        }
                        else if (newColourData[x - 1, y] == Color.Green)
                        {
                            collided = true;
                        }
                        else if (newColourData[x, y + 1] == Color.Green)
                        {
                            collided = true;
                        }
                        else if (newColourData[x, y - 1] == Color.Green)
                        {
                            collided = true;
                        }
                    }
                }

            }

        }

        void updateLines()
        {
            updateEdgePositions();

            for (int i = 0; i < 4; i++)
            {
                //float minX, maxX, minY, maxY;
                double rotationalAngle = rotation;
                switch (i)
                {
                    case 0:
                        rotationalAngle = rotation;
                        break;
                    case 1:
                        rotationalAngle = rotation + Math.Atan((double)(texture.Height / 2) / (double)(texture.Width / 2));
                        break;
                    case 2:
                        rotationalAngle = rotation + (Math.PI / 2);
                        break;
                    case 3:
                        rotationalAngle = rotation +  Math.PI - Math.Atan((double)(texture.Height / 2) / (double)(texture.Width / 2));
                        break;
                }

                float LineGradient = (float)Math.Tan(rotationalAngle);
                if (rotationalAngle == Math.PI/2)
                {
                    LineGradient = float.PositiveInfinity;
                }
                if (rotationalAngle == -Math.PI / 2)
                {
                    LineGradient = float.NegativeInfinity;
                }
                
                

                if (EdgePositions[i].X < EdgePositions[i + 4].X) //minimum x value is i
                {
                    if (EdgePositions[i].Y > EdgePositions[i + 4].Y) //minimum y value is i
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, float.NaN, EdgePositions[i].X, EdgePositions[i].Y, float.NaN);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, EdgePositions[i + 4].X, float.NaN, float.NaN, EdgePositions[i + 4].Y);

                    }
                    else if (EdgePositions[i].Y < EdgePositions[i + 4].Y)
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, float.NaN, EdgePositions[i].X, float.NaN, EdgePositions[i].Y);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, EdgePositions[i + 4].X, float.NaN, EdgePositions[i + 4].Y, float.NaN);
                    }
                    else
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, float.NaN, EdgePositions[i].X, float.NaN, float.NaN);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, EdgePositions[i + 4].X, float.NaN, float.NaN, float.NaN);
                    }
                    
                }
                else if (EdgePositions[i].X > EdgePositions[i + 4].X) //minimum x value is i
                {
                    if (EdgePositions[i].Y > EdgePositions[i + 4].Y) //minimum y value is i
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, EdgePositions[i].X, float.NaN, EdgePositions[i].Y, float.NaN);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, float.NaN, EdgePositions[i + 4].X, float.NaN, EdgePositions[i + 4].Y);

                    }
                    else if (EdgePositions[i].Y < EdgePositions[i + 4].Y)
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, EdgePositions[i].X, float.NaN, float.NaN, EdgePositions[i].Y);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, float.NaN, EdgePositions[i + 4].X, EdgePositions[i + 4].Y, float.NaN);
                    }
                    else
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, EdgePositions[i].X, float.NaN, float.NaN, float.NaN);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, float.NaN, EdgePositions[i + 4].X, float.NaN, float.NaN);
                    }

                }
                else
                {
                    if (EdgePositions[i].Y > EdgePositions[i + 4].Y) //minimum y value is i
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, float.NaN, float.NaN , EdgePositions[i].Y, float.NaN);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, float.NaN, float.NaN, float.NaN, EdgePositions[i + 4].Y);

                    }
                    else if (EdgePositions[i].Y < EdgePositions[i + 4].Y)
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, float.NaN, float.NaN, float.NaN, EdgePositions[i].Y);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, float.NaN, float.NaN, EdgePositions[i + 4].Y, float.NaN);
                    }
                    else
                    {
                        Lines[i] = new Line(LineGradient, EdgePositions[i], true, float.NaN, float.NaN, float.NaN, float.NaN);
                        Lines[i + 4] = new Line(LineGradient, EdgePositions[i + 4], true, float.NaN, float.NaN, float.NaN, float.NaN);
                    }
                }

            }

        }

        void updateEdgePositions()
        {
            Double diagonalDistance = Math.Sqrt(((texture.Width / 2) * (texture.Width / 2)) + ((texture.Height / 2) * (texture.Height / 2)));
            Double diagonalAngle = Math.Atan((double) (texture.Height / 2) / (double) (texture.Width / 2));
            EdgePositions[0] = new Vector2(position.X + (float)(Math.Cos(rotation) * (texture.Width / 2)), (float)(position.Y + (float)(Math.Sin(rotation) * (texture.Width / 2))));
            EdgePositions[1] = new Vector2(position.X + (float)(Math.Cos(rotation + diagonalAngle) * diagonalDistance), (float)(position.Y + (float)(Math.Sin(rotation + diagonalAngle) * diagonalDistance)));
            EdgePositions[2] = new Vector2(position.X + (float)(Math.Cos(rotation + MathHelper.ToRadians(90)) * (texture.Height / 2)), (float)(position.Y + (float)(Math.Sin(rotation + MathHelper.ToRadians(90)) * (texture.Height / 2))));
            EdgePositions[3] = new Vector2(position.X + (float)(Math.Cos(rotation + Math.PI - diagonalAngle) * diagonalDistance), (float)(position.Y + (float)(Math.Sin(rotation + Math.PI - diagonalAngle) * diagonalDistance)));
            EdgePositions[4] = new Vector2(position.X + (float)(Math.Cos(rotation + MathHelper.ToRadians(180)) * (texture.Width / 2)), (float)(position.Y + (float)(Math.Sin(rotation + MathHelper.ToRadians(180)) * (texture.Width / 2))));
            EdgePositions[5] = new Vector2(position.X + (float)(Math.Cos(rotation + Math.PI + diagonalAngle) * diagonalDistance), (float)(position.Y + (float)(Math.Sin(rotation + Math.PI + diagonalAngle) * diagonalDistance)));
            EdgePositions[6] = new Vector2(position.X + (float)(Math.Cos(rotation + MathHelper.ToRadians(270)) * (texture.Height / 2)), (float)(position.Y + (float)(Math.Sin(rotation + MathHelper.ToRadians(270)) * (texture.Height / 2))));
            EdgePositions[7] = new Vector2(position.X + (float)(Math.Cos(rotation + (2 * Math.PI) - diagonalAngle) * diagonalDistance), (float)(position.Y + (float)(Math.Sin(rotation + (2 * Math.PI) - diagonalAngle) * diagonalDistance)));
        }

        public Line[] getCarLines()
        {
            return Lines;
        }

        public Vector2[] getEdgePoints()
        {
            return EdgePositions;
        }

        void aiMove(float delta, float[] inputs)
        {
            //this is the method to make the car drive using its neural network

            float[] outputs = neuralNet.FeedForward(inputs);

            //methods are given in the outputs of the neural net as inputs
            //how much movement is caused will be based on these outputs
            moveForward(outputs[0], delta);
            moveBackward(outputs[1], delta);
            turnLeft(outputs[2], delta);
            turnRight(outputs[3], delta);

            speed = speed * 0.995;

            position.X += (float)(Math.Cos(rotation) * speed * delta);
            position.Y += (float)(Math.Sin(rotation) * speed * delta);
        }

        void turnLeft(float input, float delta)
        {
            rotation -= (float)anglularVelocity * input * delta;
            if (rotation <= 2 * Math.PI)
            {
                rotation += (float)(2 * Math.PI);
            }
        }

        void turnRight(float input, float delta)
        {
            rotation += (float)anglularVelocity * input * delta;
            if (rotation >= 2 * Math.PI)
            {
                rotation -= (float)(2 * Math.PI);
            }
        }

        void moveForward(float input, float delta)
        {
            speed += acceleration * input;
        }

        void moveBackward(float input, float delta)
        {
            speed -= acceleration * input;
        }

        void checkCheckPointPassed()
        {
            //method to check if the car has passed a checkpoint

            //itterates through all of the checkpoints
            for (int i = 0; i < checkpoints.Count; i++)
            {
                //finds distance between car and checkpoint
                double distance = Track.findDistance(new TrackPoint(position), checkpoints[i]);

                // if the distance is less than half of the trackwidth, then it has passed the checkpoint
                if (distance <= (trackwidth / 2) + 10)
                {
                    //This is to check that the car hasnt skipped any checkpoints and to make sure its not being rewarded for going backwards
                    //it only counts as passing a checkpoint once the previous one has been passed as well
                    if (i == 0)
                    {
                        if (passedCheckpointNumbers[passedCheckpointNumbers.Length - 1] == passedCheckpointNumbers[i])
                        {
                            passedCheckpointNumbers[i]++;
                        }


                    }
                    else if (passedCheckpointNumbers[i - 1] == passedCheckpointNumbers[i] + 1)
                    {
                        passedCheckpointNumbers[i]++;
                    }
                }
            }
        }

        float calculateFitness()
        {
            //this method is to calculate the fitness of the car

            //it just takes the sum of the 
            float sum = 0;
            for (int i = 0; i < passedCheckpointNumbers.Length; i++)
            {
                sum += passedCheckpointNumbers[i];
            }
            return sum;
        }

        public float getFitness()
        {
            return fitness;
        }

        public bool getCollided()
        {
            return collided;
        }

        int findNextCheckpointIndex(int[] passedCheckpoints)
        {
            //method to find the index of the next checkpoint that the car has to pass
            int lap = passedCheckpoints[passedCheckpoints.Length - 1];
            for (int i = 0; i < passedCheckpoints.Length; i++)
            {
                passedCheckpoints[i] = passedCheckpoints[i] - lap;
            }

            for (int i = 0; i < passedCheckpoints.Length; i++)
            {
                if (passedCheckpoints[i] == 0)
                {
                    return i;
                }
            }

            return 0;
        }

        float findDistanceToNextCheckpoint()
        {
            //method to find the distance between the next car and the next checkpoint
            int index = findNextCheckpointIndex(passedCheckpointNumbers);
            TrackPoint nextCheckPoint = checkpoints[index];

            return (float) Track.findDistance(new TrackPoint(position), nextCheckPoint);
        }

        public float getDistanceToNextCheckpoint()
        {
            return distanceToNextCheckPoint;
        }

        public NeuralNetwork getNeuralNetwork()
        {
            return neuralNet;
        }

        public void setNeuralNetwork(NeuralNetwork neuralNet)
        {
            this.neuralNet = neuralNet;
        }

        


    }
}
