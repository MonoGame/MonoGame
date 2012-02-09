/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

// !!! FIXME: this needs to be split into separate source files:
// !!! FIXME:  parse, AST, IR, etc. The problem is we need to deal with the
// !!! FIXME:  "Context" struct being passed around everywhere.

#define __MOJOSHADER_INTERNAL__ 1
#include "mojoshader_internal.h"

#if DEBUG_COMPILER_PARSER
#define LEMON_SUPPORT_TRACING 1
#endif

// !!! FIXME: I'd like to lose this. It's really inefficient. Just keep a
// !!! FIXME:  (tail) on these list structures instead?
#define REVERSE_LINKED_LIST(typ, head) { \
    if ((head) && (head->next)) { \
        typ *tmp = NULL; \
        typ *tmp1 = NULL; \
        while (head != NULL) { \
            tmp = head; \
            head = head->next; \
            tmp->next = tmp1; \
            tmp1 = tmp; \
        } \
        head = tmp; \
    } \
}

static inline int operator_is_unary(const MOJOSHADER_astNodeType op)
{
    return ( (op > MOJOSHADER_AST_OP_START_RANGE_UNARY) &&
             (op < MOJOSHADER_AST_OP_END_RANGE_UNARY) );
} // operator_is_unary

static inline int operator_is_binary(const MOJOSHADER_astNodeType op)
{
    return ( (op > MOJOSHADER_AST_OP_START_RANGE_BINARY) &&
             (op < MOJOSHADER_AST_OP_END_RANGE_BINARY) );
} // operator_is_binary

static inline int operator_is_ternary(const MOJOSHADER_astNodeType op)
{
    return ( (op > MOJOSHADER_AST_OP_START_RANGE_TERNARY) &&
             (op < MOJOSHADER_AST_OP_END_RANGE_TERNARY) );
} // operator_is_ternary


typedef union TokenData
{
    int64 i64;
    double dbl;
    const char *string;
    const MOJOSHADER_astDataType *datatype;
} TokenData;


// This tracks data types and variables, and notes when they enter/leave scope.

typedef struct SymbolScope
{
    const char *symbol;
    const MOJOSHADER_astDataType *datatype;
    int index;  // unique positive value within a function, negative if global.
    int referenced;  // non-zero if something looked for this symbol (so we know it's used).
    struct SymbolScope *next;
} SymbolScope;

typedef struct SymbolMap
{
    HashTable *hash;
    SymbolScope *scope;
} SymbolMap;

typedef struct LoopLabels
{
    int start;  // loop's start label during IR build.
    int end;    // loop's end label during IR build.
    struct LoopLabels *prev;
} LoopLabels;

// Compile state, passed around all over the place.

typedef struct Context
{
    int isfail;
    int out_of_memory;
    MOJOSHADER_malloc malloc;
    MOJOSHADER_free free;
    void *malloc_data;
    ErrorList *errors;
    ErrorList *warnings;
    StringCache *strcache;
    const char *sourcefile;  // current source file that we're parsing.
    unsigned int sourceline; // current line in sourcefile that we're parsing.
    SymbolMap usertypes;
    SymbolMap variables;
    MOJOSHADER_astNode *ast;  // Abstract Syntax Tree
    const char *source_profile;
    int is_func_scope; // non-zero if semantic analysis is in function scope.
    int loop_count;
    int switch_count;
    int var_index;  // next variable index for current function.
    int global_var_index;  // next variable index for global scope.
    int user_func_index;  // next function index for user-defined functions.
    int intrinsic_func_index;  // next function index for intrinsic functions.

    MOJOSHADER_irStatement **ir;  // intermediate representation.
    int ir_label_count;  // next unused IR label index.
    int ir_temp_count;  // next unused IR temporary value index.
    int ir_end; // current function's end label during IR build.
    int ir_ret; // temp that holds current function's retval during IR build.
    LoopLabels *ir_loop;  // nested loop boundary labels during IR build.

    // Cache intrinsic types for fast lookup and consistent pointer values.
    MOJOSHADER_astDataType dt_none;
    MOJOSHADER_astDataType dt_bool;
    MOJOSHADER_astDataType dt_int;
    MOJOSHADER_astDataType dt_uint;
    MOJOSHADER_astDataType dt_float;
    MOJOSHADER_astDataType dt_float_snorm;
    MOJOSHADER_astDataType dt_float_unorm;
    MOJOSHADER_astDataType dt_half;
    MOJOSHADER_astDataType dt_double;
    MOJOSHADER_astDataType dt_string;
    MOJOSHADER_astDataType dt_sampler1d;
    MOJOSHADER_astDataType dt_sampler2d;
    MOJOSHADER_astDataType dt_sampler3d;
    MOJOSHADER_astDataType dt_samplercube;
    MOJOSHADER_astDataType dt_samplerstate;
    MOJOSHADER_astDataType dt_samplercompstate;
    MOJOSHADER_astDataType dt_buf_bool;
    MOJOSHADER_astDataType dt_buf_int;
    MOJOSHADER_astDataType dt_buf_uint;
    MOJOSHADER_astDataType dt_buf_half;
    MOJOSHADER_astDataType dt_buf_float;
    MOJOSHADER_astDataType dt_buf_double;
    MOJOSHADER_astDataType dt_buf_float_snorm;
    MOJOSHADER_astDataType dt_buf_float_unorm;

    Buffer *garbage;  // this is sort of hacky.
} Context;


// !!! FIXME: cut and paste between every damned source file follows...
// !!! FIXME: We need to make some sort of ContextBase that applies to all
// !!! FIXME:  files and move this stuff to mojoshader_common.c ...

// Convenience functions for allocators...

static inline void out_of_memory(Context *ctx)
{
    ctx->isfail = ctx->out_of_memory = 1;
} // out_of_memory

static inline void *Malloc(Context *ctx, const size_t len)
{
    void *retval = ctx->malloc((int) len, ctx->malloc_data);
    if (retval == NULL)
        out_of_memory(ctx);
    return retval;
} // Malloc

static inline char *StrDup(Context *ctx, const char *str)
{
    char *retval = (char *) Malloc(ctx, strlen(str) + 1);
    if (retval != NULL)
        strcpy(retval, str);
    return retval;
} // StrDup

static inline void Free(Context *ctx, void *ptr)
{
    ctx->free(ptr, ctx->malloc_data);
} // Free

static void *MallocBridge(int bytes, void *data)
{
    return Malloc((Context *) data, (size_t) bytes);
} // MallocBridge

static void FreeBridge(void *ptr, void *data)
{
    Free((Context *) data, ptr);
} // FreeBridge

static void failf(Context *ctx, const char *fmt, ...) ISPRINTF(2,3);
static void failf(Context *ctx, const char *fmt, ...)
{
    ctx->isfail = 1;
    if (ctx->out_of_memory)
        return;

    va_list ap;
    va_start(ap, fmt);
    errorlist_add_va(ctx->errors, ctx->sourcefile, ctx->sourceline, fmt, ap);
    va_end(ap);
} // failf

static inline void fail(Context *ctx, const char *reason)
{
    failf(ctx, "%s", reason);
} // fail

static void warnf(Context *ctx, const char *fmt, ...) ISPRINTF(2,3);
static void warnf(Context *ctx, const char *fmt, ...)
{
    if (ctx->out_of_memory)
        return;

    va_list ap;
    va_start(ap, fmt);
    errorlist_add_va(ctx->warnings, ctx->sourcefile, ctx->sourceline, fmt, ap);
    va_end(ap);
} // warnf

static inline void warn(Context *ctx, const char *reason)
{
    warnf(ctx, "%s", reason);
} // warn

static inline int isfail(const Context *ctx)
{
    return ctx->isfail;
} // isfail


static void symbolmap_nuke(const void *k, const void *v, void *d) {/*no-op*/}

static int create_symbolmap(Context *ctx, SymbolMap *map)
{
    // !!! FIXME: should compare string pointer, with string in cache.
    map->scope = NULL;
    map->hash = hash_create(ctx, hash_hash_string, hash_keymatch_string,
                            symbolmap_nuke, 1, MallocBridge, FreeBridge, ctx);
    return (map->hash != NULL);
} // create_symbolmap

static int datatypes_match(const MOJOSHADER_astDataType *a,
                           const MOJOSHADER_astDataType *b)
{
    int i;

    if (a == b)
        return 1;
    else if (a->type != b->type)
        return 0;

    switch (a->type)
    {
        case MOJOSHADER_AST_DATATYPE_STRUCT:
            if (a->structure.member_count != b->structure.member_count)
                return 0;
            for (i = 0; i < a->structure.member_count; i++)
            {
                if (!datatypes_match(a->structure.members[i].datatype,
                                     b->structure.members[i].datatype))
                    return 0;
                // stringcache'd, pointer compare is safe.
                else if (a->structure.members[i].identifier !=
                         b->structure.members[i].identifier)
                    return 0;
            } // for
            return 1;

        case MOJOSHADER_AST_DATATYPE_ARRAY:
            if (a->array.elements != b->array.elements)
                return 0;
            else if (!datatypes_match(a->array.base, b->array.base))
                return 0;
            return 1;

        case MOJOSHADER_AST_DATATYPE_VECTOR:
            if (a->vector.elements != b->vector.elements)
                return 0;
            else if (!datatypes_match(a->vector.base, b->vector.base))
                return 0;
            return 1;

        case MOJOSHADER_AST_DATATYPE_MATRIX:
            if (a->matrix.rows != b->matrix.rows)
                return 0;
            else if (a->matrix.columns != b->matrix.columns)
                return 0;
            else if (!datatypes_match(a->matrix.base, b->matrix.base))
                return 0;
            return 1;

        case MOJOSHADER_AST_DATATYPE_BUFFER:
            return datatypes_match(a->buffer.base, b->buffer.base);

        case MOJOSHADER_AST_DATATYPE_FUNCTION:
            if (a->function.num_params != b->function.num_params)
                return 0;
            else if (a->function.intrinsic != b->function.intrinsic)
                return 0;
            else if (!datatypes_match(a->function.retval, b->function.retval))
                return 0;
            for (i = 0; i < a->function.num_params; i++)
            {
                if (!datatypes_match(a->function.params[i], b->function.params[i]))
                    return 0;
            } // for
            return 1;

        case MOJOSHADER_AST_DATATYPE_USER:
            return 0;  // pointers must match, this clearly didn't.

        default:
            assert(0 && "unexpected case");
            return 0;
    } // switch

    return 0;
} // datatypes_match

static void push_symbol(Context *ctx, SymbolMap *map, const char *sym,
                        const MOJOSHADER_astDataType *dt, const int index,
                        const int check_dupes)
{
    if (ctx->out_of_memory)
        return;

    // Decide if this symbol is defined, and if it's in the current scope.
    SymbolScope *item = NULL;
    const void *value = NULL;
    if ((check_dupes) && (sym != NULL) && (hash_find(map->hash, sym, &value)))
    {
        // check the current scope for a dupe.
        // !!! FIXME: note current scope's starting index, see if found
        // !!! FIXME:  item is < index (and thus, a previous scope).
        item = map->scope;
        while ((item) && (item->symbol))
        {
            if ( ((const void *) item) == value )
            {
                failf(ctx, "Symbol '%s' already defined", sym);
                return;
            } // if
            item = item->next;
        } // while
    } // if

    // Add the symbol to our map and scope stack.
    item = (SymbolScope *) Malloc(ctx, sizeof (SymbolScope));
    if (item == NULL)
        return;

    if (sym != NULL)  // sym can be NULL if we're pushing a new scope.
    {
        if (hash_insert(map->hash, sym, item) == -1)
        {
            Free(ctx, item);
            return;
        } // if
    } // if

    item->symbol = sym;  // cached strings, don't copy.
    item->index = index;
    item->datatype = dt;
    item->referenced = 0;
    item->next = map->scope;
    map->scope = item;
} // push_symbol

static void push_usertype(Context *ctx, const char *sym, const MOJOSHADER_astDataType *dt)
{
    if (sym != NULL)
    {
        MOJOSHADER_astDataType *userdt;
        userdt = (MOJOSHADER_astDataType *) Malloc(ctx, sizeof (*userdt));
        if (userdt != NULL)
        {
            // !!! FIXME: this is hacky.
            if (!buffer_append(ctx->garbage, &userdt, sizeof (userdt)))
            {
                Free(ctx, userdt);
                return;
            } // if

            userdt->type = MOJOSHADER_AST_DATATYPE_USER;
            userdt->user.details = dt;
            userdt->user.name = sym;

            dt = userdt;
        } // if
    } // if

    push_symbol(ctx, &ctx->usertypes, sym, dt, 0, 1);
} // push_usertype

static inline void push_variable(Context *ctx, const char *sym, const MOJOSHADER_astDataType *dt)
{
    int idx = 0;
    if (sym != NULL)
    {
        // leave space for individual member indexes. The IR will need this.
        int additional = 0;
        if (dt->type == MOJOSHADER_AST_DATATYPE_STRUCT)
            additional = dt->structure.member_count;
        if (ctx->is_func_scope)
        {
            idx = ++ctx->var_index;  // these are positive.
            ctx->var_index += additional;
        } // if
        else
        {
            idx = --ctx->global_var_index;  // these are negative.
            ctx->global_var_index -= additional;
        } // else
    } // if

    push_symbol(ctx, &ctx->variables, sym, dt, idx, 1);
} // push_variable

static int push_function(Context *ctx, const char *sym,
                          const MOJOSHADER_astDataType *dt,
                          const int just_declare)
{
    // we don't have any reason to support nested functions at the moment,
    //  so this would be a bug.
    assert(!ctx->is_func_scope);
    assert(dt->type == MOJOSHADER_AST_DATATYPE_FUNCTION);

    // Functions are always global, so no need to search scopes.
    //  Functions overload, though, so we have to continue iterating to
    //  see if it matches anything.
    const void *value = NULL;
    void *iter = NULL;
    while (hash_iter(ctx->variables.hash, sym, &value, &iter))
    {
        // !!! FIXME: this breaks if you predeclare a function.
        // !!! FIXME:  (a declare AFTER defining works, though.)
        // there's already something called this.
        SymbolScope *item = (SymbolScope *) value;
        if (datatypes_match(dt, item->datatype))
        {
            if (!just_declare)
                failf(ctx, "Function '%s' already defined.", sym);
            return item->index;
        } // if
    } // while

    int idx = 0;
    if ((sym != NULL) && (dt != NULL))
    {
        if (!dt->function.intrinsic)
            idx = ++ctx->user_func_index;  // these are positive.
        else
            idx = --ctx->intrinsic_func_index;  // these are negative.
    } // if

    // push_symbol() doesn't check dupes, because we just did.
    push_symbol(ctx, &ctx->variables, sym, dt, idx, 0);

    return idx;
} // push_function

static inline void push_scope(Context *ctx)
{
    push_usertype(ctx, NULL, NULL);
    push_variable(ctx, NULL, NULL);
} // push_scope

static void pop_symbol(Context *ctx, SymbolMap *map)
{
    SymbolScope *item = map->scope;
    if (!item)
        return;
    if (item->symbol)
        hash_remove(map->hash, item->symbol);
    map->scope = item->next;
    Free(ctx, item);
} // pop_symbol

static void pop_symbol_scope(Context *ctx, SymbolMap *map)
{
    while ((map->scope) && (map->scope->symbol))
        pop_symbol(ctx, map);

    assert(map->scope != NULL);
    assert(map->scope->symbol == NULL);
    pop_symbol(ctx, map);
} // pop_symbol_scope

static inline void pop_scope(Context *ctx)
{
    pop_symbol_scope(ctx, &ctx->usertypes);
    pop_symbol_scope(ctx, &ctx->variables);
} // push_scope

static const MOJOSHADER_astDataType *find_symbol(Context *ctx, SymbolMap *map, const char *sym, int *_index)
{
    const void *_item = NULL;
    hash_find(map->hash, sym, &_item);
    SymbolScope *item = (SymbolScope *) _item;
    if (item != NULL)
    {
        item->referenced++;
        if (_index != NULL)
            *_index = item->index;
    } // if
    return item ? item->datatype : NULL;
} // find_symbol

static inline const MOJOSHADER_astDataType *find_usertype(Context *ctx, const char *sym)
{
    return find_symbol(ctx, &ctx->usertypes, sym, NULL);
} // find_usertype

static inline const MOJOSHADER_astDataType *find_variable(Context *ctx, const char *sym, int *_index)
{
    return find_symbol(ctx, &ctx->variables, sym, _index);
} // find_variable

static void destroy_symbolmap(Context *ctx, SymbolMap *map)
{
    while (map->scope)
        pop_symbol(ctx, map);
    hash_destroy(map->hash);
} // destroy_symbolmap


static const MOJOSHADER_astDataType *new_datatype_vector(Context *ctx,
                                            const MOJOSHADER_astDataType *dt,
                                            const int columns)
{
    MOJOSHADER_astDataType *retval;
    retval = (MOJOSHADER_astDataType *) Malloc(ctx, sizeof (*retval));
    if (retval == NULL)
        return NULL;

    // !!! FIXME: this is hacky.
    // !!! FIXME:  I'd like to cache these anyhow and reuse types.
    if (!buffer_append(ctx->garbage, &retval, sizeof (retval)))
    {
        Free(ctx, retval);
        return NULL;
    } // if

    if ((columns < 1) || (columns > 4))
        fail(ctx, "Vector must have between 1 and 4 elements");

    retval->type = MOJOSHADER_AST_DATATYPE_VECTOR;
    retval->vector.base = dt;
    retval->vector.elements = columns;
    return retval;
} // new_datatype_vector

static const MOJOSHADER_astDataType *new_datatype_matrix(Context *ctx,
                                            const MOJOSHADER_astDataType *dt,
                                            const int rows, const int columns)
{
    MOJOSHADER_astDataType *retval;
    // !!! FIXME: allocate enough for a matrix, but we need to cleanup things that copy without checking for subsize.
    retval = (MOJOSHADER_astDataType *) Malloc(ctx, sizeof (*retval));
    if (retval == NULL)
        return NULL;

    // !!! FIXME: this is hacky.
    // !!! FIXME:  I'd like to cache these anyhow and reuse types.
    if (!buffer_append(ctx->garbage, &retval, sizeof (retval)))
    {
        Free(ctx, retval);
        return NULL;
    } // if

    if ((rows < 1) || (rows > 4))
        fail(ctx, "Matrix must have between 1 and 4 rows");
    if ((columns < 1) || (columns > 4))
        fail(ctx, "Matrix must have between 1 and 4 columns");

    retval->type = MOJOSHADER_AST_DATATYPE_MATRIX;
    retval->matrix.base = dt;
    retval->matrix.rows = rows;
    retval->matrix.columns = columns;
    return retval;
} // new_datatype_matrix


// !!! FIXME: move this to mojoshader_ast.c
// !!! FIXME: new_* and delete_* should take an allocator, not a context.

// These functions are mostly for construction and cleanup of nodes in the
//  parse tree. Mostly this is simple allocation and initialization, so we
//  can do as little in the lemon code as possible, and then sort it all out
//  afterwards.

#define NEW_AST_NODE(retval, cls, typ) \
    cls *retval = (cls *) Malloc(ctx, sizeof (cls)); \
    do { \
        if (retval == NULL) { return NULL; } \
        retval->ast.type = typ; \
        retval->ast.filename = ctx->sourcefile; \
        retval->ast.line = ctx->sourceline; \
    } while (0)

#define DELETE_AST_NODE(cls) do { \
    if (!cls) return; \
} while (0)


static void delete_compilation_unit(Context*, MOJOSHADER_astCompilationUnit*);
static void delete_statement(Context *ctx, MOJOSHADER_astStatement *stmt);

static MOJOSHADER_astExpression *new_identifier_expr(Context *ctx,
                                                     const char *string)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionIdentifier,
                 MOJOSHADER_AST_OP_IDENTIFIER);
    retval->datatype = NULL;
    retval->identifier = string;  // cached; don't copy string.
    retval->index = 0;
    return (MOJOSHADER_astExpression *) retval;
} // new_identifier_expr

static MOJOSHADER_astExpression *new_callfunc_expr(Context *ctx,
                                        const char *identifier,
                                        MOJOSHADER_astArguments *args)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionCallFunction,
                 MOJOSHADER_AST_OP_CALLFUNC);
    MOJOSHADER_astExpression *expr = new_identifier_expr(ctx, identifier);
    retval->datatype = NULL;
    retval->identifier = (MOJOSHADER_astExpressionIdentifier *) expr;
    retval->args = args;
    return (MOJOSHADER_astExpression *) retval;
} // new_callfunc_expr

static MOJOSHADER_astExpression *new_constructor_expr(Context *ctx,
                                            const MOJOSHADER_astDataType *dt,
                                            MOJOSHADER_astArguments *args)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionConstructor,
                 MOJOSHADER_AST_OP_CONSTRUCTOR);
    retval->datatype = dt;
    retval->args = args;
    return (MOJOSHADER_astExpression *) retval;
} // new_constructor_expr

static MOJOSHADER_astExpression *new_cast_expr(Context *ctx,
                                            const MOJOSHADER_astDataType *dt,
                                            MOJOSHADER_astExpression *operand)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionCast, MOJOSHADER_AST_OP_CAST);
    retval->datatype = dt;
    retval->operand = operand;
    return (MOJOSHADER_astExpression *) retval;
} // new_cast_expr

static MOJOSHADER_astExpression *new_unary_expr(Context *ctx,
                                            const MOJOSHADER_astNodeType op,
                                            MOJOSHADER_astExpression *operand)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionUnary, op);
    assert(operator_is_unary(op));
    retval->datatype = NULL;
    retval->operand = operand;
    return (MOJOSHADER_astExpression *) retval;
} // new_unary_expr

static MOJOSHADER_astExpression *new_binary_expr(Context *ctx,
                                            const MOJOSHADER_astNodeType op,
                                            MOJOSHADER_astExpression *left,
                                            MOJOSHADER_astExpression *right)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionBinary, op);
    assert(operator_is_binary(op));
    retval->datatype = NULL;
    retval->left = left;
    retval->right = right;
    return (MOJOSHADER_astExpression *) retval;
} // new_binary_expr

static MOJOSHADER_astExpression *new_ternary_expr(Context *ctx,
                                            const MOJOSHADER_astNodeType op,
                                            MOJOSHADER_astExpression *left,
                                            MOJOSHADER_astExpression *center,
                                            MOJOSHADER_astExpression *right)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionTernary, op);
    assert(operator_is_ternary(op));
    assert(op == MOJOSHADER_AST_OP_CONDITIONAL);
    retval->datatype = &ctx->dt_bool;
    retval->left = left;
    retval->center = center;
    retval->right = right;
    return (MOJOSHADER_astExpression *) retval;
} // new_ternary_expr

static MOJOSHADER_astExpression *new_deref_struct_expr(Context *ctx,
                                        MOJOSHADER_astExpression *identifier,
                                        const char *member)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionDerefStruct,
                 MOJOSHADER_AST_OP_DEREF_STRUCT);
    retval->datatype = NULL;
    retval->identifier = identifier;
    retval->member = member;  // cached; don't copy string.
    retval->isswizzle = 0;  // may change during semantic analysis.
    retval->member_index = 0;  // set during semantic analysis.
    return (MOJOSHADER_astExpression *) retval;
} // new_deref_struct_expr

static MOJOSHADER_astExpression *new_literal_int_expr(Context *ctx,
                                                       const int value)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionIntLiteral,
                 MOJOSHADER_AST_OP_INT_LITERAL);
    retval->datatype = &ctx->dt_int;
    retval->value = value;
    return (MOJOSHADER_astExpression *) retval;
} // new_literal_int_expr

static MOJOSHADER_astExpression *new_literal_float_expr(Context *ctx,
                                                        const double dbl)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionFloatLiteral,
                 MOJOSHADER_AST_OP_FLOAT_LITERAL);
    retval->datatype = &ctx->dt_float;
    retval->value = dbl;
    return (MOJOSHADER_astExpression *) retval;
} // new_literal_float_expr

static MOJOSHADER_astExpression *new_literal_string_expr(Context *ctx,
                                                         const char *string)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionStringLiteral,
                 MOJOSHADER_AST_OP_STRING_LITERAL);
    retval->datatype = &ctx->dt_string;
    retval->string = string;  // cached; don't copy string.
    return (MOJOSHADER_astExpression *) retval;
} // new_literal_string_expr

static MOJOSHADER_astExpression *new_literal_boolean_expr(Context *ctx,
                                                          const int value)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionBooleanLiteral,
                 MOJOSHADER_AST_OP_BOOLEAN_LITERAL);
    retval->datatype = &ctx->dt_bool;
    retval->value = value;
    return (MOJOSHADER_astExpression *) retval;
} // new_literal_boolean_expr

static void delete_arguments(Context *ctx, MOJOSHADER_astArguments *args);

static void delete_expr(Context *ctx, MOJOSHADER_astExpression *_expr)
{
    MOJOSHADER_astNode *expr = (MOJOSHADER_astNode *) _expr;

    DELETE_AST_NODE(expr);

    if (expr->ast.type == MOJOSHADER_AST_OP_CAST)
        delete_expr(ctx, expr->cast.operand);

    else if (expr->ast.type == MOJOSHADER_AST_OP_CONSTRUCTOR)
        delete_arguments(ctx, expr->constructor.args);

    else if (expr->ast.type == MOJOSHADER_AST_OP_DEREF_STRUCT)
        delete_expr(ctx, expr->derefstruct.identifier);

    else if (operator_is_unary(expr->ast.type))
        delete_expr(ctx, expr->unary.operand);

    else if (operator_is_binary(expr->ast.type))
    {
        delete_expr(ctx, expr->binary.left);
        delete_expr(ctx, expr->binary.right);
    } // else if

    else if (operator_is_ternary(expr->ast.type))
    {
        delete_expr(ctx, expr->ternary.left);
        delete_expr(ctx, expr->ternary.center);
        delete_expr(ctx, expr->ternary.right);
    } // else if

    else if (expr->ast.type == MOJOSHADER_AST_OP_CALLFUNC)
    {
        delete_expr(ctx, (MOJOSHADER_astExpression*)expr->callfunc.identifier);
        delete_arguments(ctx, expr->callfunc.args);
    } // else if

    // rest of operators don't have extra data to free.

    Free(ctx, expr);
} // delete_expr

static MOJOSHADER_astArguments *new_argument(Context *ctx,
                                             MOJOSHADER_astExpression *arg)
{
    NEW_AST_NODE(retval, MOJOSHADER_astArguments, MOJOSHADER_AST_ARGUMENTS);
    retval->argument = arg;
    retval->next = NULL;
    return retval;
} // new_argument

static void delete_arguments(Context *ctx, MOJOSHADER_astArguments *args)
{
    DELETE_AST_NODE(args);
    delete_arguments(ctx, args->next);
    delete_expr(ctx, args->argument);
    Free(ctx, args);
} // delete_arguments

static MOJOSHADER_astFunctionParameters *new_function_param(Context *ctx,
                        const MOJOSHADER_astInputModifier inputmod,
                        const MOJOSHADER_astDataType *dt,
                        const char *identifier, const char *semantic,
                        const MOJOSHADER_astInterpolationModifier interpmod,
                        MOJOSHADER_astExpression *initializer)
{
    NEW_AST_NODE(retval, MOJOSHADER_astFunctionParameters,
                 MOJOSHADER_AST_FUNCTION_PARAMS);
    retval->datatype = dt;
    retval->input_modifier = inputmod;
    retval->identifier = identifier;
    retval->semantic = semantic;
    retval->interpolation_modifier = interpmod;
    retval->initializer = initializer;
    retval->next = NULL;
    return retval;
} // new_function_param

static void delete_function_params(Context *ctx,
                                   MOJOSHADER_astFunctionParameters *params)
{
    DELETE_AST_NODE(params);
    delete_function_params(ctx, params->next);
    delete_expr(ctx, params->initializer);
    Free(ctx, params);
} // delete_function_params

static MOJOSHADER_astFunctionSignature *new_function_signature(Context *ctx,
                                    const MOJOSHADER_astDataType *dt,
                                    const char *identifier,
                                    MOJOSHADER_astFunctionParameters *params)
{
    NEW_AST_NODE(retval, MOJOSHADER_astFunctionSignature,
                 MOJOSHADER_AST_FUNCTION_SIGNATURE);
    retval->datatype = dt;
    retval->identifier = identifier;
    retval->params = params;
    retval->storage_class = MOJOSHADER_AST_FNSTORECLS_NONE;
    retval->semantic = NULL;
    return retval;
} // new_function_signature

static void delete_function_signature(Context *ctx,
                                      MOJOSHADER_astFunctionSignature *sig)
{
    DELETE_AST_NODE(sig);
    delete_function_params(ctx, sig->params);
    Free(ctx, sig);
} // delete_function_signature

static MOJOSHADER_astCompilationUnit *new_function(Context *ctx,
                                MOJOSHADER_astFunctionSignature *declaration,
                                MOJOSHADER_astStatement *definition)
{
    NEW_AST_NODE(retval, MOJOSHADER_astCompilationUnitFunction,
                 MOJOSHADER_AST_COMPUNIT_FUNCTION);
    retval->next = NULL;
    retval->declaration = declaration;
    retval->definition = definition;
    retval->index = 0;
    return (MOJOSHADER_astCompilationUnit *) retval;
} // new_function

static void delete_function(Context *ctx,
                            MOJOSHADER_astCompilationUnitFunction *unitfn)
{
    DELETE_AST_NODE(unitfn);
    delete_compilation_unit(ctx, unitfn->next);
    delete_function_signature(ctx, unitfn->declaration);
    delete_statement(ctx, unitfn->definition);
    Free(ctx, unitfn);
} // delete_function

static MOJOSHADER_astScalarOrArray *new_scalar_or_array(Context *ctx,
                                          const char *ident, const int isvec,
                                          MOJOSHADER_astExpression *dim)
{
    NEW_AST_NODE(retval, MOJOSHADER_astScalarOrArray,
                 MOJOSHADER_AST_SCALAR_OR_ARRAY);
    retval->identifier = ident;
    retval->isarray = isvec;
    retval->dimension = dim;
    return retval;
} // new_scalar_or_array

static void delete_scalar_or_array(Context *ctx,MOJOSHADER_astScalarOrArray *s)
{
    DELETE_AST_NODE(s);
    delete_expr(ctx, s->dimension);
    Free(ctx, s);
} // delete_scalar_or_array

static MOJOSHADER_astTypedef *new_typedef(Context *ctx, const int isconst,
                                          const MOJOSHADER_astDataType *dt,
                                          MOJOSHADER_astScalarOrArray *soa)
{
    // we correct this datatype to the final version during semantic analysis.
    NEW_AST_NODE(retval, MOJOSHADER_astTypedef, MOJOSHADER_AST_TYPEDEF);
    retval->datatype = dt;
    retval->isconst = isconst;
    retval->details = soa;
    return retval;
} // new_typedef

static void delete_typedef(Context *ctx, MOJOSHADER_astTypedef *td)
{
    DELETE_AST_NODE(td);
    delete_scalar_or_array(ctx, td->details);
    Free(ctx, td);
} // delete_typedef

