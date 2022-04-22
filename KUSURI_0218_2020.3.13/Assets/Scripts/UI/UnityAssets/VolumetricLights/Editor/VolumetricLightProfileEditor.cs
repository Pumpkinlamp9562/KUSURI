using UnityEngine;
using UnityEditor;

namespace VolumetricLights {

    [CustomEditor(typeof(VolumetricLightProfile))]
    public partial class VolumetricLightProfileEditor : Editor {

        SerializedProperty blendMode, raymarchPreset, raymarchQuality, raymarchMinStep, dithering, jittering, useBlueNoise, renderQueue, sortingLayerID, sortingOrder, flipDepthTexture, alwaysOn;
        SerializedProperty autoToggle, distanceStartDimming, distanceDeactivation, autoToggleCheckInterval;
        SerializedProperty useNoise, noiseTexture, noiseStrength, noiseScale, noiseFinalMultiplier, density, mediumAlbedo, brightness;
        SerializedProperty attenuationMode, attenCoefConstant, attenCoefLinear, attenCoefQuadratic, rangeFallOff, diffusionIntensity, penumbra;
        SerializedProperty tipRadius, cookieTexture, frustumAngle, windDirection;
        SerializedProperty enableDustParticles, dustBrightness, dustMinSize, dustMaxSize, dustDistanceAttenuation, dustWindSpeed, dustAutoToggle, dustDistanceDeactivation;
        SerializedProperty enableShadows, shadowIntensity, shadowResolution, shadowCullingMask, shadowBakeInterval, shadowNearDistance, shadowAutoToggle, shadowDistanceDeactivation, shadowOrientation, shadowDirection;

        void OnEnable() {

            if (target == null) return;

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
        }


        public override void OnInspectorGUI() {

            serializedObject.Update();

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

            EditorGUILayout.PropertyField(tipRadius);
            EditorGUILayout.PropertyField(cookieTexture, new GUIContent("Cookie Texture (RGB)", "Assign any colored or grayscale texture. RGB values drive the color tint."));
            EditorGUILayout.PropertyField(frustumAngle);

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
                EditorGUILayout.PropertyField(shadowOrientation, new GUIContent("Orientation"));
                if (shadowOrientation.intValue == (int)ShadowOrientation.FixedDirection) {
                    EditorGUILayout.PropertyField(shadowDirection, new GUIContent("Direction"));
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

            serializedObject.ApplyModifiedProperties();
        }

      
    }

}