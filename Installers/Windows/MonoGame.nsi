SetCompressor /SOLID /FINAL lzma

!define FrameworkPath "C:\Users\Dean\Desktop\MonoGame\"
!define VERSION "3.0"
!define REVISION "0.0"
!define INSTALLERFILENAME "MonoGame"
!define APPNAME "MonoGame"

;Include Modern UI

!include "Sections.nsh"
!include "MUI2.nsh"
!include "InstallOptions.nsh"

!define MUI_ICON "${FrameworkPath}Installers\monogame.ico"

!define MUI_UNICON "${FrameworkPath}Installers\monogame.ico"

Name '${APPNAME} ${VERSION}'
OutFile '${INSTALLERFILENAME}Installer-${VERSION}.exe'
InstallDir '$PROGRAMFILES\${APPNAME}\v${VERSION}'
VIProductVersion "${VERSION}.${REVISION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} Development Tools"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "MonoGame"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${APPNAME} Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "© Copyright MonoGame 2013"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;Interface Configuration

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${FrameworkPath}Installers\monogame.bmp"
!define MUI_ABORTWARNING

;--------------------------------
;Languages

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------

; The stuff to install
Section "MonoGame Core Components" ;No components page, name is not important
  SectionIn RO
  SetOutPath $PROGRAMFILES32\MSBuild\${APPNAME}\v${VERSION}
  File '..\monogame.ico'
  File /r '..\..\MonoGame.ContentPipeline\ContentProcessors\bin\x86\Release\*.dll'
  File '..\..\MonoGame.ContentPipeline\*.targets'
  File '..\..\ThirdParty\Libs\NAudio\*.dll'
  File '..\..\ThirdParty\Libs\SharpDX\Windows\*.*'
  File /nonfatal '..\..\ThirdParty\Libs\NAudio\*.xml'
  File /nonfatal '..\..\ThirdParty\Libs\NAudio\*.txt'
  File '..\..\Tools\2MGFX\bin\Release\*.exe'
  
  File '..\..\ThirdParty\Libs\PVRTexLib\Windows_x86_32\Dll\PVRTexLib.dll'
  File /oname=libmojoshader.dll  '..\..\ThirdParty\Libs\libmojoshader_32.dll'
  File '..\..\ThirdParty\Libs\lame_enc.dll'

  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Android'
  File /nonfatal '..\..\MonoGame.Framework\bin\Android\Release\*.dll'
  File /nonfatal ' ..\..\MonoGame.Framework\bin\Android\Release\*.xml'  
  
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsGL'
  File /nonfatal '..\..\MonoGame.Framework\bin\WindowsGL\Release\*.dll'
  File /nonfatal ' ..\..\MonoGame.Framework\bin\WindowsGL\Release\*.xml'
  
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Linux'
  File /nonfatal '..\..\MonoGame.Framework\bin\Linux\Release\*.dll'
  File /nonfatal ' ..\..\MonoGame.Framework\bin\Linux\Release\*.xml'
  
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies'
  File '..\..\ThirdParty\Libs\OpenTK.dll'
  File '..\..\ThirdParty\Libs\OpenTK.dll.config'
  File '..\..\ThirdParty\Libs\OpenTK_svnversion.txt'
  File '..\..\ThirdParty\GamepadConfig\Tao.Sdl.dll'
  File '..\..\ThirdParty\GamepadConfig\SDL.dll'
  
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\Windows8'

  File '..\..\MonoGame.Framework\bin\Windows8\Release\MonoGame.Framework.dll'
  File /nonfatal '..\..\MonoGame.Framework\bin\Windows8\Release\MonoGame.Framework.xml'
  File '..\..\ThirdParty\Libs\SharpDX\Windows 8 Metro\*.dll'
  File '..\..\ThirdParty\Libs\SharpDX\Windows 8 Metro\*.xml'

  ; Install Windows Phone ARM Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsPhone\ARM'

  File '..\..\MonoGame.Framework\bin\WindowsPhone\ARM\Release\MonoGame.Framework.dll'
  File /nonfatal '..\..\MonoGame.Framework\bin\WindowsPhone\ARM\Release\MonoGame.Framework.xml'

  ; Install Windows Phone x86 Assemblies
  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsPhone\x86'

  File '..\..\MonoGame.Framework\bin\WindowsPhone\x86\Release\MonoGame.Framework.dll'
  File /nonfatal '..\..\MonoGame.Framework\bin\WindowsPhone\86\Release\MonoGame.Framework.xml'

  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}\Assemblies\WindowsPhone'

  File /r '..\..\ThirdParty\Libs\SharpDX\Windows Phone\*.dll'
  File /r '..\..\ThirdParty\Libs\SharpDX\Windows Phone\*.xml'  

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies'
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\WindowsGL'
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\Linux'
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\Android'
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\Windows8'
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\WindowsPhone\ARM'
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\WindowsPhone\x86'
    GOTO End32Bitvs64BitCheck

  Is64bit:
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\WindowsGL'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\Linux'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\Android'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\Windows8'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\WindowsPhone\ARM'
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies\WindowsPhone\x86'

  End32Bitvs64BitCheck:
  ; Add remote programs
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayName' '${APPNAME}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayVersion' '${VERSION}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayIcon' '$INSTDIR\monogame.ico'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'InstallLocation' '$INSTDIR\'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'Publisher' 'MonoGame'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'UninstallString' '$INSTDIR\uninstall.exe'


  SetOutPath '$PROGRAMFILES\${APPNAME}\v${VERSION}'
  ; Uninstaller
  WriteUninstaller "uninstall.exe"


