using System.Text;
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
    public static class StringBuilderHelper
    {
        public class AppendInterpolatedStringHandler
        {
            private readonly StringBuilder sb;
            public AppendInterpolatedStringHandler(int literalLength, int formattedLength, StringBuilder builder)
            {
                sb = builder;
            }
            public void AppendLiteral(string literal)
            {
                sb.Append(literal);
            }
            public void AppendFormatted(object o)
            {
                sb.Append(o);
            }
        }
        public static void Append(this StringBuilder sb, ref AppendInterpolatedStringHandler handler) { }
    }
    public class ArrayHelper<T>
    {
        private static T[] empty = new T[0];
        public static T[] Empty() { return empty; }
    }
}
