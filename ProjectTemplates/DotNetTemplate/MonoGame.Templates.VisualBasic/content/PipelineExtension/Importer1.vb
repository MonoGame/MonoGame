Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Content.Pipeline
Imports Microsoft.Xna.Framework.Content.Pipeline.Graphics

Imports TImport = System.String

Namespace BlankApp
    <ContentImporter(".txt", DisplayName:="Importer1", DefaultProcessor:="Processor1")>
    Public Class Importer1 : Inherits ContentImporter(Of TImport)
        Public Overrides Function Import(ByVal filename As String, ByVal context As ContentImporterContext) As TImport
            Return Nothing
        End Function
    End Class
End Namespace