static MOJOSHADER_astPackOffset *new_pack_offset(Context *ctx,
                                                 const char *a, const char *b)
{
    NEW_AST_NODE(retval, MOJOSHADER_astPackOffset, MOJOSHADER_AST_PACK_OFFSET);
    retval->ident1 = a;
    retval->ident2 = b;
    return retval;
} // new_pack_offset

static void delete_pack_offset(Context *ctx, MOJOSHADER_astPackOffset *o)
{
    DELETE_AST_NODE(o);
    Free(ctx, o);
} // delete_pack_offset

static MOJOSHADER_astVariableLowLevel *new_variable_lowlevel(Context *ctx,
                                               MOJOSHADER_astPackOffset *po,
                                               const char *reg)
{
    NEW_AST_NODE(retval, MOJOSHADER_astVariableLowLevel,
                 MOJOSHADER_AST_VARIABLE_LOWLEVEL);
    retval->packoffset = po;
    retval->register_name = reg;
    return retval;
} // new_variable_lowlevel

static void delete_variable_lowlevel(Context *ctx,
                                     MOJOSHADER_astVariableLowLevel *vll)
{
    DELETE_AST_NODE(vll);
    delete_pack_offset(ctx, vll->packoffset);
    Free(ctx, vll);
} // delete_variable_lowlevel

static MOJOSHADER_astAnnotations *new_annotation(Context *ctx,
                                        const MOJOSHADER_astDataType *dt,
                                        MOJOSHADER_astExpression *initializer)
{
    NEW_AST_NODE(retval, MOJOSHADER_astAnnotations, MOJOSHADER_AST_ANNOTATION);
    retval->datatype = dt;
    retval->initializer = initializer;
    retval->next = NULL;
    return retval;
} // new_annotation

static void delete_annotation(Context *ctx, MOJOSHADER_astAnnotations *annos)
{
    DELETE_AST_NODE(annos);
    delete_annotation(ctx, annos->next);
    delete_expr(ctx, annos->initializer);
    Free(ctx, annos);
} // delete_annotation

static MOJOSHADER_astVariableDeclaration *new_variable_declaration(
                            Context *ctx, MOJOSHADER_astScalarOrArray *soa,
                            const char *semantic,
                            MOJOSHADER_astAnnotations *annotations,
                            MOJOSHADER_astExpression *init,
                            MOJOSHADER_astVariableLowLevel *vll)
{
    NEW_AST_NODE(retval, MOJOSHADER_astVariableDeclaration,
                 MOJOSHADER_AST_VARIABLE_DECLARATION);
    retval->datatype = NULL;
    retval->attributes = 0;
    retval->anonymous_datatype = NULL;
    retval->details = soa;
    retval->semantic = semantic;
    retval->annotations = annotations;
    retval->initializer = init;
    retval->lowlevel = vll;
    retval->next = NULL;
    return retval;
} // new_variable_declaration

static void delete_variable_declaration(Context *ctx,
                                        MOJOSHADER_astVariableDeclaration *dcl)
{
    DELETE_AST_NODE(dcl);
    delete_variable_declaration(ctx, dcl->next);
    delete_scalar_or_array(ctx, dcl->details);
    delete_annotation(ctx, dcl->annotations);
    delete_expr(ctx, dcl->initializer);
    delete_variable_lowlevel(ctx, dcl->lowlevel);
    Free(ctx, dcl);
} // delete_variable_declaration

static MOJOSHADER_astCompilationUnit *new_global_variable(Context *ctx,
                                      MOJOSHADER_astVariableDeclaration *decl)
{
    NEW_AST_NODE(retval, MOJOSHADER_astCompilationUnitVariable,
                 MOJOSHADER_AST_COMPUNIT_VARIABLE);
    retval->next = NULL;
    retval->declaration = decl;
    return (MOJOSHADER_astCompilationUnit *) retval;
} // new_global_variable

static void delete_global_variable(Context *ctx,
                                   MOJOSHADER_astCompilationUnitVariable *var)
{
    DELETE_AST_NODE(var);
    delete_compilation_unit(ctx, var->next);
    delete_variable_declaration(ctx, var->declaration);
    Free(ctx, var);
} // delete_global_variable

static MOJOSHADER_astCompilationUnit *new_global_typedef(Context *ctx,
                                                     MOJOSHADER_astTypedef *td)
{
    NEW_AST_NODE(retval, MOJOSHADER_astCompilationUnitTypedef,
                 MOJOSHADER_AST_COMPUNIT_TYPEDEF);
    retval->next = NULL;
    retval->type_info = td;
    return (MOJOSHADER_astCompilationUnit *) retval;
} // new_global_typedef

static void delete_global_typedef(Context *ctx,
                                  MOJOSHADER_astCompilationUnitTypedef *unit)
{
    DELETE_AST_NODE(unit);
    delete_compilation_unit(ctx, unit->next);
    delete_typedef(ctx, unit->type_info);
    Free(ctx, unit);
} // delete_global_typedef

static MOJOSHADER_astStructMembers *new_struct_member(Context *ctx,
                                            MOJOSHADER_astScalarOrArray *soa,
                                            const char *semantic)
{
    NEW_AST_NODE(retval, MOJOSHADER_astStructMembers,
                 MOJOSHADER_AST_STRUCT_MEMBER);
    retval->datatype = NULL;
    retval->semantic = semantic;
    retval->details = soa;
    retval->interpolation_mod = MOJOSHADER_AST_INTERPMOD_NONE;
    retval->next = NULL;
    return retval;
} // new_struct_member

static void delete_struct_member(Context *ctx,
                                 MOJOSHADER_astStructMembers *member)
{
    DELETE_AST_NODE(member);
    delete_struct_member(ctx, member->next);
    delete_scalar_or_array(ctx, member->details);
    Free(ctx, member);
} // delete_struct_member

static MOJOSHADER_astStructDeclaration *new_struct_declaration(Context *ctx,
                                        const char *name,
                                        MOJOSHADER_astStructMembers *members)
{
    NEW_AST_NODE(retval, MOJOSHADER_astStructDeclaration,
                 MOJOSHADER_AST_STRUCT_DECLARATION);
    retval->datatype = NULL;
    retval->name = name;
    retval->members = members;
    return retval;
} // new_struct_declaration

static void delete_struct_declaration(Context *ctx,
                                      MOJOSHADER_astStructDeclaration *decl)
{
    DELETE_AST_NODE(decl);
    delete_struct_member(ctx, decl->members);
    Free(ctx, decl);
} // delete_struct_declaration

static MOJOSHADER_astCompilationUnit *new_global_struct(Context *ctx,
                                           MOJOSHADER_astStructDeclaration *sd)
{
    NEW_AST_NODE(retval, MOJOSHADER_astCompilationUnitStruct,
                 MOJOSHADER_AST_COMPUNIT_STRUCT);
    retval->next = NULL;
    retval->struct_info = sd;
    return (MOJOSHADER_astCompilationUnit *) retval;
} // new_global_struct

static void delete_global_struct(Context *ctx,
                                 MOJOSHADER_astCompilationUnitStruct *unit)
{
    DELETE_AST_NODE(unit);
    delete_compilation_unit(ctx, unit->next);
    delete_struct_declaration(ctx, unit->struct_info);
    Free(ctx, unit);
} // delete_global_struct

static void delete_compilation_unit(Context *ctx,
                                    MOJOSHADER_astCompilationUnit *unit)
{
    if (!unit) return;

    // it's important to not recurse too deeply here, since you may have
    //  thousands of items in this linked list (each line of a massive
    //  function, for example). To avoid this, we iterate the list here,
    //  deleting all children and making them think they have no reason
    //  to recurse in their own delete methods.
    // Please note that everyone should _try_ to delete their "next" member,
    //  just in case, but hopefully this cleaned it out.

    MOJOSHADER_astCompilationUnit *i = unit->next;
    unit->next = NULL;
    while (i)
    {
        MOJOSHADER_astCompilationUnit *next = i->next;
        i->next = NULL;
        delete_compilation_unit(ctx, i);
        i = next;
    } // while

    switch (unit->ast.type)
    {
        #define DELETE_UNIT(typ, cls, fn) \
            case MOJOSHADER_AST_COMPUNIT_##typ: delete_##fn(ctx, (cls *) unit); break;
        DELETE_UNIT(FUNCTION, MOJOSHADER_astCompilationUnitFunction, function);
        DELETE_UNIT(TYPEDEF, MOJOSHADER_astCompilationUnitTypedef, global_typedef);
        DELETE_UNIT(VARIABLE, MOJOSHADER_astCompilationUnitVariable, global_variable);
        DELETE_UNIT(STRUCT, MOJOSHADER_astCompilationUnitStruct, global_struct);
        #undef DELETE_UNIT
        default: assert(0 && "missing cleanup code"); break;
    } // switch

    // don't free (unit) here, the class-specific functions do it.
} // delete_compilation_unit

static MOJOSHADER_astStatement *new_typedef_statement(Context *ctx,
                                                      MOJOSHADER_astTypedef *td)
{
    NEW_AST_NODE(retval, MOJOSHADER_astTypedefStatement,
                 MOJOSHADER_AST_STATEMENT_TYPEDEF);
    retval->next = NULL;
    retval->type_info = td;
    return (MOJOSHADER_astStatement *) retval;
} // new_typedef_statement

static void delete_typedef_statement(Context *ctx,
                                     MOJOSHADER_astTypedefStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_typedef(ctx, stmt->type_info);
    Free(ctx, stmt);
} // delete_typedef_statement

static MOJOSHADER_astStatement *new_return_statement(Context *ctx,
                                                MOJOSHADER_astExpression *expr)
{
    NEW_AST_NODE(retval, MOJOSHADER_astReturnStatement,
                 MOJOSHADER_AST_STATEMENT_RETURN);
    retval->next = NULL;
    retval->expr = expr;
    return (MOJOSHADER_astStatement *) retval;
} // new_return_statement

static void delete_return_statement(Context *ctx,
                                    MOJOSHADER_astReturnStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_expr(ctx, stmt->expr);
    Free(ctx, stmt);
} // delete_return_statement

static MOJOSHADER_astStatement *new_block_statement(Context *ctx,
                                               MOJOSHADER_astStatement *stmts)
{
    NEW_AST_NODE(retval, MOJOSHADER_astBlockStatement,
                 MOJOSHADER_AST_STATEMENT_BLOCK);
    retval->next = NULL;
    retval->statements = stmts;
    return (MOJOSHADER_astStatement *) retval;
} // new_block_statement

static void delete_block_statement(Context *ctx,
                                   MOJOSHADER_astBlockStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->statements);
    delete_statement(ctx, stmt->next);
    Free(ctx, stmt);
} // delete_statement_block

static MOJOSHADER_astStatement *new_for_statement(Context *ctx,
                                    MOJOSHADER_astVariableDeclaration *decl,
                                    MOJOSHADER_astExpression *initializer,
                                    MOJOSHADER_astExpression *looptest,
                                    MOJOSHADER_astExpression *counter,
                                    MOJOSHADER_astStatement *statement)
{
    NEW_AST_NODE(retval, MOJOSHADER_astForStatement,
                 MOJOSHADER_AST_STATEMENT_FOR);
    retval->next = NULL;
    retval->unroll = -1;
    retval->var_decl = decl;
    retval->initializer = initializer;
    retval->looptest = looptest;
    retval->counter = counter;
    retval->statement = statement;
    return (MOJOSHADER_astStatement *) retval;
} // new_for_statement

static void delete_for_statement(Context *ctx,MOJOSHADER_astForStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_variable_declaration(ctx, stmt->var_decl);
    delete_expr(ctx, stmt->initializer);
    delete_expr(ctx, stmt->looptest);
    delete_expr(ctx, stmt->counter);
    delete_statement(ctx, stmt->statement);
    Free(ctx, stmt);
} // delete_for_statement

static MOJOSHADER_astStatement *new_do_statement(Context *ctx,
                                                const int unroll,
                                                MOJOSHADER_astStatement *stmt,
                                                MOJOSHADER_astExpression *expr)
{
    NEW_AST_NODE(retval,MOJOSHADER_astDoStatement,MOJOSHADER_AST_STATEMENT_DO);
    retval->next = NULL;
    retval->unroll = unroll;
    retval->expr = expr;
    retval->statement = stmt;
    return (MOJOSHADER_astStatement *) retval;
} // new_do_statement

static void delete_do_statement(Context *ctx, MOJOSHADER_astDoStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_statement(ctx, stmt->statement);
    delete_expr(ctx, stmt->expr);
    Free(ctx, stmt);
} // delete_do_statement

static MOJOSHADER_astStatement *new_while_statement(Context *ctx,
                                                const int unroll,
                                                MOJOSHADER_astExpression *expr,
                                                MOJOSHADER_astStatement *stmt)
{
    NEW_AST_NODE(retval, MOJOSHADER_astWhileStatement,
                 MOJOSHADER_AST_STATEMENT_WHILE);
    retval->next = NULL;
    retval->unroll = unroll;
    retval->expr = expr;
    retval->statement = stmt;
    return (MOJOSHADER_astStatement *) retval;
} // new_while_statement

static void delete_while_statement(Context *ctx,
                                   MOJOSHADER_astWhileStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_statement(ctx, stmt->statement);
    delete_expr(ctx, stmt->expr);
    Free(ctx, stmt);
} // delete_while_statement

static MOJOSHADER_astStatement *new_if_statement(Context *ctx,
                                            const int attr,
                                            MOJOSHADER_astExpression *expr,
                                            MOJOSHADER_astStatement *stmt,
                                            MOJOSHADER_astStatement *elsestmt)
{
    NEW_AST_NODE(retval,MOJOSHADER_astIfStatement,MOJOSHADER_AST_STATEMENT_IF);
    retval->next = NULL;
    retval->attributes = attr;
    retval->expr = expr;
    retval->statement = stmt;
    retval->else_statement = elsestmt;
    return (MOJOSHADER_astStatement *) retval;
} // new_if_statement

static void delete_if_statement(Context *ctx, MOJOSHADER_astIfStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_expr(ctx, stmt->expr);
    delete_statement(ctx, stmt->statement);
    delete_statement(ctx, stmt->else_statement);
    Free(ctx, stmt);
} // delete_if_statement

static MOJOSHADER_astSwitchCases *new_switch_case(Context *ctx,
                                                MOJOSHADER_astExpression *expr,
                                                MOJOSHADER_astStatement *stmt)
{
    NEW_AST_NODE(retval, MOJOSHADER_astSwitchCases, MOJOSHADER_AST_SWITCH_CASE);
    retval->expr = expr;
    retval->statement = stmt;
    retval->next = NULL;
    return retval;
} // new_switch_case

static void delete_switch_case(Context *ctx, MOJOSHADER_astSwitchCases *sc)
{
    DELETE_AST_NODE(sc);
    delete_switch_case(ctx, sc->next);
    delete_expr(ctx, sc->expr);
    delete_statement(ctx, sc->statement);
    Free(ctx, sc);
} // delete_switch_case

static MOJOSHADER_astStatement *new_empty_statement(Context *ctx)
{
    NEW_AST_NODE(retval, MOJOSHADER_astEmptyStatement,
                 MOJOSHADER_AST_STATEMENT_EMPTY);
    retval->next = NULL;
    return (MOJOSHADER_astStatement *) retval;
} // new_empty_statement

static void delete_empty_statement(Context *ctx,
                                   MOJOSHADER_astEmptyStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    Free(ctx, stmt);
} // delete_empty_statement

static MOJOSHADER_astStatement *new_break_statement(Context *ctx)
{
    NEW_AST_NODE(retval, MOJOSHADER_astBreakStatement,
                 MOJOSHADER_AST_STATEMENT_BREAK);
    retval->next = NULL;
    return (MOJOSHADER_astStatement *) retval;
} // new_break_statement

static void delete_break_statement(Context *ctx,
                                   MOJOSHADER_astBreakStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    Free(ctx, stmt);
} // delete_break_statement

static MOJOSHADER_astStatement *new_continue_statement(Context *ctx)
{
    NEW_AST_NODE(retval, MOJOSHADER_astContinueStatement,
                 MOJOSHADER_AST_STATEMENT_CONTINUE);
    retval->next = NULL;
    return (MOJOSHADER_astStatement *) retval;
} // new_continue_statement

static void delete_continue_statement(Context *ctx,
                                      MOJOSHADER_astContinueStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    Free(ctx, stmt);
} // delete_continue_statement

static MOJOSHADER_astStatement *new_discard_statement(Context *ctx)
{
    NEW_AST_NODE(retval, MOJOSHADER_astDiscardStatement,
                 MOJOSHADER_AST_STATEMENT_DISCARD);
    retval->next = NULL;
    return (MOJOSHADER_astStatement *) retval;
} // new_discard_statement

static void delete_discard_statement(Context *ctx,
                                     MOJOSHADER_astDiscardStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    Free(ctx, stmt);
} // delete_discard_statement

static MOJOSHADER_astStatement *new_expr_statement(Context *ctx,
                                                MOJOSHADER_astExpression *expr)
{
    NEW_AST_NODE(retval, MOJOSHADER_astExpressionStatement,
                 MOJOSHADER_AST_STATEMENT_EXPRESSION);
    retval->next = NULL;
    retval->expr = expr;
    return (MOJOSHADER_astStatement *) retval;
} // new_expr_statement

static void delete_expr_statement(Context *ctx,
                                  MOJOSHADER_astExpressionStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_expr(ctx, stmt->expr);
    Free(ctx, stmt);
} // delete_expr_statement

static MOJOSHADER_astStatement *new_switch_statement(Context *ctx,
                                            const int attr,
                                            MOJOSHADER_astExpression *expr,
                                            MOJOSHADER_astSwitchCases *cases)
{
    NEW_AST_NODE(retval, MOJOSHADER_astSwitchStatement,
                 MOJOSHADER_AST_STATEMENT_SWITCH);
    retval->next = NULL;
    retval->attributes = attr;
    retval->expr = expr;
    retval->cases = cases;
    return (MOJOSHADER_astStatement *) retval;
} // new_switch_statement

static void delete_switch_statement(Context *ctx,
                                    MOJOSHADER_astSwitchStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_expr(ctx, stmt->expr);
    delete_switch_case(ctx, stmt->cases);
    Free(ctx, stmt);
} // delete_switch_statement

static MOJOSHADER_astStatement *new_struct_statement(Context *ctx,
                                        MOJOSHADER_astStructDeclaration *sd)
{
    NEW_AST_NODE(retval, MOJOSHADER_astStructStatement,
                 MOJOSHADER_AST_STATEMENT_STRUCT);
    retval->next = NULL;
    retval->struct_info = sd;
    return (MOJOSHADER_astStatement *) retval;
} // new_struct_statement

static void delete_struct_statement(Context *ctx,
                                    MOJOSHADER_astStructStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_struct_declaration(ctx, stmt->struct_info);
    Free(ctx, stmt);
} // delete_struct_statement

static MOJOSHADER_astStatement *new_vardecl_statement(Context *ctx,
                                        MOJOSHADER_astVariableDeclaration *vd)
{
    NEW_AST_NODE(retval, MOJOSHADER_astVarDeclStatement,
                 MOJOSHADER_AST_STATEMENT_VARDECL);
    retval->next = NULL;
    retval->declaration = vd;
    return (MOJOSHADER_astStatement *) retval;
} // new_vardecl_statement

static void delete_vardecl_statement(Context *ctx,
                                     MOJOSHADER_astVarDeclStatement *stmt)
{
    DELETE_AST_NODE(stmt);
    delete_statement(ctx, stmt->next);
    delete_variable_declaration(ctx, stmt->declaration);
    Free(ctx, stmt);
} // delete_vardecl_statement

static void delete_statement(Context *ctx, MOJOSHADER_astStatement *stmt)
{
    if (!stmt) return;

    // it's important to not recurse too deeply here, since you may have
    //  thousands of items in this linked list (each line of a massive
    //  function, for example). To avoid this, we iterate the list here,
    //  deleting all children and making them think they have no reason
    //  to recurse in their own delete methods.
    // Please note that everyone should _try_ to delete their "next" member,
    //  just in case, but hopefully this cleaned it out.

    MOJOSHADER_astStatement *i = stmt->next;
    stmt->next = NULL;
    while (i)
    {
        MOJOSHADER_astStatement *next = i->next;
        i->next = NULL;
        delete_statement(ctx, i);
        i = next;
    } // while

    switch (stmt->ast.type)
    {
        #define DELETE_STATEMENT(typ, cls, fn) \
            case MOJOSHADER_AST_STATEMENT_##typ: \
                delete_##fn##_statement(ctx, (cls *) stmt); break;
        DELETE_STATEMENT(BLOCK, MOJOSHADER_astBlockStatement, block);
        DELETE_STATEMENT(EMPTY, MOJOSHADER_astEmptyStatement, empty);
        DELETE_STATEMENT(IF, MOJOSHADER_astIfStatement, if);
        DELETE_STATEMENT(SWITCH, MOJOSHADER_astSwitchStatement, switch);
        DELETE_STATEMENT(EXPRESSION, MOJOSHADER_astExpressionStatement, expr);
        DELETE_STATEMENT(FOR, MOJOSHADER_astForStatement, for);
        DELETE_STATEMENT(DO, MOJOSHADER_astDoStatement, do);
        DELETE_STATEMENT(WHILE, MOJOSHADER_astWhileStatement, while);
        DELETE_STATEMENT(RETURN, MOJOSHADER_astReturnStatement, return);
        DELETE_STATEMENT(BREAK, MOJOSHADER_astBreakStatement, break);
        DELETE_STATEMENT(CONTINUE, MOJOSHADER_astContinueStatement, continue);
        DELETE_STATEMENT(DISCARD, MOJOSHADER_astDiscardStatement, discard);
        DELETE_STATEMENT(TYPEDEF, MOJOSHADER_astTypedefStatement, typedef);
        DELETE_STATEMENT(STRUCT, MOJOSHADER_astStructStatement, struct);
        DELETE_STATEMENT(VARDECL, MOJOSHADER_astVarDeclStatement, vardecl);
        #undef DELETE_STATEMENT
        default: assert(0 && "missing cleanup code"); break;
    } // switch
    // don't free (stmt) here, the class-specific functions do it.
} // delete_statement


static const MOJOSHADER_astDataType *get_usertype(const Context *ctx,
                                                  const char *token)
{
    const void *value;  // search all scopes.
    if (!hash_find(ctx->usertypes.hash, token, &value))
        return NULL;
    return value ? ((SymbolScope *) value)->datatype : NULL;
} // get_usertype


// This is where the actual parsing happens. It's Lemon-generated!
#define __MOJOSHADER_HLSL_COMPILER__ 1
#include "mojoshader_parser_hlsl.h"


#if 0
static int expr_is_constant(MOJOSHADER_astExpression *expr)
{
    const MOJOSHADER_astNodeType op = expr->ast.type;
    if (operator_is_unary(op))
        return expr_is_constant(expr->unary.operand);
    else if (operator_is_binary(op))
    {
        return ( expr_is_constant(expr->binary.left) &&
                 expr_is_constant(expr->binary.right) );
    } // else if
    else if (operator_is_ternary(op))
    {
        return ( expr_is_constant(expr->ternary.left) &&
                 expr_is_constant(expr->ternary.center) &&
                 expr_is_constant(expr->ternary.right) );
    } // else if

    return ( (op == MOJOSHADER_AST_OP_INT_LITERAL) ||
             (op == MOJOSHADER_AST_OP_FLOAT_LITERAL) ||
             (op == MOJOSHADER_AST_OP_STRING_LITERAL) ||
             (op == MOJOSHADER_AST_OP_BOOLEAN_LITERAL) );
} // expr_is_constant
#endif

typedef struct AstCalcData
{
    int isflt;
    union
    {
        double f;
        int64 i;
    } value;
} AstCalcData;

// returns 0 if this expression is non-constant, 1 if it is.
//  calculation results land in (data).
static int calc_ast_const_expr(Context *ctx, void *_expr, AstCalcData *data)
{
    const MOJOSHADER_astNode *expr = (MOJOSHADER_astNode *) _expr;
    const MOJOSHADER_astNodeType op = expr->ast.type;

    ctx->sourcefile = expr->ast.filename;
    ctx->sourceline = expr->ast.line;

    if (operator_is_unary(op))
    {
        if (!calc_ast_const_expr(ctx, expr->unary.operand, data))
            return 0;

        if (data->isflt)
        {
            switch (op)
            {
                case MOJOSHADER_AST_OP_NEGATE:
                    data->value.f = -data->value.f;
                    return 1;
                case MOJOSHADER_AST_OP_NOT:
                    data->value.f = !data->value.f;
                    return 1;
                case MOJOSHADER_AST_OP_COMPLEMENT:
                    fail(ctx, "integer operation on floating point value");
                    return 0;
                case MOJOSHADER_AST_OP_CAST:
                    // !!! FIXME: this should work, but it's complicated.
                    assert(0 && "write me");
                    return 0;
                default: break;
            } // switch
        } // if

        else  // integer version
        {
            switch (op)
            {
                case MOJOSHADER_AST_OP_NEGATE:
                    data->value.i = -data->value.i;
                    return 1;
                case MOJOSHADER_AST_OP_NOT:
                    data->value.i = !data->value.i;
                    return 1;
                case MOJOSHADER_AST_OP_COMPLEMENT:
                    data->value.i = ~data->value.i;
                    return 1;
                case MOJOSHADER_AST_OP_CAST:
                    // !!! FIXME: this should work, but it's complicated.
                    assert(0 && "write me");
                    return 0;
                default: break;
            } // switch
        } // else
        assert(0 && "unhandled operation?");
        return 0;
    } // if

    else if (operator_is_binary(op))
    {
        AstCalcData subdata2;
        if ( (!calc_ast_const_expr(ctx, expr->binary.left, data)) ||
             (!calc_ast_const_expr(ctx, expr->binary.right, &subdata2)) )
            return 0;

        // upgrade to float if either operand is float.
        if ((data->isflt) || (subdata2.isflt))
        {
            if (!data->isflt) data->value.f = (double) data->value.i;
            if (!subdata2.isflt) subdata2.value.f = (double) subdata2.value.i;
            data->isflt = subdata2.isflt = 1;
        } // if

        switch (op)
        {
            // gcc doesn't handle commas here, either (fails to parse!).
            case MOJOSHADER_AST_OP_COMMA:
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
                return 0;  // assignment is non-constant.
            default: break;
        } // switch

        if (data->isflt)
        {
            switch (op)
            {
                case MOJOSHADER_AST_OP_MULTIPLY:
                    data->value.f *= subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_DIVIDE:
                    data->value.f /= subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_ADD:
                    data->value.f += subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_SUBTRACT:
                    data->value.f -= subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_LESSTHAN:
                    data->isflt = 0;
                    data->value.i = data->value.f < subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_GREATERTHAN:
                    data->isflt = 0;
                    data->value.i = data->value.f > subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_LESSTHANOREQUAL:
                    data->isflt = 0;
                    data->value.i = data->value.f <= subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_GREATERTHANOREQUAL:
                    data->isflt = 0;
                    data->value.i = data->value.f >= subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_EQUAL:
                    data->isflt = 0;
                    data->value.i = data->value.f == subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_NOTEQUAL:
                    data->isflt = 0;
                    data->value.i = data->value.f != subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_LOGICALAND:
                    data->isflt = 0;
                    data->value.i = data->value.f && subdata2.value.f;
                    return 1;
                case MOJOSHADER_AST_OP_LOGICALOR:
                    data->isflt = 0;
                    data->value.i = data->value.f || subdata2.value.f;
                    return 1;

                case MOJOSHADER_AST_OP_LSHIFT:
                case MOJOSHADER_AST_OP_RSHIFT:
                case MOJOSHADER_AST_OP_MODULO:
                case MOJOSHADER_AST_OP_BINARYAND:
                case MOJOSHADER_AST_OP_BINARYXOR:
                case MOJOSHADER_AST_OP_BINARYOR:
                    fail(ctx, "integer operation on floating point value");
                    return 0;
                default: break;
            } // switch
        } // if

        else   // integer version.
        {
            switch (op)
            {
                case MOJOSHADER_AST_OP_MULTIPLY:
                    data->value.i *= subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_DIVIDE:
                    data->value.i /= subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_ADD:
                    data->value.i += subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_SUBTRACT:
                    data->value.i -= subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_LESSTHAN:
                    data->value.i = data->value.i < subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_GREATERTHAN:
                    data->value.i = data->value.i > subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_LESSTHANOREQUAL:
                    data->value.i = data->value.i <= subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_GREATERTHANOREQUAL:
                    data->value.i = data->value.i >= subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_EQUAL:
                    data->value.i = data->value.i == subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_NOTEQUAL:
                    data->value.i = data->value.i != subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_LOGICALAND:
                    data->value.i = data->value.i && subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_LOGICALOR:
                    data->value.i = data->value.i || subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_LSHIFT:
                    data->value.i = data->value.i << subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_RSHIFT:
                    data->value.i = data->value.i >> subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_MODULO:
                    data->value.i = data->value.i % subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_BINARYAND:
                    data->value.i = data->value.i & subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_BINARYXOR:
                    data->value.i = data->value.i ^ subdata2.value.i;
                    return 1;
                case MOJOSHADER_AST_OP_BINARYOR:
                    data->value.i = data->value.i | subdata2.value.i;
                    return 1;
                default: break;
            } // switch
        } // else

        assert(0 && "unhandled operation?");
        return 0;
    } // else if

    else if (operator_is_ternary(op))
    {
        AstCalcData subdata2;
        AstCalcData subdata3;

        assert(op == MOJOSHADER_AST_OP_CONDITIONAL);  // only one we have.

        if ( (!calc_ast_const_expr(ctx, expr->ternary.left, data)) ||
             (!calc_ast_const_expr(ctx, expr->ternary.center, &subdata2)) ||
             (!calc_ast_const_expr(ctx, expr->ternary.right, &subdata3)) )
            return 0;

        // first operand should be bool (for the one ternary operator we have).
        if (data->isflt)
        {
            data->isflt = 0;
            data->value.i = (int64) subdata3.value.f;
        } // if

        // upgrade to float if either operand is float.
        if ((subdata2.isflt) || (subdata3.isflt))
        {
            if (!subdata2.isflt) subdata2.value.f = (double) subdata2.value.i;
            if (!subdata3.isflt) subdata3.value.f = (double) subdata3.value.i;
            subdata2.isflt = subdata3.isflt = 1;
        } // if

        data->isflt = subdata2.isflt;
        if (data->isflt)
            data->value.f = data->value.i ? subdata2.value.f : subdata3.value.f;
        else
            data->value.i = data->value.i ? subdata2.value.i : subdata3.value.i;
        return 1;
    } // else if

    else  // not an operator? See if this is a literal value.
    {
        switch (op)
        {
            case MOJOSHADER_AST_OP_INT_LITERAL:
                data->isflt = 0;
                data->value.i = expr->intliteral.value;
                return 1;

            case MOJOSHADER_AST_OP_FLOAT_LITERAL:
                data->isflt = 1;
                data->value.f = expr->floatliteral.value;
                return 1;

            case MOJOSHADER_AST_OP_BOOLEAN_LITERAL:
                data->isflt = 0;
                data->value.i = expr->boolliteral.value ? 1 : 0;
                return 1;

            default: break;
        } // switch
    } // switch

    return 0;  // not constant, or unhandled.
} // calc_ast_const_expr


