/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include "mojoshader.h"

#ifdef _WIN32
#define snprintf _snprintf   // !!! FIXME: not a safe replacement!
#endif

static const char **include_paths = NULL;
static unsigned int include_path_count = 0;

#define MOJOSHADER_DEBUG_MALLOC 0

#if MOJOSHADER_DEBUG_MALLOC
static void *Malloc(int len, void *d)
{
    void *ptr = malloc(len + sizeof (int));
    int *store = (int *) ptr;
    printf("malloc() %d bytes (%p)\n", len, ptr);
    if (ptr == NULL) return NULL;
    *store = len;
    return (void *) (store + 1);
} // Malloc


static void Free(void *_ptr, void *d)
{
    int *ptr = (((int *) _ptr) - 1);
    int len = *ptr;
    printf("free() %d bytes (%p)\n", len, ptr);
    free(ptr);
} // Free
#else
#define Malloc NULL
#define Free NULL
#endif


static void fail(const char *err)
{
    printf("%s.\n", err);
    exit(1);
} // fail

static void print_unroll_attr(FILE *io, const int unroll)
{
    // -1 means "unroll at compiler's discretion",
    // -2 means user didn't specify the attribute.
    switch (unroll)
    {
        case 0:
            fprintf(io, "[loop] ");
            break;
        case -1:
            fprintf(io, "[unroll] ");
            break;
        case -2:
            /* no-op. */
            break;
        default:
            assert(unroll > 0);
            fprintf(io, "[unroll(%d)] ", unroll);
            break;
    } // case
} // print_unroll_attr

static void print_ast_datatype(FILE *io, const MOJOSHADER_astDataType *dt)
{
    int i;

    if (dt == NULL)
        return;

    switch (dt->type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL:
            fprintf(io, "bool");
            return;
        case MOJOSHADER_AST_DATATYPE_INT:
            fprintf(io, "int");
            return;
        case MOJOSHADER_AST_DATATYPE_UINT:
            fprintf(io, "uint");
            return;
        case MOJOSHADER_AST_DATATYPE_FLOAT:
            fprintf(io, "float");
            return;
        case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM:
            fprintf(io, "snorm float");
            return;
        case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM:
            fprintf(io, "unorm float");
            return;
        case MOJOSHADER_AST_DATATYPE_HALF:
            fprintf(io, "half");
            return;
        case MOJOSHADER_AST_DATATYPE_DOUBLE:
            fprintf(io, "double");
            return;
        case MOJOSHADER_AST_DATATYPE_STRING:
            fprintf(io, "string");
            return;
        case MOJOSHADER_AST_DATATYPE_SAMPLER_1D:
            fprintf(io, "sampler1D");
            return;
        case MOJOSHADER_AST_DATATYPE_SAMPLER_2D:
            fprintf(io, "sampler2D");
            return;
        case MOJOSHADER_AST_DATATYPE_SAMPLER_3D:
            fprintf(io, "sampler3D");
            return;
        case MOJOSHADER_AST_DATATYPE_SAMPLER_CUBE:
            fprintf(io, "samplerCUBE");
            return;
        case MOJOSHADER_AST_DATATYPE_SAMPLER_STATE:
            fprintf(io, "sampler_state");
            return;
        case MOJOSHADER_AST_DATATYPE_SAMPLER_COMPARISON_STATE:
            fprintf(io, "SamplerComparisonState");
            return;

        case MOJOSHADER_AST_DATATYPE_STRUCT:
            fprintf(io, "struct { ");
            for (i = 0; i < dt->structure.member_count; i++)
            {
                print_ast_datatype(io, dt->structure.members[i].datatype);
                fprintf(io, " %s; ", dt->structure.members[i].identifier);
            } // for
            fprintf(io, "}");
            return;

        case MOJOSHADER_AST_DATATYPE_ARRAY:
            print_ast_datatype(io, dt->array.base);
            if (dt->array.elements < 0)
                fprintf(io, "[]");
            else
                fprintf(io, "[%d]", dt->array.elements);
            return;

        case MOJOSHADER_AST_DATATYPE_VECTOR:
            fprintf(io, "vector<");
            print_ast_datatype(io, dt->vector.base);
            fprintf(io, ",%d>", dt->vector.elements);
            return;

        case MOJOSHADER_AST_DATATYPE_MATRIX:
            fprintf(io, "matrix<");
            print_ast_datatype(io, dt->matrix.base);
            fprintf(io, ",%d,%d>", dt->matrix.rows, dt->matrix.columns);
            return;

        case MOJOSHADER_AST_DATATYPE_BUFFER:
            fprintf(io, "buffer<");
            print_ast_datatype(io, dt->buffer.base);
            fprintf(io, ">");
            return;

        case MOJOSHADER_AST_DATATYPE_USER:
            fprintf(io, "%s", dt->user.name);
            return;

        // this should only appear if we did semantic analysis on the AST,
        //  so we only print the return value here.
        case MOJOSHADER_AST_DATATYPE_FUNCTION:
            if (!dt->function.retval)
                fprintf(io, "void");
            else
                print_ast_datatype(io, dt->function.retval);
            return;

        //case MOJOSHADER_AST_DATATYPE_NONE:
        default:
            assert(0 && "Unexpected datatype.");
            return;
    } // switch
} // print_ast_datatype

