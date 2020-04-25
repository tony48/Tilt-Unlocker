using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiltUnlocker
{
    
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class TiltManager : MonoBehaviour
    {
        public static TiltManager Instance { get; private set; }

        public static List<TiltedBody> Bodies { get; private set; } = new List<TiltedBody>(17);

        private void Awake()
        {
            if (Instance) Destroy(this);

            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Active tilted bodies: " + Bodies.Count);
            foreach (TiltedBody tb in Bodies)
            {
                GUILayout.Label(tb.name + " " + tb.Obliquity + "°" + " => " + tb.RotationAxis);
            }

            GUILayout.EndVertical();
        }
    }
}
