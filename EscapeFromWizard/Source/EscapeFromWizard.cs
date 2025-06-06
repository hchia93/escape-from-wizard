using EscapeFromWizard.Map;
using EscapeFromWizard.Source.Audio;
using EscapeFromWizard.Source.GameObject.Dynamic;
using EscapeFromWizard.Source.GameObject.Static;
using EscapeFromWizard.ViewModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TileEngine;

namespace EscapeFromWizard
{
    public enum GameScreen
    {
        HOME_SCREEN,
        GAME_SCREEN,
        GAME_OVER_SCREEN,
        VICTORY_SCREEN
    }

    public struct InputAxisInfo
    {
        public int m_YDown;
        public int m_YUp;
        public int m_XLeft;
        public int m_XRight;
    };

    public class EscapeFromWizard : Game
    {
        public GraphicsDeviceManager m_Graphics;
        public SpriteBatch m_SpriteBatch;

        // Game settings are now centralized in GameSettings class

        // Debug Mode
        public bool m_ShowGrid = false;
        public bool m_ShowTileRowCol = false;
        public bool m_IsGodMode = false;

        //GameFonts & Resources & Texture & Custom Class
        public SpriteFont m_FontArialBlack14;
        public Texture2D m_Texture1px;
        public Texture2D m_Background;
        public Texture2D m_HomeScreenOverlay;
        public Texture2D m_GameOverScreenOverlay;
        public Texture2D m_VictoryScreenOverlay;
        public Texture2D m_QuestIncompleteMessage;
        public Tile m_TileCollection;
        public Tile m_ObjectCollection;
        public Tile m_HUDCollection;
        public Tile m_OtherTileCollection;

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
        public InputAxisInfo m_InputHandler;

        //GameMap & GameView & Instances
        public Camera2D m_Camera;
        public Level m_Level;
        public KeyIDProvider m_KeyIDProvider;

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

        public Vector2 m_MovementOffset;

        //Game State
        public GameScreen m_CurrentScreen;
        public bool m_GameIsOver;
        public bool m_ShowErrorMsg = false;
        public float m_TotalGameTime;
        public float m_SecondsPlaying;
        public int m_MinutesPlaying;
        public int m_Score;

        //Minion Hit
        public bool m_DecreaseHPOnlyOnce;
        public double m_HitDetectionTimer = GameSettings.HitDetectionTimerDefault;

        public SoundManager m_SoundManager;

        // Add new field for key container view model
        private KeyContainerViewModel m_KeyContainerViewModel;

