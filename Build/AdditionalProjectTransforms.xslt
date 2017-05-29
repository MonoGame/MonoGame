<xsl:if test="/Input/Properties/DefineNET4DetectionConstant = 'True'">
  <PropertyGroup>
    <DefineConstants Condition="$([System.Version]::Parse('$(TargetFrameworkVersion.Substring(1))').CompareTo($([System.Version]::Parse('4.5'))))   &gt;= 0">$(DefineConstants);NET45</DefineConstants>
  </PropertyGroup>
</xsl:if>
