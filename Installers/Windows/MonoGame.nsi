SetCompressor /SOLID /FINAL lzma

!include "header.nsh"
!define APPNAME "MonoGame"

;Include Modern UI

!include "Sections.nsh"
!include "MUI2.nsh"
!include "InstallOptions.nsh"

!define MUI_ICON "${FrameworkPath}\monogame.ico"

!define MUI_UNICON "${FrameworkPath}\monogame.ico"

Name '${APPNAME} SDK ${INSTALLERVERSION}'
OutFile 'MonoGameSetup.exe'
InstallDir '$PROGRAMFILES\${APPNAME}\v${VERSION}'
!define MSBuildInstallDir '$PROGRAMFILES32\MSBuild\${APPNAME}\v${VERSION}'
VIProductVersion "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} SDK"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "The MonoGame Team"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${APPNAME} SDK Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © The MonoGame Team"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;Interface Configuration

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${FrameworkPath}\monogame.bmp"
!define MUI_ABORTWARNING

!define MUI_WELCOMEFINISHPAGE_BITMAP "${FrameworkPath}\panel.bmp"
;Languages

!insertmacro MUI_PAGE_WELCOME

!insertmacro MUI_PAGE_LICENSE "..\..\License.txt"

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES


!macro VS_ASSOCIATE_EDITOR TOOLNAME VSVERSION EXT TOOLPATH
  WriteRegStr   HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}' 'Custom' '${TOOLNAME}'
  WriteRegDWORD HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}' 'Type' 0x00000002
  WriteRegStr   HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}\${TOOLNAME}' '' '${TOOLPATH}'
  WriteRegStr   HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}\${TOOLNAME}' 'Arguments' ''
!macroend

!macro APP_ASSOCIATE EXT FILECLASS DESCRIPTION ICON COMMANDTEXT COMMAND
  WriteRegStr HKCR ".${EXT}" "" "${FILECLASS}" 
  WriteRegStr HKCR "${FILECLASS}" "" `${DESCRIPTION}`
  WriteRegStr HKCR "${FILECLASS}\DefaultIcon" "" `${ICON}`
  WriteRegStr HKCR "${FILECLASS}\shell" "" "open"
  WriteRegStr HKCR "${FILECLASS}\shell\open" "" `${COMMANDTEXT}`
  WriteRegStr HKCR "${FILECLASS}\shell\open\command" "" `${COMMAND}`
!macroend

;--------------------------------
;Languages

!insertmacro MUI_LANGUAGE "English"

;--------------------------------

