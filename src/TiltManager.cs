using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.UI.Screens;
using DebugStuff;
using Kopernicus.Components;

using UnityEngine.SceneManagement;

namespace TiltUnlocker
{
    
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class TiltManager : MonoBehaviour
    {
        public static TiltManager Instance { get; private set; }

        public static List<TiltedBody> Bodies { get; private set; } = new List<TiltedBody>(17);

        public const int StockMaterialIndex = 0;
        public const int ScattererMaterialIndex = 1;

        private void Awake()
        {
            if (Instance) Destroy(this);

            Instance = this;
            DontDestroyOnLoad(this);

            SceneManager.sceneLoaded += OnSceneChange;
        }

        private void OnSceneChange(Scene scene, LoadSceneMode mode)
        {
            if(HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                Camera.main.gameObject.AddComponent<TiltedRenderHelper>();
            }
        }
    }
}