static const MOJOSHADER_astDataType *reduce_datatype(Context *ctx, const MOJOSHADER_astDataType *dt)
{
    const MOJOSHADER_astDataType *retval = dt;
    while (retval && retval->type == MOJOSHADER_AST_DATATYPE_USER)
    {
        // !!! FIXME: Ugh, const removal.
        MOJOSHADER_astDataTypeUser *user = (MOJOSHADER_astDataTypeUser *) &retval->user;
        if (user->details->type == MOJOSHADER_AST_DATATYPE_NONE)
        {
            // Take this opportunity to fix up some usertype stubs that were
            //  left over from the parse phase. You HAVE to catch these in the
            //  right scope, so be aggressive about calling reduce_datatype()
            //  as soon as things come into view!
            user->details = get_usertype(ctx, user->name);
            assert(user->details != NULL);
        } // if

        retval = user->details;
    } // while

    return retval;
} // reduce_datatype


static inline const MOJOSHADER_astDataType *sanitize_datatype(Context *ctx, const MOJOSHADER_astDataType *dt)
{
    reduce_datatype(ctx, dt);
    return dt;
} // sanitize_datatype


static const MOJOSHADER_astDataType *build_function_datatype(Context *ctx,
                                        const MOJOSHADER_astDataType *rettype,
                                        const int paramcount,
                                        const MOJOSHADER_astDataType **params,
                                        const int intrinsic)
{
    if ( ((paramcount > 0) && (params == NULL)) ||
         ((paramcount == 0) && (params != NULL)) )
        return NULL;

    // !!! FIXME: this is hacky.
    const MOJOSHADER_astDataType **dtparams;
    void *ptr = Malloc(ctx, sizeof (*params) * paramcount);
    if (ptr == NULL)
        return NULL;
    if (!buffer_append(ctx->garbage, &ptr, sizeof (ptr)))
    {
        Free(ctx, ptr);
        return NULL;
    } // if
    dtparams = (const MOJOSHADER_astDataType **) ptr;
    memcpy(dtparams, params, sizeof (*params) * paramcount);

    ptr = Malloc(ctx, sizeof (MOJOSHADER_astDataType));
    if (ptr == NULL)
        return NULL;
    if (!buffer_append(ctx->garbage, &ptr, sizeof (ptr)))
    {
        Free(ctx, ptr);
        return NULL;
    } // if

    MOJOSHADER_astDataType *dt = (MOJOSHADER_astDataType *) ptr;
    dt->type = MOJOSHADER_AST_DATATYPE_FUNCTION;
    dt->function.retval = rettype;
    dt->function.params = dtparams;
    dt->function.num_params = paramcount;
    dt->function.intrinsic = intrinsic;
    return dt;
} // build_function_datatype


static const MOJOSHADER_astDataType *build_datatype(Context *ctx,
                                            const int isconst,
                                            const MOJOSHADER_astDataType *dt,
                                            MOJOSHADER_astScalarOrArray *soa)
{
    MOJOSHADER_astDataType *retval = NULL;

    assert( (soa->isarray && soa->dimension) ||
            (!soa->isarray && !soa->dimension) );

    sanitize_datatype(ctx, dt);

    // see if we can just reuse the exist datatype.
    if (!soa->isarray)
    {
        const int c1 = (dt->type & MOJOSHADER_AST_DATATYPE_CONST) != 0;
        const int c2 = (isconst != 0);
        if (c1 == c2)
            return dt;  // reuse existing datatype!
    } // if

    retval = (MOJOSHADER_astDataType *) Malloc(ctx, sizeof (*retval));
    if (retval == NULL)
        return NULL;

    // !!! FIXME: this is hacky.
    if (!buffer_append(ctx->garbage, &retval, sizeof (retval)))
    {
        Free(ctx, retval);
        return NULL;
    } // if

    if (!soa->isarray)
    {
        assert(soa->dimension == NULL);
        memcpy(retval, dt, sizeof (MOJOSHADER_astDataType));
        if (isconst)
            retval->type |= MOJOSHADER_AST_DATATYPE_CONST;
        else
            retval->type &= ~MOJOSHADER_AST_DATATYPE_CONST;
        return retval;
    } // if

    retval->type = MOJOSHADER_AST_DATATYPE_ARRAY;
    retval->array.base = dt;
    if (soa->dimension == NULL)
    {
        retval->array.elements = -1;
        return retval;
    } // if

    // Run the expression to verify it's constant and produces a positive int.
    AstCalcData data;
    data.isflt = 0;
    data.value.i = 0;
    retval->array.elements = 16;  // sane default for failure.
    const int ok = calc_ast_const_expr(ctx, soa->dimension, &data);

    // reset error position.
    ctx->sourcefile = soa->ast.filename;
    ctx->sourceline = soa->ast.line;

    if (!ok)
        fail(ctx, "array dimensions not constant");
    else if (data.isflt)
        fail(ctx, "array dimensions not integer");
    else if (data.value.i < 0)
        fail(ctx, "array dimensions negative");
    else
        retval->array.elements = data.value.i;

    return retval;
} // build_datatype


static void require_numeric_datatype(Context *ctx,
                                     const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    if (datatype->type == MOJOSHADER_AST_DATATYPE_VECTOR)
        datatype = reduce_datatype(ctx, datatype->vector.base);
    else if (datatype->type == MOJOSHADER_AST_DATATYPE_MATRIX)
        datatype = reduce_datatype(ctx, datatype->matrix.base);

    switch (datatype->type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL:
        case MOJOSHADER_AST_DATATYPE_INT:
        case MOJOSHADER_AST_DATATYPE_UINT:
        case MOJOSHADER_AST_DATATYPE_HALF:
        case MOJOSHADER_AST_DATATYPE_FLOAT:
        case MOJOSHADER_AST_DATATYPE_DOUBLE:
            return;
        default: break;
    } // switch

    fail(ctx, "Expected numeric type");  // !!! FIXME: fmt.
    // !!! FIXME: replace AST node with an AST_OP_INT_LITERAL zero, keep going.
} // require_numeric_datatype

static void require_integer_datatype(Context *ctx,
                                     const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    switch (datatype->type)
    {
        case MOJOSHADER_AST_DATATYPE_INT:
        case MOJOSHADER_AST_DATATYPE_UINT:
            return;
        default: break;
    } // switch

    fail(ctx, "Expected integer type");  // !!! FIXME: fmt.
    // !!! FIXME: replace AST node with an AST_OP_INT_LITERAL zero, keep going.
} // require_integer_datatype

static void require_boolean_datatype(Context *ctx,
                                     const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    switch (datatype->type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL:
        case MOJOSHADER_AST_DATATYPE_INT:
        case MOJOSHADER_AST_DATATYPE_UINT:
            return;
        default: break;
    } // switch

    fail(ctx, "Expected boolean type");  // !!! FIXME: fmt.
    // !!! FIXME: replace AST node with an AST_OP_BOOLEAN_LITERAL false, keep going.
} // require_numeric_datatype


static void require_array_datatype(Context *ctx,
                                   const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    if (datatype->type == MOJOSHADER_AST_DATATYPE_ARRAY)
        return;

    fail(ctx, "expected array");
    // !!! FIXME: delete array dereference for further processing.
} // require_array_datatype


static void require_struct_datatype(Context *ctx,
                                    const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    if (datatype->type == MOJOSHADER_AST_DATATYPE_STRUCT)
        return;

    fail(ctx, "expected struct");
    // !!! FIXME: delete struct dereference for further processing.
} // require_struct_datatype


static int require_function_datatype(Context *ctx,
                                     const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    if ((!datatype) || (datatype->type != MOJOSHADER_AST_DATATYPE_FUNCTION))
    {
        fail(ctx, "expected function");
        return 0;
    } // if

    return 1;
} // require_function_datatype


// Extract the individual element type from an array datatype.
static const MOJOSHADER_astDataType *array_element_datatype(Context *ctx,
                                        const MOJOSHADER_astDataType *datatype)
{
    datatype = reduce_datatype(ctx, datatype);
    assert(datatype->type == MOJOSHADER_AST_DATATYPE_ARRAY);
    return datatype->array.base;
} // array_element_datatype


// This tests two datatypes to see if they are compatible, and adds cast
//  operator nodes to the AST if the program was relying on implicit
//  casts between then. Will fail() if the datatypes can't be coerced
//  with a cast at all. (left) can be NULL to say that its datatype is
//  set in stone (an lvalue, for example). No other NULLs are allowed.
// Returns final datatype used once implicit casting is complete.
// The datatypes must be pointers from the string cache.
static const MOJOSHADER_astDataType *add_type_coercion(Context *ctx,
                                     MOJOSHADER_astExpression **left,
                                     const MOJOSHADER_astDataType *_ldatatype,
                                     MOJOSHADER_astExpression **right,
                                     const MOJOSHADER_astDataType *_rdatatype)
{
    // !!! FIXME: this whole function is probably naive at best.
    const MOJOSHADER_astDataType *ldatatype = reduce_datatype(ctx, _ldatatype);
    const MOJOSHADER_astDataType *rdatatype = reduce_datatype(ctx, _rdatatype);

    if (ldatatype == rdatatype)
        return ldatatype;   // they already match, so we're done.

    struct {
        const MOJOSHADER_astDataTypeType type;
        const int bits;
        const int is_unsigned;
        const int floating;
    } typeinf[] = {
        { MOJOSHADER_AST_DATATYPE_BOOL,    1, 1, 0 },
        { MOJOSHADER_AST_DATATYPE_HALF,   16, 0, 1 },
        { MOJOSHADER_AST_DATATYPE_INT,    32, 0, 0 },
        { MOJOSHADER_AST_DATATYPE_UINT,   32, 1, 0 },
        { MOJOSHADER_AST_DATATYPE_FLOAT,  32, 0, 1 },
        { MOJOSHADER_AST_DATATYPE_DOUBLE, 64, 0, 1 },
    };

    int lvector = 0;
    int lmatrix = 0;
    int l = STATICARRAYLEN(typeinf);
    if (ldatatype != NULL)
    {
        MOJOSHADER_astDataTypeType type = ldatatype->type;
        if (type == MOJOSHADER_AST_DATATYPE_VECTOR)
        {
            lvector = 1;
            type = ldatatype->vector.base->type;
        } // if
        else if (type == MOJOSHADER_AST_DATATYPE_MATRIX)
        {
            lmatrix = 1;
            type = ldatatype->matrix.base->type;
        } // if

        for (l = 0; l < STATICARRAYLEN(typeinf); l++)
        {
            if (typeinf[l].type == type)
                break;
        } // for
    } // if

    int rvector = 0;
    int rmatrix = 0;
    int r = STATICARRAYLEN(typeinf);
    if (rdatatype != NULL)
    {
        MOJOSHADER_astDataTypeType type = rdatatype->type;
        if (type == MOJOSHADER_AST_DATATYPE_VECTOR)
        {
            rvector = 1;
            type = rdatatype->vector.base->type;
        } // if
        else if (type == MOJOSHADER_AST_DATATYPE_MATRIX)
        {
            rmatrix = 1;
            type = rdatatype->matrix.base->type;
        } // if

        for (r = 0; r < STATICARRAYLEN(typeinf); r++)
        {
            if (typeinf[r].type == type)
                break;
        } // for
    } // if

    enum { CHOOSE_NEITHER, CHOOSE_LEFT, CHOOSE_RIGHT } choice = CHOOSE_NEITHER;
    if ((l < STATICARRAYLEN(typeinf)) && (r < STATICARRAYLEN(typeinf)))
    {
        if (left == NULL)
            choice = CHOOSE_LEFT;  // we need to force to the lvalue.
        else if (lmatrix && !rmatrix)
            choice = CHOOSE_LEFT;
        else if (!lmatrix && rmatrix)
            choice = CHOOSE_RIGHT;
        else if (lvector && !rvector)
            choice = CHOOSE_LEFT;
        else if (!lvector && rvector)
            choice = CHOOSE_RIGHT;
        else if (typeinf[l].bits > typeinf[r].bits)
            choice = CHOOSE_LEFT;
        else if (typeinf[l].bits < typeinf[r].bits)
            choice = CHOOSE_RIGHT;
        else if (typeinf[l].floating && !typeinf[r].floating)
            choice = CHOOSE_LEFT;
        else if (!typeinf[l].floating && typeinf[r].floating)
            choice = CHOOSE_RIGHT;
        else if (typeinf[l].is_unsigned && !typeinf[r].is_unsigned)
            choice = CHOOSE_LEFT;
        else if (!typeinf[l].is_unsigned && typeinf[r].is_unsigned)
            choice = CHOOSE_RIGHT;
    } // if

    if (choice == CHOOSE_LEFT)
    {
        *right = new_cast_expr(ctx, _ldatatype, *right);
        return _ldatatype;
    } // if
    else if (choice == CHOOSE_RIGHT)
    {
        *left = new_cast_expr(ctx, _rdatatype, *left);
        return _rdatatype;
    } // else if

    assert(choice == CHOOSE_NEITHER);
    fail(ctx, "incompatible data types");
    // Ditch original (*right), force a literal value that matches
    //  ldatatype, so further processing is normalized.
    // !!! FIXME: force (right) to match (left).
    delete_expr(ctx, *right);
    *right = new_cast_expr(ctx, _ldatatype, new_literal_int_expr(ctx, 0));
    return ldatatype;
} // add_type_coercion

static int is_swizzle_str(const char *str, const int veclen)
{
    int i;
    int is_xyzw = 0;
    int is_rgba = 0;

    assert(*str != '\0');  // can this actually happen?

    for (i = 0; i < veclen; i++, str++)
    {
        const char ch = *str;
        if (ch == '\0')
            break;
        else if ((ch == 'x') || (ch == 'y') || (ch == 'z') || (ch == 'w'))
            is_xyzw = 1;
        else if ((ch == 'r') || (ch == 'g') || (ch == 'b') || (ch == 'a'))
            is_rgba = 1;
    } // for

    if (*str != '\0')  // must be end of string here.
        return 0;  // not a swizzle.
    return ((is_rgba + is_xyzw) == 1);  // can only be one or the other.
} // is_swizzle_str

static int datatype_size(const MOJOSHADER_astDataType *dt)
{
    switch (dt->type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL: return 1;
        case MOJOSHADER_AST_DATATYPE_INT: return 4;
        case MOJOSHADER_AST_DATATYPE_UINT: return 4;
        case MOJOSHADER_AST_DATATYPE_FLOAT: return 4;
        case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM: return 4;
        case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM: return 4;
        case MOJOSHADER_AST_DATATYPE_HALF: return 2;
        case MOJOSHADER_AST_DATATYPE_DOUBLE: return 8;
            return 1;
        default:
            assert(0 && "Maybe should have used reduce_datatype()?");
            return 0;
    } // switch
} // datatype_size

static inline int is_scalar_datatype(const MOJOSHADER_astDataType *dt)
{
    switch (dt->type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL:
        case MOJOSHADER_AST_DATATYPE_INT:
        case MOJOSHADER_AST_DATATYPE_UINT:
        case MOJOSHADER_AST_DATATYPE_FLOAT:
        case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM:
        case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM:
        case MOJOSHADER_AST_DATATYPE_HALF:
        case MOJOSHADER_AST_DATATYPE_DOUBLE:
            return 1;
        default:
            return 0;
    } // switch
} // is_scalar_datatype

static inline int is_float_datatype(const MOJOSHADER_astDataType *dt)
{
    switch (dt->type)
    {
        case MOJOSHADER_AST_DATATYPE_FLOAT: return 1;
        case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM: return 1;
        case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM: return 1;
        default: return 0;
    } // switch
} // is_float_datatype

static int datatype_elems(Context *ctx, const MOJOSHADER_astDataType *dt)
{
    dt = reduce_datatype(ctx, dt);
    switch (dt->type)
    {
        case MOJOSHADER_AST_DATATYPE_VECTOR:
            return dt->vector.elements;
        case MOJOSHADER_AST_DATATYPE_MATRIX:
            return dt->matrix.rows * dt->matrix.columns;
        default:
            return 1;
    } // switch
} // datatype_elems

static const MOJOSHADER_astDataType *datatype_base(Context *ctx, const MOJOSHADER_astDataType *dt)
{
    dt = reduce_datatype(ctx, dt);
    switch (dt->type)
    {
        case MOJOSHADER_AST_DATATYPE_VECTOR:
            dt = dt->vector.base;
            break;
        case MOJOSHADER_AST_DATATYPE_MATRIX:
            dt = dt->matrix.base;
            break;
        case MOJOSHADER_AST_DATATYPE_BUFFER:
            dt = dt->buffer.base;
            break;
        case MOJOSHADER_AST_DATATYPE_ARRAY:
            dt = dt->array.base;
            break;
        default: break;
    } // switch

    return dt;
} // datatype_base

typedef enum
{
    DT_MATCH_INCOMPATIBLE,         // flatly incompatible
    DT_MATCH_COMPATIBLE_DOWNCAST,  // would have to lose precision
    DT_MATCH_COMPATIBLE_UPCAST,    // would have to gain precision
    DT_MATCH_COMPATIBLE,           // can cast to without serious change.
    DT_MATCH_PERFECT               // identical datatype.
} DatatypeMatch;

static DatatypeMatch compatible_arg_datatype(Context *ctx,
                                   const MOJOSHADER_astDataType *arg,
                                   const MOJOSHADER_astDataType *param)
{
    // The matching rules for HLSL function overloading, as far as I can
    //  tell from experimenting with Microsoft's compiler, seem to be this:
    //
    // - All parameters of a function must match what the caller specified
    //   after possible type promotion via the following rules.
    // - If the number of arguments and the number of parameters don't match,
    //   that overload is immediately rejected.
    // - Each overloaded function is given a score that is the sum of the
    //   "worth" of each parameter vs the caller's arguments
    //   (see DatatypeMatch). The higher the score, the more favorable this
    //   function overload would be.
    // - If there is a tie for highest score between two or more function
    //   overloads, we declare that function call to be ambiguous and fail().
    // - Scalars can be promoted to vectors to make a parameter match.
    // - Scalars can promote to other scalars (short to int, etc).
    // - Datatypes can downcast, but should generate a warning.
    //   (calling void fn(float x); as fn((double)1.0) should warn).
    // - Vectors may NOT be extend (a float2 can't implicity extend to a
    //   float4).
    // - Vectors with the same elements can promote (a half2 can become
    //   a float2). Downcasting between vectors with the same number of
    //   elements is allowed.
    // - A perfect match of all params will be favored over any functions
    //   that only match if type promotion is applied (given a perfect match
    //   of all parameters, we'll stop looking for other matches).

    if (datatypes_match(arg, param))
        return DT_MATCH_PERFECT;  // that was easy.

    arg = reduce_datatype(ctx, arg);
    param = reduce_datatype(ctx, param);

    int do_base_test = 0;

    if (is_scalar_datatype(arg))
        do_base_test = 1; // we let these all go through for now.

    else if (arg->type == param->type)
    {
        if (arg->type == MOJOSHADER_AST_DATATYPE_VECTOR)
            do_base_test = (arg->vector.elements == param->vector.elements);
        else if (arg->type == MOJOSHADER_AST_DATATYPE_MATRIX)
        {
            do_base_test =
                ((arg->matrix.rows == param->matrix.rows) &&
                 (arg->matrix.columns == param->matrix.columns));
        } // if
    } // if

    if (do_base_test)
    {
        arg = datatype_base(ctx, arg);
        param = datatype_base(ctx, param);

        const int argsize = datatype_size(arg);
        const int paramsize = datatype_size(param);
        const int argfloat = is_float_datatype(arg);
        const int paramfloat = is_float_datatype(param);

        if (argfloat && !paramfloat)
            return DT_MATCH_COMPATIBLE_DOWNCAST;  // always loss of precision.
        else if (argfloat && !paramfloat)
        {
            if (argsize < paramsize)
                return DT_MATCH_COMPATIBLE_UPCAST;
            else
                return DT_MATCH_COMPATIBLE_DOWNCAST;  // loss of precision.
        } // else if
        else if (argsize == paramsize)
            return DT_MATCH_COMPATIBLE;
        else if (argsize < paramsize)
            return DT_MATCH_COMPATIBLE_UPCAST;
        else /* if (argsize > paramsize) */
            return DT_MATCH_COMPATIBLE_DOWNCAST;
    } // if

    return DT_MATCH_INCOMPATIBLE;
} // compatible_arg_datatype


static const MOJOSHADER_astDataType *type_check_ast(Context *ctx, void *_ast);

// !!! FIXME: this function sucks.
static const MOJOSHADER_astDataType *match_func_to_call(Context *ctx,
                                    MOJOSHADER_astExpressionCallFunction *ast)
{
    SymbolScope *best = NULL;  // best choice we find.
    int best_score = 0;
    MOJOSHADER_astExpressionIdentifier *ident = ast->identifier;
    const char *sym = ident->identifier;
    const void *value = NULL;
    void *iter = NULL;

    int argcount = 0;
    MOJOSHADER_astArguments *args = ast->args;
    while (args != NULL)
    {
        argcount++;
        type_check_ast(ctx, args->argument);
        args = args->next;
    } // while;

    // we do some tapdancing to handle function overloading here.
    int match = 0;
    while (hash_iter(ctx->variables.hash, sym, &value, &iter))
    {
        SymbolScope *item = (SymbolScope *) value;
        const MOJOSHADER_astDataType *dt = item->datatype;
        dt = reduce_datatype(ctx, dt);
        // there's a locally-scoped symbol with this name? It takes precedence.
        if (dt->type != MOJOSHADER_AST_DATATYPE_FUNCTION)
            return dt;

        const MOJOSHADER_astDataTypeFunction *dtfn = (MOJOSHADER_astDataTypeFunction *) dt;
        const int perfect = argcount * ((int) DT_MATCH_PERFECT);
        int score = 0;

        if (argcount == dtfn->num_params)  // !!! FIXME: default args.
        {
            args = ast->args;
            int i;
            for (i = 0; i < argcount; i++)
            {
                assert(args != NULL);
                dt = args->argument->datatype;
                args = args->next;
                const DatatypeMatch compatible = compatible_arg_datatype(ctx, dt, dtfn->params[i]);
                if (compatible == DT_MATCH_INCOMPATIBLE)
                {
                    args = NULL;
                    score = 0;
                    break;
                } // if

                score += (int) compatible;
            } // for

            if (args != NULL)
                score = 0;  // too many arguments supplied. No match.
        } // else

        if (score == 0)  // incompatible.
            continue;

        else if (score == perfect)  // perfection! stop looking!
        {
            match = 1;  // ignore all other compatible matches.
            best = item;
            break;
        } // if

        else if (score >= best_score)  // compatible, but not perfect, match.
        {
            if (score == best_score)
            {
                match++;
                // !!! FIXME: list each possible function in a fail(),
                // !!! FIXME:  but you can't actually fail() here, since
                // !!! FIXME:  this may cease to be ambiguous if we get
                // !!! FIXME:  a better match on a later overload.
            } // if

            else if (score > best_score)
            {
                match = 1;  // reset the ambiguousness count.
                best = item;
                best_score = score;
            } // if
        } // else if
    } // while

    if (match > 1)
    {
        assert(best != NULL);
        failf(ctx, "Ambiguous function call to '%s'", sym);
    } // if

    if (best == NULL)
    {
        assert(match == 0);
        assert(best_score == 0);
        // !!! FIXME: ident->datatype = ?
        failf(ctx, "No matching function named '%s'", sym);
    } // if
    else
    {
        ident->datatype = reduce_datatype(ctx, best->datatype);
        ident->index = best->index;
    } // else

    return ident->datatype;
} // match_func_to_call


static const MOJOSHADER_astDataType *vectype_from_base(Context *ctx,
                                            const MOJOSHADER_astDataType *base,
                                            const int len)
{
    assert(len > 0);
    assert(len <= 4);

    if (len == 1)  // return "float" and not "float1"
        return base;

    const char *typestr = NULL;
    switch (base->type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL: typestr = "bool"; break;
        case MOJOSHADER_AST_DATATYPE_INT: typestr = "int"; break;
        case MOJOSHADER_AST_DATATYPE_UINT: typestr = "uint"; break;
        case MOJOSHADER_AST_DATATYPE_HALF: typestr = "half"; break;
        case MOJOSHADER_AST_DATATYPE_FLOAT: typestr = "float"; break;
        case MOJOSHADER_AST_DATATYPE_DOUBLE: typestr = "double"; break;
        default: assert(0 && "This shouldn't happen"); break;
    } // switch

    char buf[32];
    snprintf(buf, sizeof (buf), "%s%d", typestr, len);
    const MOJOSHADER_astDataType *datatype = get_usertype(ctx, buf);
    assert(datatype != NULL);
    return datatype;
} // vectype_from_base


