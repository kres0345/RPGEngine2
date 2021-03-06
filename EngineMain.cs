﻿using RPGEngine2.InputSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RPGEngine2
{
    // TODO: Make a configuration object to keep track of engine settings.
    // TODO: Come up with a system for scaling the screen and make the game have a consistent resolution and size. Change font size.
    public static class EngineMain
    {
        public static GameConfig GameConfig = new GameConfig();

        public static float DeltaTime { get; private set; } = 1;
        public static float FixedDeltaTime { get; private set; } = 1;
        private static readonly List<BaseObject> baseObjects = new List<BaseObject>();
        private static readonly List<BaseObject> instantiatedBaseObjects = new List<BaseObject>();
        private static bool isRunning;
        private static bool physicsFrame;
        private static float updateConsoleTitleTimer;

        public static event Action OnStart;
        public static event Action OnUpdate;
        public static event Action OnFixedUpdate;

        public static void Instantiate(BaseObject baseObject) => instantiatedBaseObjects.Add(baseObject);
        public static void EngineStop() => isRunning = false;

        public static void EngineStart()
        {
            Stopwatch swUpdate = new Stopwatch();
            Stopwatch swFixedUpdate = new Stopwatch();
            swUpdate.Start();
            swFixedUpdate.Start();


            Renderer.ScreenHeight = (short)(Console.WindowHeight - 1);
            Renderer.ScreenWidth = (short)(Console.WindowWidth);

            Console.CursorVisible = false;

            isRunning = true;
            OnStart?.Invoke();

            // Initialize gameconfig
            updateConsoleTitleTimer = GameConfig.ConsoleTitleRefreshRate;

            while (isRunning)
            {
                if (GameConfig.FramesPerSecondCap != 0)
                {
                    while (swUpdate.ElapsedMilliseconds < 1000f / GameConfig.FramesPerSecondCap && swFixedUpdate.ElapsedMilliseconds < 1000f / GameConfig.PhysicsUpdateRate)
                        Thread.Sleep(1);
                }

                if (GameConfig.FramesPerSecondCap == 0 || swUpdate.ElapsedMilliseconds >= 1000f / GameConfig.FramesPerSecondCap)
                {
                    DeltaTime = (float)swUpdate.Elapsed.TotalSeconds;
                    swUpdate.Restart();

                    UpdateFrame();
                }
                if (GameConfig.PhysicsUpdateRate == 0 || swFixedUpdate.ElapsedMilliseconds >= 1000f / GameConfig.PhysicsUpdateRate)
                {
                    FixedDeltaTime = (float)swFixedUpdate.Elapsed.TotalSeconds;
                    swFixedUpdate.Restart();

                    FixedUpdateFrame();
                }
            }
        }

        private static void UpdateFrame()
        {
            updateConsoleTitleTimer += DeltaTime;
            if (updateConsoleTitleTimer >= GameConfig.ConsoleTitleRefreshRate)
            {
                Console.Title = "FPS: " + Math.Floor(1 / DeltaTime);
                updateConsoleTitleTimer -= GameConfig.ConsoleTitleRefreshRate;
            }

            // Input
            InputDeviceHandler.RefreshDevices();

            // Logic
            OnUpdate?.Invoke();

            UpdateBaseObjects(baseObjects, physicsFrame);
            physicsFrame = false;

            // Render & draw
            Renderer.ResetScreenBuffer();
            Renderer.PerformRendering(baseObjects);
            Renderer.FlushScreenBuffer();

            // High-level garbage collection
            baseObjects.RemoveAll(gameObject => gameObject is null || gameObject.isDestroyed);

            // Spawn instantiated objects
            baseObjects.AddRange(instantiatedBaseObjects);
            instantiatedBaseObjects.Clear();
        }

        private static void FixedUpdateFrame()
        {
            physicsFrame = true;
            OnFixedUpdate?.Invoke();
        }

        private static void UpdateBaseObjects(List<BaseObject> baseObjects, bool physics)
        {
            List<GameObjectBase> physicsObjects = null;

            if (physics)
                physicsObjects = new List<GameObjectBase>(baseObjects.Count);

            foreach (BaseObject item in baseObjects)
            {
                if (item?.Active != true)
                    continue;

                switch (item)
                {
                    case GameObjectBase gameObject:
                        gameObject.Update();

                        gameObject.InternalPosition += gameObject.Velocity * DeltaTime;

                        if (physics && gameObject.PhysicsEnabled)
                        {
                            physicsObjects.Add(gameObject);
                        }
                        break;

                    case UIElementBase uiElement:
                        if (InputDeviceHandler.InternalMouseDevice is null)
                        {
                            uiElement.Update();
                            break;
                        }

                        bool hoverFrame = uiElement.InsideBounds(new Vector2(InputDeviceHandler.InternalMouseDevice.x, InputDeviceHandler.InternalMouseDevice.y));
                        UIElementBase.HoverState previousHoverState = uiElement.CurrentHoverState;

                        if (hoverFrame)
                        {
                            if (uiElement.CurrentHoverState == UIElementBase.HoverState.Enter)
                            {
                                uiElement.CurrentHoverState = UIElementBase.HoverState.Stay;
                            }
                            else if (uiElement.CurrentHoverState == UIElementBase.HoverState.Leave || uiElement.CurrentHoverState == UIElementBase.HoverState.None)
                            {
                                uiElement.CurrentHoverState = UIElementBase.HoverState.Enter;
                            }
                        }
                        else
                        {
                            if (uiElement.CurrentHoverState == UIElementBase.HoverState.Stay || uiElement.CurrentHoverState == UIElementBase.HoverState.Enter)
                            {
                                uiElement.CurrentHoverState = UIElementBase.HoverState.Leave;
                            }
                            else if (uiElement.CurrentHoverState == UIElementBase.HoverState.Leave)
                            {
                                uiElement.CurrentHoverState = UIElementBase.HoverState.None;
                            }
                        }

                        if (uiElement.CurrentHoverState != previousHoverState)
                        {
                            uiElement.HoverUpdate();
                        }

                        uiElement.Update();
                        break;

                    default:
                        item.Update();
                        break;
                }

                if (physics)
                {
                    Physics.GameObjectPhysics(physicsObjects);
                }
            }
        }
    }
}
