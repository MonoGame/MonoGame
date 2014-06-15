SetCompressor /SOLID /FINAL lzma

!include "header.nsh"
!define INSTALLERFILENAME "MonoGame"
!define APPNAME "MonoGame"

;Include Modern UI

!include "Sections.nsh"
!include "MUI2.nsh"
!include "InstallOptions.nsh"

!define MUI_ICON "${FrameworkPath}\monogame.ico"

!define MUI_UNICON "${FrameworkPath}\monogame.ico"

Name '${APPNAME} ${INSTALLERVERSION}'
OutFile '${INSTALLERFILENAME}Installer-${INSTALLERVERSION}.exe'
InstallDir '$PROGRAMFILES\${APPNAME}\v${VERSION}'
VIProductVersion "${VERSION}.${REVISION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} Development Tools"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "MonoGame"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${APPNAME} Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "� Copyright MonoGame 2013"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;Interface Configuration

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${FrameworkPath}\monogame.bmp"
!define MUI_ABORTWARNING

!define MUI_WELCOMEFINISHPAGE_BITMAP "${FrameworkPath}\panel.bmp"
;Languages

!insertmacro MUI_PAGE_WELCOME

;!insertmacro MUI_PAGE_LICENSE "License.txt"

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

;Page custom XamarinPageEnter XamarinPageLeave

;  Function XamarinPageEnter
;      ReserveFile "xamarin.ini"
;      ReserveFile "mono.bmp"
;      ReserveFile "xamarin.bmp"
;      !insertmacro INSTALLOPTIONS_EXTRACT "xamarin.ini"
;      !insertmacro INSTALLOPTIONS_WRITE "xamarin.ini" "Field 1" "Text" "mono.bmp"
;      !insertmacro INSTALLOPTIONS_WRITE "xamarin.ini" "Field 2" "Text" "xamarin.bmp"
;      !insertmacro INSTALLOPTIONS_DISPLAY "xamarin.ini"
;  FunctionEnd
  
;  Function XamarinPageLeave
;  FunctionEnd

!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
;Languages

!insertmacro MUI_LANGUAGE "English"

;--------------------------------

Function CustomPageFunction ;Function name defined with Page command
  !insertmacro INSTALLOPTIONS_DISPLAY "xamarin.ini"
FunctionEnd