; The stuff to install
Section "MonoGame Core Components" CoreComponents ;No components page, name is not important
  SectionIn RO
  
  ; Install the VS support files.
  SetOutPath ${MSBuildInstallDir}
  File '..\..\MonoGame.Framework.Content.Pipeline\MonoGame.Content.Builder.targets'
  File '..\..\MonoGame.Framework.Content.Pipeline\MonoGame.Common.props'
  File '..\..\Tools\Pipeline\bin\Windows\AnyCPU\Release\MonoGame.Build.Tasks.dll'

  ; Install the MonoGame tools to a single shared folder.
  SetOutPath ${MSBuildInstallDir}\Tools
  File /r '..\..\Tools\2MGFX\bin\Windows\AnyCPU\Release\*.exe'
  File /r '..\..\Tools\2MGFX\bin\Windows\AnyCPU\Release\*.dll'
  File /r '..\..\Tools\MGCB\bin\Windows\AnyCPU\Release\*.exe'
  File /r '..\..\Tools\MGCB\bin\Windows\AnyCPU\Release\*.dll'
  File /r '..\..\Tools\Pipeline\bin\Windows\AnyCPU\Release\*.exe'
  File /r '..\..\Tools\Pipeline\bin\Windows\AnyCPU\Release\*.dll'
  File /r '..\..\Tools\Pipeline\bin\Windows\AnyCPU\Release\*.xml'
  File /r '..\..\Tools\Pipeline\bin\Windows\AnyCPU\Release\Templates'

  ; Associate .mgcb files open in the Pipeline tool.
  !insertmacro VS_ASSOCIATE_EDITOR 'MonoGame Pipeline' '10.0' 'mgcb' '${MSBuildInstallDir}\Tools\Pipeline.exe'
  !insertmacro VS_ASSOCIATE_EDITOR 'MonoGame Pipeline' '11.0' 'mgcb' '${MSBuildInstallDir}\Tools\Pipeline.exe'
  !insertmacro VS_ASSOCIATE_EDITOR 'MonoGame Pipeline' '12.0' 'mgcb' '${MSBuildInstallDir}\Tools\Pipeline.exe'
  !insertmacro VS_ASSOCIATE_EDITOR 'MonoGame Pipeline' '14.0' 'mgcb' '${MSBuildInstallDir}\Tools\Pipeline.exe'
  !insertmacro VS_ASSOCIATE_EDITOR 'MonoGame Pipeline' '15.0' 'mgcb' '${MSBuildInstallDir}\Tools\Pipeline.exe'
  !insertmacro APP_ASSOCIATE 'mgcb' 'MonoGame.ContentBuilderFile' 'A MonoGame content builder project.' '${MSBuildInstallDir}\Tools\Pipeline.exe,0' 'Open with Pipeline' '${MSBuildInstallDir}\Tools\Pipeline.exe "%1"'

  ; Install the assemblies for all the platforms we can 
  ; target from a Windows desktop system.

  ; Install Android Assemblies
  SetOutPath '$INSTDIR\Assemblies\Android'
  File '..\..\MonoGame.Framework\bin\Android\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\Android\AnyCPU\Release\*.xml'
  
  ; Install DesktopGL Assemblies
  SetOutPath '$INSTDIR\Assemblies\DesktopGL'
  File /nonfatal '..\..\MonoGame.Framework\bin\WindowsGL\AnyCPU\Release\*.dll'
  File /nonfatal ' ..\..\MonoGame.Framework\bin\WindowsGL\AnyCPU\Release\*.xml'
  File '..\..\ThirdParty\Dependencies\SDL\MacOS\Universal\libSDL2-2.0.0.dylib'
  File '..\..\ThirdParty\Dependencies\openal-soft\MacOS\Universal\libopenal.1.dylib'
  File '..\..\ThirdParty\Dependencies\MonoGame.Framework.dll.config'
  
  ; Install x86 DesktopGL Dependencies
  SetOutPath '$INSTDIR\Assemblies\DesktopGL\x86'
  File '..\..\ThirdParty\Dependencies\SDL\Windows\x86\SDL2.dll'
  File '..\..\ThirdParty\Dependencies\openal-soft\Windows\x86\soft_oal.dll'
  File '..\..\ThirdParty\Dependencies\SDL\Linux\x86\libSDL2-2.0.so.0'
  File '..\..\ThirdParty\Dependencies\openal-soft\Linux\x86\libopenal.so.1'
  
  ; Install x64 DesktopGL Dependencies
  SetOutPath '$INSTDIR\Assemblies\DesktopGL\x64'
  File '..\..\ThirdParty\Dependencies\SDL\Windows\x64\SDL2.dll'
  File '..\..\ThirdParty\Dependencies\openal-soft\Windows\x64\soft_oal.dll'
  File '..\..\ThirdParty\Dependencies\SDL\Linux\x64\libSDL2-2.0.so.0'
  File '..\..\ThirdParty\Dependencies\openal-soft\Linux\x64\libopenal.so.1'
  
  ; Install Windows Desktop DirectX Assemblies
  SetOutPath '$INSTDIR\Assemblies\Windows'
  File '..\..\MonoGame.Framework\bin\Windows\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\Windows\AnyCPU\Release\*.xml'

  ; Install Windows 10 UAP Assemblies
  SetOutPath '$INSTDIR\Assemblies\WindowsUniversal'
  File '..\..\MonoGame.Framework\bin\WindowsUniversal\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\WindowsUniversal\AnyCPU\Release\*.xml'

  ; Install iOS Assemblies
  IfFileExists `$PROGRAMFILES\MSBuild\Xamarin\iOS\*.*` InstalliOSAssemblies SkipiOSAssemblies
  InstalliOSAssemblies:
  SetOutPath '$INSTDIR\Assemblies\iOS'
  File '..\..\MonoGame.Framework\bin\iOS\iPhoneSimulator\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\iOS\iPhoneSimulator\Release\*.xml'
  SetOutPath '$INSTDIR\Assemblies\tvOS'
  File '..\..\MonoGame.Framework\bin\tvOS\iPhoneSimulator\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\tvOS\iPhoneSimulator\Release\*.xml'
  SkipiOSAssemblies:

  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Desktop OpenGL' '' '$INSTDIR\Assemblies\DesktopGL'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows' '' '$INSTDIR\Assemblies\Windows'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows 10 Universal' '' '$INSTDIR\Assemblies\WindowsUniversal'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android' '' '$INSTDIR\Assemblies\Android'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS' '' '$INSTDIR\Assemblies\iOS'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    GOTO End32Bitvs64BitCheck
  Is64bit:
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Desktop OpenGL' '' '$INSTDIR\Assemblies\DesktopGL'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows' '' '$INSTDIR\Assemblies\Windows'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS' '' '$INSTDIR\Assemblies\iOS'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows 10 Universal' '' '$INSTDIR\Assemblies\WindowsUniversal'

  End32Bitvs64BitCheck:
  ; Add remote programs
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayName' '${APPNAME} SDK'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayVersion' '${INSTALLERVERSION}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayIcon' '$INSTDIR\monogame.ico'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'InstallLocation' '$INSTDIR\'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'Publisher' 'The MonoGame Team'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'UninstallString' '$INSTDIR\uninstall.exe'


  SetOutPath '$INSTDIR'
  File '..\monogame.ico'

  ; Uninstaller
  WriteUninstaller "uninstall.exe"


