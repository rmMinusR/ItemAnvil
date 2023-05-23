using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace rmMinusR.Tooltips
{

    /// <summary>
    /// Handles enabling, positioning, and sending what is currently displayed to content parts
    /// </summary>
    public sealed class TooltipDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject renderRoot;

        private List<TooltipSectionManager> sections = new List<TooltipSectionManager>();
        internal void RegisterSection(TooltipSectionManager s) => sections.Add(s);
        internal void UnregisterSection(TooltipSectionManager s) => sections.Remove(s);

#if USING_INSPECTORSUGAR
        [InspectorReadOnly]
#endif
        [SerializeField] private Tooltippable target;

        /*
        // Buggy. Disabled for now.

#if USING_INSPECTORSUGAR
        [InspectorReadOnly]
#endif
        [SerializeField] private PositionMode positionMode;
        [SerializeField] private PositionMode defaultPositionMode = PositionMode.FollowTarget;

        public enum PositionMode
        {
            NoAction = 0,
            FollowCursor = 1,
            FollowTarget = 2
        }
        */

        public Tooltippable GetTarget()
        {
            return target;
        }

        public void SetTarget(Tooltippable newTarget)
        {
            if (newTarget == null) throw new ArgumentNullException("Cannot target null - use ClearTarget instead");

            target = newTarget;
            //this.positionMode = positionMode ?? defaultPositionMode;
            renderRoot.SetActive(true);

            foreach (TooltipSectionManager l in sections) l.UpdateTarget(newTarget);
        }

        public void ClearTarget(Tooltippable target)
        {
            if (this.target == target)
            {
                this.target = null;
                renderRoot.SetActive(false);
            }

        }

        private void Start()
        {
            renderRoot.SetActive(false);
        }

        private void Update()
        {
            if(target != null)
            {
                PositionToCursor();
                /*
                //Update position
                switch (positionMode)
                {
                    case PositionMode.NoAction:
                        //Do nothing
                        break;

                    case PositionMode.FollowCursor:
                        PositionToCursor();
                        break;

                    case PositionMode.FollowTarget:
                        PositionToTarget();
                        break;

                    default: throw new System.NotImplementedException("Unknown PositionMode "+positionMode);
                }
                */
            }
        }

        private void PositionToCursor()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.position = GetMousePosition();
            }
            else
            {
                //Raycast screen point onto canvas plane
                Plane canvasPlane = new Plane(canvas.transform.forward, canvas.transform.position);
                Ray cameraRay = Camera.main.ScreenPointToRay(GetMousePosition());
                canvasPlane.Raycast(cameraRay, out float dist);
                transform.position = cameraRay.GetPoint(dist);
            }
        }

        private Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#endif
        }

        private void PositionToTarget()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                //Convert world point to screen space
                transform.position = Camera.main.WorldToScreenPoint(target.transform.position);
            }
            else
            {
                //Raycast world point onto canvas plane
                transform.position = target.transform.position;
                Plane canvasPlane = new Plane(canvas.transform.forward, canvas.transform.position);
                Ray cameraToTargetRay = new Ray(Camera.main.transform.position, (target.transform.position-Camera.main.transform.position).normalized);
                canvasPlane.Raycast(cameraToTargetRay, out float dist);
                transform.position = cameraToTargetRay.GetPoint(dist);
            }
        }

    #region Cache

        private static List<TooltipDisplay> __instances = new List<TooltipDisplay>();

        public static TooltipDisplay GetInstance(Transform target)
        {
            bool selector(TooltipDisplay r) => r.transform == target || r.transform.parent == target.parent;

            //Scan current hierarchy
            if (__instances.Any(selector)) return __instances.First(selector);

            //Try to grab from scene
            if (target.parent != null) return GetInstance(target.parent);

            //None present
            else return null;
        }

        private void OnEnable()
        {
            if (!__instances.Contains(this)) __instances.Add(this);
            else Debug.LogError("TooltipDisplay already registered!", this);
        }

        private void OnDisable()
        {
            if (__instances.Contains(this)) __instances.Remove(this);
            else Debug.LogError("TooltipDisplay instance already disabled? This should never happen!");
        }

        #endregion
    }

}