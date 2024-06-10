namespace System
{
    public static class ArgumentNullExceptionHelper
    {
        public static void ThrowIfNull(object argument, string argumentParam = "")
        {
            if (argument == null)
                throw new ArgumentException(argumentParam);
        }
    }

    public class ArrayHelper<T>
    {
        private static T[] empty = new T[0];
        public static T[] Empty() { return empty; }
    }
}