// Go through the AST and make sure all datatypes check out okay. For datatypes
//  that are compatible but are relying on an implicit cast, we add explicit
//  casts to the AST here, so further processing doesn't have to worry about
//  type coercion.
// For things that are incompatible, we generate errors and
//  then replace them with reasonable defaults so further processing can
//  continue (but code generation will be skipped due to errors).
// This means further processing can assume the AST is sane and not have to
//  spend effort verifying it again.
// This stage will also set every AST node's datatype field, if it is
//  meaningful to do so. This will allow conversion to IR to know what
//  type/size a given node is.
static const MOJOSHADER_astDataType *type_check_ast(Context *ctx, void *_ast)
{
    MOJOSHADER_astNode *ast = (MOJOSHADER_astNode *) _ast;
    const MOJOSHADER_astDataType *datatype = NULL;
    const MOJOSHADER_astDataType *datatype2 = NULL;
    const MOJOSHADER_astDataType *datatype3 = NULL;

    if ((!ast) || (ctx->out_of_memory))
        return NULL;

    // upkeep so we report correct error locations...
    ctx->sourcefile = ast->ast.filename;
    ctx->sourceline = ast->ast.line;

    switch (ast->ast.type)
    {
        case MOJOSHADER_AST_OP_POSTINCREMENT:
        case MOJOSHADER_AST_OP_POSTDECREMENT:
        case MOJOSHADER_AST_OP_PREINCREMENT:
        case MOJOSHADER_AST_OP_PREDECREMENT:
        case MOJOSHADER_AST_OP_COMPLEMENT:
        case MOJOSHADER_AST_OP_NEGATE:
            // !!! FIXME: must be lvalue.
            // !!! FIXME: bools must type-promote to ...int?
            // !!! FIXME: complement must not be float (...right?)
            datatype = type_check_ast(ctx, ast->unary.operand);
            require_numeric_datatype(ctx, datatype);
            ast->unary.datatype = datatype;
            return datatype;

        case MOJOSHADER_AST_OP_NOT:
            datatype = type_check_ast(ctx, ast->unary.operand);
            require_boolean_datatype(ctx, datatype);
            // !!! FIXME: coerce to bool here.
            ast->unary.datatype = &ctx->dt_bool;
            return datatype;

        case MOJOSHADER_AST_OP_DEREF_ARRAY:
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            require_integer_datatype(ctx, datatype2);
            add_type_coercion(ctx, NULL, &ctx->dt_int, &ast->binary.right, datatype2);

            datatype = reduce_datatype(ctx, datatype);
            if (datatype->type == MOJOSHADER_AST_DATATYPE_VECTOR)
            {
                // !!! FIXME: if constant int, fail if not 0 >= value <= vecsize.
                ast->binary.datatype = datatype->vector.base;
            } // if
            else if (datatype->type == MOJOSHADER_AST_DATATYPE_MATRIX)
            {
                // !!! FIXME: if constant int, fail if not 0 >= value <= rowsize (colsize?).
                ast->binary.datatype = vectype_from_base(ctx, datatype->matrix.base, datatype->matrix.columns);  // !!! FIXME: rows?
            }
            else
            {
                require_array_datatype(ctx, datatype);
                ast->binary.datatype = array_element_datatype(ctx, datatype);
            } // else

            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_DEREF_STRUCT:
        {
            const char *member = ast->derefstruct.member;
            datatype = type_check_ast(ctx, ast->derefstruct.identifier);
            const MOJOSHADER_astDataType *reduced = reduce_datatype(ctx, datatype);

            // Is this a swizzle and not a struct deref?
            if (reduced->type == MOJOSHADER_AST_DATATYPE_VECTOR)
            {
                const int veclen = reduced->vector.elements;
                ast->derefstruct.isswizzle = 1;
                if (!is_swizzle_str(member, veclen))
                {
                    fail(ctx, "invalid swizzle on vector");
                    // force this to be sane for further processing.
                    const char *sane_swiz = stringcache(ctx->strcache, "xyzw");
                    member = ast->derefstruct.member = sane_swiz;
                } // if

                const int swizlen = (int) strlen(member);
                if (swizlen != veclen)
                    datatype = vectype_from_base(ctx, reduced->vector.base, swizlen);

                ast->derefstruct.datatype = datatype;
                return ast->derefstruct.datatype;
            } // if

            // maybe this is an actual struct?
            // !!! FIXME: replace with an int or something if not.
            require_struct_datatype(ctx, reduced);

            // map member to datatype
            assert(ast->derefstruct.datatype == NULL);
            const MOJOSHADER_astDataTypeStructMember *mbrs = reduced->structure.members;
            int i;
            for (i = 0; i < reduced->structure.member_count; i++)
            {
                if (strcmp(mbrs[i].identifier, member) == 0)
                {
                    ast->derefstruct.datatype = mbrs[i].datatype;
                    ast->derefstruct.member_index = i;
                    break;
                } // if
            } // for

            if (ast->derefstruct.datatype == NULL)
            {
                // !!! FIXME: replace with an int or something.
                failf(ctx, "Struct has no member named '%s'", member);
            } // if

            return ast->derefstruct.datatype;
        } // case

        case MOJOSHADER_AST_OP_COMMA:
            // evaluate and throw away left, return right.
            type_check_ast(ctx, ast->binary.left);
            ast->binary.datatype = type_check_ast(ctx, ast->binary.right);
            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_MULTIPLY:
        case MOJOSHADER_AST_OP_DIVIDE:
        case MOJOSHADER_AST_OP_ADD:
        case MOJOSHADER_AST_OP_SUBTRACT:
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            require_numeric_datatype(ctx, datatype);
            require_numeric_datatype(ctx, datatype2);
            ast->binary.datatype = add_type_coercion(ctx, &ast->binary.left,
                                      datatype, &ast->binary.right, datatype2);
            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_LSHIFT:
        case MOJOSHADER_AST_OP_RSHIFT:
        case MOJOSHADER_AST_OP_MODULO:
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            require_integer_datatype(ctx, datatype);
            require_integer_datatype(ctx, datatype2);
            ast->binary.datatype = add_type_coercion(ctx, &ast->binary.left,
                                     datatype,  &ast->binary.right, datatype2);
            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_LESSTHAN:
        case MOJOSHADER_AST_OP_GREATERTHAN:
        case MOJOSHADER_AST_OP_LESSTHANOREQUAL:
        case MOJOSHADER_AST_OP_GREATERTHANOREQUAL:
        case MOJOSHADER_AST_OP_NOTEQUAL:
        case MOJOSHADER_AST_OP_EQUAL:
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            add_type_coercion(ctx, &ast->binary.left, datatype,
                              &ast->binary.right, datatype2);
            ast->binary.datatype = &ctx->dt_bool;
            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_BINARYAND:
        case MOJOSHADER_AST_OP_BINARYXOR:
        case MOJOSHADER_AST_OP_BINARYOR:
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            require_integer_datatype(ctx, datatype);
            require_integer_datatype(ctx, datatype2);
            ast->binary.datatype = add_type_coercion(ctx, &ast->binary.left,
                                      datatype, &ast->binary.right, datatype2);
            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_LOGICALAND:
        case MOJOSHADER_AST_OP_LOGICALOR:
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            require_boolean_datatype(ctx, datatype);
            require_boolean_datatype(ctx, datatype2);
            // !!! FIXME: coerce each to bool here, separately.
            add_type_coercion(ctx, &ast->binary.left, datatype,
                              &ast->binary.right, datatype2);
            ast->binary.datatype = &ctx->dt_bool;

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
            // !!! FIXME: verify binary.left is an lvalue, or fail()!
            datatype = type_check_ast(ctx, ast->binary.left);
            datatype2 = type_check_ast(ctx, ast->binary.right);
            ast->binary.datatype = add_type_coercion(ctx, NULL, datatype,
                                                &ast->binary.right, datatype2);
            return ast->binary.datatype;

        case MOJOSHADER_AST_OP_CONDITIONAL:
            datatype = type_check_ast(ctx, ast->ternary.left);
            datatype2 = type_check_ast(ctx, ast->ternary.center);
            datatype3 = type_check_ast(ctx, ast->ternary.right);
            require_numeric_datatype(ctx, datatype);
            ast->ternary.datatype = add_type_coercion(ctx, &ast->ternary.center,
                                    datatype2, &ast->ternary.right, datatype3);
            return ast->ternary.datatype;

        case MOJOSHADER_AST_OP_IDENTIFIER:
            datatype = find_variable(ctx, ast->identifier.identifier, &ast->identifier.index);
            if (datatype == NULL)
            {
                fail(ctx, "Unknown identifier");
                // !!! FIXME: replace with a sane default, move on.
                datatype = &ctx->dt_int;
            } // if
            ast->identifier.datatype = datatype;
            return ast->identifier.datatype;

        case MOJOSHADER_AST_OP_INT_LITERAL:
        case MOJOSHADER_AST_OP_FLOAT_LITERAL:
        case MOJOSHADER_AST_OP_STRING_LITERAL:
        case MOJOSHADER_AST_OP_BOOLEAN_LITERAL:
            assert(ast->expression.datatype != NULL);
            return ast->expression.datatype;  // already set up during parsing.

        case MOJOSHADER_AST_ARGUMENTS:
            assert(0 && "Should be done by MOJOSHADER_AST_OP_CALLFUNC/CONSTRUCTOR");
            return NULL;

        case MOJOSHADER_AST_OP_CALLFUNC:
        {
            datatype = match_func_to_call(ctx, &ast->callfunc);
            const MOJOSHADER_astDataType *reduced = reduce_datatype(ctx, datatype);
            // !!! FIXME: replace AST node with an int if this isn't a func.
            if (!require_function_datatype(ctx, reduced))
            {
                ast->callfunc.datatype = &ctx->dt_int;
                return ast->callfunc.datatype;
            } // if

            MOJOSHADER_astArguments *arg = ast->callfunc.args;
            int i;
            for (i = 0; i < reduced->function.num_params; i++)
            {
                if (arg == NULL)  // !!! FIXME: check for default parameters, fill them in.
                {
                    fail(ctx, "Too few arguments");
                    // !!! FIXME: replace AST here.
                    break;
                } // if
                datatype2 = arg->argument->datatype;  // already type-checked.
                add_type_coercion(ctx, NULL, reduced->function.params[i],
                                  &arg->argument, datatype2);
                arg = arg->next;
            } // for

            assert(arg == NULL);  // shouldn't have chosen func if too many args.

            ast->callfunc.datatype = reduced->function.retval;
            return ast->callfunc.datatype;
        } // case

        case MOJOSHADER_AST_OP_CONSTRUCTOR:
        {
            const MOJOSHADER_astDataType *reduced = reduce_datatype(ctx, ast->constructor.datatype);
            const MOJOSHADER_astDataType *base_dt = reduced;
            int num_params = 1;

            assert(reduced != NULL);
            switch (reduced->type)
            {
                case MOJOSHADER_AST_DATATYPE_VECTOR:
                    num_params = reduced->vector.elements;
                    base_dt = reduced->vector.base;
                    break;
                case MOJOSHADER_AST_DATATYPE_MATRIX:
                    num_params = reduced->matrix.rows * reduced->matrix.columns;
                    base_dt = reduced->matrix.base;
                    break;

                case MOJOSHADER_AST_DATATYPE_BOOL:
                case MOJOSHADER_AST_DATATYPE_INT:
                case MOJOSHADER_AST_DATATYPE_UINT:
                case MOJOSHADER_AST_DATATYPE_FLOAT:
                case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM:
                case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM:
                case MOJOSHADER_AST_DATATYPE_HALF:
                case MOJOSHADER_AST_DATATYPE_DOUBLE:
                case MOJOSHADER_AST_DATATYPE_STRING:
                    num_params = 1;
                    break;

                // !!! FIXME: can you construct a MOJOSHADER_AST_DATATYPE_STRUCT?
                // !!! FIXME: can you construct a MOJOSHADER_AST_DATATYPE_ARRAY?
                // !!! FIXME: can you construct a MOJOSHADER_AST_DATATYPE_BUFFER?

                default:
                    fail(ctx, "Invalid type for constructor");
                    delete_arguments(ctx, ast->constructor.args);
                    ast->constructor.args = new_argument(ctx, new_literal_int_expr(ctx, 0));
                    ast->constructor.datatype = &ctx->dt_int;
                    return ast->constructor.datatype;
            } // switch

            assert(num_params > 0);

            MOJOSHADER_astArguments *arg = ast->constructor.args;
            MOJOSHADER_astArguments *prev = NULL;
            int i;
            for (i = 0; i < num_params; i++)
            {
                if (arg == NULL)  // !!! FIXME: check for default parameters.
                {
                    fail(ctx, "Too few arguments");
                    // !!! FIXME: replace AST here.
                    break;
                } // if
                datatype2 = type_check_ast(ctx, arg->argument);

                // "float4(float3(1,2,3),4)" is legal, so we need to see if
                //  we're a vector, and jump that number of parameters instead
                //  of doing type coercion.
                reduced = reduce_datatype(ctx, datatype2);
                if (reduced->type == MOJOSHADER_AST_DATATYPE_VECTOR)
                {
                    // make sure things like float4(half3(1,2,3),1) convert that half3 to float3.
                    const int count = reduced->vector.elements;
                    datatype3 = vectype_from_base(ctx, base_dt, count);
                    add_type_coercion(ctx, NULL, datatype3, &arg->argument, datatype2);
                    i += count - 1;
                } // else
                else
                {
                    add_type_coercion(ctx, NULL, base_dt, &arg->argument, datatype2);
                } // else
                prev = arg;
                arg = arg->next;
            } // for

            if (arg != NULL)
            {
                fail(ctx, "Too many arguments");
                // Process extra arguments then chop them out.
                MOJOSHADER_astArguments *argi;
                for (argi = arg; argi != NULL; argi = argi->next)
                    type_check_ast(ctx, argi->argument);
                if (prev != NULL)
                    prev->next = NULL;
                delete_arguments(ctx, arg);
            } // if

            return ast->constructor.datatype;
        } // case

        case MOJOSHADER_AST_OP_CAST:
            datatype = sanitize_datatype(ctx, ast->cast.datatype);
            datatype2 = type_check_ast(ctx, ast->cast.operand);
            // you still need type coercion, since you could do a wrong cast,
            //  like "int x = (short) mychar;"
            add_type_coercion(ctx, NULL, datatype, &ast->cast.operand, datatype2);
            return datatype;

        case MOJOSHADER_AST_STATEMENT_BREAK:
            if ((ctx->loop_count == 0) && (ctx->switch_count == 0))
                fail(ctx, "Break outside loop or switch");
            // !!! FIXME: warn if unreachable statements follow?
            type_check_ast(ctx, ast->stmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_CONTINUE:
            if (ctx->loop_count == 0)
                fail(ctx, "Continue outside loop");
            // !!! FIXME: warn if unreachable statements follow?
            type_check_ast(ctx, ast->stmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_DISCARD:
            // !!! FIXME: warn if unreachable statements follow?
            type_check_ast(ctx, ast->stmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_EMPTY:
            type_check_ast(ctx, ast->stmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_EXPRESSION:
            // !!! FIXME: warn about expressions without a side-effect here?
            type_check_ast(ctx, ast->exprstmt.expr);  // !!! FIXME: This is named badly...
            type_check_ast(ctx, ast->exprstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_IF:
            push_scope(ctx);  // new scope for "if ((int x = blah()) != 0)"
            type_check_ast(ctx, ast->ifstmt.expr);
            type_check_ast(ctx, ast->ifstmt.statement);
            pop_scope(ctx);
            type_check_ast(ctx, ast->ifstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_TYPEDEF:
            type_check_ast(ctx, ast->typedefstmt.type_info);
            type_check_ast(ctx, ast->typedefstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_SWITCH:
        {
            ctx->switch_count++;
            MOJOSHADER_astSwitchCases *cases = ast->switchstmt.cases;
            // !!! FIXME: expr must be POD (no structs, arrays, etc!).
            datatype = type_check_ast(ctx, ast->switchstmt.expr);
            while (cases)
            {
                // !!! FIXME: case must be POD (no structs, arrays, etc!).
                datatype2 = type_check_ast(ctx, cases->expr);
                add_type_coercion(ctx, NULL, datatype,
                                  &cases->expr, datatype2);
                type_check_ast(ctx, cases->statement);
                cases = cases->next;
            } // while
            ctx->switch_count--;
            type_check_ast(ctx, ast->switchstmt.next);
            return NULL;
        } // case

        case MOJOSHADER_AST_SWITCH_CASE:
            assert(0 && "Should be done by MOJOSHADER_AST_STATEMENT_SWITCH.");
            return NULL;

        case MOJOSHADER_AST_STATEMENT_STRUCT:
            type_check_ast(ctx, ast->structstmt.struct_info);
            type_check_ast(ctx, ast->structstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_VARDECL:
            type_check_ast(ctx, ast->vardeclstmt.declaration);
            type_check_ast(ctx, ast->vardeclstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_BLOCK:
            push_scope(ctx);  // new vars declared here live until '}'.
            type_check_ast(ctx, ast->blockstmt.statements);
            pop_scope(ctx);
            type_check_ast(ctx, ast->blockstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_FOR:
            ctx->loop_count++;
            push_scope(ctx);  // new scope for "for (int x = 0; ...)"
            type_check_ast(ctx, ast->forstmt.var_decl);
            type_check_ast(ctx, ast->forstmt.initializer);
            type_check_ast(ctx, ast->forstmt.looptest);
            type_check_ast(ctx, ast->forstmt.counter);
            type_check_ast(ctx, ast->forstmt.statement);
            pop_scope(ctx);
            ctx->loop_count--;
            type_check_ast(ctx, ast->forstmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_DO:
            ctx->loop_count++;
            // !!! FIXME: should there be a push_scope() here?
            type_check_ast(ctx, ast->dostmt.statement);
            push_scope(ctx);  // new scope for "while ((int x = blah()) != 0)"
            type_check_ast(ctx, ast->dostmt.expr);
            pop_scope(ctx);
            ctx->loop_count--;
            type_check_ast(ctx, ast->dostmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_WHILE:
            ctx->loop_count++;
            push_scope(ctx);  // new scope for "while ((int x = blah()) != 0)"
            type_check_ast(ctx, ast->whilestmt.expr);
            type_check_ast(ctx, ast->whilestmt.statement);
            pop_scope(ctx);
            ctx->loop_count--;
            type_check_ast(ctx, ast->whilestmt.next);
            return NULL;

        case MOJOSHADER_AST_STATEMENT_RETURN:
            // !!! FIXME: type coercion to outer function's return type.
            // !!! FIXME: warn if unreachable statements follow?
            type_check_ast(ctx, ast->returnstmt.expr);
            type_check_ast(ctx, ast->returnstmt.next);
            return NULL;

        case MOJOSHADER_AST_COMPUNIT_FUNCTION:
            assert(!ctx->is_func_scope);

            // We have to tapdance here to make sure the function is in
            //  the global scope, but it's parameters are pushed as variables
            //  in the function's scope.
            datatype = type_check_ast(ctx, ast->funcunit.declaration);
            ast->funcunit.index = push_function(ctx,
                                ast->funcunit.declaration->identifier,
                                datatype, ast->funcunit.definition == NULL);

            // not just a declaration, but a full function definition?
            if (ast->funcunit.definition != NULL)
            {
                assert(ctx->loop_count == 0);
                assert(ctx->switch_count == 0);
                ctx->is_func_scope = 1;
                ctx->var_index = 0;  // reset this every function.
                push_scope(ctx);  // so function params are in function scope.
                // repush the parameters before checking the actual function.
                MOJOSHADER_astFunctionParameters *param;
                for (param = ast->funcunit.declaration->params; param; param = param->next)
                    push_variable(ctx, param->identifier, param->datatype);
                type_check_ast(ctx, ast->funcunit.definition);
                pop_scope(ctx);
                ctx->is_func_scope = 0;
                assert(ctx->loop_count == 0);
                assert(ctx->switch_count == 0);
            } // else

            type_check_ast(ctx, ast->funcunit.next);
            return NULL;

        case MOJOSHADER_AST_COMPUNIT_TYPEDEF:
            type_check_ast(ctx, ast->typedefunit.type_info);
            type_check_ast(ctx, ast->typedefunit.next);
            return NULL;

        case MOJOSHADER_AST_COMPUNIT_STRUCT:
            type_check_ast(ctx, ast->structunit.struct_info);
            type_check_ast(ctx, ast->structunit.next);
            return NULL;

        case MOJOSHADER_AST_COMPUNIT_VARIABLE:
            type_check_ast(ctx, ast->varunit.declaration);
            type_check_ast(ctx, ast->varunit.next);
            return NULL;

        case MOJOSHADER_AST_SCALAR_OR_ARRAY:
            assert(0 && "Should be done by other AST nodes.");
            return NULL;

        case MOJOSHADER_AST_TYPEDEF:
        {
            MOJOSHADER_astScalarOrArray *soa = ast->typdef.details;
            datatype = get_usertype(ctx, soa->identifier);
            if (datatype != NULL)
            {
                fail(ctx, "typedef already defined");
                ast->typdef.datatype = datatype;
                return datatype;
            } // if

            datatype = build_datatype(ctx, ast->typdef.isconst,
                                      ast->typdef.datatype, soa);
            if (datatype == NULL)
                return NULL;  // out of memory?

            push_usertype(ctx, soa->identifier, datatype);
            ast->typdef.datatype = datatype;
            return ast->typdef.datatype;
        } // case

        case MOJOSHADER_AST_FUNCTION_PARAMS:
            assert(0 && "Should be done by MOJOSHADER_AST_FUNCTION_SIGNATURE");

        case MOJOSHADER_AST_FUNCTION_SIGNATURE:
        {
            MOJOSHADER_astFunctionParameters *param;
            const MOJOSHADER_astDataType *dtparams[64];

            int i = 0;
            for (param = ast->funcsig.params; param; param = param->next)
            {
                assert(i <= STATICARRAYLEN(dtparams));  // laziness.
                sanitize_datatype(ctx, param->datatype);
                if (param->initializer != NULL)
                {
                    datatype2 = type_check_ast(ctx, param->initializer);
                    add_type_coercion(ctx, NULL, param->datatype,
                                      &param->initializer, datatype2);
                } // if
                dtparams[i] = param->datatype;
                i++;
            } // for

            ast->funcsig.datatype = build_function_datatype(ctx,
                                                        ast->funcsig.datatype,
                                                        i, dtparams, 0);
            return ast->funcsig.datatype;
        } // case

        case MOJOSHADER_AST_STRUCT_DECLARATION:
        {
            // !!! FIXME: We don't handle struct predeclaration at all right now
            // !!! FIXME:  (neither does the grammar)...not only does that mean
            // !!! FIXME:  you need to know the struct definition up front, but
            // !!! FIXME:  you can't do "struct XXX *next;" for a self-referencing
            // !!! FIXME:  linked list struct thing. This probably isn't a big
            // !!! FIXME:  deal, as there aren't (CURRENTLY!) pointers in HLSL,
            // !!! FIXME:  but you never know.

            const MOJOSHADER_astStructMembers *mbrs;

            // !!! FIXME: count this during parsing?
            int count = 0;
            mbrs = ast->structdecl.members;
            while (mbrs != NULL)
            {
                count++;
                mbrs = mbrs->next;
            } // while

            // !!! FIXME: this is hacky.
            MOJOSHADER_astDataTypeStructMember *dtmbrs;
            void *ptr = Malloc(ctx, sizeof (*dtmbrs) * count);
            if (ptr == NULL)
                return NULL;
            if (!buffer_append(ctx->garbage, &ptr, sizeof (ptr)))
            {
                Free(ctx, ptr);
                return NULL;
            } // if
            dtmbrs = (MOJOSHADER_astDataTypeStructMember *) ptr;

            ptr = Malloc(ctx, sizeof (MOJOSHADER_astDataType));
            if (ptr == NULL)
                return NULL;
            if (!buffer_append(ctx->garbage, &ptr, sizeof (ptr)))
            {
                Free(ctx, ptr);
                return NULL;
            } // if
            MOJOSHADER_astDataType *dt = (MOJOSHADER_astDataType *) ptr;

            mbrs = ast->structdecl.members;
            int i;
            for (i = 0; i < count; i++)
            {
                // !!! FIXME: current grammar forbids const keyword on struct members!
                dtmbrs[i].datatype = build_datatype(ctx, 0, mbrs->datatype, mbrs->details);
                dtmbrs[i].identifier = mbrs->details->identifier;  // cached!
                mbrs = mbrs->next;
            } // for

            dt->structure.type = MOJOSHADER_AST_DATATYPE_STRUCT;
            dt->structure.members = dtmbrs;
            dt->structure.member_count = count;
            ast->structdecl.datatype = dt;

            // !!! FIXME: this shouldn't push for anonymous structs: "struct { int x; } myvar;"
            // !!! FIXME:  but right now, the grammar is wrong and requires a name for the struct.
            push_usertype(ctx, ast->structdecl.name, ast->structdecl.datatype);
            return ast->structdecl.datatype;
        } // case

        case MOJOSHADER_AST_STRUCT_MEMBER:
            assert(0 && "Should be done by MOJOSHADER_AST_STRUCT_DECLARATION.");
            return NULL;

        case MOJOSHADER_AST_VARIABLE_DECLARATION:
        {
            MOJOSHADER_astVariableDeclaration *decl = &ast->vardecl;

            // this is true now, but we'll fill in ->datatype no matter what.
            assert((decl->datatype && !decl->anonymous_datatype) ||
                   (!decl->datatype && decl->anonymous_datatype));

            // An anonymous struct? That AST node does the heavy lifting.
            if (decl->anonymous_datatype != NULL)
                datatype = type_check_ast(ctx, decl->anonymous_datatype);
            else
            {
                datatype = build_datatype(ctx, (decl->attributes & MOJOSHADER_AST_VARATTR_CONST) != 0,
                                          decl->datatype, decl->details);
            } // else

            while (decl != NULL)
            {
                decl->datatype = datatype;
                push_variable(ctx, decl->details->identifier, datatype);
                if (decl->initializer != NULL)
                {
                    datatype2 = type_check_ast(ctx, decl->initializer);
                    add_type_coercion(ctx, NULL, datatype, &decl->initializer, datatype2);
                } // if

                type_check_ast(ctx, decl->annotations);
                type_check_ast(ctx, decl->lowlevel);
                decl = decl->next;
            } // while

            return datatype;
        } // case

        case MOJOSHADER_AST_ANNOTATION:
        {
            MOJOSHADER_astAnnotations *anno = &ast->annotations;
            while (anno)
            {
                type_check_ast(ctx, anno->initializer);
                anno = anno->next;
            } // while
            return NULL;
        } // case

        case MOJOSHADER_AST_PACK_OFFSET:
        case MOJOSHADER_AST_VARIABLE_LOWLEVEL:
            return NULL;  // no-op (for now, at least).

        default:
            assert(0 && "unexpected type");
    } // switch

    return NULL;
} // type_check_ast


static inline void semantic_analysis(Context *ctx)
{
    type_check_ast(ctx, ctx->ast);
} // semantic_analysis

// !!! FIXME: isn't this a cut-and-paste of somewhere else?
static inline int64 strtoi64(const char *str, unsigned int len)
{
    int64 retval = 0;
    int64 mult = 1;
    int i = 0;

    while ((len) && (*str == ' '))
    {
        str++;
        len--;
    } // while

    if ((len) && (*str == '-'))
    {
        mult = -1;
        str++;
        len--;
    } // if

    while (i < len)
    {
        const char ch = str[i];
        if ((ch < '0') || (ch > '9'))
            break;
        i++;
    } // while

    while (--i >= 0)
    {
        const char ch = str[i];
        retval += ((int64) (ch - '0')) * mult;
        mult *= 10;
    } // while

    return retval;
} // strtoi64

// !!! FIXME: isn't this a cut-and-paste of somewhere else?
static inline double strtodouble(const char *_str, unsigned int len)
{
    // !!! FIXME: laziness prevails.
    char *str = (char *) alloca(len+1);
    memcpy(str, _str, len);
    str[len] = '\0';
    return strtod(str, NULL);
} // strtodouble

#if 0
// This does not check correctness (POSITIONT993842 passes, etc).
static int is_semantic(const Context *ctx, const char *token,
                       const unsigned int tokenlen)
{
    static const char *names[] = {
        "BINORMAL", "BLENDINDICES", "BLENDWEIGHT",
        "COLOR", "NORMAL", "POSITION", "POSITIONT", "PSIZE", "TANGENT",
        "TEXCOORD", "FOG", "TESSFACTOR", "TEXCOORD", "VFACE", "VPOS",
        "DEPTH", NULL
    };

    // !!! FIXME: DX10 has SV_* ("System Value Semantics").
    const char **i;
    for (i = names; *i; i++)
    {
        const char *name = *i;
        const size_t namelen = strlen(name);
        if (tokenlen < namelen)
            continue;
        else if (memcmp(token, name, namelen) != 0)
            continue;

        for (name += namelen; *name; name++)
        {
            if ((*name < '0') || (*name > '9'))
                break;
        } // for

        if (*name == '\0')
            return 1;
    } // for

    return 0;
} // is_semantic
#endif

static int convert_to_lemon_token(Context *ctx, const char *token,
                                  unsigned int tokenlen, const Token tokenval)
{
    switch (tokenval)
    {
        case ((Token) ','): return TOKEN_HLSL_COMMA;
        case ((Token) '='): return TOKEN_HLSL_ASSIGN;
        case ((Token) TOKEN_ADDASSIGN): return TOKEN_HLSL_ADDASSIGN;
        case ((Token) TOKEN_SUBASSIGN): return TOKEN_HLSL_SUBASSIGN;
        case ((Token) TOKEN_MULTASSIGN): return TOKEN_HLSL_MULASSIGN;
        case ((Token) TOKEN_DIVASSIGN): return TOKEN_HLSL_DIVASSIGN;
        case ((Token) TOKEN_MODASSIGN): return TOKEN_HLSL_MODASSIGN;
        case ((Token) TOKEN_LSHIFTASSIGN): return TOKEN_HLSL_LSHIFTASSIGN;
        case ((Token) TOKEN_RSHIFTASSIGN): return TOKEN_HLSL_RSHIFTASSIGN;
        case ((Token) TOKEN_ANDASSIGN): return TOKEN_HLSL_ANDASSIGN;
        case ((Token) TOKEN_ORASSIGN): return TOKEN_HLSL_ORASSIGN;
        case ((Token) TOKEN_XORASSIGN): return TOKEN_HLSL_XORASSIGN;
        case ((Token) '?'): return TOKEN_HLSL_QUESTION;
        case ((Token) TOKEN_OROR): return TOKEN_HLSL_OROR;
        case ((Token) TOKEN_ANDAND): return TOKEN_HLSL_ANDAND;
        case ((Token) '|'): return TOKEN_HLSL_OR;
        case ((Token) '^'): return TOKEN_HLSL_XOR;
        case ((Token) '&'): return TOKEN_HLSL_AND;
        case ((Token) TOKEN_EQL): return TOKEN_HLSL_EQL;
        case ((Token) TOKEN_NEQ): return TOKEN_HLSL_NEQ;
        case ((Token) '<'): return TOKEN_HLSL_LT;
        case ((Token) TOKEN_LEQ): return TOKEN_HLSL_LEQ;
        case ((Token) '>'): return TOKEN_HLSL_GT;
        case ((Token) TOKEN_GEQ): return TOKEN_HLSL_GEQ;
        case ((Token) TOKEN_LSHIFT): return TOKEN_HLSL_LSHIFT;
        case ((Token) TOKEN_RSHIFT): return TOKEN_HLSL_RSHIFT;
        case ((Token) '+'): return TOKEN_HLSL_PLUS;
        case ((Token) '-'): return TOKEN_HLSL_MINUS;
        case ((Token) '*'): return TOKEN_HLSL_STAR;
        case ((Token) '/'): return TOKEN_HLSL_SLASH;
        case ((Token) '%'): return TOKEN_HLSL_PERCENT;
        case ((Token) '!'): return TOKEN_HLSL_EXCLAMATION;
        case ((Token) '~'): return TOKEN_HLSL_COMPLEMENT;
        case ((Token) TOKEN_DECREMENT): return TOKEN_HLSL_MINUSMINUS;
        case ((Token) TOKEN_INCREMENT): return TOKEN_HLSL_PLUSPLUS;
        case ((Token) '.'): return TOKEN_HLSL_DOT;
        case ((Token) '['): return TOKEN_HLSL_LBRACKET;
        case ((Token) ']'): return TOKEN_HLSL_RBRACKET;
        case ((Token) '('): return TOKEN_HLSL_LPAREN;
        case ((Token) ')'): return TOKEN_HLSL_RPAREN;
        case ((Token) TOKEN_INT_LITERAL): return TOKEN_HLSL_INT_CONSTANT;
        case ((Token) TOKEN_FLOAT_LITERAL): return TOKEN_HLSL_FLOAT_CONSTANT;
        case ((Token) TOKEN_STRING_LITERAL): return TOKEN_HLSL_STRING_LITERAL;
        case ((Token) ':'): return TOKEN_HLSL_COLON;
        case ((Token) ';'): return TOKEN_HLSL_SEMICOLON;
        case ((Token) '{'): return TOKEN_HLSL_LBRACE;
        case ((Token) '}'): return TOKEN_HLSL_RBRACE;
        //case ((Token) TOKEN_PP_PRAGMA): return TOKEN_HLSL_PRAGMA;
        //case ((Token) '\n'): return TOKEN_HLSL_NEWLINE;

        case ((Token) TOKEN_IDENTIFIER):
            #define tokencmp(t) ((tokenlen == strlen(t)) && (memcmp(token, t, tokenlen) == 0))
            //case ((Token) ''): return TOKEN_HLSL_TYPECAST
            //if (tokencmp("")) return TOKEN_HLSL_TYPE_NAME
            //if (tokencmp("...")) return TOKEN_HLSL_ELIPSIS
            if (tokencmp("else")) return TOKEN_HLSL_ELSE;
            if (tokencmp("inline")) return TOKEN_HLSL_INLINE;
            if (tokencmp("void")) return TOKEN_HLSL_VOID;
            if (tokencmp("in")) return TOKEN_HLSL_IN;
            if (tokencmp("inout")) return TOKEN_HLSL_INOUT;
            if (tokencmp("out")) return TOKEN_HLSL_OUT;
            if (tokencmp("uniform")) return TOKEN_HLSL_UNIFORM;
            if (tokencmp("linear")) return TOKEN_HLSL_LINEAR;
            if (tokencmp("centroid")) return TOKEN_HLSL_CENTROID;
            if (tokencmp("nointerpolation")) return TOKEN_HLSL_NOINTERPOLATION;
            if (tokencmp("noperspective")) return TOKEN_HLSL_NOPERSPECTIVE;
            if (tokencmp("sample")) return TOKEN_HLSL_SAMPLE;
            if (tokencmp("struct")) return TOKEN_HLSL_STRUCT;
            if (tokencmp("typedef")) return TOKEN_HLSL_TYPEDEF;
            if (tokencmp("const")) return TOKEN_HLSL_CONST;
            if (tokencmp("packoffset")) return TOKEN_HLSL_PACKOFFSET;
            if (tokencmp("register")) return TOKEN_HLSL_REGISTER;
            if (tokencmp("extern")) return TOKEN_HLSL_EXTERN;
            if (tokencmp("shared")) return TOKEN_HLSL_SHARED;
            if (tokencmp("static")) return TOKEN_HLSL_STATIC;
            if (tokencmp("volatile")) return TOKEN_HLSL_VOLATILE;
            if (tokencmp("row_major")) return TOKEN_HLSL_ROWMAJOR;
            if (tokencmp("column_major")) return TOKEN_HLSL_COLUMNMAJOR;
            if (tokencmp("bool")) return TOKEN_HLSL_BOOL;
            if (tokencmp("int")) return TOKEN_HLSL_INT;
            if (tokencmp("uint")) return TOKEN_HLSL_UINT;
            if (tokencmp("half")) return TOKEN_HLSL_HALF;
            if (tokencmp("float")) return TOKEN_HLSL_FLOAT;
            if (tokencmp("double")) return TOKEN_HLSL_DOUBLE;
            if (tokencmp("string")) return TOKEN_HLSL_STRING;
            if (tokencmp("snorm")) return TOKEN_HLSL_SNORM;
            if (tokencmp("unorm")) return TOKEN_HLSL_UNORM;
            if (tokencmp("buffer")) return TOKEN_HLSL_BUFFER;
            if (tokencmp("vector")) return TOKEN_HLSL_VECTOR;
            if (tokencmp("matrix")) return TOKEN_HLSL_MATRIX;
            if (tokencmp("break")) return TOKEN_HLSL_BREAK;
            if (tokencmp("continue")) return TOKEN_HLSL_CONTINUE;
            if (tokencmp("discard")) return TOKEN_HLSL_DISCARD;
            if (tokencmp("return")) return TOKEN_HLSL_RETURN;
            if (tokencmp("while")) return TOKEN_HLSL_WHILE;
            if (tokencmp("for")) return TOKEN_HLSL_FOR;
            if (tokencmp("unroll")) return TOKEN_HLSL_UNROLL;
            if (tokencmp("loop")) return TOKEN_HLSL_LOOP;
            if (tokencmp("do")) return TOKEN_HLSL_DO;
            if (tokencmp("if")) return TOKEN_HLSL_IF;
            if (tokencmp("branch")) return TOKEN_HLSL_BRANCH;
            if (tokencmp("flatten")) return TOKEN_HLSL_FLATTEN;
            if (tokencmp("switch")) return TOKEN_HLSL_SWITCH;
            if (tokencmp("forcecase")) return TOKEN_HLSL_FORCECASE;
            if (tokencmp("call")) return TOKEN_HLSL_CALL;
            if (tokencmp("case")) return TOKEN_HLSL_CASE;
            if (tokencmp("default")) return TOKEN_HLSL_DEFAULT;
            if (tokencmp("sampler")) return TOKEN_HLSL_SAMPLER;
            if (tokencmp("sampler1D")) return TOKEN_HLSL_SAMPLER1D;
            if (tokencmp("sampler2D")) return TOKEN_HLSL_SAMPLER2D;
            if (tokencmp("sampler3D")) return TOKEN_HLSL_SAMPLER3D;
            if (tokencmp("samplerCUBE")) return TOKEN_HLSL_SAMPLERCUBE;
            if (tokencmp("sampler_state")) return TOKEN_HLSL_SAMPLER_STATE;
            if (tokencmp("SamplerState")) return TOKEN_HLSL_SAMPLERSTATE;
            if (tokencmp("true")) return TOKEN_HLSL_TRUE;
            if (tokencmp("false")) return TOKEN_HLSL_FALSE;
            if (tokencmp("SamplerComparisonState")) return TOKEN_HLSL_SAMPLERCOMPARISONSTATE;
            if (tokencmp("isolate")) return TOKEN_HLSL_ISOLATE;
            if (tokencmp("maxInstructionCount")) return TOKEN_HLSL_MAXINSTRUCTIONCOUNT;
            if (tokencmp("noExpressionOptimizations")) return TOKEN_HLSL_NOEXPRESSIONOPTIMIZATIONS;
            if (tokencmp("unused")) return TOKEN_HLSL_UNUSED;
            if (tokencmp("xps")) return TOKEN_HLSL_XPS;
            #undef tokencmp

            // get a canonical copy of the string now, as we'll need it.
            token = stringcache_len(ctx->strcache, token, tokenlen);
            if (get_usertype(ctx, token) != NULL)
                return TOKEN_HLSL_USERTYPE;
            return TOKEN_HLSL_IDENTIFIER;

        case TOKEN_EOI: return 0;
        default: assert(0 && "unexpected token from lexer\n"); return 0;
    } // switch

    return 0;
} // convert_to_lemon_token


static void delete_ir(Context *ctx, void *_ir);  // !!! FIXME: move this code around.

static void destroy_context(Context *ctx)
{
    if (ctx != NULL)
    {
        MOJOSHADER_free f = ((ctx->free != NULL) ? ctx->free : MOJOSHADER_internal_free);
        void *d = ctx->malloc_data;
        size_t i = 0;

        // !!! FIXME: this is kinda hacky.
        const size_t count = buffer_size(ctx->garbage) / sizeof (void *);
        if (count > 0)
        {
            void **garbage = (void **) buffer_flatten(ctx->garbage);
            if (garbage != NULL)
            {
                for (i = 0; i < count; i++)
                    f(garbage[i], d);
                f(garbage, d);
            } // if
        } // if
        buffer_destroy(ctx->garbage);

        delete_compilation_unit(ctx, (MOJOSHADER_astCompilationUnit*)ctx->ast);
        destroy_symbolmap(ctx, &ctx->usertypes);
        destroy_symbolmap(ctx, &ctx->variables);
        stringcache_destroy(ctx->strcache);
        errorlist_destroy(ctx->errors);
        errorlist_destroy(ctx->warnings);

        if (ctx->ir != NULL)
        {
            for (i = 0; i <= ctx->user_func_index; i++)
                delete_ir(ctx, ctx->ir[i]);
            f(ctx->ir, d);
        } // if

        // !!! FIXME: more to clean up here, now.

        f(ctx, d);
    } // if
} // destroy_context

static Context *build_context(MOJOSHADER_malloc m, MOJOSHADER_free f, void *d)
{
    if (!m) m = MOJOSHADER_internal_malloc;
    if (!f) f = MOJOSHADER_internal_free;

    Context *ctx = (Context *) m(sizeof (Context), d);
    if (ctx == NULL)
        return NULL;

    memset(ctx, '\0', sizeof (Context));
    ctx->malloc = m;
    ctx->free = f;
    ctx->malloc_data = d;
    //ctx->parse_phase = MOJOSHADER_PARSEPHASE_NOTSTARTED;
    create_symbolmap(ctx, &ctx->usertypes); // !!! FIXME: check for failure.
    create_symbolmap(ctx, &ctx->variables); // !!! FIXME: check for failure.
    ctx->strcache = stringcache_create(MallocBridge, FreeBridge, ctx);  // !!! FIXME: check for failure.
    ctx->errors = errorlist_create(MallocBridge, FreeBridge, ctx);  // !!! FIXME: check for failure.
    ctx->warnings = errorlist_create(MallocBridge, FreeBridge, ctx);  // !!! FIXME: check for failure.

    // !!! FIXME: this feels hacky.
    ctx->garbage = buffer_create(256*sizeof(void*),MallocBridge,FreeBridge,ctx);  // !!! FIXME: check for failure.

    ctx->dt_none.type = MOJOSHADER_AST_DATATYPE_NONE;
    ctx->dt_bool.type = MOJOSHADER_AST_DATATYPE_BOOL;
    ctx->dt_int.type = MOJOSHADER_AST_DATATYPE_INT;
    ctx->dt_uint.type = MOJOSHADER_AST_DATATYPE_UINT;
    ctx->dt_float.type = MOJOSHADER_AST_DATATYPE_FLOAT;
    ctx->dt_float_snorm.type = MOJOSHADER_AST_DATATYPE_FLOAT_SNORM;
    ctx->dt_float_unorm.type = MOJOSHADER_AST_DATATYPE_FLOAT_UNORM;
    ctx->dt_half.type = MOJOSHADER_AST_DATATYPE_HALF;
    ctx->dt_double.type = MOJOSHADER_AST_DATATYPE_DOUBLE;
    ctx->dt_string.type = MOJOSHADER_AST_DATATYPE_STRING;
    ctx->dt_sampler1d.type = MOJOSHADER_AST_DATATYPE_SAMPLER_1D;
    ctx->dt_sampler2d.type = MOJOSHADER_AST_DATATYPE_SAMPLER_2D;
    ctx->dt_sampler3d.type = MOJOSHADER_AST_DATATYPE_SAMPLER_3D;
    ctx->dt_samplercube.type = MOJOSHADER_AST_DATATYPE_SAMPLER_CUBE;
    ctx->dt_samplerstate.type = MOJOSHADER_AST_DATATYPE_SAMPLER_STATE;
    ctx->dt_samplercompstate.type = MOJOSHADER_AST_DATATYPE_SAMPLER_COMPARISON_STATE;

    #define INIT_DT_BUFFER(t) \
        ctx->dt_buf_##t.type = MOJOSHADER_AST_DATATYPE_BUFFER; \
        ctx->dt_buf_##t.buffer.base = &ctx->dt_##t;
    INIT_DT_BUFFER(bool);
    INIT_DT_BUFFER(int);
    INIT_DT_BUFFER(uint);
    INIT_DT_BUFFER(half);
    INIT_DT_BUFFER(float);
    INIT_DT_BUFFER(double);
    INIT_DT_BUFFER(float_snorm);
    INIT_DT_BUFFER(float_unorm);
    #undef INIT_DT_BUFFER

    return ctx;
} // build_context


// This macro salsa is kinda nasty, but it's the smallest, least error-prone
//  way I can find to do this well in C.  :/

#define ADD_INTRINSIC(fn, ret, params) do { \
    push_function(ctx, fn, \
        build_function_datatype(ctx, ret, STATICARRAYLEN(params), params, 1), \
        0); \
} while (0)

#define ADD_INTRINSIC_VECTOR(typestr, code) do { \
    const MOJOSHADER_astDataType *dt; \
    dt = get_usertype(ctx, typestr "1"); code; \
    dt = get_usertype(ctx, typestr "2"); code; \
    dt = get_usertype(ctx, typestr "3"); code; \
    dt = get_usertype(ctx, typestr "4"); code; \
} while (0)

#define ADD_INTRINSIC_VECTOR_FLOAT(code) { \
    ADD_INTRINSIC_VECTOR("float", code); \
    ADD_INTRINSIC_VECTOR("half", code); \
    ADD_INTRINSIC_VECTOR("double", code); \
}
#define ADD_INTRINSIC_VECTOR_INT(code) { \
    ADD_INTRINSIC_VECTOR("int", code); \
    ADD_INTRINSIC_VECTOR("uint", code); \
}
#define ADD_INTRINSIC_VECTOR_BOOL(code) { \
    ADD_INTRINSIC_VECTOR("bool", code); \
}

#define ADD_INTRINSIC_MATRIX(typestr, code) do { \
    const MOJOSHADER_astDataType *dt; \
    dt = get_usertype(ctx, typestr "1x1"); code; \
    dt = get_usertype(ctx, typestr "1x2"); code; \
    dt = get_usertype(ctx, typestr "1x3"); code; \
    dt = get_usertype(ctx, typestr "1x4"); code; \
    dt = get_usertype(ctx, typestr "2x1"); code; \
    dt = get_usertype(ctx, typestr "2x2"); code; \
    dt = get_usertype(ctx, typestr "2x3"); code; \
    dt = get_usertype(ctx, typestr "2x4"); code; \
    dt = get_usertype(ctx, typestr "3x1"); code; \
    dt = get_usertype(ctx, typestr "3x2"); code; \
    dt = get_usertype(ctx, typestr "3x3"); code; \
    dt = get_usertype(ctx, typestr "3x4"); code; \
    dt = get_usertype(ctx, typestr "4x1"); code; \
    dt = get_usertype(ctx, typestr "4x2"); code; \
    dt = get_usertype(ctx, typestr "4x3"); code; \
    dt = get_usertype(ctx, typestr "4x4"); code; \
} while (0)

#define ADD_INTRINSIC_MATRIX_FLOAT(code) { \
    ADD_INTRINSIC_MATRIX("float", code); \
    ADD_INTRINSIC_MATRIX("half", code); \
    ADD_INTRINSIC_MATRIX("double", code); \
}
#define ADD_INTRINSIC_MATRIX_INT(code) { \
    ADD_INTRINSIC_MATRIX("int", code); \
    ADD_INTRINSIC_MATRIX("uint", code); \
}
#define ADD_INTRINSIC_MATRIX_BOOL(code) { \
    ADD_INTRINSIC_MATRIX("bool", code); \
}

#define ADD_INTRINSIC_ANY(scalar, typestr, code) do { \
    { const MOJOSHADER_astDataType *dt = scalar; code; } \
    ADD_INTRINSIC_VECTOR(typestr, code); \
    ADD_INTRINSIC_MATRIX(typestr, code); \
} while (0)

#define ADD_INTRINSIC_ANY_FLOAT(code) do { \
    ADD_INTRINSIC_ANY(&ctx->dt_double, "double", code); \
    ADD_INTRINSIC_ANY(&ctx->dt_half, "half", code); \
    ADD_INTRINSIC_ANY(&ctx->dt_float, "float", code); \
} while (0)
#define ADD_INTRINSIC_ANY_INT(code) do { \
    ADD_INTRINSIC_ANY(&ctx->dt_uint, "uint", code); \
    ADD_INTRINSIC_ANY(&ctx->dt_int, "int", code); \
} while (0)

