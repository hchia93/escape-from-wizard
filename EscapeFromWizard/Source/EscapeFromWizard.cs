using EscapeFromWizard.Map;
using EscapeFromWizard.Source.Audio;
using EscapeFromWizard.Source.GameObject.Dynamic;
using EscapeFromWizard.Source.GameObject.Static;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TileEngine;

namespace EscapeFromWizard
{
    public enum ScreenType
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
        public GraphicsDeviceManager m_Graphics;
        public SpriteBatch m_MapSpriteBatch;

        //Game Setting
        public const int pixelHeightPerTile = 32;
        public const int pixelWidthPerTile = 32;
        public const int squaresAcross = 18; //How many rows on screen
        public const int squaresDown = 18; //How many coloum on screen
        public int m_TileRow = 0; //Iterator that draw Grid and Label on Screen
        public int m_TileCol = 0; //Iterator that draw Grid and Label on Screen
        public const int numOfKey = 4;
        public const int numOfLock = 4;

        // Debug Mode
        public bool m_ShowGrid = false;
        public bool m_ShowTileRowCol = false;
        public bool m_IsGodMode = false;

        //GameFonts & Resources & Texture & Custom Class
        public SpriteFont m_FontArialBlack14;
        public Texture2D m_Texture1px;
        public Texture2D m_Background;
        public Texture2D m_HomeScreenBackground;
        public Texture2D m_GameOverScreenBackground;
        public Texture2D m_YouWonScreenBackground;
        public Texture2D m_QuestNotCompletedErrorNotice;
        public Tile m_TileTileSet;
        public Tile m_ObjectTileSet;
        public Tile m_HudTileSet;
        public Tile m_OtherTileSet;

        //Sound State
        public bool m_PlayGameBGMOnlyOnce = true;
        public bool m_PlayGameOverOnlyOnce = true;
        public bool m_PlayButtonOnlyOnce = true;
        public bool m_PlayPickUpOnlyOnce = true;
        public bool[] m_PlayUnlockDoorOnlyOnce = new bool[] { true, true, true, true };
        public bool m_PlayerPickUpSomething = false;
        public double m_FootStepTimer = 0.0f;

        //Inputs
        public KeyboardState m_InputKey;
        public KeyboardState m_PreviousInputKey;
        public MouseState m_MouseState;
        public Input m_InputStruct;

        //GameMap & GameView & Instances
        public Camera2D m_CameraView;
        public Level m_MapData;
        public QuestionableEnum m_EnumMapData;

        //Game Objects
        public Player m_Player;
        public Wizard m_Wizard;
        public Minion[] m_Minions;
        public StaticObjectHandler m_StaticObjectHandler;
        public Key[] m_Key;
        public Lock[] m_DoorLock;
        public SpellItem[] m_SpellItem;

        //Other buffer values
        public Vector2 m_ScreenCenter;
        public int m_DemoMapTileWidth; //The Dimension W of the loaded map
        public int m_DemoMapTileHeight; //The Dimension H of the loaded map

        public Vector2 m_MovementOffset;

        //Game State
        public ScreenType m_CurrentScreen;
        public bool m_GameIsOver;
        public bool m_ShowErrorMsg = false;
        public float m_TotalGameTime;
        public float m_SecondsPlaying;
        public int m_MinutesPlaying;
        public int m_Score;

        //Minion Hit
        private const int minionHitDamage = 1;
        public bool m_DecreaseHPOnlyOnce;
        public double m_HitDetectionTimer = 5.0f;

        public SoundManager m_SoundManager;

