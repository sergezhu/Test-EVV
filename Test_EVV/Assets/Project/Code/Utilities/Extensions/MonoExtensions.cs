namespace Utilities.Extensions
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class MonoExtensions
	{
		public static void IfNotNull<T>(this T component, Action<T> action) where T : Component
        {
            if (component != null)
                action?.Invoke(component);
        }

        public static void IfNull<T>(this T component, Action<T> action) where T : Component
        {
            if (component == null)
                action?.Invoke(component);
        }

        public static void Show(this Component component)
        {
            Show(component.gameObject);
        }

        public static void Show(this GameObject gameObject)
        {
            gameObject.SetActive(true);
        }
        
        public static void Hide(this Component component)
        {
            Hide(component.gameObject);
        }
        
        public static void Hide(this GameObject gameObject)
        {
            gameObject.SetActive(false);
        }
        
        public static void EnableParent(this Component component)
        {
            var parent = component.transform.parent;
            
            if (parent == null)
                component.Show();
            else
                parent.Show();
        }

        public static void DisableParent(this Component component)
        {
            var parent = component.transform.parent;
            
            if (parent == null)
                component.Hide();
            else
                parent.Hide();
        }

        public static Transform TryGetParent(this Component component)
        {
            var transform = component.transform;
            var parent = transform.parent;
            return parent == null ? transform : parent;
        }

        public static Transform TryGetChild(this Component component)
        {
            var transform = component.transform;
            var children = transform.GetChild(0);
            return children == null ? transform : children;
        }
        
        public static T GetNearby<T>(this Component component) where T : Component
        {
            T instance = null;

            if (component.transform.parent != null)
                instance = component.GetComponentInParent<T>();

            if (instance == null)
                instance = component.GetComponentInChildren<T>();

            if (instance == null)
                throw new NullReferenceException(typeof(T).Name);

            return instance;
        }

        public static void DestroyChildren( this Transform transform )
        {
            for ( int i = transform.childCount - 1; i >= 0; i-- )
            {
                SmartDestroy( transform.GetChild( i ) );
            }
        }

        public static void SmartDestroy( this Transform transform )
        {
            if ( Application.isPlaying )
                Object.Destroy( transform.gameObject );
            else
                Object.DestroyImmediate( transform.gameObject, true );
        }

        public static void SmartDestroyComponent( this Component component )
        {
            if ( Application.isPlaying )
                Object.Destroy( component );
            else
                Object.DestroyImmediate( component, true );
        }

        public static void SetXY( this Transform transform, Vector2 xy ) => transform.position = transform.position.WithXY( xy );

        public static void SetZ( this Transform transform, float z ) => transform.position = transform.position.WithZ( z );
	}
}