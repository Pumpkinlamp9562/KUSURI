using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

namespace VolumetricLights {

    [CustomEditor(typeof(VolumetricLight))]
    public partial class VolumetricLightEditor : Editor {

        SerializedProperty profile, profileSync;

        SerializedProperty blendMode, raymarchPreset, raymarchQuality, raymarchMinStep, dithering, jittering, useBlueNoise, renderQueue, sortingLayerID, sortingOrder, flipDepthTexture, alwaysOn;
        SerializedProperty autoToggle, distanceStartDimming, distanceDeactivation, autoToggleCheckInterval;
        SerializedProperty useNoise, noiseTexture, noiseStrength, noiseScale, noiseFinalMultiplier, density, mediumAlbedo, brightness;
        SerializedProperty attenuationMode, attenCoefConstant, attenCoefLinear, attenCoefQuadratic, rangeFallOff, diffusionIntensity, penumbra;
        SerializedProperty tipRadius, cookieTexture, frustumAngle, windDirection;
        SerializedProperty enableDustParticles, dustBrightness, dustMinSize, dustMaxSize, dustDistanceAttenuation, dustWindSpeed, dustAutoToggle, dustDistanceDeactivation;
        SerializedProperty enableShadows, shadowIntensity, shadowResolution, shadowCullingMask, shadowBakeInterval, shadowNearDistance, shadowAutoToggle, shadowDistanceDeactivation, shadowOrientation, shadowDirection;

        SerializedProperty useCustomBounds, bounds;
        SerializedProperty areaWidth, areaHeight;
        SerializedProperty customRange, useCustomSize;
        SerializedProperty targetCamera;

        static GUIStyle boxStyle;
        bool profileChanged, enableProfileApply;
        VolumetricLight vl;