// !!! FIXME: this screws up on order of operations.
static void print_ast(FILE *io, const int substmt, const void *_ast)
{
    const MOJOSHADER_astNode *ast = (const MOJOSHADER_astNode *) _ast;
    const char *nl = substmt ? "" : "\n";
    int typeint = 0;
    static int indent = 0;
    int isblock = 0;
    int i;

    // These _HAVE_ to be in the same order as MOJOSHADER_astNodeType!
    static const char *binary[] =
    {
        ",", "*", "/", "%", "+", "-", "<<", ">>", "<", ">", "<=", ">=", "==",
        "!=", "&", "^", "|", "&&", "||", "=", "*=", "/=", "%=", "+=", "-=",
        "<<=", ">>=", "&=", "^=", "|="
    };

    static const char *pre_unary[] = { "++", "--", "-", "~", "!" };
    static const char *post_unary[] = { "++", "--" };
    static const char *simple_stmt[] = { "", "break", "continue", "discard" };
    static const char *inpmod[] = { "", "in ", "out ", "in out ", "uniform " };
    static const char *fnstorage[] = { "", "inline " };

    static const char *interpmod[] = {
        "", " linear", " centroid", " nointerpolation",
        " noperspective", " sample"
    };

    if (!ast) return;

    typeint = (int) ast->ast.type;

    #define DO_INDENT do { \
        if (!substmt) { for (i = 0; i < indent; i++) fprintf(io, "    "); } \
    } while (0)

    switch (ast->ast.type)
    {
        case MOJOSHADER_AST_OP_PREINCREMENT:
        case MOJOSHADER_AST_OP_PREDECREMENT:
        case MOJOSHADER_AST_OP_NEGATE:
        case MOJOSHADER_AST_OP_COMPLEMENT:
        case MOJOSHADER_AST_OP_NOT:
            fprintf(io, "%s", pre_unary[(typeint-MOJOSHADER_AST_OP_START_RANGE_UNARY)-1]);
            print_ast(io, 0, ast->unary.operand);
            break;

        case MOJOSHADER_AST_OP_POSTINCREMENT:
        case MOJOSHADER_AST_OP_POSTDECREMENT:
            print_ast(io, 0, ast->unary.operand);
            fprintf(io, "%s", post_unary[typeint-MOJOSHADER_AST_OP_POSTINCREMENT]);
            break;

        case MOJOSHADER_AST_OP_MULTIPLY:
        case MOJOSHADER_AST_OP_DIVIDE:
        case MOJOSHADER_AST_OP_MODULO:
        case MOJOSHADER_AST_OP_ADD:
        case MOJOSHADER_AST_OP_SUBTRACT:
        case MOJOSHADER_AST_OP_LSHIFT:
        case MOJOSHADER_AST_OP_RSHIFT:
        case MOJOSHADER_AST_OP_LESSTHAN:
        case MOJOSHADER_AST_OP_GREATERTHAN:
        case MOJOSHADER_AST_OP_LESSTHANOREQUAL:
        case MOJOSHADER_AST_OP_GREATERTHANOREQUAL:
        case MOJOSHADER_AST_OP_EQUAL:
        case MOJOSHADER_AST_OP_NOTEQUAL:
        case MOJOSHADER_AST_OP_BINARYAND:
        case MOJOSHADER_AST_OP_BINARYXOR:
        case MOJOSHADER_AST_OP_BINARYOR:
        case MOJOSHADER_AST_OP_LOGICALAND:
        case MOJOSHADER_AST_OP_LOGICALOR:
        case MOJOSHADER_AST_OP_ASSIGN:
        case MOJOSHADER_AST_OP_MULASSIGN:
        case MOJOSHADER_AST_OP_DIVASSIGN:
        case MOJOSHADER_AST_OP_MODASSIGN:
        case MOJOSHADER_AST_OP_ADDASSIGN:
        case MOJOSHADER_AST_OP_SUBASSIGN:
        case MOJOSHADER_AST_OP_LSHIFTASSIGN:
        case MOJOSHADER_AST_OP_RSHIFTASSIGN:
        case MOJOSHADER_AST_OP_ANDASSIGN:
        case MOJOSHADER_AST_OP_XORASSIGN:
        case MOJOSHADER_AST_OP_ORASSIGN:
        case MOJOSHADER_AST_OP_COMMA:
            print_ast(io, 0, ast->binary.left);
            if (ast->ast.type != MOJOSHADER_AST_OP_COMMA)
                fprintf(io, " ");  // no space before the comma.
            fprintf(io, "%s ", binary[
                (typeint - MOJOSHADER_AST_OP_START_RANGE_BINARY) - 1]);
            print_ast(io, 0, ast->binary.right);
            break;

        case MOJOSHADER_AST_OP_DEREF_ARRAY:
            print_ast(io, 0, ast->binary.left);
            fprintf(io, "[");
            print_ast(io, 0, ast->binary.right);
            fprintf(io, "]");
            break;

        case MOJOSHADER_AST_OP_DEREF_STRUCT:
            print_ast(io, 0, ast->derefstruct.identifier);
            fprintf(io, ".");
            fprintf(io, "%s", ast->derefstruct.member);
            break;

        case MOJOSHADER_AST_OP_CONDITIONAL:
            print_ast(io, 0, ast->ternary.left);
            fprintf(io, " ? ");
            print_ast(io, 0, ast->ternary.center);
            fprintf(io, " : ");
            print_ast(io, 0, ast->ternary.right);
            break;

        case MOJOSHADER_AST_OP_IDENTIFIER:
            fprintf(io, "%s", ast->identifier.identifier);
            break;

        case MOJOSHADER_AST_OP_INT_LITERAL:
            fprintf(io, "%d", ast->intliteral.value);
            break;

        case MOJOSHADER_AST_OP_FLOAT_LITERAL:
        {
            const float f = ast->floatliteral.value;
            const long long flr = (long long) f;
            if (((float) flr) == f)
                fprintf(io, "%lld.0", flr);
            else
                fprintf(io, "%.16g", f);
            break;
        } // case

        case MOJOSHADER_AST_OP_STRING_LITERAL:
            fprintf(io, "\"%s\"", ast->stringliteral.string);
            break;

        case MOJOSHADER_AST_OP_BOOLEAN_LITERAL:
            fprintf(io, "%s", ast->boolliteral.value ? "true" : "false");
            break;

        case MOJOSHADER_AST_ARGUMENTS:
            print_ast(io, 0, ast->arguments.argument);
            if (ast->arguments.next != NULL)
            {
                fprintf(io, ", ");
                print_ast(io, 0, ast->arguments.next);
            } // if
            break;

        case MOJOSHADER_AST_OP_CALLFUNC:
            print_ast(io, 0, ast->callfunc.identifier);
            fprintf(io, "(");
            print_ast(io, 0, ast->callfunc.args);
            fprintf(io, ")");
            break;

        case MOJOSHADER_AST_OP_CONSTRUCTOR:
            print_ast_datatype(io, ast->constructor.datatype);
            fprintf(io, "(");
            print_ast(io, 0, ast->constructor.args);
            fprintf(io, ")");
            break;

        case MOJOSHADER_AST_OP_CAST:
            fprintf(io, "(");
            print_ast_datatype(io, ast->cast.datatype);
            fprintf(io, ") (");
            print_ast(io, 0, ast->cast.operand);
            fprintf(io, ")");
            break;

        case MOJOSHADER_AST_STATEMENT_EXPRESSION:
            DO_INDENT;
            print_ast(io, 0, ast->exprstmt.expr);  // !!! FIXME: This is named badly...
            fprintf(io, ";%s", nl);
            print_ast(io, 0, ast->exprstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_IF:
            DO_INDENT;
            fprintf(io, "if (");
            print_ast(io, 0, ast->ifstmt.expr);
            fprintf(io, ")\n");
            isblock = ast->ifstmt.statement->ast.type == MOJOSHADER_AST_STATEMENT_BLOCK;
            if (!isblock) indent++;
            print_ast(io, 0, ast->ifstmt.statement);
            if (!isblock) indent--;
            print_ast(io, 0, ast->ifstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_TYPEDEF:
            DO_INDENT;
            print_ast(io, 1, ast->typedefstmt.type_info);
            fprintf(io, "%s", nl);
            print_ast(io, 0, ast->typedefstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_SWITCH:
            DO_INDENT;
            switch ( ast->switchstmt.attributes )
            {
                case MOJOSHADER_AST_SWITCHATTR_NONE: break;
                case MOJOSHADER_AST_SWITCHATTR_FLATTEN: fprintf(io, "[flatten] "); break;
                case MOJOSHADER_AST_SWITCHATTR_BRANCH: fprintf(io, "[branch] "); break;
                case MOJOSHADER_AST_SWITCHATTR_FORCECASE: fprintf(io, "[forcecase] "); break;
                case MOJOSHADER_AST_SWITCHATTR_CALL: fprintf(io, "[call] "); break;
            } // switch

            fprintf(io, "switch (");
            print_ast(io, 0, ast->switchstmt.expr);
            fprintf(io, ")\n");
            DO_INDENT;
            fprintf(io, "{\n");
            indent++;
            print_ast(io, 0, ast->switchstmt.cases);
            indent--;
            fprintf(io, "\n");
            DO_INDENT;
            fprintf(io, "}\n");
            print_ast(io, 0, ast->switchstmt.next);
            break;

        case MOJOSHADER_AST_SWITCH_CASE:
            DO_INDENT;
            fprintf(io, "case ");
            print_ast(io, 0, ast->cases.expr);
            fprintf(io, ":\n");
            isblock = ast->cases.statement->ast.type == MOJOSHADER_AST_STATEMENT_BLOCK;
            if (!isblock) indent++;
            print_ast(io, 0, ast->cases.statement);
            if (!isblock) indent--;
            print_ast(io, 0, ast->cases.next);
            break;

        case MOJOSHADER_AST_STATEMENT_STRUCT:
            DO_INDENT;
            print_ast(io, 0, ast->structstmt.struct_info);
            fprintf(io, ";%s%s", nl, nl);  // always space these out.
            print_ast(io, 0, ast->structstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_VARDECL:
            DO_INDENT;
            print_ast(io, 1, ast->vardeclstmt.declaration);
            fprintf(io, ";%s", nl);
            print_ast(io, 0, ast->vardeclstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_BLOCK:
            DO_INDENT;
            fprintf(io, "{\n");
            indent++;
            print_ast(io, 0, ast->blockstmt.statements);
            indent--;
            DO_INDENT;
            fprintf(io, "}\n");
            print_ast(io, 0, ast->blockstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_FOR:
            DO_INDENT;
            print_unroll_attr(io, ast->forstmt.unroll);
            fprintf(io, "for (");
            print_ast(io, 1, ast->forstmt.var_decl);
            print_ast(io, 1, ast->forstmt.initializer);
            fprintf(io, "; ");
            print_ast(io, 1, ast->forstmt.looptest);
            fprintf(io, "; ");
            print_ast(io, 1, ast->forstmt.counter);

            fprintf(io, ")\n");
            isblock = ast->forstmt.statement->ast.type == MOJOSHADER_AST_STATEMENT_BLOCK;
            if (!isblock) indent++;
            print_ast(io, 0, ast->forstmt.statement);
            if (!isblock) indent--;

            print_ast(io, 0, ast->forstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_DO:
            DO_INDENT;
            print_unroll_attr(io, ast->dostmt.unroll);
            fprintf(io, "do\n");

            isblock = ast->dostmt.statement->ast.type == MOJOSHADER_AST_STATEMENT_BLOCK;
            if (!isblock) indent++;
            print_ast(io, 0, ast->dostmt.statement);
            if (!isblock) indent--;

            DO_INDENT;
            fprintf(io, "while (");
            print_ast(io, 0, ast->dostmt.expr);
            fprintf(io, ");\n");

            print_ast(io, 0, ast->dostmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_WHILE:
            DO_INDENT;
            print_unroll_attr(io, ast->whilestmt.unroll);
            fprintf(io, "while (");
            print_ast(io, 0, ast->whilestmt.expr);
            fprintf(io, ")\n");

            isblock = ast->whilestmt.statement->ast.type == MOJOSHADER_AST_STATEMENT_BLOCK;
            if (!isblock) indent++;
            print_ast(io, 0, ast->whilestmt.statement);
            if (!isblock) indent--;

            print_ast(io, 0, ast->whilestmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_RETURN:
            DO_INDENT;
            fprintf(io, "return");
            if (ast->returnstmt.expr)
            {
                fprintf(io, " ");
                print_ast(io, 0, ast->returnstmt.expr);
            } // if
            fprintf(io, ";%s", nl);
            print_ast(io, 0, ast->returnstmt.next);
            break;

        case MOJOSHADER_AST_STATEMENT_EMPTY:
        case MOJOSHADER_AST_STATEMENT_BREAK:
        case MOJOSHADER_AST_STATEMENT_CONTINUE:
        case MOJOSHADER_AST_STATEMENT_DISCARD:
            DO_INDENT;
            fprintf(io, "%s;%s",
                simple_stmt[(typeint-MOJOSHADER_AST_STATEMENT_START_RANGE)-1],
                nl);
            print_ast(io, 0, ast->stmt.next);
            break;

        case MOJOSHADER_AST_COMPUNIT_FUNCTION:
            DO_INDENT;
            print_ast(io, 0, ast->funcunit.declaration);
            if (ast->funcunit.definition == NULL)
                fprintf(io, ";%s", nl);
            else
            {
                fprintf(io, "%s", nl);
                print_ast(io, 0, ast->funcunit.definition);
                fprintf(io, "%s", nl);
            } // else
            print_ast(io, 0, ast->funcunit.next);
            break;

        case MOJOSHADER_AST_COMPUNIT_TYPEDEF:
            DO_INDENT;
            print_ast(io, 0, ast->typedefunit.type_info);
            fprintf(io, "%s", nl);
            print_ast(io, 0, ast->typedefunit.next);
            break;

        case MOJOSHADER_AST_COMPUNIT_STRUCT:
            DO_INDENT;
            print_ast(io, 0, ast->structunit.struct_info);
            fprintf(io, ";%s%s", nl, nl);  // always space these out.
            print_ast(io, 0, ast->structunit.next);
            break;

        case MOJOSHADER_AST_COMPUNIT_VARIABLE:
            DO_INDENT;
            print_ast(io, 1, ast->varunit.declaration);
            fprintf(io, ";%s", nl);
            if (ast->varunit.next &&
                ast->varunit.next->ast.type!=MOJOSHADER_AST_COMPUNIT_VARIABLE)
            {
                fprintf(io, "%s", nl);  // group vars together, and space out other things.
            } // if
            print_ast(io, 0, ast->varunit.next);
            break;

        case MOJOSHADER_AST_SCALAR_OR_ARRAY:
            fprintf(io, "%s", ast->soa.identifier);
            if (ast->soa.isarray)
            {
                fprintf(io, "[");
                print_ast(io, 0, ast->soa.dimension);
                fprintf(io, "]");
            } // if
            break;

        case MOJOSHADER_AST_TYPEDEF:
            DO_INDENT;
            fprintf(io, "typedef %s", ast->typdef.isconst ? "const " : "");
            print_ast_datatype(io, ast->typdef.datatype);
            fprintf(io, " ");
            print_ast(io, 0, ast->typdef.details);
            fprintf(io, ";%s", nl);
            break;

        case MOJOSHADER_AST_FUNCTION_PARAMS:
            fprintf(io, "%s", inpmod[(int) ast->params.input_modifier]);
            print_ast_datatype(io, ast->params.datatype);
            fprintf(io, " %s", ast->params.identifier);
            if (ast->params.semantic)
                fprintf(io, " : %s", ast->params.semantic);
            fprintf(io, "%s", interpmod[(int) ast->params.interpolation_modifier]);

            if (ast->params.initializer)
            {
                fprintf(io, " = ");
                print_ast(io, 0, ast->params.initializer);
            } // if

            if (ast->params.next)
            {
                fprintf(io, ", ");
                print_ast(io, 0, ast->params.next);
            } // if
            break;

        case MOJOSHADER_AST_FUNCTION_SIGNATURE:
            fprintf(io, "%s", fnstorage[(int) ast->funcsig.storage_class]);
            if (ast->funcsig.datatype)
                print_ast_datatype(io, ast->funcsig.datatype);
            else
                fprintf(io, "void");
            fprintf(io, " %s(", ast->funcsig.identifier);
            print_ast(io, 0, ast->funcsig.params);
            fprintf(io, ")");
            if (ast->funcsig.semantic)
                fprintf(io, " : %s", ast->funcsig.semantic);
            break;

        case MOJOSHADER_AST_STRUCT_DECLARATION:
            fprintf(io, "struct %s\n", ast->structdecl.name);
            DO_INDENT;
            fprintf(io, "{\n");
            indent++;
            print_ast(io, 0, ast->structdecl.members);
            indent--;
            DO_INDENT;
            fprintf(io, "}");
            break;

        case MOJOSHADER_AST_STRUCT_MEMBER:
            DO_INDENT;
            fprintf(io, "%s", interpmod[(int)ast->structmembers.interpolation_mod]);
            print_ast_datatype(io, ast->structmembers.datatype);
            fprintf(io, " ");
            print_ast(io, 0, ast->structmembers.details);
            if (ast->structmembers.semantic)
                fprintf(io, " : %s", ast->structmembers.semantic);
            fprintf(io, ";%s", nl);
            print_ast(io, 0, ast->structmembers.next);
            break;

        case MOJOSHADER_AST_VARIABLE_DECLARATION:
            DO_INDENT;
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_EXTERN)
                fprintf(io, "extern ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_NOINTERPOLATION)
                fprintf(io, "nointerpolation ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_SHARED)
                fprintf(io, "shared");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_STATIC)
                fprintf(io, "static ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_UNIFORM)
                fprintf(io, "uniform ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_VOLATILE)
                fprintf(io, "nointerpolation ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_CONST)
                fprintf(io, "const ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_ROWMAJOR)
                fprintf(io, "rowmajor ");
            if (ast->vardecl.attributes & MOJOSHADER_AST_VARATTR_COLUMNMAJOR)
                fprintf(io, "columnmajor ");

            if (ast->vardecl.datatype)
                print_ast_datatype(io, ast->vardecl.datatype);
            else
                print_ast(io, 0, ast->vardecl.anonymous_datatype);
            fprintf(io, " ");
            print_ast(io, 0, ast->vardecl.details);
            if (ast->vardecl.semantic)
                fprintf(io, " : %s", ast->vardecl.semantic);
            if (ast->vardecl.annotations)
            {
                fprintf(io, " ");
                print_ast(io, 0, ast->vardecl.annotations);
            } // if
            if (ast->vardecl.initializer != NULL)
            {
                fprintf(io, " = ");
                print_ast(io, 0, ast->vardecl.initializer);
            } // if
            print_ast(io, 0, ast->vardecl.lowlevel);

            if (ast->vardecl.next == NULL)
                fprintf(io, "%s", nl);
            else
            {
                const int attr = ast->vardecl.next->attributes;
                fprintf(io, ", ");
                ast->vardecl.next->attributes = 0;
                print_ast(io, 1, ast->vardecl.next);
                ast->vardecl.next->attributes = attr;
            } // if
            break;

        case MOJOSHADER_AST_PACK_OFFSET:
            fprintf(io, " : packoffset(%s%s%s)", ast->packoffset.ident1,
                    ast->packoffset.ident2 ? "." : "",
                    ast->packoffset.ident2 ? ast->packoffset.ident2 : "");
            break;

        case MOJOSHADER_AST_VARIABLE_LOWLEVEL:
            print_ast(io, 0, ast->varlowlevel.packoffset);
            if (ast->varlowlevel.register_name)
                fprintf(io, " : register(%s)", ast->varlowlevel.register_name);
            break;

        case MOJOSHADER_AST_ANNOTATION:
        {
            const MOJOSHADER_astAnnotations *a = &ast->annotations;
            fprintf(io, "<");
            while (a)
            {
                fprintf(io, " ");
                print_ast_datatype(io, a->datatype);
                if (a->initializer != NULL)
                {
                    fprintf(io, " = ");
                    print_ast(io, 0, a->initializer);
                } // if
                if (a->next)
                    fprintf(io, ",");
                a = a->next;
            } // while
            fprintf(io, " >");
            break;
        } // case

        default:
            assert(0 && "unexpected type");
            break;
    } // switch

    #undef DO_INDENT
} // print_ast


static int open_include(MOJOSHADER_includeType inctype, const char *fname,
                        const char *parent, const char **outdata,
                        unsigned int *outbytes, MOJOSHADER_malloc m,
                        MOJOSHADER_free f, void *d)
{
    int i;
    for (i = 0; i < include_path_count; i++)
    {
        const char *path = include_paths[i];
        const size_t len = strlen(path) + strlen(fname) + 2;
        char *buf = (char *) m(len, d);
        if (buf == NULL)
            return 0;

        snprintf(buf, len, "%s/%s", path, fname);
        FILE *io = fopen(buf, "rb");
        f(buf, d);
        if (io == NULL)
            continue;

        if (fseek(io, 0, SEEK_END) != -1)
        {
            const long fsize = ftell(io);
            if ((fsize == -1) || (fseek(io, 0, SEEK_SET) == -1))
            {
                fclose(io);
                return 0;
            } // if

            char *data = (char *) m(fsize, d);
            if (data == NULL)
            {
                fclose(io);
                return 0;
            } // if

            if (fread(data, fsize, 1, io) != 1)
            {
                f(data, d);
                fclose(io);
                return 0;
            } // if

            fclose(io);
            *outdata = data;
            *outbytes = (unsigned int) fsize;
            return 1;
        } // if
    } // for

    return 0;
} // open_include


static void close_include(const char *data, MOJOSHADER_malloc m,
                          MOJOSHADER_free f, void *d)
{
    f((void *) data, d);
} // close_include


static int preprocess(const char *fname, const char *buf, int len,
                      const char *outfile,
                      const MOJOSHADER_preprocessorDefine *defs,
                      unsigned int defcount, FILE *io)
{
    const MOJOSHADER_preprocessData *pd;
    int retval = 0;

    pd = MOJOSHADER_preprocess(fname, buf, len, defs, defcount, open_include,
                               close_include, Malloc, Free, NULL);

    if (pd->error_count > 0)
    {
        int i;
        for (i = 0; i < pd->error_count; i++)
        {
            fprintf(stderr, "%s:%d: ERROR: %s\n",
                    pd->errors[i].filename ? pd->errors[i].filename : "???",
                    pd->errors[i].error_position,
                    pd->errors[i].error);
        } // for
    } // if
    else
    {
        if (pd->output != NULL)
        {
            const int len = pd->output_len;
            if ((len) && (fwrite(pd->output, len, 1, io) != 1))
                printf(" ... fwrite('%s') failed.\n", outfile);
            else if ((outfile != NULL) && (fclose(io) == EOF))
                printf(" ... fclose('%s') failed.\n", outfile);
            else
                retval = 1;
        } // if
    } // else
    MOJOSHADER_freePreprocessData(pd);

    return retval;
} // preprocess


static int assemble(const char *fname, const char *buf, int len,
                    const char *outfile,
                    const MOJOSHADER_preprocessorDefine *defs,
                    unsigned int defcount, FILE *io)
{
    const MOJOSHADER_parseData *pd;
    int retval = 0;

    pd = MOJOSHADER_assemble(fname, buf, len, NULL, 0, NULL, 0,
                             defs, defcount, open_include, close_include,
                             Malloc, Free, NULL);

    if (pd->error_count > 0)
    {
        int i;
        for (i = 0; i < pd->error_count; i++)
        {
            fprintf(stderr, "%s:%d: ERROR: %s\n",
                    pd->errors[i].filename ? pd->errors[i].filename : "???",
                    pd->errors[i].error_position,
                    pd->errors[i].error);
        } // for
    } // if
    else
    {
        if (pd->output != NULL)
        {
            const int len = pd->output_len;
            if ((len) && (fwrite(pd->output, len, 1, io) != 1))
                printf(" ... fwrite('%s') failed.\n", outfile);
            else if ((outfile != NULL) && (fclose(io) == EOF))
                printf(" ... fclose('%s') failed.\n", outfile);
            else
                retval = 1;
        } // if
    } // else
    MOJOSHADER_freeParseData(pd);

    return retval;
} // assemble

static int ast(const char *fname, const char *buf, int len,
               const char *outfile, const MOJOSHADER_preprocessorDefine *defs,
               unsigned int defcount, FILE *io)
{
    const MOJOSHADER_astData *ad;
    int retval = 0;

    ad = MOJOSHADER_parseAst(MOJOSHADER_SRC_PROFILE_HLSL_PS_1_1,  // !!! FIXME
                        fname, buf, len, defs, defcount,
                        open_include, close_include, Malloc, Free, NULL);
    
    if (ad->error_count > 0)
    {
        int i;
        for (i = 0; i < ad->error_count; i++)
        {
            fprintf(stderr, "%s:%d: ERROR: %s\n",
                    ad->errors[i].filename ? ad->errors[i].filename : "???",
                    ad->errors[i].error_position,
                    ad->errors[i].error);
        } // for
    } // if
    else
    {
        print_ast(io, 0, ad->ast);
        if ((outfile != NULL) && (fclose(io) == EOF))
            printf(" ... fclose('%s') failed.\n", outfile);
        else
            retval = 1;
    } // else
    MOJOSHADER_freeAstData(ad);

    return retval;
} // ast

static int compile(const char *fname, const char *buf, int len,
                    const char *outfile,
                    const MOJOSHADER_preprocessorDefine *defs,
                    unsigned int defcount, FILE *io)
{
    // !!! FIXME: write me.
    //const MOJOSHADER_parseData *pd;
    //int retval = 0;

    MOJOSHADER_compile(MOJOSHADER_SRC_PROFILE_HLSL_PS_1_1,  // !!! FIXME
                        fname, buf, len, defs, defcount,
                             open_include, close_include,
                             Malloc, Free, NULL);
    return 1;
} // compile

typedef enum
{
    ACTION_UNKNOWN,
    ACTION_VERSION,
    ACTION_PREPROCESS,
    ACTION_ASSEMBLE,
    ACTION_AST,
    ACTION_COMPILE,
} Action;


int main(int argc, char **argv)
{
    Action action = ACTION_UNKNOWN;
    int retval = 1;
    const char *infile = NULL;
    const char *outfile = NULL;
    int i;

    MOJOSHADER_preprocessorDefine *defs = NULL;
    unsigned int defcount = 0;

    include_paths = (const char **) malloc(sizeof (char *));
    include_paths[0] = ".";
    include_path_count = 1;

    // !!! FIXME: clean this up.
    for (i = 1; i < argc; i++)
    {
        const char *arg = argv[i];

        if (strcmp(arg, "-P") == 0)
        {
            if ((action != ACTION_UNKNOWN) && (action != ACTION_PREPROCESS))
                fail("Multiple actions specified");
            action = ACTION_PREPROCESS;
        } // if

        else if (strcmp(arg, "-A") == 0)
        {
            if ((action != ACTION_UNKNOWN) && (action != ACTION_ASSEMBLE))
                fail("Multiple actions specified");
            action = ACTION_ASSEMBLE;
        } // else if

        else if (strcmp(arg, "-T") == 0)
        {
            if ((action != ACTION_UNKNOWN) && (action != ACTION_AST))
                fail("Multiple actions specified");
            action = ACTION_AST;
        } // else if

        else if (strcmp(arg, "-C") == 0)
        {
            if ((action != ACTION_UNKNOWN) && (action != ACTION_COMPILE))
                fail("Multiple actions specified");
            action = ACTION_COMPILE;
        } // else if

        else if ((strcmp(arg, "-V") == 0) || (strcmp(arg, "--version") == 0))
        {
            if ((action != ACTION_UNKNOWN) && (action != ACTION_VERSION))
                fail("Multiple actions specified");
            action = ACTION_VERSION;
        } // else if

        else if (strcmp(arg, "-o") == 0)
        {
            if (outfile != NULL)
                fail("multiple output files specified");

            arg = argv[++i];
            if (arg == NULL)
                fail("no filename after '-o'");
            outfile = arg;
        } // if

        else if (strcmp(arg, "-I") == 0)
        {
            arg = argv[++i];
            if (arg == NULL)
                fail("no path after '-I'");

            include_paths = (const char **) realloc(include_paths,
                       (include_path_count+1) * sizeof (char *));
            include_paths[include_path_count] = arg;
            include_path_count++;
        } // if

        else if (strncmp(arg, "-D", 2) == 0)
        {
            arg += 2;
            char *ident = strdup(arg);
            char *ptr = strchr(ident, '=');
            const char *val = "";
            if (ptr)
            {
                *ptr = '\0';
                val = ptr+1;
            } // if

            defs = (MOJOSHADER_preprocessorDefine *) realloc(defs,
                       (defcount+1) * sizeof (MOJOSHADER_preprocessorDefine));
            defs[defcount].identifier = ident;
            defs[defcount].definition = val;
            defcount++;
        } // else if

        else
        {
            if (infile != NULL)
                fail("multiple input files specified");
            infile = arg;
        } // else
    } // for

    if (action == ACTION_UNKNOWN)
        action = ACTION_ASSEMBLE;

    if (action == ACTION_VERSION)
    {
        printf("mojoshader-compiler, changeset %s\n", MOJOSHADER_CHANGESET);
        return 0;
    } // if

    if (infile == NULL)
        fail("no input file specified");

    FILE *io = fopen(infile, "rb");
    if (io == NULL)
        fail("failed to open input file");

    fseek(io, 0, SEEK_END);
    long fsize = ftell(io);
    fseek(io, 0, SEEK_SET);
    if (fsize == -1)
        fsize = 1000000;
    char *buf = (char *) malloc(fsize);
    const int rc = fread(buf, 1, fsize, io);
    fclose(io);
    if (rc == EOF)
        fail("failed to read input file");

    FILE *outio = outfile ? fopen(outfile, "wb") : stdout;
    if (outio == NULL)
        fail("failed to open output file");


    if (action == ACTION_PREPROCESS)
        retval = (!preprocess(infile, buf, rc, outfile, defs, defcount, outio));
    else if (action == ACTION_ASSEMBLE)
        retval = (!assemble(infile, buf, rc, outfile, defs, defcount, outio));
    else if (action == ACTION_AST)
        retval = (!ast(infile, buf, rc, outfile, defs, defcount, outio));
    else if (action == ACTION_COMPILE)
        retval = (!compile(infile, buf, rc, outfile, defs, defcount, outio));

    if ((retval != 0) && (outfile != NULL))
        remove(outfile);

    free(buf);

    for (i = 0; i < defcount; i++)
        free((void *) defs[i].identifier);
    free(defs);

    free(include_paths);

    return retval;
} // main

// end of mojoshader-compiler.c ...

