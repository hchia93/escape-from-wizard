using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TileEngine;
using System;
using EscapeFromWizard.Map;
using EscapeFromWizard.Source.GameObject.Dynamic;
using EscapeFromWizard.Source.GameObject.Static;
using EscapeFromWizard.Source.Audio;

namespace EscapeFromWizard
{
    public enum CurrentScreen
    {
        HOME_SCREEN, 
        GAME_SCREEN, 
        GAME_OVER_SCREEN, 
        YOU_WON_SCREEN
    }

    public struct Input
    {
        public int VerticalDown;
        public int VerticalUp;
        public int HorizontalLeft;
        public int HorizontalRight;
    };

    public class EscapeFromWizard : Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch mapSpriteBatch;

        //Game Setting
        public const int pixelHeightPerTile = 32;
        public const int pixelWidthPerTile = 32;
        public const int squaresAcross = 18; //How many rows on screen
        public const int squaresDown = 18; //How many coloum on screen
        public int tileRow = 0; //Iterator that draw Grid and Label on Screen
        public int tileCol = 0; //Iterator that draw Grid and Label on Screen
        public const int numOfKey = 4;
        public const int numOfLock = 4;

        // Debug Mode
        public bool showGrid = false;
        public bool showTileRowCol = false;
        public bool isGodMode = false;

        //GameFonts & Resources & Texture & Custom Class
        public SpriteFont font_ArialBlack_14;
        public Texture2D texture1px;
        public Texture2D background;
        public Texture2D homeScreenBackground;
        public Texture2D gameOverScreenBackground;
        public Texture2D youWonScreenBackground;
        public Texture2D questNotCompletedErrorNotice;
        public Tile tileTileSet;
        public Tile objectTileSet;
        public Tile hudTileSet;
        public Tile otherTileSet;

        //Sound State
        public bool playGameBGMOnlyOnce = true;
        public bool playGameOverOnlyOnce = true;
        public bool playButtonOnlyOnce = true;
        public bool playPickUpOnlyOnce = true;
        public bool[] playUnlockDoorOnlyOnce = new bool[] { true, true, true, true };
        public bool playerPickUpSomething = false;
        public double footStepTimer = 0.0f;

        //Inputs
        public KeyboardState inputKey;
        public KeyboardState previousInputKey;
        public MouseState mouseState;
        public Input inputStruct;

        //GameMap & GameView & Instances
        public Camera2D cameraView;
        public MapData m_MapData; 
        public QuestionableEnum enumMapData;

        //Game Objects
        public Player player;
        public Wizard wizard;
        public Minion[] minions;
        public StaticObjectHandler staticObjectHandler;
        public Key[] key;
        public Lock[] doorLock;
        public SpellItem[] spellItem;

        //Other buffer values
        public Vector2 screenCenter;
        public int demoMapTileWidth; //The Dimension W of the loaded map
        public int demoMapTileHeight; //The Dimension H of the loaded map

        //
        public Vector2 movementOffset;

        //Game State
        public CurrentScreen currentScreen;
        public bool gameIsOver;
        public bool showErrorMsg = false;
        public float totalGameTime;
        public float secondsPlaying;
        public int minutesPlaying;
        public int score;

        //Minion Hit
        private const int minionHitDamage = 1;
        public bool decreaseHPOnlyOnce;
        public double hitDetectionTimer = 5.0f;


        public SoundManager soundManager;

        public EscapeFromWizard()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = pixelWidthPerTile * squaresAcross;
            graphics.PreferredBackBufferHeight = pixelHeightPerTile * squaresDown;
            screenCenter = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            currentScreen = CurrentScreen.HOME_SCREEN;
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.IsMouseVisible = true;

            //Map Related
            enumMapData = new QuestionableEnum();
            m_MapData = new MapData();
            m_MapData.InitializeMapData();
            demoMapTileWidth = m_MapData.GetMapTileWidth();
            demoMapTileHeight = m_MapData.GetMapTileHeight();

            //Camera
            cameraView = new Camera2D();
            cameraView.SetBoundary(0, 0, ((demoMapTileWidth - squaresAcross) * pixelWidthPerTile), ((demoMapTileHeight - squaresDown) * pixelHeightPerTile));

