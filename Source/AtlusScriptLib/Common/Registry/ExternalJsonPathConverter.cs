using System;
using System.IO;
using Newtonsoft.Json;

namespace AtlusScriptLib.Common.Registry
{
    internal class ExternalJsonPathConverter : JsonConverter
    {
        public override bool CanConvert( Type objectType ) => false;

        public override bool CanWrite => false;

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            var path = ( string ) reader.Value;
            if ( string.IsNullOrEmpty( path ) )
                return null;

            var fullPath = Path.Combine( LibraryRegistryCache.RegistryDirectoryPath, path );
            var jsonString = File.ReadAllText( fullPath );
            var obj = JsonConvert.DeserializeObject( jsonString, objectType );

            return obj;
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            throw new NotImplementedException();
        }
    }
}