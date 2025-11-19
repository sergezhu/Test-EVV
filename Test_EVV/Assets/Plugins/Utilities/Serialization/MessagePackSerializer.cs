namespace Utilities.Serialization
{
	#if MESSAGE_PACK_INSTALLED
	
	using System;
	using MessagePack;
	using MessagePack.Resolvers;
	using UnityEngine;

	public class MessagePackSerializerImpl : ISerializer
	{
		static MessagePackSerializerImpl()
		{
			// Регистрируем стандартные + Unity-friendly сериализаторы
			MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
				.WithResolver( CompositeResolver.Create(
								   TypelessObjectResolver.Instance, // Для object
								   StandardResolver.Instance        // Для обычных типов
							   ) )
				.WithCompression( MessagePackCompression.Lz4BlockArray ); // По желанию, можно и без компрессии
		}

		public byte[] SerializeBinary( object obj )
		{
			return MessagePackSerializer.Serialize<object>( obj, MessagePackSerializer.DefaultOptions );
		}

		public object DeserializeBinary( byte[] bytes )
		{
			return MessagePackSerializer.Deserialize<object>( bytes, MessagePackSerializer.DefaultOptions );
		}

		public string SerializeJSON<T>( T obj )
		{
			throw new NotImplementedException("MessagePackSerializer does not recommended JSON, use other serializer");
			//return MessagePackSerializer.SerializeToJson( obj );
		}

		public T DeserializeJSON<T>( string json )
		{
			throw new NotImplementedException( "MessagePackSerializer does not recommended JSON, use other serializer" );
			//return MessagePackSerializer.DeserializeFromJson<T>( json );
		}
	}

	#endif
}