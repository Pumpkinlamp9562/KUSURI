using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

using uNature.Core.FoliageClasses;

namespace uNature.Demo
{
    public class UN_DemoPrototypeUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private RawImage _icon;
        public RawImage icon
        {
            get
            {
                return _icon;
            }
        }

        [SerializeField]
        private Image _highlight;
        public Image highlight
        {
            get
            {
                return _highlight;
            }
        }

        private bool _selected = false;
        public bool selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if(_selected != value)
                {
                    _selected = value;

                    highlight.enabled = value;
                }
            }
        }

        public bool isBrush = false;

        public PaintBrush paintBrush;
        public FoliagePrototype prototype;

        public void Initialize(Texture2D icon, PaintBrush paintBrush, FoliagePrototype prototype)
        {
            this.icon.texture = icon;

            this.paintBrush = paintBrush;
            this.prototype = prototype;

            isBrush = paintBrush != null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if (isBrush)
                {
                    if(UN_ProceduralDemo_UIController.instance.chosenBrush != null)
                    {
                        UN_ProceduralDemo_UIController.instance.chosenBrush.selected = false;
                    }

                    if (UN_ProceduralDemo_UIController.instance.chosenBrush == this)
                    {
                        UN_ProceduralDemo_UIController.instance.chosenBrush = null;
                    }
                    else
                    {
                        UN_ProceduralDemo_UIController.instance.chosenBrush = this;
                        selected = true;
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (UN_ProceduralDemo_UIController.instance.chosenPrototypes.Contains(this))
                        {
                            UN_ProceduralDemo_UIController.instance.chosenPrototypes.Remove(this);
                            selected = false;
                        }
                        else
                        {
                            UN_ProceduralDemo_UIController.instance.chosenPrototypes.Add(this);
                            selected = true;
                        }
                    }
                    else
                    {
                        bool wasChosen = UN_ProceduralDemo_UIController.instance.chosenPrototypes.Contains(this);

                        for (int i = 0; i < UN_ProceduralDemo_UIController.instance.chosenPrototypes.Count; i++)
                        {
                            UN_ProceduralDemo_UIController.instance.chosenPrototypes[i].selected = false;
                        }

                        UN_ProceduralDemo_UIController.instance.chosenPrototypes.Clear();

                        if (!wasChosen)
                        {
                            UN_ProceduralDemo_UIController.instance.chosenPrototypes.Add(this);
                            selected = true;
                        }
                    }
                }
            }
        }
    }
}