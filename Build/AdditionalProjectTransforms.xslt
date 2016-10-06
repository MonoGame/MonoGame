<!-- Have MacOS builds use the right framework version. -->
<xsl:if test="/Input/Generation/Platform = 'MacOS'">
  <PropertyGroup>
    <UseXamMacFullFramework>true</UseXamMacFullFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="OpenTK">
      <HintPath>/Library/Frameworks/Xamarin.Mac.framework/Versions/2.10.0.105/lib/reference/net_4_5/OpenTK.dll</HintPath>
    </Reference>
  </ItemGroup>
</xsl:if>