        public EscapeFromWizard()
        {
            m_Graphics = new GraphicsDeviceManager(this);
            m_Graphics.PreferredBackBufferWidth = (int)GameSettings.m_ViewportSize.X;
            m_Graphics.PreferredBackBufferHeight = (int)GameSettings.m_ViewportSize.Y;
            m_Graphics.IsFullScreen = false;
            m_Graphics.ApplyChanges();

            m_SpriteBatch = new SpriteBatch(GraphicsDevice);

            m_ScreenCenter = GameSettings.m_ViewportCenter;

            m_CurrentScreen = GameScreen.HOME_SCREEN;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.IsMouseVisible = false;

            // Map Related
            m_KeyIDProvider = new KeyIDProvider();
            m_Level = new Level();
            m_Level.Initialize();

            // Camera
            Vector2 cameraBound = GameSettings.CalculateCameraBound(m_Level.GetTotalTileWidth(), m_Level.GetTotalTileHeight());
            m_Camera = new Camera2D();
            m_Camera.SetBoundary(0, 0, (int) cameraBound.X, (int) cameraBound.Y);

            // Key And Locks
            m_StaticObjectHandler = new StaticObjectHandler(m_Level);
            m_Key = m_StaticObjectHandler.GetKeys();
            m_DoorLock = m_StaticObjectHandler.GetLocks();
            m_SpellItem = m_StaticObjectHandler.GetSpellItems();

            m_KeyContainerViewModel = new KeyContainerViewModel();
            Rectangle keyViewRect = GameSettings.CreateTileRectangleAt(GameSettings.m_TilePerRow - 4, GameSettings.m_TilePerColumn - 1);
            m_KeyContainerViewModel.SetBaseWidgetPosition(keyViewRect.Location.ToVector2());

            // Initialize key view models with HUD textures
            for (int i = 0; i < m_Key.Length; i++)
            {
                KeyViewModel keyViewModel = new KeyViewModel();
                
                keyViewModel.SetSourceTexture(m_HUDCollection.m_TileSetTexture);
                keyViewModel.SetPreLootedRectangle(m_HUDCollection.GetSourceRectangle(m_KeyIDProvider.GetKeyIndex(i + 4))); // Empty key slot
                keyViewModel.SetPostLootedRectangle(m_HUDCollection.GetSourceRectangle(m_KeyIDProvider.GetKeyIndex(i))); // Filled key slot
                
                m_KeyContainerViewModel.AddKeyViewModel(keyViewModel);
            }

            // Sound
            m_SoundManager = new SoundManager(this.Content);

            // Player
            m_Player = new Player();
            m_Player.SetWorld(m_Level);
            m_Player.SetPosition(1, 1);
            m_Player.SetUpLockInformation(m_DoorLock);
            m_Player.OnHitByMinion = () => m_SoundManager.PlayHitByMinionSound();
            m_Player.OnHitByWizard = () => m_SoundManager.PlayHitByWizardSound();

            // Wizard
            m_Wizard = new Wizard();
            m_Wizard.SetWorld(m_Level);
            m_Wizard.SetTargetPosition(12, 4);

            // Minions array
            m_Minions = new Minion[GameSettings.NumOfMinions];
            for (int i = 0; i < m_Minions.Length; i++)
            {
                m_Minions[i] = new Minion();
                m_Minions[i].SetMinionId(i);
                m_Minions[i].SetWorld(m_Level);
                m_Minions[i].SetPatrolStartPos(GameSettings.MinionsInitialPatrolData[i]);
                m_Minions[i].SetTargetPosition(GameSettings.MinionsInitialPatrolData[i][0], GameSettings.MinionsInitialPatrolData[i][1]);
            }

            m_GameIsOver = false;
            m_MinutesPlaying = 0;
            m_SecondsPlaying = 0.0f;

            // Sound
            m_SoundManager = new SoundManager(this.Content);
            m_PlayGameBGMOnlyOnce = true;
            m_PlayGameOverOnlyOnce = true;
            m_PlayButtonOnlyOnce = true;
            m_PlayPickUpOnlyOnce = true;
            m_PlayUnlockDoorOnlyOnce = new bool[GameSettings.NumOfLocks] { true, true, true, true };
            m_PlayerPickUpSomething = false;
            m_FootStepTimer = 0.0f;
        }