SectionEnd

Section "Visual Studio 2010 Templates" VS2010

  IfFileExists `$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    SetOutPath "$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\Visual C#\MonoGame"
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:
    DetailPrint "Visual Studio 2010 not found"
  EndTemplates:

SectionEnd

Section "Visual Studio 2013 Templates" VS2013

  IfFileExists `$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    SetOutPath "$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\Visual C#\MonoGame"
    File /r '..\..\ProjectTemplates\VisualStudio2013\*.zip'
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:
    DetailPrint "Visual Studio 2013 not found"
  EndTemplates:

SectionEnd

Section "Visual Studio 2015 Templates" VS2015

  IfFileExists `$DOCUMENTS\Visual Studio 2015\Templates\ProjectTemplates\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    SetOutPath "$DOCUMENTS\Visual Studio 2015\Templates\ProjectTemplates\Visual C#\MonoGame"
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    File /r '..\..\ProjectTemplates\VisualStudio2015\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:
    DetailPrint "Visual Studio 2015 not found"
  EndTemplates:

SectionEnd

Section "Visual Studio 2017 Templates" VS2017

  IfFileExists `$DOCUMENTS\Visual Studio 2017\Templates\ProjectTemplates\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    SetOutPath "$DOCUMENTS\Visual Studio 2017\Templates\ProjectTemplates\Visual C#\MonoGame"
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    File /r '..\..\ProjectTemplates\VisualStudio2013\MonoGameShared.zip'
    File /r '..\..\ProjectTemplates\VisualStudio2015\WindowsUniversal10.VB.zip'
    File /r '..\..\ProjectTemplates\VisualStudio2017\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:
    DetailPrint "Visual Studio 2017 not found"
  EndTemplates:

SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts" Menu
	CreateDirectory $SMPROGRAMS\${APPNAME}
	SetOutPath "$INSTDIR"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Uninstall MonoGame.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
	SetOutPath "${MSBuildInstallDir}\Tools"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\MonoGame Pipeline.lnk" "${MSBuildInstallDir}\Tools\Pipeline.exe" "" "${MSBuildInstallDir}\Tools\Pipeline.exe" 0
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Website.url" "InternetShortcut" "URL" "http://www.monogame.net"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Website.url" "InternetShortcut" "IconFile" "$INSTDIR\monogame.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Website.url" "InternetShortcut" "IconIndex" "0"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Documentation.url" "InternetShortcut" "URL" "http://www.monogame.net/documentation"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Documentation.url" "InternetShortcut" "IconFile" "$INSTDIR\monogame.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Documentation.url" "InternetShortcut" "IconIndex" "0"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Support.url" "InternetShortcut" "URL" "http://community.monogame.net/"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Support.url" "InternetShortcut" "IconFile" "$INSTDIR\monogame.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Support.url" "InternetShortcut" "IconIndex" "0"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Bug Reports.url" "InternetShortcut" "URL" "https://github.com/mono/MonoGame/issues"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Bug Reports.url" "InternetShortcut" "IconFile" "$INSTDIR\monogame.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Bug Reports.url" "InternetShortcut" "IconIndex" "0"

