using UnityEngine;
using System.Collections;

namespace uNature.Core.Settings
{
    /// <summary>
    /// A class which should be used on custom classes that needs to be shown and serialized as a setting.
    /// </summary>
    [System.Serializable]
    public class UNSetting
    {
        /// <summary>
        /// Draw the gui of the setting on this method.
        /// This will be called from the UNSettingsEditor.
        /// </summary>
        public virtual void DrawGUI()
        {

        }
    }
}