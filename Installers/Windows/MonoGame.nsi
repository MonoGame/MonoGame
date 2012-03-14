SetCompressor /SOLID /FINAL lzma

!define FrameworkPath "C:\Sandbox\MonoGame\"
!define VERSION "2.5"
!define REVISION "0.0"
!define INSTALLERFILENAME "MonoGame-MonoDevelop"
!define APPNAME "MonoGame"

;Include Modern UI

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

!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------


Function .onInit
FunctionEnd

; The stuff to install
Section "" ;No components page, name is not important
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
  File /r '..\..\ProjectTemplates\MonoDevelop.MonoGame.${VERSION}\*.*'
  
  SetOutPath '$INSTDIR\Assemblies'
  File '..\monogame.ico'
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

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

SectionEnd
