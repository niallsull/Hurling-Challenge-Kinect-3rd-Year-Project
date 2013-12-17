using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BetterSkinned;
using AnimationAux;

namespace KinectWithGameState
{
    
    public class Level : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
       
        KinectSensor kinect;                // actual sensor
        Texture2D leftWrist, wrist, windArrow, overLay, image;            // various textures
        Skeleton[] skeletonData;
        Skeleton skeleton;             // the users skeleton
        SpriteFont spritefont,instructText;        // for text
        
        //String print;
        
        Vector2 wind,originWind,writeVector;
        float hipHeight;

        Joint HEAD, rightHand, leftHand, hipCentre;       // head space comparison,for right hand space comparion , left hand space comparison, hip for refernce during squat
            
        
        JointCollection joints;         // skeletons joint collection will be assigned to this in update
        enum HandCordination { leftHanded = -1, undefined, rightHanded };
        
        
        HandCordination hand;
        enum State { leftOrRightHand, confimHand, rotate, confirmRotate, trajectory, confirmTrajectory, power, standParallel,squatTransition, squatPick, strikePosition, struck, watch };
        State state;
        
        Boolean score = false;
        Boolean wide = false;
        Boolean first = false;

        

        
        
        Screen confirmScreen = new Screen(new Vector2(0, 0));

        float[] power = new float[3];  // trigonometry for this

        Texture2D yellowBall, yellowTarget,scoreText,wideText;
        
        Vector2 yellowBallPos;
        int accuracy = 0;
        int frame, increment,speed;

        
        
        Model Stadium, GoalPosts, Sliothar;

        
        

        Vector3 modelPosition = new Vector3(0, 0, 0);
        Vector3 camPosition = new Vector3(90f, 1.5f, 46f);
        Vector3 camSpot = new Vector3(90.0f, 1.5f, 0.0f);
        Vector3 goalPosition = new Vector3(90, 0, 0);
        float gooalRotation = MathHelper.ToRadians(0);
        Vector3 sliotharPosition = new Vector3(90f, 1.5f, 41);
        Vector3 camOrigin,sliotharOrigin;

        SoundEffect puck,crowd;
        SoundEffectInstance puckInstance,crowdInstance;
        

        BoxObject stadium;
        GoalPostObject Goals;
        BallObject ball;
        float trajectory, powerToball, Xwind, Zwind, aspectRatio, rotation, windRotation, arrowRotation;
        // various floats, power to the ball will be with trigonometry


        int gameTimer = 0;
        
        Space space;
        
        Random rand;
        int levelEndTimer = 0;

        AnimatedModel model, animation;  // taken form the box, BetterSkinnedSample   http://metlab.cse.msu.edu/betterskinned.html
        // and bone,camera were taken from the betterskinned project into KinectWithGameState project, and referneces to
        // animationPipeline for loading the models and animationAux

        Vector3 playerPosition;

        public Level(GraphicsDeviceManager g)
        {
            //graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics = g;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 720;          // output screen dimensions
        }

        
        public void Initializes(KinectSensor kinect, Skeleton skeleton,Skeleton[] skelData)
        {
            // TODO: Add your initialization logic here

            this.kinect = kinect;        // copy over all the kinect stuff
            this.skeleton = skeleton;
            this.skeletonData = skelData;
           
            rotation = 0;                  // player rotation is set to zero at start
            //print = "Raise Preferred Hand Forward";
            hand = HandCordination.undefined;
            state = State.leftOrRightHand;
           
            yellowBallPos = new Vector2(400, 300);

            space = new Space();
            rand = new Random();

            sliotharPosition = new Vector3(rand.Next(10, 170),.1f, rand.Next(40, 150));
            camPosition.X = sliotharPosition.X;
            camPosition.Y = 0.5f;
            camPosition.Z = sliotharPosition.Z+5;  // place camera behnd sliothar
            camOrigin = camPosition;
            
            playerPosition = new Vector3(sliotharPosition.X - 0.5f, 0, sliotharPosition.Z); // and player to the side
            sliotharOrigin = sliotharPosition;

            Xwind = rand.Next(-20, 20);  // supposedly random wind
            Zwind = rand.Next(-20, 20);  // applied like gravity

            hipHeight = 0;       // reference taken at start, for the squatting part
            frame = 0;
            increment = 0;
            windRotation = (float)Math.Atan(mod(Zwind) / mod(Xwind));  // get rotation to apply to the arrow
            // applies to 1st quadrant, below fixes it depending on wind values


            wind = new Vector2(Xwind, Zwind);
            originWind = new Vector2(Xwind, Zwind);
            double Windspeed = Math.Sqrt((Xwind * Xwind) + (Zwind * Zwind));   // give player a wind speed

            

            if (Windspeed >= 5)
                speed = 10;

            if (Windspeed > 10)
                speed = 5;

            if (speed < 5)
                speed = 15;


            calcWindRotate();
            //base.Initialize();
        }