            //Key And Locks
            staticObjectHandler = new StaticObjectHandler(m_MapData);
            key = staticObjectHandler.GetKeys();
            doorLock = staticObjectHandler.GetLocks();
            spellItem = staticObjectHandler.GetSpellItems();

            //Player
            player = new Player();
            player.SetMapReference(m_MapData);
            player.SetPosition(1, 1);
            player.SetUpLockInformation(doorLock);

            //Wizard
            wizard = new Wizard();
            wizard.SetMapReference(m_MapData);
            wizard.SetTargetPosition(12, 4);

            //Minions array
            minions = new Minion[5];
            int[][] minionsInitialPatrolData = new int[5][] {
                new int[] {4,1,5,6},
                new int[] {4,15,5,6},
                new int[] {16,1,10,10},
                new int[] {20,15,4,5},
                new int[] {11,8,7,7}
            };
            for (int i = 0; i < minions.Length; i++)
            {
                minions[i] = new Minion();
                minions[i].SetMinionId(i);
                minions[i].SetMapReference(m_MapData);
                minions[i].SetPatrolStartPos(minionsInitialPatrolData[i]);
                minions[i].SetTargetPosition(minionsInitialPatrolData[i][0], minionsInitialPatrolData[i][1]);
            }

            gameIsOver = false;
            minutesPlaying = 0;
            secondsPlaying = 0.0f;

