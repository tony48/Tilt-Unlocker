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
using ModularFI;

namespace TiltUnlocker
{
    [DefaultExecutionOrder(1000)]
    public class TiltedBody : MonoBehaviour
    {
        public GameObject ScaledTiltedBody { get; private set; }
        public MeshRenderer ScaledTiltedMR;
        public GameObject ScaledBody { get; private set; }
        private MeshRenderer OriginalScaledRenderer;
        
        public GameObject NewScattererObject { get; private set; }
        public MeshRenderer NewScattererObjectMR { get; private set; }
        public MeshFilter NewScattererObjectMF { get; private set; }

        public SphereCollider SunBlocker { get; private set; }

        public Quaternion TiltedRotation { get; private set; }

        public CelestialBody Body { get; private set; }

        public Double Obliquity = 0.0F;
        public Double RightAscension = 0.0F;
        public bool RotateOrbits = false;

        public Ring[] Rings { get; set; }

        private static CelestialBody _Kerbin { get; set; }

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

            GameObject stb = this.ScaledTiltedBody = new GameObject(this.Body.name + " Tilted");
            stb.transform.parent = this.Body.scaledBody.transform.parent;

            if (this.Type != BodyTypes.Star)
            {
                Collider baseCollider = ScaledBody.GetComponent<Collider>();
                
                SunBlocker = stb.AddComponent<SphereCollider>();
                SunBlocker.sharedMaterial = baseCollider.sharedMaterial;
                SunBlocker.radius = 1000.0F;

                Destroy(baseCollider);
            }

            stb.transform.localScale = ScaledBody.transform.localScale;
            stb.layer = GameLayers.SCALED_SPACE;

            if (this.Body.pqsController)
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

            DontDestroyOnLoad(stb);

            TiltManager.Bodies.Add(this);
        }

        private void LateUpdate()
        {
            GameScenes scene = HighLogic.LoadedScene;

            if (scene != GameScenes.FLIGHT && scene != GameScenes.SPACECENTER && !MapView.MapIsEnabled) return;

            if (this.Body == null) return;

            ScaledSpaceOnDemand ondemand = this.Body.scaledBody.GetComponent<ScaledSpaceOnDemand>();
            if(ondemand != null)
            {
                ondemand.isLoaded = true;
            }

            GameObject stb = ScaledTiltedBody.gameObject;
            GameObject sb = ScaledBody.gameObject;

            if(!ScaledBody)
            {
                ScaledBody = this.Body.scaledBody;

                if (!ScaledBody) return;
            }


            stb.transform.position = sb.transform.position;

            //Make tilt correctly. The universe seems to be kerbinocentered, eh.
            stb.transform.up = _Kerbin.scaledBody.transform.TransformDirection(this.RotationAxis);
            stb.transform.Rotate(Vector3.up, (float)_Kerbin.rotationAngle, Space.World);
            //Then *actually*, *finally* rotate it.
            stb.transform.Rotate(Vector3.up, -(float)_Kerbin.rotationAngle + sb.transform.rotation.eulerAngles.y, Space.Self);

            UpdateMaterials();

            Vector3 dir = (ScaledTiltedBody.transform.position - KopernicusStar.GetNearest(this.Body).sun.scaledBody.transform.position).normalized;
            dir = ScaledTiltedBody.transform.worldToLocalMatrix * dir;
            ScaledTiltedMR.sharedMaterials[TiltManager.StockMaterialIndex].SetVector("_localLightDirection", dir);

            
        }

