// Project:         HUD Be Gone mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    8/13/2024, 7:00 PM
// Last Edit:		8/17/2024, 8:40 PM
// Version:			1.01
// Special Thanks:  
// Modifier:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop;

namespace HUDBeGone
{
    public partial class HUDBeGoneMain : MonoBehaviour
    {
        public static HUDBeGoneMain Instance;

        static Mod mod;

        // General Options
        public static bool AllowHiding { get; set; }

        // HUD Hiding Options
        public static bool HideEverything { get; set; }
        public static bool HideCompass { get; set; }
        public static bool HideVitals { get; set; }
        public static bool HideCrosshair { get; set; }
        public static bool HideInteractionModeIcon { get; set; }
        public static bool HideActiveSpells { get; set; }
        public static bool HideArrowCount { get; set; }
        public static bool HideBreathBar { get; set; }
        public static bool HidePopupText { get; set; }
        public static bool HideMidScreenText { get; set; }
        public static bool HideEscortingFaces { get; set; }
        public static bool HideLocalQuestPlaces { get; set; }

        // Misc Options
        public static bool AllowKeyPressQuickToggle { get; set; }
        public static KeyCode QuickToggleKey { get; set; }

        // Variables
        public static bool[] hudElements = { false, false, false, false, false, false, false, false, false, false, false }; // Compass, Vitals, Crosshair, InteractionModeIcon, ActiveSpells, ArrowCount, BreathBar, PopupText, MidScreenText, EscortingFaces, LocalQuestPlaces
        public static bool QuickToggleState { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<HUDBeGoneMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: HUD Be Gone");

            Instance = this;

            QuickToggleState = false;

            mod.LoadSettings();

            StartGameBehaviour.OnStartGame += RefreshHUDVisibility_OnStartGame;
            SaveLoadManager.OnLoad += RefreshHUDVisibility_OnSaveLoad;

            Debug.Log("Finished mod init: HUD Be Gone");
        }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            AllowHiding = mod.GetSettings().GetValue<bool>("GeneralSettings", "AllowHudHiding");

            HideEverything = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideEverything");
            HideCompass = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideCompass");
            HideVitals = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideVitals");
            HideCrosshair = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideCrosshair");
            HideInteractionModeIcon = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideInteractionModeIcon");
            HideActiveSpells = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideActiveSpells");
            HideArrowCount = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideArrowCount");
            HideBreathBar = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideBreathBar");
            HidePopupText = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HidePopupText");
            HideMidScreenText = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideMidScreenText");
            HideEscortingFaces = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideEscortingFaces");
            HideLocalQuestPlaces = mod.GetSettings().GetValue<bool>("HudHidingSettings", "HideLocalQuestPlaces");

            AllowKeyPressQuickToggle = mod.GetSettings().GetValue<bool>("MiscSettings", "EnableKeyPressQuickToggle");
            var quickToggleKeyText = mod.GetSettings().GetValue<string>("MiscSettings", "QuickToggleKey");
            if (Enum.TryParse(quickToggleKeyText, out KeyCode result))
                QuickToggleKey = result;
            else
            {
                QuickToggleKey = KeyCode.G;
                Debug.Log("HUD Be Gone: Invalid quick toggle keybind detected. Setting default. 'G' Key");
                DaggerfallUI.AddHUDText("HUD Be Gone:", 6f);
                DaggerfallUI.AddHUDText("Invalid quick toggle keybind detected. Setting default. 'G' Key", 6f);
            }

            if (AllowHiding)
            {
                if (HideEverything)
                {
                    for (int i = 0; i < hudElements.Length; i++)
                    {
                        hudElements[i] = true;
                    }
                }
                else
                {
                    hudElements[0] = HideCompass ? true : false;
                    hudElements[1] = HideVitals ? true : false;
                    hudElements[2] = HideCrosshair ? true : false;
                    hudElements[3] = HideInteractionModeIcon ? true : false;
                    hudElements[4] = HideActiveSpells ? true : false;
                    hudElements[5] = HideArrowCount ? true : false;
                    hudElements[6] = HideBreathBar ? true : false;
                    hudElements[7] = HidePopupText ? true : false;
                    hudElements[8] = HideMidScreenText ? true : false;
                    hudElements[9] = HideEscortingFaces ? true : false;
                    hudElements[10] = HideLocalQuestPlaces ? true : false;
                }
            }
            else
            {
                for (int i = 0; i < hudElements.Length; i++)
                {
                    hudElements[i] = false;
                }
            }