            //Sound
            soundManager = new SoundManager(this.Content);
            playGameBGMOnlyOnce = true;
            playGameOverOnlyOnce = true;
            playButtonOnlyOnce = true;
            playPickUpOnlyOnce = true;
            playUnlockDoorOnlyOnce = new bool[] { true, true, true, true };
            playerPickUpSomething = false;
            footStepTimer = 0.0f;
        }

        protected override void LoadContent()
        {
            //---------------------------------------------------------------------
            // Map 
            // --------------------------------------------------------------------
            mapSpriteBatch = new SpriteBatch(GraphicsDevice);

            tileTileSet = new Tile();
            tileTileSet.TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelTiles\32Pixel_SpriteSheet_Tiles");

            objectTileSet = new Tile();
            objectTileSet.TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelObjects\32Pixel_SpriteSheet_Object2");

            hudTileSet = new Tile();
            hudTileSet.TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelHUD\32Pixel_SpriteSheet_HUD");

            otherTileSet = new Tile();

            //---------------------------------------------------------------------
            // Texture
            // --------------------------------------------------------------------

            texture1px = new Texture2D(graphics.GraphicsDevice, 1, 1);
            texture1px.SetData(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

            background = new Texture2D(graphics.GraphicsDevice, 256, 256);
            background = Content.Load<Texture2D>(@"Resource\Image\floor_256px");

            homeScreenBackground = new Texture2D(graphics.GraphicsDevice, 576, 576);
            homeScreenBackground = Content.Load<Texture2D>(@"Resource\Image\home_fit");

            gameOverScreenBackground = new Texture2D(graphics.GraphicsDevice, 576, 576);
            gameOverScreenBackground = Content.Load<Texture2D>(@"Resource\Image\gameover_fit");

            youWonScreenBackground = new Texture2D(graphics.GraphicsDevice, 576, 576);
            youWonScreenBackground = Content.Load<Texture2D>(@"Resource\Image\youwon_fit");

            questNotCompletedErrorNotice = new Texture2D(graphics.GraphicsDevice, 576, 576);
            questNotCompletedErrorNotice = Content.Load<Texture2D>(@"Resource\Image\questnotcompleted");


            //---------------------------------------------------------------------
            // Font Loading
            //---------------------------------------------------------------------
            font_ArialBlack_14 = Content.Load<SpriteFont>(@"Resource\Font\Arial_Black_14pxl");
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            // Exit();

            inputKey = Keyboard.GetState();
            mouseState = Mouse.GetState();

            if (currentScreen == CurrentScreen.GAME_SCREEN)
            {

                //----------------------------------------------------------------------
                // Timer
                //----------------------------------------------------------------------
                footStepTimer += gameTime.ElapsedGameTime.TotalSeconds;
                hitDetectionTimer += gameTime.ElapsedGameTime.TotalSeconds;

                //calculate total game time elapsed
                secondsPlaying += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (secondsPlaying >= 60)
                {
                    minutesPlaying++;
                    secondsPlaying = 0;
                }

                //----------------------------------------------------------------------
                // Inputs Updates
                //----------------------------------------------------------------------

                //No longer listen user input if game is over
                if (!gameIsOver)
                {
                    inputStruct.VerticalDown = (inputKey.IsKeyDown(Keys.S) || inputKey.IsKeyDown(Keys.Down)) ? -1 : 0;
                    inputStruct.VerticalUp = (inputKey.IsKeyDown(Keys.W) || inputKey.IsKeyDown(Keys.Up)) ? 1 : 0;
                    inputStruct.HorizontalLeft = (inputKey.IsKeyDown(Keys.A) || inputKey.IsKeyDown(Keys.Left)) ? -1 : 0;
                    inputStruct.HorizontalRight = (inputKey.IsKeyDown(Keys.D) || inputKey.IsKeyDown(Keys.Right)) ? 1 : 0;
                }

                int finalVertical = inputStruct.VerticalDown + inputStruct.VerticalUp;
                int finalHorizontal = inputStruct.HorizontalLeft + inputStruct.HorizontalRight;


                if (inputKey.IsKeyDown(Keys.F1))
                {
                    DebugToggleGuideLine();
                }

                if (inputKey.IsKeyDown(Keys.F2))
                {
                    DebugToggleGodMode();
                }

                if (inputKey.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }

                if (showErrorMsg && (inputKey.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed))
                {
                    showErrorMsg = false;
                }
                   

                //----------------------------------------------------------------------
                // Movement
                //----------------------------------------------------------------------

                movementOffset.X = finalHorizontal;
                movementOffset.Y = -finalVertical;
                player.ProcessMovement(movementOffset);
                player.UpdateMovement(gameTime);


                wizard.SetPlayerExitFlag(player.IsOnExit());
                wizard.SetPlayerHideFlag(player.IsHiding());
                wizard.UpdateMovement(gameTime, player.GetPosition());


                for (int i = 0; i < minions.Length; i++)
                {
                    minions[i].SetPlayerExitFlag(player.IsOnExit());
                    minions[i].SetPlayerHideFlag(player.IsOnExit());
                    minions[i].UpdateMovement(gameTime, player.GetPosition());
                }


                staticObjectHandler.CheckLooted(gameTime, player);

                cameraView.UpdateMovement(player.GetMovingDirection(), player.GetPosition(), screenCenter);
                //----------------------------------------------------------------------
                // Flag Updates
                //----------------------------------------------------------------------

                if (playGameBGMOnlyOnce)
                {
                    soundManager.PlayBGM();
                    playGameBGMOnlyOnce = false;
                }

                //Delay Playing Interval, 0.2 secons
                if (!player.IsStanding() && footStepTimer > 0.2f)
                {
                    soundManager.PlayFootstepSound();
                    footStepTimer = 0.0f;
                    playButtonOnlyOnce = true;
                }

                for (int i = 0; i < doorLock.Length; i++)
                {
                    if (doorLock[i].IsDestroyed() && playUnlockDoorOnlyOnce[i])
                    {
                        soundManager.PlayUnlockDoorSound();
                        playUnlockDoorOnlyOnce[i] = false;
                    }
                }

                //Play Just Step on Hiding Tile
                if (player.IsHiding() == true && playButtonOnlyOnce)
                {
                    soundManager.PlayHidingSound();
                    playButtonOnlyOnce = false;
                }

                if (player.IsLootedSomething())
                {
                    soundManager.PlayPickUpSound();
                    player.SetPickUpFlag(false);
                }

                if (player.IsHPIncreased())
                {
                    soundManager.PlayRecoverHPSound();
                }

                //If Player On Exit
                if (player.IsOnExit() == true)
                {
                    _GameFinished(gameTime);
                }

                //If Player hit Minions
                if (hitDetectionTimer > 1.0f)
                {
                    if (!isGodMode)
                    {
                        for (int i = 0; i < minions.Length; i++)
                        {
                            if (minions[i].GetPlayerWasHitFlag() == true)
                            {
                                player.TakeDamage(minionHitDamage);
                                soundManager.PlayHitByMinionSound();
                                minions[i].SetPlayerWasHitFlag(false);
                            }
                        }

                        if (wizard.GetPlayerWasHitFlag() == true)
                        {
                            player.TakeDamage(3);
                            soundManager.PlayHitByWizardSound();
                            wizard.SetPlayerWasHitFlag(false);
                        }
                    }

                    hitDetectionTimer = 0.0f;
                }

                if (player.GetHP() <= 0)
                {
                    _GameOver(gameTime);
                }

                base.Update(gameTime);
            }
            else if (currentScreen == CurrentScreen.HOME_SCREEN)
            {
                if ((inputKey.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed) && !previousInputKey.IsKeyDown(Keys.Enter))
                    currentScreen = CurrentScreen.GAME_SCREEN;
            }
            else if (currentScreen == CurrentScreen.GAME_OVER_SCREEN)
            {
                if (inputKey.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed)
                {
                    soundManager.StopGameOverSound();
                    Initialize();
                    currentScreen = CurrentScreen.GAME_SCREEN;
                }
            }
            else if (currentScreen == CurrentScreen.YOU_WON_SCREEN)
            {
                if (inputKey.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed)
                {
                    Initialize();
                    currentScreen = CurrentScreen.HOME_SCREEN;
                }
            }

            previousInputKey = inputKey;
        }

        protected override void Draw(GameTime gameTime)
        {
            if (currentScreen == CurrentScreen.GAME_SCREEN)
            {
                GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
                _DrawBackground();
                _DrawMap();
                _DrawSpellItem();
                _DrawKey();
                _DrawLock();
                _DrawPlayer();
                _DrawWizard();
                _DrawMinions();
                _DrawHUD();
                _DrawGameTime();

                if (showGrid)
                    _ShowGrid();

                if (showTileRowCol)
                    _ShowRowAndColumn();

                if (showErrorMsg)
                    _DrawQuestNotCompletedError();

                base.Draw(gameTime);
            }
            else if (currentScreen == CurrentScreen.HOME_SCREEN)
                _DrawScreenBackground(homeScreenBackground);
            else if (currentScreen == CurrentScreen.GAME_OVER_SCREEN)
                _DrawScreenBackground(gameOverScreenBackground);
            else if (currentScreen == CurrentScreen.YOU_WON_SCREEN)
            {
                _DrawScreenBackground(youWonScreenBackground);
                _DrawGameScore();
            }
        }

        private void _GameFinished(GameTime gameTime)
        {
            if (player.GetCurrentNumOfQuestItem() >= player.GetMaxNumOfQuestItem())
            {
                _CalculateScore();
                currentScreen = CurrentScreen.YOU_WON_SCREEN;
                soundManager.StopBGM();
                soundManager.PlayWinningSound();
            }
            else
            {
                showErrorMsg = true;
                player.SetOnExit(false);
                player.ResetPositionIfQuestUncompleted();
            }

        }

        private void _GameOver(GameTime gameTime)
        {
            currentScreen = CurrentScreen.GAME_OVER_SCREEN;
            gameIsOver = true;

            for (int i = 0; i < minions.Length; i++)
                minions[i].SetBehavior(EBehaviorState.STOP);

            if (playGameOverOnlyOnce == true)
            {
                soundManager.PlayGameOverSound();
                playGameOverOnlyOnce = false;
            }
        }

        private void _CalculateScore()
        {
            score = 300 / ((minutesPlaying * 60) + (int)secondsPlaying) * 100 + player.GetCurrentNumOfStar() * 5;
        }

        private void _DrawQuestNotCompletedError()
        {
            mapSpriteBatch.Begin();
            mapSpriteBatch.Draw(questNotCompletedErrorNotice, new Vector2(50, screenCenter.Y), Microsoft.Xna.Framework.Color.White);
            mapSpriteBatch.End();
        }

        private void _DrawScreenBackground(Texture2D screenBackground)
        {
            mapSpriteBatch.Begin();
            mapSpriteBatch.Draw(screenBackground, new Vector2(0, 0), Microsoft.Xna.Framework.Color.White);
            mapSpriteBatch.End();
        }

        private void _DrawGameScore()
        {
            if (currentScreen != CurrentScreen.YOU_WON_SCREEN)
                return;

            string gameScoreStr = "Your score is " + score.ToString();
            mapSpriteBatch.Begin();
            mapSpriteBatch.DrawString(font_ArialBlack_14, gameScoreStr,
                new Vector2(screenCenter.X - font_ArialBlack_14.MeasureString(gameScoreStr).Length() / 2, screenCenter.Y - 50), Microsoft.Xna.Framework.Color.Black);
            mapSpriteBatch.End();
        }

        private void _DrawMap()
        {
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                     BlendState.AlphaBlend,
                                     SamplerState.LinearClamp,
                                     DepthStencilState.None,
                                     RasterizerState.CullCounterClockwise,
                                     null,
                                     cameraView.TransformMatrix());
            for (int row = 0; row < demoMapTileHeight; ++row)
                for (int col = 0; col < demoMapTileWidth; ++col)
                {
                    int tileID = m_MapData.levelData[row * demoMapTileWidth + col];
                    mapSpriteBatch.Draw(tileTileSet.TileSetTexture,
                                        new Rectangle((col * pixelWidthPerTile), (row * pixelHeightPerTile),
                                        pixelWidthPerTile, pixelHeightPerTile),
                                        tileTileSet.GetSourceRectangle(tileID),
                                        Microsoft.Xna.Framework.Color.White);
                }
            mapSpriteBatch.End();
        }

        private void _DrawBackground()
        {
            int x = 0, y = 32;
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                     BlendState.AlphaBlend,
                                     SamplerState.LinearClamp,
                                     DepthStencilState.None,
                                     RasterizerState.CullCounterClockwise,
                                     null,
                                     cameraView.TransformMatrix());
            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 3; ++col)
                {
                    mapSpriteBatch.Draw(background, new Vector2(x, y), Microsoft.Xna.Framework.Color.White);
                    x += 8 * pixelWidthPerTile;
                }
                y += 8 * pixelHeightPerTile;
                x = 0;
            }
            mapSpriteBatch.End();
        }

        private void _DrawPlayer()
        {
            int screenOffsetX = (int)player.GetPosition().X * pixelWidthPerTile;
            int screenOffsetY = (int)player.GetPosition().Y * pixelHeightPerTile;
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                   BlendState.AlphaBlend,
                                   SamplerState.LinearClamp,
                                   DepthStencilState.None,
                                   RasterizerState.CullCounterClockwise,
                                   null,
                                   cameraView.TransformMatrix());

            mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                new Rectangle(screenOffsetX, screenOffsetY, pixelWidthPerTile, pixelHeightPerTile),
                                objectTileSet.GetSourceRectangle(4),
                                Microsoft.Xna.Framework.Color.White);

            mapSpriteBatch.End();

        }

        private void _DrawWizard()
        {
            int screenOffsetX = (int)wizard.GetTargetPosition().X * pixelWidthPerTile;
            int screenOffsetY = (int)wizard.GetTargetPosition().Y * pixelHeightPerTile;
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                   BlendState.AlphaBlend,
                                   SamplerState.LinearClamp,
                                   DepthStencilState.None,
                                   RasterizerState.CullCounterClockwise,
                                   null,
                                   cameraView.TransformMatrix());

            mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                new Rectangle(screenOffsetX, screenOffsetY, pixelWidthPerTile, pixelHeightPerTile),
                                objectTileSet.GetSourceRectangle(14),
                                Microsoft.Xna.Framework.Color.White);
            mapSpriteBatch.End();

        }


        private void _DrawMinions()
        {
            for (int i = 0; i < minions.Length; i++)
            {
                int screenOffsetX = (int)minions[i].GetTargetPosition().X * pixelWidthPerTile;
                int screenOffsetY = (int)minions[i].GetTargetPosition().Y * pixelHeightPerTile;
                mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                       BlendState.AlphaBlend,
                                       SamplerState.LinearClamp,
                                       DepthStencilState.None,
                                       RasterizerState.CullCounterClockwise,
                                       null,
                                       cameraView.TransformMatrix());

                mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                    new Rectangle(screenOffsetX, screenOffsetY, pixelWidthPerTile, pixelHeightPerTile),
                                    objectTileSet.GetSourceRectangle(19),
                                    Microsoft.Xna.Framework.Color.White);
                mapSpriteBatch.End();
            }

        }

        private void _DrawSpellItem()
        {
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                   BlendState.AlphaBlend,
                                   SamplerState.LinearClamp,
                                   DepthStencilState.None,
                                   RasterizerState.CullCounterClockwise,
                                   null,
                                   cameraView.TransformMatrix());

            for (int i = 0; i < spellItem.Length; i++)
            {
                if (!spellItem[i].isLooted())
                {
                    mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                        new Rectangle((int)(spellItem[i].GetItemTilePositionX() * pixelWidthPerTile),
                                        (int)(spellItem[i].GetItemTilePositionY() * pixelHeightPerTile),
                                        pixelWidthPerTile, pixelHeightPerTile),
                                        objectTileSet.GetSourceRectangle(spellItem[i].GetItemTypeIndex()),
                                        Microsoft.Xna.Framework.Color.White);
                }
            }

            mapSpriteBatch.End();
        }

        private void _DrawKey()
        {
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                   BlendState.AlphaBlend,
                                   SamplerState.LinearClamp,
                                   DepthStencilState.None,
                                   RasterizerState.CullCounterClockwise,
                                   null,
                                   cameraView.TransformMatrix());

            for (int i = 0; i < key.Length; i++)
            {
                if (!key[i].IsLooted())
                {
                    mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                        new Rectangle((int)(key[i].GetPosition().X * pixelWidthPerTile),
                                        (int)(key[i].GetPosition().Y * pixelHeightPerTile),
                                        pixelWidthPerTile, pixelHeightPerTile),
                                        objectTileSet.GetSourceRectangle((int) key[i].GetColor()),
                                        Microsoft.Xna.Framework.Color.White);
                }
            }

            mapSpriteBatch.End();
        }

        private void _DrawLock()
        {
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                   BlendState.AlphaBlend,
                                   SamplerState.LinearClamp,
                                   DepthStencilState.None,
                                   RasterizerState.CullCounterClockwise,
                                   null,
                                   cameraView.TransformMatrix());
            for (int i = 0; i < doorLock.Length; i++)
            {
                if (!doorLock[i].IsDestroyed())
                    mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                   new Rectangle((int)(doorLock[i].GetPosition().X * pixelWidthPerTile),
                                   (int)(doorLock[i].GetPosition().Y * pixelHeightPerTile),
                                   pixelWidthPerTile, pixelHeightPerTile),
                                   objectTileSet.GetSourceRectangle((int)(doorLock[i].GetColor()) + 15),
                                   Microsoft.Xna.Framework.Color.White);
            }

            mapSpriteBatch.End();
        }

        private void _DrawHUD()
        {

            mapSpriteBatch.Begin();
            int count = 4;
            for (int i = 0; i < 4; i++)
            {
                if (key[i].IsLooted())
                    mapSpriteBatch.Draw(hudTileSet.TileSetTexture,
                                        new Rectangle((int)((squaresAcross - count) * pixelWidthPerTile),
                                        (int)((squaresDown - 1) * pixelHeightPerTile),
                                        pixelWidthPerTile, pixelHeightPerTile),
                                        hudTileSet.GetSourceRectangle(enumMapData.GetKeyIndex(i)),
                                        Microsoft.Xna.Framework.Color.White);
                else
                    mapSpriteBatch.Draw(hudTileSet.TileSetTexture,
                                    new Rectangle((int)((squaresAcross - count) * pixelWidthPerTile),
                                    (int)((squaresDown - 1) * pixelHeightPerTile),
                                    pixelWidthPerTile, pixelHeightPerTile),
                                    hudTileSet.GetSourceRectangle(enumMapData.GetKeyIndex(i + 4)),
                                    Microsoft.Xna.Framework.Color.White);
                count--;
            }

            // Draw Player's HP
            for (int i = 0; i < player.GetMaxHP(); i++)
            {

                if (i < player.GetHP())
                    mapSpriteBatch.Draw(hudTileSet.TileSetTexture,
                                                new Rectangle((int)(i * pixelWidthPerTile),
                                                (int)((squaresDown - 1) * pixelHeightPerTile),
                                                pixelWidthPerTile, pixelHeightPerTile),
                                                hudTileSet.GetSourceRectangle((int)HUDIcon.FULL_HEART),
                                                Microsoft.Xna.Framework.Color.White);
                else
                    mapSpriteBatch.Draw(hudTileSet.TileSetTexture,
                                                new Rectangle((int)(i * pixelWidthPerTile),
                                                (int)((squaresDown - 1) * pixelHeightPerTile),
                                                pixelWidthPerTile, pixelHeightPerTile),
                                                hudTileSet.GetSourceRectangle((int)HUDIcon.EMPTY_HEART),
                                                Microsoft.Xna.Framework.Color.White);
            }

            //Draw Player's Quest Item
            for (int i = 0; i < player.GetMaxNumOfQuestItem(); i++)
            {
                if (i < player.GetCurrentNumOfQuestItem())
                    mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                                new Rectangle((int)((squaresAcross - (player.GetMaxNumOfQuestItem() - i)) * pixelWidthPerTile),
                                                (int)((squaresDown - 2) * pixelHeightPerTile),
                                                pixelWidthPerTile, pixelHeightPerTile),
                                                objectTileSet.GetSourceRectangle((int)SpellItems.QUEST_POTION),
                                                Microsoft.Xna.Framework.Color.White);
                else
                    mapSpriteBatch.Draw(objectTileSet.TileSetTexture,
                                                new Rectangle((int)((squaresAcross - (player.GetMaxNumOfQuestItem() - i)) * pixelWidthPerTile),
                                                (int)((squaresDown - 2) * pixelHeightPerTile),
                                                pixelWidthPerTile, pixelHeightPerTile),
                                                objectTileSet.GetSourceRectangle((int)SpellItems.UNLOOTED_QUEST_ITEM),
                                                Microsoft.Xna.Framework.Color.White);
            }

            mapSpriteBatch.End();
        }

        private void _DrawGameTime()
        {
            String gameTimeStr = minutesPlaying.ToString("00") + ":" + secondsPlaying.ToString("00");
            mapSpriteBatch.Begin();
            mapSpriteBatch.DrawString(font_ArialBlack_14, gameTimeStr,
                new Vector2((screenCenter.X) - font_ArialBlack_14.MeasureString(gameTimeStr).Length() / 2, 0), Microsoft.Xna.Framework.Color.Black);
            mapSpriteBatch.End();
        }

        private void _ShowGrid()
        {
            for (int i = 0; i < doorLock.Length; i++)
            {
                doorLock[i].SetDestroyed(true);
                doorLock[i].SetUnlocked();
            }
            mapSpriteBatch.Begin(SpriteSortMode.Immediate,
                                    BlendState.AlphaBlend,
                                    SamplerState.LinearClamp,
                                    DepthStencilState.None,
                                    RasterizerState.CullCounterClockwise,
                                    null,
                                    cameraView.TransformMatrix());


            for (tileCol = 0; tileCol < demoMapTileWidth; tileCol++)
            {
                Rectangle horizontal_thin_rec = new Rectangle((int)(0 + (tileCol * pixelWidthPerTile)), 0, 1, (pixelHeightPerTile * demoMapTileWidth));
                mapSpriteBatch.Draw(texture1px, horizontal_thin_rec, Microsoft.Xna.Framework.Color.DarkGray);
            }

            for (tileRow = 0; tileRow < demoMapTileHeight; tileRow++)
            {
                Rectangle vertical_thin_rec = new Rectangle(0, (int)(0 + (tileRow * pixelHeightPerTile)), (pixelWidthPerTile * demoMapTileHeight), 1);
                mapSpriteBatch.Draw(texture1px, vertical_thin_rec, Microsoft.Xna.Framework.Color.DarkGray);
            }
            mapSpriteBatch.End();
        }

        private void _ShowRowAndColumn()
        {
            mapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, cameraView.TransformMatrix());

            string _mapTileRowColInfoString;
            Vector2 _spriteFontOriginPos = new Vector2(0, 0);
            for (tileRow = 0; tileRow < demoMapTileHeight; tileRow++)
                for (tileCol = 0; tileCol < demoMapTileWidth; tileCol++)
                {
                    _mapTileRowColInfoString = "(" + tileRow + "." + tileCol + ")";
                    _spriteFontOriginPos = font_ArialBlack_14.MeasureString(_mapTileRowColInfoString) / 2;
                    mapSpriteBatch.DrawString(font_ArialBlack_14, _mapTileRowColInfoString, new Vector2(tileRow * pixelHeightPerTile + 16, tileCol * pixelWidthPerTile + 16), Microsoft.Xna.Framework.Color.Blue, 0, _spriteFontOriginPos, 1.0f, SpriteEffects.None, 0.5f);
                }


            mapSpriteBatch.End();
        }

        private void DebugToggleGuideLine()
        {
            showGrid = !showGrid;
            showTileRowCol = !showTileRowCol;
        }

        private void DebugToggleGodMode()
        {
            isGodMode = !isGodMode;
        }
    }
}