        public EscapeFromWizard()
        {
            m_Graphics = new GraphicsDeviceManager(this);
            m_Graphics.PreferredBackBufferWidth = pixelWidthPerTile * squaresAcross;
            m_Graphics.PreferredBackBufferHeight = pixelHeightPerTile * squaresDown;
            m_Graphics.IsFullScreen = false;
            m_Graphics.ApplyChanges();

            m_ScreenCenter = new Vector2(m_Graphics.PreferredBackBufferWidth / 2, m_Graphics.PreferredBackBufferHeight / 2);
         
            m_CurrentScreen = ScreenType.HOME_SCREEN;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.IsMouseVisible = true;

            //Map Related
            m_EnumMapData = new QuestionableEnum();
            m_MapData = new Level();
            m_MapData.Initialize();
            m_DemoMapTileWidth = m_MapData.GetMapTileWidth();
            m_DemoMapTileHeight = m_MapData.GetMapTileHeight();

            //Camera
            m_CameraView = new Camera2D();
            m_CameraView.SetBoundary(0, 0, ((m_DemoMapTileWidth - squaresAcross) * pixelWidthPerTile), ((m_DemoMapTileHeight - squaresDown) * pixelHeightPerTile));

            //Key And Locks
            m_StaticObjectHandler = new StaticObjectHandler(m_MapData);
            m_Key = m_StaticObjectHandler.GetKeys();
            m_DoorLock = m_StaticObjectHandler.GetLocks();
            m_SpellItem = m_StaticObjectHandler.GetSpellItems();

            //Player
            m_Player = new Player();
            m_Player.SetMapReference(m_MapData);
            m_Player.SetPosition(1, 1);
            m_Player.SetUpLockInformation(m_DoorLock);

            //Wizard
            m_Wizard = new Wizard();
            m_Wizard.SetMapReference(m_MapData);
            m_Wizard.SetTargetPosition(12, 4);

            //Minions array
            m_Minions = new Minion[5];
            int[][] minionsInitialPatrolData = new int[5][] {
                new int[] {4,1,5,6},
                new int[] {4,15,5,6},
                new int[] {16,1,10,10},
                new int[] {20,15,4,5},
                new int[] {11,8,7,7}
            };
            for (int i = 0; i < m_Minions.Length; i++)
            {
                m_Minions[i] = new Minion();
                m_Minions[i].SetMinionId(i);
                m_Minions[i].SetMapReference(m_MapData);
                m_Minions[i].SetPatrolStartPos(minionsInitialPatrolData[i]);
                m_Minions[i].SetTargetPosition(minionsInitialPatrolData[i][0], minionsInitialPatrolData[i][1]);
            }

            m_GameIsOver = false;
            m_MinutesPlaying = 0;
            m_SecondsPlaying = 0.0f;

            //Sound
            m_SoundManager = new SoundManager(this.Content);
            m_PlayGameBGMOnlyOnce = true;
            m_PlayGameOverOnlyOnce = true;
            m_PlayButtonOnlyOnce = true;
            m_PlayPickUpOnlyOnce = true;
            m_PlayUnlockDoorOnlyOnce = new bool[] { true, true, true, true };
            m_PlayerPickUpSomething = false;
            m_FootStepTimer = 0.0f;
        }