        void OnEnable() {

            vl = (VolumetricLight)target;
            if (vl.lightComp == null) {
                vl.Init();
            }

            profile = serializedObject.FindProperty("profile");
            profileSync = serializedObject.FindProperty("profileSync");

            blendMode = serializedObject.FindProperty("blendMode");
            raymarchPreset = serializedObject.FindProperty("raymarchPreset");
            raymarchQuality = serializedObject.FindProperty("raymarchQuality");
            raymarchMinStep = serializedObject.FindProperty("raymarchMinStep");
            dithering = serializedObject.FindProperty("dithering");
            jittering = serializedObject.FindProperty("jittering");
            useBlueNoise = serializedObject.FindProperty("useBlueNoise");
            renderQueue = serializedObject.FindProperty("renderQueue");
            sortingLayerID = serializedObject.FindProperty("sortingLayerID");
            sortingOrder = serializedObject.FindProperty("sortingOrder");
            flipDepthTexture = serializedObject.FindProperty("flipDepthTexture");
            alwaysOn = serializedObject.FindProperty("alwaysOn");
            useNoise = serializedObject.FindProperty("useNoise");
            noiseTexture = serializedObject.FindProperty("noiseTexture");
            noiseStrength = serializedObject.FindProperty("noiseStrength");
            noiseScale = serializedObject.FindProperty("noiseScale");
            noiseFinalMultiplier = serializedObject.FindProperty("noiseFinalMultiplier");
            density = serializedObject.FindProperty("density");
            mediumAlbedo = serializedObject.FindProperty("mediumAlbedo");
            brightness = serializedObject.FindProperty("brightness");
            attenuationMode = serializedObject.FindProperty("attenuationMode");
            attenCoefConstant = serializedObject.FindProperty("attenCoefConstant");
            attenCoefLinear = serializedObject.FindProperty("attenCoefLinear");
            attenCoefQuadratic = serializedObject.FindProperty("attenCoefQuadratic");
            rangeFallOff = serializedObject.FindProperty("rangeFallOff");
            diffusionIntensity = serializedObject.FindProperty("diffusionIntensity");
            penumbra = serializedObject.FindProperty("penumbra");
            tipRadius = serializedObject.FindProperty("tipRadius");
            cookieTexture = serializedObject.FindProperty("cookieTexture");
            frustumAngle = serializedObject.FindProperty("frustumAngle");
            windDirection = serializedObject.FindProperty("windDirection");
            enableDustParticles = serializedObject.FindProperty("enableDustParticles");
            dustBrightness = serializedObject.FindProperty("dustBrightness");
            dustMinSize = serializedObject.FindProperty("dustMinSize");
            dustMaxSize = serializedObject.FindProperty("dustMaxSize");
            dustWindSpeed = serializedObject.FindProperty("dustWindSpeed");
            dustDistanceAttenuation = serializedObject.FindProperty("dustDistanceAttenuation");
            dustAutoToggle = serializedObject.FindProperty("dustAutoToggle");
            dustDistanceDeactivation = serializedObject.FindProperty("dustDistanceDeactivation");
            enableShadows = serializedObject.FindProperty("enableShadows");
            shadowIntensity = serializedObject.FindProperty("shadowIntensity");
            shadowResolution = serializedObject.FindProperty("shadowResolution");
            shadowCullingMask = serializedObject.FindProperty("shadowCullingMask");
            shadowBakeInterval = serializedObject.FindProperty("shadowBakeInterval");
            shadowNearDistance = serializedObject.FindProperty("shadowNearDistance");
            shadowAutoToggle = serializedObject.FindProperty("shadowAutoToggle");
            shadowDistanceDeactivation = serializedObject.FindProperty("shadowDistanceDeactivation");
            shadowOrientation = serializedObject.FindProperty("shadowOrientation");
            shadowDirection = serializedObject.FindProperty("shadowDirection");
            autoToggle = serializedObject.FindProperty("autoToggle");
            distanceDeactivation = serializedObject.FindProperty("distanceDeactivation");
            distanceStartDimming = serializedObject.FindProperty("distanceStartDimming");
            autoToggleCheckInterval = serializedObject.FindProperty("autoToggleCheckInterval");

            useCustomBounds = serializedObject.FindProperty("useCustomBounds");
            bounds = serializedObject.FindProperty("bounds");
            useCustomSize = serializedObject.FindProperty("useCustomSize");
            areaWidth = serializedObject.FindProperty("areaWidth");
            areaHeight = serializedObject.FindProperty("areaHeight");
            customRange = serializedObject.FindProperty("customRange");
            targetCamera = serializedObject.FindProperty("targetCamera");
        }


        public override void OnInspectorGUI() {

            if (vl.lightComp != null && vl.lightComp.type == LightType.Directional) {
                EditorGUILayout.HelpBox("Volumetric Lights works with Point, Spot and Area lights only.\nYou can place a volumetric area light in the desired area instead or use Volumetric Fog & Mist asset for other fog related effects.", MessageType.Info);
                if (GUILayout.Button("Create a Volumetric Area Light")) {
                    CreateVolumetricAreaLight(vl.lightComp);
                }
                if (GUILayout.Button("Volumetric Fog & Mist information")) {
                    Application.OpenURL("https://assetstore.unity.com/packages/slug/162694?aid=1101lGsd");
                }
                return;
            }

            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset pipe = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipe == null) {
                EditorGUILayout.HelpBox("Universal Rendering Pipeline asset is not set in Project Settings / Graphics !", MessageType.Error);
                return;
            }

