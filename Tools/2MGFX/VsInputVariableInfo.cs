namespace TwoMGFX
{
    public class VsInputVariableInfo
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string SemanticName { get; set; }
        // true if the variable is declared with the (older) 'attribute' keyword, false if it is declared with 'in'
        // we need this distinction because only 'attribute' variables need to be deleted when the shader is passed
        // to the glsl-optimizer
        public bool AttributeSyntax;
        public ParseNode Node { get; set; }
    }
}