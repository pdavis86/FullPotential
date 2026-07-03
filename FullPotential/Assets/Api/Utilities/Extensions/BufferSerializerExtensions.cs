using Unity.Netcode;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class BufferSerializerExtensions
    {
        public static void SerializeStringArray<T>(this BufferSerializer<T> serializer, ref string[] array)
            where T : IReaderWriter
        {
            var length = array?.Length ?? 0;
            serializer.SerializeValue(ref length);

            if (serializer.IsReader)
            {
                array = new string[length];
            }

            if (array == null)
            {
                return;
            }

            for (var i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref array[i]);
            }
        }
    }
}