using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.UI.Screens;
using CommNet;

namespace TiltUnlocker
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class TiltApp : MonoBehaviour
    {
        private static ApplicationLauncherButton AppButton { get; set; } = null;

        public static TiltApp Instance { get; private set; }
        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
            }

            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            //AddAppButton();
        }

        private void OnDestroy()
        {
            if (AppButton)
            {
                ApplicationLauncher.Instance.RemoveModApplication(AppButton);
                Destroy(AppButton);
            }

            Instance = null;
        }

        private void AddAppButton()
        {
            if (AppButton != null) return;

            Debug.Log("[Tilt] Creating app");

            GameScenes scene = HighLogic.LoadedScene;

            if (scene == GameScenes.FLIGHT || scene == GameScenes.TRACKSTATION)
            {
                Texture tex = GameDatabase.Instance.GetTexture("TiltUnlocker/Icon/test-icon", false);
                AppButton = ApplicationLauncher.Instance.AddModApplication(
                    EnableWindow, DisableWindow, null,null, null, null, ApplicationLauncher.AppScenes.ALWAYS, tex);
            }
        }


        private void EnableWindow()
        {
            WindowEnabled = true;
        }

        private void DisableWindow()
        {
            WindowEnabled = false;
        }

        private bool WindowEnabled = false;
        private Rect WindowRect = new Rect(20, 20, 120, 120);
        public static bool DebugMode = false;
        //private VectorLine AxisLine;
        private Material DebugMaterial;

        private void OnGUI()
        {
            if (!WindowEnabled) return;

            WindowRect = GUI.Window(0, WindowRect, DrawWindow, "Tilt Unlocker");

            if(DebugMode)
            {
            }
        }

        private void DrawWindow(int id)
        {
            DebugMode = GUI.Toggle(new Rect(10, 20, 100, 20), DebugMode, "Debug");
            GUI.DragWindow();
        }
    }
}
