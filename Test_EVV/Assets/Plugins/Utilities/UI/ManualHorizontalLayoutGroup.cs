namespace Utilities.UI
{
	using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ManualHorizontalLayoutGroup : HorizontalLayoutGroup
{
    [SerializeField]
    private float[] targetChildrenPositionsX;

    public float[] TargetChildrenPositionsX => targetChildrenPositionsX;

    private void OnEnable()
    {
        RecalculateLayout();
    }

    protected override void OnTransformChildrenChanged()
    {
        base.OnTransformChildrenChanged();
        RecalculateLayout();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        RecalculateLayout();
    }
#endif

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        RecalculateLayout();
    }

    public override void SetLayoutHorizontal()
    {
        int count = rectChildren.Count;

        if (targetChildrenPositionsX == null || targetChildrenPositionsX.Length != count)
        {
            return; // Некорректный размер — не размещаем
        }

        for (int i = 0; i < count; i++)
        {
            var child = rectChildren[i];
            float width = LayoutUtility.GetPreferredSize(child, 0);

            // fallback на sizeDelta, если LayoutUtility вернул 0
            if (width <= 0f)
                width = child.sizeDelta.x;

            float posX = targetChildrenPositionsX[i];

            SetChildAlongAxis(child, 0, posX, width);
        }
    }

    public override void SetLayoutVertical()
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            var child = rectChildren[i];
            float height = LayoutUtility.GetPreferredSize(child, 1);

            if (height <= 0f)
                height = child.sizeDelta.y;

            SetChildAlongAxis(child, 1, padding.top, height);
        }
    }

    private void RecalculateLayout()
    {
        if (rectChildren == null) return;

        int count = rectChildren.Count;

        // Убедимся, что массив имеет правильную длину
        if (targetChildrenPositionsX == null || targetChildrenPositionsX.Length != count)
        {
            targetChildrenPositionsX = new float[count];

            // Расставим их по умолчанию с spacing
            float currentX = padding.left;
            for (int i = 0; i < count; i++)
            {
                float width = LayoutUtility.GetPreferredSize(rectChildren[i], 0);
                if (width <= 0f)
                    width = rectChildren[i].sizeDelta.x;

                targetChildrenPositionsX[i] = currentX;
                currentX += width + spacing;
            }
        }

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
}

}