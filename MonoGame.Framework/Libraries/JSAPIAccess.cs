namespace MonoGame.Web
{
    public class JSAPIAccess
    {
        private static JSAPI m_Instance;

        public static JSAPI Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new JSAPI();
                }

                return m_Instance;
            }
        }
    }
}