            if (!pipe.supportsCameraDepthTexture) {
                EditorGUILayout.HelpBox("Depth Texture option is required in Universal Rendering Pipeline asset!", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
                GUI.enabled = false;
            } else if (!pipe.supportsHDR && pipe.msaaSampleCount == 1 && pipe.renderScale == 1f && !vl.profile.flipDepthTexture) {
                EditorGUILayout.HelpBox("Depth Texture might be inverted due to current pipeline setup. To fix depth texture orientation, enable Flip Depth Texture option in the Volumetric Light profile or enable HDR or MSAA in Universal Rendering Pipeline asset.", MessageType.Error);
                if (GUILayout.Button("Go to Universal Rendering Pipeline Asset")) {
                    Selection.activeObject = pipe;
                }
                EditorGUILayout.Separator();
            }

            if (VolumetricLightsRenderFeature.installed) {
                if (GUILayout.Button("Show Global Settings")) {
                    if (pipe != null) {
                        var so = new SerializedObject(pipe);
                        var prop = so.FindProperty("m_RendererDataList");
                        if (prop != null && prop.arraySize > 0) {
                            var o = prop.GetArrayElementAtIndex(0);
                            if (o != null) {
                                Selection.SetActiveObjectWithContext(o.objectReferenceValue, null);
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                }
            }


            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(15, 10, 5, 5);
            }

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            VolumetricLightProfile prevProfile = (VolumetricLightProfile)profile.objectReferenceValue;
            EditorGUILayout.PropertyField(profile, new GUIContent("Profile", "Create or load stored presets."));

            if (profile.objectReferenceValue != null) {

                if (prevProfile != profile.objectReferenceValue) {
                    profileChanged = true;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button(new GUIContent("Create", "Creates a new profile which is a copy of the current values."), GUILayout.Width(60))) {
                    CreateProfile();
                    profileChanged = false;
                    enableProfileApply = false;
                    GUIUtility.ExitGUI();
                    return;
                }
                if (GUILayout.Button(new GUIContent("Load", "Updates volumetric light settings with the profile values."), GUILayout.Width(60))) {
                    vl.profile.ApplyTo(vl);
                    EditorUtility.SetDirty(vl);
                    serializedObject.Update();
                    profileChanged = true;
                }
                if (!enableProfileApply)
                    GUI.enabled = false;
                if (GUILayout.Button(new GUIContent("Save", "Updates profile values with changes in this inspector."), GUILayout.Width(60))) {
                    enableProfileApply = false;
                    profileChanged = false;
                    vl.profile.LoadFrom(vl);
                    EditorUtility.SetDirty(vl.profile);
                    GUIUtility.ExitGUI();
                    return;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(profileSync, new GUIContent("Sync With Profile", "If disabled, profile settings will only be loaded when clicking 'Load' which allows you to customize settings after loading a profile and keep those changes."));
                if (profileSync.boolValue) GUI.enabled = false;
                EditorGUILayout.BeginHorizontal();
            } else {
                if (GUILayout.Button(new GUIContent("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width(60))) {
                    CreateProfile();
                    GUIUtility.ExitGUI();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(blendMode);
            EditorGUILayout.PropertyField(raymarchPreset);
            if (raymarchPreset.intValue != (int)RaymarchPresets.UserDefined) {
                EditorGUI.indentLevel++;
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(raymarchQuality);
            EditorGUILayout.PropertyField(raymarchMinStep);
            EditorGUILayout.PropertyField(jittering);
            if (EditorGUI.EndChangeCheck()) {
                raymarchPreset.intValue = (int)RaymarchPresets.UserDefined;
            }
            if (raymarchPreset.intValue != (int)RaymarchPresets.UserDefined) {
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(dithering);
            EditorGUILayout.PropertyField(useBlueNoise);
            EditorGUILayout.PropertyField(renderQueue);
            EditorGUILayout.PropertyField(sortingLayerID);
            EditorGUILayout.PropertyField(sortingOrder);
            EditorGUILayout.PropertyField(flipDepthTexture);
            EditorGUILayout.PropertyField(alwaysOn);
            EditorGUILayout.PropertyField(autoToggle, new GUIContent("Auto Toggle"));
            if (autoToggle.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(distanceStartDimming, new GUIContent("Distance Start Dimming"));
                EditorGUILayout.PropertyField(distanceDeactivation, new GUIContent("Distance Deactivation"));
                EditorGUILayout.PropertyField(autoToggleCheckInterval, new GUIContent("Check Time Interval"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(useNoise);
            if (useNoise.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(noiseTexture);
                EditorGUILayout.PropertyField(noiseStrength, new GUIContent("Strength"));
                EditorGUILayout.PropertyField(noiseScale, new GUIContent("Scale"));
                EditorGUILayout.PropertyField(noiseFinalMultiplier, new GUIContent("Final Multiplier"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(density);
            EditorGUILayout.PropertyField(mediumAlbedo);
            EditorGUILayout.PropertyField(brightness);

            EditorGUILayout.PropertyField(attenuationMode);
            if (attenuationMode.intValue == (int)AttenuationMode.Quadratic) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(attenCoefConstant, new GUIContent("Constant Coef"));
                EditorGUILayout.PropertyField(attenCoefLinear, new GUIContent("Linear Coef"));
                EditorGUILayout.PropertyField(attenCoefQuadratic, new GUIContent("Quadratic Coef"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(rangeFallOff);
            EditorGUILayout.PropertyField(diffusionIntensity);
            EditorGUILayout.PropertyField(penumbra);

            switch (vl.lightComp.type) {
                case LightType.Spot:
                    EditorGUILayout.PropertyField(tipRadius);
                    EditorGUILayout.PropertyField(cookieTexture, new GUIContent("Cookie Texture (RGB)", "Assign any colored or grayscale texture. RGB values drive the color tint."));
                    break;
                case LightType.Area:
                case LightType.Disc:
                    EditorGUILayout.PropertyField(frustumAngle);
                    break;
            }

            if (useNoise.boolValue) {
                EditorGUILayout.PropertyField(windDirection);
            }

            EditorGUILayout.PropertyField(enableDustParticles);
            if (enableDustParticles.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dustBrightness, new GUIContent("Brightness"));
                EditorGUILayout.PropertyField(dustMinSize, new GUIContent("Min Size"));
                EditorGUILayout.PropertyField(dustMaxSize, new GUIContent("Max Size"));
                EditorGUILayout.PropertyField(dustWindSpeed, new GUIContent("Wind Speed"));
                EditorGUILayout.PropertyField(dustDistanceAttenuation, new GUIContent("Distance Attenuation"));
                EditorGUILayout.PropertyField(dustAutoToggle, new GUIContent("Auto Toggle"));
                if (dustAutoToggle.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(dustDistanceDeactivation, new GUIContent("Distance"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(enableShadows);
            if (enableShadows.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(shadowIntensity, new GUIContent("Intensity"));
                EditorGUILayout.PropertyField(shadowResolution, new GUIContent("Resolution"));
                EditorGUILayout.PropertyField(shadowCullingMask, new GUIContent("Culling Mask"));
                EditorGUILayout.PropertyField(shadowBakeInterval, new GUIContent("Bake Interval"));
                if (vl.lightComp != null && vl.lightComp.type == LightType.Point) {
                    EditorGUILayout.PropertyField(shadowOrientation, new GUIContent("Orientation", "Only for point lights: specify the direction for the baked shadows (shadows are captured in a half sphere or 180º). You can choose a fixed direction or make the shadows be aligned with the direction to the player camera."));
                    if (shadowOrientation.intValue == (int)ShadowOrientation.FixedDirection) {
                        EditorGUILayout.PropertyField(shadowDirection, new GUIContent("Direction"));
                    }
                }
                EditorGUILayout.PropertyField(shadowNearDistance, new GUIContent("Near Clip Distance"));
                EditorGUILayout.PropertyField(shadowAutoToggle, new GUIContent("Auto Toggle"));
                if (shadowAutoToggle.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(shadowDistanceDeactivation, new GUIContent("Distance"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();

            GUI.enabled = true;

            // Additional options
            EditorGUILayout.PropertyField(targetCamera);

            if (vl.lightComp != null) {
                EditorGUILayout.PropertyField(useCustomSize);
                if (useCustomSize.boolValue) {
                    EditorGUI.indentLevel++;
                    switch (vl.lightComp.type) {
                        case LightType.Area:
                            EditorGUILayout.PropertyField(areaWidth, new GUIContent("Width"));
                            EditorGUILayout.PropertyField(areaHeight, new GUIContent("Height"));
                            break;
                        case LightType.Disc:
                            EditorGUILayout.PropertyField(areaWidth, new GUIContent("Radius"));
                            break;
                        case LightType.Spot:
                        case LightType.Point:
                            break;
                    }
                    EditorGUILayout.PropertyField(customRange, new GUIContent("Range"));
                    EditorGUI.indentLevel--;
                }
            }

            if (vl.ps != null) {
                if (GUILayout.Button("Select Particle System")) {
                    Selection.activeGameObject = vl.ps.gameObject;
                }
            }

            EditorGUILayout.PropertyField(useCustomBounds);
            if (useCustomBounds.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(bounds);
                EditorGUI.indentLevel--;
            }

            if (serializedObject.ApplyModifiedProperties()) {
                if (vl.profile != null) {
                    if (profileChanged) {
                        vl.profile.ApplyTo(vl);
                        profileChanged = false;
                        enableProfileApply = false;
                    } else {
                        enableProfileApply = true;
                    }
                }
            }
        }

        void CreateProfile() {
            string path = "Assets";
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null) {
#if UNITY_2020_3_OR_NEWER
                var prefabPath = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
#else
                var prefabPath = PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath;
#endif
                if (!string.IsNullOrEmpty(prefabPath)) {
                    path = Path.GetDirectoryName(prefabPath);
                }
            } else {
                foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets)) {
                    path = AssetDatabase.GetAssetPath(obj);
                    if (File.Exists(path)) {
                        path = Path.GetDirectoryName(path);
                    }
                    break;
                }
            }
            VolumetricLightProfile fp = CreateInstance<VolumetricLightProfile>();
            fp.LoadFrom(vl);
            fp.name = "New Volumetric Light Profile";
            string fullPath;
            int counter = 0;
            do {
                fullPath = path + "/" + fp.name;
                if (counter > 0) fullPath += " " + counter;
                fullPath += ".asset";
                counter++;
            } while (File.Exists(fullPath));
            AssetDatabase.CreateAsset(fp, fullPath);
            AssetDatabase.SaveAssets();

            serializedObject.Update();
            profile.objectReferenceValue = fp;
            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.PingObject(fp);
        }

        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        protected virtual void OnSceneGUI() {
            VolumetricLight vl = (VolumetricLight)target;
            if (!vl.useCustomBounds) return;

            m_BoundsHandle.center = vl.bounds.center;
            m_BoundsHandle.size = vl.bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(vl, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                vl.bounds = newBounds;
                vl.UpdateVolumeGeometry();
            }
        }

        void CreateVolumetricAreaLight(Light directionalLight) {
            GameObject go = new GameObject("Volumetric Area Light (Directional Light)", typeof(Light), typeof(VolumetricLightDirectionalSync));
            Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
            if (sceneCamera != null) {
                go.transform.position = sceneCamera.transform.TransformPoint(Vector3.forward * 50f);
            }
            Light light = go.GetComponent<Light>();
            light.type = LightType.Area;
            light.areaSize = new Vector2(50, 20);
            light.range = 20;
            light.enabled = false;
            VolumetricLight vl = go.AddComponent<VolumetricLight>();
            vl.density = 0.015f;
            vl.useNoise = false;
            vl.alwaysOn = true;
            vl.Refresh();
            VolumetricLightDirectionalSync dirSync = go.GetComponent<VolumetricLightDirectionalSync>();
            dirSync.directionalLight = directionalLight;
            VolumetricLight current = (VolumetricLight)target;
            DestroyImmediate(current);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
            GUIUtility.ExitGUI();
        }
    }

}