#define ADD_INTRINSIC_ANY_BOOL(code) ADD_INTRINSIC_ANY(&ctx->dt_bool, "bool", code)

static void add_intrinsic1(Context *ctx, const char *fn,
                           const MOJOSHADER_astDataType *ret,
                           const MOJOSHADER_astDataType *dt1)
{
    const MOJOSHADER_astDataType *params[] = { dt1 };
    ADD_INTRINSIC(fn, ret, params);
} // add_intrinsic1

static void add_intrinsic2(Context *ctx, const char *fn,
                           const MOJOSHADER_astDataType *ret,
                           const MOJOSHADER_astDataType *dt1,
                           const MOJOSHADER_astDataType *dt2)
{
    const MOJOSHADER_astDataType *params[] = { dt1, dt2 };
    ADD_INTRINSIC(fn, ret, params);
} // add_intrinsic2

static void add_intrinsic3(Context *ctx, const char *fn,
                           const MOJOSHADER_astDataType *ret,
                           const MOJOSHADER_astDataType *dt1,
                           const MOJOSHADER_astDataType *dt2,
                           const MOJOSHADER_astDataType *dt3)
{
    const MOJOSHADER_astDataType *params[] = { dt1, dt2, dt3 };
    ADD_INTRINSIC(fn, ret, params);
} // add_intrinsic3

static void add_intrinsic4(Context *ctx, const char *fn,
                           const MOJOSHADER_astDataType *ret,
                           const MOJOSHADER_astDataType *dt1,
                           const MOJOSHADER_astDataType *dt2,
                           const MOJOSHADER_astDataType *dt3,
                           const MOJOSHADER_astDataType *dt4)
{
    const MOJOSHADER_astDataType *params[] = { dt1, dt2, dt3, dt4 };
    ADD_INTRINSIC(fn, ret, params);
} // add_intrinsic4

// PLEASE NOTE that add_intrinsic*() is called AFTER the various
//  ADD_INTRINSIC_* macros, even though these look like functions that
//  should be called first. They might be called multiple times by the macro.
//  The variable "dt" is defined by the macro for use by your code.
static void add_intrinsic_SAME1_ANYf(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_FLOAT(add_intrinsic1(ctx, fn, dt, dt));
} // add_intrinsic_SAME1_ANYf

static void add_intrinsic_SAME1_ANYfi(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_INT(add_intrinsic1(ctx, fn, dt, dt));
    add_intrinsic_SAME1_ANYf(ctx, fn);
} // add_intrinsic_SAME1_ANYfi

static void add_intrinsic_BOOL_ANYf(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_FLOAT(add_intrinsic1(ctx, fn, &ctx->dt_bool, dt));
} // add_intrinsic_BOOL_ANYf

static void add_intrinsic_BOOL_ANYfib(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_BOOL(add_intrinsic1(ctx, fn, &ctx->dt_bool, dt));
    ADD_INTRINSIC_ANY_INT(add_intrinsic1(ctx, fn, &ctx->dt_bool, dt));
    add_intrinsic_BOOL_ANYf(ctx, fn);
} // add_intrinsic_BOOL_ANYfib

static void add_intrinsic_SAME1_ANYf_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_FLOAT(add_intrinsic2(ctx, fn, dt, dt, dt));
} // add_intrinsic_SAME1_ANYf_SAME1

static void add_intrinsic_SAME1_ANYfi_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_INT(add_intrinsic2(ctx, fn, dt, dt, dt));
    add_intrinsic_SAME1_ANYf_SAME1(ctx, fn);
} // add_intrinsic_SAME1_ANYfi_SAME1

static void add_intrinsic_SAME1_ANYf_SAME1_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_FLOAT(add_intrinsic3(ctx, fn, dt, dt, dt, dt));
} // add_intrinsic_SAME1_ANYf_SAME1_SAME1

static void add_intrinsic_SAME1_ANYfi_SAME1_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_INT(add_intrinsic3(ctx, fn, dt, dt, dt, dt));
    add_intrinsic_SAME1_ANYf_SAME1_SAME1(ctx, fn);
} // add_intrinsic_SAME1_ANYfi_SAME1_SAME1

static void add_intrinsic_SAME1_Mfib(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_MATRIX_BOOL(add_intrinsic1(ctx, fn, dt, dt));
    ADD_INTRINSIC_MATRIX_INT(add_intrinsic1(ctx, fn, dt, dt));
    ADD_INTRINSIC_MATRIX_FLOAT(add_intrinsic1(ctx, fn, dt, dt));
} // add_intrinsic_SAME1_Mfib

static void add_intrinsic_SAME1_Vf(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic1(ctx, fn, dt, dt));
} // add_intrinsic_SAME1_Vf

static void add_intrinsic_SAME1_Vf_SAME1_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic3(ctx, fn, dt, dt, dt, dt));
} // add_intrinsic_SAME1_Vf_SAME1_SAME1

static void add_intrinsic_SAME1_Vf_SAME1_f(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic3(ctx, fn, dt, dt, dt, dt->user.details->vector.base));
} // add_intrinsic_SAME1_Vf_SAME1_f

static void add_intrinsic_VOID_ANYf(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_FLOAT(add_intrinsic1(ctx, fn, NULL, dt));
} // add_intrinsic_VOID_ANYf

static void add_intrinsic_VOID_ANYf_SAME1_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_ANY_FLOAT(add_intrinsic3(ctx, fn, NULL, dt, dt, dt));
} // add_intrinsic_VOID_ANYf_SAME1_SAME1

static void add_intrinsic_f_SQUAREMATRIXf(Context *ctx, const char *fn)
{
    add_intrinsic1(ctx, fn, &ctx->dt_float, get_usertype(ctx, "float1x1"));
    add_intrinsic1(ctx, fn, &ctx->dt_float, get_usertype(ctx, "float2x2"));
    add_intrinsic1(ctx, fn, &ctx->dt_float, get_usertype(ctx, "float3x3"));
    add_intrinsic1(ctx, fn, &ctx->dt_float, get_usertype(ctx, "float4x4"));
} // add_intrinsic_f_SQUAREMATRIXf

static void add_intrinsic_f_Vf(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic1(ctx, fn, dt->user.details->vector.base, dt));
} // add_intrinsic_f_Vf

static void add_intrinsic_fi_Vfi_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_VECTOR_INT(add_intrinsic2(ctx, fn, dt->user.details->vector.base, dt, dt));
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic2(ctx, fn, dt->user.details->vector.base, dt, dt));
} // add_intrinsic_fi_Vfi_SAME1

static void add_intrinsic_f_Vf_SAME1(Context *ctx, const char *fn)
{
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic2(ctx, fn, dt->user.details->vector.base, dt, dt));
} // add_intrinsic_f_Vf_SAME1

static void add_intrinsic_3f_3f_3f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *dt = get_usertype(ctx, "float3");
    add_intrinsic2(ctx, fn, dt, dt, dt);
} // add_intrinsic_3f_3f_3f

static void add_intrinsic_4f_f_f_f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f = &ctx->dt_float;
    add_intrinsic3(ctx, fn, f4, f, f, f);
} // add_intrinsic_4f_f_f_f

static void add_intrinsic_4f_s1_4f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *dt = get_usertype(ctx, "float4");
    add_intrinsic2(ctx, fn, dt, &ctx->dt_sampler1d, dt);
} // add_intrinsic_4f_s1_4f

static void add_intrinsic_4f_s1_f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *dt = get_usertype(ctx, "float4");
    add_intrinsic2(ctx, fn, dt, &ctx->dt_sampler1d, &ctx->dt_float);
} // add_intrinsic_4f_s1_f

static void add_intrinsic_4f_s1_f_f_f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *dt = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f = &ctx->dt_float;
    add_intrinsic4(ctx, fn, dt, &ctx->dt_sampler1d, f, f, f);
} // add_intrinsic_4f_s1_f_f_f

static void add_intrinsic_4f_s2_2f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f2 = get_usertype(ctx, "float2");
    add_intrinsic2(ctx, fn, f4, &ctx->dt_sampler2d, f2);
} // add_intrinsic_4f_s2_2f

static void add_intrinsic_4f_s2_2f_2f_2f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f2 = get_usertype(ctx, "float2");
    add_intrinsic4(ctx, fn, f4, &ctx->dt_sampler2d, f2, f2, f2);
} // add_intrinsic_4f_s2_2f_2f_2f

static void add_intrinsic_4f_s2_4f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    add_intrinsic2(ctx, fn, f4, &ctx->dt_sampler2d, f4);
} // add_intrinsic_4f_s2_4f

static void add_intrinsic_4f_s3_3f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f3 = get_usertype(ctx, "float3");
    add_intrinsic2(ctx, fn, f4, &ctx->dt_sampler3d, f3);
} // add_intrinsic_4f_s3_3f

static void add_intrinsic_4f_s3_3f_3f_3f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f3 = get_usertype(ctx, "float3");
    add_intrinsic4(ctx, fn, f4, &ctx->dt_sampler3d, f3, f3, f3);
} // add_intrinsic_4f_s3_3f_3f_3f

static void add_intrinsic_4f_s3_4f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    add_intrinsic2(ctx, fn, f4, &ctx->dt_sampler3d, f4);
} // add_intrinsic_4f_s3_4f

static void add_intrinsic_4f_sc_3f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f3 = get_usertype(ctx, "float3");
    add_intrinsic2(ctx, fn, f4, &ctx->dt_samplercube, f3);
} // add_intrinsic_4f_sc_3f

static void add_intrinsic_4f_sc_3f_3f_3f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *f3 = get_usertype(ctx, "float3");
    add_intrinsic4(ctx, fn, f4, &ctx->dt_samplercube, f3, f3, f3);
} // add_intrinsic_4f_sc_3f_3f_3f

static void add_intrinsic_4f_sc_4f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    add_intrinsic2(ctx, fn, f4, &ctx->dt_samplercube, f4);
} // add_intrinsic_4f_sc_4f

static void add_intrinsic_4i_4f(Context *ctx, const char *fn)
{
    const MOJOSHADER_astDataType *i4 = get_usertype(ctx, "int4");
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    add_intrinsic1(ctx, fn, i4, f4);
} // add_intrinsic_4i_4f

