using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("")]
public class VolumetricLightDirectionalSync : MonoBehaviour {

    public Light directionalLight;

    Light fakeLight;

    private void OnEnable() {
        fakeLight = GetComponent<Light>();
    }


    void LateUpdate() {
        if (directionalLight != null) {
            transform.forward = directionalLight.transform.forward;
            fakeLight.color = directionalLight.color;
            fakeLight.intensity = directionalLight.intensity;
        }

    }
}
