<xsl:if test="/Input/Properties/DefineNET4DetectionConstant = 'True'">
  <PropertyGroup>
    <DefineConstants Condition=" '$(TargetFrameworkVersion)' != 'v4.0' And '$(TargetFrameworkVersion)' != 'v3.5' And '$(TargetFrameworkVersion)' != 'v3.0' And '$(TargetFrameworkVersion)' != 'v2.0' ">$(DefineConstants);NET45</DefineConstants>
  </PropertyGroup>
</xsl:if>
<xsl:if test="/Input/Properties/ImportPipelineTargets = 'True'">
  <Import Project="Pipeline.targets" />
</xsl:if>