; The stuff to install
Section "MonoGame Core Components" CoreComponents ;No components page, name is not important
  SectionIn RO

  
  ; TODO: This stuff below is a bit of a mess and i 
  ; suspect some of it can be removed now.  Some can
  ; be removed after we kill off the old XNA content 
  ; pipeline support.

  SetOutPath $PROGRAMFILES32\MSBuild\${APPNAME}\v${VERSION}
  File '..\monogame.ico'
  File /r '..\..\MonoGame.ContentPipeline\ContentProcessors\bin\Release\*.dll'
  File '..\..\MonoGame.ContentPipeline\*.targets'
  File '..\..\ThirdParty\Dependencies\NAudio\*.dll'
  File '..\..\ThirdParty\Dependencies\SharpDX\Windows\*.*'
  File /nonfatal '..\..\ThirdParty\Dependencies\NAudio\*.xml'
  File /nonfatal '..\..\ThirdParty\Dependencies\NAudio\*.txt' 
  File '..\..\ThirdParty\Dependencies\ManagedPVRTC\x86\pvrtc.dll'
  File /oname=libmojoshader.dll  '..\..\ThirdParty\Dependencies\MojoShader\Windows\libmojoshader_32.dll'
  File '..\..\ThirdParty\Dependencies\lame_enc.dll'

  
  ; Install the MonoGame tools to individual subfolders both
  ; to avoid conflicting assemblies and to make it easy for end
  ; users to copy all the necessary files for distribution.

  SetOutPath $PROGRAMFILES32\MSBuild\${APPNAME}\v${VERSION}\2MGFX
  File /r '..\..\Tools\2MGFX\bin\x64\Release\*.exe'
  File /r '..\..\Tools\2MGFX\bin\x64\Release\*.dll'

  SetOutPath $PROGRAMFILES32\MSBuild\${APPNAME}\v${VERSION}\MGCB
  File /r '..\..\Tools\MGCB\bin\x64\Release\*.exe'
  File /r '..\..\Tools\MGCB\bin\x64\Release\*.dll'
  
  ; TODO: Add the new Pipeline GUI tool here along with
  ; registration of file extensions and shortcuts.


  ; Install the assemblies for all the platforms we can 
  ; target from a Windows desktop system.

  ; Install Android Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Android'
  File '..\..\MonoGame.Framework\bin\Android\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\Android\AnyCPU\Release\*.xml'
  
  ; Install OUYA Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\OUYA'
  File '..\..\MonoGame.Framework\bin\Ouya\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\Ouya\AnyCPU\Release\*.xml'
  
  ; Install Windows Desktop OpenGL Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsGL'
  File /nonfatal '..\..\MonoGame.Framework\bin\WindowsGL\AnyCPU\Release\*.dll'
  File /nonfatal ' ..\..\MonoGame.Framework\bin\WindowsGL\AnyCPU\Release\*.xml'
  File '..\..\ThirdParty\Dependencies\OpenTK.dll'
  File '..\..\ThirdParty\Dependencies\OpenTK.dll.config'
  File '..\..\ThirdParty\Dependencies\OpenTK_svnversion.txt'
  File '..\..\ThirdParty\GamepadConfig\Tao.Sdl.dll'
  File '..\..\ThirdParty\GamepadConfig\SDL.dll'
  
  ; Install Windows Desktop DirectX Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Windows'
  File '..\..\MonoGame.Framework\bin\Windows\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\Windows\AnyCPU\Release\*.xml'
  
  ; Install Linux Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Linux'
  File /nonfatal '..\..\MonoGame.Framework\bin\Linux\AnyCPU\Release\*.dll'
  File /nonfatal ' ..\..\MonoGame.Framework\bin\Linux\AnyCPU\Release\*.xml'
  File '..\..\ThirdParty\Dependencies\OpenTK.dll'
  File '..\..\ThirdParty\Dependencies\OpenTK.dll.config'
  File '..\..\ThirdParty\Dependencies\OpenTK_svnversion.txt'
  File '..\..\ThirdParty\GamepadConfig\Tao.Sdl.dll'
  File '..\..\ThirdParty\GamepadConfig\SDL.dll'
  File '..\..\ThirdParty\GamepadConfig\SDL_Mixer.dll'

  ; Install Windows 8 Store Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Windows8'
  File '..\..\MonoGame.Framework\bin\Windows8\AnyCPU\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\Windows8\AnyCPU\Release\*.xml'

  ; Install Windows Phone ARM Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsPhone\ARM'
  File '..\..\MonoGame.Framework\bin\WindowsPhone\ARM\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\WindowsPhone\ARM\Release\*.xml'

  ; Install Windows Phone x86 Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsPhone\x86'
  File '..\..\MonoGame.Framework\bin\WindowsPhone\x86\Release\*.dll'
  File '..\..\MonoGame.Framework\bin\WindowsPhone\x86\Release\*.xml'

  ; Intall iOS Assemblies
  IfFileExists `$PROGRAMFILES\MSBuild\Xamarin\iOS\*.*` InstalliOSAssemblies SkipiOSAssemblies
  InstalliOSAssemblies:
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\iOS'
  File /nonfatal 'iOS\*.dll'
  ;File /nonfatal 'iOS\*.xml'  
  SkipiOSAssemblies:

  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows GL' '' '$INSTDIR\Assemblies\WindowsGL'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows' '' '$INSTDIR\Assemblies\Windows'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Linux' '' '$INSTDIR\Assemblies\Linux'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Store' '' '$INSTDIR\Assemblies\Windows8'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone ARM' '' '$INSTDIR\Assemblies\WindowsPhone\ARM'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone x86' '' '$INSTDIR\Assemblies\WindowsPhone\x86'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android' '' '$INSTDIR\Assemblies\Android'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for OUYA' '' '$INSTDIR\Assemblies\OUYA'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS' '' '$INSTDIR\Assemblies\iOS'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    GOTO End32Bitvs64BitCheck
  Is64bit:
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows GL' '' '$INSTDIR\Assemblies\WindowsGL'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows' '' '$INSTDIR\Assemblies\Windows'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Store' '' '$INSTDIR\Assemblies\Windows8'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android' '' '$INSTDIR\Assemblies\Android'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for OUYA' '' '$INSTDIR\Assemblies\OUYA'
	WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS' '' '$INSTDIR\Assemblies\iOS'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Linux' '' '$INSTDIR\Assemblies\Linux'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone ARM' '' '$INSTDIR\Assemblies\WindowsPhone\ARM'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone x86' '' '$INSTDIR\Assemblies\WindowsPhone\x86'

  End32Bitvs64BitCheck:
  ; Add remote programs
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayName' '${APPNAME}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayVersion' '${VERSION}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayIcon' '$INSTDIR\monogame.ico'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'InstallLocation' '$INSTDIR\'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'Publisher' 'MonoGame'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'UninstallString' '$INSTDIR\uninstall.exe'


  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}'
  File '..\monogame.ico'
  ; Uninstaller
  WriteUninstaller "uninstall.exe"


