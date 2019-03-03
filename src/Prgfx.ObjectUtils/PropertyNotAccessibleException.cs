namespace Prgfx.ObjectUtils
{
    [System.Serializable]
    public class PropertyNotAccessibleException : System.Exception
    {
        public PropertyNotAccessibleException() { }
        public PropertyNotAccessibleException(string message) : base(message) { }
        public PropertyNotAccessibleException(string message, System.Exception inner) : base(message, inner) { }
        protected PropertyNotAccessibleException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}