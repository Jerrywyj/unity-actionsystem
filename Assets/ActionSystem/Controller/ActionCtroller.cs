﻿using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class ActionCtroller
    {
        public ActionObjCtroller activeObjCtrl { get;private set; }
        private List<IOperateController> controllerList = new List<IOperateController>();
        protected Coroutine coroutine;
        private ControllerType activeTypes = 0;
        public static bool log = false;
        public UnityAction<IActionObj> onActionStart;
        private CameraController cameraCtrl
        {
            get
            {
                return ActionSystem.Instence.cameraCtrl;
            }
        }
        public PickUpController pickupCtrl { get{ return ActionSystem.Instence.pickUpCtrl; } }

        public ActionCtroller()
        {
            RegisterControllers();
        }

        public void Update()
        {
            foreach (var ctrl in controllerList)
            {
                if ((ctrl.CtrlType & activeTypes) != 0)
                {
                    ctrl.Update();
                   
                }
            }
        }
        private void RegisterControllers()
        {
            pickupCtrl.RegistOnPickup(OnPickUpObj);

            controllerList.Add(new PlaceController());
            controllerList.Add(new ClickCtrl());
            controllerList.Add(new RopeCtrl());
            controllerList.Add(new RotateCtrl());
            controllerList.Add(new ConnectCtrl());
            controllerList.Add(new DragCtrl());

            foreach (var ctrl in controllerList)
            {
                ctrl.userErr = OnUserError;
            }
        }

        public void OnUserError(string error)
        {
            if(activeObjCtrl !=null)
                activeObjCtrl.trigger.UserError(error);
        }
        /// 激活首要对象
        /// </summary>
        /// <param name="obj"></param>
        internal void OnPickUpObj(IPickUpAbleItem obj)
        {
            if (activeObjCtrl != null) activeObjCtrl.OnPickUpObj(obj);
        }

        public virtual void OnStartExecute(ActionObjCtroller activeObjCtrl, bool forceAuto)
        {
            this.activeObjCtrl = activeObjCtrl;
            this.activeObjCtrl.onCtrlStart = OnActionStart;
            this.activeObjCtrl.onCtrlStop = OnActionStop;
            this.activeObjCtrl.OnStartExecute(forceAuto);
        }

        private void OnActionStart(ControllerType ctrlType)
        {
            activeTypes |= ctrlType;
        }

        private void OnActionStop(ControllerType ctrlType)
        {
            activeTypes ^= ctrlType;
        }

        public virtual void OnEndExecute()
        {
            if (activeObjCtrl != null) activeObjCtrl.OnEndExecute();
        }

        public virtual void OnUnDoExecute()
        {
            if (activeObjCtrl != null) activeObjCtrl.OnUnDoExecute();
        }
    }
}