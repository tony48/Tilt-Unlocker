using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.UI.Screens;
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
        /// <summary>
        /// Pre scatterer 0.0722 material index
        /// </summary>
        public const int ScattererMaterialIndex = 1;

        public static Version ScattererVersion { get; internal set; }= new Version(0,0,0, "Undetermined");

        public static Version OldScattererVersion { get; internal set; } = new Version(0,0,632, "Old Scatterer");
        public static Version NewScattererVersion { get; internal set; } = new Version(0,0,722, "New Scatterer");
        
        public static bool ScattererInstalled { get; private set; }
        
        public bool NewScatterer => ScattererVersion >= NewScattererVersion;
        
        private void Awake()
        {
            if (Instance) Destroy(this);

            Instance = this;
            DontDestroyOnLoad(this);

            ScattererInstalled = GameObject.Find("Scatterer") != null;
        }
    }
}