            if (AllowKeyPressQuickToggle && QuickToggleState)
            {
                ForceHUDVisible();
            }
            else
            {
                UpdateHUDVisibilityState();
            }
        }

        public static void RefreshHUDVisibility_OnStartGame(object sender, EventArgs e)
        {
            if (AllowKeyPressQuickToggle && QuickToggleState)
            {
                ForceHUDVisible();
            }
            else
            {
                QuickToggleState = false;
                UpdateHUDVisibilityState();
            }
        }

        public static void RefreshHUDVisibility_OnSaveLoad(SaveData_v1 saveData)
        {
            if (AllowKeyPressQuickToggle && QuickToggleState)
            {
                ForceHUDVisible();
            }
            else
            {
                UpdateHUDVisibilityState();
            }
        }

        public static void UpdateHUDVisibilityState()
        {
            DaggerfallHUD dfuHud = DaggerfallUI.Instance.DaggerfallHUD;

            dfuHud.ShowCompass = !hudElements[0];
            dfuHud.ShowVitals = !hudElements[1];
            dfuHud.ShowCrosshair = !hudElements[2];
            dfuHud.ShowInteractionModeIcon = !hudElements[3];
            dfuHud.ShowActiveSpells = !hudElements[4];
            dfuHud.ShowArrowCount = !hudElements[5];
            dfuHud.ShowBreathBar = !hudElements[6];
            dfuHud.ShowPopupText = !hudElements[7];
            dfuHud.ShowMidScreenText = !hudElements[8];
            dfuHud.ShowEscortingFaces = !hudElements[9];
            dfuHud.ShowLocalQuestPlaces = !hudElements[10];

            if (!DaggerfallUnity.Settings.Crosshair) { dfuHud.ShowCrosshair = false; }
            if (DaggerfallUnity.Settings.InteractionModeIcon.ToLower() == "none") { dfuHud.ShowInteractionModeIcon = false; }
            if (!DaggerfallUnity.Settings.EnableArrowCounter) { dfuHud.ShowArrowCount = false; }
        }

        public static void ForceHUDVisible()
        {
            DaggerfallHUD dfuHud = DaggerfallUI.Instance.DaggerfallHUD;

            dfuHud.ShowCompass = true;
            dfuHud.ShowVitals = true;
            dfuHud.ShowCrosshair = true;
            dfuHud.ShowInteractionModeIcon = true;
            dfuHud.ShowActiveSpells = true;
            dfuHud.ShowArrowCount = true;
            dfuHud.ShowBreathBar = true;
            dfuHud.ShowPopupText = true;
            dfuHud.ShowMidScreenText = true;
            dfuHud.ShowEscortingFaces = true;
            dfuHud.ShowLocalQuestPlaces = true;

            if (!DaggerfallUnity.Settings.Crosshair) { dfuHud.ShowCrosshair = false; }
            if (DaggerfallUnity.Settings.InteractionModeIcon.ToLower() == "none") { dfuHud.ShowInteractionModeIcon = false; }
            if (!DaggerfallUnity.Settings.EnableArrowCounter) { dfuHud.ShowArrowCount = false; }
        }

        private void Update()
        {
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            // Handle key presses
            if (AllowKeyPressQuickToggle && InputManager.Instance.GetAnyKeyDown() == QuickToggleKey)
            {
                if (!QuickToggleState)
                {
                    QuickToggleState = true;
                    ForceHUDVisible();
                }
                else
                {
                    QuickToggleState = false;
                    UpdateHUDVisibilityState();
                }
            }    
        }
    }
}
