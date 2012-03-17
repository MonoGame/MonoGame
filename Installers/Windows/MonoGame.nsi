SetCompressor /SOLID /FINAL lzma

!define FrameworkPath "C:\Sandbox\MonoGame\"
!define VERSION "2.5"
!define REVISION "0.0"
!define INSTALLERFILENAME "MonoGame"
!define APPNAME "MonoGame"

;Include Modern UI

!include "Sections.nsh"
!include "MUI2.nsh"
!include "InstallOptions.nsh"

Name '${APPNAME} ${VERSION} for MonoDevelop'
OutFile '${INSTALLERFILENAME}-${VERSION}.exe'
InstallDir '$PROGRAMFILES64\${APPNAME}'
VIProductVersion "${VERSION}.${REVISION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} for MonoDevelop"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "MonoGame"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${APPNAME} for MonoDevelop Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "© Copyright MonoGame 2012"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;Interface Configuration

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${FrameworkPath}Installers\monogame.bmp"
!define MUI_ABORTWARNING

; Pages

!insertmacro MUI_PAGE_DIRECTORY

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
  SetOutPath $INSTDIR
  File '..\monogame.ico'
  SetOutPath '$INSTDIR\Assemblies'
  File /r '..\..\ThirdParty\Lidgren.Network\bin\Release\*.dll'
  File /r '..\..\ThirdParty\Lidgren.Network\bin\Release\*.xml'

  File /r '..\..\MonoGame.Framework\bin\Release\*.dll'
  File /r '..\..\MonoGame.Framework\bin\Release\*.xml'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    WriteRegStr HKLM 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies'
    GOTO End32Bitvs64BitCheck

  Is64bit:
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319\AssemblyFoldersEx\${APPNAME}' '' '$INSTDIR\Assemblies'

  End32Bitvs64BitCheck:
  ; Add remote programs
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayName' '${APPNAME}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayVersion' '${VERSION}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayIcon' '$INSTDIR\monogame.ico'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'InstallLocation' '$INSTDIR\'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'Publisher' 'MonoGame'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'UninstallString' '$INSTDIR\uninstall.exe'

  ; Uninstaller
  WriteUninstaller "uninstall.exe"


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
  Abort
  ${Else}
  DetailPrint "MonoDevelop Found at $0"
  ${EndIf}


  SetOutPath "$0AddIns\MonoDevelop.MonoGame"
  ; install the Templates for MonoDevelop
  File '..\..\ProjectTemplates\MonoDevelop.MonoGame.${VERSION}\*.*'
  File '..\..\ProjectTemplates\MonoDevelop.MonoGame.${VERSION}\MonoDevelop.MonoGame\MonoDevelop.MonoGame\bin\Release\MonoDevelop.MonoGame.dll'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\icons"
  File /r '..\..\ProjectTemplates\MonoDevelop.MonoGame.${VERSION}\icons\*.*'
  SetOutPath "$0AddIns\MonoDevelop.MonoGame\templates"
  File /r '..\..\ProjectTemplates\MonoDevelop.MonoGame.${VERSION}\templates\*.*'

  
SectionEnd


Section "Visual Studio 2010 Templates"

  IfFileExists `$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\Visual C#\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    ; Set output path to the installation directory.
    SetOutPath "$DOCUMENTS\Visual Studio 2010\Templates\ProjectTemplates\Visual C#\MonoGame"

    ; install the Templates for MonoDevelop
    File /r '..\..\ProjectTemplates\VisualStudio2010.MonoGame.${VERSION}\*.*'
    GOTO EndTemplates
  CannotInstallTemplates:
  
    DetailPrint "Visual Studio 2010 not found"
  EndTemplates:

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

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

SectionEnd

