using Unity.Netcode;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class BufferSerializerExtensions
    {
        public static void SerializeStringArray<T>(this BufferSerializer<T> serializer, ref string[] array)
            where T : IReaderWriter
        {
            int length = array?.Length ?? 0;
            serializer.SerializeValue(ref length);

            if (serializer.IsReader)
            {
                array = new string[length];
            }

            for (int i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref array[i]);
            }
        }
    }
}