static void add_intrinsic_mul(Context *ctx, const char *fn)
{
    // mul() is nasty, since there's a bunch of overloads that aren't just
    //  related to vector size.
    // !!! FIXME: needs half, double, uint...
    const MOJOSHADER_astDataType *dtf = &ctx->dt_float;
    const MOJOSHADER_astDataType *dti = &ctx->dt_int;
    const MOJOSHADER_astDataType *f1 = get_usertype(ctx, "float1");
    const MOJOSHADER_astDataType *f2 = get_usertype(ctx, "float2");
    const MOJOSHADER_astDataType *f3 = get_usertype(ctx, "float3");
    const MOJOSHADER_astDataType *f4 = get_usertype(ctx, "float4");
    const MOJOSHADER_astDataType *i1 = get_usertype(ctx, "int1");
    const MOJOSHADER_astDataType *i2 = get_usertype(ctx, "int2");
    const MOJOSHADER_astDataType *i3 = get_usertype(ctx, "int3");
    const MOJOSHADER_astDataType *i4 = get_usertype(ctx, "int4");
    const MOJOSHADER_astDataType *f1x1 = get_usertype(ctx, "float1x1");
    const MOJOSHADER_astDataType *f1x2 = get_usertype(ctx, "float1x2");
    const MOJOSHADER_astDataType *f1x3 = get_usertype(ctx, "float1x3");
    const MOJOSHADER_astDataType *f1x4 = get_usertype(ctx, "float1x4");
    const MOJOSHADER_astDataType *f2x1 = get_usertype(ctx, "float2x1");
    const MOJOSHADER_astDataType *f2x2 = get_usertype(ctx, "float2x2");
    const MOJOSHADER_astDataType *f2x3 = get_usertype(ctx, "float2x3");
    const MOJOSHADER_astDataType *f2x4 = get_usertype(ctx, "float2x4");
    const MOJOSHADER_astDataType *f3x1 = get_usertype(ctx, "float3x1");
    const MOJOSHADER_astDataType *f3x2 = get_usertype(ctx, "float3x2");
    const MOJOSHADER_astDataType *f3x3 = get_usertype(ctx, "float3x3");
    const MOJOSHADER_astDataType *f3x4 = get_usertype(ctx, "float3x4");
    const MOJOSHADER_astDataType *f4x1 = get_usertype(ctx, "float4x1");
    const MOJOSHADER_astDataType *f4x2 = get_usertype(ctx, "float4x2");
    const MOJOSHADER_astDataType *f4x3 = get_usertype(ctx, "float4x3");
    const MOJOSHADER_astDataType *f4x4 = get_usertype(ctx, "float4x4");
    const MOJOSHADER_astDataType *i1x1 = get_usertype(ctx, "int1x1");
    const MOJOSHADER_astDataType *i1x2 = get_usertype(ctx, "int1x2");
    const MOJOSHADER_astDataType *i1x3 = get_usertype(ctx, "int1x3");
    const MOJOSHADER_astDataType *i1x4 = get_usertype(ctx, "int1x4");
    const MOJOSHADER_astDataType *i2x1 = get_usertype(ctx, "int2x1");
    const MOJOSHADER_astDataType *i2x2 = get_usertype(ctx, "int2x2");
    const MOJOSHADER_astDataType *i2x3 = get_usertype(ctx, "int2x3");
    const MOJOSHADER_astDataType *i2x4 = get_usertype(ctx, "int2x4");
    const MOJOSHADER_astDataType *i3x1 = get_usertype(ctx, "int3x1");
    const MOJOSHADER_astDataType *i3x2 = get_usertype(ctx, "int3x2");
    const MOJOSHADER_astDataType *i3x3 = get_usertype(ctx, "int3x3");
    const MOJOSHADER_astDataType *i3x4 = get_usertype(ctx, "int3x4");
    const MOJOSHADER_astDataType *i4x1 = get_usertype(ctx, "int4x1");
    const MOJOSHADER_astDataType *i4x2 = get_usertype(ctx, "int4x2");
    const MOJOSHADER_astDataType *i4x3 = get_usertype(ctx, "int4x3");
    const MOJOSHADER_astDataType *i4x4 = get_usertype(ctx, "int4x4");

    // scalar * scalar
    add_intrinsic2(ctx, fn, dti, dti, dti);
    add_intrinsic2(ctx, fn, dtf, dtf, dtf);

    // scalar * vector
    ADD_INTRINSIC_VECTOR_INT(add_intrinsic2(ctx, fn, dt, dti, dt));
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic2(ctx, fn, dt, dtf, dt));

    // scalar * matrix
    ADD_INTRINSIC_MATRIX_INT(add_intrinsic2(ctx, fn, dt, dti, dt));
    ADD_INTRINSIC_MATRIX_FLOAT(add_intrinsic2(ctx, fn, dt, dtf, dt));

    // vector * scalar
    ADD_INTRINSIC_VECTOR_INT(add_intrinsic2(ctx, fn, dt, dt, dti));
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic2(ctx, fn, dt, dt, dtf));

    // vector * vector
    ADD_INTRINSIC_VECTOR_INT(add_intrinsic2(ctx, fn, dti, dt, dt));
    ADD_INTRINSIC_VECTOR_FLOAT(add_intrinsic2(ctx, fn, dtf, dt, dt));

    // vector * matrix
    add_intrinsic2(ctx, fn, i1, i1, i1x1);
    add_intrinsic2(ctx, fn, i2, i1, i1x2);
    add_intrinsic2(ctx, fn, i3, i1, i1x3);
    add_intrinsic2(ctx, fn, i4, i1, i1x4);
    add_intrinsic2(ctx, fn, i1, i2, i2x1);
    add_intrinsic2(ctx, fn, i2, i2, i2x2);
    add_intrinsic2(ctx, fn, i3, i2, i2x3);
    add_intrinsic2(ctx, fn, i4, i2, i2x4);
    add_intrinsic2(ctx, fn, i1, i3, i3x1);
    add_intrinsic2(ctx, fn, i2, i3, i3x2);
    add_intrinsic2(ctx, fn, i3, i3, i3x3);
    add_intrinsic2(ctx, fn, i4, i3, i3x4);
    add_intrinsic2(ctx, fn, i1, i4, i4x1);
    add_intrinsic2(ctx, fn, i2, i4, i4x2);
    add_intrinsic2(ctx, fn, i3, i4, i4x3);
    add_intrinsic2(ctx, fn, i4, i4, i4x4);
    add_intrinsic2(ctx, fn, f1, f1, f1x1);
    add_intrinsic2(ctx, fn, f2, f1, f1x2);
    add_intrinsic2(ctx, fn, f3, f1, f1x3);
    add_intrinsic2(ctx, fn, f4, f1, f1x4);
    add_intrinsic2(ctx, fn, f1, f2, f2x1);
    add_intrinsic2(ctx, fn, f2, f2, f2x2);
    add_intrinsic2(ctx, fn, f3, f2, f2x3);
    add_intrinsic2(ctx, fn, f4, f2, f2x4);
    add_intrinsic2(ctx, fn, f1, f3, f3x1);
    add_intrinsic2(ctx, fn, f2, f3, f3x2);
    add_intrinsic2(ctx, fn, f3, f3, f3x3);
    add_intrinsic2(ctx, fn, f4, f3, f3x4);
    add_intrinsic2(ctx, fn, f1, f4, f4x1);
    add_intrinsic2(ctx, fn, f2, f4, f4x2);
    add_intrinsic2(ctx, fn, f3, f4, f4x3);
    add_intrinsic2(ctx, fn, f4, f4, f4x4);

    // matrix * scalar
    ADD_INTRINSIC_MATRIX_INT(add_intrinsic2(ctx, fn, dt, dt, dti));
    ADD_INTRINSIC_MATRIX_FLOAT(add_intrinsic2(ctx, fn, dt, dt, dtf));

    // matrix * vector
    add_intrinsic2(ctx, fn, i1, i1x1, i1);
    add_intrinsic2(ctx, fn, i1, i1x2, i2);
    add_intrinsic2(ctx, fn, i1, i1x3, i3);
    add_intrinsic2(ctx, fn, i1, i1x4, i4);
    add_intrinsic2(ctx, fn, i2, i2x1, i1);
    add_intrinsic2(ctx, fn, i2, i2x2, i2);
    add_intrinsic2(ctx, fn, i2, i2x3, i3);
    add_intrinsic2(ctx, fn, i2, i2x4, i4);
    add_intrinsic2(ctx, fn, i3, i3x1, i1);
    add_intrinsic2(ctx, fn, i3, i3x2, i2);
    add_intrinsic2(ctx, fn, i3, i3x3, i3);
    add_intrinsic2(ctx, fn, i3, i3x4, i4);
    add_intrinsic2(ctx, fn, i4, i4x1, i1);
    add_intrinsic2(ctx, fn, i4, i4x2, i2);
    add_intrinsic2(ctx, fn, i4, i4x3, i3);
    add_intrinsic2(ctx, fn, i4, i4x4, i4);
    add_intrinsic2(ctx, fn, f1, f1x1, f1);
    add_intrinsic2(ctx, fn, f1, f1x2, f2);
    add_intrinsic2(ctx, fn, f1, f1x3, f3);
    add_intrinsic2(ctx, fn, f1, f1x4, f4);
    add_intrinsic2(ctx, fn, f2, f2x1, f1);
    add_intrinsic2(ctx, fn, f2, f2x2, f2);
    add_intrinsic2(ctx, fn, f2, f2x3, f3);
    add_intrinsic2(ctx, fn, f2, f2x4, f4);
    add_intrinsic2(ctx, fn, f3, f3x1, f1);
    add_intrinsic2(ctx, fn, f3, f3x2, f2);
    add_intrinsic2(ctx, fn, f3, f3x3, f3);
    add_intrinsic2(ctx, fn, f3, f3x4, f4);
    add_intrinsic2(ctx, fn, f4, f4x1, f1);
    add_intrinsic2(ctx, fn, f4, f4x2, f2);
    add_intrinsic2(ctx, fn, f4, f4x3, f3);
    add_intrinsic2(ctx, fn, f4, f4x4, f4);

    // matrix * matrix
    add_intrinsic2(ctx, fn, i1x1, i1x1, i1x1);
    add_intrinsic2(ctx, fn, i1x2, i1x1, i1x2);
    add_intrinsic2(ctx, fn, i1x3, i1x1, i1x3);
    add_intrinsic2(ctx, fn, i1x4, i1x1, i1x4);
    add_intrinsic2(ctx, fn, i1x1, i1x2, i2x1);
    add_intrinsic2(ctx, fn, i1x2, i1x2, i2x2);
    add_intrinsic2(ctx, fn, i1x3, i1x2, i2x3);
    add_intrinsic2(ctx, fn, i1x4, i1x2, i2x4);
    add_intrinsic2(ctx, fn, i1x1, i1x3, i3x1);
    add_intrinsic2(ctx, fn, i1x2, i1x3, i3x2);
    add_intrinsic2(ctx, fn, i1x3, i1x3, i3x3);
    add_intrinsic2(ctx, fn, i1x4, i1x3, i3x4);
    add_intrinsic2(ctx, fn, i1x1, i1x4, i4x1);
    add_intrinsic2(ctx, fn, i1x2, i1x4, i4x2);
    add_intrinsic2(ctx, fn, i1x3, i1x4, i4x3);
    add_intrinsic2(ctx, fn, i1x4, i1x4, i4x4);
    add_intrinsic2(ctx, fn, i2x1, i2x1, i1x1);
    add_intrinsic2(ctx, fn, i2x2, i2x1, i1x2);
    add_intrinsic2(ctx, fn, i2x3, i2x1, i1x3);
    add_intrinsic2(ctx, fn, i2x4, i2x1, i1x4);
    add_intrinsic2(ctx, fn, i2x1, i2x2, i2x1);
    add_intrinsic2(ctx, fn, i2x2, i2x2, i2x2);
    add_intrinsic2(ctx, fn, i2x3, i2x2, i2x3);
    add_intrinsic2(ctx, fn, i2x4, i2x2, i2x4);
    add_intrinsic2(ctx, fn, i2x1, i2x3, i3x1);
    add_intrinsic2(ctx, fn, i2x2, i2x3, i3x2);
    add_intrinsic2(ctx, fn, i2x3, i2x3, i3x3);
    add_intrinsic2(ctx, fn, i2x4, i2x3, i3x4);
    add_intrinsic2(ctx, fn, i2x1, i2x4, i4x1);
    add_intrinsic2(ctx, fn, i2x2, i2x4, i4x2);
    add_intrinsic2(ctx, fn, i2x3, i2x4, i4x3);
    add_intrinsic2(ctx, fn, i2x4, i2x4, i4x4);
    add_intrinsic2(ctx, fn, i3x1, i3x1, i1x1);
    add_intrinsic2(ctx, fn, i3x2, i3x1, i1x2);
    add_intrinsic2(ctx, fn, i3x3, i3x1, i1x3);
    add_intrinsic2(ctx, fn, i3x4, i3x1, i1x4);
    add_intrinsic2(ctx, fn, i3x1, i3x2, i2x1);
    add_intrinsic2(ctx, fn, i3x2, i3x2, i2x2);
    add_intrinsic2(ctx, fn, i3x3, i3x2, i2x3);
    add_intrinsic2(ctx, fn, i3x4, i3x2, i2x4);
    add_intrinsic2(ctx, fn, i3x1, i3x3, i3x1);
    add_intrinsic2(ctx, fn, i3x2, i3x3, i3x2);
    add_intrinsic2(ctx, fn, i3x3, i3x3, i3x3);
    add_intrinsic2(ctx, fn, i3x4, i3x3, i3x4);
    add_intrinsic2(ctx, fn, i3x1, i3x4, i4x1);
    add_intrinsic2(ctx, fn, i3x2, i3x4, i4x2);
    add_intrinsic2(ctx, fn, i3x3, i3x4, i4x3);
    add_intrinsic2(ctx, fn, i3x4, i3x4, i4x4);
    add_intrinsic2(ctx, fn, i4x1, i4x1, i1x1);
    add_intrinsic2(ctx, fn, i4x2, i4x1, i1x2);
    add_intrinsic2(ctx, fn, i4x3, i4x1, i1x3);
    add_intrinsic2(ctx, fn, i4x4, i4x1, i1x4);
    add_intrinsic2(ctx, fn, i4x1, i4x2, i2x1);
    add_intrinsic2(ctx, fn, i4x2, i4x2, i2x2);
    add_intrinsic2(ctx, fn, i4x3, i4x2, i2x3);
    add_intrinsic2(ctx, fn, i4x4, i4x2, i2x4);
    add_intrinsic2(ctx, fn, i4x1, i4x3, i3x1);
    add_intrinsic2(ctx, fn, i4x2, i4x3, i3x2);
    add_intrinsic2(ctx, fn, i4x3, i4x3, i3x3);
    add_intrinsic2(ctx, fn, i4x4, i4x3, i3x4);
    add_intrinsic2(ctx, fn, i4x1, i4x4, i4x1);
    add_intrinsic2(ctx, fn, i4x2, i4x4, i4x2);
    add_intrinsic2(ctx, fn, i4x3, i4x4, i4x3);
    add_intrinsic2(ctx, fn, i4x4, i4x4, i4x4);
    add_intrinsic2(ctx, fn, f1x1, f1x1, f1x1);
    add_intrinsic2(ctx, fn, f1x2, f1x1, f1x2);
    add_intrinsic2(ctx, fn, f1x3, f1x1, f1x3);
    add_intrinsic2(ctx, fn, f1x4, f1x1, f1x4);
    add_intrinsic2(ctx, fn, f1x1, f1x2, f2x1);
    add_intrinsic2(ctx, fn, f1x2, f1x2, f2x2);
    add_intrinsic2(ctx, fn, f1x3, f1x2, f2x3);
    add_intrinsic2(ctx, fn, f1x4, f1x2, f2x4);
    add_intrinsic2(ctx, fn, f1x1, f1x3, f3x1);
    add_intrinsic2(ctx, fn, f1x2, f1x3, f3x2);
    add_intrinsic2(ctx, fn, f1x3, f1x3, f3x3);
    add_intrinsic2(ctx, fn, f1x4, f1x3, f3x4);
    add_intrinsic2(ctx, fn, f1x1, f1x4, f4x1);
    add_intrinsic2(ctx, fn, f1x2, f1x4, f4x2);
    add_intrinsic2(ctx, fn, f1x3, f1x4, f4x3);
    add_intrinsic2(ctx, fn, f1x4, f1x4, f4x4);
    add_intrinsic2(ctx, fn, f2x1, f2x1, f1x1);
    add_intrinsic2(ctx, fn, f2x2, f2x1, f1x2);
    add_intrinsic2(ctx, fn, f2x3, f2x1, f1x3);
    add_intrinsic2(ctx, fn, f2x4, f2x1, f1x4);
    add_intrinsic2(ctx, fn, f2x1, f2x2, f2x1);
    add_intrinsic2(ctx, fn, f2x2, f2x2, f2x2);
    add_intrinsic2(ctx, fn, f2x3, f2x2, f2x3);
    add_intrinsic2(ctx, fn, f2x4, f2x2, f2x4);
    add_intrinsic2(ctx, fn, f2x1, f2x3, f3x1);
    add_intrinsic2(ctx, fn, f2x2, f2x3, f3x2);
    add_intrinsic2(ctx, fn, f2x3, f2x3, f3x3);
    add_intrinsic2(ctx, fn, f2x4, f2x3, f3x4);
    add_intrinsic2(ctx, fn, f2x1, f2x4, f4x1);
    add_intrinsic2(ctx, fn, f2x2, f2x4, f4x2);
    add_intrinsic2(ctx, fn, f2x3, f2x4, f4x3);
    add_intrinsic2(ctx, fn, f2x4, f2x4, f4x4);
    add_intrinsic2(ctx, fn, f3x1, f3x1, f1x1);
    add_intrinsic2(ctx, fn, f3x2, f3x1, f1x2);
    add_intrinsic2(ctx, fn, f3x3, f3x1, f1x3);
    add_intrinsic2(ctx, fn, f3x4, f3x1, f1x4);
    add_intrinsic2(ctx, fn, f3x1, f3x2, f2x1);
    add_intrinsic2(ctx, fn, f3x2, f3x2, f2x2);
    add_intrinsic2(ctx, fn, f3x3, f3x2, f2x3);
    add_intrinsic2(ctx, fn, f3x4, f3x2, f2x4);
    add_intrinsic2(ctx, fn, f3x1, f3x3, f3x1);
    add_intrinsic2(ctx, fn, f3x2, f3x3, f3x2);
    add_intrinsic2(ctx, fn, f3x3, f3x3, f3x3);
    add_intrinsic2(ctx, fn, f3x4, f3x3, f3x4);
    add_intrinsic2(ctx, fn, f3x1, f3x4, f4x1);
    add_intrinsic2(ctx, fn, f3x2, f3x4, f4x2);
    add_intrinsic2(ctx, fn, f3x3, f3x4, f4x3);
    add_intrinsic2(ctx, fn, f3x4, f3x4, f4x4);
    add_intrinsic2(ctx, fn, f4x1, f4x1, f1x1);
    add_intrinsic2(ctx, fn, f4x2, f4x1, f1x2);
    add_intrinsic2(ctx, fn, f4x3, f4x1, f1x3);
    add_intrinsic2(ctx, fn, f4x4, f4x1, f1x4);
    add_intrinsic2(ctx, fn, f4x1, f4x2, f2x1);
    add_intrinsic2(ctx, fn, f4x2, f4x2, f2x2);
    add_intrinsic2(ctx, fn, f4x3, f4x2, f2x3);
    add_intrinsic2(ctx, fn, f4x4, f4x2, f2x4);
    add_intrinsic2(ctx, fn, f4x1, f4x3, f3x1);
    add_intrinsic2(ctx, fn, f4x2, f4x3, f3x2);
    add_intrinsic2(ctx, fn, f4x3, f4x3, f3x3);
    add_intrinsic2(ctx, fn, f4x4, f4x3, f3x4);
    add_intrinsic2(ctx, fn, f4x1, f4x4, f4x1);
    add_intrinsic2(ctx, fn, f4x2, f4x4, f4x2);
    add_intrinsic2(ctx, fn, f4x3, f4x4, f4x3);
    add_intrinsic2(ctx, fn, f4x4, f4x4, f4x4);
} // add_intrinsic_mul

static void init_builtins(Context *ctx)
{
    // add in standard typedefs...
    const struct
    {
        const char *str;
        const MOJOSHADER_astDataType *datatype;
    } types[] = {
        { "bool", &ctx->dt_bool },
        { "int", &ctx->dt_int },
        { "uint", &ctx->dt_uint },
        { "half", &ctx->dt_half },
        { "float", &ctx->dt_float },
        { "double", &ctx->dt_double },
    };

    int i, j, k;
    for (i = 0; i < STATICARRAYLEN(types); i++)
    {
        char buf[32];
        int len;
        const MOJOSHADER_astDataType *dt;

        for (j = 1; j <= 4; j++)
        {
            // "float2"
            dt = new_datatype_vector(ctx, types[i].datatype, j);
            len = snprintf(buf, sizeof (buf), "%s%d", types[i].str, j);
            push_usertype(ctx, stringcache_len(ctx->strcache, buf, len), dt);
            for (k = 1; k <= 4; k++)
            {
                // "float2x2"
                dt = new_datatype_matrix(ctx, types[i].datatype, j, k);
                len = snprintf(buf, sizeof (buf), "%s%dx%d", types[i].str,j,k);
                push_usertype(ctx, stringcache_len(ctx->strcache,buf,len), dt);
            } // for
        } // for
    } // for

    // !!! FIXME: block these out by pixel/vertex/etc shader.
    // !!! FIXME: calculate actual shader model (or maybe just let bytecode verifier throw up?).
    const int shader_model = 3;
    if (shader_model >= 1)
    {
        add_intrinsic_SAME1_ANYfi(ctx, stringcache(ctx->strcache, "abs"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "acos"));
        add_intrinsic_BOOL_ANYfib(ctx, stringcache(ctx->strcache, "all"));
        add_intrinsic_BOOL_ANYfib(ctx, stringcache(ctx->strcache, "any"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "asin"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "atan"));
        add_intrinsic_SAME1_ANYf_SAME1(ctx, stringcache(ctx->strcache, "atan2"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "ceil"));
        add_intrinsic_SAME1_ANYfi_SAME1_SAME1(ctx, stringcache(ctx->strcache, "clamp"));
        add_intrinsic_VOID_ANYf(ctx, stringcache(ctx->strcache, "clip"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "cos"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "cosh"));
        add_intrinsic_3f_3f_3f(ctx, stringcache(ctx->strcache, "cross"));
        add_intrinsic_4i_4f(ctx, stringcache(ctx->strcache, "D3DCOLORtoUBYTE4"));
        add_intrinsic_f_Vf_SAME1(ctx, stringcache(ctx->strcache, "distance"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "degrees"));
        add_intrinsic_f_SQUAREMATRIXf(ctx, stringcache(ctx->strcache, "determinant"));
        add_intrinsic_fi_Vfi_SAME1(ctx, stringcache(ctx->strcache, "dot"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "exp"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "exp2"));
        add_intrinsic_SAME1_Vf_SAME1_SAME1(ctx, stringcache(ctx->strcache, "faceforward"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "floor"));
        add_intrinsic_SAME1_ANYf_SAME1(ctx, stringcache(ctx->strcache, "fmod"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "frac"));
        add_intrinsic_BOOL_ANYf(ctx, stringcache(ctx->strcache, "isfinite"));
        add_intrinsic_BOOL_ANYf(ctx, stringcache(ctx->strcache, "isinf"));
        add_intrinsic_BOOL_ANYf(ctx, stringcache(ctx->strcache, "isnan"));
        add_intrinsic_SAME1_ANYf_SAME1(ctx, stringcache(ctx->strcache, "ldexp"));
        add_intrinsic_f_Vf(ctx, stringcache(ctx->strcache, "length"));
        add_intrinsic_SAME1_ANYf_SAME1_SAME1(ctx, stringcache(ctx->strcache, "lerp"));
        add_intrinsic_4f_f_f_f(ctx, stringcache(ctx->strcache, "lit"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "log"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "log10"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "log2"));
        add_intrinsic_SAME1_ANYfi_SAME1(ctx, stringcache(ctx->strcache, "max"));
        add_intrinsic_SAME1_ANYfi_SAME1(ctx, stringcache(ctx->strcache, "min"));
        add_intrinsic_SAME1_ANYfi_SAME1(ctx, stringcache(ctx->strcache, "modf"));  // !!! FIXME: out var?
        add_intrinsic_mul(ctx, stringcache(ctx->strcache, "mul"));
        add_intrinsic_f_Vf(ctx, stringcache(ctx->strcache, "noise"));
        add_intrinsic_SAME1_Vf(ctx, stringcache(ctx->strcache, "normalize"));
        add_intrinsic_SAME1_ANYf_SAME1(ctx, stringcache(ctx->strcache, "pow"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "radians"));
        add_intrinsic_SAME1_ANYfi_SAME1(ctx, stringcache(ctx->strcache, "reflect"));
        add_intrinsic_SAME1_Vf_SAME1_f(ctx, stringcache(ctx->strcache, "refract"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "round"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "rsqrt"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "saturate"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "sign"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "sin"));
        add_intrinsic_VOID_ANYf_SAME1_SAME1(ctx, stringcache(ctx->strcache, "sincos"));  // !!! FIXME: out var?
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "sinh"));
        add_intrinsic_SAME1_ANYf_SAME1_SAME1(ctx, stringcache(ctx->strcache, "smoothstep"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "sqrt"));
        add_intrinsic_SAME1_ANYf_SAME1(ctx, stringcache(ctx->strcache, "step"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "tan"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "tanh"));
        add_intrinsic_4f_s1_f(ctx, stringcache(ctx->strcache, "tex1D"));
        add_intrinsic_4f_s2_2f(ctx, stringcache(ctx->strcache, "tex2D"));
        add_intrinsic_4f_s3_3f(ctx, stringcache(ctx->strcache, "tex3D"));
        add_intrinsic_4f_sc_3f(ctx, stringcache(ctx->strcache, "texCUBE"));
        add_intrinsic_SAME1_Mfib(ctx, stringcache(ctx->strcache, "transpose"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "trunc"));
    } // if

    if (shader_model >= 2)
    {
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "ddx"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "ddy"));
        add_intrinsic_SAME1_ANYf_SAME1(ctx, stringcache(ctx->strcache, "frexp"));
        add_intrinsic_SAME1_ANYf(ctx, stringcache(ctx->strcache, "fwidth"));
        add_intrinsic_4f_s1_f_f_f(ctx, stringcache(ctx->strcache, "tex1D"));
        add_intrinsic_4f_s1_4f(ctx, stringcache(ctx->strcache, "tex1Dbias"));
        add_intrinsic_4f_s1_f_f_f(ctx, stringcache(ctx->strcache, "tex1Dgrad"));
        add_intrinsic_4f_s1_4f(ctx, stringcache(ctx->strcache, "tex1Dproj"));
        add_intrinsic_4f_s2_2f_2f_2f(ctx, stringcache(ctx->strcache, "tex2D"));
        add_intrinsic_4f_s2_4f(ctx, stringcache(ctx->strcache, "tex2Dbias"));
        add_intrinsic_4f_s2_2f_2f_2f(ctx, stringcache(ctx->strcache, "tex2Dgrad"));
        add_intrinsic_4f_s2_4f(ctx, stringcache(ctx->strcache, "tex2Dproj"));
        add_intrinsic_4f_s3_3f_3f_3f(ctx, stringcache(ctx->strcache, "tex3D"));
        add_intrinsic_4f_s3_4f(ctx, stringcache(ctx->strcache, "tex3Dbias"));
        add_intrinsic_4f_s3_3f_3f_3f(ctx, stringcache(ctx->strcache, "tex3Dgrad"));
        add_intrinsic_4f_s3_4f(ctx, stringcache(ctx->strcache, "tex3Dproj"));
        add_intrinsic_4f_sc_3f_3f_3f(ctx, stringcache(ctx->strcache, "texCUBE"));
        add_intrinsic_4f_sc_4f(ctx, stringcache(ctx->strcache, "texCUBEbias"));
        add_intrinsic_4f_sc_3f_3f_3f(ctx, stringcache(ctx->strcache, "texCUBEgrad"));
        add_intrinsic_4f_sc_4f(ctx, stringcache(ctx->strcache, "texCUBEproj"));
    } // if

    if (shader_model >= 3)
    {
        add_intrinsic_4f_s1_4f(ctx, stringcache(ctx->strcache, "tex1Dlod"));
        add_intrinsic_4f_s2_4f(ctx, stringcache(ctx->strcache, "tex2Dlod"));
        add_intrinsic_4f_s3_4f(ctx, stringcache(ctx->strcache, "tex3Dlod"));
        add_intrinsic_4f_sc_4f(ctx, stringcache(ctx->strcache, "texCUBElod"));
    } // if
} // init_builtins


// parse the source code into an AST.
static void parse_source(Context *ctx, const char *filename,
                         const char *source, unsigned int sourcelen,
                         const MOJOSHADER_preprocessorDefine *defines,
                         unsigned int define_count,
                         MOJOSHADER_includeOpen include_open,
                         MOJOSHADER_includeClose include_close)
{
    TokenData data;
    unsigned int tokenlen;
    Token tokenval;
    const char *token;
    int lemon_token;
    const char *fname;
    Preprocessor *pp;
    void *parser;

    if (!include_open) include_open = MOJOSHADER_internal_include_open;
    if (!include_close) include_close = MOJOSHADER_internal_include_close;

    pp = preprocessor_start(filename, source, sourcelen, include_open,
                            include_close, defines, define_count, 0,
                            MallocBridge, FreeBridge, ctx);
    if (pp == NULL)
    {
        assert(ctx->out_of_memory);  // shouldn't fail for any other reason.
        return;
    } // if

    parser = ParseHLSLAlloc(ctx->malloc, ctx->malloc_data);
    if (parser == NULL)
    {
        assert(ctx->out_of_memory);  // shouldn't fail for any other reason.
        preprocessor_end(pp);
        return;
    } // if

    // !!! FIXME: check if (parser == NULL)...

    init_builtins(ctx);

    SymbolScope *start_scope = ctx->usertypes.scope;

    #if DEBUG_COMPILER_PARSER
    ParseHLSLTrace(stdout, "COMPILER: ");
    #endif

    // Run the preprocessor/lexer/parser...
    int is_pragma = 0;   // !!! FIXME: remove this later when we can parse #pragma.
    int skipping = 0; // !!! FIXME: remove this later when we can parse #pragma.
    do {
        token = preprocessor_nexttoken(pp, &tokenlen, &tokenval);

        if (ctx->out_of_memory)
            break;

        fname = preprocessor_sourcepos(pp, &ctx->sourceline);
        ctx->sourcefile = fname ? stringcache(ctx->strcache, fname) : 0;

        if ((tokenval == TOKEN_HASH) || (tokenval == TOKEN_HASHHASH))
            tokenval = TOKEN_BAD_CHARS;

        if (tokenval == TOKEN_BAD_CHARS)
        {
            fail(ctx, "Bad characters in source file");
            continue;
        } // else if

        else if (tokenval == TOKEN_PREPROCESSING_ERROR)
        {
            fail(ctx, token);  // this happens to be null-terminated.
            continue;
        } // else if

        else if (tokenval == TOKEN_PP_PRAGMA)
        {
            assert(!is_pragma);
            is_pragma = 1;
            skipping = 1;
            continue;
        }

        else if (tokenval == ((Token) '\n'))
        {
            assert(is_pragma);
            is_pragma = 0;
            skipping = 0;
            continue;
        }

        else if (skipping)
        {
            continue;
        }

        // !!! FIXME: this is a mess, decide who should be doing this stuff, and only do it once.
        lemon_token = convert_to_lemon_token(ctx, token, tokenlen, tokenval);
        switch (lemon_token)
        {
            case TOKEN_HLSL_INT_CONSTANT:
                data.i64 = strtoi64(token, tokenlen);
                break;

            case TOKEN_HLSL_FLOAT_CONSTANT:
                data.dbl = strtodouble(token, tokenlen);
                break;

            case TOKEN_HLSL_USERTYPE:
                data.string = stringcache_len(ctx->strcache, token, tokenlen);
                data.datatype = get_usertype(ctx, data.string);  // !!! FIXME: do we need this? It's kind of useless during parsing.
                assert(data.datatype != NULL);
                break;

            case TOKEN_HLSL_STRING_LITERAL:
            case TOKEN_HLSL_IDENTIFIER:
                data.string = stringcache_len(ctx->strcache, token, tokenlen);
                break;

            default:
                data.i64 = 0;
                break;
        } // switch

        ParseHLSL(parser, lemon_token, data, ctx);

        // this probably isn't perfect, but it's good enough for surviving
        //  the parse. We'll sort out correctness once we have a tree.
        if (lemon_token == TOKEN_HLSL_LBRACE)
            push_scope(ctx);
        else if (lemon_token == TOKEN_HLSL_RBRACE)
            pop_scope(ctx);
    } while (tokenval != TOKEN_EOI);

    // Clean out extra usertypes; they are dummies until semantic analysis.
    while (ctx->usertypes.scope != start_scope)
        pop_symbol(ctx, &ctx->usertypes);

    ParseHLSLFree(parser, ctx->free, ctx->malloc_data);
    preprocessor_end(pp);
} // parse_source


/* Intermediate representation... */

static inline int generate_ir_label(Context *ctx)
{
    return ctx->ir_label_count++;
} // generate_ir_label

static inline int generate_ir_temp(Context *ctx)
{
    return ctx->ir_temp_count++;
} // generate_ir_temp

static const LoopLabels *push_ir_loop(Context *ctx, const int isswitch)
{
    // !!! FIXME: cache these allocations?
    LoopLabels *retval = Malloc(ctx, sizeof (LoopLabels));
    if (retval)
    {
        retval->start = (isswitch) ? -1 : generate_ir_label(ctx);
        retval->end = generate_ir_label(ctx);
        retval->prev = ctx->ir_loop;
        ctx->ir_loop = retval;
    } // if

    return retval;
} // push_ir_loop

static void pop_ir_loop(Context *ctx)
{
    assert(ctx->ir_loop != NULL);
    LoopLabels *labels = ctx->ir_loop;
    ctx->ir_loop = ctx->ir_loop->prev;
    Free(ctx, labels);
} // pop_ir_loop


#define NEW_IR_NODE(retval, cls, typ) \
    cls *retval = (cls *) Malloc(ctx, sizeof (cls)); \
    do { \
        if (retval == NULL) { return NULL; } \
        retval->ir.type = typ; \
        retval->ir.filename = ctx->sourcefile; \
        retval->ir.line = ctx->sourceline; \
    } while (0)

#define NEW_IR_EXPR(retval, cls, typ, dt, numelems) \
    cls *retval = (cls *) Malloc(ctx, sizeof (cls)); \
    do { \
        if (retval == NULL) { return NULL; } \
        retval->info.ir.type = typ; \
        retval->info.ir.filename = ctx->sourcefile; \
        retval->info.ir.line = ctx->sourceline; \
        retval->info.type = dt; \
        retval->info.elements = numelems; \
    } while (0)

// syntactic sugar.
static inline MOJOSHADER_irNode *build_ir(Context *ctx, void *_ast);
static inline MOJOSHADER_irExpression *build_ir_expr(Context *ctx, void *_ast)
{
    MOJOSHADER_irNode *retval = build_ir(ctx, _ast);
    assert(!retval || (retval->ir.type > MOJOSHADER_IR_START_RANGE_EXPR));
    assert(!retval || (retval->ir.type < MOJOSHADER_IR_END_RANGE_EXPR));
    return (MOJOSHADER_irExpression *) retval;
} // build_ir_expr

static inline MOJOSHADER_irStatement *build_ir_stmt(Context *ctx, void *_ast)
{
    MOJOSHADER_irNode *retval = build_ir(ctx, _ast);
    assert(!retval || (retval->ir.type > MOJOSHADER_IR_START_RANGE_STMT));
    assert(!retval || (retval->ir.type < MOJOSHADER_IR_END_RANGE_STMT));
    return (MOJOSHADER_irStatement *) retval;
} // build_ir_stmt


static MOJOSHADER_irExpression *new_ir_binop(Context *ctx,
                                       const MOJOSHADER_irBinOpType op,
                                       MOJOSHADER_irExpression *left,
                                       MOJOSHADER_irExpression *right)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irBinOp, MOJOSHADER_IR_BINOP, left->info.type, left->info.elements);
    assert(left->info.type == right->info.type);
    assert(left->info.elements == right->info.elements);
    retval->op = op;
    retval->left = left;
    retval->right = right;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_binop

static MOJOSHADER_irExpression *new_ir_eseq(Context *ctx,
                                      MOJOSHADER_irStatement *stmt,
                                      MOJOSHADER_irExpression *expr)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irESeq, MOJOSHADER_IR_ESEQ, expr->info.type, expr->info.elements);
    retval->stmt = stmt;
    retval->expr = expr;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_eseq

static MOJOSHADER_irExpression *new_ir_temp(Context *ctx, const int index,
                                            const MOJOSHADER_astDataTypeType type,
                                            const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irTemp, MOJOSHADER_IR_TEMP, type, elements);
    retval->index = index;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_temp



#define NEW_IR_BINOP(op,l,r) new_ir_binop(ctx, MOJOSHADER_IR_BINOP_##op, l, r)
#define EASY_IR_BINOP(op) \
    NEW_IR_BINOP(op, build_ir_expr(ctx, ast->binary.left), \
                 build_ir_expr(ctx, ast->binary.right))

// You have to fill in ->value yourself!
static MOJOSHADER_irExpression *new_ir_constant(Context *ctx,
                                                const MOJOSHADER_astDataTypeType type,
                                                const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irConstant, MOJOSHADER_IR_CONSTANT, type, elements);
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_constant

static MOJOSHADER_irExpression *new_ir_constint(Context *ctx, const int val)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irConstant, MOJOSHADER_IR_CONSTANT, MOJOSHADER_AST_DATATYPE_INT, 1);
    retval->value.ival[0] = val;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_constint

static MOJOSHADER_irExpression *new_ir_constfloat(Context *ctx, const float val)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irConstant, MOJOSHADER_IR_CONSTANT, MOJOSHADER_AST_DATATYPE_FLOAT, 1);
    retval->value.fval[0] = val;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_constfloat

