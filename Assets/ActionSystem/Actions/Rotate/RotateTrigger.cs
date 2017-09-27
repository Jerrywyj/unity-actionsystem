﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class RotateTrigger : ActionTrigger
    {
        public bool highlight;
        private Dictionary<string, List<RotObj>> objDic = new Dictionary<string, List<RotObj>>();
        private RotateAnimController rotAnimCtrl;
        private List<int> queueID = new List<int>();
        private string currStepName;
        private Coroutine coroutine;
        protected override void Awake()
        {
            base.Awake();
            InitObjects();
          
        }
        public void CrateRotAnimCtrl()
        {
            if(rotAnimCtrl == null)
            {
                rotAnimCtrl = new WorldActionSystem.RotateAnimController();
                rotAnimCtrl.onHover = OnHover;
                rotAnimCtrl.OnRotateOk = OnRoateOK;
                rotAnimCtrl.onStartRot = OnStartRot;
                rotAnimCtrl.onEndRot = OnEndRot;
                coroutine = StartCoroutine(rotAnimCtrl.StartRotateAnimContrl());
            }
          
        }
        public void CompleteRotateAnimCtrl()
        {
            if (coroutine != null) StopCoroutine(coroutine);
            rotAnimCtrl = null;
        }

        public override IList<IActionCommand> CreateCommands()
        {
            var cmds = new List<IActionCommand>();
            cmds.Add(new RotateCommand(StepName, this));

            return cmds;
        }

        void InitObjects()
        {
            foreach (RotObj obj in actionObjs)
            {
                if (objDic.ContainsKey(obj.StepName))
                {
                    objDic[obj.StepName].Add(obj);
                }
                else
                {
                    objDic[obj.StepName] = new List<RotObj>() { obj };
                }
                obj.SetHighLight(highlight);
            }
        }

        internal void SetStepUnDo(string stepName)
        {
            var list = objDic[currStepName];
            foreach (var item in list)
            {
                item.UnDoExecute();
            }
        }
        private void OnHover(RotObj arg0)
        {
            Debug.Log("hover");
        }

        private void OnEndRot(RotObj arg0)
        {
            Debug.Log("OnEndRot");
        }

        private void OnStartRot(RotObj arg0)
        {
            Debug.Log("OnStartRot");
        }

        void OnRoateOK(RotObj obj)
        {
            if (!SetNextRotateAble()) {
                OnComplete();
            }
        }

        internal void SetRotateComplete(bool forceAuto = false)
        {
            var list = objDic[currStepName];
            foreach (var item in list) {
                item.EndExecute();
            }
            if (forceAuto)
            {
                OnComplete();
            }

        }

        internal void SetRotateQueue(string stepName)
        {
            this.currStepName = stepName;
            queueID.Clear();
            var btns = objDic[stepName];
            foreach (var item in btns)
            {
                if (!queueID.Contains(item.queueID))
                {
                    queueID.Add(item.queueID);
                }
            }
            queueID.Sort();
            SetNextRotateAble();
        }

        private bool SetNextRotateAble()
        {
            if (queueID.Count > 0)
            {
                var id = queueID[0];
                queueID.RemoveAt(0);
                var items = objDic[currStepName];
                var neetActive = items.FindAll(x => x.queueID == id);
                foreach (var item in neetActive) {
                    item.StartExecute();
                    rotAnimCtrl.SetViewCamera(item.ViewCamera);
                }
                return true;
            }
            return false;
        }

        internal void ActiveStep(string currStepName)
        {
            this.currStepName = currStepName;
        }

        internal void SetRotateStart(string stepName)
        {
            var list = objDic[stepName];
            foreach (var item in list)
            {
                item.StartExecute();
            }
        }

    }

}