﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions.Comparers;
namespace WorldActionSystem
{
    [RequireComponent(typeof(LineRenderer))]
    public class RotObj : ActionObj
    {
        public enum DirType
        {
            x, y, z
        }

        public Color color = Color.green;
        public float triggerRadius = 1;
        public float minAngle = 0;
        public float maxAngle = 30;
        public float triggerAngle = 28;
        public float circleDetail = 40;
        public float deviation = 1;
        public float rotSpeed = 40;
        public float flashSpeed = 1f;
        public bool highLight = true;
        public DirType dirType;

        public Vector3 Direction
        {
            get
            {
                switch (dirType)
                {
                    case DirType.x:
                        return -transform.right;
                    case DirType.y:
                        return -transform.up;
                    case DirType.z:
                        return -transform.forward;
                }
                return Vector3.zero;
            }
        }
        public float autoCompleteTime = 2f;
        private float currAngle;
        private List<Vector3> _lines = new List<Vector3>();
        private LineRenderer lineRender;
        private Quaternion startRot;
        private FloatComparer comparer;
        internal Renderer render;
        private float flash = 0;
        private float rangleCercle = .1f;
        private bool right;

        protected override void Start()
        {
            base.Start();
            startRot = transform.rotation;
            lineRender = gameObject.GetComponent<LineRenderer>();
            if (lineRender == null) lineRender = gameObject.AddComponent<LineRenderer>();
            lineRender.material = new Material(Shader.Find("Sprites/Default"));
#if UNITY_5_6_OR_NEWER
            lineRender.startWidth = 0.1f;
            lineRender.endWidth = 0.01f;
#else
            lineRender.SetWidth(.1f, .01f);
#endif
            gameObject.layer = Setting.rotateItemLayer;
            comparer = new FloatComparer(deviation);
            if (render == null) render = GetComponent<Renderer>();
        }
        private bool Flash()
        {
            if (Started && !Complete)
            {
                if (right)
                {
                    flash += Time.deltaTime * flashSpeed;
                    if (flash > rangleCercle)
                    {
                        right = false;
                    }
                }
                else
                {
                    flash -= Time.deltaTime * flashSpeed;
                    if (flash < -rangleCercle)
                    {
                        right = true;
                    }
                }
                return true;
            }
            else
            {
                flash = 0;
                return false;
            }
        }
        protected override void Update()
        {
            base.Update();
            if (Flash())
            {
                _lines.Clear();
                AddCircle(transform.position, Direction, triggerRadius + flash, _lines);
                if (highLight)
                {
                    DrawCircle(_lines, color);
                }
            }
            else
            {
                ClearCircle();
            }
        }

        void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer)
        {
            Vector3 up = axisDirection.normalized * size;
            Vector3 forward = Vector3.Slerp(up, -up, .5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * size;
            //Camera myCamera = Camera.main;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = right.x;
            matrix[1] = right.y;
            matrix[2] = right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = forward.x;
            matrix[9] = forward.y;
            matrix[10] = forward.z;

            Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 nextPoint = Vector3.zero;
            float multiplier = 360f / circleDetail;

            //Plane plane = new Plane((myCamera.transform.position - transform.position).normalized, transform.position);

            for (var i = 0; i < circleDetail + 1; i++)
            {
                nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);

                resultsBuffer.Add(lastPoint);
                resultsBuffer.Add(nextPoint);

                lastPoint = nextPoint;
            }
        }

        public override void OnStartExecute(bool forceauto)
        {
            base.OnStartExecute(forceauto);
            transform.rotation = startRot;
            if (forceauto) StartCoroutine(AutoRotateTo(TryEndExecute));
        }

        private IEnumerator AutoRotateTo(UnityAction callBack)
        {
            var target = Quaternion.Euler(Direction * triggerAngle) * startRot;
            var start = transform.rotation;
            for (float timer = 0; timer < autoCompleteTime; timer += Time.deltaTime)
            {
                yield return null;
                transform.rotation = Quaternion.Lerp(start, target, timer/autoCompleteTime);
            }
            callBack();
        }

        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            transform.rotation = startRot;
            currAngle = 0;
        }

        internal void SetHighLight(bool on)
        {
            this.highLight = on;
        }

        void DrawCircle(List<Vector3> lines, Color color)
        {
#if UNITY_5_6_OR_NEWER
            lineRender.startColor = color;
            lineRender.endColor = color;
            lineRender.positionCount = lines.Count;
#else
            lineRender.SetColors(color, color);
            lineRender.SetVertexCount(lines.Count);
#endif
            lineRender.SetPositions(lines.ToArray());
        }

        void ClearCircle()
        {
#if UNITY_5_6_OR_NEWER
            lineRender.positionCount = 0;
#else
            lineRender.SetVertexCount(0);
#endif
        }

        internal bool TryMarchRot()
        {
            return comparer.Equals(currAngle, triggerAngle);
        }
        public void ClampAsync()
        {
            StartCoroutine(Clamp());
        }
        private IEnumerator Clamp()
        {
            if (currAngle > maxAngle || currAngle < minAngle)
            {
                currAngle = Mathf.Clamp(currAngle, minAngle, maxAngle);
                var target = Quaternion.Euler(Direction * currAngle) * startRot;
                var start = transform.rotation;
                for (float timer = 0; timer < 1f; timer += Time.deltaTime)
                {
                    yield return new WaitForEndOfFrame();
                    transform.rotation = Quaternion.Lerp(start, target, timer);
                }
            }
        }

        public void Rotate(float amount)
        {
            currAngle += amount;
            transform.Rotate(Direction, amount, Space.World);
        }

    }
}

