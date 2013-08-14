<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  exclude-result-prefixes="xsl msxsl user"
  version="1.0">
  
  <xsl:output method="text" indent="no" />
 
  <msxsl:script language="C#" implements-prefix="user">
    <msxsl:assembly name="System.Web" />
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Web" />
    <![CDATA[
    public string GetRelativePath(string from, string to)
    {
      try
      {
        var current = Environment.CurrentDirectory;
        from = System.IO.Path.Combine(current, from.Replace('\\', '/'));
        to = System.IO.Path.Combine(current, to.Replace('\\', '/'));
        return (new Uri(from).MakeRelativeUri(new Uri(to)))
          .ToString().Replace('/', '\\');
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }
    ]]>
  </msxsl:script> 
 
  <xsl:template match="/">
    <xsl:text>
Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010
</xsl:text>
    <xsl:for-each select="/Input/Projects/Project">
      <xsl:call-template name="project-definition">
        <xsl:with-param name="type" select="current()/@Type" />
        <xsl:with-param name="name" select="current()/@Name" />
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="path" select="concat(
                      current()/@Path,
                      '\',
                      current()/@Name,
                      '.',
                      /Input/Generation/Platform,
                      '.csproj')" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject/Project">
      <xsl:call-template name="project-definition">
        <xsl:with-param name="type" select="current()/@Type" />
        <xsl:with-param name="name" select="current()/@Name" />
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="path" select="current()/@Path" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject
                          /Platform[@Type=/Input/Generation/Platform]
                          /Project">
      <xsl:call-template name="project-definition">
        <xsl:with-param name="type" select="current()/@Type" />
        <xsl:with-param name="name" select="current()/@Name" />
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="path" select="current()/@Path" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
                Release|Any CPU = Release|Any CPU
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
</xsl:text>
    <xsl:for-each select="/Input/Projects/Project">
      <xsl:call-template name="project-configuration">
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="root" select="current()" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject/Project">
      <xsl:call-template name="project-configuration">
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="root" select="current()" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject
                          /Platform[@Type=/Input/Generation/Platform]
                          /Project">
      <xsl:call-template name="project-configuration">
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="root" select="current()" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:text>        EndGlobalSection
EndGlobal
</xsl:text>
  </xsl:template>
  
  <xsl:template name="project-definition">
    <xsl:param name="name" />
    <xsl:param name="type" />
    <xsl:param name="path" />
    <xsl:param name="guid" />
    <xsl:text>Project("{</xsl:text>
    <xsl:choose>
      <xsl:when test="$type = 'Content'">
        <xsl:text>9344BDBB-3E7F-41FC-A0DD-8665D75EE146</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>FAE04EC0-301F-11D3-BF4B-00C04F79EFBC</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>}") = </xsl:text>
    <xsl:text>"</xsl:text>
    <xsl:value-of select="$name" />
    <xsl:text>", "</xsl:text>
    <xsl:value-of select="$path" />
    <xsl:text>", "{</xsl:text>
    <xsl:value-of select="$guid" />
    <xsl:text>}"
EndProject
</xsl:text>
  </xsl:template>
  
  <xsl:template name="project-configuration">
    <xsl:param name="guid" />
    <xsl:param name="root" />
    <xsl:variable name="debug-mapping">
      <xsl:value-of select="$root/ConfigurationMapping[@Old='Debug']/@New" />
    </xsl:variable>
    <xsl:variable name="release-mapping">
      <xsl:value-of select="$root/ConfigurationMapping[@Old='Debug']/@New" />
    </xsl:variable>
    <xsl:text>                {</xsl:text>
    <xsl:value-of select="$guid" />
    <xsl:text>}.Debug|Any CPU.ActiveCfg = </xsl:text>
    <xsl:choose>
      <xsl:when test="$debug-mapping != ''">
        <xsl:value-of select="$debug-mapping" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Debug|Any CPU</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>
</xsl:text>
    <xsl:text>                {</xsl:text>
    <xsl:value-of select="$guid" />
    <xsl:text>}.Debug|Any CPU.Build.0 = </xsl:text>
    <xsl:choose>
      <xsl:when test="$debug-mapping != ''">
        <xsl:value-of select="$debug-mapping" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Debug|Any CPU</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>
</xsl:text>
    <xsl:text>                {</xsl:text>
    <xsl:value-of select="$guid" />
    <xsl:text>}.Release|Any CPU.ActiveCfg = </xsl:text>
    <xsl:choose>
      <xsl:when test="$release-mapping != ''">
        <xsl:value-of select="$release-mapping" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Release|Any CPU</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>
</xsl:text>
    <xsl:text>                {</xsl:text>
    <xsl:value-of select="$guid" />
    <xsl:text>}.Release|Any CPU.Build.0 = </xsl:text>
    <xsl:choose>
      <xsl:when test="$release-mapping != ''">
        <xsl:value-of select="$release-mapping" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Release|Any CPU</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>
</xsl:text>
  </xsl:template>
  
  <xsl:template match="*">
    <xsl:element 
      name="{name()}" 
      namespace="http://schemas.microsoft.com/developer/msbuild/2003">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>
