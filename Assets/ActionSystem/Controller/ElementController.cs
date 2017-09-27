﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class ElementController
    {
        public event UnityAction onInstall;
        private Dictionary<string, List<PickUpAbleElement>> objectList = new Dictionary<string, List<PickUpAbleElement>>();

        /// <summary>
        /// 外部添加Element
        /// </summary>
        public void RegistElement(PickUpAbleElement item)
        {
            var obj = item;
            if (objectList.ContainsKey(obj.name))
            {
                objectList[obj.name].Add(obj);
            }
            else
            {
                objectList[obj.name] = new List<PickUpAbleElement>() { obj };
            }

            obj.onInstallOkEvent = ()=> {
                if (onInstall != null) onInstall.Invoke();
            };
        }
        /// <summary>
        /// 获取指定元素名的列表
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public List<PickUpAbleElement> GetElements(string elementName)
        {
            if(objectList.ContainsKey(elementName))
            {
                return objectList[elementName];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 找出一个没有安装的元素
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
       public  PickUpAbleElement GetUnInstalledObj(string elementName)
        {
            List<PickUpAbleElement> listObj;

            if (objectList.TryGetValue(elementName, out listObj))
            {
                for (int i = 0; i < listObj.Count; i++)
                {
                    if (!listObj[i].Installed)
                    {
                        return listObj[i];
                    }
                }
            }
            return null;
        }
    }

}