        public void calcWindRotate() // re calculate wind when player does rotation part
        {

            wind.X = (float)(((originWind.X) * Math.Cos(MathHelper.ToRadians(rotation))) - ((originWind.Y) * Math.Sin(MathHelper.ToRadians(rotation))));
            wind.Y = (float)(((originWind.X) * Math.Sin(MathHelper.ToRadians(rotation))) + ((originWind.Y) * Math.Cos(MathHelper.ToRadians(rotation))));


            windRotation = (float)Math.Atan(wind.Y / wind.X);

            
        }
        
        public void LoadContents(SpriteBatch s, ContentManager C,float windX, float windZ)
        {
            // load everything

           
            image = C.Load<Texture2D>("Images\\jimmy");        //  red square for joints
            confirmScreen.image = C.Load<Texture2D>("Images\\ConfirmOverlay");
           
            
            leftWrist = C.Load<Texture2D>("Images\\handLeft");
            wrist = C.Load<Texture2D>("Images\\handRight");
            yellowBall = C.Load<Texture2D>("Images\\yellowBall");
            yellowTarget = C.Load<Texture2D>("Images\\yellowTarget");
            windArrow = C.Load<Texture2D>("Images\\WindArrow");
            overLay = C.Load<Texture2D>("Images\\OverlayNew");
            scoreText = C.Load<Texture2D>("Images\\Score");
            wideText = C.Load<Texture2D>("Images\\Wide");


            instructText = C.Load<SpriteFont>("Text\\instructions");
            spritefont = C.Load<SpriteFont>("Text\\priteFont"); 
            

            Stadium = C.Load<Model>("Models\\Stadium2");
            GoalPosts = C.Load<Model>("Models\\GoalPosts2");
            Sliothar = C.Load<Model>("Models\\Sliothar");

            
            model = new AnimatedModel("HurlerSimpleRigUV",this.graphics,s);  // standing model
            animation = new AnimatedModel("HurlerFullAnim", this.graphics, s); // model with movements
            model.LoadContent(C);
            animation.LoadContent(C);

            

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            puck = C.Load<SoundEffect>("Audio\\slit1");
            puckInstance = puck.CreateInstance();
            crowd = C.Load<SoundEffect>("Audio\\Sports_Crowd");
            crowdInstance = crowd.CreateInstance();
            crowdInstance.IsLooped = true;

            stadium = new BoxObject(new Vector3(0,-.1f,0), Stadium, 1000, 0.1f, 1000);
            Goals = new GoalPostObject(goalPosition, GoalPosts, 80, 0.1f);
            ball = new BallObject(sliotharPosition, Sliothar, 0.05f);
           

            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
            Box behindGoals = new Box(new Vector3(90, 40, -5), 150, 100, 2);
            ball.sphere.LinearDamping = 0.1f;
            ball.sphere.Material.KineticFriction = 1f;
            ball.sphere.Material.StaticFriction = 3f;     // ball doesn't really roll or bounce,  go straight over the bar lads!!!
            stadium.box.Material.KineticFriction = 3f;
            stadium.box.Material.StaticFriction = 3f;

            space.Add(stadium.box);
            space.Add(Goals.post1);   // add parts to our space
            space.Add(Goals.post2);
            space.Add(ball.sphere);
            space.Add(behindGoals);   // just to stop the ball going into the stadium and drawing some quare stuff


            rotation = MathHelper.ToDegrees((float)Math.Atan((modulus(ball.sphere.Position.X - camSpot.X)) / (ball.sphere.Position.Z)));

            if (ball.sphere.Position.X > camSpot.X)
            {
                camPosition.X = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Sin(MathHelper.ToRadians(rotation))) + ((camOrigin.X - sliotharPosition.X) * Math.Cos(MathHelper.ToRadians(rotation)))) + sliotharPosition.X;
                camPosition.Z = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Cos(MathHelper.ToRadians(rotation))) - ((camOrigin.X - sliotharPosition.X) * Math.Sin(MathHelper.ToRadians(rotation)))) + sliotharPosition.Z;
                // rotate the camera behind the player in line with where we are looking at
            }

            else
            {
                camPosition.X = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Sin(MathHelper.ToRadians(-rotation))) + ((camOrigin.X - sliotharPosition.X) * Math.Cos(MathHelper.ToRadians(-rotation)))) + sliotharPosition.X;
                camPosition.Z = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Cos(MathHelper.ToRadians(-rotation))) - ((camOrigin.X - sliotharPosition.X) * Math.Sin(MathHelper.ToRadians(-rotation)))) + sliotharPosition.Z;
                
            }

            calcWindRotate();
            // TODO: use this.Content to load your game content here
        }

       
        public void UnloadContents()
        {
            // TODO: Unload any non ContentManager content here
        }



        // need to work with positive values sometimes
        public float modulus(float x)
        {
            if (x < 0)
                x *= -1;

            return x;

        }

        // cheering crowd starts
        public void startCrowd()
        {
            crowdInstance.Volume = 0.6f;
            crowdInstance.Play();

        }

        // and stops
        public void stopCrowd()
        {
            crowdInstance.Stop();
        }
        public int Update(GameTime gameTime)
        {


            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            space.Update(); // udpdate space, ball dropping , hit the posts, etc
            model.Update(gameTime);

            


            if (skeleton != null)
            {

                if (skeleton.Joints != null)
                {

                    joints = skeleton.Joints;        // copy the jointCollection to joints

                    rightHand = joints[JointType.HandRight];     // get the right hand of skeleton being tracked
                    leftHand = joints[JointType.HandLeft];  // get position of left hand

                    HEAD = joints[JointType.Head];     // know yourself

                    hipCentre = joints[JointType.HipCenter];

                    if (state == State.leftOrRightHand)
                    {
                        hipHeight = skeleton.Joints[JointType.HipCenter].Position.Y;  // initial hipHeight

                       
                       // rotation = MathHelper.ToDegrees((float)Math.Atan((modulus(ball.sphere.Position.X - camSpot.X)) / (ball.sphere.Position.Z)));
                        
                        //wind.X = (float)(((originWind.X) * Math.Cos(MathHelper.ToRadians(rotation))) - ((originWind.Y) * Math.Sin(MathHelper.ToRadians(rotation))));
                        //wind.Y = (float)(((originWind.X) * Math.Sin(MathHelper.ToRadians(rotation))) - ((originWind.Y) * Math.Cos(MathHelper.ToRadians(rotation))));

                        //calcWindRotate();

                        //camPosition.X += (sliotharPosition.X);
                        //camPosition.Z += (sliotharPosition.Z);

                        if (rightHand.Position.Z >= (leftHand.Position.Z + .3))
                        {
                            
                            hand = HandCordination.leftHanded;
                            state = State.confimHand;
                            
                           
                           

                        }
                        if ((leftHand.Position.Z) >= (rightHand.Position.Z + .3))
                        {
                           
                            hand = HandCordination.rightHanded;
                            state = State.confimHand;
                           
                            
                            
                        }

                    }
                }


                if (state == State.confimHand)
                {

                    Vector2 positionLeft = new Vector2(Convert.ToInt32(((0.5f * leftHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), Convert.ToInt32(((-0.5f * leftHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));
                    Vector2 positionRight = new Vector2(Convert.ToInt32(((0.5f * rightHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), Convert.ToInt32(((-0.5f * rightHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));


                    if ((positionLeft.X > 300) && (positionLeft.X < 400) && (positionLeft.Y < 460) && (positionLeft.Y > 360))
                    {
                       
                        state = State.leftOrRightHand;
                        
                    }
                    else if ((positionRight.X > 600) && (positionRight.X < 700) && (positionRight.Y < 460) && (positionRight.Y > 360))
                    {
                        
                        state = State.rotate;
                        
                    }

                }




                if (state == State.rotate)
                {
                    if ((rightHand.Position.Y >= HEAD.Position.Y) && (leftHand.Position.Y < HEAD.Position.Y))
                    {  
                        camSpot.X += 0.1f;
                    }


                    if ((leftHand.Position.Y >= HEAD.Position.Y) && (rightHand.Position.Y < HEAD.Position.Y))
                    {    
                        camSpot.X -= 0.1f;
                    }


                   // camPosition.X -= (sliotharPosition.X);
                    //camPosition.Y -= (sliotharPosition.Y);

                    rotation = MathHelper.ToDegrees((float)Math.Atan((modulus(ball.sphere.Position.X - camSpot.X)) / (ball.sphere.Position.Z)));
                    
                   // arrowRotation = windRotation + MathHelper.ToRadians(rotation);

                    //camPosition.X += (sliotharPosition.X);
                    //camPosition.Z += (sliotharPosition.Z);

                    
                    calcWindRotate();

                    if (ball.sphere.Position.X > camSpot.X)
                    {
                        camPosition.X = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Sin(MathHelper.ToRadians(rotation))) + ((camOrigin.X - sliotharPosition.X) * Math.Cos(MathHelper.ToRadians(rotation)))) + sliotharPosition.X;
                        camPosition.Z = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Cos(MathHelper.ToRadians(rotation))) - ((camOrigin.X - sliotharPosition.X) * Math.Sin(MathHelper.ToRadians(rotation)))) + sliotharPosition.Z;
                        // rotate the camera behind the player in line with where we are looking at
                    }

                    else
                    {
                        camPosition.X = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Sin(MathHelper.ToRadians(-rotation))) + ((camOrigin.X - sliotharPosition.X) * Math.Cos(MathHelper.ToRadians(-rotation)))) + sliotharPosition.X;
                        camPosition.Z = (float)(((camOrigin.Z - sliotharPosition.Z) * Math.Cos(MathHelper.ToRadians(-rotation))) - ((camOrigin.X - sliotharPosition.X) * Math.Sin(MathHelper.ToRadians(-rotation)))) + sliotharPosition.Z;

                    }

                    
                    if ((leftHand.Position.Y >= HEAD.Position.Y) && (rightHand.Position.Y >= HEAD.Position.Y))
                    {
                       
                       
                        state = State.confirmRotate;

                        
                    }
                }



                if (state == State.confirmRotate)
                {

                    Vector2 positionLeft = new Vector2(Convert.ToInt32(((0.5f * leftHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), Convert.ToInt32(((-0.5f * leftHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));
                    Vector2 positionRight = new Vector2(Convert.ToInt32(((0.5f * rightHand.Position.X) + 0.5f) * graphics.PreferredBackBufferWidth), Convert.ToInt32(((-0.5f * rightHand.Position.Y) + 0.5f) * graphics.PreferredBackBufferHeight));


                    if ((positionLeft.X > 300) && (positionLeft.X < 400) && (positionLeft.Y < 460) && (positionLeft.Y > 360))
                    {
                       
                        state = State.rotate;
                    }
                    else if ((positionRight.X > 600) && (positionRight.X < 700) && (positionRight.Y < 460) && (positionRight.Y > 360))
                    {
                      
                        state = State.trajectory;
                    }

                }



                if (state == State.trajectory)
                {

                    gameTimer++;

                    double opposite = 0;
                    double adjacent = 0;

                    if (hand == HandCordination.rightHanded)
                    {
                        opposite = skeleton.Joints[JointType.HandRight].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y;
                        adjacent = skeleton.Joints[JointType.ShoulderRight].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z;
                    }

                    if (hand == HandCordination.leftHanded)
                    {
                        opposite = skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y;
                        adjacent = skeleton.Joints[JointType.ShoulderLeft].Position.Z - skeleton.Joints[JointType.HandLeft].Position.Z;
                    }

                    trajectory = (float)Math.Atan(opposite / adjacent);
                    trajectory = MathHelper.ToDegrees(trajectory);
                    if (trajectory < 0) // unless you want to drill it into the ground
                        trajectory *= -1;

                    if (gameTimer > 350)
                    {
                       
                        state = State.standParallel;
                    }

                }

                /*
                if (state == State.power)
                {
                    gameTimer++;

                    double MaxPower = skeleton.Joints[JointType.Head].Position.Y - skeleton.Joints[JointType.HipCenter].Position.Y;
                    double currentPowerRight = skeleton.Joints[JointType.HandRight].Position.Y - skeleton.Joints[JointType.HipCenter].Position.Y;
                    double currentPowerLeft = skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.HipCenter].Position.Y;

                    double avgPower = (currentPowerLeft + currentPowerRight) / 2;

                    double actualPower = (avgPower / MaxPower) * 100;

                    if (actualPower > 100)
                        actualPower = 100;

                    if (actualPower < 10)
                        actualPower = 10;

                    powerToball = (float)actualPower;
                    if (gameTimer > 700)
                    {
                        powerToball = (float)actualPower;
                        calcPowerValues(powerToball, rotation, trajectory);
                        state = State.standParallel;
                        print = "Stand To Strike";
                    }

                }
                 * */



                if (state == State.standParallel)
                {

                    if (((skeleton.Joints[JointType.HipRight].Position.X) >= ((skeleton.Joints[JointType.HipLeft].Position.X) - .14)) && (skeleton.Joints[JointType.HipRight].Position.X) <= ((skeleton.Joints[JointType.HipLeft].Position.X) + .14))
                    {
                        
                        state = State.squatTransition;
                    }




                }

                if (state == State.squatTransition)
                {
                    

                    double x1 = Convert.ToDouble(skeleton.Joints[JointType.HipCenter].Position.X);
                    double y1 = Convert.ToDouble(skeleton.Joints[JointType.HipCenter].Position.Y);

                    double x2 = Convert.ToDouble(skeleton.Joints[JointType.KneeLeft].Position.X);
                    double y2 = Convert.ToDouble(skeleton.Joints[JointType.KneeLeft].Position.Y);

                    double distance = Math.Sqrt( ((x1-x2)*(x1-x2)) + ( (y1-y2)*(y1-y2)) );

                    double height = 0;
                    height = mod(mod(y2) - mod(y1));
                    

                  height = mod(y2 - y1);

                    if (y1 < hipHeight-0.18f)
                    {
                        
                        state = State.squatPick;
                    }
                }


                if (state == State.squatPick)
                {

                    double x1 = Convert.ToDouble(skeleton.Joints[JointType.HipCenter].Position.X);
                    double y1 = Convert.ToDouble(skeleton.Joints[JointType.HipCenter].Position.Y);

                    double x2 = Convert.ToDouble(skeleton.Joints[JointType.KneeLeft].Position.X);
                    double y2 = Convert.ToDouble(skeleton.Joints[JointType.KneeLeft].Position.Y);

                    double distance = Math.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)));

                    double height = 0;
                    height=mod(mod(y2)-mod(y1));
                    
                    if (y1 < hipHeight - 0.18f)
                    {
                        powerToball+=0.5f;
                    }

                    else
                    {
                       
                        if (powerToball < 10)
                            powerToball = 10;

                        if (powerToball > 100)
                            powerToball = rand.Next(20, 80);

                        calcPowerValues(powerToball, rotation, trajectory);
                        state = State.strikePosition;
                    }




                }


                if (state == State.strikePosition)
                {
                    //drawBall = true;
                    yellowBallPos.Y += 1;


                    if ((leftHand.Position.Z < hipCentre.Position.Z) && (rightHand.Position.Z < hipCentre.Position.Z) && (((joints[JointType.WristLeft].Position.X > joints[JointType.WristRight].Position.X) && (joints[JointType.WristLeft].Position.X > 0)) || ((joints[JointType.WristLeft].Position.X < joints[JointType.WristRight].Position.X) && (joints[JointType.WristLeft].Position.X < 0))))
                    {
                        // ballStruck = true;
                        AnimationClip clip = animation.Clips[0];


                        AnimationPlayer player = model.PlayClip(clip);
                        player.Looping = false;     // play the animation once
                        gameTimer = 0;
                        state = State.struck;
                       
                    }

                }

                if (state == State.struck)
                {
                    accuracy = 400 - (int)yellowBallPos.Y;

                    gameTimer++;

                    if (gameTimer > 110)
                    {

                        power[0] += (accuracy / 5);  // screw up the accuracy a bit
                        ball.sphere.LinearVelocity = new Vector3(power[0], power[1], power[2]);



                        puckInstance.Volume = .99f;
                          puckInstance.IsLooped = false;
                        puckInstance.Play();
                        //puckInstance.IsLooped = false;
                        space.ForceUpdater.Gravity = new Vector3(-Xwind/10, -9.81f, -Zwind/10);  // apply the wind

                        /*
                        if (accuracy == 0)
                           // print = "     PERFECT";

                        else if((accuracy>10) || (accuracy<-10))
                           // print="MIS-HIT";
                        */
                        state = State.watch;
                    }

                    if (gameTimer > 70)
                    {
                        sliotharPosition.Y += 0.029f;   // moving the ball up to be hit
                        sliotharPosition.X += 0.022f;

                       sliotharPosition.X = (float)(((sliotharPosition.Z-sliotharOrigin.Z) * Math.Sin(MathHelper.ToRadians(rotation))) + ((sliotharPosition.X-sliotharOrigin.X) * Math.Cos(MathHelper.ToRadians(rotation)))) + sliotharOrigin.X;
                        sliotharPosition.Z = (float)(((sliotharPosition.Z-sliotharOrigin.Z) * Math.Cos(MathHelper.ToRadians(rotation))) - ((sliotharPosition.X-sliotharOrigin.X) * Math.Sin(MathHelper.ToRadians(rotation)))) + sliotharOrigin.Z;
                       // moves properly to the player and angle

                        ball.sphere.Position = sliotharPosition;
                        ball.position = sliotharPosition;
                    }
                }


                if (state == State.watch)
                {
                    levelEndTimer++;
                    camSpot = ball.sphere.Position;
                    camPosition.X = ball.position.X;
                    camPosition.Y = ball.position.Y + 10;
                    camPosition.Z = ball.position.Z + 10;
                }


                if ((ball.position.Y > 10) && (first==false))
                {

                    first = true;
                   // space.ForceUpdater.Gravity = new Vector3(Xwind, -9.81f, Zwind);
                }
               

                if((ball.position.Y <10 ) && (first==true))
                {
                    space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
                }

            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                camPosition.Z++;

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                camPosition.X++;

            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                camPosition.X--;

            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                camPosition.Z--;

            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                kinect.ElevationAngle = (kinect.ElevationAngle + 1) % 15;

            }

            if (Keyboard.GetState().IsKeyDown(Keys.Z))
            {
                kinect.ElevationAngle = (kinect.ElevationAngle - 1) % 15;

            }
            
            


            if ((ball.position.Z >-1) && ( ball.position.Z<1) && ((score==false) && (wide==false)) )
            {
                if ((ball.position.X < 96.5f) && (ball.position.X > 83.5f))
                    score = true;         // 
                else
                    wide = true;


            }

            increment++;

            if (speed % increment == 0)
            {
                increment = 1;
                frame++;
            }

            if (frame == 3)
                frame = 0;
           
            
            ball.Update();



            if (levelEndTimer > 500)
            {
                stopCrowd();
                return 1;
            }
            else
            {
                return 0;
            }

            //base.Update(gameTime);
        }

       

        public void Draw(SpriteBatch spriteBatch)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            //spriteBatch.Begin();

            stadium.Draw(camPosition, camSpot, aspectRatio);
            Goals.Draw(camPosition, camSpot, aspectRatio);
            ball.Draw(camPosition, camSpot, aspectRatio);
            model.Draw(graphics.GraphicsDevice,camPosition,camSpot,Matrix.Identity,aspectRatio,playerPosition,rotation,ball.sphere.Position);// better skinned work

            spriteBatch.Draw(overLay, new Vector2(0, 0), Color.White);
            // draw ocerlay hud , whatever you want to call it

            
             DrawSkeleton(spriteBatch, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), image);// call skeleton draw
           
            // giving instructions to the player
            if (state == State.leftOrRightHand)
            {
                spriteBatch.DrawString(instructText, "Left or Right Handed" , new Vector2(380, 12), Color.White);
            }

            if (state == State.confimHand)
            {
                spriteBatch.DrawString(instructText, "Confirm Hand" , new Vector2(380, 12), Color.White);
            }

            if (state == State.rotate)
            {
                spriteBatch.DrawString(instructText, "Rotate Target area" , new Vector2(380, 12), Color.White);
            }


            if (state == State.confirmRotate)
            {
                spriteBatch.DrawString(instructText, "Confirm" , new Vector2(380, 12), Color.White);
            }

            if (state == State.trajectory)
            {
                spriteBatch.DrawString(instructText, "Pick trajectory" , new Vector2(380, 12), Color.White);
            }

            if (state == State.standParallel)
            {
                spriteBatch.DrawString(instructText, "Stand perpendicular" , new Vector2(380, 12), Color.White);
            }

            if (state == State.squatTransition)
            {
                spriteBatch.DrawString(instructText, "Squat to add power" , new Vector2(380, 12), Color.White);
            }


            if (state == State.strikePosition)
            {
                spriteBatch.DrawString(instructText, "Ready to strike" , new Vector2(380, 12), Color.White);
            }

            if ((hand==HandCordination.leftHanded) && (state==State.confimHand))
            {
                spriteBatch.DrawString(instructText, "Left Handed ", new Vector2(380, 40), Color.White);
            }

            if ((hand==HandCordination.rightHanded) && (state==State.confimHand))
            {
                spriteBatch.DrawString(instructText, "Right Handed ", new Vector2(380, 40), Color.White);
            }


            if (state == State.strikePosition)
            {
                
                spriteBatch.Draw(yellowTarget, new Vector2(400, 400), Color.White);
                spriteBatch.Draw(yellowBall, yellowBallPos, Color.White);
            }

            if ((state == State.confirmRotate) || (state == State.confimHand))
            {
                spriteBatch.Draw(confirmScreen.image, confirmScreen.position, Color.White);
               // spriteBatch.Draw(image, new Vector2(600, 360), Color.White);
            }

            
            
            
            // debug stuff
            //spriteBatch.DrawString(instructText, "Xwind: " + Xwind, new Vector2(700, 100), Color.Yellow);
            //spriteBatch.DrawString(instructText, "Zwind: " + Zwind, new Vector2(700, 150), Color.Yellow);

            spriteBatch.DrawString(instructText,  ""+ (int)trajectory, new Vector2(135, 270), Color.Yellow);
            spriteBatch.DrawString(instructText,  ""+ (int)rotation, new Vector2(135, 195), Color.Yellow);
            spriteBatch.DrawString(instructText,  ""+ (int)powerToball, new Vector2(135, 343), Color.Yellow);


            // and other debug stuff
            //spriteBatch.DrawString(instructText, "Impulse x: " + power[0], new Vector2(50, 250), Color.Yellow);
            //spriteBatch.DrawString(instructText, "Impulse y: " + power[1], new Vector2(50, 300), Color.Yellow);
            //spriteBatch.DrawString(instructText, "Impulse z: " + power[2], new Vector2(50, 350), Color.Yellow);
           // spriteBatch.DrawString(instructText, "sliothar position ( "+sliotharPosition.X+" , "+sliotharPosition.Y+" , "+sliotharPosition.Z+" )", new Vector2(10, 10), Color.Black);

            if (score == true)
            {
                spriteBatch.Draw(scoreText, new Vector2(312, 260), Color.White);
            }
            // if you score you're a hero, 
            if (wide == true)
            {

                spriteBatch.Draw(wideText, new Vector2(312, 260), Color.White);
            }
            // if ya miss, well @!*$ ya then

            
            spriteBatch.Draw(windArrow, new Vector2(75+30, 75+21), new Rectangle(150*frame,0,150,150), Color.White,windRotation,new Vector2(75,75),1.0f,SpriteEffects.None,0f);
            spriteBatch.DrawString(instructText, "" + (int)speed, new Vector2(30+75, 21+75), Color.Red); // draw arrow and speed
           // spriteBatch.DrawString(instructText, "" + (int)rotation, new Vector2(400, 400), Color.Red); // draw arrow and speed
           
        }


        public float mod(float f)
        {
            if (f < 0)
                f *= -1;

            return f;

        }
        // more positive values
        public double mod(double f)
        {
            if (f < 0)
                f *= -1;

            return f;

        }

        private void calcPowerValues(float powerOfBall, float rotation, float trajectory)
        {

            this.power[1] = modulus((float)Math.Sin(MathHelper.ToRadians(trajectory)) * powerOfBall);
            // how the ball goes up, has to be positive


            float distance = (float)Math.Sin(MathHelper.ToRadians(90 - trajectory)) * powerOfBall;
            //then get the horizontal distance

            this.power[0] = (float)Math.Sin(MathHelper.ToRadians(-rotation)) * distance;

            this.power[2] = (float)Math.Sin(MathHelper.ToRadians(90) - MathHelper.ToRadians(rotation)) * distance;

            if (this.power[2] > 0) // has to go towards the goal
                this.power[2] *= -1;

            if ((this.power[0] < 0) && (ball.sphere.Position.X < 90))
                this.power[0] *= -1;

        }


       



        public void DrawSkeleton(SpriteBatch spritebatch, Vector2 resolution, Texture2D img)
        {


            if (skeleton != null)
            {

                foreach (Joint joint in skeleton.Joints)
                {

                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * resolution.X), (((-0.5f * joint.Position.Y) + 0.5f) * resolution.Y));



                    if (joint.JointType == JointType.Head)
                    {
                        
                        writeVector = new Vector2(10, 10);
                        
                        spritebatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.White);

                    }
                   /*
                    if (joint.JointType == JointType.ShoulderCenter)
                    {
                        print = "ShoulderCenter: ";
                        writeVector = new Vector2(10, 20);

                    }
                    //////////////////////////////////////////
                    if (joint.JointType == JointType.ShoulderLeft)
                    {
                        print = "ShoulderLeft: ";
                        writeVector = new Vector2(10, 30);
                    }
                    //////////////////////////////////////////////
                    if (joint.JointType == JointType.ShoulderRight)
                    {
                        print = "ShoulderRight: ";
                        writeVector = new Vector2(10, 40);
                    }
                    ////////////////////////////////////////////////
                    if (joint.JointType == JointType.ElbowLeft)
                    {
                        print = "ElbowLeft: ";
                        writeVector = new Vector2(10, 50);
                    }
                    /////////////////////////////////////////////////
                    if (joint.JointType == JointType.ElbowRight)
                    {
                        print = "ElbowRight: ";
                        writeVector = new Vector2(10, 60);
                    }
                    ////////////////////////////////////////////////////////
                    
                    if (joint.JointType == JointType.WristLeft)
                    {
                        print = "WristLeft: ";
                        writeVector = new Vector2(10, 70);
                        spritebatch.Draw(leftWrist, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 50, 50), Color.White);
                    }

                    ///////////////////////////
                    if (joint.JointType == JointType.WristRight)
                    {
                        print = "WristRight: ";
                        writeVector = new Vector2(10, 80);
                        spritebatch.Draw(wrist, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 50, 50), Color.White);
                    }

                    ////////////////////////////////////////////
                    */
                    if (joint.JointType == JointType.HandLeft)
                    {
                       // print = "HandLeft: ";
                        writeVector = new Vector2(10, 90);
                        spritebatch.Draw(leftWrist, new Vector2(Convert.ToInt32(position.X), Convert.ToInt32(position.Y)), Color.White);
                    }
                    /////////////////////////////////////////

                    if (joint.JointType == JointType.HandRight)
                    {
                       // print = "HandRight: ";
                        writeVector = new Vector2(10, 100);
                        spritebatch.Draw(wrist, new Vector2(Convert.ToInt32(position.X), Convert.ToInt32(position.Y)), Color.White);
                    }

                    //////////////////////////////
                    /*
                    if (joint.JointType == JointType.Spine)
                    {
                        print = "Spine: ";
                        writeVector = new Vector2(10, 110);
                    }
                    /////////////////////////////////////////////////
                    if (joint.JointType == JointType.HipCenter)
                    {
                        print = "HipCenter: ";
                        writeVector = new Vector2(10, 120);
                       // spritebatch.Draw(kneeHip, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 50, 50), Color.White);
                    }
                    ///////////////////////////////////////////////////////////
                    if (joint.JointType == JointType.HipLeft)
                    {
                        print = "HipLeft: ";
                        writeVector = new Vector2(10, 130);
                       // spritebatch.Draw(leftHip, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 50, 50), Color.White);
                    }
                    ////////////////////////////////////////////////////////////////
                    if (joint.JointType == JointType.HipRight)
                    {
                        print = "HipRight: ";
                        writeVector = new Vector2(10, 140);
                       // spritebatch.Draw(rightHip, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 50, 50), Color.White);
                    }
                    /////////////////////////////////////////////////////////
                    if (joint.JointType == JointType.KneeLeft)
                    {
                        print = "KneeLeft: ";
                        writeVector = new Vector2(10, 150);
                       // spritebatch.Draw(kneeHip, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 50, 50), Color.White);
                    }
                    /////////////////////////////
                    if (joint.JointType == JointType.KneeRight)
                    {
                        print = "KneeRight: ";
                        writeVector = new Vector2(10, 160);
                    }
                    /////////////////////////////////////////////////////
                    if (joint.JointType == JointType.AnkleLeft)
                    {
                        print = "AnkleLeft: ";
                        writeVector = new Vector2(10, 170);
                    }
                    //////////////////////////////////////////////////////
                    if (joint.JointType == JointType.AnkleRight)
                    {
                        print = "AnkleRight: ";
                        writeVector = new Vector2(10, 180);
                    }
                    ///////////////////////////////////////
                    if (joint.JointType == JointType.FootLeft)
                    {
                        print = "FootLeft: ";
                        writeVector = new Vector2(10, 190);
                        
                    }
                    if (joint.JointType == JointType.FootRight)
                    {
                        print = "FootRight: ";
                        writeVector = new Vector2(10, 200);
                    }
                     * */
                    // spritebatch.DrawString(spritefont, print +"Position X: " + Convert.ToDouble(joint.Position.X) + "Position Y: " + Convert.ToDouble(joint.Position.Y) + "Position Z: " + Convert.ToDouble(joint.Position.Z), writeVector, Color.Green);


                }
                
            }


        }


    }
}
