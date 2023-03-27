using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

using System;
using System.Collections.Generic;

namespace Random_Track_Generation
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //textures
        Texture2D greenRectangle;
        Texture2D carTexture;

        //coordinates to represent opposite corners of the rectangular game screen
        Vector2 gameBorderTL = new Vector2(320, 0); //Top Left Corner of game section
        Vector2 gameBorderBR = new Vector2(1920, 1080); //Bottom Right Corner of game section

        //Font used for the writing of any text in the program
        SpriteFont arial;

        KeyboardState previousKState;

        //button attributes
        GenerateTrackButton generateTrackBtn;
        GenerateThreeTracksButton genThreeTracksBtn;
        ShowTrackButton show1;
        ShowTrackButton show2;
        ShowTrackButton show3;

        GenerateStraightLineTrackButton genStraightTrackBtn;
        GenerateThreeStraightLineTracksButton genThreeStraightTracksBtn;
        ShowTrackButton showStraight1;
        ShowTrackButton showStraight2;
        ShowTrackButton showStraight3;

        SaveButton saveBtn;
        LoadButton loadBtn;
        InputTextBox saveLoadInput;

        ResetCarsButton resetCarsBtn;

        //status string to display any success or error messages 
        string statusString;

        ToggleButton AIModeBtn;
        ToggleButton trainingModeBtn;
        ToggleButton autoNextGenBtn;

        NextGenerationButton nextGenBtn;

        SaveNeuralNetworkButton saveNeuralNetBtn;
        LoadNeuralNetworkButton loadNeuralNetBtn;
        InputTextBox saveLoadNeuralNetInput;

        //current track and car attributes
        Track currentTrack;
        Car currentCar;

        //arrays for generate three tracks functionality
        Track[] threeTracks;
        Track[] threeStraightTracks;

        //all attributes used for the training mode of the program
        int[] layers = new int[] { 9, 7, 7, 4 };
        int mutationChancePercentage = 45;
        int numberOfTrainingCars = 20;
        Car[] trainingCars;
        bool allCarsCollided = false;
        int generation = 0;
        


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            //_graphics.ToggleFullScreen();
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Make the Green Rectangle for the background
            greenRectangle = loadRectangle(Convert.ToInt32(_graphics.PreferredBackBufferWidth - gameBorderTL.X), _graphics.PreferredBackBufferHeight, Color.Green);

            //load the font that is used to display all of the text in the program
            arial = Content.Load<SpriteFont>("Arial");

            previousKState = Keyboard.GetState();

            //the first track that is displayed is the default track (just a straight line)
            currentTrack = new Track(gameBorderTL, gameBorderBR, arial, true);

            //Initailise all of the buttons
            generateTrackBtn = new GenerateTrackButton(50, 200, "Generate Track", new Vector2(25, 5), arial, Mouse.GetState(), gameBorderTL, gameBorderBR, Color.Red);
            genThreeTracksBtn = new GenerateThreeTracksButton(50, 250, "Generate 3 Tracks", new Vector2(5, 60), arial, Mouse.GetState(), gameBorderTL, gameBorderBR, Color.Red);
            show1 = new ShowTrackButton(50, 50, "1", new Vector2(20, 115), arial, Mouse.GetState(), Color.Red);
            show2 = new ShowTrackButton(50, 50, "2", new Vector2(130, 115), arial, Mouse.GetState(), Color.Red);
            show3 = new ShowTrackButton(50, 50, "3", new Vector2(250, 115), arial, Mouse.GetState(), Color.Red);

            genStraightTrackBtn = new GenerateStraightLineTrackButton(50, 300, "Gen Straight Line Track", new Vector2(5, 170), arial, Mouse.GetState(), gameBorderTL, gameBorderBR, Color.Red);
            genThreeStraightTracksBtn = new GenerateThreeStraightLineTracksButton(50, 310, "Gen 3 Straight Line Track", new Vector2(5, 225), arial, Mouse.GetState(), gameBorderTL, gameBorderBR, Color.Red);
            showStraight1 = new ShowTrackButton(50, 50, "1", new Vector2(20, 280), arial, Mouse.GetState(), Color.Red);
            showStraight2 = new ShowTrackButton(50, 50, "2", new Vector2(130, 280), arial, Mouse.GetState(), Color.Red);
            showStraight3 = new ShowTrackButton(50, 50, "3", new Vector2(250, 280), arial, Mouse.GetState(), Color.Red);

            resetCarsBtn = new ResetCarsButton(50, 200, "Reset Car(s)", new Vector2(60, 345), arial, Mouse.GetState(), gameBorderTL, gameBorderBR, Color.Red);

            saveBtn = new SaveButton(50, 140, "Save Track", new Vector2(5, 400), arial, Mouse.GetState(), Color.Red);
            loadBtn = new LoadButton(50, 140, "Load Track", new Vector2(170, 400), arial, Mouse.GetState(), Color.Red);
            saveLoadInput = new InputTextBox(50, 310, "", new Vector2(5, 455), arial, Mouse.GetState(), Color.LightGray);


            AIModeBtn = new ToggleButton(50, 300, "Enable AI Mode", "Enable Manual Mode", new Vector2(10, 515), arial, Mouse.GetState(), Color.Aqua, Color.Aquamarine);
            trainingModeBtn = new ToggleButton(50, 300, "Enable training Mode", "Disable training Mode", new Vector2(10, 570), arial, Mouse.GetState(), Color.DarkOrange, Color.Yellow);
            autoNextGenBtn = new ToggleButton(75, 300, "Enable Auto Next \n generation Mode", "Disable Auto Next \n generation Mode", new Vector2(10, 625), arial, Mouse.GetState(), Color.DarkOrange, Color.Yellow);
            nextGenBtn = new NextGenerationButton(75, 300, "Click to Load \n Next Generation", new Vector2(10, 705), arial, Mouse.GetState(), gameBorderTL, gameBorderBR, Color.Orange);

            saveNeuralNetBtn = new SaveNeuralNetworkButton(50, 300, "Save Neural Network", new Vector2(10, 785), arial, Mouse.GetState(), Color.Red);
            loadNeuralNetBtn = new LoadNeuralNetworkButton(50, 300, "Load Neural Network", new Vector2(10, 785), arial, Mouse.GetState(), Color.Red);
            saveLoadNeuralNetInput = new InputTextBox(50, 310, "", new Vector2(5, 840), arial, Mouse.GetState(), Color.LightGray);

            //initialise arrays used for the functionality of the generate three tracks buttons
            threeTracks = new Track[3];
            threeStraightTracks = new Track[3];
            
            //initialise cars
            carTexture = Content.Load<Texture2D>("SmallRectangle");
            double carRotation = Math.Atan(currentTrack.getLastLine().getGradient());
            currentCar = new Car(currentTrack.getStartPoint(), (float) carRotation, carTexture, currentTrack.getCheckpoints(), currentTrack.getTrackWidth());

            trainingCars = new Car[numberOfTrainingCars];
            for (int i = 0; i < trainingCars.Length; i++)
            {
                trainingCars[i] = new Car(currentTrack.getStartPoint(), (float)carRotation, carTexture, currentTrack.getCheckpoints(), currentTrack.getTrackWidth(), layers);
            }

            //initialise the status string
            statusString = "Ready";
            
            
        }

        Texture2D loadRectangle(int width, int height, Color color) //I Made this method before i added monogame extended
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] colourPixels = new Color[width * height];
            for (int i = 0; i < colourPixels.Length; i++)
            {
                colourPixels[i] = color;
            }
            texture.SetData<Color>(colourPixels);

            return texture;
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float[] distances;
            
            generateTrackBtn.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar,ref trainingCars, ref statusString);

            genThreeTracksBtn.Update(gameTime, Mouse.GetState(), ref threeTracks[0], ref threeTracks[1], ref threeTracks[2], ref statusString);
            show1.Update(gameTime, Mouse.GetState(),ref currentTrack, ref currentCar,ref trainingCars, threeTracks[0], ref statusString);
            show2.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, threeTracks[1], ref statusString);
            show3.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, threeTracks[2], ref statusString);

            genStraightTrackBtn.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, ref statusString);

            genThreeStraightTracksBtn.Update(gameTime, Mouse.GetState(), ref threeStraightTracks[0], ref threeStraightTracks[1], ref threeStraightTracks[2], ref statusString);
            showStraight1.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, threeStraightTracks[0], ref statusString);
            showStraight2.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, threeStraightTracks[1], ref statusString);
            showStraight3.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, threeStraightTracks[2], ref statusString);

            saveLoadInput.Update(gameTime, Mouse.GetState(), Keyboard.GetState(), previousKState);
            saveBtn.Update(gameTime, Mouse.GetState(), ref currentTrack, saveLoadInput.getText(), ref statusString);
            loadBtn.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars, saveLoadInput.getText(), ref statusString);

            resetCarsBtn.Update(gameTime, Mouse.GetState(), ref currentTrack, ref currentCar, ref trainingCars);

            AIModeBtn.Update(gameTime, Mouse.GetState());
            if (AIModeBtn.getToggled() == false)
            {
                trainingModeBtn.setToggled(false);
                //could also set autoNextGenBtn to false
                //but i thought it might get annoying if the user is constantly switching between manual and AI modes
            }
            else if (AIModeBtn.getToggled() == true)
            {
                trainingModeBtn.Update(gameTime, Mouse.GetState());
                autoNextGenBtn.Update(gameTime, Mouse.GetState());
            }


            if (AIModeBtn.getToggled() == false)
            {
                //if its in manual mode
                currentCar.Update(gameTime, GraphicsDevice);
            }
            else if (AIModeBtn.getToggled() == true && trainingModeBtn.getToggled() == false)
            {
                //if its in Ai mode with just 1 car
                distances = findDistancesFromCar(currentCar);
                currentCar.Update(gameTime, GraphicsDevice, distances, false, autoNextGenBtn.getToggled());
                saveLoadNeuralNetInput.Update(gameTime, Mouse.GetState(), Keyboard.GetState(), previousKState);
                loadNeuralNetBtn.Update(gameTime, Mouse.GetState(), ref currentCar, saveLoadNeuralNetInput.getText(), ref statusString);
            }
            else if (trainingModeBtn.getToggled() == true)
            {
                //if its in training mode

                for (int i = 0; i < trainingCars.Length; i++)
                {
                    distances = findDistancesFromCar(trainingCars[i]);
                    trainingCars[i].Update(gameTime, GraphicsDevice, distances, true, autoNextGenBtn.getToggled());
                }

                saveLoadNeuralNetInput.Update(gameTime, Mouse.GetState(), Keyboard.GetState(), previousKState);

                //Had to do the save Neural Net logic in here because it requires the use of the sorting algorithms
                saveNeuralNetBtn.UpdateClicked(gameTime, Mouse.GetState());
                if (saveNeuralNetBtn.getClicked() == true)
                {
                    saveNeuralNetBtn.setClicked(false);

                    Car[] distanceSortedCars = mergeSortDistances(trainingCars);
                    Car[] fitnessSortedCars = mergeSortFitnesses(distanceSortedCars);
                    fitnessSortedCars[fitnessSortedCars.Length - 1].getNeuralNetwork().saveNeuralNetwork(saveLoadNeuralNetInput.getText(), ref statusString);
                }

                if (autoNextGenBtn.getToggled() == true)
                {
                    //if you want it to automaticcaly load the next generation once all the cars have crashed, or a certain amount of time has crashed

                    allCarsCollided = true;
                    for (int i = 0; i < trainingCars.Length; i++)
                    {
                        if (trainingCars[i].getCollided() == false)
                        {
                            allCarsCollided = false;
                            break;
                        }
                    }

                    if (allCarsCollided)
                    {
                        generateNextGeneration();
                    }
                }
                else if (autoNextGenBtn.getToggled() == false)
                {
                    //For when you want to manually move to the next generation
                    bool nextGen = nextGenBtn.Update(gameTime, Mouse.GetState());

                    if (nextGen)
                    {
                        generateNextGeneration();
                    }
                }
                
            }
            

            previousKState = Keyboard.GetState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.WhiteSmoke);
            _spriteBatch.Begin();

            //drawing the green background for the 
            _spriteBatch.Draw(greenRectangle, gameBorderTL, Color.White);


            generateTrackBtn.Draw(_spriteBatch);
            genThreeTracksBtn.Draw(_spriteBatch);
            show1.Draw(_spriteBatch);
            show2.Draw(_spriteBatch);
            show3.Draw(_spriteBatch);

            genStraightTrackBtn.Draw(_spriteBatch);
            genThreeStraightTracksBtn.Draw(_spriteBatch);
            showStraight1.Draw(_spriteBatch);
            showStraight2.Draw(_spriteBatch);
            showStraight3.Draw(_spriteBatch);

            saveBtn.Draw(_spriteBatch);
            loadBtn.Draw(_spriteBatch);
            saveLoadInput.Draw(_spriteBatch);

            resetCarsBtn.Draw(_spriteBatch);

            AIModeBtn.Draw(_spriteBatch);
            if (AIModeBtn.getToggled() == true)
            {
                // if Ai mode
                trainingModeBtn.Draw(_spriteBatch);
                saveLoadNeuralNetInput.Draw(_spriteBatch);

            }
            if (AIModeBtn.getToggled() == true && trainingModeBtn.getToggled() == false)
            {
                //if its only 1 car in ai mode
                loadNeuralNetBtn.Draw(_spriteBatch);
            }
            if (trainingModeBtn.getToggled() == true)
            {
                // if training mode
                autoNextGenBtn.Draw(_spriteBatch);
                if (autoNextGenBtn.getToggled() == false)
                {
                    nextGenBtn.Draw(_spriteBatch);
                }

                saveNeuralNetBtn.Draw(_spriteBatch);
            }
            
            //darwing the status string at the bottom
            _spriteBatch.FillRectangle(new Vector2(0, 900), new Size2(320, 75), Color.LightGreen);
            _spriteBatch.DrawString(arial, statusString, new Vector2(5, 910), Color.Black);

            //draw the track
            currentTrack.Draw(_spriteBatch);

            
            //Drawing the cars/
            if (trainingModeBtn.getToggled() == true)
            {
                for (int i = 0; i < trainingCars.Length; i++)
                {
                    trainingCars[i].Draw(_spriteBatch);
                }
            }
            else
            {
                currentCar.Draw(_spriteBatch);
                _spriteBatch.DrawString(arial, $"Speed: {Math.Round(currentCar.getSpeed(), 2)}", new Vector2(20, gameBorderBR.Y - 100), Color.Black);
            }


            _spriteBatch.End();
            base.Draw(gameTime);
        }

        float[] findDistancesFromCar(Car car)
        {
            //this is the method to find the distances between the car and the edge of the track
            float[] distancesFromCar = new float[8];
            Line[] carLines;
            List<Line> insideLineBorders;
            List<Line> outsideLineBorders;
            Vector2[] carEdgePositions;
            float minDistance = 0;


            carLines = car.getCarLines();
            carEdgePositions = car.getEdgePoints();
            insideLineBorders = currentTrack.getInsideLineBorders();
            outsideLineBorders = currentTrack.getOutsideLineBorders();

            //go through each of the cars radar lines
            for (int i = 0; i < carLines.Length; i++)
            {
                //check if there are any points of intersection between the car's line and the track's border-lines
                List<Vector2> insidePOIs = new List<Vector2>();
                for (int j = 0; j < insideLineBorders.Count; j++)
                {
                    insidePOIs.Add(Line.findPOI(carLines[i], insideLineBorders[j]));
                }

                List<Vector2> outsidePOIs = new List<Vector2>();
                for (int j = 0; j < outsideLineBorders.Count; j++)
                {
                    outsidePOIs.Add(Line.findPOI(carLines[i], outsideLineBorders[j]));
                }

                List<Vector2> allPOIs = new List<Vector2>();
                for (int j = 0; j < insidePOIs.Count; j++)
                {
                    if (!float.IsNaN(insidePOIs[j].X))
                    {
                        allPOIs.Add(insidePOIs[j]);
                    }
                }
                for (int j = 0; j < outsidePOIs.Count; j++)
                {
                    if (!float.IsNaN(outsidePOIs[j].X))
                    {
                        allPOIs.Add(outsidePOIs[j]);
                    }
                }

                
                //find distances between the car's point and any of the points of intersection
                List<float> distances = new List<float>();
                
                for (int j = 0; j < allPOIs.Count; j++)
                {
                    distances.Add((float) Track.findDistance(new TrackPoint(carEdgePositions[i]), new TrackPoint(allPOIs[j])));
                }

                if (distances.Count > 0)
                {
                    minDistance = distances[0];
                }
                
                //only keep the distance that is the lowest, i.e. the distance to the closest wall in that direction
                for (int j = 0; j < distances.Count; j++)
                {
                    if (distances[j] < minDistance)
                    {
                        minDistance = distances[j];
                    }
                }

                distancesFromCar[i] = minDistance;
            }

            return distancesFromCar;
        }


        Car[] mergeSortDistances(Car[] items)
        {
            //Sort cars by their distance to the next checkpoint, highest to lowest
            Car[] left_half;
            Car[] right_half;

            //Base case for recursion
            if (items.Length < 2)
            {
                return items;
            }

            int midpoint = items.Length / 2;

            //Do the left half
            left_half = new Car[midpoint];
            for (int i = 0; i < midpoint; i++)
            {
                left_half[i] = items[i];
            }

            //figure out how big the right half should be
            if (items.Length % 2 == 0)
            {
                right_half = new Car[midpoint];
            }
            else
            {
                right_half = new Car[midpoint + 1];
            }

            //fill in hte right half
            int rightIndex = 0;
            for (int i = midpoint; i < items.Length; i++)
            {
                right_half[rightIndex] = items[i];
                rightIndex++;
            }

            //recursion bit
            left_half = mergeSortDistances(left_half);
            right_half = mergeSortDistances(right_half);

            items = mergeDistances(left_half, right_half);

            return items;
        }

        Car[] mergeDistances(Car[] list1, Car[] list2)
        {

            Car[] merged = new Car[list1.Length + list2.Length];

            int index1 = 0;
            int index2 = 0;
            int indexMerged = 0;

            while (index1 < list1.Length && index2 < list2.Length)
            {
                if (list1[index1].getDistanceToNextCheckpoint() > list2[index2].getDistanceToNextCheckpoint())
                {
                    merged[indexMerged] = list1[index1];
                    index1++;
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

        Car[] mergeSortFitnesses(Car[] items)
        {
            //Sort cars by their fitness, lowest to highest
            Car[] left_half;
            Car[] right_half;

            //Base case for recursion
            if (items.Length < 2)
            {
                return items;
            }

            int midpoint = items.Length / 2;

            //Do the left half
            left_half = new Car[midpoint];
            for (int i = 0; i < midpoint; i++)
            {
                left_half[i] = items[i];
            }

            //figure out how big the right half should be
            if (items.Length % 2 == 0)
            {
                right_half = new Car[midpoint];
            }
            else
            {
                right_half = new Car[midpoint + 1];
            }

            //fill in hte right half
            int rightIndex = 0;
            for (int i = midpoint; i < items.Length; i++)
            {
                right_half[rightIndex] = items[i];
                rightIndex++;
            }

            //recursion bit
            left_half = mergeSortFitnesses(left_half);
            right_half = mergeSortFitnesses(right_half);

            items = mergeFitnesses(left_half, right_half);

            return items;
        }

        Car[] mergeFitnesses(Car[] list1, Car[] list2)
        {
            Car[] merged = new Car[list1.Length + list2.Length];

            int index1 = 0;
            int index2 = 0;
            int indexMerged = 0;

            while (index1 < list1.Length && index2 < list2.Length)
            {
                if (list1[index1].getFitness() <= list2[index2].getFitness())
                {
                    merged[indexMerged] = list1[index1];
                    index1++;
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

        void generateNextGeneration()
        {
            //sort cars so that in the end they are sorted by fitness (lowest to highest) and
            //any cars with the same fitness is sorted by their distance to next checkpoint (highest to lowest)
            //meaning that the worst cars will be at the lower indexes of the array
            Car[] distanceSortedCars = mergeSortDistances(trainingCars);
            Car[] fitnessSortedCars = mergeSortFitnesses(distanceSortedCars);

            double carRotation = Math.Atan(currentTrack.getLastLine().getGradient());
            
            for (int i = 0; i < trainingCars.Length; i++)
            {
                trainingCars[i] = fitnessSortedCars[i];

                //reset the cars so that all of their attributes are reset and dont have an effect on the next generation
                trainingCars[i].reset(currentTrack.getStartPoint(), (float)carRotation);
            }

            //take the Neural networks from the best half of the cars, copy them, mutate them and then give them to the worst half of the cars
            for (int i = 0; i < (trainingCars.Length / 2); i++)
            {
                NeuralNetwork copyNet = new NeuralNetwork(layers);
                copyNet = trainingCars[i + trainingCars.Length / 2].getNeuralNetwork().copyNetwork(copyNet);
                copyNet.mutate(mutationChancePercentage);
                trainingCars[i].setNeuralNetwork(copyNet);

            }

            generation++;
            statusString = $"Generation: {generation}";
            
        }

        

    }
}