        protected override void LoadContent()
        {
            m_TileCollection = new Tile();
            m_TileCollection.m_TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelTiles\32Pixel_SpriteSheet_Tiles");

            m_ObjectCollection = new Tile();
            m_ObjectCollection.m_TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelObjects\32Pixel_SpriteSheet_Object2");

            m_HUDCollection = new Tile();
            m_HUDCollection.m_TileSetTexture = Content.Load<Texture2D>(@"Resource\Image\32PixelHUD\32Pixel_SpriteSheet_HUD");

            m_OtherTileCollection = new Tile();

            m_Texture1px = new Texture2D(GraphicsDevice, 1, 1);
            m_Texture1px.SetData(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

            m_Background = new Texture2D(GraphicsDevice, 256, 256);
            m_Background = Content.Load<Texture2D>(@"Resource\Image\floor_256px");

            m_HomeScreenOverlay = new Texture2D(GraphicsDevice, 576, 576);
            m_HomeScreenOverlay = Content.Load<Texture2D>(@"Resource\Image\home_fit");

            m_GameOverScreenOverlay = new Texture2D(GraphicsDevice, 576, 576);
            m_GameOverScreenOverlay = Content.Load<Texture2D>(@"Resource\Image\gameover_fit");

            m_VictoryScreenOverlay = new Texture2D(GraphicsDevice, 576, 576);
            m_VictoryScreenOverlay = Content.Load<Texture2D>(@"Resource\Image\youwon_fit");

            m_QuestIncompleteMessage = new Texture2D(GraphicsDevice, 576, 576);
            m_QuestIncompleteMessage = Content.Load<Texture2D>(@"Resource\Image\questnotcompleted");

            m_FontArialBlack14 = Content.Load<SpriteFont>(@"Resource\Font\Arial_Black_14pxl");

           
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            // Exit();

            m_InputKey = Keyboard.GetState();
            m_MouseState = Mouse.GetState();

            if (m_CurrentScreen == GameScreen.GAME_SCREEN)
            {
                m_FootStepTimer += gameTime.ElapsedGameTime.TotalSeconds;
                m_HitDetectionTimer += gameTime.ElapsedGameTime.TotalSeconds;

                //calculate total game time elapsed
                m_SecondsPlaying += (float) gameTime.ElapsedGameTime.TotalSeconds;
                if (m_SecondsPlaying >= 60)
                {
                    m_MinutesPlaying++;
                    m_SecondsPlaying = 0;
                }

                //No longer listen user input if game is over
                if (!m_GameIsOver)
                {
                    m_InputHandler.m_YDown = ( m_InputKey.IsKeyDown(Keys.S) || m_InputKey.IsKeyDown(Keys.Down) ) ? -1 : 0;
                    m_InputHandler.m_YUp = ( m_InputKey.IsKeyDown(Keys.W) || m_InputKey.IsKeyDown(Keys.Up) ) ? 1 : 0;
                    m_InputHandler.m_XLeft = ( m_InputKey.IsKeyDown(Keys.A) || m_InputKey.IsKeyDown(Keys.Left) ) ? -1 : 0;
                    m_InputHandler.m_XRight = ( m_InputKey.IsKeyDown(Keys.D) || m_InputKey.IsKeyDown(Keys.Right) ) ? 1 : 0;
                }

                int finalVertical = m_InputHandler.m_YDown + m_InputHandler.m_YUp;
                int finalHorizontal = m_InputHandler.m_XLeft + m_InputHandler.m_XRight;

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

                if (m_ShowErrorMsg && ( m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed ))
                {
                    m_ShowErrorMsg = false;
                }

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
                m_Camera.UpdateMovement(m_Player.GetMovingDirection(), m_Player.GetPosition(), m_ScreenCenter);

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
                                m_Player.TakeDamageFromMinion(GameSettings.MinionHitDamage);
                                m_Minions[i].SetPlayerWasHitFlag(false);
                            }
                        }

                        if (m_Wizard.GetPlayerWasHitFlag() == true)
                        {
                            m_Player.TakeDamageFromWizard(3);
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
            else if (m_CurrentScreen == GameScreen.HOME_SCREEN)
            {
                if (( m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed ) && !m_PreviousInputKey.IsKeyDown(Keys.Enter))
                {
                    m_CurrentScreen = GameScreen.GAME_SCREEN;
                }
            }
            else if (m_CurrentScreen == GameScreen.GAME_OVER_SCREEN)
            {
                if (m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed)
                {
                    m_SoundManager.StopGameOverSound();
                    Initialize();
                    m_CurrentScreen = GameScreen.GAME_SCREEN;
                }
            }
            else if (m_CurrentScreen == GameScreen.VICTORY_SCREEN)
            {
                if (m_InputKey.IsKeyDown(Keys.Enter) || m_MouseState.LeftButton == ButtonState.Pressed)
                {
                    Initialize();
                    m_CurrentScreen = GameScreen.HOME_SCREEN;
                }
            }

            m_PreviousInputKey = m_InputKey;
        }

        protected override void Draw(GameTime gameTime)
        {
            if (m_CurrentScreen == GameScreen.GAME_SCREEN)
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
                    DrawQuestIncompleteMessage();
                }

                base.Draw(gameTime);
            }
            else if (m_CurrentScreen == GameScreen.HOME_SCREEN)
            {
                DrawScreenOverlay(m_HomeScreenOverlay);
            }
            else if (m_CurrentScreen == GameScreen.GAME_OVER_SCREEN)
            {
                DrawScreenOverlay(m_GameOverScreenOverlay);
            }
            else if (m_CurrentScreen == GameScreen.VICTORY_SCREEN)
            {
                DrawScreenOverlay(m_VictoryScreenOverlay);
                DrawGameScore();
            }
        }
        private void GameFinished(GameTime gameTime)
        {
            if (m_Player.GetCurrentNumOfQuestItem() >= m_Player.GetMaxNumOfQuestItem())
            {
                CalculateScore();
                m_CurrentScreen = GameScreen.VICTORY_SCREEN;
                m_SoundManager.OnGameFinished();
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
            m_CurrentScreen = GameScreen.GAME_OVER_SCREEN;
            m_GameIsOver = true;

            foreach (var minion in m_Minions)
            {
                minion.SetBehavior(EBehaviorState.STOP);
            }

            if (m_PlayGameOverOnlyOnce)
            {
                m_SoundManager.OnGameOver();
                m_PlayGameOverOnlyOnce = false;
            }
        }
        private void CalculateScore()
        {
            m_Score = 300 / ( ( m_MinutesPlaying * 60 ) + (int) m_SecondsPlaying ) * 100 + m_Player.GetCurrentNumOfStar() * 5;
        }

        private void DrawQuestIncompleteMessage()
        {
            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(m_QuestIncompleteMessage, new Vector2(50, m_ScreenCenter.Y), Microsoft.Xna.Framework.Color.White);
            m_SpriteBatch.End();
        }
        private void DrawScreenOverlay(Texture2D screenBackground)
        {
            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(screenBackground, new Vector2(0, 0), Microsoft.Xna.Framework.Color.White);
            m_SpriteBatch.End();
        }

        private void DrawGameScore()
        {
            if (m_CurrentScreen != GameScreen.VICTORY_SCREEN)
            {
                return;
            }

            string scoreInfo = "Your score is " + m_Score.ToString();
            m_SpriteBatch.Begin();
            m_SpriteBatch.DrawString(m_FontArialBlack14, scoreInfo, new Vector2(m_ScreenCenter.X - m_FontArialBlack14.MeasureString(scoreInfo).Length() / 2, m_ScreenCenter.Y - 50), Microsoft.Xna.Framework.Color.Black);
            m_SpriteBatch.End();
        }

        private void DrawMap()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());
            for (int row = 0; row < m_Level.GetTotalTileHeight(); ++row)
            {
                for (int col = 0; col < m_Level.GetTotalTileWidth(); ++col)
                {
                    int tileID = m_Level.m_Data[row * m_Level.GetTotalTileWidth() + col];
                    Rectangle tileRect = GameSettings.CreateTileRectangleAt(col, row);
                    m_SpriteBatch.Draw(m_TileCollection.m_TileSetTexture, tileRect, m_TileCollection.GetSourceRectangle(tileID), Microsoft.Xna.Framework.Color.White);
                }
            }
            m_SpriteBatch.End();
        }

        private void DrawBackground()
        {
            int x = 0, y = 32;
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());
            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 3; ++col)
                {
                    m_SpriteBatch.Draw(m_Background, new Vector2(x, y), Microsoft.Xna.Framework.Color.White);
                    x += 8 * GameSettings.m_TileWidthInPx;
                }
                y += 8 * GameSettings.m_TileHeightInPx;
                x = 0;
            }
            m_SpriteBatch.End();
        }

        private void DrawPlayer()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            Rectangle playerRect = GameSettings.CreateTileRectangleAt(m_Player.GetPosition());
            m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, playerRect, m_ObjectCollection.GetSourceRectangle(4), Microsoft.Xna.Framework.Color.White);

            m_SpriteBatch.End();

        }

        private void DrawWizard()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            Rectangle wizardRect = GameSettings.CreateTileRectangleAt(m_Wizard.GetTargetPosition());
            m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, wizardRect, m_ObjectCollection.GetSourceRectangle(14), Microsoft.Xna.Framework.Color.White);
            m_SpriteBatch.End();
        }

        private void DrawMinions()
        {
            for (int i = 0; i < m_Minions.Length; i++)
            {
                m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

                Rectangle minionRect = GameSettings.CreateTileRectangleAt(m_Minions[i].GetTargetPosition());
                m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, minionRect, m_ObjectCollection.GetSourceRectangle(19), Microsoft.Xna.Framework.Color.White);
                m_SpriteBatch.End();
            }
        }

        private void DrawSpellItem()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            for (int i = 0; i < m_SpellItem.Length; i++)
            {
                if (!m_SpellItem[i].isLooted())
                {
                    Rectangle spellItemRect = GameSettings.CreateTileRectangleAt((int) m_SpellItem[i].GetItemTilePositionX(), (int) m_SpellItem[i].GetItemTilePositionY());
                    m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, spellItemRect, m_ObjectCollection.GetSourceRectangle(m_SpellItem[i].GetItemTypeIndex()), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_SpriteBatch.End();
        }

        private void DrawKey()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            for (int i = 0; i < m_Key.Length; i++)
            {
                if (!m_Key[i].IsLooted())
                {
                    Rectangle keyRect = GameSettings.CreateTileRectangleAt(m_Key[i].GetPosition());
                    m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, keyRect, m_ObjectCollection.GetSourceRectangle((int) m_Key[i].GetColor()), Microsoft.Xna.Framework.Color.White);
                }
            }
            m_SpriteBatch.End();
        }

        private void DrawLock()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());
            for (int i = 0; i < m_DoorLock.Length; i++)
            {
                if (!m_DoorLock[i].IsDestroyed())
                {
                    Rectangle lockRect = GameSettings.CreateTileRectangleAt(m_DoorLock[i].GetPosition());
                    m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, lockRect, m_ObjectCollection.GetSourceRectangle((int) ( m_DoorLock[i].GetColor() ) + 15), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_SpriteBatch.End();
        }

        private void DrawHUD()
        {
            m_SpriteBatch.Begin();
            
            // Draw keys using the view model
            for (int i = 0; i < m_Key.Length; i++)
            {
                m_KeyContainerViewModel.UpdateKeyState(i, m_Key[i].IsLooted());
            }
            m_KeyContainerViewModel.Draw(m_SpriteBatch);

            // Draw Player's HP
            for (int i = 0; i < m_Player.GetMaxHP(); i++)
            {
                if (i < m_Player.GetHP())
                {
                    Rectangle heartRect = GameSettings.CreateTileRectangleAt(i, GameSettings.m_TilePerColumn - 1);
                    m_SpriteBatch.Draw(m_HUDCollection.m_TileSetTexture, heartRect, m_HUDCollection.GetSourceRectangle((int) HUDIcon.FULL_HEART), Microsoft.Xna.Framework.Color.White);
                }
                else
                {
                    Rectangle emptyHeartRect = GameSettings.CreateTileRectangleAt(i, GameSettings.m_TilePerColumn - 1);
                    m_SpriteBatch.Draw(m_HUDCollection.m_TileSetTexture, emptyHeartRect, m_HUDCollection.GetSourceRectangle((int) HUDIcon.EMPTY_HEART), Microsoft.Xna.Framework.Color.White);
                }
            }

            //Draw Player's Quest Item
            for (int i = 0; i < m_Player.GetMaxNumOfQuestItem(); i++)
            {
                if (i < m_Player.GetCurrentNumOfQuestItem())
                {
                    Rectangle questItemRect = GameSettings.CreateTileRectangleAt(GameSettings.m_TilePerRow - ( m_Player.GetMaxNumOfQuestItem() - i ), GameSettings.m_TilePerColumn - 2);
                    m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, questItemRect, m_ObjectCollection.GetSourceRectangle((int) SpellItems.QUEST_POTION), Microsoft.Xna.Framework.Color.White);
                }
                else
                {
                    Rectangle emptyQuestItemRect = GameSettings.CreateTileRectangleAt(GameSettings.m_TilePerRow - ( m_Player.GetMaxNumOfQuestItem() - i ), GameSettings.m_TilePerColumn - 2);
                    m_SpriteBatch.Draw(m_ObjectCollection.m_TileSetTexture, emptyQuestItemRect, m_ObjectCollection.GetSourceRectangle((int) SpellItems.UNLOOTED_QUEST_ITEM), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_SpriteBatch.End();
        }

        private void DrawGameTime()
        {
            String gameTimeStr = m_MinutesPlaying.ToString("00") + ":" + m_SecondsPlaying.ToString("00");
            m_SpriteBatch.Begin();
            m_SpriteBatch.DrawString(m_FontArialBlack14, gameTimeStr, new Vector2(( m_ScreenCenter.X ) - m_FontArialBlack14.MeasureString(gameTimeStr).Length() / 2, 0), Microsoft.Xna.Framework.Color.Black);
            m_SpriteBatch.End();
        }

        private void ShowLevelGrid()
        {
            for (int i = 0; i < m_DoorLock.Length; i++)
            {
                m_DoorLock[i].SetDestroyed(true);
                m_DoorLock[i].SetUnlocked(true);
            }
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());


            for (int column = 0; column < m_Level.GetTotalTileWidth(); column++)
            {
                Rectangle horizontal_thin_rec = new Rectangle((int) ( 0 + ( column * GameSettings.m_TileWidthInPx ) ), 0, 1, ( GameSettings.m_TileHeightInPx * m_Level.GetTotalTileWidth() ));
                m_SpriteBatch.Draw(m_Texture1px, horizontal_thin_rec, Microsoft.Xna.Framework.Color.DarkGray);
            }

            for (int row = 0; row < m_Level.GetTotalTileHeight(); row++)
            {
                Rectangle vertical_thin_rec = new Rectangle(0, (int) ( 0 + ( row * GameSettings.m_TileHeightInPx ) ), ( GameSettings.m_TileWidthInPx * m_Level.GetTotalTileHeight() ), 1);
                m_SpriteBatch.Draw(m_Texture1px, vertical_thin_rec, Microsoft.Xna.Framework.Color.DarkGray);
            }
            m_SpriteBatch.End();
        }

        private void ShowRowColumnIndex()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            string infoString;
            Vector2 originPosition = new Vector2(0, 0);
            for (int i = 0; i < m_Level.GetTotalTileHeight(); i++)
            {
                for (int j = 0; j < m_Level.GetTotalTileWidth(); j++)
                {
                    infoString = i + "." + j;
                    originPosition = m_FontArialBlack14.MeasureString(infoString) / 2;
                    m_SpriteBatch.DrawString(m_FontArialBlack14, infoString, new Vector2(i * GameSettings.m_TileHeightInPx + 16, j * GameSettings.m_TileWidthInPx + 16), Microsoft.Xna.Framework.Color.Blue, 0, originPosition, 1.0f, SpriteEffects.None, 0.5f);
                }
            }
            m_SpriteBatch.End();
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
