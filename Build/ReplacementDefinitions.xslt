<xsl:choose>
  <xsl:when test="/Input/Generation/Platform = 'Android'">
    <xsl:text>TRACE;ANDROID;GLES;OPENGL</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'iOS'">
    <xsl:text>IOS;GLES;OPENGL</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'Linux'">
    <xsl:text>TRACE;LINUX;OPENGL</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'MacOS'">
    <xsl:text>MONOMAC;OPENGL</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'Ouya'">
    <xsl:text>TRACE;ANDROID;GLES;OPENGL;OUYA</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'PSMobile'">
    <xsl:text>PSM</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'Windows'">
    <xsl:text>TRACE;WINDOWS;DIRECTX;WINDOWS_MEDIA_SESSION</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'Windows8'">
    <xsl:text>TRACE;NETFX_CORE;WINRT;WINDOWS_STOREAPP;DIRECTX;DIRECTX11_1;WINDOWS_MEDIA_ENGINE</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'WindowsGL'">
    <xsl:text>TRACE;WINDOWS;OPENGL</xsl:text>
  </xsl:when>
  <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
    <xsl:text>TRACE;SILVERLIGHT;WINDOWS_PHONE;WINRT;DIRECTX</xsl:text>
  </xsl:when>
</xsl:choose>

