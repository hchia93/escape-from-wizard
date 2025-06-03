using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using EscapeFromWizard.Map;
using EscapeFromWizard.Source.GameObject.Dynamic;

namespace EscapeFromWizard.Source.GameObject.Static
{
    public class StaticObjectHandler
    {
        //Number setting of each item type
        private static int numOfKeyAndLock = 4;
        private static int numOfTotalItems = 20; //SpellItem + Stars
        private static int numOfSpellItem = 5;
        private static int numOfStars = 15;

        //Instance of each item type
        private Key[] m_Keys = new Key[numOfKeyAndLock];
        private Lock[] m_Locks = new Lock[numOfKeyAndLock];
        private SpellItem[] m_SpellItems = new SpellItem[numOfTotalItems];
        private SpellItem[] m_Stars = new SpellItem[numOfStars];

        //MapData
        private MapData m_MapData;
        private int levelRow;
        private int levelColumn;
        private List<Vector2> staticObjectPositions = new List<Vector2>();
        private List<Vector2> walkablePositions = new List<Vector2>();

        public StaticObjectHandler()
        {
            InitializeKey();
            InitializeLock();
            InitializeSpellItem();
        }

        public StaticObjectHandler(MapData mapData)
        {
            this.m_MapData = mapData;
            walkablePositions = this.m_MapData.GetWalkablePaths();
            levelRow = this.m_MapData.GetMapTileHeight();
            levelColumn = this.m_MapData.GetMapTileWidth();

            InitializeKey();
            InitializeLock();
            InitializeSpellItem();
        }

        public void SetMapReference(MapData mapData)
        {
            this.m_MapData = mapData;
            walkablePositions = this.m_MapData.GetWalkablePaths();
            levelRow = this.m_MapData.GetMapTileHeight();
            levelColumn = this.m_MapData.GetMapTileWidth();
        }

        private void InitializeKey()
        {
            Color[] keyLockColors = { Color.RED, Color.YELLOW, Color.GREEN, Color.BLUE };
            Vector2[] keyTilePositions = { new Vector2(5, 3), new Vector2(23, 1), new Vector2(23, 23), new Vector2(1, 23) };

            for (int i = 0; i < numOfKeyAndLock; i++)
            {
                m_Keys[i] = new Key();
                m_Keys[i].SetPosition((int)keyTilePositions[i].X, (int)keyTilePositions[i].Y);
                m_Keys[i].SetColor(keyLockColors[i]);

                //add keyTilePositions into the position list
                staticObjectPositions.Add(keyTilePositions[i]);
            }
        }

        private void InitializeSpellItem()
        {
            SpellItems[] spellItemTypes = { SpellItems.HP_POTION, SpellItems.HP_POTION,
                                                 SpellItems.QUEST_POTION, SpellItems.QUEST_POTION, SpellItems.QUEST_POTION };
            Vector2[] itemTilePositions = { new Vector2(23, 11), new Vector2(21, 13), new Vector2(1, 19), new Vector2(9, 11), new Vector2(23, 13) };

            for (int i = 0; i < numOfTotalItems; i++)
            {
                m_SpellItems[i] = new SpellItem();
            }

            for (int i = 0; i < numOfSpellItem; i++)
            {
                m_SpellItems[i].SetPosition((int)itemTilePositions[i].X, (int)itemTilePositions[i].Y);
                m_SpellItems[i].SetItemType(spellItemTypes[i]);

                //add itemTilePositions into the position list
                staticObjectPositions.Add(itemTilePositions[i]);
            }

            //Randomly generate locations for stars
            Random rnd = new Random();
            walkablePositions = walkablePositions.Except(staticObjectPositions).ToList();
            for (int i = 0; i < numOfStars; i++)
            {
                //Random pick a vector2 position from the walkable paths list
                Vector2 rndPos = walkablePositions[rnd.Next(walkablePositions.Count)];

                int x = (int)rndPos.X;
                int y = (int)rndPos.Y;

                m_SpellItems[numOfSpellItem + i].SetPosition(x, y);
                m_SpellItems[numOfSpellItem + i].SetItemType(SpellItems.STAR);
            }
        }


        private void InitializeLock()
        {
            Vector2[] lockTilePositions = { new Vector2(8, 23), new Vector2(14, 6), new Vector2(15, 12), new Vector2(10, 14) };
            Color[] keyLockColors = { Color.RED, Color.YELLOW, Color.GREEN, Color.BLUE };

            for (int i = 0; i < numOfKeyAndLock; i++)
            {
                m_Locks[i] = new Lock();
                m_Locks[i].SetPosition((int)lockTilePositions[i].X, (int)lockTilePositions[i].Y);
                m_Locks[i].SetColor(keyLockColors[i]);

                //add lockTilePositions into the position list
                staticObjectPositions.Add(lockTilePositions[i]);
            }
        }

        public void CheckLooted(GameTime gameTime, Player player)
        {
            foreach (var colorKey in m_Keys)
            {
                if (colorKey.IsOverlapped(player.GetPosition()))
                {
                    player.SetPickUpFlag(true);
                }

                foreach (var colorLock in m_Locks)
                {
                    if (colorKey.GetColor() == colorLock.GetColor())
                    {
                        colorLock.CheckIfDoorLockIsUnlocked(colorKey.IsLooted());
                    }
                }

            }

            for (int i = 0; i < m_SpellItems.Length; i++)
            {
                if (m_SpellItems[i].CheckPlayerPos(player.GetPosition()))
                {
                    player.SetPickUpFlag(true);

                    if (m_SpellItems[i].GetItemType() == SpellItems.HP_POTION)
                    {
                        player.Heal(1);
                    }
                    else if (m_SpellItems[i].GetItemType() == SpellItems.QUEST_POTION)
                    {
                        player.LootQuestItem();
                    }
                    else if (m_SpellItems[i].GetItemType() == SpellItems.STAR)
                    {
                        player.LootStar();
                    }
                }
            }


        }

        public Key[] GetKeys()
        {
            return m_Keys;
        }

        public Lock[] GetLocks()
        {
            return m_Locks;
        }

        public SpellItem[] GetSpellItems()
        {
            return m_SpellItems;
        }
    }
}