SectionEnd

Section "OpenAL"
  ; SetOutPath $INSTDIR
  File '..\..\ThirdParty\Libs\oalinst.exe'
  ExecWait '$INSTDIR\oalinst.exe'
SectionEnd

Section "MonoDevelop Templates"

; Set output path to the installation directory.
  SetOutPath $INSTDIR
  ; check pre-requsites
  ReadRegStr $0 HKLM 'SOFTWARE\Wow6432Node\Xamarin\MonoDevelop' "Path"
  ${If} $0 == "" ; check on 32 bit machines just in case
  ReadRegStr $0 HKLM 'SOFTWARE\Xamarin\MonoDevelop' "Path"
  ${EndIf}

  ${If} $0 == ""
  DetailPrint "MonoDevelop Not Found."
  ${Else}
  DetailPrint "MonoDevelop Found at $0"
  SetOutPath "$0AddIns\MonoDevelop.MonoGame"
  ; install the Templates for MonoDevelop
;  File '..\..\ProjectTemplates\MonoDevelop.MonoGame.${VERSION}\*.*'
  File '..\..\ProjectTemplates\MonoDevelop\MonoDevelop.MonoGame\MonoDevelop.MonoGame\bin\Release\MonoDevelop.MonoGame.dll'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\icons"
  File /r '..\..\ProjectTemplates\MonoDevelop\MonoDevelop.MonoGame\MonoDevelop.MonoGame\icons\*.*'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\templates"
  File /r '..\..\ProjectTemplates\MonoDevelop\MonoDevelop.MonoGame\MonoDevelop.MonoGame\templates\*.*'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\assemblies"
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\assemblies\WindowsGL"
  File /nonfatal '..\..\MonoGame.Framework\bin\WindowsGL\Release\*.dll'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\assemblies\Android"
  File /nonfatal '..\..\MonoGame.Framework\bin\Android\Release\*.dll'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\assemblies\Linux"
  File /nonfatal '..\..\MonoGame.Framework\bin\Linux\Release\*.dll'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\assemblies\MacOS"
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\assemblies\iOS"
  ${EndIf}
  
SectionEnd


Section "Visual Studio 2010 Templates"

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

Section "Visual Studio 2012 Templates"

  IfFileExists `$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    ; Set output path to the installation directory.
    SetOutPath "$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\MonoGame"

    ; install the Templates for MonoDevelop
    File /r '..\..\ProjectTemplates\VisualStudio2012\*.*'
    ; Install the VS 2010 templates as well 
    File /r '..\..\ProjectTemplates\VisualStudio2010\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:

    DetailPrint "Visual Studio 2012 not found"
  EndTemplates:

SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"
	CreateDirectory $SMPROGRAMS\MonoGame
	CreateShortCut "$SMPROGRAMS\MonoGame\Uninstall.lnk" "$PROGRAMFILES\${APPNAME}\v${VERSION}\uninstall.exe" "" "$PROGRAMFILES\${APPNAME}\v${VERSION}\uninstall.exe" 0
	;CreateShortCut "$SMPROGRAMS\MonoGame\GettingStarted.lnk" "$PROGRAMFILES\${APPNAME}\v${VERSION}\GettingStarted.pdf" "" "$PROGRAMFILES\${APPNAME}\v${VERSION}\GettingStarted.pdf" 0
	WriteINIStr "$SMPROGRAMS\MonoGame\Shortcut.url" "InternetShortcut" "URL" "http://www.monogame.net"

SectionEnd


Function .onInit

  IntOp $0 ${SF_SELECTED} | ${SF_RO}
  SectionSetFlags ${core_id} $0
FunctionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  DeleteRegKey HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}'
  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    DeleteRegKey HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}'
    GOTO End32Bitvs64BitCheck

  Is64bit:
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}'

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
  RMDir /r "$SMPROGRAMS\MonoGame"

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

SectionEnd

