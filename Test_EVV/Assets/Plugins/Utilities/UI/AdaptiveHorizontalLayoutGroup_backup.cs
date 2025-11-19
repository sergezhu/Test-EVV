/*namespace Utilities.UI
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

        private Coroutine spacingCoroutine;
        private Coroutine elementsMoveCoroutine;
        
        //private float animatedSpacing;
        //private float targetSpacing;
        //private float activeElementCurrentPosX;
        
        private float[] StartChildrenPositionsX;
        private float[] AnimatedChildrenPositionsX;
        private float[] TargetChildrenPositionsX;

        /*private float[] StartChildrenPositionsX => startChildrenPositionsX ?? new float[rectChildren.Count];
        private float[] AnimatedChildrenPositionsX => animatedChildrenPositionsX ?? new float[rectChildren.Count];
        private float[] TargetChildrenPositionsX => targetChildrenPositionsX ?? new float[rectChildren.Count];#1#
        
        //private float activeElementTargetPosX;
        //private bool pendingLayoutUpdate;

        [ContextMenu("Set Debug Active Element")]
        private void SetDebugActiveElement()
        {
            ActiveElement = debugActiveElement;
            
        }

        /*[ExecuteAlways]
        private void LateUpdate()
        {
            if ( pendingLayoutUpdate )
            {
                LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
                pendingLayoutUpdate = false;
            }
        }#1#

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
                        //AnimateActiveElementPosition();
                        //AnimateSpacing();
                        
                        AnimateElementsPositions();
                        SetLayoutHorizontal();
                    }
                    else
                    {
                        //UpdateSpacing();
                        //UpdateActiveElementPos();
                        SetLayoutHorizontal();
                        LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
                        //pendingLayoutUpdate = true;
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
            //UpdateSpacing();
        }

        public override void SetLayoutHorizontal()
        {
            //EvaluatePositionsX();

            UpdateChildrenAnimatedPositionsX();

            /#1#/ Если активный элемент есть, устанавливаем его позицию с анимацией
            if ( activeElement != null )
            {
                // Начальная позиция активного элемента (по X)
                if ( activeElementMoveCoroutine == null )
                    activeElementCurrentPosX = activeElement.anchoredPosition.x;

                // Целевая позиция активного элемента (пересчёт)
                //activeElementTargetPosX = CalculateActiveElementPosX( activeIndex );

                // Если корутина не запущена — просто установим позицию
                /*if ( !Application.isPlaying )
                {
                    activeElement.anchoredPosition =
                        new Vector2( activeElementTargetPosX, activeElement.anchoredPosition.y );
                }#2#
                
            }#1#
        }

        private void EvaluatePositionsX()
        {
            int activeIndex = rectChildren.IndexOf( activeElement );
            //CalcChildrenTargetPositions( animatedSpacing, activeIndex );

            if ( !Application.isPlaying )
            {
                var targetSpacing = CalculateTargetSpacing();
                CalcChildrenTargetPositions( targetSpacing, activeIndex );
                SetAnimatedPositionsAsTarget();
            }
        }

        private IEnumerator AnimatePositionsRoutine()
        {
            float duration = spacesChangeDuration;
            //float start = animatedSpacing;
            float time = 0f;
            
            for ( var i = 0; i < rectChildren.Count; i++ )
            {
                StartChildrenPositionsX[i] = rectChildren[i].anchoredPosition.x;
            }

            var targetSpacing = CalculateTargetSpacing();
            int activeIndex = rectChildren.IndexOf( activeElement );
            CalcChildrenTargetPositions( targetSpacing, activeIndex );

            Debug.Log( $"AnimatePositionsRoutine" );

            while ( time < duration )
            {
                float t = time / duration;
                float easedT = spacingEasing.Evaluate( t );

                for ( var i = 0; i < rectChildren.Count; i++ )
                {
                    var startPos = StartChildrenPositionsX[i];
                    var endPos = TargetChildrenPositionsX[i];
                    AnimatedChildrenPositionsX[i] = Mathf.Lerp( startPos, endPos, easedT );
                }

                //animatedSpacing = Mathf.Lerp( start, targetSpacing, easedT );
                LayoutRebuilder.MarkLayoutForRebuild( rectTransform );

                time += Time.deltaTime;

                yield return null;
            }

            SetAnimatedPositionsAsTarget();
            LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
            //pendingLayoutUpdate = true;
        }

        private void SetAnimatedPositionsAsTarget()
        {
            if ( AnimatedChildrenPositionsX == null ||
                 AnimatedChildrenPositionsX.Length != TargetChildrenPositionsX.Length )
            {
                AnimatedChildrenPositionsX = new float[TargetChildrenPositionsX.Length];
            }
            
            for ( var i = 0; i < TargetChildrenPositionsX.Length; i++ )
            {
                AnimatedChildrenPositionsX[i] = TargetChildrenPositionsX[i];
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

        private float CalculateTargetSpacing()
        {
            float totalWidth = padding.left + padding.right;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                totalWidth += LayoutUtility.GetPreferredSize(rectChildren[i], 0);
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

            return (count > 1) 
                ? extraSpace / (count - 1 - (activeIndex >= 0 ? 1 : 0)) 
                : 0f;
        }

        /*private void UpdateSpacing()
        {
            targetSpacing = CalculateTargetSpacing();
            animatedSpacing = targetSpacing;
        }#1#

        /*private void AnimateSpacing()
        {
            if (!Application.isPlaying)
            {
                UpdateSpacing();
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                //pendingLayoutUpdate = true;
                return;
            }

            if (spacingCoroutine != null)
                StopCoroutine(spacingCoroutine);

            spacingCoroutine = StartCoroutine(AnimateSpacingCoroutine());
        }

        private IEnumerator AnimateSpacingCoroutine()
        {
            float duration = spacesChangeDuration;
            float start = animatedSpacing;
            float time = 0f;
            
            targetSpacing = CalculateTargetSpacing();

            Debug.Log( $"AnimateSpacingCoroutine : [{start}] -> [{targetSpacing}]" );

            while (time < duration)
            {
                float t = time / duration;
                float easedT = spacingEasing.Evaluate(t);
                
                animatedSpacing = Mathf.Lerp(start, targetSpacing, easedT);
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                //pendingLayoutUpdate = true;

                Debug.Log( $"AnimateSpacingCoroutine : ANIMATION [{animatedSpacing}]" );
                
                time += Time.deltaTime;
                
                yield return null;
            }

            animatedSpacing = targetSpacing;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            //pendingLayoutUpdate = true;
        }#1#

        /*private void SetChildAlongAnimatedSpacing(float adaptiveSpacing, int activeIndex)
        {
            float posX = padding.left;
            int count = rectChildren.Count;

            for (int i = 0; i < count; i++)
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize(child, 0);
                SetChildAlongAxis(child, 0, posX, width);

                if (i == activeIndex)
                {
                    if (i == 0 && count > 1)
                        posX += width + spacing;
                    else if (i == count - 1 && count > 1)
                        posX += width;
                    else
                        posX += width + spacing;
                }
                else
                {
                    if (i + 1 == activeIndex)
                    {
                        posX += width + spacing;
                    }
                    else
                    {
                        posX += width + adaptiveSpacing;
                    }
                }
            }
        }#1#

        private void UpdateChildrenAnimatedPositionsX()
        {
            int count = rectChildren.Count;

            for ( int i = 0; i < count; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize( child, 0 );
                float posX = AnimatedChildrenPositionsX[i];

                SetChildAlongAxis( child, 0, posX, width );
            }
        }

        private void CalcChildrenTargetPositions( float adaptiveSpacing, int activeIndex )
        {
            float posX = padding.left;
            int count = rectChildren.Count;

            if ( TargetChildrenPositionsX == null || TargetChildrenPositionsX.Length != rectChildren.Count )
            {
                TargetChildrenPositionsX = new float[rectChildren.Count];
            }

            for ( int i = 0; i < count; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize( child, 0 );
                
                
                TargetChildrenPositionsX[i] = posX;

                // Решаем, какой отступ применить ПОСЛЕ текущего элемента
                bool isCurrentActive = i == activeIndex;
                bool isBeforeActive = i + 1 == activeIndex;

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
                StopCoroutine( elementsMoveCoroutine );

            elementsMoveCoroutine = StartCoroutine( AnimatePositionsRoutine() );
        }

        /*private float CalculateActiveElementPosX( int activeIndex )
        {
            float posX = padding.left;
            int count = rectChildren.Count;

            for ( int i = 0; i < count; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize( child, 0 );
                
                if ( i == activeIndex )
                {
                    return posX;
                }

                if ( i == activeIndex - 1 || i == activeIndex )
                {
                    posX += width + spacing;
                }
                else
                {
                    posX += width + animatedSpacing;
                }
            }

            return posX;
        }#1#

        /*private float CalculateActiveElementPosX( int activeIndex )
        {
            float posX = padding.left;

            for ( int i = 0; i < activeIndex; i++ )
            {
                RectTransform child = rectChildren[i];
                float width = LayoutUtility.GetPreferredSize( child, 0 );

                if ( i == activeIndex - 1 )
                    posX += width + spacing;
                else
                    posX += width + animatedSpacing;
            }

            return posX;
        }#1#


        /*private void AnimateActiveElementPosition()
        {
            if ( !Application.isPlaying )
            {
                // В редакторе сразу позиция
                UpdateActiveElementPos();
                return;
            }

            if ( activeElementMoveCoroutine != null )
                StopCoroutine( activeElementMoveCoroutine );

            activeElementMoveCoroutine = StartCoroutine( AnimateActiveElementPositionCoroutine() );
        }#1#

        /*private void UpdateActiveElementPos()
        {
            if ( activeElement != null )
            {
                int activeIndex = rectChildren.IndexOf( activeElement );
                activeElement.anchoredPosition = new Vector2( CalculateActiveElementPosX( activeIndex ), activeElement.anchoredPosition.y );
            }
        }#1#

        /*private IEnumerator AnimateActiveElementPositionCoroutine()
        {
            if ( activeElement == null ) 
                yield break;

            int activeIndex = rectChildren.IndexOf( activeElement );

            float start = activeElementCurrentPosX;
            float end = CalculateActiveElementPosX( activeIndex );
            
            Debug.Log( $"AnimateActiveElementPositionCoroutine : {start} -> {end}" );

            float time = 0f;

            while ( time < spacesChangeDuration )
            {
                float t = time / spacesChangeDuration;
                float easedT = spacingEasing.Evaluate( t );
                activeElementCurrentPosX = Mathf.Lerp( start, end, easedT );
                activeElement.anchoredPosition = new Vector2( activeElementCurrentPosX, activeElement.anchoredPosition.y );

                time += Time.deltaTime;

                Debug.Log( $"AnimateActiveElementPositionCoroutine : ANIMATION [{activeElementCurrentPosX}]" );
                
                yield return null;
            }

            activeElementCurrentPosX = end;
            activeElement.anchoredPosition = new Vector2( end, activeElement.anchoredPosition.y );
        }#1#

        #if UNITY_EDITOR
        [ContextMenu( "Force Update Layout" )]
        private void ForceUpdateLayout()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if ( this == null || rectTransform == null )
                    return;

                // теперь rectChildren точно обновлён
                //UpdateSpacing();
                //UpdateActiveElementPos();

                SetLayoutHorizontal();

                LayoutRebuilder.ForceRebuildLayoutImmediate( rectTransform );
            };
        }
        #endif

        protected override void OnValidate()
        {
            base.OnValidate();

            if ( !Application.isPlaying )
            {
                ActiveElement = debugActiveElement;

                /*UpdateSpacing();
                UpdateActiveElementPos();#1#
                
                SetLayoutHorizontal();

                LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
                
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if ( this != null ) // проверка, что объект всё ещё существует
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate( rectTransform );
                    }
                };
                #endif
            }
        }

    } 

}*/