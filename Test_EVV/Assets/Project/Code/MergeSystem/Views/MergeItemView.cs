namespace Code.MergeSystem
{
    using System;
    using Code.FX;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;

    public class MergeItemView : MonoBehaviour
	{
        [SerializeField] private BoxCollider _raycastCollider;
        [SerializeField] private TextMeshProUGUI _mergeLevelText;
        [SerializeField] FXWrapper _spawnFX;
        [SerializeField] FXWrapper _mergeFX;
       
        
        private Vector3 _defaultPos;
        private MergeConfig _mergeConfig;

        public Transform Transform => transform;
        public string Name => gameObject.name;

        public bool TouchStartFlag { get; private set; }
        public bool TouchEndFlag { get; private set; }


        private void OnDisable()
        {
        }

        public void Construct(MergeConfig mergeConfig)
        {
            _mergeConfig = mergeConfig;
        }

        public void SetInitialGeometry( Vector3 worldPos, Transform parent )
        {
            _defaultPos = worldPos;
            transform.position = worldPos;
            transform.SetParent( parent );
        }

        public void SetPosition( Vector3 worldPos )
        {
            transform.position = worldPos;
        }

        public void ResetPosition()
        {
            transform.position = _defaultPos;
        }

        public void PlaySpawnFX()
        {
            //Debug.Log($"play spawn FX");
            _spawnFX.Play();
        }

        public void PlayMergeFX()
        {
            //Debug.Log($"play merge FX");
            _mergeFX.Play();
        }

        public void DoScaleInAnimation( Action completeCallback = null )
        {
        }

        public void DoScaleOutAnimation( Action completeCallback = null )
        {
        }

        public void AnimateScale(Vector3 from, Vector3 to, float duration, Ease ease, Action completeCallback = null)
        {
            Transform.localScale = from;

            Transform
                .DOScale(to, duration)
                .SetEase(ease)
                .OnComplete(() => completeCallback?.Invoke());
        }


        public void OnTouchStartHit()
        {
            TouchStartFlag = true;
        }

        public void OnTouchEndHit()
        {
            TouchEndFlag = true;
        }

        public void ResetTouchFlags()
        {
            TouchStartFlag = false;
            TouchEndFlag = false;
        }

        public void SetDraggedItemLayer() => gameObject.layer = _mergeConfig.DraggedMergeItemLayer;
        public void SetDefaultItemLayer() => gameObject.layer = _mergeConfig.MergeItemLayer;

        public void SetMergeLevelText( int mergeLevel )
        {
            _mergeLevelText.text = $"{mergeLevel + 1}";
        }

        private void OnValidate()
        {
            _raycastCollider = GetComponent<BoxCollider>();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube( transform.position + _raycastCollider.center, _raycastCollider.size );
        }
    }
}