using EscapeFromWizard.Map;
using EscapeFromWizard.Source;
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

    public class EscapeFromWizard : Game
    {
        public GraphicsDeviceManager m_Graphics;
        public SpriteBatch m_SpriteBatch;

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
        public SpriteSheet m_TileSpriteSheet;
        public SpriteSheet m_ObjectSpriteSheet;
        public SpriteSheet m_HUDSpriteSheet;
        public SpriteSheet m_MiscSpriteSheet;

        // GameLevel & GameView & GameState
        public Level m_Level;
        public Camera2D m_Camera;
        public GameStates m_GameStates;
        private GameInput m_GameInput;

        // Game Objects
        public Player m_Player;
        public Wizard m_Wizard;
        public Minion[] m_Minions;
        public SpellItem[] m_SpellItem;

        public Vector2 m_MovementOffset;

        //Game State
        public GameScreen m_CurrentScreen = GameScreen.HOME_SCREEN;
        public bool m_GameIsOver = false;
        public bool m_ShowErrorMsg = false;

        //Minion Hit
        public double m_HitDetectionTimer = GameSettings.HitDetectionTimerDefault;

        public SoundManager m_SoundManager;

        // Add new field for key container view model
        private KeyContainerViewModel m_KeyContainerViewModel;
        private PlayerHealthViewModel m_PlayerHealthViewModel;
        private QuestItemViewModel m_QuestItemViewModel;
        private ElapsedTimerViewModel m_ElapsedTimerViewModel;

        public EscapeFromWizard()
        {
            m_Graphics = new GraphicsDeviceManager(this);
            m_Graphics.PreferredBackBufferWidth = (int) GameSettings.m_ViewportSize.X;
            m_Graphics.PreferredBackBufferHeight = (int) GameSettings.m_ViewportSize.Y;
            m_Graphics.IsFullScreen = false;
            m_Graphics.ApplyChanges();

            m_SpriteBatch = new SpriteBatch(GraphicsDevice);

            m_CurrentScreen = GameScreen.HOME_SCREEN;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.IsMouseVisible = false;

            m_Level = new Level();
            m_Level.Initialize();

            // Camera
            Vector2 cameraBound = GameSettings.CalculateCameraBound(m_Level.GetTotalTileWidth(), m_Level.GetTotalTileHeight());
            m_Camera = new Camera2D();
            m_Camera.SetBoundary(0, 0, (int) cameraBound.X, (int) cameraBound.Y);

            // Player
            m_Player = new Player();
            m_Player.SetLevel(m_Level);
            m_Player.SetPosition(1, 1);
            m_Player.OnHitByMinion = () => m_SoundManager.PlayHitByMinionSound();
            m_Player.OnHitByWizard = () => m_SoundManager.PlayHitByWizardSound();
            m_Player.OnEnterOrExitHideState = () => m_SoundManager.PlayHideSound();
            m_Player.OnHealed = () => m_SoundManager.PlayHealSound();

            // Key And Locks
            m_GameStates = new GameStates(m_Level, m_Player);
            m_SpellItem = m_GameStates.GetSpellItems();

            m_Player.SetUpLockInformation(m_GameStates.GetLocks());

            // Sound
            m_SoundManager = new SoundManager(this.Content);


            // Wizard
            m_Wizard = new Wizard();
            m_Wizard.SetLevel(m_Level);
            m_Wizard.SetTargetPosition(12, 4);

            // Minions array
            m_Minions = new Minion[GameSettings.m_MaxMinionCount];
            for (int i = 0; i < m_Minions.Length; i++)
            {
                m_Minions[i] = new Minion();
                m_Minions[i].SetMinionId(i);
                m_Minions[i].SetLevel(m_Level);
                m_Minions[i].SetPatrolStartPos(GameSettings.MinionsInitialPatrolData[i]);
                m_Minions[i].SetTargetPosition(GameSettings.MinionsInitialPatrolData[i][0], GameSettings.MinionsInitialPatrolData[i][1]);
            }

            m_GameIsOver = false;

            // Sound
            m_SoundManager = new SoundManager(this.Content);

            m_GameStates.BindItemPickupSoundCallbacks(() => m_SoundManager.PlayPickUpSound());
            m_GameStates.BindLockDestroyedSoundCallback(() => m_SoundManager.PlayUnlockDoorSound());

            // Dispose previous GameInput instance if it exists
            m_GameInput?.Dispose();

            // Initialize GameInput with callback bindings
            m_GameInput = new GameInput();
            m_GameInput.OnF1Pressed = DebugToggleGodMode;
            m_GameInput.OnF2Pressed = DebugFullyHealPlayer;
            m_GameInput.OnF3Pressed = DebugCollectAllKeys;
            m_GameInput.OnF4Pressed = DebugCollectAllQuestItems;
            m_GameInput.OnF5Pressed = DebugUnlockAllLocks;
            m_GameInput.OnF6Pressed = DebugToggleGuideLine;
            m_GameInput.OnEscapePressed = () => this.Exit();

            // CreateViewModels has dependencies to player, hence create last. 
            CreateViewModels();
        }

        protected override void LoadContent()
        {
            m_TileSpriteSheet = new SpriteSheet();
            m_TileSpriteSheet.m_Texture = Content.Load<Texture2D>(@"Resource\Image\32PixelTiles\32Pixel_SpriteSheet_Tiles");

            m_ObjectSpriteSheet = new SpriteSheet();
            m_ObjectSpriteSheet.m_Texture = Content.Load<Texture2D>(@"Resource\Image\32PixelObjects\32Pixel_SpriteSheet_Object2");

            m_HUDSpriteSheet = new SpriteSheet();
            m_HUDSpriteSheet.m_Texture = Content.Load<Texture2D>(@"Resource\Image\32PixelHUD\32Pixel_SpriteSheet_HUD");

            m_MiscSpriteSheet = new SpriteSheet();

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
            m_GameStates.Update(gameTime);
            m_GameInput.Update(gameTime);
            m_SoundManager.Update(gameTime);

            if (m_CurrentScreen == GameScreen.GAME_SCREEN)
            {
                m_HitDetectionTimer += gameTime.ElapsedGameTime.TotalSeconds;

                //No longer listen user input if game is over
                if (!m_GameIsOver)
                {
                    // Get processed movement input from GameInput
                    ProcessedInput processedInput = m_GameInput.GetProcessedInput();
                    m_MovementOffset = processedInput.MovementOffset;
                }
                else
                {
                    m_MovementOffset = Vector2.Zero;
                }

                if (m_ShowErrorMsg && ( m_GameInput.IsKeyDown(Keys.Enter) || m_GameInput.GetCurrentMouseState().LeftButton == ButtonState.Pressed ))
                {
                    m_ShowErrorMsg = false;
                }

                m_Player.ProcessMovement(m_MovementOffset);
                m_Player.UpdateMovement(gameTime);

                m_Wizard.SetPlayerExitFlag(m_Player.IsOnExit());
                m_Wizard.SetPlayerHideFlag(m_Player.GetIsHiding());
                m_Wizard.UpdateMovement(gameTime, m_Player.GetPosition());

                for (int i = 0; i < m_Minions.Length; i++)
                {
                    m_Minions[i].SetPlayerExitFlag(m_Player.IsOnExit());
                    m_Minions[i].SetPlayerHideFlag(m_Player.IsOnExit());
                    m_Minions[i].UpdateMovement(gameTime, m_Player.GetPosition());
                }

                m_Camera.UpdateMovement(m_Player.GetMovingDirection(), m_Player.GetPosition(), GameSettings.m_ViewportCenter);

                // Smart sound playing with encapsulated timing logic
                m_SoundManager.PlayBGM();
                m_SoundManager.TryPlayFootstepSound(!m_Player.IsStanding());

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
                                m_Player.TakeDamage(GameSettings.m_MinionHitDamage, DamageSource.Minion);
                                m_Minions[i].SetPlayerWasHitFlag(false);
                            }
                        }

                        if (m_Wizard.GetPlayerWasHitFlag() == true)
                        {
                            m_Player.TakeDamage(GameSettings.m_WizardHitDamage, DamageSource.Wizard);
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
                if (( m_GameInput.IsKeyDown(Keys.Enter) || m_GameInput.GetCurrentMouseState().LeftButton == ButtonState.Pressed ) && !m_GameInput.GetPreviousKeyboardState().IsKeyDown(Keys.Enter))
                {
                    m_CurrentScreen = GameScreen.GAME_SCREEN;
                }
            }
            else if (m_CurrentScreen == GameScreen.GAME_OVER_SCREEN)
            {
                if (m_GameInput.IsKeyDown(Keys.Enter) || m_GameInput.GetCurrentMouseState().LeftButton == ButtonState.Pressed)
                {
                    m_SoundManager.StopGameOverSound();
                    Initialize();
                    m_CurrentScreen = GameScreen.GAME_SCREEN;
                }
            }
            else if (m_CurrentScreen == GameScreen.VICTORY_SCREEN)
            {
                if (m_GameInput.IsKeyDown(Keys.Enter) || m_GameInput.GetCurrentMouseState().LeftButton == ButtonState.Pressed)
                {
                    Initialize();
                    m_CurrentScreen = GameScreen.HOME_SCREEN;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (m_CurrentScreen == GameScreen.GAME_SCREEN)
            {
                GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
                DrawBackground();
                DrawLevel();
                DrawSpellItem();
                DrawKey();
                DrawLock();
                DrawPlayer();
                DrawWizard();
                DrawMinions();
                DrawWidget();

                if (m_ShowGrid)
                {
                    DrawLevelGridInfo();
                }

                if (m_ShowTileRowCol)
                {
                    DrawTileIndex();
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
                m_GameStates.CalculateScore();
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

        void CreateViewModels()
        {
            // Initialize Key Container View Model
            m_KeyContainerViewModel = new KeyContainerViewModel();

            // Initialize key view models with HUD textures
            foreach (var key in m_GameStates.GetKeys())
            {
                KeyViewModel keyViewModel = new KeyViewModel(key);
                keyViewModel.SetSourceSpriteSheet(m_HUDSpriteSheet);
                m_KeyContainerViewModel.AddKeyViewModel(keyViewModel);
            }

            // Initialize Player Health View Model
            m_PlayerHealthViewModel = new PlayerHealthViewModel(m_Player);
            m_PlayerHealthViewModel.SetSourceSpriteSheet(m_HUDSpriteSheet);
            Vector2 healthPosition = new Vector2(0, ( GameSettings.m_TilePerColumn - 1 ) * GameSettings.m_TileHeightInPx); // Position at bottom row
            m_PlayerHealthViewModel.SetWidgetPosition(healthPosition);

            // Initialize Quest Item View Model
            m_QuestItemViewModel = new QuestItemViewModel(m_Player);
            m_QuestItemViewModel.SetSourceSpriteSheet(m_ObjectSpriteSheet);
            Vector2 questItemPosition = new Vector2(( GameSettings.m_TilePerRow - m_Player.GetMaxNumOfQuestItem() ) * GameSettings.m_TileWidthInPx,
                                                  ( GameSettings.m_TilePerColumn - 2 ) * GameSettings.m_TileHeightInPx);
            m_QuestItemViewModel.SetWidgetPosition(questItemPosition);

            // Initialize Elapsed Timer View Model
            m_ElapsedTimerViewModel = new ElapsedTimerViewModel(m_GameStates);
            m_ElapsedTimerViewModel.SetFont(m_FontArialBlack14);
            m_ElapsedTimerViewModel.SetWidgetPosition(new Vector2(GameSettings.m_ViewportCenter.X, 0));
        }

        private void GameOver(GameTime gameTime)
        {
            m_CurrentScreen = GameScreen.GAME_OVER_SCREEN;
            m_GameIsOver = true;

            foreach (var minion in m_Minions)
            {
                minion.SetBehavior(EBehaviorState.STOP);
            }

            m_SoundManager.PlayGameOver();
        }

        private void DrawQuestIncompleteMessage()
        {
            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(m_QuestIncompleteMessage, new Vector2(50, GameSettings.m_ViewportCenter.Y), Microsoft.Xna.Framework.Color.White);
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
            m_SpriteBatch.Begin();
            string scoreInfo = "Your score is " + m_GameStates.GetScore().ToString();
            m_SpriteBatch.DrawString(m_FontArialBlack14, scoreInfo, new Vector2(GameSettings.m_ViewportCenter.X - m_FontArialBlack14.MeasureString(scoreInfo).Length() / 2, GameSettings.m_ViewportCenter.Y - 50), Microsoft.Xna.Framework.Color.Black);
            m_SpriteBatch.End();
        }
        private void DrawLevel()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());
            for (int row = 0; row < m_Level.GetTotalTileHeight(); ++row)
            {
                for (int col = 0; col < m_Level.GetTotalTileWidth(); ++col)
                {
                    int tileID = m_Level.m_Data[row * m_Level.GetTotalTileWidth() + col];
                    Rectangle tileRect = GameSettings.CreateTileRectangleAt(col, row);
                    m_SpriteBatch.Draw(m_TileSpriteSheet.m_Texture, tileRect, m_TileSpriteSheet.GetSourceRectangle(tileID), Microsoft.Xna.Framework.Color.White);
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
            m_SpriteBatch.Draw(m_ObjectSpriteSheet.m_Texture, playerRect, m_ObjectSpriteSheet.GetSourceRectangle(4), Microsoft.Xna.Framework.Color.White);

            m_SpriteBatch.End();
        }

        private void DrawWizard()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            Rectangle wizardRect = GameSettings.CreateTileRectangleAt(m_Wizard.GetTargetPosition());
            m_SpriteBatch.Draw(m_ObjectSpriteSheet.m_Texture, wizardRect, m_ObjectSpriteSheet.GetSourceRectangle(14), Microsoft.Xna.Framework.Color.White);
            m_SpriteBatch.End();
        }
        private void DrawMinions()
        {
            for (int i = 0; i < m_Minions.Length; i++)
            {
                m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

                Rectangle minionRect = GameSettings.CreateTileRectangleAt(m_Minions[i].GetTargetPosition());
                m_SpriteBatch.Draw(m_ObjectSpriteSheet.m_Texture, minionRect, m_ObjectSpriteSheet.GetSourceRectangle(19), Microsoft.Xna.Framework.Color.White);
                m_SpriteBatch.End();
            }
        }
        private void DrawSpellItem()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            for (int i = 0; i < m_SpellItem.Length; i++)
            {
                if (!m_SpellItem[i].GetIsLooted())
                {
                    Rectangle spellItemRect = GameSettings.CreateTileRectangleAt(m_SpellItem[i].GetPosition());
                    m_SpriteBatch.Draw(m_ObjectSpriteSheet.m_Texture, spellItemRect, m_ObjectSpriteSheet.GetSourceRectangle(m_SpellItem[i].GetItemTypeIndex()), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_SpriteBatch.End();
        }

        private void DrawKey()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());

            foreach (var key in m_GameStates.GetKeys())
            {
                if (!key.IsLooted())
                {
                    Rectangle keyRect = GameSettings.CreateTileRectangleAt(key.GetPosition());
                    m_SpriteBatch.Draw(m_ObjectSpriteSheet.m_Texture, keyRect, m_ObjectSpriteSheet.GetSourceRectangle((int) key.GetColor()), Microsoft.Xna.Framework.Color.White);
                }
            }
            m_SpriteBatch.End();
        }
        private void DrawLock()
        {
            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m_Camera.TransformMatrix());
            foreach (var doorlock in m_GameStates.GetLocks())
            {
                if (!doorlock.IsDestroyed())
                {
                    Rectangle lockRect = GameSettings.CreateTileRectangleAt(doorlock.GetPosition());
                    m_SpriteBatch.Draw(m_ObjectSpriteSheet.m_Texture, lockRect, m_ObjectSpriteSheet.GetSourceRectangle((int) ( doorlock.GetColor() ) + 15), Microsoft.Xna.Framework.Color.White);
                }
            }

            m_SpriteBatch.End();
        }
        private void DrawWidget()
        {
            m_SpriteBatch.Begin();
            m_KeyContainerViewModel.DrawWidget(m_SpriteBatch);
            m_PlayerHealthViewModel.DrawWidget(m_SpriteBatch);
            m_QuestItemViewModel.DrawWidget(m_SpriteBatch);
            m_ElapsedTimerViewModel.DrawWidget(m_SpriteBatch);
            m_SpriteBatch.End();
        }
        private void DrawLevelGridInfo()
        {
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

        private void DrawTileIndex()
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

        private void DebugUnlockAllLocks()
        {
            foreach (var doorlock in m_GameStates.GetLocks()) 
            {
                doorlock.SetDestroyed(true);
                doorlock.SetUnlocked(true);
            }
        }
        private void DebugCollectAllKeys()
        {
            foreach (var key in m_GameStates.GetKeys())
            {
                if (!key.IsLooted())
                {
                    m_Player.CollectKey(key);
                    key.SetLooted(true);

                    foreach (var doorLock in m_GameStates.GetLocks())
                    {
                        if (doorLock.GetColor() == key.GetColor())
                        {
                            doorLock.SetUnlocked(true);
                        }
                    }
                }
            }
        }

        private void DebugFullyHealPlayer()
        {
            int healAmount = m_Player.GetMaxHP() - m_Player.GetHP();
            if (healAmount > 0)
            {
                m_Player.Heal(healAmount);
            }
        }

        private void DebugCollectAllQuestItems()
        {
            foreach (var spellItem in m_GameStates.GetSpellItems())
            {
                if (spellItem.GetItemType() == SpellItems.QUEST_POTION && !spellItem.GetIsLooted())
                {
                    m_Player.LootQuestItem();
                    spellItem.SetLooted(true);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose GameInput to clean up callbacks
                m_GameInput?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
