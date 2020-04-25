using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Kopernicus;
using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus.Constants;
using Kopernicus.RuntimeUtility;

using UnityEngine.SceneManagement;

namespace TiltUnlocker
{
    [DefaultExecutionOrder(100)]
    public class TiltedBody : MonoBehaviour
    {
        public GameObject ScaledTiltedBody { get; private set; }
        public MeshRenderer ScaledTiltedMR;
        public GameObject ScaledBody { get; private set; }
        private MeshRenderer OriginalScaledRenderer;

        public CelestialBody Body { get; private set; }

        public Double Obliquity = 0.0F;
        public Double RightAscension = 0.0F;

        

        public BodyTypes Type;

        public Vector3 RotationAxis
        {
            get
            {
                Vector3 rot = Vector3.up;
                rot = Quaternion.Euler((float)Obliquity, 0.0F, 0.0F) * rot;
                rot = Quaternion.Euler(0.0F, (float)RightAscension, 0.0F) * rot;
                return rot;
            }
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneChange;
        }

        private void Start()
        {
            this.Body = gameObject.GetComponent<CelestialBody>();

            ScaledBody = this.Body.scaledBody;
            GameObject sb = this.ScaledTiltedBody = new GameObject(this.Body.name + " Tilted");
            sb.layer = GameLayers.SCALED_SPACE;

            sb.transform.parent = ScaledBody.transform;
            sb.transform.localScale = Vector3.one;
            sb.transform.localPosition = Vector3.zero;
            sb.transform.localRotation = Quaternion.identity;


            if(this.Body.pqsController)
            {
                Type = BodyTypes.Rocky;
            }
            else
            {
                if (this.Body.scaledBody.GetComponent<StarComponent>())
                {
                    Type = BodyTypes.Star;
                }

                else
                {
                    Type = BodyTypes.Gas;
                }
            }

            // TODO: rocky bodies. haha.
            /*if(this.Type == BodyTypes.Rocky)
            {
                Destroy(sb);
                Destroy(this);
            }*/

            DontDestroyOnLoad(sb);

            TiltManager.Bodies.Add(this);
        }

        private static int MaterialSizes = 1;

        private void LateUpdate()
        {
            if (Type == BodyTypes.Rocky || RotationAxis == Vector3.up)
            {
                return;
            }

            float angle = -(float)this.Body.rotationAngle + 230.32F;

            GameObject sb = ScaledTiltedBody.gameObject;

            if (OriginalScaledRenderer && OriginalScaledRenderer.enabled)
                OriginalScaledRenderer.enabled = false;

            if(!ScaledBody)
            {
                ScaledBody = this.Body.scaledBody;

                if (!ScaledBody) return;
            }
            
            sb.transform.rotation = Quaternion.identity;
            sb.transform.up = this.RotationAxis;
            sb.transform.Rotate(Vector3.up, angle, Space.Self);

            UpdateMaterials();

            
            Vector3 dir = (ScaledTiltedBody.transform.position - KopernicusStar.GetNearest(this.Body).sun.scaledBody.transform.position).normalized;
            dir = ScaledTiltedBody.transform.worldToLocalMatrix * dir;
            ScaledTiltedMR.sharedMaterials[TiltManager.StockMaterialIndex].SetVector("_localLightDirection", dir);
        }

        private void UpdateMaterials()
        {
            int sizeOriginal = OriginalScaledRenderer.sharedMaterials.Length;
            int sizeTilted = ScaledTiltedMR.sharedMaterials.Length;

            //Debug.Log("[Tilt] " + this.Body.name + " Original: " + OriginalScaledRenderer.sharedMaterials.Length + " / Tilted: "  + ScaledTiltedMR.sharedMaterials.Length);
            if(
                sizeOriginal != sizeTilted || 
                (sizeOriginal > TiltManager.ScattererMaterialIndex && sizeTilted > TiltManager.ScattererMaterialIndex && OriginalScaledRenderer.sharedMaterials[TiltManager.ScattererMaterialIndex] != ScaledTiltedMR.sharedMaterials[TiltManager.ScattererMaterialIndex])
                )
            {
                ScaledTiltedMR.sharedMaterials = OriginalScaledRenderer.sharedMaterials;
            }
        }

        private void OnSceneChange(Scene scene, LoadSceneMode mode)
        {
            if(HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                MeshFilter mf = this.ScaledTiltedBody.AddComponent<MeshFilter>();
                mf.sharedMesh = this.Body.scaledBody.GetComponent<MeshFilter>().sharedMesh;

                ScaledTiltedMR = this.ScaledTiltedBody.AddComponent<MeshRenderer>();
                OriginalScaledRenderer = this.Body.scaledBody.GetComponent<MeshRenderer>();
                ScaledTiltedMR.sharedMaterials = OriginalScaledRenderer.sharedMaterials;
                OriginalScaledRenderer.enabled = false;
            }
        }
    }
}
