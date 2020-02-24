namespace VaughnVernon.Mockroservices.VaughnVernon.Mockroservices
{
    public static class StreamNameBuilder
    {
        public static string BuildStreamNameFor<T>(string value) => $"{typeof(T).Name}_{value}";
    }
}