static MOJOSHADER_irExpression *new_ir_constbool(Context *ctx, const int val)
{
    // !!! FIXME: cache true and false in (ctx), ignore in delete_ir().
    NEW_IR_EXPR(retval, MOJOSHADER_irConstant, MOJOSHADER_IR_CONSTANT, MOJOSHADER_AST_DATATYPE_BOOL, 1);
    retval->value.ival[0] = val;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_constbool

static MOJOSHADER_irExpression *new_ir_convert(Context *ctx, MOJOSHADER_irExpression *expr,
                                               const MOJOSHADER_astDataTypeType type,
                                               const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irConvert, MOJOSHADER_IR_CONVERT, type, elements);
    retval->expr = expr;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_convert

static MOJOSHADER_irExpression *new_ir_construct(Context *ctx, MOJOSHADER_irExprList *args,
                                               const MOJOSHADER_astDataTypeType type,
                                               const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irConstruct, MOJOSHADER_IR_CONSTRUCT, type, elements);
    retval->args = args;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_construct

static MOJOSHADER_irExpression *new_ir_call(Context *ctx, const int index,
                                            MOJOSHADER_irExprList *args,
                                            const MOJOSHADER_astDataTypeType type,
                                            const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irCall, MOJOSHADER_IR_CALL, type, elements);
    retval->args = args;
    retval->index = index;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_call

static MOJOSHADER_irExpression *new_ir_swizzle(Context *ctx,
                                               MOJOSHADER_irExpression *expr,
                                               const char *channels,
                                               const MOJOSHADER_astDataTypeType type,
                                               const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irSwizzle, MOJOSHADER_IR_SWIZZLE, type, elements);
    retval->expr = expr;
    memcpy(retval->channels, channels, sizeof (retval->channels));
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_swizzle

static MOJOSHADER_irExpression *new_ir_memory(Context *ctx, const int index,
                                              const MOJOSHADER_astDataTypeType type,
                                              const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irMemory, MOJOSHADER_IR_MEMORY, type, elements);
    retval->index = index;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_memory

static MOJOSHADER_irExpression *new_ir_array(Context *ctx,
                                             MOJOSHADER_irExpression *array,
                                             MOJOSHADER_irExpression *element,
                                             const MOJOSHADER_astDataTypeType type,
                                             const int elements)
{
    NEW_IR_EXPR(retval, MOJOSHADER_irArray, MOJOSHADER_IR_ARRAY, type, elements);
    retval->array = array;
    retval->element = element;
    return (MOJOSHADER_irExpression *) retval;
} // new_ir_array

static MOJOSHADER_irStatement *new_ir_seq(Context *ctx,
                                     MOJOSHADER_irStatement *first,
                                     MOJOSHADER_irStatement *next)
{
    assert((first != NULL) || (next != NULL));
    if (first == NULL)  // don't generate a SEQ if unnecessary.
        return next;
    else if (next == NULL)
        return first;

    NEW_IR_NODE(retval, MOJOSHADER_irSeq, MOJOSHADER_IR_SEQ);
    retval->first = first;
    retval->next = next;
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_seq

static MOJOSHADER_irStatement *new_ir_jump(Context *ctx, const int label)
{
    NEW_IR_NODE(retval, MOJOSHADER_irJump, MOJOSHADER_IR_JUMP);
    retval->label = label;
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_jump

static MOJOSHADER_irStatement *new_ir_cjump(Context *ctx,
                                       const MOJOSHADER_irConditionType cond,
                                       MOJOSHADER_irExpression *left,
                                       MOJOSHADER_irExpression *right,
                                       const int iftrue, const int iffalse)
{
    NEW_IR_NODE(retval, MOJOSHADER_irCJump, MOJOSHADER_IR_CJUMP);
    retval->cond = cond;
    retval->left = left;
    retval->right = right;
    retval->iftrue = iftrue;
    retval->iffalse = iffalse;
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_cjump

static MOJOSHADER_irStatement *new_ir_label(Context *ctx, const int index)
{
    NEW_IR_NODE(retval, MOJOSHADER_irLabel, MOJOSHADER_IR_LABEL);
    retval->index = index;
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_label

static MOJOSHADER_irStatement *new_ir_move(Context *ctx,
                                      MOJOSHADER_irExpression *dst,
                                      MOJOSHADER_irExpression *src,
                                      const int writemask)
{
    NEW_IR_NODE(retval, MOJOSHADER_irMove, MOJOSHADER_IR_MOVE);
    assert(dst->info.type == src->info.type);
    assert(dst->info.elements == src->info.elements);
    retval->dst = dst;
    retval->src = src;
    retval->writemask = writemask;
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_move

static MOJOSHADER_irStatement *new_ir_expr_stmt(Context *ctx, MOJOSHADER_irExpression *expr)
{
    NEW_IR_NODE(retval, MOJOSHADER_irExprStmt, MOJOSHADER_IR_EXPR_STMT);
    retval->expr = expr;
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_expr_stmt

static MOJOSHADER_irStatement *new_ir_discard(Context *ctx)
{
    NEW_IR_NODE(retval, MOJOSHADER_irDiscard, MOJOSHADER_IR_DISCARD);
    return (MOJOSHADER_irStatement *) retval;
} // new_ir_discard

static MOJOSHADER_irExprList *new_ir_exprlist(Context *ctx, MOJOSHADER_irExpression *expr)
{
    NEW_IR_NODE(retval, MOJOSHADER_irExprList, MOJOSHADER_IR_EXPRLIST);
    retval->expr = expr;
    retval->next = NULL;
    return (MOJOSHADER_irExprList *) retval;
} // new_ir_exprlist


// This handles most comparison operators (less-than, equals, etc...)
static MOJOSHADER_irExpression *build_ir_compare(Context *ctx,
                                    const MOJOSHADER_irConditionType operation,
                                    MOJOSHADER_irExpression *left,
                                    MOJOSHADER_irExpression *right,
                                    MOJOSHADER_irExpression *tval,
                                    MOJOSHADER_irExpression *fval)
{
    /* The gist...
            cjump x < y, t, f  // '<' is whatever operation
        t:
            move tmp, tval
            jump join
        f:
            move tmp, fval
        join:
    */

    const int t = generate_ir_label(ctx);
    const int f = generate_ir_label(ctx);
    const int join = generate_ir_label(ctx);
    const int tmp = generate_ir_temp(ctx);

    assert(tval->info.type == fval->info.type);
    assert(tval->info.elements == fval->info.elements);

    const MOJOSHADER_astDataTypeType dt = tval->info.type;
    const int elements = tval->info.elements;

    return new_ir_eseq(ctx,
                new_ir_seq(ctx, new_ir_cjump(ctx, operation, left, right, t, f),
                new_ir_seq(ctx, new_ir_label(ctx, t),
                new_ir_seq(ctx, new_ir_move(ctx, new_ir_temp(ctx, tmp, dt, elements), tval, -1),
                new_ir_seq(ctx, new_ir_jump(ctx, join),
                new_ir_seq(ctx, new_ir_label(ctx, f),
                new_ir_seq(ctx, new_ir_move(ctx, new_ir_temp(ctx, tmp, dt, elements), fval, -1),
                                new_ir_label(ctx, join))))))),
                    new_ir_temp(ctx, tmp, dt, elements));
} // build_ir_compare

#define EASY_IR_COMPARE(op) \
    build_ir_compare(ctx, MOJOSHADER_IR_COND_##op, \
                   build_ir_expr(ctx, ast->binary.left), \
                   build_ir_expr(ctx, ast->binary.right), \
                   new_ir_constbool(ctx, 1), \
                   new_ir_constbool(ctx, 0))


// This handles && and || operators.
static MOJOSHADER_irExpression *build_ir_logical_and_or(Context *ctx,
                                    const MOJOSHADER_astExpressionBinary *ast,
                                    const int left_testval)
{
    /* The gist...
            cjump left == left_testval, maybe, f
        maybe:
            cjump right == true, t, f
        t:
            move tmp, 1
            jump join
        f:
            move tmp, 0
        join:
    */

    assert(ast->left->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);
    assert(ast->right->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);

    const int t = generate_ir_label(ctx);
    const int f = generate_ir_label(ctx);
    const int maybe = generate_ir_label(ctx);
    const int join = generate_ir_label(ctx);
    const int tmp = generate_ir_temp(ctx);

    return new_ir_eseq(ctx,
                new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->left), new_ir_constbool(ctx, left_testval), maybe, f),
                new_ir_seq(ctx, new_ir_label(ctx, maybe),
                new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->right), new_ir_constbool(ctx, 1), t, f),
                new_ir_seq(ctx, new_ir_label(ctx, t),
                new_ir_seq(ctx, new_ir_move(ctx, new_ir_temp(ctx, tmp, MOJOSHADER_AST_DATATYPE_BOOL, 1), new_ir_constbool(ctx, 1), -1),
                new_ir_seq(ctx, new_ir_jump(ctx, join),
                new_ir_seq(ctx, new_ir_label(ctx, f),
                new_ir_seq(ctx, new_ir_move(ctx, new_ir_temp(ctx, tmp, MOJOSHADER_AST_DATATYPE_BOOL, 1), new_ir_constbool(ctx, 0), -1),
                                new_ir_label(ctx, join))))))))),
                    new_ir_temp(ctx, tmp, MOJOSHADER_AST_DATATYPE_BOOL, 1));
} // build_ir_logical_and_or

static inline MOJOSHADER_irExpression *build_ir_logical_and(Context *ctx,
                                    const MOJOSHADER_astExpressionBinary *ast)
{
    // this needs to not evaluate (right) if (left) is false!
    return build_ir_logical_and_or(ctx, ast, 1);
} // build_ir_logical_and

static inline MOJOSHADER_irExpression *build_ir_logical_or(Context *ctx,
                                    const MOJOSHADER_astExpressionBinary *ast)
{
    // this needs to not evaluate (right) if (left) is true!
    return build_ir_logical_and_or(ctx, ast, 0);
} // build_ir_logical_or

static inline MOJOSHADER_irStatement *build_ir_no_op(Context *ctx)
{
    return new_ir_label(ctx, generate_ir_label(ctx));
} // build_ir_no_op

static MOJOSHADER_irStatement *build_ir_ifstmt(Context *ctx,
                                          const MOJOSHADER_astIfStatement *ast)
{
    assert(ast->expr->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);

    // !!! FIXME: ast->attributes?

    // IF statement without an ELSE.
    if (ast->else_statement == NULL)
    {
        /* The gist...
                cjump expr, t, join
            t:
                statement
            join:
        */

        const int t = generate_ir_label(ctx);
        const int join = generate_ir_label(ctx);

        return new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->expr), new_ir_constbool(ctx, 1), t, join),
               new_ir_seq(ctx, new_ir_label(ctx, t),
               new_ir_seq(ctx, build_ir_stmt(ctx, ast->statement),
               new_ir_seq(ctx, new_ir_label(ctx, join),
                               build_ir_stmt(ctx, ast->next)))));
    } // if

    // IF statement _with_ an ELSE.
    /* The gist...
            cjump expr, t, f
        t:
            statement
            jump join
        f:
            elsestatement
        join:
    */

    const int t = generate_ir_label(ctx);
    const int f = generate_ir_label(ctx);
    const int join = generate_ir_label(ctx);

    return new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->expr), new_ir_constbool(ctx, 1), t, f),
           new_ir_seq(ctx, new_ir_label(ctx, t),
           new_ir_seq(ctx, build_ir_stmt(ctx, ast->statement),
           new_ir_seq(ctx, new_ir_jump(ctx, join),
           new_ir_seq(ctx, new_ir_label(ctx, f),
           new_ir_seq(ctx, build_ir_stmt(ctx, ast->else_statement),
           new_ir_seq(ctx, new_ir_label(ctx, join),
                           build_ir_stmt(ctx, ast->next))))))));
} // build_ir_ifstmt


static MOJOSHADER_irStatement *build_ir_forstmt(Context *ctx,
                                       const MOJOSHADER_astForStatement *ast)
{
    // !!! FIXME: ast->unroll

    assert(ast->looptest->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);

    /* The gist...
            initializer  // (or var_decl->initializer!)
        test:
            cjump looptest == true, loop, join
        loop:
            statement
        increment:  // needs to be here; this is where "continue" jumps!
            counter
            jump test
        join:
    */

    const int test = generate_ir_label(ctx);
    const int loop = generate_ir_label(ctx);

    const LoopLabels *labels = push_ir_loop(ctx, 0);
    if (labels == NULL)
        return NULL;  // out of memory...

    const int increment = labels->start;
    const int join = labels->end;

    assert( (ast->var_decl && !ast->initializer) ||
            (!ast->var_decl && ast->initializer) );

    MOJOSHADER_irStatement *init = NULL;
    if (ast->var_decl != NULL)
    {
//sdfsdf
        // !!! FIXME: map the initializer to the variable? Need fix to var_decl parsing.
//        new_ir_move(ctx, FIXME MAP TO REGISTER ast->var_decl->index, build_ir_expr(ctx, ast->fsdf));
//        FIXME
//        init = build_ir_vardecl(ctx, ast->var_decl);
    } // if
    else
    {
//        init = build_ir_expr(ctx, ast->initializer);
    } // else

    MOJOSHADER_irStatement *retval =
        new_ir_seq(ctx, init,
        new_ir_seq(ctx, new_ir_label(ctx, test),
        new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->looptest), new_ir_constbool(ctx, 1), loop, join),
        new_ir_seq(ctx, new_ir_label(ctx, loop),
        new_ir_seq(ctx, build_ir_stmt(ctx, ast->statement),
        new_ir_seq(ctx, new_ir_label(ctx, increment),
        new_ir_seq(ctx, new_ir_expr_stmt(ctx, build_ir_expr(ctx, ast->counter)),
        new_ir_seq(ctx, new_ir_jump(ctx, test),
                        new_ir_label(ctx, join)))))))));

    pop_ir_loop(ctx);

    return new_ir_seq(ctx, retval, build_ir_stmt(ctx, ast->next));
} // build_ir_forstmt

static MOJOSHADER_irStatement *build_ir_whilestmt(Context *ctx,
                                          const MOJOSHADER_astWhileStatement *ast)
{
    // !!! FIXME: ast->unroll

    assert(ast->expr->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);

    /* The gist...
        loop:
            cjump expr == true, t, join
        t:
            statement
            jump loop
        join:
    */

    const LoopLabels *labels = push_ir_loop(ctx, 0);
    if (labels == NULL)
        return NULL;  // out of memory...

    const int loop = labels->start;
    const int t = generate_ir_label(ctx);
    const int join = labels->end;

    MOJOSHADER_irStatement *retval =
        new_ir_seq(ctx, new_ir_label(ctx, loop),
        new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->expr), new_ir_constbool(ctx, 1), t, join),
        new_ir_seq(ctx, new_ir_label(ctx, t),
        new_ir_seq(ctx, build_ir_stmt(ctx, ast->statement),
        new_ir_seq(ctx, new_ir_jump(ctx, loop),
                        new_ir_label(ctx, join))))));

    pop_ir_loop(ctx);

    return new_ir_seq(ctx, retval, build_ir_stmt(ctx, ast->next));
} // build_ir_whilestmt

static MOJOSHADER_irStatement *build_ir_dostmt(Context *ctx,
                                          const MOJOSHADER_astDoStatement *ast)
{
    // !!! FIXME: ast->unroll

    assert(ast->expr->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);

    /* The gist...
        loop:
            statement
            cjump expr == true, loop, join
        join:
    */

    const LoopLabels *labels = push_ir_loop(ctx, 0);
    if (labels == NULL)
        return NULL;  // out of memory...

    const int loop = labels->start;
    const int join = labels->end;

    MOJOSHADER_irStatement *retval =
        new_ir_seq(ctx, new_ir_label(ctx, loop),
        new_ir_seq(ctx, build_ir_stmt(ctx, ast->statement),
        new_ir_seq(ctx, new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, ast->expr), new_ir_constbool(ctx, 1), loop, join),
                        new_ir_label(ctx, join))));

    pop_ir_loop(ctx);

    return new_ir_seq(ctx, retval, build_ir_stmt(ctx, ast->next));
} // build_ir_dostmt

static MOJOSHADER_irStatement *build_ir_switch(Context *ctx, const MOJOSHADER_astSwitchStatement *ast)
{
    // Dithering down to a list of if-statements in all cases
    //  isn't ideal, but we can't do jumptables in D3D bytecode.

    // !!! FIXME: attributes?

    /* The gist...
            move tmp, expr
            cjump tmp == case1expr, case1, testcase2
        testcase2:  // etc
            cjump tmp == case2expr, case2, join
        case1:
            case1stmt  // might have a break in it somewhere.
        case2:
            case2stmt
        join:
    */

    const LoopLabels *labels = push_ir_loop(ctx, 1);
    if (labels == NULL)
        return NULL;  // out of memory...

    const int join = labels->end;
    const int elems = datatype_elems(ctx, ast->expr->datatype);
    const MOJOSHADER_astDataTypeType dt = datatype_base(ctx, ast->expr->datatype)->type;

    const MOJOSHADER_astSwitchCases *cases = ast->cases;
    const int tmp = generate_ir_temp(ctx);
    MOJOSHADER_irStatement *startseqs = new_ir_move(ctx, new_ir_temp(ctx, tmp, dt, elems), build_ir_expr(ctx, ast->expr), -1);
    MOJOSHADER_irStatement *testseqs = startseqs;
    MOJOSHADER_irStatement *startcaseseqs = NULL;
    MOJOSHADER_irStatement *caseseqs = NULL;
    while (cases)
    {
        const int t = generate_ir_label(ctx);
        const int f = (cases->next == NULL) ? join : generate_ir_label(ctx);
        MOJOSHADER_irStatement *cjump = new_ir_cjump(ctx, MOJOSHADER_IR_COND_EQL, build_ir_expr(ctx, cases->expr), new_ir_temp(ctx, tmp, dt, elems), t, f);

        if (cases->next == NULL)  // last one, do the join label.
        {
            testseqs = new_ir_seq(ctx, testseqs, cjump);
            caseseqs = new_ir_seq(ctx, caseseqs, new_ir_seq(ctx, new_ir_label(ctx, t), build_ir_stmt(ctx, cases->statement)));
            caseseqs = new_ir_seq(ctx, caseseqs, new_ir_label(ctx, f));
        } // if
        else
        {
            testseqs = new_ir_seq(ctx, testseqs, new_ir_seq(ctx, cjump, new_ir_label(ctx, f)));
            caseseqs = new_ir_seq(ctx, caseseqs, new_ir_seq(ctx, new_ir_label(ctx, t), build_ir_stmt(ctx, cases->statement)));
        } // else

        if (startcaseseqs == NULL)
            startcaseseqs = caseseqs;

        cases = cases->next;
    } // while

    pop_ir_loop(ctx);

    return new_ir_seq(ctx, startseqs, new_ir_seq(ctx, startcaseseqs, build_ir_stmt(ctx, ast->next)));
} // build_ir_switch

static MOJOSHADER_irExpression *build_ir_increxpr(Context *ctx, const MOJOSHADER_astDataType *_dt,
                                                  const int val)
{
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, _dt);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    MOJOSHADER_irConstant *retval = (MOJOSHADER_irConstant *) new_ir_constant(ctx, type, elems);
    int i;

    switch (type)
    {
        case MOJOSHADER_AST_DATATYPE_BOOL:
        case MOJOSHADER_AST_DATATYPE_INT:
        case MOJOSHADER_AST_DATATYPE_UINT:
            for (i = 0; i < elems; i++)
                retval->value.ival[i] = (int) val;
            break;

        case MOJOSHADER_AST_DATATYPE_FLOAT:
        case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM:
        case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM:
        case MOJOSHADER_AST_DATATYPE_HALF:
        case MOJOSHADER_AST_DATATYPE_DOUBLE:
            for (i = 0; i < elems; i++)
                retval->value.fval[i] = (float) val;
            break;

        default:
            assert(0 && "Semantic analysis should have caught this!");
    } // switch

    return (MOJOSHADER_irExpression *) retval;
} // build_ir_increxpr

static MOJOSHADER_irExpression *build_ir_preincdec(Context *ctx, MOJOSHADER_astExpressionUnary *ast, const MOJOSHADER_irBinOpType binop)
{
    /* The gist...
        move expr, expr + 1
        return expr
    */
    // !!! FIXME: can you writemask an increment operator?
    MOJOSHADER_irExpression *constant = build_ir_increxpr(ctx, ast->datatype, 1);
    return new_ir_eseq(ctx,
                new_ir_move(ctx,
                    build_ir_expr(ctx, ast->operand),
                    new_ir_binop(ctx, binop, build_ir_expr(ctx, ast->operand), constant), -1),
                build_ir_expr(ctx, ast->operand));
} // build_ir_preincdec

static MOJOSHADER_irExpression *build_ir_postincdec(Context *ctx, MOJOSHADER_astExpressionUnary *ast, const MOJOSHADER_irBinOpType binop)
{
    /* The gist...
        move tmp, expr
        move expr, expr + 1
        return tmp
    */

    // !!! FIXME: can you writemask an increment operator?
    MOJOSHADER_irExpression *constant = build_ir_increxpr(ctx, ast->datatype, 1);
    const int tmp = generate_ir_temp(ctx);
    return new_ir_eseq(ctx,
                new_ir_seq(ctx,
                    new_ir_move(ctx, new_ir_temp(ctx, tmp, constant->info.type, constant->info.elements), build_ir_expr(ctx, ast->operand), -1),
                    new_ir_move(ctx, build_ir_expr(ctx, ast->operand),
                        new_ir_binop(ctx, binop, build_ir_expr(ctx, ast->operand), constant), -1)),
                new_ir_temp(ctx, tmp, constant->info.type, constant->info.elements));
} // build_ir_postincdec

static MOJOSHADER_irExpression *build_ir_convert(Context *ctx, const MOJOSHADER_astExpressionCast *ast)
{
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    return new_ir_convert(ctx, build_ir_expr(ctx, ast->operand), type, elems);
} // build_ir_convert

static MOJOSHADER_irExprList *build_ir_exprlist(Context *ctx, MOJOSHADER_astArguments *args)
{
    MOJOSHADER_irExprList *retval = NULL;
    MOJOSHADER_irExprList *prev = NULL;

    while (args != NULL)
    {
        assert((retval && prev) || ((!retval) && (!prev)));

        MOJOSHADER_irExprList *item = new_ir_exprlist(ctx, build_ir_expr(ctx, args->argument));
        if (prev == NULL)
            prev = retval = item;
        else
            prev->next = item;

        args = args->next;
    } // while

    return retval;
} // build_ir_exprlist

static MOJOSHADER_irExpression *build_ir_constructor(Context *ctx, const MOJOSHADER_astExpressionConstructor *ast)
{
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    assert(elems <= 16);  // just in case (matrix4x4 constructor is largest).
    return new_ir_construct(ctx, build_ir_exprlist(ctx, ast->args), type, elems);
} // build_ir_constructor

static MOJOSHADER_irExpression *build_ir_call(Context *ctx, const MOJOSHADER_astExpressionCallFunction *ast)
{
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    return new_ir_call(ctx, ast->identifier->index, build_ir_exprlist(ctx, ast->args), type, elems);
} // build_ir_call

static char swiz_to_channel(const char swiz)
{
    if ((swiz == 'r') || (swiz == 'x')) return 0;
    if ((swiz == 'g') || (swiz == 'y')) return 1;
    if ((swiz == 'b') || (swiz == 'z')) return 2;
    if ((swiz == 'a') || (swiz == 'w')) return 3;
    assert(0 && "Should have been caught by semantic analysis.");
    return 0;
} // swiz_to_channel

static MOJOSHADER_irExpression *build_ir_swizzle(Context *ctx, const MOJOSHADER_astExpressionDerefStruct *ast)
{
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    char chans[4] = { 0, 0, 0, 0 };
    const char *swizstr = ast->member;
    int i;

    for (i = 0; swizstr[i]; i++)
        chans[i] = swiz_to_channel(swizstr[i]);

    return new_ir_swizzle(ctx, build_ir_expr(ctx, ast->identifier), chans, type, elems);
} // build_ir_swizzle

static MOJOSHADER_irExpression *build_ir_identifier(Context *ctx, const MOJOSHADER_astExpressionIdentifier *ast)
{
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    return new_ir_memory(ctx, ast->index, type, elems);
} // build_ir_identifier

static MOJOSHADER_irExpression *build_ir_derefstruct(Context *ctx, const MOJOSHADER_astExpressionDerefStruct *ast)
{
    // There are only three possible IR nodes that contain a struct:
    //  an irTemp, an irMemory, or an irESeq that results in a temp or memory.
    //  As such, we figure out which it is, and offset appropriately for the
    //  member.
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);
    MOJOSHADER_irExpression *expr = build_ir_expr(ctx, ast->identifier);
    MOJOSHADER_irExpression *finalexpr = expr;

    assert(!ast->isswizzle);

    while (finalexpr->ir.type == MOJOSHADER_IR_ESEQ)
        finalexpr = finalexpr->eseq.expr;

    if (finalexpr->ir.type == MOJOSHADER_IR_TEMP)
        finalexpr->temp.index += ast->member_index;
    else if (finalexpr->ir.type == MOJOSHADER_IR_MEMORY)
        finalexpr->memory.index += ast->member_index;
    else
        assert(0 && "Unexpected condition");

    // Replace the struct type with the type of the member.
    expr->info.type = type;
    expr->info.elements = elems;

    return expr;
} // build_ir_derefstruct

static MOJOSHADER_irExpression *build_ir_derefarray(Context *ctx, const MOJOSHADER_astExpressionBinary *ast)
{
    // In most compilers, arrays dither down to offsets into memory, but
    //  they're somewhat special in D3D, since they might have to deal with
    //  vectors, etc...so we keep them as first-class citizens of the IR,
    //  and let the optimizer/codegen sort it out.
    // !!! FIXME: this might be the wrong move. Maybe remove this IR node type?
    const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->datatype);
    const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
    const int elems = datatype_elems(ctx, dt);

    // !!! FIXME: Array dereference of a vector can become a simple swizzle operation, if we have a constant index.
    // !!! FIXME: Matrix dereference of a vector can become a simple reference to a temp/memory, if we have a constant index.
    return new_ir_array(ctx, build_ir_expr(ctx, ast->left), build_ir_expr(ctx, ast->right), type, elems);
} // build_ir_derefarray

static MOJOSHADER_irExpression *build_ir_assign_binop(Context *ctx,
                                                const MOJOSHADER_irBinOpType op,
                                                const MOJOSHADER_astExpressionBinary *ast)
{
    MOJOSHADER_irExpression *lvalue = build_ir_expr(ctx, ast->left);
    MOJOSHADER_irExpression *rvalue = build_ir_expr(ctx, ast->right);
    const MOJOSHADER_astDataTypeType type = lvalue->info.type;
    const int elems = lvalue->info.elements;
    const int tmp = generate_ir_temp(ctx);

    // Semantic analysis should have inserted casts if necessary.
    assert(type == rvalue->info.type);
    assert(elems == rvalue->info.elements);

    // The destination must eventually be lvalue, which means memory or temp.
    MOJOSHADER_irExpression *dst = lvalue;
    while (dst->ir.type == MOJOSHADER_IR_ESEQ)
        dst = dst->eseq.expr;

    if (dst->ir.type == MOJOSHADER_IR_TEMP)
        dst = new_ir_temp(ctx, dst->temp.index, dst->info.type, dst->info.elements);
    else if (dst->ir.type == MOJOSHADER_IR_MEMORY)
        dst = new_ir_memory(ctx, dst->memory.index, dst->info.type, dst->info.elements);
    else
        assert(0 && "Unexpected condition");

    // !!! FIXME: write masking!
    return new_ir_eseq(ctx,
                new_ir_seq(ctx,
                    new_ir_move(ctx, new_ir_temp(ctx, tmp, type, elems), new_ir_binop(ctx, op, lvalue, rvalue), -1),
                    new_ir_move(ctx, dst, new_ir_temp(ctx, tmp, type, elems), -1)),
                new_ir_temp(ctx, tmp, type, elems));
} // build_ir_assign_binop

static MOJOSHADER_irExpression *build_ir_assign(Context *ctx,
                                                const MOJOSHADER_astExpressionBinary *ast)
{
    MOJOSHADER_irExpression *lvalue = build_ir_expr(ctx, ast->left);
    MOJOSHADER_irExpression *rvalue = build_ir_expr(ctx, ast->right);
    const MOJOSHADER_astDataTypeType type = lvalue->info.type;
    const int elems = lvalue->info.elements;
    const int tmp = generate_ir_temp(ctx);

    // Semantic analysis should have inserted casts if necessary.
    assert(type == rvalue->info.type);
    assert(elems == rvalue->info.elements);

    // !!! FIXME: write masking!
    // !!! FIXME: whole array/struct assignments need to become a sequence of moves.
    return new_ir_eseq(ctx,
                new_ir_seq(ctx,
                    new_ir_move(ctx, new_ir_temp(ctx, tmp, type, elems), rvalue, -1),
                    new_ir_move(ctx, lvalue, new_ir_temp(ctx, tmp, type, elems), -1)),
                new_ir_temp(ctx, tmp, type, elems));
} // build_ir_assign


// The AST must be perfect and normalized and sane here. If there are any
//  strange corner cases, you should strive to handle them in semantic
//  analysis, so conversion to IR can proceed with a minimum of drama.
static void *build_ir_internal(Context *ctx, void *_ast);
static inline MOJOSHADER_irNode *build_ir(Context *ctx, void *_ast)
{
    return (MOJOSHADER_irNode *) build_ir_internal(ctx, _ast);
} // build_ir

