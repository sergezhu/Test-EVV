namespace Utilities.UI
{
	using System.Collections;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [ExecuteAlways]
    [AddComponentMenu("Layout/Adaptive Horizontal Layout Group")]
    public class AdaptiveHorizontalLayoutGroup : HorizontalLayoutGroup
    {
        [SerializeField] private RectTransform activeElement;
        [SerializeField] private float spacesChangeDuration = 0.3f;
        [SerializeField] private AnimationCurve spacingEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        

        [Header( "Debug" )]
        [SerializeField] private RectTransform debugActiveElement;
        [SerializeField] private float[] startChildrenPositionsX;
        [SerializeField] private float[] animatedChildrenPositionsX;
        [SerializeField] private float[] targetChildrenPositionsX;
        
        private Coroutine spacingCoroutine;
        private Coroutine elementsMoveCoroutine;

        public bool IsAnimating { get; private set; }
        
        

        [ContextMenu("Set Debug Active Element")]
        private void SetDebugActiveElement()
        {
            ActiveElement = debugActiveElement;
            
        }

        private new void OnEnable()
        {
            base.OnEnable();
            
            if ( !Application.isPlaying )
            {
                EvaluatePositionsX();
                LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
            }
        }


        public RectTransform ActiveElement
        {
            get => activeElement;
            set
            {
                if (activeElement != value)
                {
                    activeElement = value;

                    if ( Application.isPlaying )
                    {
                        AnimateElementsPositions();
                        SetLayoutHorizontal();
                    }
                    else
                    {
                        EvaluatePositionsX();
                        LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
                    }
                }
            }
        }

        public float SpacesChangeDuration
        {
            get => spacesChangeDuration;
            set => spacesChangeDuration = Mathf.Max(0f, value);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            EvaluatePositionsX();
        }

        public override void SetLayoutHorizontal()
        {
            Debug.Log( "[SetLayoutHorizontal]" );
            
            if ( animatedChildrenPositionsX == null || animatedChildrenPositionsX.Length != rectChildren.Count )
            {
                EvaluatePositionsX(); // fallback в Editor
            }

            UpdateChildrenAnimatedPositionsX();
        }

        private void EvaluatePositionsX()
        {
            Debug.Log( "[EvaluatePositions]" );
            
            int count = rectChildren.Count;
            int activeIndex = rectChildren.IndexOf( activeElement );

            if ( !Application.isPlaying )
            {
                var targetSpacing = CalculateTargetSpacing();
                CalcChildrenTargetPositions( targetSpacing, activeIndex );
                SetAnimatedPositionsAsTarget();
            }
        }


        private IEnumerator AnimatePositionsRoutine()
        {
            IsAnimating = true;
            
            float duration = spacesChangeDuration;
            float time = 0f;

            if ( startChildrenPositionsX == null || startChildrenPositionsX.Length != rectChildren.Count )
            {
                startChildrenPositionsX = new float[rectChildren.Count];
            }

            for ( var i = 0; i < rectChildren.Count; i++ )
            {
                var child = rectChildren[i];
                startChildrenPositionsX[i] = child.anchoredPosition.x - child.pivot.x * child.rect.width;
            }

            var targetSpacing = CalculateTargetSpacing();
            int activeIndex = rectChildren.IndexOf( activeElement );
            
            CalcChildrenTargetPositions( targetSpacing, activeIndex );

            Debug.Log( $"[Calc] AnimatePositionsRoutine, targetSpacing = {targetSpacing}" );

            while ( time < duration )
            {
                float t = time / duration;
                float easedT = spacingEasing.Evaluate( t );

                for ( var i = 0; i < rectChildren.Count; i++ )
                {
                    var startPos = startChildrenPositionsX[i];
                    var endPos = targetChildrenPositionsX[i];
                    animatedChildrenPositionsX[i] = Mathf.Lerp( startPos, endPos, easedT );
                }

                //animatedSpacing = Mathf.Lerp( start, targetSpacing, easedT );
                LayoutRebuilder.MarkLayoutForRebuild( rectTransform );

                time += Time.deltaTime;

                yield return null;
            }

            SetAnimatedPositionsAsTarget();
            LayoutRebuilder.MarkLayoutForRebuild( rectTransform );

            IsAnimating = false;
        }

        private void SetAnimatedPositionsAsTarget()
        {
            if ( animatedChildrenPositionsX == null || animatedChildrenPositionsX.Length != targetChildrenPositionsX.Length )
            {
                animatedChildrenPositionsX = new float[targetChildrenPositionsX.Length];
            }
            
            for ( var i = 0; i < targetChildrenPositionsX.Length; i++ )
            {
                animatedChildrenPositionsX[i] = targetChildrenPositionsX[i];
            }
        }


        public override void SetLayoutVertical()
        {
            for (int i = 0; i < rectChildren.Count; i++)
            {
                var child = rectChildren[i];
                SetChildAlongAxis(child, 1, padding.top, LayoutUtility.GetPreferredSize(child, 1));
            }
        }

        /*private float  CalculateTargetSpacing()
        {
            float totalWidth = padding.left + padding.right;
            
            foreach ( var rect in rectChildren )
            {
                totalWidth += LayoutUtility.GetPreferredSize(rect, 0);
            }

            int activeIndex = rectChildren.IndexOf(activeElement);
            int count = rectChildren.Count;

            float requiredSpacing = 0;
            
            if (count > 1)
            {
                requiredSpacing = (count - 1) * spacing;
                
                if (activeIndex >= 0)
                {
                    requiredSpacing -= spacing;
                    
                    if (activeIndex != 0 && activeIndex != count - 1)
                        requiredSpacing -= spacing;
                }
            }

            float availableWidth = rectTransform.rect.width;
            
            if ( availableWidth <= 0 ) 
                return spacing;
            
            float extraSpace = availableWidth - totalWidth - requiredSpacing;
            var adaptiveSpacingSlots = extraSpace / (count - 1 - (activeIndex >= 0 ? 1 : 0));
            float extraSpacePerSlot = count > 1
                ? extraSpace / adaptiveSpacingSlots
                : 0f;

            Debug.Log(
                $"[CalculateTargetSpacing] extra-sp [{extraSpace}], slots [{adaptiveSpacingSlots}], extra-sp-per-slot [{extraSpacePerSlot}]" );
            
            return extraSpacePerSlot;
        }*/

        private float CalculateTargetSpacing()
        {
            float totalElementsWidth = padding.left + padding.right;

            foreach ( var rect in rectChildren )
            {
                totalElementsWidth += LayoutUtility.GetPreferredSize( rect, 0 );
            }

            int count = rectChildren.Count;
            
            if ( count <= 1 ) 
                return 0f;

            int activeIndex = rectChildren.IndexOf( activeElement );

            // Кол-во слотов под adaptive spacing
            int adaptiveSpacingSlots = count - 1;

            // Если есть активный, у него будет spacing слева и справа
            // Но ты добавляешь spacing вручную в CalcChildrenTargetPositions, так что исключим эти слоты
            if ( activeIndex >= 0 )
            {
                adaptiveSpacingSlots -= 1; // убираем один slot, который займёт spacing вокруг активного

                if ( activeIndex > 0 && activeIndex < count - 1 )
                    adaptiveSpacingSlots -= 1;
            }

            float availableWidth = rectTransform.rect.width;
            float fixedSpacing = 0;

            if ( activeIndex >= 0 )
            {
                // Активному добавляется spacing 1-2 раза, в зависимости от его позиции
                if ( activeIndex == 0 || activeIndex == count - 1 )
                    fixedSpacing = spacing;
                else
                    fixedSpacing = spacing * 2;
            }

            float extraSpace = availableWidth - totalElementsWidth - fixedSpacing;

            Debug.Log( $"[Calc][CalculateTargetSpacing] avail-width [{availableWidth}], total-width [{totalElementsWidth}], fix-sp [{fixedSpacing}], " +
                       $"extra-sp [{extraSpace}], slots [{adaptiveSpacingSlots}]" );


            if ( adaptiveSpacingSlots == 0 )
                return spacing;

            var extraSpacePerSlot = extraSpace / adaptiveSpacingSlots;
            
            //Debug.Log( $"[CalculateTargetSpacing] aw-w [{availableWidth}], total-ew [{totalElementsWidth}], fix-sp [{fixedSpacing}], extra-sp [{extraSpace}], slots [{adaptiveSpacingSlots}], extra-sp-per-slot [{extraSpacePerSlot}]" );
            Debug.Log( $"[Calc][CalculateTargetSpacing] extraSpacePerSlot [{extraSpacePerSlot}]" );
            
            return extraSpacePerSlot;
        }


        private void UpdateChildrenAnimatedPositionsX()
        {
            Debug.Log( "[UpdateChildrenAnimatedPositionsX]" );
            
            int count = rectChildren.Count;

            for ( int i = 0; i < count; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize( child, 0 );
                float posX = animatedChildrenPositionsX[i];

                if ( width <= 0 )
                {
                    Debug.LogWarning( $"Child {child.name} width is {width}" );
                }

                //Debug.Log( $"[UpdateChildrenAnimatedPositionsX] [{i}] : pos [{posX}], width [{width}]" );

                SetChildAlongAxis( child, 0, posX, width );
            }
        }

        private void CalcChildrenTargetPositions( float adaptiveSpacing, int activeIndex )
        {
            float posX = padding.left;
            int count = rectChildren.Count;

            if ( targetChildrenPositionsX == null || targetChildrenPositionsX.Length != rectChildren.Count )
            {
                targetChildrenPositionsX = new float[rectChildren.Count];
            }

            Debug.Log( $"[Calc][CalcChildrenTargetPositions] adaptiveSpacing [{adaptiveSpacing}]" );

            for ( int i = 0; i < count; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize( child, 0 );
                
                
                targetChildrenPositionsX[i] = posX;

                // Решаем, какой отступ применить ПОСЛЕ текущего элемента
                bool isCurrentActive = i == activeIndex;
                bool isBeforeActive = i == activeIndex - 1;

                if ( isCurrentActive )
                {
                    if ( i == 0 && count > 1 )
                    {
                        posX += width + spacing; // только справа
                    }
                    else if ( i == count - 1 && count > 1 )
                    {
                        posX += width + spacing; // только слева, но реализуем через предыдущий отступ
                    }
                    else
                    {
                        posX += width + spacing; // обе стороны, правая учитывается здесь
                    }
                }
                else if ( isBeforeActive )
                {
                    posX += width + spacing; // spacing перед активным
                }
                else
                {
                    posX += width + adaptiveSpacing;
                }
            }
        }

        private void AnimateElementsPositions()
        {
            if ( elementsMoveCoroutine != null )
            {
                StopCoroutine( elementsMoveCoroutine );
                IsAnimating = false;
            }

            elementsMoveCoroutine = StartCoroutine( AnimatePositionsRoutine() );
        }

        #if UNITY_EDITOR
        
        [ContextMenu( "Force Update Layout" )]
        private void ForceUpdateLayout()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if ( this == null || rectTransform == null )
                    return;

                EvaluatePositionsX();
                LayoutRebuilder.ForceRebuildLayoutImmediate( rectTransform );
            };
        }
        
        #endif

        #if UNITY_EDITOR
        
        protected override void OnValidate()
        {
            base.OnValidate();

            if ( !Application.isPlaying )
            {
                ActiveElement = debugActiveElement;

                EvaluatePositionsX();
                LayoutRebuilder.MarkLayoutForRebuild( rectTransform );

                #if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if ( this != null )
                        LayoutRebuilder.ForceRebuildLayoutImmediate( rectTransform );
                };
                #endif
            }
        }
        
        #endif
    }
}