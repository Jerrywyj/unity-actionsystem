﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class ClickCommand : IActionCommand
    {
        private ClickTrigger trigger;
        public string StepName { get; private set; }

        public ClickCommand(string stepName,ClickTrigger trigger)
        {
            this.StepName = stepName;
            this.trigger = trigger;
        }
        public  void StartExecute(bool forceAuto)
        {
            trigger.CreateStartController();
            if (forceAuto)
            {
                trigger.SetAllButtonClicked(StepName, true);
            }
            else
            {
                trigger.SetButtonClickAbleQueue(StepName);
            }
        }
        public  void EndExecute()
        {
            trigger.StopStartController();
            trigger.SetAllButtonClicked(StepName,false);
        }
        public  void UnDoExecute()
        {
            trigger.StopStartController();
            trigger.SetAllButtonUnClickAble(StepName);
            trigger.SetButtonNotClicked(StepName);
        }
    }

}