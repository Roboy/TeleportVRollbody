﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Widgets
{
    public class IconWidget : Widget
    {
        public Dictionary<string, Texture2D> icons;
        public Texture2D[] iconsArray;
        
        private string currentIconName;
        public Texture2D currentIcon;

        /// <summary>
        /// Initializes icon widget subclass with default values from widget template.
        /// </summary>
        /// <param name="context">Widget context</param>
        /// <param name="viewDesignPrefab">Prefab for icon views</param>
        /// <param name="icons">Icons registered for this widget as dictionary with file names as keys</param>
        public void Init(RosJsonMessage context, GameObject viewDesignPrefab, Dictionary<string, Texture2D> icons)
        {
            this.icons = icons;
            iconsArray = new Texture2D[icons.Count];
            icons.Values.CopyTo(iconsArray, 0);

            childWidgetId = context.childWidgetId;

            ProcessRosMessage(context);

            base.Init(context, viewDesignPrefab);
        }

        /// <summary>
        /// Change icon in view according to new ros message.
        /// </summary>
        /// <param name="rosMessage"></param>
        public override void ProcessRosMessage(RosJsonMessage rosMessage)
        {
            if (!rosMessage.currentIcon.Equals(""))
            {
                currentIconName = rosMessage.currentIcon;
                //print("Current Icon name: " + currentIconName);
                if (icons.TryGetValue(currentIconName, out currentIcon))
                {
                    //print("Got value " + currentIcon);
                    if (view != null)
                    {
                        //print(view);
                        ((IconView)view).SetIcon(currentIcon);
                        if (!rosMessage.currentIconAlpha.Equals(""))
                            ((IconView)view).SetIconAlpha(rosMessage.currentIconAlpha);
                        //print("Finito");
                    }
                }
                else
                {
                    print("Icon " + currentIconName + " is not among the icons for this widget");
                }
            }
        }

        /// <summary>
        /// Icon widgets don't need subclass update calls.
        /// </summary>
        protected override void UpdateInClass()
        {

        }

        public override View AddViewComponent(GameObject viewGameObject)
        {
            return viewGameObject.AddComponent<IconView>();
        }
    }
}