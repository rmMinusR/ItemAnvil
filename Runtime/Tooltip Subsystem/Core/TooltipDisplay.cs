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

        [SerializeField] private Tooltippable target;

        [SerializeField] private PositionMode positionMode = PositionMode.Default;

        public enum PositionMode
        {
            FixedPosition,
            FollowCursor,
            FollowTarget,

            Default = FollowTarget
        }

        public Tooltippable GetTarget()
        {
            return target;
        }

        public void SetTarget(Tooltippable newTarget)
        {
            if (newTarget == null) throw new ArgumentNullException("Cannot target null - use ClearTarget instead");

            target = newTarget;
            //positionMode = newTarget.positionMode ?? PositionMode.Default;
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
                switch (positionMode)
                {
                    case PositionMode.FixedPosition:
                        //Do nothing
                        break;

                    case PositionMode.FollowCursor:
                        PositionToCursor();
                        break;

                    case PositionMode.FollowTarget:
                        PositionToTarget();
                        break;

                    default: throw new NotImplementedException("Unknown PositionMode "+positionMode);
                }
            }
        }

        public void PositionToCursor() //NOTE: Do not use for controller input schemes
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

        public void PositionToTarget()
        {
            //Determine target pos in screen coordinates
            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Vector2 targetPosOnScreen;
            if (targetCanvas && targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay) targetPosOnScreen = target.TooltipAnchor.position * targetCanvas.scaleFactor;
            else targetPosOnScreen = Camera.main.WorldToScreenPoint(target.TooltipAnchor.position);

            //Determine pos in parent coordinates such that our screen pos = target screen pos
            Canvas ownCanvas = GetComponentInParent<Canvas>();
            switch (ownCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    //Convert screen space to canvas space
                    transform.position = targetPosOnScreen / ownCanvas.scaleFactor;
                    break;

                case RenderMode.ScreenSpaceCamera:
                case RenderMode.WorldSpace:
                    //Raycast onto canvas plane
                    transform.position = target.transform.position;
                    Plane canvasPlane = new Plane(ownCanvas.transform.forward, ownCanvas.transform.position);
                    Ray cameraRay = Camera.main.ScreenPointToRay(targetPosOnScreen);
                    canvasPlane.Raycast(cameraRay, out float dist);
                    transform.position = cameraRay.GetPoint(dist);
                    break;

                default: throw new NotImplementedException();
            }
        }

        #region Cache

        //Transform is the scope which the display applies to (aka its parent)
        private static Dictionary<Transform, TooltipDisplay> __instances = new Dictionary<Transform, TooltipDisplay>();

        public static TooltipDisplay GetInstance(Transform target)
        {
            //Try to lookup
            if (__instances.TryGetValue(target, out TooltipDisplay display)) return display;
            else
            {
                //Try to recurse
                if (target.parent != null) return GetInstance(target.parent);

                //Nothing present in hierarchy
                else return null;
            }
        }

        private void OnEnable()
        {
            if (!__instances.ContainsKey(transform.parent)) __instances.Add(transform.parent, this);
            else Debug.LogError("TooltipDisplays cannot be siblings! Try putting one at a different hierarchy level?", this);
        }

        private void OnDisable()
        {
            if (__instances.TryGetValue(transform.parent, out TooltipDisplay display) && display == this)
            {
                __instances.Remove(transform.parent);
            }
        }

        #endregion
    }

}