SectionEnd

Section "OpenAL" OpenAL
  ; SetOutPath $INSTDIR
  File '..\..\ThirdParty\Dependencies\oalinst.exe'
  ExecWait '"$INSTDIR\oalinst.exe /S"'
SectionEnd

Section "Visual Studio 2010 Templates" VS2010

  IfFileExists `$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\Visual C#\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    ; Set output path to the installation directory.
    SetOutPath "$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\Visual C#\MonoGame"

    ; install the Templates for MonoDevelop
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:
  
    DetailPrint "Visual Studio 2010 not found"
  EndTemplates:

SectionEnd

Section "Visual Studio 2012 Templates" VS2012

  IfFileExists `$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    ; Set output path to the installation directory.
    SetOutPath "$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\MonoGame"

    ; install the Templates for MonoDevelop
    File /r '..\..\ProjectTemplates\VisualStudio2012\*.zip'
    ; Install the VS 2010 templates as well 
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:

    DetailPrint "Visual Studio 2012 not found"
  EndTemplates:

SectionEnd

Section "Visual Studio 2013 Templates" VS2013

  IfFileExists `$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\Visual C#\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    ; Set output path to the installation directory.
    SetOutPath "$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\Visual C#\MonoGame"

    ; install the Templates for MonoDevelop
    File /r '..\..\ProjectTemplates\VisualStudio2012\*.zip'
    ; Install the VS 2010 templates as well 
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:

    DetailPrint "Visual Studio 2013 not found"
  EndTemplates:

SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts" Menu
	CreateDirectory $SMPROGRAMS\MonoGame
	CreateShortCut "$SMPROGRAMS\MonoGame\Uninstall.lnk" "$PROGRAMFILES\${APPNAME}\v${VERSION}\uninstall.exe" "" "$PROGRAMFILES\${APPNAME}\v${VERSION}\uninstall.exe" 0
	;CreateShortCut "$SMPROGRAMS\MonoGame\GettingStarted.lnk" "$PROGRAMFILES\${APPNAME}\v${VERSION}\GettingStarted.pdf" "" "$PROGRAMFILES\${APPNAME}\v${VERSION}\GettingStarted.pdf" 0
	WriteINIStr "$SMPROGRAMS\MonoGame\MonoGame Web Site.url" "InternetShortcut" "URL" "http://www.monogame.net"
	WriteINIStr "$SMPROGRAMS\MonoGame\MonoGame Web Site.url" "InternetShortcut" "IconFile" "$PROGRAMFILES\${APPNAME}\v${VERSION}\monogame.ico"
	WriteINIStr "$SMPROGRAMS\MonoGame\MonoGame Web Site.url" "InternetShortcut" "IconIndex" "0"

SectionEnd

LangString CoreComponentsDesc ${LANG_ENGLISH} "Install the Runtimes and the MSBuild extensions for MonoGame"
LangString OpenALDesc ${LANG_ENGLISH} "Install the OpenAL drivers"
LangString MonoDevelopDesc ${LANG_ENGLISH} "Install the project templates for MonoDevelop"
LangString VS2010Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2010"
LangString VS2012Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2012"
LangString MenuDesc ${LANG_ENGLISH} "Add a link to the MonoGame website to your start menu"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${CoreComponents} $(CoreComponentsDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${OpenAL} $(OpenALDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${MonoDevelop} $(MonoDevelopDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2010} $(VS2010Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2012} $(VS2012Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${Menu} $(MenuDesc)
!insertmacro MUI_FUNCTION_DESCRIPTION_END



Function .onInit

  IntOp $0 ${SF_SELECTED} | ${SF_RO}
  SectionSetFlags ${core_id} $0
FunctionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  DeleteRegKey HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows GL'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Linux'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Store' 
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone ARM'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone x86'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for OUYA'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    GOTO End32Bitvs64BitCheck
  Is64bit:
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows GL'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Linux'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\${APPNAME} for Windows Store'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone ARM'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME} for Windows Phone x86'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android'
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for OUYA'
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
  RMDir /r "$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\MonoGame"
  RMDir /r "$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\Visual C#\MonoGame"
  RMDir /r "$PROGRAMFILES32\MSBuild\${APPNAME}\v${VERSION}"
  RMDir /r "$SMPROGRAMS\MonoGame"

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

SectionEnd