static void *build_ir_internal(Context *ctx, void *_ast)
{
    if ((_ast == NULL) || (ctx->out_of_memory))
        return NULL;

    MOJOSHADER_astNode *ast = (MOJOSHADER_astNode *) _ast;

    // upkeep so we report correct error locations...
    ctx->sourcefile = ast->ast.filename;
    ctx->sourceline = ast->ast.line;

    switch (ast->ast.type)
    {
        case MOJOSHADER_AST_OP_PREINCREMENT:  // !!! FIXME: sequence points?
            return build_ir_preincdec(ctx, &ast->unary, MOJOSHADER_IR_BINOP_ADD);

        case MOJOSHADER_AST_OP_POSTINCREMENT: // !!! FIXME: sequence points?
            return build_ir_postincdec(ctx, &ast->unary, MOJOSHADER_IR_BINOP_ADD);

        case MOJOSHADER_AST_OP_PREDECREMENT:  // !!! FIXME: sequence points?
            return build_ir_preincdec(ctx, &ast->unary, MOJOSHADER_IR_BINOP_SUBTRACT);

        case MOJOSHADER_AST_OP_POSTDECREMENT: // !!! FIXME: sequence points?
            return build_ir_postincdec(ctx, &ast->unary, MOJOSHADER_IR_BINOP_SUBTRACT);

        case MOJOSHADER_AST_OP_COMPLEMENT:
            return NEW_IR_BINOP(XOR, build_ir_expr(ctx, ast->unary.operand),
                                new_ir_constint(ctx, 0xFFFFFFFF));

        case MOJOSHADER_AST_OP_NEGATE:  // !!! FIXME: -0.0f != +0.0f
            return NEW_IR_BINOP(SUBTRACT, build_ir_increxpr(ctx, ast->unary.datatype, -1),
                                build_ir_expr(ctx, ast->unary.operand));

        case MOJOSHADER_AST_OP_NOT:  // operand must be bool here!
            assert(ast->unary.operand->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);
            return NEW_IR_BINOP(XOR, build_ir_expr(ctx, ast->unary.operand),
                                new_ir_constint(ctx, 1));

        case MOJOSHADER_AST_OP_DEREF_ARRAY:
            return build_ir_derefarray(ctx, &ast->binary);

        case MOJOSHADER_AST_OP_DEREF_STRUCT:
            if (ast->derefstruct.isswizzle)
                return build_ir_swizzle(ctx, &ast->derefstruct);
            return build_ir_derefstruct(ctx, &ast->derefstruct);

        case MOJOSHADER_AST_OP_COMMA:
            // evaluate and throw away left, return right.
            return new_ir_eseq(ctx, new_ir_expr_stmt(ctx, build_ir_expr(ctx, ast->binary.left)),
                               build_ir_expr(ctx, ast->binary.right));

        case MOJOSHADER_AST_OP_LESSTHAN: return EASY_IR_COMPARE(LT);
        case MOJOSHADER_AST_OP_GREATERTHAN: return EASY_IR_COMPARE(GT);
        case MOJOSHADER_AST_OP_LESSTHANOREQUAL: return EASY_IR_COMPARE(LEQ);
        case MOJOSHADER_AST_OP_GREATERTHANOREQUAL: return EASY_IR_COMPARE(GEQ);
        case MOJOSHADER_AST_OP_NOTEQUAL: return EASY_IR_COMPARE(NEQ);
        case MOJOSHADER_AST_OP_EQUAL: return EASY_IR_COMPARE(EQL);

        case MOJOSHADER_AST_OP_MULTIPLY: return EASY_IR_BINOP(MULTIPLY);
        case MOJOSHADER_AST_OP_DIVIDE: return EASY_IR_BINOP(DIVIDE);
        case MOJOSHADER_AST_OP_MODULO: return EASY_IR_BINOP(MODULO);
        case MOJOSHADER_AST_OP_ADD: return EASY_IR_BINOP(ADD);
        case MOJOSHADER_AST_OP_SUBTRACT: return EASY_IR_BINOP(SUBTRACT);
        case MOJOSHADER_AST_OP_LSHIFT: return EASY_IR_BINOP(LSHIFT);
        case MOJOSHADER_AST_OP_RSHIFT: return EASY_IR_BINOP(RSHIFT);
        case MOJOSHADER_AST_OP_BINARYAND: return EASY_IR_BINOP(AND);
        case MOJOSHADER_AST_OP_BINARYXOR: return EASY_IR_BINOP(XOR);
        case MOJOSHADER_AST_OP_BINARYOR: return EASY_IR_BINOP(OR);

        case MOJOSHADER_AST_OP_LOGICALAND:
            return build_ir_logical_and(ctx, &ast->binary);

        case MOJOSHADER_AST_OP_LOGICALOR:
            return build_ir_logical_or(ctx, &ast->binary);

        case MOJOSHADER_AST_OP_ASSIGN:
            return build_ir_assign(ctx, &ast->binary);

        case MOJOSHADER_AST_OP_MULASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_MULTIPLY, &ast->binary);
        case MOJOSHADER_AST_OP_DIVASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_DIVIDE, &ast->binary);
        case MOJOSHADER_AST_OP_MODASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_MODULO, &ast->binary);
        case MOJOSHADER_AST_OP_ADDASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_ADD, &ast->binary);
        case MOJOSHADER_AST_OP_SUBASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_SUBTRACT, &ast->binary);
        case MOJOSHADER_AST_OP_LSHIFTASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_LSHIFT, &ast->binary);
        case MOJOSHADER_AST_OP_RSHIFTASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_RSHIFT, &ast->binary);
        case MOJOSHADER_AST_OP_ANDASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_AND, &ast->binary);
        case MOJOSHADER_AST_OP_XORASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_XOR, &ast->binary);
        case MOJOSHADER_AST_OP_ORASSIGN: return build_ir_assign_binop(ctx, MOJOSHADER_IR_BINOP_OR, &ast->binary);

        case MOJOSHADER_AST_OP_CONDITIONAL:
            assert(ast->binary.left->datatype->type == MOJOSHADER_AST_DATATYPE_BOOL);
            return build_ir_compare(ctx, MOJOSHADER_IR_COND_EQL,
                                  build_ir_expr(ctx, ast->ternary.left),
                                  new_ir_constbool(ctx, 1),
                                  build_ir_expr(ctx, ast->ternary.center),
                                  build_ir_expr(ctx, ast->ternary.right));

        case MOJOSHADER_AST_OP_IDENTIFIER:
            return build_ir_identifier(ctx, &ast->identifier);

        case MOJOSHADER_AST_OP_INT_LITERAL:
            return new_ir_constint(ctx, ast->intliteral.value);

        case MOJOSHADER_AST_OP_FLOAT_LITERAL:
            return new_ir_constfloat(ctx, ast->floatliteral.value);

        case MOJOSHADER_AST_OP_BOOLEAN_LITERAL:
            return new_ir_constbool(ctx, ast->boolliteral.value);

        case MOJOSHADER_AST_OP_CALLFUNC:
            return build_ir_call(ctx, &ast->callfunc);

        case MOJOSHADER_AST_OP_CONSTRUCTOR:
            return build_ir_constructor(ctx, &ast->constructor);

        case MOJOSHADER_AST_OP_CAST:
            return build_ir_convert(ctx, &ast->cast);

        case MOJOSHADER_AST_STATEMENT_BREAK:
        {
            const LoopLabels *labels = ctx->ir_loop;
            assert(labels != NULL);  // semantic analysis should catch this.
            return new_ir_jump(ctx, labels->end);
        } // case

        case MOJOSHADER_AST_STATEMENT_CONTINUE:
        {
            const LoopLabels *labels = ctx->ir_loop;
            assert(labels != NULL);  // semantic analysis should catch this.
            return new_ir_jump(ctx, labels->start);
        } // case

        case MOJOSHADER_AST_STATEMENT_DISCARD:
            return new_ir_seq(ctx, new_ir_discard(ctx), build_ir_stmt(ctx, ast->discardstmt.next));

        case MOJOSHADER_AST_STATEMENT_EMPTY:
            return build_ir(ctx, ast->stmt.next);  // skip it, do next thing.

        case MOJOSHADER_AST_STATEMENT_EXPRESSION:
            return new_ir_seq(ctx, new_ir_expr_stmt(ctx, build_ir_expr(ctx, ast->exprstmt.expr)), build_ir_stmt(ctx, ast->exprstmt.next));

        case MOJOSHADER_AST_STATEMENT_IF:
            return build_ir_ifstmt(ctx, &ast->ifstmt);

        case MOJOSHADER_AST_STATEMENT_TYPEDEF:  // ignore this, move on.
            return build_ir(ctx, ast->typedefstmt.next);

        case MOJOSHADER_AST_STATEMENT_SWITCH:
            return build_ir_switch(ctx, &ast->switchstmt);

        case MOJOSHADER_AST_STATEMENT_STRUCT:  // ignore this, move on.
            return build_ir(ctx, ast->structstmt.next);

        case MOJOSHADER_AST_STATEMENT_VARDECL: // ignore this, move on.
            return build_ir(ctx, ast->vardeclstmt.next);

        case MOJOSHADER_AST_STATEMENT_BLOCK:
            return new_ir_seq(ctx, build_ir_stmt(ctx, ast->blockstmt.statements), build_ir_stmt(ctx, ast->blockstmt.next));

        case MOJOSHADER_AST_STATEMENT_FOR:
            return build_ir_forstmt(ctx, &ast->forstmt);

        case MOJOSHADER_AST_STATEMENT_DO:
            return build_ir_dostmt(ctx, &ast->dostmt);

        case MOJOSHADER_AST_STATEMENT_WHILE:
            return build_ir_whilestmt(ctx, &ast->whilestmt);

        case MOJOSHADER_AST_STATEMENT_RETURN:
        {
            const int label = ctx->ir_end;
            assert(label >= 0);  // parser should have caught this!
            MOJOSHADER_irStatement *retval = NULL;
            if (ast->returnstmt.expr != NULL)
            {
                // !!! FIXME: whole array/struct returns need to move more into the temp.
                const MOJOSHADER_astDataType *dt = reduce_datatype(ctx, ast->returnstmt.expr->datatype);
                const MOJOSHADER_astDataTypeType type = datatype_base(ctx, dt)->type;
                const int elems = datatype_elems(ctx, dt);
                assert(ctx->ir_ret >= 0);
                retval = new_ir_move(ctx, new_ir_temp(ctx, ctx->ir_ret, type, elems), build_ir_expr(ctx, ast->returnstmt.expr), -1);
            } // if
            return new_ir_seq(ctx, retval, new_ir_jump(ctx, label));
        } // case

        case MOJOSHADER_AST_COMPUNIT_TYPEDEF:
        case MOJOSHADER_AST_COMPUNIT_STRUCT:
        case MOJOSHADER_AST_COMPUNIT_VARIABLE:
        case MOJOSHADER_AST_COMPUNIT_FUNCTION:
        case MOJOSHADER_AST_ARGUMENTS:
        case MOJOSHADER_AST_OP_STRING_LITERAL:
        case MOJOSHADER_AST_SWITCH_CASE:
        case MOJOSHADER_AST_SCALAR_OR_ARRAY:
        case MOJOSHADER_AST_TYPEDEF:
        case MOJOSHADER_AST_FUNCTION_PARAMS:
        case MOJOSHADER_AST_FUNCTION_SIGNATURE:
        case MOJOSHADER_AST_STRUCT_DECLARATION:
        case MOJOSHADER_AST_STRUCT_MEMBER:
        case MOJOSHADER_AST_VARIABLE_DECLARATION:
        case MOJOSHADER_AST_ANNOTATION:
        case MOJOSHADER_AST_PACK_OFFSET:
        case MOJOSHADER_AST_VARIABLE_LOWLEVEL:
            assert(0 && "Shouldn't hit this in build_ir.");
            return NULL;

        default:
            assert(0 && "unexpected type");
            return NULL;
    } // switch
} // build_ir

static void print_ir(FILE *io, unsigned int depth, void *_ir)
{
    MOJOSHADER_irNode *ir = (MOJOSHADER_irNode *) _ir;
    if (ir == NULL)
        return;

    const char *fname = strrchr(ir->ir.filename, '/');
    if (fname != NULL)
        fname++;
    else
    {
        fname = strrchr(ir->ir.filename, '\\');
        if (fname != NULL)
            fname++;
        else
            fname = ir->ir.filename;
    } // else

    int i;
    for (i = 0; i < depth; i++)
        fprintf(io, "  ");
    depth++;

    fprintf(io, "[ %s:%d ", fname, ir->ir.line);

    switch (ir->ir.type)
    {
        case MOJOSHADER_IR_LABEL:
            fprintf(io, "LABEL %d ]\n", ir->stmt.label.index);
            break;

        case MOJOSHADER_IR_CONSTANT:
            fprintf(io, "CONSTANT ");
            switch (ir->expr.constant.info.type)
            {
                case MOJOSHADER_AST_DATATYPE_BOOL:
                case MOJOSHADER_AST_DATATYPE_INT:
                case MOJOSHADER_AST_DATATYPE_UINT:
                    for (i = 0; i < ir->expr.constant.info.elements-1; i++)
                        fprintf(io, "%d, ", ir->expr.constant.value.ival[i]);
                    if (ir->expr.constant.info.elements > 0)
                        fprintf(io, "%d", ir->expr.constant.value.ival[i]);
                    break;

                case MOJOSHADER_AST_DATATYPE_FLOAT:
                case MOJOSHADER_AST_DATATYPE_FLOAT_SNORM:
                case MOJOSHADER_AST_DATATYPE_FLOAT_UNORM:
                case MOJOSHADER_AST_DATATYPE_HALF:
                case MOJOSHADER_AST_DATATYPE_DOUBLE:
                    for (i = 0; i < ir->expr.constant.info.elements-1; i++)
                        fprintf(io, "%ff, ", ir->expr.constant.value.fval[i]);
                    if (ir->expr.constant.info.elements > 0)
                        fprintf(io, "%ff", ir->expr.constant.value.fval[i]);
                    break;

                default: assert(0 && "shouldn't happen");
            } // switch
            fprintf(io, " ]\n");
            break;

        case MOJOSHADER_IR_TEMP:
            fprintf(io, "TEMP %d ]\n", ir->expr.temp.index);
            break;

        case MOJOSHADER_IR_DISCARD:
            fprintf(io, "DISCARD ]\n");
            break;

        case MOJOSHADER_IR_SWIZZLE:
            fprintf(io, "SWIZZLE");
            for (i = 0; i < ir->expr.swizzle.info.elements-1; i++)
                fprintf(io, " %d", (int) ir->expr.swizzle.channels[i]);
            fprintf(io, " ]\n");
            print_ir(io, depth, ir->expr.swizzle.expr);
            break;

        case MOJOSHADER_IR_CONSTRUCT:
            fprintf(io, "CONSTRUCT ]\n");
            print_ir(io, depth, ir->expr.construct.args);
            break;

        case MOJOSHADER_IR_CONVERT:
            fprintf(io, "CONVERT ]\n");
            print_ir(io, depth, ir->expr.convert.expr);
            break;

        case MOJOSHADER_IR_BINOP:
            fprintf(io, "BINOP ");
            switch (ir->expr.binop.op)
            {
                #define PRINT_IR_BINOP(x) \
                    case MOJOSHADER_IR_BINOP_##x: fprintf(io, #x); break;
                PRINT_IR_BINOP(ADD)
                PRINT_IR_BINOP(SUBTRACT)
                PRINT_IR_BINOP(MULTIPLY)
                PRINT_IR_BINOP(DIVIDE)
                PRINT_IR_BINOP(MODULO)
                PRINT_IR_BINOP(AND)
                PRINT_IR_BINOP(OR)
                PRINT_IR_BINOP(XOR)
                PRINT_IR_BINOP(LSHIFT)
                PRINT_IR_BINOP(RSHIFT)
                PRINT_IR_BINOP(UNKNOWN)
                #undef PRINT_IR_BINOP
                default: assert(0 && "unexpected case"); break;
            } // switch
            fprintf(io, " ]\n");
            print_ir(io, depth, ir->expr.binop.left);
            print_ir(io, depth, ir->expr.binop.right);
            break;

        case MOJOSHADER_IR_MEMORY:
            fprintf(io, "MEMORY %d ]\n", ir->expr.memory.index);
            break;

        case MOJOSHADER_IR_CALL:
            fprintf(io, "CALL %d ]\n", ir->expr.call.index);
            print_ir(io, depth, ir->expr.call.args);
            break;

        case MOJOSHADER_IR_ESEQ:
            fprintf(io, "ESEQ ]\n");
            print_ir(io, depth, ir->expr.eseq.stmt);
            break;

        case MOJOSHADER_IR_ARRAY:
            fprintf(io, "ARRAY ]\n");
            print_ir(io, depth, ir->expr.array.array);
            print_ir(io, depth, ir->expr.array.element);
            break;

        case MOJOSHADER_IR_MOVE:
            fprintf(io, "MOVE ]\n");
            print_ir(io, depth, ir->stmt.move.dst);
            print_ir(io, depth, ir->stmt.move.src);
            break;

        case MOJOSHADER_IR_EXPR_STMT:
            fprintf(io, "EXPRSTMT ]\n");
            print_ir(io, depth, ir->stmt.expr.expr);
            break;

        case MOJOSHADER_IR_JUMP:
            fprintf(io, "JUMP %d ]\n", ir->stmt.jump.label);
            break;

        case MOJOSHADER_IR_CJUMP:
            fprintf(io, "CJUMP ");
            switch (ir->stmt.cjump.cond)
            {
                #define PRINT_IR_COND(x) \
                    case MOJOSHADER_IR_COND_##x: fprintf(io, #x); break;
                PRINT_IR_COND(EQL)
                PRINT_IR_COND(NEQ)
                PRINT_IR_COND(LT)
                PRINT_IR_COND(GT)
                PRINT_IR_COND(LEQ)
                PRINT_IR_COND(GEQ)
                PRINT_IR_COND(UNKNOWN)
                #undef PRINT_IR_COND
                default: assert(0 && "unexpected case"); break;
            } // switch
            fprintf(io, " %d %d ]\n", ir->stmt.cjump.iftrue, ir->stmt.cjump.iffalse);
            print_ir(io, depth, ir->stmt.cjump.left);
            print_ir(io, depth, ir->stmt.cjump.right);
            break;

        case MOJOSHADER_IR_SEQ:
            fprintf(io, "SEQ ]\n");
            print_ir(io, depth, ir->stmt.seq.first);
            print_ir(io, depth, ir->stmt.seq.next);  // !!! FIXME: don't recurse?
            break;

        case MOJOSHADER_IR_EXPRLIST:
            fprintf(io, "EXPRLIST ]\n");
            print_ir(io, depth, ir->misc.exprlist.expr);
            print_ir(io, depth, ir->misc.exprlist.next);  // !!! FIXME: don't recurse?
            break;

        default: assert(0 && "unexpected IR node"); break;
    } // switch
} // print_ir

static void print_whole_ir(Context *ctx, FILE *io)
{
    if (ctx->ir != NULL)
    {
        int i;
        for (i = 0; i <= ctx->user_func_index; i++)
        {
            printf("[FUNCTION %d ]\n", i);
            print_ir(io, 1, ctx->ir[i]);
        } // for
    } // if
} // print_whole_ir

static void delete_ir(Context *ctx, void *_ir)
{
    MOJOSHADER_irNode *ir = (MOJOSHADER_irNode *) _ir;
    if (ir == NULL)
        return;

    switch (ir->ir.type)
    {
        case MOJOSHADER_IR_JUMP:
        case MOJOSHADER_IR_LABEL:
        case MOJOSHADER_IR_CONSTANT:
        case MOJOSHADER_IR_TEMP:
        case MOJOSHADER_IR_DISCARD:
        case MOJOSHADER_IR_MEMORY:
            break;  // nothing extra to free here.

        case MOJOSHADER_IR_BINOP:
            delete_ir(ctx, ir->expr.binop.left);
            delete_ir(ctx, ir->expr.binop.right);
            break;

        case MOJOSHADER_IR_CALL:
            delete_ir(ctx, ir->expr.call.args);
            break;

        case MOJOSHADER_IR_ESEQ:
            delete_ir(ctx, ir->expr.eseq.stmt);
            delete_ir(ctx, ir->expr.eseq.expr);
            break;

        case MOJOSHADER_IR_ARRAY:
            delete_ir(ctx, ir->expr.array.array);
            delete_ir(ctx, ir->expr.array.element);
            break;

        case MOJOSHADER_IR_MOVE:
            delete_ir(ctx, ir->stmt.move.dst);
            delete_ir(ctx, ir->stmt.move.src);
            break;

        case MOJOSHADER_IR_EXPR_STMT:
            delete_ir(ctx, ir->stmt.expr.expr);
            break;

        case MOJOSHADER_IR_CJUMP:
            delete_ir(ctx, ir->stmt.cjump.left);
            delete_ir(ctx, ir->stmt.cjump.right);
            break;

        case MOJOSHADER_IR_SEQ:
            delete_ir(ctx, ir->stmt.seq.first);
            delete_ir(ctx, ir->stmt.seq.next);  // !!! FIXME: don't recurse?
            break;

        case MOJOSHADER_IR_EXPRLIST:
            delete_ir(ctx, ir->misc.exprlist.expr);
            delete_ir(ctx, ir->misc.exprlist.next);  // !!! FIXME: don't recurse?
            break;

        case MOJOSHADER_IR_SWIZZLE:
            delete_ir(ctx, ir->expr.swizzle.expr);
            break;

        case MOJOSHADER_IR_CONSTRUCT:
            delete_ir(ctx, ir->expr.construct.args);
            break;

        case MOJOSHADER_IR_CONVERT:
            delete_ir(ctx, ir->expr.convert.expr);
            break;

        default: assert(0 && "unexpected IR node"); break;
    } // switch

    Free(ctx, ir);
} // delete_ir

static void intermediate_representation(Context *ctx)
{
    const MOJOSHADER_astCompilationUnit *ast = NULL;
    const MOJOSHADER_astCompilationUnitFunction *astfn = NULL;
    const size_t arraylen = (ctx->user_func_index+1) * sizeof (MOJOSHADER_irStatement *);

    ctx->ir = Malloc(ctx, arraylen);
    if (ctx->ir == NULL)
        return;
    memset(ctx->ir, '\0', arraylen);

    ctx->ir_end = -1;
    ctx->ir_ret = -1;

    for (ast = &ctx->ast->compunit; ast != NULL; ast = ast->next)
    {
        assert(ast->ast.type > MOJOSHADER_AST_COMPUNIT_START_RANGE);
        assert(ast->ast.type < MOJOSHADER_AST_COMPUNIT_END_RANGE);
        if (ast->ast.type != MOJOSHADER_AST_COMPUNIT_FUNCTION)
            continue;  // only care about functions right now.

        astfn = (MOJOSHADER_astCompilationUnitFunction *) ast;
        if (astfn->definition == NULL)  // just a predeclare; skip.
            continue;

        assert(ctx->ir_loop == NULL);  // parser should have caught this!
        assert(ctx->ir_end < 0);  // parser should have caught this!
        assert(ctx->ir_ret < 0);  // parser should have caught this!
        const int start = generate_ir_label(ctx);  // !!! FIXME: store somewhere.
        const int end = generate_ir_label(ctx);
        ctx->ir_end = end;

        if (astfn->declaration->datatype != NULL)
            ctx->ir_ret = generate_ir_temp(ctx);

        MOJOSHADER_irStatement *funcseq = new_ir_seq(ctx, new_ir_label(ctx, start), build_ir_stmt(ctx, astfn->definition));
        funcseq = new_ir_seq(ctx, funcseq, new_ir_label(ctx, end));
        assert(ctx->ir_loop == NULL);  // parser should have caught this!
        ctx->ir_end = -1;
        ctx->ir_ret = -1;

        assert(astfn->index <= ctx->user_func_index);
        assert(ctx->ir[astfn->index] == NULL);
        ctx->ir[astfn->index] = funcseq;
    } // for

    print_whole_ir(ctx, stdout);

    // done with the AST, nuke it.
    // !!! FIXME: we're going to need CTAB data from this at some point.
    delete_compilation_unit(ctx, (MOJOSHADER_astCompilationUnit *) ctx->ast);
    ctx->ast = NULL;
} // intermediate_representation



static MOJOSHADER_astData MOJOSHADER_out_of_mem_ast_data = {
    1, &MOJOSHADER_out_of_mem_error, 0, 0, 0, 0, 0, 0
};


// !!! FIXME: cut and paste from assembler.
static const MOJOSHADER_astData *build_failed_ast(Context *ctx)
{
    assert(isfail(ctx));

    if (ctx->out_of_memory)
        return &MOJOSHADER_out_of_mem_ast_data;
        
    MOJOSHADER_astData *retval = NULL;
    retval = (MOJOSHADER_astData *) Malloc(ctx, sizeof (MOJOSHADER_astData));
    if (retval == NULL)
        return &MOJOSHADER_out_of_mem_ast_data;

    memset(retval, '\0', sizeof (MOJOSHADER_astData));
    retval->source_profile = ctx->source_profile;
    retval->malloc = (ctx->malloc == MOJOSHADER_internal_malloc) ? NULL : ctx->malloc;
    retval->free = (ctx->free == MOJOSHADER_internal_free) ? NULL : ctx->free;
    retval->malloc_data = ctx->malloc_data;
    retval->error_count = errorlist_count(ctx->errors);
    retval->errors = errorlist_flatten(ctx->errors);

    if (ctx->out_of_memory)
    {
        Free(ctx, retval);
        return &MOJOSHADER_out_of_mem_ast_data;
    } // if

    return retval;
} // build_failed_ast


static const MOJOSHADER_astData *build_astdata(Context *ctx)
{
    MOJOSHADER_astData *retval = NULL;

    if (ctx->out_of_memory)
        return &MOJOSHADER_out_of_mem_ast_data;

    retval = (MOJOSHADER_astData *) Malloc(ctx, sizeof (MOJOSHADER_astData));
    if (retval == NULL)
        return &MOJOSHADER_out_of_mem_ast_data;

    memset(retval, '\0', sizeof (MOJOSHADER_astData));
    retval->malloc = (ctx->malloc == MOJOSHADER_internal_malloc) ? NULL : ctx->malloc;
    retval->free = (ctx->free == MOJOSHADER_internal_free) ? NULL : ctx->free;
    retval->malloc_data = ctx->malloc_data;

    if (!isfail(ctx))
    {
        retval->source_profile = ctx->source_profile;
        retval->ast = ctx->ast;
    } // if

    retval->error_count = errorlist_count(ctx->errors);
    retval->errors = errorlist_flatten(ctx->errors);
    if (ctx->out_of_memory)
    {
        Free(ctx, retval);
        return &MOJOSHADER_out_of_mem_ast_data;
    } // if

    retval->opaque = ctx;

    return retval;
} // build_astdata


static void choose_src_profile(Context *ctx, const char *srcprofile)
{
    ctx->source_profile = srcprofile;

    #define TEST_PROFILE(x) if (strcmp(srcprofile, x) == 0) { return; }

    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_VS_1_1);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_VS_2_0);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_VS_3_0);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_PS_1_1);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_PS_1_2);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_PS_1_3);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_PS_1_4);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_PS_2_0);
    TEST_PROFILE(MOJOSHADER_SRC_PROFILE_HLSL_PS_3_0);

    #undef TEST_PROFILE

    fail(ctx, "Unknown profile");
} // choose_src_profile


static MOJOSHADER_compileData MOJOSHADER_out_of_mem_compile_data = {
    1, &MOJOSHADER_out_of_mem_error, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};


// !!! FIXME: cut and paste from assembler.
static const MOJOSHADER_compileData *build_failed_compile(Context *ctx)
{
    assert(isfail(ctx));

    MOJOSHADER_compileData *retval = NULL;
    retval = (MOJOSHADER_compileData *) Malloc(ctx, sizeof (MOJOSHADER_compileData));
    if (retval == NULL)
        return &MOJOSHADER_out_of_mem_compile_data;

    memset(retval, '\0', sizeof (MOJOSHADER_compileData));
    retval->malloc = (ctx->malloc == MOJOSHADER_internal_malloc) ? NULL : ctx->malloc;
    retval->free = (ctx->free == MOJOSHADER_internal_free) ? NULL : ctx->free;
    retval->malloc_data = ctx->malloc_data;
    retval->source_profile = ctx->source_profile;
    retval->error_count = errorlist_count(ctx->errors);
    retval->errors = errorlist_flatten(ctx->errors);
    retval->warning_count = errorlist_count(ctx->warnings);
    retval->warnings = errorlist_flatten(ctx->warnings);

    if (ctx->out_of_memory)  // in case something failed up there.
    {
        MOJOSHADER_freeCompileData(retval);
        return &MOJOSHADER_out_of_mem_compile_data;
    } // if

    return retval;
} // build_failed_compile


static const MOJOSHADER_compileData *build_compiledata(Context *ctx)
{
    assert(!isfail(ctx));

    MOJOSHADER_compileData *retval = NULL;

    retval = (MOJOSHADER_compileData *) Malloc(ctx, sizeof (MOJOSHADER_compileData));
    if (retval == NULL)
        return &MOJOSHADER_out_of_mem_compile_data;

    memset(retval, '\0', sizeof (MOJOSHADER_compileData));
    retval->malloc = (ctx->malloc == MOJOSHADER_internal_malloc) ? NULL : ctx->malloc;
    retval->free = (ctx->free == MOJOSHADER_internal_free) ? NULL : ctx->free;
    retval->malloc_data = ctx->malloc_data;
    retval->source_profile = ctx->source_profile;

    if (!isfail(ctx))
    {
        // !!! FIXME: build output and output_len here.
    } // if

    if (!isfail(ctx))
    {
        // !!! FIXME: build symbols and symbol_count here.
    } // if

    retval->error_count = errorlist_count(ctx->errors);
    retval->errors = errorlist_flatten(ctx->errors);
    retval->warning_count = errorlist_count(ctx->warnings);
    retval->warnings = errorlist_flatten(ctx->warnings);

    if (ctx->out_of_memory)  // in case something failed up there.
    {
        MOJOSHADER_freeCompileData(retval);
        return &MOJOSHADER_out_of_mem_compile_data;
    } // if

    return retval;
} // build_compiledata


// API entry point...

// !!! FIXME: move this (and a lot of other things) to mojoshader_ast.c.
const MOJOSHADER_astData *MOJOSHADER_parseAst(const char *srcprofile,
                                    const char *filename, const char *source,
                                    unsigned int sourcelen,
                                    const MOJOSHADER_preprocessorDefine *defs,
                                    unsigned int define_count,
                                    MOJOSHADER_includeOpen include_open,
                                    MOJOSHADER_includeClose include_close,
                                    MOJOSHADER_malloc m, MOJOSHADER_free f,
                                    void *d)
{
    const MOJOSHADER_astData *retval = NULL;
    Context *ctx = NULL;

    if ( ((m == NULL) && (f != NULL)) || ((m != NULL) && (f == NULL)) )
        return &MOJOSHADER_out_of_mem_ast_data;  // supply both or neither.

    ctx = build_context(m, f, d);
    if (ctx == NULL)
        return &MOJOSHADER_out_of_mem_ast_data;

    choose_src_profile(ctx, srcprofile);

    if (!isfail(ctx))
    {
        parse_source(ctx, filename, source, sourcelen, defs, define_count,
                     include_open, include_close);
    } // if

    if (!isfail(ctx))
        retval = build_astdata(ctx);  // ctx isn't destroyed yet!
    else
    {
        retval = (MOJOSHADER_astData *) build_failed_ast(ctx);
        destroy_context(ctx);
    } // else

    return retval;
} // MOJOSHADER_parseAst


void MOJOSHADER_freeAstData(const MOJOSHADER_astData *_data)
{
    MOJOSHADER_astData *data = (MOJOSHADER_astData *) _data;
    if ((data == NULL) || (data == &MOJOSHADER_out_of_mem_ast_data))
        return;  // no-op.

    // !!! FIXME: this needs to live for deleting the stringcache and the ast.
    Context *ctx = (Context *) data->opaque;
    MOJOSHADER_free f = (data->free == NULL) ? MOJOSHADER_internal_free : data->free;
    void *d = data->malloc_data;
    int i;

    // we don't f(data->source_profile), because that's internal static data.

    for (i = 0; i < data->error_count; i++)
    {
        f((void *) data->errors[i].error, d);
        f((void *) data->errors[i].filename, d);
    } // for
    f((void *) data->errors, d);

    // don't delete data->ast (it'll delete with the context).
    f(data, d);

    destroy_context(ctx);  // finally safe to destroy this.
} // MOJOSHADER_freeAstData


const MOJOSHADER_compileData *MOJOSHADER_compile(const char *srcprofile,
                                    const char *filename, const char *source,
                                    unsigned int sourcelen,
                                    const MOJOSHADER_preprocessorDefine *defs,
                                    unsigned int define_count,
                                    MOJOSHADER_includeOpen include_open,
                                    MOJOSHADER_includeClose include_close,
                                    MOJOSHADER_malloc m, MOJOSHADER_free f,
                                    void *d)
{
    // !!! FIXME: cut and paste from MOJOSHADER_parseAst().
    MOJOSHADER_compileData *retval = NULL;
    Context *ctx = NULL;

    if ( ((m == NULL) && (f != NULL)) || ((m != NULL) && (f == NULL)) )
        return &MOJOSHADER_out_of_mem_compile_data;  // supply both or neither.

    ctx = build_context(m, f, d);
    if (ctx == NULL)
        return &MOJOSHADER_out_of_mem_compile_data;

    choose_src_profile(ctx, srcprofile);

    if (!isfail(ctx))
    {
        parse_source(ctx, filename, source, sourcelen, defs, define_count,
                     include_open, include_close);
    } // if

    if (!isfail(ctx))
        semantic_analysis(ctx);

    if (!isfail(ctx))
        intermediate_representation(ctx);

    if (isfail(ctx))
        retval = (MOJOSHADER_compileData *) build_failed_compile(ctx);
    else
        retval = (MOJOSHADER_compileData *) build_compiledata(ctx);

    destroy_context(ctx);
    return retval;
} // MOJOSHADER_compile


void MOJOSHADER_freeCompileData(const MOJOSHADER_compileData *_data)
{
    MOJOSHADER_compileData *data = (MOJOSHADER_compileData *) _data;
    if ((data == NULL) || (data == &MOJOSHADER_out_of_mem_compile_data))
        return;  // no-op.

    MOJOSHADER_free f = (data->free == NULL) ? MOJOSHADER_internal_free : data->free;
    void *d = data->malloc_data;
    int i;

    // we don't f(data->source_profile), because that's internal static data.

    for (i = 0; i < data->error_count; i++)
    {
        f((void *) data->errors[i].error, d);
        f((void *) data->errors[i].filename, d);
    } // for
    f((void *) data->errors, d);

    for (i = 0; i < data->warning_count; i++)
    {
        f((void *) data->warnings[i].error, d);
        f((void *) data->warnings[i].filename, d);
    } // for
    f((void *) data->warnings, d);

    for (i = 0; i < data->symbol_count; i++)
    {
        f((void *) data->symbols[i].name, d);
        // !!! FIXME: this is missing stuff (including freeing substructs).
    } // for
    f((void *) data->symbols, d);

    f((void *) data->output, d);
    f(data, d);
} // MOJOSHADER_freeCompileData

// end of mojoshader_compiler.c ...

