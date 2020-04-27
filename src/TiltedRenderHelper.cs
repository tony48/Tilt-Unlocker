using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiltUnlocker
{
    public class TiltedRenderHelper : MonoBehaviour
    {
        public static Callback PreRender;
        public static Callback PostRender;

        private void OnPreRender()
        {
            PreRender?.Invoke();
        }

        private void OnPostRender()
        {
            PostRender?.Invoke();
        }
    }
}
