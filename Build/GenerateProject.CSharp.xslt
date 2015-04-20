<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  exclude-result-prefixes="xsl msxsl user"
  version="1.0">

  <xsl:output method="xml" indent="no" />

  <msxsl:script language="C#" implements-prefix="user">
    <msxsl:assembly name="System.Core" />
    <msxsl:assembly name="System.Web" />
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Web" />
    <![CDATA[
    public string NormalizeXAPName(string origName)
    {
      return origName.Replace('.','_');
    }
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

    public bool ProjectAndServiceIsActive(
      string platformString,
      string includePlatformString,
      string excludePlatformString,
      string serviceString,
      string includeServiceString,
      string excludeServiceString,
      string activePlatform,
      string activeServicesString)
    {
      if (!ProjectIsActive(platformString, includePlatformString, excludePlatformString, activePlatform))
      {
        return false;
      }

      return ServiceIsActive(serviceString, includeServiceString, excludeServiceString, activeServicesString);
    }

    public bool ProjectIsActive(
      string platformString,
      string includePlatformString,
      string excludePlatformString,
      string activePlatform)
    {
      // Choose either <Platforms> or <IncludePlatforms>
      if (string.IsNullOrEmpty(platformString))
      {
        platformString = includePlatformString;
      }

      // If the exclude string is set, then we must check this first.
      if (!string.IsNullOrEmpty(excludePlatformString))
      {
        var excludePlatforms = excludePlatformString.Split(',');
        foreach (var i in excludePlatforms)
        {
          if (i == activePlatform)
          {
            // This platform is excluded.
            return false;
          }
        }
      }

      // If the platform string is empty at this point, then we allow
      // all platforms since there's no whitelist of platforms configured.
      if (string.IsNullOrEmpty(platformString))
      {
        return true;
      }

      // Otherwise ensure the platform is in the include list.
      var platforms = platformString.Split(',');
      foreach (var i in platforms)
      {
        if (i == activePlatform)
        {
          return true;
        }
      }

      return false;
    }

    public bool ServiceIsActive(
      string serviceString,
      string includeServiceString,
      string excludeServiceString,
      string activeServicesString)
    {
      var activeServices = activeServicesString.Split(',');

      // Choose either <Services> or <IncludeServices>
      if (string.IsNullOrEmpty(serviceString))
      {
        serviceString = includeServiceString;
      }

      // If the exclude string is set, then we must check this first.
      if (!string.IsNullOrEmpty(excludeServiceString))
      {
        var excludeServices = excludeServiceString.Split(',');
        foreach (var i in excludeServices)
        {
          if (System.Linq.Enumerable.Contains(activeServices, i))
          {
            // This service is excluded.
            return false;
          }
        }
      }

      // If the service string is empty at this point, then we allow
      // all services since there's no whitelist of services configured.
      if (string.IsNullOrEmpty(serviceString))
      {
        return true;
      }

      // Otherwise ensure the service is in the include list.
      var services = serviceString.Split(',');
      foreach (var i in services)
      {
        if (System.Linq.Enumerable.Contains(activeServices, i))
        {
          return true;
        }
      }

      return false;
    }

    public bool IsTrue(string text)
    {
      return text.ToLower() == "true";
    }

    public bool IsTrueDefault(string text)
    {
      return text.ToLower() != "false";
    }

    public string ReadFile(string path)
    {
      path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
      path = path.Replace('\\', System.IO.Path.DirectorySeparatorChar);

      using (var reader = new System.IO.StreamReader(path))
      {
        return reader.ReadToEnd();
      }
    }

    public bool HasXamarinMac()
    {
      return System.IO.File.Exists("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/XamMac.dll");
    }

    public bool CodesignKeyExists()
    {
      var home = System.Environment.GetEnvironmentVariable("HOME");
      if (string.IsNullOrEmpty(home))
      {
        home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
      }
      var path = System.IO.Path.Combine(home, ".codesignkey");
      return System.IO.File.Exists(path);
    }

    public string GetCodesignKey()
    {
      var home = System.Environment.GetEnvironmentVariable("HOME");
      if (string.IsNullOrEmpty(home))
      {
        home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
      }
      var path = System.IO.Path.Combine(home, ".codesignkey");
      using (var reader = new System.IO.StreamReader(path))
      {
        return reader.ReadToEnd().Trim();
      }
    }

    public string CalculateDefines(string addDefines, string removeDefines)
    {
      var addArray = addDefines.Trim(';').Split(';');
      var removeArray = removeDefines.Trim(';').Split(';');

      var list = new System.Collections.Generic.List<string>();
      foreach (var a in addArray)
      {
        if (!list.Contains(a))
        {
          list.Add(a);
        }
      }
      foreach (var r in removeArray)
      {
        if (list.Contains(r))
        {
          list.Remove(r);
        }
      }

      return string.Join(";", list.ToArray());
    }

    public string GetFilename(string name)
    {
      var components = name.Split(new[] { '\\', '/' });
      if (components.Length == 0)
      {
        throw new Exception("No name specified for NativeBinary");
      }
      return components[components.Length - 1];
    }

    ]]>
  </msxsl:script>

  <xsl:variable name="assembly_name">
    <xsl:choose>
      <xsl:when test="/Input/Properties/AssemblyName
	        /Platform[@Name=/Input/Generation/Platform]">
        <xsl:value-of select="/Input/Properties/AssemblyName/Platform[@Name=/Input/Generation/Platform]" />
      </xsl:when>
      <xsl:when test="/Input/Properties/AssemblyName">
        <xsl:value-of select="/Input/Properties/AssemblyName" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Input/Projects/Project[@Name=/Input/Generation/ProjectName]/@Name" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template name="profile_and_version"
    xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <xsl:choose>
      <xsl:when test="/Input/Properties/FrameworkVersions
                      /Platform[@Name=/Input/Generation/Platform]
                      /Version">
        <TargetFrameworkVersion>
          <xsl:value-of select="/Input/Properties/FrameworkVersions
                                                      /Platform[@Name=/Input/Generation/Platform]
                                                      /Version" />
        </TargetFrameworkVersion>
      </xsl:when>
      <xsl:when test="/Input/Properties/FrameworkVersions/Version">
        <TargetFrameworkVersion>
          <xsl:value-of select="/Input/Properties/FrameworkVersions/Version" />
        </TargetFrameworkVersion>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="/Input/Generation/Platform = 'Android'">
            <TargetFrameworkVersion>v4.2</TargetFrameworkVersion>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Ouya'">
            <TargetFrameworkVersion>v4.2</TargetFrameworkVersion>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Windows8'">
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
            <TargetFrameworkVersion>v8.0</TargetFrameworkVersion>
            <TargetFrameworkIdentifier>WindowsPhone</TargetFrameworkIdentifier>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
            <TargetPlatformVersion>8.1</TargetPlatformVersion>
            <MinimumVisualStudioVersion>12</MinimumVisualStudioVersion>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsUAP'">
			  <TargetPlatformIdentifier>UAP</TargetPlatformIdentifier>
			  <TargetPlatformVersion>10.0.10030.0</TargetPlatformVersion>
			  <TargetPlatformMinVersion>10.0.10030.0</TargetPlatformMinVersion>
			  <MinimumVisualStudioVersion>14</MinimumVisualStudioVersion>
		  </xsl:when>			
          <xsl:when test="/Input/Generation/Platform = 'iOS' or /Input/Generation/Platform = 'PSMobile'">
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'PCL'">
            <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
            <TargetFrameworkProfile>Profile328</TargetFrameworkProfile>
            <MinimumVisualStudioVersion>10.0</MinimumVisualStudioVersion>
          </xsl:when>
          <xsl:otherwise>
            <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="/Input/Properties/FrameworkVersions
                      /Platform[@Name=/Input/Generation/Platform]
                      /Profile">
        <TargetFrameworkProfile>
          <xsl:value-of select="/Input/Properties/FrameworkVersions
                                                      /Platform[@Name=/Input/Generation/Platform]
                                                      /Profile" />
        </TargetFrameworkProfile>
      </xsl:when>
      <xsl:when test="/Input/Properties/FrameworkVersions/Profile">
        <TargetFrameworkProfile>
          <xsl:value-of select="/Input/Properties/FrameworkVersions/Profile" />
        </TargetFrameworkProfile>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'WindowsUAP' or /Input/Generation/Platform = 'PSMobile' or /Input/Generation/Platform = 'PCL'">
      </xsl:when>
      <xsl:otherwise>
        <TargetFrameworkProfile></TargetFrameworkProfile>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'WindowsUAP'">
      <DefaultLanguage>en-US</DefaultLanguage>
    </xsl:if>
  </xsl:template>

  <xsl:template name="configuration"
    xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <xsl:param name="type" />
    <xsl:param name="debug" />
    <xsl:param name="config" />
    <xsl:param name="platform" />
    <xsl:param name="projectname" />
    <PropertyGroup>
      <xsl:attribute name="Condition">
        <xsl:text> '$(Configuration)|$(Platform)' == '</xsl:text>
        <xsl:value-of select="$config" />
	<xsl:text>|</xsl:text>
	<xsl:value-of select="$platform" />
        <xsl:text>' </xsl:text>
      </xsl:attribute>
    <xsl:choose>
      <xsl:when test="$debug = 'true'">
        <DebugSymbols>true</DebugSymbols>
        <Optimize>false</Optimize>
        <DebugType>full</DebugType>
        <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
          <!-- This ensures that DirectX errors are reported to the Output window on Windows. -->
        <EnableUnmanagedDebugging>true</EnableUnmanagedDebugging>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <Optimize>true</Optimize>
				<DebugType>
          <xsl:choose>
            <xsl:when test="/Input/Properties/DebugSymbolsOnRelease">
              <xsl:value-of select="/Input/Properties/DebugSymbolsOnRelease" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>none</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
				</DebugType>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:variable name="platform_path">
      <xsl:choose>
        <xsl:when test="$type = 'Website'">
          <xsl:text></xsl:text>
        </xsl:when>
        <!-- 
            IMPORTANT: When modifying this, or adding new options, 
            remember to update AutomaticProjectPackager as well.
        -->
        <xsl:when test="user:IsTrue(/Input/Properties/ProjectSpecificOutputFolder)">
          <xsl:value-of select="$projectname" />
          <xsl:text>\</xsl:text>
          <xsl:value-of select="/Input/Generation/Platform" />
          <xsl:text>\</xsl:text>
          <xsl:value-of select="$platform" />
          <xsl:text>\</xsl:text>
          <xsl:value-of select="$config" />
        </xsl:when>
        <xsl:when test="user:IsTrueDefault(/Input/Properties/PlatformSpecificOutputFolder)">
          <xsl:value-of select="/Input/Generation/Platform" />
      	  <xsl:text>\</xsl:text>
      	  <xsl:value-of select="$platform" />
      	  <xsl:text>\</xsl:text>
      	  <xsl:value-of select="$config" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$config" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <OutputPath><xsl:text>bin\</xsl:text><xsl:copy-of select="$platform_path" /></OutputPath>
    <IntermediateOutputPath><xsl:text>obj\</xsl:text><xsl:copy-of select="$platform_path" /></IntermediateOutputPath>
    <DocumentationFile><xsl:text>bin\</xsl:text><xsl:copy-of select="$platform_path" /><xsl:text>\</xsl:text><xsl:copy-of select="$assembly_name" /><xsl:text>.xml</xsl:text></DocumentationFile>
    <DefineConstants>
      <xsl:variable name="addDefines">
        <xsl:if test="$debug = 'true'">
          <xsl:text>DEBUG;</xsl:text>
        </xsl:if>
        <xsl:for-each select="/Input/Services/Service[@Project=/Input/Generation/ProjectName]">
          <xsl:for-each select="./AddDefines/AddDefine">
            <xsl:value-of select="." />
            <xsl:text>;</xsl:text>
          </xsl:for-each>
        </xsl:for-each>
        <xsl:choose>
          <xsl:when test="/Input/Properties/CustomDefinitions">
            <xsl:for-each select="/Input/Properties/CustomDefinitions/Platform">
              <xsl:if test="/Input/Generation/Platform = ./@Name">
                <xsl:value-of select="." />
              </xsl:if>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="/Input/Generation/Platform = 'Android'">
                <xsl:text>PLATFORM_ANDROID</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'iOS'">
                <xsl:text>PLATFORM_IOS</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'Linux'">
                <xsl:text>PLATFORM_LINUX</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'MacOS'">
                <xsl:text>PLATFORM_MACOS</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'Ouya'">
                <xsl:text>PLATFORM_OUYA</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'PSMobile'">
                <xsl:text>PLATFORM_PSMOBILE</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'Windows'">
                <xsl:text>PLATFORM_WINDOWS</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'Windows8'">
                <xsl:text>PLATFORM_WINDOWS8</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'WindowsGL'">
                <xsl:text>PLATFORM_WINDOWSGL</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
                <xsl:text>PLATFORM_WINDOWSPHONE</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
                <xsl:text>PLATFORM_WINDOWSPHONE81</xsl:text>
              </xsl:when>
              <xsl:when test="/Input/Generation/Platform = 'Web'">
                <xsl:text>PLATFORM_WEB</xsl:text>
              </xsl:when>
            </xsl:choose>
            <xsl:text>;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="removeDefines">
        <xsl:for-each select="/Input/Services/Service[@Project=/Input/Generation/ProjectName]">
          <xsl:for-each select="./RemoveDefines/RemoveDefine">
            <xsl:value-of select="." />
            <xsl:text>;</xsl:text>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:variable>
      <xsl:value-of select="user:CalculateDefines($addDefines, $removeDefines)" />
    </DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <xsl:choose>
      <xsl:when test="/Input/Properties/ForceArchitecture">
        <PlatformTarget>
          <xsl:value-of select="/Input/Properties/ForceArchitecture" />
        </PlatformTarget>
      </xsl:when>
    </xsl:choose>
    <xsl:if test="user:IsTrue(/Input/Properties/CheckForOverflowUnderflow)">
      <CheckForOverflowUnderflow>True</CheckForOverflowUnderflow>
    </xsl:if>
    <!--<xsl:call-template name="profile_and_version" />-->
    <xsl:choose>
      <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
        <xsl:choose>
          <xsl:when test="$debug = 'true'">
            <MonoDroidLinkMode>None</MonoDroidLinkMode>
            <AndroidLinkMode>None</AndroidLinkMode>
          </xsl:when>
          <xsl:otherwise>
            <AndroidUseSharedRuntime>False</AndroidUseSharedRuntime>
            <!--<AndroidLinkMode>SdkOnly</AndroidLinkMode>-->
            <EmbedAssembliesIntoApk>True</EmbedAssembliesIntoApk>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'iOS'">
        <xsl:if test="$debug = 'true'">
          <MtouchDebug>True</MtouchDebug>
        </xsl:if>
        <MtouchUseArmv7>
          <xsl:choose>
            <xsl:when test="/Input/Properties/iOSUseArmv7">
              <xsl:value-of select="/Input/Properties/iOSUseArmv7" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>false</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </MtouchUseArmv7>
        <xsl:if test="/Input/Properties/iOSUseLlvm">
          <MtouchUseLlvm>
            <xsl:value-of select="/Input/Properties/iOSUseLlvm" />
          </MtouchUseLlvm>
        </xsl:if>
        <xsl:if test="/Input/Properties/iOSUseSGen">
          <MtouchUseSGen>
            <xsl:value-of select="/Input/Properties/iOSUseSGen" />
          </MtouchUseSGen>
        </xsl:if>
        <xsl:if test="/Input/Properties/iOSUseRefCounting">
          <MtouchUseRefCounting>
            <xsl:value-of select="/Input/Properties/iOSUseRefCounting" />
          </MtouchUseRefCounting>
        </xsl:if>
        <xsl:if test="/Input/Properties/iOSI18n">
          <MtouchI18n>
            <xsl:value-of select="/Input/Properties/iOSI18n" />
          </MtouchI18n>
        </xsl:if>
        <xsl:if test="/Input/Properties/iOSArch">
          <MtouchArch>
            <xsl:value-of select="/Input/Properties/iOSArch" />
          </MtouchArch>
        </xsl:if>
        <xsl:if test="/Input/Properties/iOSExtraArgs">
          <MtouchExtraArgs>
            <xsl:value-of select="/Input/Properties/iOSExtraArgs" />
          </MtouchExtraArgs>
        </xsl:if>
        <xsl:if test="/Input/Properties/SignAssembly">
          <SignAssembly>
            <xsl:value-of select="/Input/Properties/SignAssembly" />
          </SignAssembly>
        </xsl:if>
        <xsl:if test="user:CodesignKeyExists()">
          <CodesignKey>
            <xsl:value-of select="user:GetCodesignKey()" />
          </CodesignKey>
        </xsl:if>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'MacOS'">
        <EnableCodeSigning>False</EnableCodeSigning>
        <CreatePackage>False</CreatePackage>
        <EnablePackageSigning>False</EnablePackageSigning>
        <xsl:choose>
          <xsl:when test="user:HasXamarinMac()">
            <xsl:choose>
              <xsl:when test="/Input/Properties/IncludeMonoRuntimeOnMac">
                <IncludeMonoRuntime><xsl:value-of select="/Input/Properties/IncludeMonoRuntimeOnMac" /></IncludeMonoRuntime>
	            <xsl:if test="/Input/Properties/MonoMacRuntimeLinkMode">
	              <LinkMode><xsl:value-of select="/Input/Properties/MonoMacRuntimeLinkMode" /></LinkMode>
	            </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                <IncludeMonoRuntime>False</IncludeMonoRuntime>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <IncludeMonoRuntime>False</IncludeMonoRuntime>
          </xsl:otherwise>
        </xsl:choose>
        <UseSGen>False</UseSGen>
      </xsl:when>
    </xsl:choose>
    </PropertyGroup>
  </xsl:template>

  <xsl:template name="NativeBinary"
    xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <xsl:param name="project_path" />
    <xsl:param name="project_name" />
    <xsl:param name="path" />
    <None>
      <xsl:attribute name="Include">
        <xsl:value-of
          select="user:GetRelativePath(
            concat(
              $project_path,
              '\',
              $project_name,
              '.',
              /Input/Generation/Platform,
              '.csproj'),
            $path)" />
      </xsl:attribute>
      <Link><xsl:value-of select="user:GetFilename($path)" /></Link>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </xsl:template>

  <xsl:template match="/">

    <xsl:variable
      name="project"
      select="/Input/Projects/Project[@Name=/Input/Generation/ProjectName]" />

    <xsl:variable name="ToolsVersion">
      <xsl:variable name="__FrameworkVersion">
        <xsl:choose>
          <xsl:when test="/Input/Properties/FrameworkVersions
                          /Platform[@Name=/Input/Generation/Platform]
                          /Version">
            <TargetFrameworkVersion>
              <xsl:value-of select="/Input/Properties/FrameworkVersions
                                                          /Platform[@Name=/Input/Generation/Platform]
                                                          /Version" />
            </TargetFrameworkVersion>
          </xsl:when>
          <xsl:when test="/Input/Properties/FrameworkVersions/Version">
            <TargetFrameworkVersion>
              <xsl:value-of select="/Input/Properties/FrameworkVersions/Version" />
            </TargetFrameworkVersion>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Unset</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:choose>
        <xsl:when test="/Input/Generation/Platform = 'WindowsUAP'">
          <xsl:text>14.0</xsl:text>
        </xsl:when>		  
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
          <xsl:text>12.0</xsl:text>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'PCL'">
          <xsl:text>12.0</xsl:text>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'Windows' or 
          /Input/Generation/Platform = 'MacOS' or 
          /Input/Generation/Platform = 'Linux'">
          <!--
            We have to choose the ToolsVersion based on the framework, since .NET 4.5
            and later use a ToolsVersion that aligns with Visual Studio's version.
          -->
          <xsl:choose>
            <xsl:when test="$__FrameworkVersion = 'v4.5'">
              <xsl:text>12.0</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>4.0</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>4.0</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <Project
      DefaultTargets="Build"
      xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="{$ToolsVersion}">

      <xsl:if test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'WindowsPhone81' or /Input/Generation/Platform = 'WindowsUAP' or /Input/Generation/Platform = 'PCL'">
        <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
      </xsl:if>

      <PropertyGroup>
        <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
        <xsl:choose>
          <xsl:when test="/Input/Properties/ForceArchitecture">
            <Platform Condition=" '$(Platform)' == '' ">
              <xsl:value-of select="/Input/Properties/ForceArchitecture" />
            </Platform>
          </xsl:when>
          <xsl:otherwise>
            <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
			<xsl:when test="/Input/Generation/Platform = 'WindowsUAP'">
				<FileAlignment>512</FileAlignment>
			</xsl:when>
			<xsl:when test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'WindowsPhone81'">
            <ProductVersion>8.0.30703</ProductVersion>
            <SchemaVersion>2.0</SchemaVersion>				
          </xsl:when>
          <xsl:otherwise>
            <ProductVersion>10.0.0</ProductVersion>
            <SchemaVersion>2.0</SchemaVersion>
		  </xsl:otherwise>
        </xsl:choose>
        <ProjectGuid>{<xsl:value-of select="$project/@Guid" />}</ProjectGuid>
        <xsl:choose>
          <xsl:when test="$project/@Type = 'Website'">
            <ProjectTypeGuids>
              <xsl:text>{349C5851-65DF-11DA-9384-00065B846F21};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
            <ProjectTypeGuids>
              <xsl:text>{EFBA0AD7-5A72-4C68-AF49-83D382785DCF};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'iOS'">
            <ProjectTypeGuids>
              <xsl:choose>
                <xsl:when test="user:IsTrue(/Input/Properties/UseLegacyiOSAPI)">
                  <xsl:text>{6BC8ED88-2882-458C-8E55-DFD12B67127B};</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>{FEACFBD2-3405-455C-9665-78FE426C6842};</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'MacOS'">
            <ProjectTypeGuids>
              <xsl:choose>
                <xsl:when test="user:HasXamarinMac()">
                  <xsl:text>{42C0BBD9-55CE-4FC1-8D90-A7348ABAFB23};</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>{948B3504-5B70-4649-8FE4-BDE1FB46EC69};</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'PSMobile'">
            <ProjectTypeGuids>
              <xsl:text>{69878862-DA7D-4DC6-B0A1-50D8FAB4242F};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Windows8'">
            <ProjectTypeGuids>
              <xsl:text>{BC8A1FFA-BEE3-4634-8014-F334798102B3};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
            <ProjectTypeGuids>
              <xsl:text>{C089C8C0-30E0-4E22-80C0-CE093F111A43};</xsl:text>
              <xsl:text>{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
            <ProjectTypeGuids>
              <xsl:text>{76F1466A-8B6D-4E39-A767-685A06062A39};</xsl:text>
              <xsl:text>{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsUAP'">
            <ProjectTypeGuids>
              <xsl:text>{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>			
          <xsl:when test="/Input/Generation/Platform = 'PCL'">
            <ProjectTypeGuids>
              <xsl:text>{786C830F-07A1-408B-BD7F-6EE04809D6DB};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:otherwise>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="/Input/Properties/WindowsApplicationIcon">
          <ApplicationIcon><xsl:value-of select="/Input/Properties/WindowsApplicationIcon" /></ApplicationIcon>
        </xsl:if>
        <OutputType>
          <xsl:choose>
            <xsl:when test="$project/@Type = 'XNA'">
              <xsl:choose>
                <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                 <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows8'">
                  <xsl:text>AppContainerExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows'">
                  <xsl:text>WinExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'iOS'">
                  <xsl:text>Exe</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Exe</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$project/@Type = 'Console'">
              <xsl:text>Exe</xsl:text>
            </xsl:when>
            <xsl:when test="$project/@Type = 'GUI'">
              <xsl:text>WinExe</xsl:text>
            </xsl:when>
            <xsl:when test="$project/@Type = 'GTK'">
              <xsl:text>WinExe</xsl:text>
            </xsl:when>
            <xsl:when test="$project/@Type = 'App'">
              <xsl:choose>
                <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows8'">
                  <xsl:text>AppContainerExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows'">
                  <xsl:text>WinExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'iOS'">
                  <xsl:text>Exe</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Exe</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>Library</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </OutputType>
        <RootNamespace>
          <xsl:choose>
            <xsl:when test="/Input/Properties/RootNamespace">
              <xsl:value-of select="/Input/Properties/RootNamespace" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$project/@Name" />
            </xsl:otherwise>
          </xsl:choose>
        </RootNamespace>
        <AssemblyName><xsl:copy-of select="$assembly_name" /></AssemblyName>
        <xsl:if test="/Input/Generation/Platform != 'PCL'">
          <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
        </xsl:if>
        <NoWarn><xsl:value-of select="/Input/Properties/NoWarn" /></NoWarn>
        <xsl:call-template name="profile_and_version" />
        <xsl:choose>
          <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
            <FileAlignment>512</FileAlignment>
            <AndroidSupportedAbis>armeabi,armeabi-v7a,x86</AndroidSupportedAbis>
            <AndroidStoreUncompressedFileExtensions />
            <MandroidI18n />
            <DeployExternal>False</DeployExternal>
            <xsl:if test="$project/@Type = 'App'">
              <xsl:choose>
                <xsl:when test="Input/Properties/ManifestPrefix">
                  <AndroidManifest>
                    <xsl:value-of select="concat(
                                  '..\',
                                  $project/@Name,
                                  '.',
                                  /Input/Generation/Platform,
                                  '\Properties\AndroidManifest.xml')"/>
                  </AndroidManifest>
                </xsl:when>
                <xsl:otherwise>
                  <AndroidManifest>Properties\AndroidManifest.xml</AndroidManifest>
                </xsl:otherwise>
              </xsl:choose>
              <AndroidApplication>True</AndroidApplication>
              <AndroidResgenFile>Resources\Resource.designer.cs</AndroidResgenFile>
              <AndroidResgenClass>Resource</AndroidResgenClass>
            </xsl:if>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'iOS'">
            <SynchReleaseVersion>False</SynchReleaseVersion>
            <xsl:choose>
              <xsl:when test="$project/@Type = 'App'">
                <ConsolePause>false</ConsolePause>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'MacOS'">
            <xsl:if test="user:HasXamarinMac() = false()">
              <SuppressXamMacUpsell>True</SuppressXamMacUpsell>
            </xsl:if>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
            <xsl:choose>
              <xsl:when test="$project/@Type = 'App'">
                <SilverlightVersion>$(TargetFrameworkVersion)</SilverlightVersion>
                <SilverlightApplication>true</SilverlightApplication>
                <XapFilename>
                  <xsl:value-of select="concat( user:NormalizeXAPName(
                                concat($project/@Name ,'_$(Configuration)','_$(Platform)')),'.xap'
                                )"/>
                </XapFilename>
                <XapOutputs>true</XapOutputs>
                <GenerateSilverlightManifest>true</GenerateSilverlightManifest>
                <xsl:choose>
                  <xsl:when test="Input/Properties/ManifestPrefix">
                    <SilverlightManifestTemplate>
                      <xsl:value-of select="concat(
                        '..\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '\Properties\AppManifest.xml')"/>
                    </SilverlightManifestTemplate>
                  </xsl:when>
                  <xsl:otherwise>
                    <SilverlightManifestTemplate>Properties\AppManifest.xml</SilverlightManifestTemplate>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </PropertyGroup>
      <xsl:choose>
        <xsl:when test="/Input/Generation/Platform = 'iOS'">
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">iPhoneSimulator</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">iPhoneSimulator</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Ad-Hoc</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">AppStore</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">x86</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">x86</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">ARM</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">ARM</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
            <xsl:with-param name="projectname"><xsl:value-of select="$project/@Name" /></xsl:with-param>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="$project/@Type = 'Website'">
          <Import>
            <xsl:attribute name="Project">
              <xsl:text>$(MSBuildExtensionsPath)\Microsoft\</xsl:text>
              <xsl:text>VisualStudio\v10.0\WebApplications\</xsl:text>
              <xsl:text>Microsoft.WebApplication.targets</xsl:text>
            </xsl:attribute>
          </Import>
          <xsl:if test="/Input/Properties/RazorGeneratorTargetsPath != ''">
            <Import>
              <xsl:attribute name="Project">
                <xsl:value-of select="/Input/Properties/RazorGeneratorTargetsPath" />
              </xsl:attribute>
            </Import>
            <Target Name="BeforeBuild">
              <CallTarget Targets="PrecompileRazorFiles" />
            </Target>
          </xsl:if>
        </xsl:when>
      </xsl:choose>

      <ItemGroup>
        <xsl:if test="$project/@Type = 'GTK'">
          <Reference Include="gtk-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="gdk-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="glib-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="glade-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="pango-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="atk-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
        </xsl:if>

        <xsl:if test="/Input/Generation/Platform = 'MacOS'">
          <xsl:choose>
            <xsl:when test="user:HasXamarinMac()">
              <Reference Include="XamMac" />
            </xsl:when>
            <xsl:otherwise>
              <Reference Include="MonoMac" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>

        <xsl:if test="/Input/Generation/Platform = 'iOS'">
          <xsl:choose>
            <xsl:when test="user:IsTrue(/Input/Properties/UseLegacyiOSAPI)">
              <Reference Include="monotouch" />
            </xsl:when>
            <xsl:otherwise>
              <Reference Include="Xamarin.iOS" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-path" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-path]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-path]) = 0">
              <xsl:if test="
                count(/Input/Projects/ContentProject[@Name=$include-path]) = 0">

                <Reference>
                  <xsl:attribute name="Include">
                    <xsl:value-of select="@Include" />
                  </xsl:attribute>
                  <xsl:text />
                </Reference>
              </xsl:if>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/Reference">
                <xsl:variable name="refd-name" select="@Include" />
                <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) = 0 and 
                              count(/Input/Projects/ExternalProject[@Name=$refd-name]) = 0">
                  <Reference>
                    <xsl:attribute name="Include">
                      <xsl:value-of select="@Include" />
                    </xsl:attribute>
                    <xsl:for-each select="./Alias">
                      <xsl:if test="@Platform = /Input/Generation/Platform">
                        <Aliases><xsl:value-of select="." /></Aliases>
                      </xsl:if>
                    </xsl:for-each>
                  </Reference>
                </xsl:if>
              </xsl:for-each>
              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Reference">
                  <xsl:variable name="refd-name" select="@Include" />
                  <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) = 0 and 
                                count(/Input/Projects/ExternalProject[@Name=$refd-name]) = 0">
                    <Reference>
                      <xsl:attribute name="Include">
                        <xsl:value-of select="@Include" />
                      </xsl:attribute>
                      <xsl:for-each select="./Alias">
                        <xsl:if test="@Platform = /Input/Generation/Platform">
                          <Aliases><xsl:value-of select="." /></Aliases>
                        </xsl:if>
                      </xsl:for-each>
                    </Reference>
                  </xsl:if>
                </xsl:for-each>
                <xsl:for-each select="./Service">
                  <xsl:if test="user:ServiceIsActive(
                    ./@Name,
                    '',
                    '',
                    /Input/Services/ActiveServicesNames)">
                    <xsl:for-each select="./Reference">
                      <xsl:variable name="refd-name" select="@Include" />
                      <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) = 0 and 
                                    count(/Input/Projects/ExternalProject[@Name=$refd-name]) = 0">
                        <Reference>
                          <xsl:attribute name="Include">
                            <xsl:value-of select="@Include" />
                          </xsl:attribute>
                          <xsl:for-each select="./Alias">
                            <xsl:if test="@Platform = /Input/Generation/Platform">
                              <Aliases><xsl:value-of select="." /></Aliases>
                            </xsl:if>
                          </xsl:for-each>
                        </Reference>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>
              <xsl:for-each select="$extern/Service">
                <xsl:if test="user:ServiceIsActive(
                  ./@Name,
                  '',
                  '',
                  /Input/Services/ActiveServicesNames)">
                  <xsl:for-each select="./Reference">
                    <xsl:variable name="refd-name" select="@Include" />
                    <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) = 0 and 
                                  count(/Input/Projects/ExternalProject[@Name=$refd-name]) = 0">
                      <Reference>
                        <xsl:attribute name="Include">
                          <xsl:value-of select="@Include" />
                        </xsl:attribute>
                        <xsl:for-each select="./Alias">
                          <xsl:if test="@Platform = /Input/Generation/Platform">
                            <Aliases><xsl:value-of select="." /></Aliases>
                          </xsl:if>
                        </xsl:for-each>
                      </Reference>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:if>
              </xsl:for-each>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:if test="/Input/Generation/Platform = 'Web'">
          <Reference>
            <xsl:attribute name="Include">
              <xsl:text>JSIL.Meta</xsl:text>
            </xsl:attribute>
            <HintPath>
              <xsl:value-of select="/Input/Generation/JSILDirectory" />
              <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                <xsl:text>/</xsl:text>
              </xsl:if>
              <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
                <xsl:text>\</xsl:text>
              </xsl:if>
              <xsl:text>JSIL.Meta.dll</xsl:text>
            </HintPath>
          </Reference>
        </xsl:if>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/Binary">
                <Reference>
                  <xsl:attribute name="Include">
                    <xsl:value-of select="@Name" />
                  </xsl:attribute>
                  <xsl:for-each select="./Alias">
                    <xsl:if test="@Platform = /Input/Generation/Platform">
                      <Aliases><xsl:value-of select="." /></Aliases>
                    </xsl:if>
                  </xsl:for-each>
                  <HintPath>
                    <xsl:value-of
                      select="user:GetRelativePath(
                        concat(
                          $project/@Path,
                          '\',
                          $project/@Name,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'),
                        @Path)" />
                  </HintPath>
                </Reference>
              </xsl:for-each>
              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Binary">
                  <Reference>
                    <xsl:attribute name="Include">
                      <xsl:value-of select="@Name" />
                    </xsl:attribute>
                    <xsl:for-each select="./Alias">
                      <xsl:if test="@Platform = /Input/Generation/Platform">
                        <Aliases><xsl:value-of select="." /></Aliases>
                      </xsl:if>
                    </xsl:for-each>
                    <HintPath>
                      <xsl:value-of
                        select="user:GetRelativePath(
                          concat(
                            $project/@Path,
                            '\',
                            $project/@Name,
                            '.',
                            /Input/Generation/Platform,
                            '.csproj'),
                          @Path)" />
                    </HintPath>
                  </Reference>
                </xsl:for-each>
                <xsl:for-each select="./Service">
                  <xsl:if test="user:ServiceIsActive(
                    ./@Name,
                    '',
                    '',
                    /Input/Services/ActiveServicesNames)">
                    <xsl:for-each select="./Binary">
                      <Reference>
                        <xsl:attribute name="Include">
                          <xsl:value-of select="@Name" />
                        </xsl:attribute>
                        <xsl:for-each select="./Alias">
                          <xsl:if test="@Platform = /Input/Generation/Platform">
                            <Aliases><xsl:value-of select="." /></Aliases>
                          </xsl:if>
                        </xsl:for-each>
                        <HintPath>
                          <xsl:value-of
                            select="user:GetRelativePath(
                              concat(
                                $project/@Path,
                                '\',
                                $project/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '.csproj'),
                              @Path)" />
                        </HintPath>
                      </Reference>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>
              <xsl:for-each select="$extern/Service">
                <xsl:if test="user:ServiceIsActive(
                  ./@Name,
                  '',
                  '',
                  /Input/Services/ActiveServicesNames)">
                  <xsl:for-each select="./Binary">
                    <Reference>
                      <xsl:attribute name="Include">
                        <xsl:value-of select="@Name" />
                      </xsl:attribute>
                      <xsl:for-each select="./Alias">
                        <xsl:if test="@Platform = /Input/Generation/Platform">
                          <Aliases><xsl:value-of select="." /></Aliases>
                        </xsl:if>
                      </xsl:for-each>
                      <HintPath>
                        <xsl:value-of
                          select="user:GetRelativePath(
                            concat(
                              $project/@Path,
                              '\',
                              $project/@Name,
                              '.',
                              /Input/Generation/Platform,
                              '.csproj'),
                            @Path)" />
                      </HintPath>
                    </Reference>
                  </xsl:for-each>
                </xsl:if>
              </xsl:for-each>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="/Input/NuGet/Package">
          <Reference>
            <xsl:attribute name="Include">
              <xsl:value-of select="@Name" />
            </xsl:attribute>
            <HintPath>
              <xsl:value-of
                select="user:GetRelativePath(
                  concat(
                    $project/@Path,
                    '\',
                    $project/@Name,
                    '.',
                    /Input/Generation/Platform,
                    '.csproj'),
                  .)" />
            </HintPath>
          </Reference>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Compile">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/None">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/NativeBinary">
                <xsl:call-template name="NativeBinary">
                  <xsl:with-param name="project_path"><xsl:value-of select="$project/@Path" /></xsl:with-param>
                  <xsl:with-param name="project_name"><xsl:value-of select="$project/@Name" /></xsl:with-param>
                  <xsl:with-param name="path"><xsl:value-of select="@Path" /></xsl:with-param>
                </xsl:call-template>
              </xsl:for-each>
              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./NativeBinary">
                  <xsl:call-template name="NativeBinary">
                    <xsl:with-param name="project_path"><xsl:value-of select="$project/@Path" /></xsl:with-param>
                    <xsl:with-param name="project_name"><xsl:value-of select="$project/@Name" /></xsl:with-param>
                    <xsl:with-param name="path"><xsl:value-of select="@Path" /></xsl:with-param>
                  </xsl:call-template>
                </xsl:for-each>
                <xsl:for-each select="./Service">
                  <xsl:if test="user:ServiceIsActive(
                    ./@Name,
                    '',
                    '',
                    /Input/Services/ActiveServicesNames)">
                    <xsl:for-each select="./NativeBinary">
                      <xsl:call-template name="NativeBinary">
                        <xsl:with-param name="project_path"><xsl:value-of select="$project/@Path" /></xsl:with-param>
                        <xsl:with-param name="project_name"><xsl:value-of select="$project/@Name" /></xsl:with-param>
                        <xsl:with-param name="path"><xsl:value-of select="@Path" /></xsl:with-param>
                      </xsl:call-template>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>
              <xsl:for-each select="$extern/Service">
                <xsl:if test="user:ServiceIsActive(
                  ./@Name,
                  '',
                  '',
                  /Input/Services/ActiveServicesNames)">
                  <xsl:for-each select="./NativeBinary">
                    <xsl:call-template name="NativeBinary">
                      <xsl:with-param name="project_path"><xsl:value-of select="$project/@Path" /></xsl:with-param>
                      <xsl:with-param name="project_name"><xsl:value-of select="$project/@Name" /></xsl:with-param>
                      <xsl:with-param name="path"><xsl:value-of select="@Path" /></xsl:with-param>
                    </xsl:call-template>
                  </xsl:for-each>
                </xsl:if>
              </xsl:for-each>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Content">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/EmbeddedResource">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/EmbeddedNativeLibrary">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/EmbeddedShaderProgram">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <xsl:if test="/Input/Generation/Platform = 'Web'">
        <xsl:if test="$project/@Type = 'App' or $project/@Type = 'Console' or $project/@Type = 'GUI' or $project/@Type = 'GTK'">
          <ItemGroup>
            <xsl:for-each select="/Input/Generation/JSILLibraries/Library">
              <None>
                <xsl:attribute name="Include">
                  <xsl:value-of select="./@Path" />
                </xsl:attribute>
                <Link>
                  <xsl:text>Libraries\</xsl:text>
                  <xsl:value-of select="./@Name" />
                </Link>
                <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
              </None>
            </xsl:for-each>
            <None Include="index.htm">
              <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
            </None>
          </ItemGroup>
        </xsl:if>
      </xsl:if>

      <ItemGroup>
        <xsl:for-each select="$project/Files/ShaderProgram">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/ApplicationDefinition">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Page">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/AppxManifest">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/BundleResource">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/InterfaceDefinition">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/AndroidResource">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/SplashScreen">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Resource">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/XamarinComponentReference">
          <xsl:if test="user:ProjectAndServiceIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              ./Services,
              ./IncludeServices,
              ./ExcludeServices,
              /Input/Generation/Platform,
              /Input/Services/ActiveServicesNames)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-path" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/ContentProject[@Name=$include-path]) > 0">

            <xsl:for-each select="/Input
                                  /Projects
                                  /ContentProject[@Name=$include-path]
                                  /Compiled">
              <xsl:choose>
                <xsl:when test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'Windows'">
                  <Content>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Content</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </Content>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
                  <AndroidAsset>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Assets</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                  </AndroidAsset>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'MacOS' or /Input/Generation/Platform = 'iOS'">
                  <Content>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Content</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </Content>
                </xsl:when>
                <xsl:otherwise>
                  <None>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Content</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </None>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <xsl:choose>
        <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
          <Import Project="$(MSBuildExtensionsPath)\Novell\Novell.MonoDroid.CSharp.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'Windows8'">
          <PropertyGroup Condition=" '$(VisualStudioVersion)' == '' or '$(VisualStudioVersion)' &lt; '11.0' ">
            <VisualStudioVersion>11.0</VisualStudioVersion>
          </PropertyGroup>
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\WindowsXaml\v$(VisualStudioVersion)\Microsoft.Windows.UI.Xaml.CSharp.targets" />
        </xsl:when>
		  <xsl:when test="/Input/Generation/Platform = 'WindowsUAP'">
			  <PropertyGroup Condition=" '$(VisualStudioVersion)' == '' or '$(VisualStudioVersion)' &lt; '14.0' ">
				  <VisualStudioVersion>14.0</VisualStudioVersion>
			  </PropertyGroup>
			  <Import Project="$(MSBuildExtensionsPath)\Microsoft\WindowsXaml\v$(VisualStudioVersion)\Microsoft.Windows.UI.Xaml.CSharp.targets" />
		  </xsl:when>
		  <xsl:when test="/Input/Generation/Platform = 'WindowsPhone81'">
          <PropertyGroup Condition=" '$(VisualStudioVersion)' == '' or '$(VisualStudioVersion)' &lt; '12.0' ">
            <VisualStudioVersion>12.0</VisualStudioVersion>
          </PropertyGroup>
          <PropertyGroup Condition=" '$(TargetPlatformIdentifier)' == '' ">
            <TargetPlatformIdentifier>WindowsPhoneApp</TargetPlatformIdentifier>
          </PropertyGroup>
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\WindowsXaml\v$(VisualStudioVersion)\Microsoft.Windows.UI.Xaml.CSharp.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\$(TargetFrameworkIdentifier)\$(TargetFrameworkVersion)\Microsoft.$(TargetFrameworkIdentifier).$(TargetFrameworkVersion).Overrides.targets" />
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\$(TargetFrameworkIdentifier)\$(TargetFrameworkVersion)\Microsoft.$(TargetFrameworkIdentifier).CSharp.targets" />
          <xsl:if test="user:IsTrue(/Input/Properties/RemoveXnaAssembliesOnWP8)">
            <Target Name="MonoGame_RemoveXnaAssemblies" AfterTargets="ImplicitlyExpandTargetFramework">
              <Message Text="MonoGame - Removing XNA Assembly references!" Importance="normal" />
              <ItemGroup>
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.GamerServices.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.GamerServicesExtensions.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.Input.Touch.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.MediaLibraryExtensions.dll'" />
              </ItemGroup>
            </Target>
          </xsl:if>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'PSMobile'">
          <Import Project="$(MSBuildExtensionsPath)\Sce\Sce.Psm.CSharp.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'PCL'">
          <Import Project="$(MSBuildExtensionsPath32)\Microsoft\Portable\$(TargetFrameworkVersion)\Microsoft.Portable.CSharp.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'iOS' and not(user:IsTrue(/Input/Properties/UseLegacyiOSAPI))">
          <Import Project="$(MSBuildExtensionsPath)\Xamarin\iOS\Xamarin.iOS.CSharp.targets" />
        </xsl:when>
        <xsl:otherwise>
          <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
        </xsl:otherwise>
      </xsl:choose>

      <xsl:if test="/Input/Generation/Platform = 'Web'">
        <xsl:if test="$project/@Type = 'App' or $project/@Type = 'Console' or $project/@Type = 'GUI' or $project/@Type = 'GTK'">

          <xsl:choose>
            <xsl:when test="user:IsTrue(/Input/Generation/Properties/IgnoreWebPlatform)">
            </xsl:when>
            <xsl:otherwise>
              <Target Name="JSILCompile" AfterTargets="AfterBuild">
                <Exec>
                  <xsl:attribute name="WorkingDirectory">
                    <xsl:value-of
                      select="/Input/Generation/JSILDirectory" />
                  </xsl:attribute>
                  <xsl:attribute name="Command">
                    <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                      <xsl:text>mono </xsl:text>
                    </xsl:if>
                    <xsl:value-of select="/Input/Generation/JSILCompilerFile" />
                    <xsl:text> "</xsl:text>
                    <xsl:value-of select="/Input/Generation/RootPath" />
                    <xsl:value-of select="$project/@Path" />
                    <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                      <xsl:text>/bin/</xsl:text>
                      <xsl:if test="user:IsTrueDefault(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>/$(Platform)/</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)/</xsl:text>
                    </xsl:if>
                    <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
                      <xsl:text>\bin\</xsl:text>
                      <xsl:if test="user:IsTrueDefault(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>\$(Platform)\</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)\</xsl:text>
                    </xsl:if>
                    <xsl:choose>
                      <xsl:when test="/Input/Properties/AssemblyName">
                        <xsl:value-of select="/Input/Properties/AssemblyName" />
                        <xsl:text>.exe</xsl:text>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$project/@Name" />
                        <xsl:text>.exe</xsl:text>
                      </xsl:otherwise>
                    </xsl:choose>
                    <xsl:text>" --out="</xsl:text>
                    <xsl:value-of select="/Input/Generation/RootPath" />
                    <xsl:value-of select="$project/@Path" />
                    <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                      <xsl:text>/bin/</xsl:text>
                      <xsl:if test="user:IsTrueDefault(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>/$(Platform)/</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)</xsl:text>
                    </xsl:if>
                    <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
                      <xsl:text>\bin\</xsl:text>
                      <xsl:if test="user:IsTrueDefault(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>\$(Platform)\</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)</xsl:text>
                    </xsl:if>
                    <xsl:text>"</xsl:text>
                  </xsl:attribute>
                </Exec>
              </Target>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:if>

      {ADDITIONAL_TRANSFORMS}

      <ItemGroup>
        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/Project">
                <ProjectReference>
                  <xsl:attribute name="Include">
                    <xsl:value-of
                      select="user:GetRelativePath(
                        concat(
                          $project/@Path,
                          '\',
                          $project/@Name,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'),
                        ./@Path)" />
                  </xsl:attribute>
                  <Project>{<xsl:value-of select="./@Guid" />}</Project>
                  <Name><xsl:value-of select="./@Name" /></Name>
                  <xsl:for-each select="./Alias">
                    <xsl:if test="@Platform = /Input/Generation/Platform">
                      <Aliases><xsl:value-of select="." /></Aliases>
                    </xsl:if>
                  </xsl:for-each>
                </ProjectReference>
              </xsl:for-each>

              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Project">
                  <ProjectReference>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                          concat(
                            $project/@Path,
                            '\',
                            $project/@Name,
                            '.',
                            /Input/Generation/Platform,
                            '.csproj'),
                          ./@Path)" />
                    </xsl:attribute>
                    <Project>{<xsl:value-of select="./@Guid" />}</Project>
                    <Name><xsl:value-of select="./@Name" /></Name>
                    <xsl:for-each select="./Alias">
                      <xsl:if test="@Platform = /Input/Generation/Platform">
                        <Aliases><xsl:value-of select="." /></Aliases>
                      </xsl:if>
                    </xsl:for-each>
                  </ProjectReference>
                </xsl:for-each>
                <xsl:for-each select="./Service">
                  <xsl:if test="user:ServiceIsActive(
                    ./@Name,
                    '',
                    '',
                    /Input/Services/ActiveServicesNames)">
                    <xsl:for-each select="./Project">
                      <ProjectReference>
                        <xsl:attribute name="Include">
                          <xsl:value-of
                            select="user:GetRelativePath(
                              concat(
                                $project/@Path,
                                '\',
                                $project/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '.csproj'),
                              ./@Path)" />
                        </xsl:attribute>
                        <Project>{<xsl:value-of select="./@Guid" />}</Project>
                        <Name><xsl:value-of select="./@Name" /></Name>
                        <xsl:for-each select="./Alias">
                          <xsl:if test="@Platform = /Input/Generation/Platform">
                            <Aliases><xsl:value-of select="." /></Aliases>
                          </xsl:if>
                        </xsl:for-each>
                      </ProjectReference>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>
              <xsl:for-each select="$extern/Service">
                <xsl:if test="user:ServiceIsActive(
                  ./@Name,
                  '',
                  '',
                  /Input/Services/ActiveServicesNames)">
                  <xsl:for-each select="./Project">
                    <ProjectReference>
                      <xsl:attribute name="Include">
                        <xsl:value-of
                          select="user:GetRelativePath(
                            concat(
                              $project/@Path,
                              '\',
                              $project/@Name,
                              '.',
                              /Input/Generation/Platform,
                              '.csproj'),
                            ./@Path)" />
                      </xsl:attribute>
                      <Project>{<xsl:value-of select="./@Guid" />}</Project>
                      <Name><xsl:value-of select="./@Name" /></Name>
                      <xsl:for-each select="./Alias">
                        <xsl:if test="@Platform = /Input/Generation/Platform">
                          <Aliases><xsl:value-of select="." /></Aliases>
                        </xsl:if>
                      </xsl:for-each>
                    </ProjectReference>
                  </xsl:for-each>
                </xsl:if>
              </xsl:for-each>

              <xsl:for-each select="$extern/Reference">
                <xsl:variable name="refd-name" select="./@Include" />
                <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) > 0">
                  <xsl:variable name="refd"
                    select="/Input/Projects/Project[@Name=$refd-name]" />

                  <xsl:if test="user:ProjectIsActive(
                    $refd/@Platforms,
                    '',
                    '',
                    /Input/Generation/Platform)">

                    <ProjectReference>
                      <xsl:attribute name="Include">
                        <xsl:value-of
                          select="user:GetRelativePath(
                            concat(
                              $project/@Path,
                              '\',
                              $project/@Name,
                              '.',
                              /Input/Generation/Platform,
                              '.csproj'),
                            concat(
                              $refd/@Path,
                              '\',
                              $refd/@Name,
                              '.',
                              /Input/Generation/Platform,
                              '.csproj'))" />
                      </xsl:attribute>
                      <Project>{<xsl:value-of select="$refd/@Guid" />}</Project>
                      <Name><xsl:value-of select="$refd/@Name" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                      <xsl:for-each select="$refd/Alias">
                        <xsl:if test="@Platform = /Input/Generation/Platform">
                          <Aliases><xsl:value-of select="." /></Aliases>
                        </xsl:if>
                      </xsl:for-each>
                    </ProjectReference>
                  </xsl:if>

                </xsl:if>
              </xsl:for-each>


              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Reference">
                  <xsl:variable name="refd-name" select="./@Include" />
                  <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) > 0">
                    <xsl:variable name="refd"
                      select="/Input/Projects/Project[@Name=$refd-name]" />

                    <xsl:if test="user:ProjectIsActive(
                      $refd/@Platforms,
                      '',
                      '',
                      /Input/Generation/Platform)">

                      <ProjectReference>
                        <xsl:attribute name="Include">
                          <xsl:value-of
                            select="user:GetRelativePath(
                              concat(
                                $project/@Path,
                                '\',
                                $project/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '.csproj'),
                              concat(
                                $refd/@Path,
                                '\',
                                $refd/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '.csproj'))" />
                        </xsl:attribute>
                        <Project>{<xsl:value-of select="$refd/@Guid" />}</Project>
                        <Name><xsl:value-of select="$refd/@Name" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                        <xsl:for-each select="$refd/Alias">
                          <xsl:if test="@Platform = /Input/Generation/Platform">
                            <Aliases><xsl:value-of select="." /></Aliases>
                          </xsl:if>
                        </xsl:for-each>
                      </ProjectReference>
                    </xsl:if>

                  </xsl:if>
                </xsl:for-each>
                <xsl:for-each select="./Service">
                  <xsl:if test="user:ServiceIsActive(
                    ./@Name,
                    '',
                    '',
                    /Input/Services/ActiveServicesNames)">
                    <xsl:for-each select="./Reference">
                      <xsl:variable name="refd-name" select="./@Include" />
                      <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) > 0">
                        <xsl:variable name="refd"
                          select="/Input/Projects/Project[@Name=$refd-name]" />

                        <xsl:if test="user:ProjectIsActive(
                          $refd/@Platforms,
                          '',
                          '',
                          /Input/Generation/Platform)">

                          <ProjectReference>
                            <xsl:attribute name="Include">
                              <xsl:value-of
                                select="user:GetRelativePath(
                                  concat(
                                    $project/@Path,
                                    '\',
                                    $project/@Name,
                                    '.',
                                    /Input/Generation/Platform,
                                    '.csproj'),
                                  concat(
                                    $refd/@Path,
                                    '\',
                                    $refd/@Name,
                                    '.',
                                    /Input/Generation/Platform,
                                    '.csproj'))" />
                            </xsl:attribute>
                            <Project>{<xsl:value-of select="$refd/@Guid" />}</Project>
                            <Name><xsl:value-of select="$refd/@Name" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                            <xsl:for-each select="$refd/Alias">
                              <xsl:if test="@Platform = /Input/Generation/Platform">
                                <Aliases><xsl:value-of select="." /></Aliases>
                              </xsl:if>
                            </xsl:for-each>
                          </ProjectReference>
                        </xsl:if>

                      </xsl:if>
                    </xsl:for-each>
                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>
              <xsl:for-each select="$extern/Service">
                <xsl:if test="user:ServiceIsActive(
                  ./@Name,
                  '',
                  '',
                  /Input/Services/ActiveServicesNames)">
                  <xsl:for-each select="./Reference">
                    <xsl:variable name="refd-name" select="./@Include" />
                    <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) > 0">
                      <xsl:variable name="refd"
                        select="/Input/Projects/Project[@Name=$refd-name]" />

                      <xsl:if test="user:ProjectIsActive(
                        $refd/@Platforms,
                        '',
                        '',
                        /Input/Generation/Platform)">

                        <ProjectReference>
                          <xsl:attribute name="Include">
                            <xsl:value-of
                              select="user:GetRelativePath(
                                concat(
                                  $project/@Path,
                                  '\',
                                  $project/@Name,
                                  '.',
                                  /Input/Generation/Platform,
                                  '.csproj'),
                                concat(
                                  $refd/@Path,
                                  '\',
                                  $refd/@Name,
                                  '.',
                                  /Input/Generation/Platform,
                                  '.csproj'))" />
                          </xsl:attribute>
                          <Project>{<xsl:value-of select="$refd/@Guid" />}</Project>
                          <Name><xsl:value-of select="$refd/@Name" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                          <xsl:for-each select="$refd/Alias">
                            <xsl:if test="@Platform = /Input/Generation/Platform">
                              <Aliases><xsl:value-of select="." /></Aliases>
                            </xsl:if>
                          </xsl:for-each>
                        </ProjectReference>
                      </xsl:if>

                    </xsl:if>
                  </xsl:for-each>
                </xsl:if>
              </xsl:for-each>

            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-path" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-path]) > 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-path]) = 0">

              <xsl:if test="user:ProjectIsActive(
                /Input/Projects/Project[@Name=$include-path]/@Platforms,
                '',
                '',
                /Input/Generation/Platform)">

                <ProjectReference>
                  <xsl:attribute name="Include">
                    <xsl:value-of
                      select="user:GetRelativePath(
                        concat(
                          $project/@Path,
                          '\',
                          $project/@Name,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'),
                        concat(
                          /Input/Projects/Project[@Name=$include-path]/@Path,
                          '\',
                          @Include,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'))" />
                  </xsl:attribute>
                  <Project>{<xsl:value-of select="/Input/Projects/Project[@Name=$include-path]/@Guid" />}</Project>
                  <Name><xsl:value-of select="@Include" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                  <xsl:for-each select="./Alias">
                    <xsl:if test="@Platform = /Input/Generation/Platform">
                      <Aliases><xsl:value-of select="." /></Aliases>
                    </xsl:if>
                  </xsl:for-each>
                </ProjectReference>
              </xsl:if>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <xsl:if test="/Input/Properties/MonoDevelopPoliciesFile">
        <ProjectExtensions>
          <MonoDevelop>
            <Properties>
              <xsl:value-of
                select="user:ReadFile(concat(/Input/Generation/RootPath, '\', /Input/Properties/MonoDevelopPoliciesFile))"
                disable-output-escaping="yes" />
            </Properties>
          </MonoDevelop>
        </ProjectExtensions>
      </xsl:if>

    </Project>

  </xsl:template>

  <xsl:template match="*">
    <xsl:element
      name="{name()}"
      namespace="http://schemas.microsoft.com/developer/msbuild/2003">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
