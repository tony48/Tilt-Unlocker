using System.Collections.Generic;
using UnityEngine;

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
        
        public static bool ScattererInstalled { get; private set; }
        
        private void Awake()
        {
            if (Instance) Destroy(this);

            Instance = this;
            DontDestroyOnLoad(this);

            ScattererInstalled = GameObject.Find("Scatterer") != null;
        }
    }
}