SectionEnd

LangString CoreComponentsDesc ${LANG_ENGLISH} "Install the Runtimes and the MSBuild extensions for MonoGame"
LangString MonoDevelopDesc ${LANG_ENGLISH} "Install the project templates for MonoDevelop"
LangString VS2010Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2010"
LangString VS2012Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2012"
LangString VS2013Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2013"
LangString VS2015Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2015"
LangString VS2017Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2017"
LangString MenuDesc ${LANG_ENGLISH} "Add a link to the MonoGame website to your start menu"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${CoreComponents} $(CoreComponentsDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${MonoDevelop} $(MonoDevelopDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2010} $(VS2010Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2012} $(VS2012Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2013} $(VS2013Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2015} $(VS2015Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2017} $(VS2017Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${Menu} $(MenuDesc)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

Function checkVS2010
IfFileExists `$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\*.*` end disable
  disable:
	 SectionSetFlags ${VS2010} $0
  end:
FunctionEnd
 
Function checkVS2012
IfFileExists `$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\*.*` end disable
  disable:
	 SectionSetFlags ${VS2012} $0
  end:
FunctionEnd

Function checkVS2013
IfFileExists `$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\*.*` end disable
  disable:
	 SectionSetFlags ${VS2013} $0
  end:
FunctionEnd

Function checkVS2015
IfFileExists `$DOCUMENTS\Visual Studio 2015\Templates\ProjectTemplates\*.*` end disable
  disable:
	 SectionSetFlags ${VS2015} $0
  end:
FunctionEnd

Function checkVS2017
IfFileExists `$DOCUMENTS\Visual Studio 2017\Templates\ProjectTemplates\*.*` end disable
  disable:
	 SectionSetFlags ${VS2017} $0
  end:
FunctionEnd

Function .onInit 
  IntOp $0 $0 | ${SF_RO}
  Call checkVS2010
  Call checkVS2012
  Call checkVS2013
  Call checkVS2015
  Call checkVS2017
  IntOp $0 ${SF_SELECTED} | ${SF_RO}
  SectionSetFlags ${core_id} $0
FunctionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  DeleteRegKey HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Desktop OpenGL'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Store' 
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Phone 8.1' 
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone ARM'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone x86'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows 10 UAP' 
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS'

  DeleteRegKey HKCU 'Software\Microsoft\VisualStudio\10.0\Default Editors\mgcb'
  DeleteRegKey HKCU 'Software\Microsoft\VisualStudio\11.0\Default Editors\mgcb'
  DeleteRegKey HKCU 'Software\Microsoft\VisualStudio\12.0\Default Editors\mgcb'

  DeleteRegKey HKCR '.mgcb'
  DeleteRegKey HKCR 'MonoGame.ContentBuilderFile'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    GOTO End32Bitvs64BitCheck
  Is64bit:
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Desktop OpenGL'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Store'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Phone 8.1'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone ARM'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone x86'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows 10 UAP'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android'
	DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS'


  End32Bitvs64BitCheck:

  ReadRegStr $0 HKLM 'SOFTWARE\Wow6432Node\Xamarin\MonoDevelop' "Path"
  ${If} $0 == "" ; check on 32 bit machines just in case
  ReadRegStr $0 HKLM 'SOFTWARE\Xamarin\MonoDevelop' "Path"
  ${EndIf}

  ${If} $0 == ""
  ${Else}
  RMDir /r "$0\AddIns\MonoDevelop.MonoGame"
  ${EndIf}
  
  RMDir /r "$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\Visual C#\MonoGame"
  RMDir /r "$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\Visual C#\MonoGame"
  RMDir /r "$DOCUMENTS\Visual Studio 2015\Templates\ProjectTemplates\Visual C#\MonoGame"
  RMDir /r "$DOCUMENTS\Visual Studio 2017\Templates\ProjectTemplates\Visual C#\MonoGame"
  RMDir /r "${MSBuildInstallDir}"
  RMDir /r "$SMPROGRAMS\${APPNAME}"

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

SectionEnd

