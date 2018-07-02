Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Content.Pipeline
Imports Microsoft.Xna.Framework.Content.Pipeline.Graphics

Imports TInput = System.String
Imports TOutput = System.String

Namespace BlankApp
    <ContentProcessor(DisplayName:="Processor1")>
    Class Processor1 : Inherits ContentProcessor(Of TInput, TOutput)

        Public Overrides Function Process(ByVal input As TInput, ByVal context As ContentProcessorContext) As TOutput
            Return Nothing
        End Function
    End Class
End Namespace

