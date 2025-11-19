namespace Utilities.Serialization
{
	using System.Reflection;
	using Sirenix.Serialization;

	public class OdinSerializer : ISerializer
	{
		public byte[] SerializeBinary( object obj )
		{
		    var context = new SerializationContext();
			return SerializationUtility.SerializeValue( obj, DataFormat.Binary, context );
		}


		public object DeserializeBinary( byte[] bytes )
		{
		    var context = new DeserializationContext();
			return SerializationUtility.DeserializeValue<object>( bytes, DataFormat.Binary, context );
		}


		public byte[] SerializeBinaryLight( object obj )
		{
		    var context = new SerializationContext();
			
			var config = new SerializationConfig();
			config.SerializationPolicy = new LightweightPolicy();
			context.Config = config;

			return SerializationUtility.SerializeValue( obj, DataFormat.Binary, context );
		}


		public object DeserializeBinaryLight( byte[] bytes )
		{
		    var context = new DeserializationContext();
			
			var config = new SerializationConfig();
			config.SerializationPolicy = new LightweightPolicy();
			context.Config = config;
			
			return SerializationUtility.DeserializeValue<object>( bytes, DataFormat.Binary, context );
		}


		public byte[] SerializeNodes( object obj )
		{
		    var context = new SerializationContext();

			return SerializationUtility.SerializeValue( obj, DataFormat.Nodes, context );
		}


		public object DeserializeNodes( byte[] bytes )
		{
		    var context = new DeserializationContext();

			return SerializationUtility.DeserializeValue<object>( bytes, DataFormat.Nodes, context );
		}


		public string SerializeJSON<T>( T obj )
		{
			var context = new SerializationContext();
			var buffer = SerializationUtility.SerializeValue<T>( obj, DataFormat.JSON, context );
			var json = System.Text.Encoding.UTF8.GetString( buffer, 0, buffer.Length );
			
			return json;
		}


		public T DeserializeJSON<T>( string json )
		{
			var context = new DeserializationContext();
			var buffer = System.Text.Encoding.UTF8.GetBytes( json );
			var obj = SerializationUtility.DeserializeValue<T>( buffer, DataFormat.JSON, context );

			return obj;
		}

		public class LightweightPolicy : ISerializationPolicy
		{
			public string ID => "Lightweight";
			public bool AllowNonSerializableTypes => false;

			public bool ShouldSerializeMember( MemberInfo member )
			{
				// Только явно помеченные
				return member.IsDefined( typeof(OdinSerializeAttribute), true );
			}
		}
	}
}