        private void UpdateMaterials()
        {
            if (TiltManager.ScattererInstalled)
            {
                if (OriginalScaledRenderer && ScaledTiltedMR)
                {
                    int sizeOriginal = OriginalScaledRenderer.sharedMaterials.Length;
                    int sizeTilted = ScaledTiltedMR.sharedMaterials.Length;

                    if ( // scatterer in loading range and old version.
                        TiltManager.ScattererVersion != TiltManager.NewScattererVersion &&
                        sizeOriginal != sizeTilted ||
                        (sizeOriginal > TiltManager.ScattererMaterialIndex &&
                         sizeTilted > TiltManager.ScattererMaterialIndex &&
                         OriginalScaledRenderer.sharedMaterials[TiltManager.ScattererMaterialIndex] !=
                         ScaledTiltedMR.sharedMaterials[TiltManager.ScattererMaterialIndex])
                    )
                    {
                        if (TiltManager.ScattererVersion.Minor == 0)
                            TiltManager.ScattererVersion = TiltManager.OldScattererVersion;
                        ScaledTiltedMR.sharedMaterials = OriginalScaledRenderer.sharedMaterials;
                    }
                    else if (NewScattererObject != null) // new scatterer object already found, keep track of it and change it if needed
                    {
                        bool isInScaledSpace = NewScattererObject.transform.parent == ScaledBody.transform;

                        SetNewScattererState(isInScaledSpace);
                    }
                    else if(TiltManager.ScattererVersion != TiltManager.OldScattererVersion) // either not in loading range or new version.
                    {
                        // new scatterer includes a "New GameObject" (lol) child to the original MR
                        // as the new atmosphere mesh / material. This gameobject moves from local
                        // to scaled space depending of the situation.

                        GameObject potentialAtmoObject = null;
                        bool foundObject = false;
                        
                        
                        foreach (Transform child in ScaledBody.transform)
                            if (child.name == "New Game Object")
                            {
                                potentialAtmoObject = child.gameObject;
                                foundObject = true;
                                break;
                            }

                        if (foundObject)
                        {
                            MeshRenderer scatmr = potentialAtmoObject.GetComponent<MeshRenderer>();

                            if (scatmr != null)
                            { // scatterer object
                                if (TiltManager.ScattererVersion.Minor == 0)
                                    TiltManager.ScattererVersion = TiltManager.NewScattererVersion;
                                
                                NewScattererObject = potentialAtmoObject;
                                NewScattererObjectMR = scatmr;
                                NewScattererObjectMF = potentialAtmoObject.GetComponent<MeshFilter>();

                                SetNewScattererState(true);
                            }
                        }
                    }
                }
            }
        }

        private void SetNewScattererState(bool scaled)
        {
            if (scaled)
            {
                ScaledTiltedMR.sharedMaterials = new []
                {
                    ScaledTiltedMR.sharedMaterials[0],
                    NewScattererObjectMR.sharedMaterials[0]
                };

                NewScattererObjectMR.enabled = false;
            }
            else
            {
                ScaledTiltedMR.sharedMaterials = new[]
                {
                    ScaledTiltedMR.sharedMaterials[0]
                };
                
                NewScattererObjectMR.enabled = true;
            }
        }

        private void OnSceneChange(Scene scene, LoadSceneMode mode)
        {     
            if(!Initialized && HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                _Kerbin = FlightGlobals.GetHomeBody();//FlightGlobals.Bodies.Find(cb => cb.isHomeWorld == "Kerbin");
                
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

                List<Ring> rings = new List<Ring>();
                foreach (Transform child in ScaledBody.transform)
                {
                    Ring r = child.GetComponent<Ring>();
                    if (r) rings.Add(r);
                }
                this.Rings = rings.ToArray();

                for (int i = 0; i < Rings.Length; i++)
                {
                    Rings[i].transform.SetParent(this.ScaledTiltedBody.transform);
                    Rings[i].transform.localPosition = Vector3.zero;
                    Vector3 rot = Rings[i].rotation.eulerAngles;
                    Rings[i].rotation = Quaternion.Euler(rot.x, rot.y + Rings[i].longitudeOfAscendingNode * 2, rot.z);
                    Rings[i].transform.localRotation = Rings[i].rotation;
                }

                this.ScaledBody.layer = 26;
                Initialized = true;
            }
        }
    }
}
