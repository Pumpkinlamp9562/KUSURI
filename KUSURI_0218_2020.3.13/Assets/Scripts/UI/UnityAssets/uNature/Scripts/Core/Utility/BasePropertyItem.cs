using UnityEngine;

namespace uNature.Core.Utility
{
    /// <summary>
    /// An base prototype item which is used for the uNature ui utility.
    /// </summary>
    public class BasePrototypeItem
    {
        private Texture2D _preview;
        public Texture2D preview
        {
            get
            {
                if (_preview == null)
                {
                    _preview = GetPreview();
                }

                return _preview;
            }
            set
            {
                _preview = value;
            }
        }

        public virtual bool isEnabled
        {
            get
            {
                return true;
            }
        }

        public virtual bool chooseableOnDisabled
        {
            get
            {
                return false;
            }
        }

        protected virtual Texture2D GetPreview()
        {
            return null;
        }
    }
}