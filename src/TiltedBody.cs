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

using Kopernicus.OnDemand;

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

        public Ring[] Rings { get; set; }

        public bool OnDemand
        {
            get
            {
                return Demand;
            }
        }
        public ScaledSpaceOnDemand Demand;

        private bool Initialized = false;

        public BodyTypes Type;

        public Vector3 RotationAxis
        {
            get
            {
                return Quaternion.Euler((float)Obliquity, (float)RightAscension, 0.0F) * Vector3.up;
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

            List<Ring> rings = new List<Ring>();
            foreach (Transform child in this.Body.scaledBody.transform)
            {
                Ring r = child.GetComponent<Ring>();
                if (r) rings.Add(r);
            }
            this.Rings = rings.ToArray();

            

            DontDestroyOnLoad(sb);

            TiltManager.Bodies.Add(this);
        }

        private void LateUpdate()
        {
            GameScenes scene = HighLogic.LoadedScene;

            if (scene != GameScenes.FLIGHT && scene != GameScenes.SPACECENTER && !MapView.MapIsEnabled) return;

            if (this.Body == null) return;

            if (Type == BodyTypes.Rocky || RotationAxis == Vector3.up)
            {
                return;
            }

            ScaledSpaceOnDemand ondemand = this.Body.scaledBody.GetComponent<ScaledSpaceOnDemand>();
            if(ondemand != null)
            {
                ondemand.isLoaded = true;
            }

            float angle = -(float)this.Body.rotationAngle + 230.32F;

            GameObject sb = ScaledTiltedBody.gameObject;


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
            if (OriginalScaledRenderer && ScaledTiltedMR)
            {
                int sizeOriginal = OriginalScaledRenderer.sharedMaterials.Length;
                int sizeTilted = ScaledTiltedMR.sharedMaterials.Length;

                if (
                    sizeOriginal != sizeTilted ||
                    (sizeOriginal > TiltManager.ScattererMaterialIndex && sizeTilted > TiltManager.ScattererMaterialIndex && OriginalScaledRenderer.sharedMaterials[TiltManager.ScattererMaterialIndex] != ScaledTiltedMR.sharedMaterials[TiltManager.ScattererMaterialIndex])
                    )
                {
                    ScaledTiltedMR.sharedMaterials = OriginalScaledRenderer.sharedMaterials;
                }
            }
        }

        private void OnSceneChange(Scene scene, LoadSceneMode mode)
        {
            if(!Initialized && HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                MeshFilter mf = this.ScaledTiltedBody.AddComponent<MeshFilter>();
                mf.sharedMesh = this.Body.scaledBody.GetComponent<MeshFilter>().sharedMesh;

                ScaledTiltedMR = this.ScaledTiltedBody.AddComponent<MeshRenderer>();
                OriginalScaledRenderer = this.Body.scaledBody.GetComponent<MeshRenderer>();

                ScaledTiltedMR.sharedMaterials = OriginalScaledRenderer.sharedMaterials;
                ScaledSpaceOnDemand originalDemand = this.Body.scaledBody.GetComponent<ScaledSpaceOnDemand>();

                if (originalDemand != null)
                {
                    Demand = this.ScaledTiltedBody.AddComponent<ScaledSpaceOnDemand>();

                    Demand.texture = originalDemand.texture;
                    Demand.normals = originalDemand.normals;
                }

                Destroy(originalDemand);

                for (int i = 0; i < Rings.Length; i++)
                {
                    Rings[i].transform.SetParent(this.ScaledTiltedBody.transform);
                    Vector3 rot = Rings[i].rotation.eulerAngles;
                    Rings[i].rotation = Quaternion.Euler(rot.x, rot.y + Rings[i].longitudeOfAscendingNode * 2, rot.z);
                    Rings[i].transform.localRotation = Rings[i].rotation;
                }

                this.Body.scaledBody.layer = 26;
            }
        }

        public void GlobalToLocalOrbit(Orbit orbit)
        {

        }
    }
}
