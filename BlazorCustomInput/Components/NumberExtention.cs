namespace BlazorCustomInput.Components
{
    public static class NumberExtention
    {
        public static string GetMaxsize<Tval>()
        {
            var tvalType = Nullable.GetUnderlyingType(typeof(Tval)) ?? typeof(Tval);
            if (tvalType == typeof(short)) return short.MaxValue.ToString();
            else if (tvalType == typeof(int)) return int.MaxValue.ToString();
            else if (tvalType == typeof(uint)) return uint.MaxValue.ToString();
            else if (tvalType == typeof(long)) return long.MaxValue.ToString();
            else if (tvalType == typeof(ulong)) return ulong.MaxValue.ToString();
            else if (tvalType == typeof(float)) return float.MaxValue.ToString();
            else return double.MaxValue.ToString();
        }
    }
}
