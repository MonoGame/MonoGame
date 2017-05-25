<xsl:if test="/Input/Properties/DefineNET4DetectionConstant = 'True'">
  <PropertyGroup>
    <DefineConstants Condition=" '$(TargetFrameworkVersion)' == 'v4.0' ">NET4</DefineConstants>
  </PropertyGroup>
</xsl:if>
