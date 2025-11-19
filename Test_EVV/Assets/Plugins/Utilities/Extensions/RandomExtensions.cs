namespace Utilities.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class RandomExtensions
	{
		public static int RandomIndex<T>( this IEnumerable<T> collection )
		{
			var itemsCount = collection.Count();
			
			if ( itemsCount == 0 )
				throw new InvalidOperationException( "List is empty!" );
			
			return UnityEngine.Random.Range( 0, itemsCount );
		}


		public static int GetRandomWeightedIndex( this List<float> weights )
		{
			// https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/#post-5173340

			if ( weights == null || weights.Count == 0 )
				return -1;

			float total = 0;
			for ( int i = 0; i < weights.Count; i++ )
			{
				float w = weights[i];

				if ( float.IsPositiveInfinity( w ) )
					return i;

				if ( w > 0 && !float.IsNaN( w ) )
					total += w;
			}

			float random = UnityEngine.Random.value;

			float sum = 0;
			for ( int i = 0; i < weights.Count; i++ )
			{
				float w = weights[i];

				if ( float.IsNaN( w ) || w <= 0 )
					continue;

				sum += w / total;

				if ( sum >= random )
					return i;
			}

			return -1;
		}


		public static T Random<T>( this IEnumerable<T> collection )
		{
			var list = collection.ToList();
			return list[list.RandomIndex()];
		}

		public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> collection )
		{
			Random random = new Random();
			var array = collection.ToArray();
			int n = array.Length;

			while ( n > 1 )
			{
				n--;
				int k = random.Next( n + 1 );

				(array[k], array[n]) = (array[n], array[k]);
			}

			return array;
		}

		public static T PopRandom<T>( this IEnumerable<T> collection )
		{
			var list = collection.ToList();
			return list.Pop( list.RandomIndex() );
		}

		public static T Pop<T>( this List<T> list, int index )
		{
			T item = list[index];
			list[index] = list[list.Count - 1];
			list.RemoveAt( list.Count - 1 );
			return item;
		}
		
	}
}