        protected override void LoadContent()
        {
            //---------------------------------------------------------------------
            // Map 
            // --------------------------------------------------------------------
            m_MapSpriteBatch = new SpriteBatch(GraphicsDevice);

            m_TileTileSet = new Tile();
            m_TileTileSet.TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelTiles\32Pixel_SpriteSheet_Tiles");

            m_ObjectTileSet = new Tile();
            m_ObjectTileSet.TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelObjects\32Pixel_SpriteSheet_Object2");

            m_HudTileSet = new Tile();
            m_HudTileSet.TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelHUD\32Pixel_SpriteSheet_HUD");

            m_OtherTileSet = new Tile();

            m_Texture1px = new Texture2D(GraphicsDevice, 1, 1);
            m_Texture1px.SetData(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

            m_Background = new Texture2D(GraphicsDevice, 256, 256);
            m_Background = Content.Load<Texture2D>(@"Resource\Image\floor_256px");

            m_HomeScreenBackground = new Texture2D(GraphicsDevice, 576, 576);
            m_HomeScreenBackground = Content.Load<Texture2D>(@"Resource\Image\home_fit");

            m_GameOverScreenBackground = new Texture2D(GraphicsDevice, 576, 576);
            m_GameOverScreenBackground = Content.Load<Texture2D>(@"Resource\Image\gameover_fit");

            m_YouWonScreenBackground = new Texture2D(GraphicsDevice, 576, 576);
            m_YouWonScreenBackground = Content.Load<Texture2D>(@"Resource\Image\youwon_fit");

            m_QuestNotCompletedErrorNotice = new Texture2D(GraphicsDevice, 576, 576);
            m_QuestNotCompletedErrorNotice = Content.Load<Texture2D>(@"Resource\Image\questnotcompleted");

            m_FontArialBlack14 = Content.Load<SpriteFont>(@"Resource\Font\Arial_Black_14pxl");
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            // Exit();

            m_InputKey = Keyboard.GetState();
            m_MouseState = Mouse.GetState();

            if (m_CurrentScreen == ScreenType.GAME_SCREEN)
            {

                //----------------------------------------------------------------------
                // Timer
                //----------------------------------------------------------------------
                m_FootStepTimer += gameTime.ElapsedGameTime.TotalSeconds;
                m_HitDetectionTimer += gameTime.ElapsedGameTime.TotalSeconds;

                //calculate total game time elapsed
                m_SecondsPlaying += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (m_SecondsPlaying >= 60)
                {
                    m_MinutesPlaying++;
                    m_SecondsPlaying = 0;
                }

                //----------------------------------------------------------------------
                // Inputs Updates
                //----------------------------------------------------------------------

                //No longer listen user input if game is over
                if (!m_GameIsOver)
                {
                    m_InputStruct.VerticalDown = (m_InputKey.IsKeyDown(Keys.S) || m_InputKey.IsKeyDown(Keys.Down)) ? -1 : 0;
                    m_InputStruct.VerticalUp = (m_InputKey.IsKeyDown(Keys.W) || m_InputKey.IsKeyDown(Keys.Up)) ? 1 : 0;
                    m_InputStruct.HorizontalLeft = (m_InputKey.IsKeyDown(Keys.A) || m_InputKey.IsKeyDown(Keys.Left)) ? -1 : 0;
                    m_InputStruct.HorizontalRight = (m_InputKey.IsKeyDown(Keys.D) || m_InputKey.IsKeyDown(Keys.Right)) ? 1 : 0;
                }

                int finalVertical = m_InputStruct.VerticalDown + m_InputStruct.VerticalUp;
                int finalHorizontal = m_InputStruct.HorizontalLeft + m_InputStruct.HorizontalRight;

                if (m_InputKey.IsKeyDown(Keys.F1))
                {
                    DebugToggleGuideLine();
                }

                if (m_InputKey.IsKeyDown(Keys.F2))
                {
                    DebugToggleGodMode();
                }

                if (m_InputKey.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }

                if (m_ShowErrorMsg && (m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed))
                {
                    m_ShowErrorMsg = false;
                }

                //----------------------------------------------------------------------
                // Movement
                //----------------------------------------------------------------------

                m_MovementOffset.X = finalHorizontal;
                m_MovementOffset.Y = -finalVertical;
                m_Player.ProcessMovement(m_MovementOffset);
                m_Player.UpdateMovement(gameTime);

                m_Wizard.SetPlayerExitFlag(m_Player.IsOnExit());
                m_Wizard.SetPlayerHideFlag(m_Player.IsHiding());
                m_Wizard.UpdateMovement(gameTime, m_Player.GetPosition());

                for (int i = 0; i < m_Minions.Length; i++)
                {
                    m_Minions[i].SetPlayerExitFlag(m_Player.IsOnExit());
                    m_Minions[i].SetPlayerHideFlag(m_Player.IsOnExit());
                    m_Minions[i].UpdateMovement(gameTime, m_Player.GetPosition());
                }

                m_StaticObjectHandler.CheckLooted(gameTime, m_Player);
                m_CameraView.UpdateMovement(m_Player.GetMovingDirection(), m_Player.GetPosition(), m_ScreenCenter);
               
                if (m_PlayGameBGMOnlyOnce)
                {
                    m_SoundManager.PlayBGM();
                    m_PlayGameBGMOnlyOnce = false;
                }

                //Delay Playing Interval, 0.2 secons
                if (!m_Player.IsStanding() && m_FootStepTimer > 0.2f)
                {
                    m_SoundManager.PlayFootstepSound();
                    m_FootStepTimer = 0.0f;
                    m_PlayButtonOnlyOnce = true;
                }

                for (int i = 0; i < m_DoorLock.Length; i++)
                {
                    if (m_DoorLock[i].IsDestroyed() && m_PlayUnlockDoorOnlyOnce[i])
                    {
                        m_SoundManager.PlayUnlockDoorSound();
                        m_PlayUnlockDoorOnlyOnce[i] = false;
                    }
                }

                if (m_Player.IsHiding() == true && m_PlayButtonOnlyOnce)
                {
                    m_SoundManager.PlayHidingSound();
                    m_PlayButtonOnlyOnce = false;
                }

                if (m_Player.IsLootedSomething())
                {
                    m_SoundManager.PlayPickUpSound();
                    m_Player.SetPickUpFlag(false);
                }

                if (m_Player.IsOnExit() == true)
                {
                    GameFinished(gameTime);
                }

                //If Player hit Minions
                if (m_HitDetectionTimer > 1.0f)
                {
                    if (!m_IsGodMode)
                    {
                        for (int i = 0; i < m_Minions.Length; i++)
                        {
                            if (m_Minions[i].GetPlayerWasHitFlag() == true)
                            {
                                m_Player.TakeDamage(minionHitDamage);
                                m_SoundManager.PlayHitByMinionSound();
                                m_Minions[i].SetPlayerWasHitFlag(false);
                            }
                        }

                        if (m_Wizard.GetPlayerWasHitFlag() == true)
                        {
                            m_Player.TakeDamage(3);
                            m_SoundManager.PlayHitByWizardSound();
                            m_Wizard.SetPlayerWasHitFlag(false);
                        }
                    }

                    m_HitDetectionTimer = 0.0f;
                }

                if (m_Player.GetHP() <= 0)
                {
                    GameOver(gameTime);
                }

                base.Update(gameTime);
            }
            else if (m_CurrentScreen == ScreenType.HOME_SCREEN)
            {
                if ((m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed) && !m_PreviousInputKey.IsKeyDown(Keys.Enter))
                {
                    m_CurrentScreen = ScreenType.GAME_SCREEN;
                }
            }
            else if (m_CurrentScreen == ScreenType.GAME_OVER_SCREEN)
            {
                if (m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed)
                {
                    m_SoundManager.StopGameOverSound();
                    Initialize();
                    m_CurrentScreen = ScreenType.GAME_SCREEN;
                }
            }
            else if (m_CurrentScreen == ScreenType.YOU_WON_SCREEN)
            {
                if (m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed)
                {
                    Initialize();
                    m_CurrentScreen = ScreenType.HOME_SCREEN;
                }
            }

            m_PreviousInputKey = m_InputKey;
        }

        protected override void Draw(GameTime gameTime)
        {
            if (m_CurrentScreen == ScreenType.GAME_SCREEN)
            {
                GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
                DrawBackground();
                DrawMap();
                DrawSpellItem();
                DrawKey();
                DrawLock();
                DrawPlayer();
                DrawWizard();
                DrawMinions();
                DrawHUD();
                DrawGameTime();

                if (m_ShowGrid)
                {
                    ShowLevelGrid();
                }

                if (m_ShowTileRowCol)
                {
                    ShowRowColumnIndex();
                }

                if (m_ShowErrorMsg)
                {
                    DrawQuestNotCompletedError();
                }

                base.Draw(gameTime);
            }
            else if (m_CurrentScreen == ScreenType.HOME_SCREEN)
            {
                DrawScreenBackground(m_HomeScreenBackground);
            }
            else if (m_CurrentScreen == ScreenType.GAME_OVER_SCREEN)
            {
                DrawScreenBackground(m_GameOverScreenBackground);
            }
            else if (m_CurrentScreen == ScreenType.YOU_WON_SCREEN)
            {
                DrawScreenBackground(m_YouWonScreenBackground);
                DrawGameScore();
            }
        }
        private void GameFinished(GameTime gameTime)
        {
            if (m_Player.GetCurrentNumOfQuestItem() >= m_Player.GetMaxNumOfQuestItem())
            {
                CalculateScore();
                m_CurrentScreen = ScreenType.YOU_WON_SCREEN;
                m_SoundManager.StopBGM();
                m_SoundManager.PlayWinningSound();
            }
            else
            {
                m_ShowErrorMsg = true;
                m_Player.SetOnExit(false);
                m_Player.RevertPositionOnIncompleteQuest();
            }

        }
        private void GameOver(GameTime gameTime)
        {
            m_CurrentScreen = ScreenType.GAME_OVER_SCREEN;
            m_GameIsOver = true;

            foreach (var minion in m_Minions)
            {
                minion.SetBehavior(EBehaviorState.STOP);
            }

            if (m_PlayGameOverOnlyOnce)
            {
                m_SoundManager.PlayGameOverSound();
                m_PlayGameOverOnlyOnce = false;
            }
        }
        private void CalculateScore()
        {
            m_Score = 300 / ((m_MinutesPlaying * 60) + (int)m_SecondsPlaying) * 100 + m_Player.GetCurrentNumOfStar() * 5;
        }

        private void DrawQuestNotCompletedError()
        {
            m_MapSpriteBatch.Begin();
            m_MapSpriteBatch.Draw(m_QuestNotCompletedErrorNotice, new Vector2(50, m_ScreenCenter.Y), Microsoft.Xna.Framework.Color.White);
            m_MapSpriteBatch.End();
        }
        private void DrawScreenBackground(Texture2D screenBackground)
        {
            m_MapSpriteBatch.Begin();
            m_MapSpriteBatch.Draw(screenBackground, new Vector2(0, 0), Microsoft.Xna.Framework.Color.White);
            m_MapSpriteBatch.End();
        }

        private void DrawGameScore()
        {
            if (m_CurrentScreen != ScreenType.YOU_WON_SCREEN)
            {
                return;
            }

            string gameScoreStr = "Your score is " + m_Score.ToString();
            m_MapSpriteBatch.Begin();
            m_MapSpriteBatch.DrawString(m_FontArialBlack14, gameScoreStr, new Vector2(m_ScreenCenter.X - m_FontArialBlack14.MeasureString(gameScoreStr).Length() / 2, m_ScreenCenter.Y - 50), Microsoft.Xna.Framework.Color.Black);
            m_MapSpriteBatch.End();
        }

        private void DrawMap()
        {
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());
            for (int row = 0; row < m_DemoMapTileHeight; ++row)
            {
                for (int col = 0; col < m_DemoMapTileWidth; ++col)
                {
                    int tileID = m_MapData.m_Data[row * m_DemoMapTileWidth + col];
                    m_MapSpriteBatch.Draw(m_TileTileSet.TileSetTexture, new Rectangle((col * pixelWidthPerTile), (row * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_TileTileSet.GetSourceRectangle(tileID), Microsoft.Xna.Framework.Color.White);
                }
            }
            m_MapSpriteBatch.End();
        }

        private void DrawBackground()
        {
            int x = 0, y = 32;
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());
            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 3; ++col)
                {
                    m_MapSpriteBatch.Draw(m_Background, new Vector2(x, y), Microsoft.Xna.Framework.Color.White);
                    x += 8 * pixelWidthPerTile;
                }
                y += 8 * pixelHeightPerTile;
                x = 0;
            }
            m_MapSpriteBatch.End();
        }

        private void DrawPlayer()
        {
            int screenOffsetX = (int)m_Player.GetPosition().X * pixelWidthPerTile;
            int screenOffsetY = (int)m_Player.GetPosition().Y * pixelHeightPerTile;
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());

            m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle(screenOffsetX, screenOffsetY, pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle(4), Microsoft.Xna.Framework.Color.White);

            m_MapSpriteBatch.End();

        }

        private void DrawWizard()
        {
            int screenOffsetX = (int)m_Wizard.GetTargetPosition().X * pixelWidthPerTile;
            int screenOffsetY = (int)m_Wizard.GetTargetPosition().Y * pixelHeightPerTile;
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());

            m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle(screenOffsetX, screenOffsetY, pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle(14), Microsoft.Xna.Framework.Color.White);
            m_MapSpriteBatch.End();
        }

        private void DrawMinions()
        {
            for (int i = 0; i < m_Minions.Length; i++)
            {
                int screenOffsetX = (int)m_Minions[i].GetTargetPosition().X * pixelWidthPerTile;
                int screenOffsetY = (int)m_Minions[i].GetTargetPosition().Y * pixelHeightPerTile;
                m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());

                m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle(screenOffsetX, screenOffsetY, pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle(19), Microsoft.Xna.Framework.Color.White);
                m_MapSpriteBatch.End();
            }
        }

        private void DrawSpellItem()
        {
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());

            for (int i = 0; i < m_SpellItem.Length; i++)
            {
                if (!m_SpellItem[i].isLooted())
                {
                    m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle((int)(m_SpellItem[i].GetItemTilePositionX() * pixelWidthPerTile), (int)(m_SpellItem[i].GetItemTilePositionY() * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle(m_SpellItem[i].GetItemTypeIndex()), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_MapSpriteBatch.End();
        }

        private void DrawKey()
        {
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());

            for (int i = 0; i < m_Key.Length; i++)
            {
                if (!m_Key[i].IsLooted())
                {
                    m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle((int)(m_Key[i].GetPosition().X * pixelWidthPerTile), (int)(m_Key[i].GetPosition().Y * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle((int)m_Key[i].GetColor()), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_MapSpriteBatch.End();
        }

        private void DrawLock()
        {
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());
            for (int i = 0; i < m_DoorLock.Length; i++)
            {
                if (!m_DoorLock[i].IsDestroyed())
                {
                    m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle((int)(m_DoorLock[i].GetPosition().X * pixelWidthPerTile), (int)(m_DoorLock[i].GetPosition().Y * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle((int)(m_DoorLock[i].GetColor()) + 15), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_MapSpriteBatch.End();
        }

        private void DrawHUD()
        {

            m_MapSpriteBatch.Begin();
            int count = 4;
            for (int i = 0; i < 4; i++)
            {
                if (m_Key[i].IsLooted())
                {
                    m_MapSpriteBatch.Draw(m_HudTileSet.TileSetTexture, new Rectangle((int)((squaresAcross - count) * pixelWidthPerTile), (int)((squaresDown - 1) * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_HudTileSet.GetSourceRectangle(m_EnumMapData.GetKeyIndex(i)), Microsoft.Xna.Framework.Color.White);
                }
                else
                {
                    m_MapSpriteBatch.Draw(m_HudTileSet.TileSetTexture, new Rectangle((int)((squaresAcross - count) * pixelWidthPerTile), (int)((squaresDown - 1) * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_HudTileSet.GetSourceRectangle(m_EnumMapData.GetKeyIndex(i + 4)), Microsoft.Xna.Framework.Color.White);
                }
                count--;
            }

            // Draw Player's HP
            for (int i = 0; i < m_Player.GetMaxHP(); i++)
            {

                if (i < m_Player.GetHP())
                {
                    m_MapSpriteBatch.Draw(m_HudTileSet.TileSetTexture, new Rectangle((int)(i * pixelWidthPerTile), (int)((squaresDown - 1) * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_HudTileSet.GetSourceRectangle((int)HUDIcon.FULL_HEART), Microsoft.Xna.Framework.Color.White);
                }
                else
                {
                    m_MapSpriteBatch.Draw(m_HudTileSet.TileSetTexture, new Rectangle((int)(i * pixelWidthPerTile), (int)((squaresDown - 1) * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_HudTileSet.GetSourceRectangle((int)HUDIcon.EMPTY_HEART), Microsoft.Xna.Framework.Color.White);
                }
            }

            //Draw Player's Quest Item
            for (int i = 0; i < m_Player.GetMaxNumOfQuestItem(); i++)
            {
                if (i < m_Player.GetCurrentNumOfQuestItem())
                {
                    m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle((int)((squaresAcross - (m_Player.GetMaxNumOfQuestItem() - i)) * pixelWidthPerTile), (int)((squaresDown - 2) * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle((int)SpellItems.QUEST_POTION), Microsoft.Xna.Framework.Color.White);
                }
                else
                {
                    m_MapSpriteBatch.Draw(m_ObjectTileSet.TileSetTexture, new Rectangle((int)((squaresAcross - (m_Player.GetMaxNumOfQuestItem() - i)) * pixelWidthPerTile), (int)((squaresDown - 2) * pixelHeightPerTile), pixelWidthPerTile, pixelHeightPerTile), m_ObjectTileSet.GetSourceRectangle((int)SpellItems.UNLOOTED_QUEST_ITEM), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_MapSpriteBatch.End();
        }

        private void DrawGameTime()
        {
            String gameTimeStr = m_MinutesPlaying.ToString("00") + ":" + m_SecondsPlaying.ToString("00");
            m_MapSpriteBatch.Begin();
            m_MapSpriteBatch.DrawString(m_FontArialBlack14, gameTimeStr, new Vector2((m_ScreenCenter.X) - m_FontArialBlack14.MeasureString(gameTimeStr).Length() / 2, 0), Microsoft.Xna.Framework.Color.Black);
            m_MapSpriteBatch.End();
        }

        private void ShowLevelGrid()
        {
            for (int i = 0; i < m_DoorLock.Length; i++)
            {
                m_DoorLock[i].SetDestroyed(true);
                m_DoorLock[i].SetUnlocked();
            }
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());


            for (m_TileCol = 0; m_TileCol < m_DemoMapTileWidth; m_TileCol++)
            {
                Rectangle horizontal_thin_rec = new Rectangle((int)(0 + (m_TileCol * pixelWidthPerTile)), 0, 1, (pixelHeightPerTile * m_DemoMapTileWidth));
                m_MapSpriteBatch.Draw(m_Texture1px, horizontal_thin_rec, Microsoft.Xna.Framework.Color.DarkGray);
            }

            for (m_TileRow = 0; m_TileRow < m_DemoMapTileHeight; m_TileRow++)
            {
                Rectangle vertical_thin_rec = new Rectangle(0, (int)(0 + (m_TileRow * pixelHeightPerTile)), (pixelWidthPerTile * m_DemoMapTileHeight), 1);
                m_MapSpriteBatch.Draw(m_Texture1px, vertical_thin_rec, Microsoft.Xna.Framework.Color.DarkGray);
            }
            m_MapSpriteBatch.End();
        }

        private void ShowRowColumnIndex()
        {
            m_MapSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_CameraView.TransformMatrix());

            string infoString;
            Vector2 originPosition = new Vector2(0, 0);
            for (int i = 0; i < m_DemoMapTileHeight; i++)
            {
                for (int j = 0; j < m_DemoMapTileWidth; j++)
                {
                    infoString = i + "." + j;
                    originPosition = m_FontArialBlack14.MeasureString(infoString) / 2;
                    m_MapSpriteBatch.DrawString(m_FontArialBlack14, infoString, new Vector2(i * pixelHeightPerTile + 16, j * pixelWidthPerTile + 16), Microsoft.Xna.Framework.Color.Blue, 0, originPosition, 1.0f, SpriteEffects.None, 0.5f);
                }
            }
            m_MapSpriteBatch.End();
        }

        private void DebugToggleGuideLine()
        {
            m_ShowGrid = !m_ShowGrid;
            m_ShowTileRowCol = !m_ShowTileRowCol;
        }

        private void DebugToggleGodMode()
        {
            m_IsGodMode = !m_IsGodMode;
        }
    }
}
