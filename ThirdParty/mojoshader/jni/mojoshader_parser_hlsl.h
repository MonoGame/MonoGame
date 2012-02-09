/*
 * My changes over the original lempar.c from SQLite are encased in
 *  #if __MOJOSHADER__ blocks.  --ryan.
 */
#ifndef __MOJOSHADER__
#define __MOJOSHADER__ 1
#endif

#if !__MOJOSHADER__
#define LEMON_SUPPORT_TRACING (!defined(NDEBUG))
#endif

/* Driver template for the LEMON parser generator.
** The original author(s) of lempar.c disclaim copyright to this source code.
** However, changes made for MojoShader fall under the same license as the
** rest of MojoShader. Please see the file LICENSE.txt in the source's root
** directory.
*/
/* First off, code is included that follows the "include" declaration
** in the input grammar file. */
#include <stdio.h>
#line 31 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"

#ifndef __MOJOSHADER_HLSL_COMPILER__
#error Do not compile this file directly.
#endif
#line 28 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
/* Next is all token values, in a form suitable for use by makeheaders.
** This section will be null unless lemon is run with the -m switch.
*/
/* 
** These constants (all generated automatically by the parser generator)
** specify the various kinds of tokens (terminals) that the parser
** understands. 
**
** Each symbol here is a terminal symbol in the grammar.
*/
#define TOKEN_HLSL_COMMA                           1
#define TOKEN_HLSL_ASSIGN                          2
#define TOKEN_HLSL_ADDASSIGN                       3
#define TOKEN_HLSL_SUBASSIGN                       4
#define TOKEN_HLSL_MULASSIGN                       5
#define TOKEN_HLSL_DIVASSIGN                       6
#define TOKEN_HLSL_MODASSIGN                       7
#define TOKEN_HLSL_LSHIFTASSIGN                    8
#define TOKEN_HLSL_RSHIFTASSIGN                    9
#define TOKEN_HLSL_ANDASSIGN                      10
#define TOKEN_HLSL_ORASSIGN                       11
#define TOKEN_HLSL_XORASSIGN                      12
#define TOKEN_HLSL_QUESTION                       13
#define TOKEN_HLSL_OROR                           14
#define TOKEN_HLSL_ANDAND                         15
#define TOKEN_HLSL_OR                             16
#define TOKEN_HLSL_XOR                            17
#define TOKEN_HLSL_AND                            18
#define TOKEN_HLSL_EQL                            19
#define TOKEN_HLSL_NEQ                            20
#define TOKEN_HLSL_LT                             21
#define TOKEN_HLSL_LEQ                            22
#define TOKEN_HLSL_GT                             23
#define TOKEN_HLSL_GEQ                            24
#define TOKEN_HLSL_LSHIFT                         25
#define TOKEN_HLSL_RSHIFT                         26
#define TOKEN_HLSL_PLUS                           27
#define TOKEN_HLSL_MINUS                          28
#define TOKEN_HLSL_STAR                           29
#define TOKEN_HLSL_SLASH                          30
#define TOKEN_HLSL_PERCENT                        31
#define TOKEN_HLSL_TYPECAST                       32
#define TOKEN_HLSL_EXCLAMATION                    33
#define TOKEN_HLSL_COMPLEMENT                     34
#define TOKEN_HLSL_MINUSMINUS                     35
#define TOKEN_HLSL_PLUSPLUS                       36
#define TOKEN_HLSL_DOT                            37
#define TOKEN_HLSL_LBRACKET                       38
#define TOKEN_HLSL_RBRACKET                       39
#define TOKEN_HLSL_LPAREN                         40
#define TOKEN_HLSL_RPAREN                         41
#define TOKEN_HLSL_ELSE                           42
#define TOKEN_HLSL_SEMICOLON                      43
#define TOKEN_HLSL_TYPEDEF                        44
#define TOKEN_HLSL_CONST                          45
#define TOKEN_HLSL_IDENTIFIER                     46
#define TOKEN_HLSL_VOID                           47
#define TOKEN_HLSL_INLINE                         48
#define TOKEN_HLSL_IN                             49
#define TOKEN_HLSL_INOUT                          50
#define TOKEN_HLSL_OUT                            51
#define TOKEN_HLSL_UNIFORM                        52
#define TOKEN_HLSL_COLON                          53
#define TOKEN_HLSL_LINEAR                         54
#define TOKEN_HLSL_CENTROID                       55
#define TOKEN_HLSL_NOINTERPOLATION                56
#define TOKEN_HLSL_NOPERSPECTIVE                  57
#define TOKEN_HLSL_SAMPLE                         58
#define TOKEN_HLSL_EXTERN                         59
#define TOKEN_HLSL_SHARED                         60
#define TOKEN_HLSL_STATIC                         61
#define TOKEN_HLSL_VOLATILE                       62
#define TOKEN_HLSL_ROWMAJOR                       63
#define TOKEN_HLSL_COLUMNMAJOR                    64
#define TOKEN_HLSL_LBRACE                         65
#define TOKEN_HLSL_RBRACE                         66
#define TOKEN_HLSL_STRUCT                         67
#define TOKEN_HLSL_PACKOFFSET                     68
#define TOKEN_HLSL_REGISTER                       69
#define TOKEN_HLSL_USERTYPE                       70
#define TOKEN_HLSL_SAMPLER                        71
#define TOKEN_HLSL_SAMPLER1D                      72
#define TOKEN_HLSL_SAMPLER2D                      73
#define TOKEN_HLSL_SAMPLER3D                      74
#define TOKEN_HLSL_SAMPLERCUBE                    75
#define TOKEN_HLSL_SAMPLER_STATE                  76
#define TOKEN_HLSL_SAMPLERSTATE                   77
#define TOKEN_HLSL_SAMPLERCOMPARISONSTATE         78
#define TOKEN_HLSL_BOOL                           79
#define TOKEN_HLSL_INT                            80
#define TOKEN_HLSL_UINT                           81
#define TOKEN_HLSL_HALF                           82
#define TOKEN_HLSL_FLOAT                          83
#define TOKEN_HLSL_DOUBLE                         84
#define TOKEN_HLSL_STRING                         85
#define TOKEN_HLSL_SNORM                          86
#define TOKEN_HLSL_UNORM                          87
#define TOKEN_HLSL_BUFFER                         88
#define TOKEN_HLSL_VECTOR                         89
#define TOKEN_HLSL_INT_CONSTANT                   90
#define TOKEN_HLSL_MATRIX                         91
#define TOKEN_HLSL_ISOLATE                        92
#define TOKEN_HLSL_MAXINSTRUCTIONCOUNT            93
#define TOKEN_HLSL_NOEXPRESSIONOPTIMIZATIONS      94
#define TOKEN_HLSL_REMOVEUNUSEDINPUTS             95
#define TOKEN_HLSL_UNUSED                         96
#define TOKEN_HLSL_XPS                            97
#define TOKEN_HLSL_BREAK                          98
#define TOKEN_HLSL_CONTINUE                       99
#define TOKEN_HLSL_DISCARD                        100
#define TOKEN_HLSL_DO                             101
#define TOKEN_HLSL_WHILE                          102
#define TOKEN_HLSL_RETURN                         103
#define TOKEN_HLSL_UNROLL                         104
#define TOKEN_HLSL_LOOP                           105
#define TOKEN_HLSL_FOR                            106
#define TOKEN_HLSL_BRANCH                         107
#define TOKEN_HLSL_IF                             108
#define TOKEN_HLSL_FLATTEN                        109
#define TOKEN_HLSL_IFALL                          110
#define TOKEN_HLSL_IFANY                          111
#define TOKEN_HLSL_PREDICATE                      112
#define TOKEN_HLSL_PREDICATEBLOCK                 113
#define TOKEN_HLSL_SWITCH                         114
#define TOKEN_HLSL_FORCECASE                      115
#define TOKEN_HLSL_CALL                           116
#define TOKEN_HLSL_CASE                           117
#define TOKEN_HLSL_DEFAULT                        118
#define TOKEN_HLSL_FLOAT_CONSTANT                 119
#define TOKEN_HLSL_STRING_LITERAL                 120
#define TOKEN_HLSL_TRUE                           121
#define TOKEN_HLSL_FALSE                          122
/* Make sure the INTERFACE macro is defined.
*/
#ifndef INTERFACE
# define INTERFACE 1
#endif
/* The next thing included is series of defines which control
** various aspects of the generated parser.
**    YYCODETYPE         is the data type used for storing terminal
**                       and nonterminal numbers.  "unsigned char" is
**                       used if there are fewer than 250 terminals
**                       and nonterminals.  "int" is used otherwise.
**    YYNOCODE           is a number of type YYCODETYPE which corresponds
**                       to no legal terminal or nonterminal number.  This
**                       number is used to fill in empty slots of the hash 
**                       table.
**    YYFALLBACK         If defined, this indicates that one or more tokens
**                       have fall-back values which should be used if the
**                       original value of the token will not parse.
**    YYACTIONTYPE       is the data type used for storing terminal
**                       and nonterminal numbers.  "unsigned char" is
**                       used if there are fewer than 250 rules and
**                       states combined.  "int" is used otherwise.
**    ParseHLSLTOKENTYPE     is the data type used for minor tokens given 
**                       directly to the parser from the tokenizer.
**    YYMINORTYPE        is the data type used for all minor tokens.
**                       This is typically a union of many types, one of
**                       which is ParseHLSLTOKENTYPE.  The entry in the union
**                       for base tokens is called "yy0".
**    YYSTACKDEPTH       is the maximum depth of the parser's stack.  If
**                       zero the stack is dynamically sized using realloc()
**    ParseHLSLARG_SDECL     A static variable declaration for the %extra_argument
**    ParseHLSLARG_PDECL     A parameter declaration for the %extra_argument
**    ParseHLSLARG_STORE     Code to store %extra_argument into yypParser
**    ParseHLSLARG_FETCH     Code to extract %extra_argument from yypParser
**    YYNSTATE           the combined number of states.
**    YYNRULE            the number of rules in the grammar
**    YYERRORSYMBOL      is the code number of the error symbol.  If not
**                       defined, then do no error processing.
*/
#define YYCODETYPE unsigned char
#define YYNOCODE 198
#define YYACTIONTYPE unsigned short int
#define ParseHLSLTOKENTYPE  TokenData 
typedef union {
  int yyinit;
  ParseHLSLTOKENTYPE yy0;
  MOJOSHADER_astPackOffset * yy8;
  MOJOSHADER_astVariableDeclaration * yy24;
  MOJOSHADER_astArguments * yy26;
  const MOJOSHADER_astDataType * yy37;
  MOJOSHADER_astTypedef * yy71;
  MOJOSHADER_astInputModifier yy75;
  MOJOSHADER_astVariableLowLevel * yy82;
  MOJOSHADER_astInterpolationModifier yy111;
  MOJOSHADER_astCompilationUnit * yy139;
  MOJOSHADER_astSwitchCases * yy165;
  MOJOSHADER_astFunctionStorageClass yy175;
  MOJOSHADER_astStatement * yy233;
  MOJOSHADER_astStructDeclaration * yy249;
  MOJOSHADER_astAnnotations * yy268;
  int yy270;
  const char * yy306;
  MOJOSHADER_astFunctionParameters * yy307;
  MOJOSHADER_astExpression * yy322;
  MOJOSHADER_astStructMembers * yy346;
  MOJOSHADER_astFunctionSignature * yy364;
  MOJOSHADER_astScalarOrArray * yy380;
} YYMINORTYPE;
#ifndef YYSTACKDEPTH
#define YYSTACKDEPTH 100
#endif
#define ParseHLSLARG_SDECL  Context *ctx ;
#define ParseHLSLARG_PDECL , Context *ctx 
#define ParseHLSLARG_FETCH  Context *ctx  = yypParser->ctx 
#define ParseHLSLARG_STORE yypParser->ctx  = ctx 
#define YYNSTATE 525
#define YYNRULE 288
#define YY_NO_ACTION      (YYNSTATE+YYNRULE+2)
#define YY_ACCEPT_ACTION  (YYNSTATE+YYNRULE+1)
#define YY_ERROR_ACTION   (YYNSTATE+YYNRULE)

/* The yyzerominor constant is used to initialize instances of
** YYMINORTYPE objects to zero. */
static const YYMINORTYPE yyzerominor = { 0 };

/* Define the yytestcase() macro to be a no-op if is not already defined
** otherwise.
**
** Applications can choose to define yytestcase() in the %include section
** to a macro that can assist in verifying code coverage.  For production
** code the yytestcase() macro should be turned off.  But it is useful
** for testing.
*/
#ifndef yytestcase
# define yytestcase(X)
#endif


/* Next are the tables used to determine what action to take based on the
** current state and lookahead token.  These tables are used to implement
** functions that take a state number and lookahead value and return an
** action integer.  
**
** Suppose the action integer is N.  Then the action is determined as
** follows
**
**   0 <= N < YYNSTATE                  Shift N.  That is, push the lookahead
**                                      token onto the stack and goto state N.
**
**   YYNSTATE <= N < YYNSTATE+YYNRULE   Reduce by rule N-YYNSTATE.
**
**   N == YYNSTATE+YYNRULE              A syntax error has occurred.
**
**   N == YYNSTATE+YYNRULE+1            The parser accepts its input.
**
**   N == YYNSTATE+YYNRULE+2            No such action.  Denotes unused
**                                      slots in the yy_action[] table.
**
** The action table is constructed as a single large table named yy_action[].
** Given state S and lookahead X, the action is computed as
**
**      yy_action[ yy_shift_ofst[S] + X ]
**
** If the index value yy_shift_ofst[S]+X is out of range or if the value
** yy_lookahead[yy_shift_ofst[S]+X] is not equal to X or if yy_shift_ofst[S]
** is equal to YY_SHIFT_USE_DFLT, it means that the action is not in the table
** and that yy_default[S] should be used instead.  
**
** The formula above is for computing the action when the lookahead is
** a terminal symbol.  If the lookahead is a non-terminal (as occurs after
** a reduce action) then the yy_reduce_ofst[] array is used in place of
** the yy_shift_ofst[] array and YY_REDUCE_USE_DFLT is used in place of
** YY_SHIFT_USE_DFLT.
**
** The following are the tables generated in this section:
**
**  yy_action[]        A single table containing all actions.
**  yy_lookahead[]     A table containing the lookahead for each entry in
**                     yy_action.  Used to detect hash collisions.
**  yy_shift_ofst[]    For each state, the offset into yy_action for
**                     shifting terminals.
**  yy_reduce_ofst[]   For each state, the offset into yy_action for
**                     shifting non-terminals after a reduce.
**  yy_default[]       Default action for each state.
*/
#define YY_ACTTAB_COUNT (5407)
static const YYACTIONTYPE yy_action[] = {
 /*     0 */    86,   85,  446,  447,  264,   46,   83,   84,   90,   91,
 /*    10 */    39,  123,   88,   47,   27,  114,  344,   97,  405,  143,
 /*    20 */   104,  422,   82,   81,   80,  407,  348,  140,  139,  410,
 /*    30 */    78,   77,  411,  409,  408,  406,  404,  403,    1,  383,
 /*    40 */   249,  495,   60,  511,  510,  509,  508,  507,  506,  505,
 /*    50 */   504,  503,  502,  501,  500,  499,  498,  497,  496,  295,
 /*    60 */   294,  293,  282,  482,  278,  250,  398,  397,  396,  395,
 /*    70 */   394,  248,  247,  246,  334,  338,   37,   40,  214,  336,
 /*    80 */   518,  333,   74,   72,   73,   71,   39,  332,  494,  100,
 /*    90 */    86,   85,  481,  480,  479,  478,   83,   84,   90,   91,
 /*   100 */   186,  123,  413,   47,   27,  115,  344,   97,  405,  143,
 /*   110 */   362,  429,   70,   69,  432,  407,  361,  140,  139,  410,
 /*   120 */   140,  139,  411,  409,  408,  406,  404,  403,    1,  384,
 /*   130 */   249,   76,   75,  511,  510,  509,  508,  507,  506,  505,
 /*   140 */   504,  503,  502,  501,  500,  499,  498,  497,  496,  295,
 /*   150 */   294,  293,  282,  482,  278,  146,  398,  397,  396,  395,
 /*   160 */   394,  248,  247,  246,  334,  338,   37,  369,  371,  336,
 /*   170 */   119,  333,  370,  366,  368,  132,  310,  332,  367,  100,
 /*   180 */    86,   85,  481,  480,  479,  478,   83,   84,   90,   91,
 /*   190 */   180,  123,  413,   47,  113,  423,  344,   97,  405,  143,
 /*   200 */   416,  140,  139,  493,  417,  407,  140,  139,  100,  410,
 /*   210 */   140,  139,  411,  409,  408,  406,  404,  403,    1,  181,
 /*   220 */   249,  413,   60,  511,  510,  509,  508,  507,  506,  505,
 /*   230 */   504,  503,  502,  501,  500,  499,  498,  497,  496,  295,
 /*   240 */   294,  293,  282,  482,  278,   63,  436,  117,  418,  138,
 /*   250 */   492,  248,  247,  246,  334,  338,   37,  415,  122,  336,
 /*   260 */   121,  333,  477,  140,  139,   62,   63,  332,  263,  259,
 /*   270 */    86,   85,  481,  480,  479,  478,   83,   84,   90,   91,
 /*   280 */   391,  261,   24,   47,   61,  434,   33,  515,  405,  143,
 /*   290 */   512,  517,  516,  514,  513,  407,  363,  365,  100,  410,
 /*   300 */   438,  364,  411,  409,  408,  406,  404,  403,   48,  181,
 /*   310 */   249,  413,   60,  511,  510,  509,  508,  507,  506,  505,
 /*   320 */   504,  503,  502,  501,  500,  499,  498,  497,  496,  295,
 /*   330 */   294,  293,  282,  482,  278,  263,  259,  352,  118,  340,
 /*   340 */   345,  109,  102,  130,  306,  122,  474,  428,  443,  122,
 /*   350 */   475,  152,  241,   94,  360,  399,  491,  421,  400,  252,
 /*   360 */   359,  296,  481,  480,  479,  478,  170,   24,   60,  515,
 /*   370 */    38,   79,  512,  517,  516,  514,  513,   18,  346,   60,
 /*   380 */   223,  219,  218,  216,  251,  339,  135,  402,   60,  476,
 /*   390 */   150,  401,   60,  450,  145,  467,  157,  199,  198,  149,
 /*   400 */   191,  272,  271,  269,  267,  189,  462,  352,  220,  340,
 /*   410 */   345,  109,  102,  502,  501,  500,  499,  498,  497,  496,
 /*   420 */   295,  294,   60,   94,  100,  399,  390,  522,   21,  252,
 /*   430 */   490,    3,  426,  489,  343,   38,  170,  437,   60,  515,
 /*   440 */    60,  296,  512,  517,  516,  514,  513,    5,  346,    1,
 /*   450 */   223,  219,  218,  216,  318,  339,  135,  296,   27,  476,
 /*   460 */   150,   27,   20,  450,  145,  467,  157,  199,  198,  149,
 /*   470 */   191,  272,  271,  269,  267,  189,  462,  105,  215,  285,
 /*   480 */   105,  352,  341,  340,  345,  109,  102,  502,  501,  500,
 /*   490 */   499,  498,  497,  496,  295,  294,  101,   94,  488,  399,
 /*   500 */   126,  487,  112,  252,  320,   40,  214,  103,  414,  184,
 /*   510 */   170,  283,  154,  515,  140,  139,  512,  517,  516,  514,
 /*   520 */   513,    4,  346,  486,  223,  219,  218,  216,   60,  339,
 /*   530 */   135,   60,  213,  476,  150,  134,  125,  450,  145,  467,
 /*   540 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*   550 */   462,  352,  137,  340,  345,  109,  102,  280,   60,  515,
 /*   560 */    60,   60,  512,  517,  516,  514,  513,   94,  485,  399,
 /*   570 */    36,   17,  179,  252,  292,  291,  290,  289,  288,  287,
 /*   580 */   170,  286,  284,  515,   60,  124,  512,  517,  516,  514,
 /*   590 */   513,   60,  222,   60,  223,  219,  218,  216,   14,  339,
 /*   600 */   135,   12,   31,  476,  150,  144,   60,  450,  145,  467,
 /*   610 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*   620 */   462,   27,  133,  312,  279,  352,   35,  340,  345,  109,
 /*   630 */   102,   10,  515,    9,  276,  512,  517,  516,  514,  513,
 /*   640 */    60,   94,  275,  399,  131,  308,  274,  252,   29,  420,
 /*   650 */   138,  273,  484,   68,  170,   67,   65,  515,   64,  448,
 /*   660 */   512,  517,  516,  514,  513,  262,  350,  185,  223,  219,
 /*   670 */   218,  216,  184,  339,  135,  209,  260,  476,  150,  259,
 /*   680 */     6,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*   690 */   271,  269,  267,  189,  462,  352,  435,  340,  345,  109,
 /*   700 */   102,  258,  515,  257,  433,  512,  517,  516,  514,  513,
 /*   710 */    60,   94,  263,  399,  419,   89,  389,  252,  436,  385,
 /*   720 */   381,  380,  379,  244,  170,  377,  243,  515,  136,  239,
 /*   730 */   512,  517,  516,  514,  513,  153,  217,  240,  223,  219,
 /*   740 */   218,  216,  151,  339,  135,  177,  176,  476,  150,  141,
 /*   750 */   234,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*   760 */   271,  269,  267,  189,  462,  232,  358,  357,  230,  352,
 /*   770 */   356,  340,  345,  109,  102,  228,  515,  355,  226,  512,
 /*   780 */   517,  516,  514,  513,  224,   94,   22,  399,  354,  353,
 /*   790 */    44,  252,  221,   43,  351,   42,   19,   41,  170,  120,
 /*   800 */     2,  515,  212,   88,  512,  517,  516,  514,  513,  316,
 /*   810 */   349,   93,  223,  219,  218,  216,  107,  339,  135,  106,
 /*   820 */   305,  476,  150,  303,  204,  450,  145,  467,  157,  199,
 /*   830 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  352,
 /*   840 */    87,  340,  345,  109,  102,  300,  520,  296,  483,   38,
 /*   850 */   439,  256,  431,  430,   27,   94,  255,  399,  253,  388,
 /*   860 */   250,  252,  474,  337,  372,   23,  317,    1,  170,  313,
 /*   870 */   311,  515,  521,  309,  512,  517,  516,  514,  513,  307,
 /*   880 */   382,  281,  223,  219,  218,  216,  347,  339,  135,  242,
 /*   890 */   184,  476,  150,  158,  299,  450,  145,  467,  157,  199,
 /*   900 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  815,
 /*   910 */   815,  277,  815,  352,  815,  340,  345,  109,  102,  815,
 /*   920 */   815,  815,  815,  815,  815,  815,  815,  815,  815,   94,
 /*   930 */   815,  399,  815,  815,  815,  252,  815,  815,  815,  815,
 /*   940 */   815,  815,  170,  815,  815,  515,  815,  815,  512,  517,
 /*   950 */   516,  514,  513,  815,  335,  815,  223,  219,  218,  216,
 /*   960 */   815,  339,  135,  815,  815,  476,  150,  815,  815,  450,
 /*   970 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*   980 */   267,  189,  462,  352,  815,  340,  345,  109,  102,  815,
 /*   990 */   815,  815,  815,  815,  815,  815,  815,  815,  815,   94,
 /*  1000 */   815,  399,  815,  815,  815,  252,  815,  815,  815,  815,
 /*  1010 */   815,  815,  170,  815,  815,  515,  815,  815,  512,  517,
 /*  1020 */   516,  514,  513,  815,  331,  815,  223,  219,  218,  216,
 /*  1030 */   815,  339,  135,  815,  815,  476,  150,  815,  815,  450,
 /*  1040 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  1050 */   267,  189,  462,  815,  815,  815,  815,  352,  815,  340,
 /*  1060 */   345,  109,  102,  815,  815,  815,  815,  815,  815,  815,
 /*  1070 */   815,  815,  815,   94,  815,  399,  815,  815,  815,  252,
 /*  1080 */   815,  815,  815,  815,  815,  815,  170,  815,  815,  515,
 /*  1090 */   815,  815,  512,  517,  516,  514,  513,  815,  330,  815,
 /*  1100 */   223,  219,  218,  216,  815,  339,  135,  815,  815,  476,
 /*  1110 */   150,  815,  815,  450,  145,  467,  157,  199,  198,  149,
 /*  1120 */   191,  272,  271,  269,  267,  189,  462,  352,  815,  340,
 /*  1130 */   345,  109,  102,  815,  815,  815,  815,  815,  815,  815,
 /*  1140 */   815,  815,  815,   94,  815,  399,  815,  815,  815,  252,
 /*  1150 */   815,  815,  815,  815,  815,  815,  170,  815,  815,  515,
 /*  1160 */   815,  815,  512,  517,  516,  514,  513,  815,  329,  815,
 /*  1170 */   223,  219,  218,  216,  815,  339,  135,  815,  815,  476,
 /*  1180 */   150,  815,  815,  450,  145,  467,  157,  199,  198,  149,
 /*  1190 */   191,  272,  271,  269,  267,  189,  462,  815,  815,  815,
 /*  1200 */   815,  352,  815,  340,  345,  109,  102,  815,  815,  815,
 /*  1210 */   815,  815,  815,  815,  815,  815,  815,   94,  815,  399,
 /*  1220 */   815,  815,  815,  252,  815,  815,  815,  815,  815,  815,
 /*  1230 */   170,  815,  815,  515,  815,  815,  512,  517,  516,  514,
 /*  1240 */   513,  815,  328,  815,  223,  219,  218,  216,  815,  339,
 /*  1250 */   135,  815,  815,  476,  150,  815,  815,  450,  145,  467,
 /*  1260 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  1270 */   462,  352,  815,  340,  345,  109,  102,  815,  815,  815,
 /*  1280 */   815,  815,  815,  815,  815,  815,  815,   94,  815,  399,
 /*  1290 */   815,  815,  815,  252,  815,  815,  815,  815,  815,  815,
 /*  1300 */   170,  815,  815,  515,  815,  815,  512,  517,  516,  514,
 /*  1310 */   513,  815,  327,  815,  223,  219,  218,  216,  815,  339,
 /*  1320 */   135,  815,  815,  476,  150,  815,  815,  450,  145,  467,
 /*  1330 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  1340 */   462,  815,  815,  815,  815,  352,  815,  340,  345,  109,
 /*  1350 */   102,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  1360 */   815,   94,  815,  399,  815,  815,  815,  252,  815,  815,
 /*  1370 */   815,  815,  815,  815,  170,  815,  815,  515,  815,  815,
 /*  1380 */   512,  517,  516,  514,  513,  815,  326,  815,  223,  219,
 /*  1390 */   218,  216,  815,  339,  135,  815,  815,  476,  150,  815,
 /*  1400 */   815,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  1410 */   271,  269,  267,  189,  462,  352,  815,  340,  345,  109,
 /*  1420 */   102,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  1430 */   815,   94,  815,  399,  815,  815,  815,  252,  815,  815,
 /*  1440 */   815,  815,  815,  815,  170,  815,  815,  515,  815,  815,
 /*  1450 */   512,  517,  516,  514,  513,  815,  325,  815,  223,  219,
 /*  1460 */   218,  216,  815,  339,  135,  815,  815,  476,  150,  815,
 /*  1470 */   815,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  1480 */   271,  269,  267,  189,  462,  815,  815,  815,  815,  352,
 /*  1490 */   815,  340,  345,  109,  102,  815,  815,  815,  815,  815,
 /*  1500 */   815,  815,  815,  815,  815,   94,  815,  399,  815,  815,
 /*  1510 */   815,  252,  815,  815,  815,  815,  815,  815,  170,  815,
 /*  1520 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  1530 */   324,  815,  223,  219,  218,  216,  815,  339,  135,  815,
 /*  1540 */   815,  476,  150,  815,  815,  450,  145,  467,  157,  199,
 /*  1550 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  352,
 /*  1560 */   815,  340,  345,  109,  102,  815,  815,  815,  815,  815,
 /*  1570 */   815,  815,  815,  815,  815,   94,  815,  399,  815,  815,
 /*  1580 */   815,  252,  815,  815,  815,  815,  815,  815,  170,  815,
 /*  1590 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  1600 */   323,  815,  223,  219,  218,  216,  815,  339,  135,  815,
 /*  1610 */   815,  476,  150,  815,  815,  450,  145,  467,  157,  199,
 /*  1620 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  815,
 /*  1630 */   815,  815,  815,  352,  815,  340,  345,  109,  102,  815,
 /*  1640 */   815,  815,  815,  815,  815,  815,  815,  815,  815,   94,
 /*  1650 */   815,  399,  815,  815,  815,  252,  815,  815,  815,  815,
 /*  1660 */   815,  815,  170,  815,  815,  515,  815,  815,  512,  517,
 /*  1670 */   516,  514,  513,  815,  322,  815,  223,  219,  218,  216,
 /*  1680 */   815,  339,  135,  815,  815,  476,  150,  815,  815,  450,
 /*  1690 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  1700 */   267,  189,  462,  352,  815,  340,  345,  109,  102,  815,
 /*  1710 */   815,  815,  815,  815,  815,  815,  815,  815,  815,   94,
 /*  1720 */   815,  399,  815,  815,  815,  252,  815,  815,  815,  815,
 /*  1730 */   815,  815,  170,  525,  815,  515,  815,  815,  512,  517,
 /*  1740 */   516,  514,  513,  815,  321,  815,  223,  219,  218,  216,
 /*  1750 */   815,  339,  135,  815,  815,  476,  150,  815,  815,  450,
 /*  1760 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  1770 */   267,  189,  462,  815,  815,  815,  815,   97,  405,  815,
 /*  1780 */   205,  298,  815,  815,  815,  407,  815,  815,  815,  410,
 /*  1790 */   815,  815,  411,  409,  408,  406,  404,  403,  815,  815,
 /*  1800 */   249,  815,  815,  511,  510,  509,  508,  507,  506,  505,
 /*  1810 */   504,  503,  502,  501,  500,  499,  498,  497,  496,  295,
 /*  1820 */   294,  293,  282,  815,  278,   86,   85,  815,  815,  815,
 /*  1830 */   815,   83,   84,   90,   91,  815,  815,  519,   47,  815,
 /*  1840 */   815,  815,  815,  815,  143,  815,  815,   86,   85,  815,
 /*  1850 */   815,  815,  815,   83,   84,   90,   91,  815,  815,  815,
 /*  1860 */    47,  444,  815,  815,  815,  815,  143,  815,  511,  510,
 /*  1870 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  1880 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  1890 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  1900 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  1910 */   482,  278,  815,  815,  815,   86,   85,  481,  480,  479,
 /*  1920 */   478,   83,   84,   90,   91,  815,  815,  815,   47,  815,
 /*  1930 */   815,  342,  815,  815,  143,  815,  815,   86,   85,  481,
 /*  1940 */   480,  479,  478,   83,   84,   90,   91,  815,  815,  815,
 /*  1950 */    47,  815,  815,   34,  815,  815,  143,  815,  511,  510,
 /*  1960 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  1970 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  1980 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  1990 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2000 */   482,  278,  815,  815,  815,   86,   85,  481,  480,  479,
 /*  2010 */   478,   83,   84,   90,   91,  815,  815,  815,   47,   16,
 /*  2020 */   815,  815,  815,  815,  143,  815,  815,   86,   85,  481,
 /*  2030 */   480,  479,  478,   83,   84,   90,   91,  815,  815,  815,
 /*  2040 */    47,   15,  815,  815,  815,  815,  143,  815,  511,  510,
 /*  2050 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  2060 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  2070 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  2080 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2090 */   482,  278,  815,  815,  815,   86,   85,  481,  480,  479,
 /*  2100 */   478,   83,   84,   90,   91,  815,  815,  815,   47,  815,
 /*  2110 */   815,   32,  815,  815,  143,  815,  815,   86,   85,  481,
 /*  2120 */   480,  479,  478,   83,   84,   90,   91,  815,  815,  815,
 /*  2130 */    47,   13,  815,  815,  815,  815,  143,  815,  511,  510,
 /*  2140 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  2150 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  2160 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  2170 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2180 */   482,  278,  815,  815,  815,   86,   85,  481,  480,  479,
 /*  2190 */   478,   83,   84,   90,   91,  815,  815,  815,   47,   11,
 /*  2200 */   815,  815,  815,  815,  143,  815,  815,   86,   85,  481,
 /*  2210 */   480,  479,  478,   83,   84,   90,   91,  815,  815,  815,
 /*  2220 */    47,  815,  815,   28,  815,  815,  143,  815,  511,  510,
 /*  2230 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  2240 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  2250 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  2260 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2270 */   482,  278,  815,  815,  815,   86,   85,  481,  480,  479,
 /*  2280 */   478,   83,   84,   90,   91,  815,  815,  815,   47,    8,
 /*  2290 */   815,  815,  815,  815,  143,  815,  815,   86,   85,  481,
 /*  2300 */   480,  479,  478,   83,   84,   90,   91,  815,  815,  815,
 /*  2310 */    47,    7,  815,  815,  815,  815,  143,  815,  511,  510,
 /*  2320 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  2330 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  2340 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  2350 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2360 */   482,  278,  815,  815,  815,   86,   85,  481,  480,  479,
 /*  2370 */   478,   83,   84,   90,   91,  815,  815,  815,   47,  815,
 /*  2380 */   815,  815,  815,  815,  143,  815,  815,  815,  815,  481,
 /*  2390 */   480,  479,  478,  815,  815,  815,  815,  815,  815,  815,
 /*  2400 */   815,  815,  815,   26,  815,  815,  815,  815,  511,  510,
 /*  2410 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  2420 */   499,  498,  497,  496,  295,  294,  293,  282,  482,  278,
 /*  2430 */   815,   86,   85,  815,  121,  815,  815,   83,   84,   90,
 /*  2440 */    91,  815,  815,   96,   47,  815,  815,  815,  815,  815,
 /*  2450 */   143,  815,   92,  386,  387,  815,  815,  481,  480,  479,
 /*  2460 */   478,  515,  815,  815,  512,  517,  516,  514,  513,   25,
 /*  2470 */   815,  815,  815,  815,  511,  510,  509,  508,  507,  506,
 /*  2480 */   505,  504,  503,  502,  501,  500,  499,  498,  497,  496,
 /*  2490 */   295,  294,  293,  282,  482,  278,  815,   86,   85,  815,
 /*  2500 */   815,  815,  815,   83,   84,   90,   91,  815,  815,  815,
 /*  2510 */    45,  815,  815,  815,  815,  815,  143,  815,  815,  815,
 /*  2520 */   815,  815,  815,  481,  480,  479,  478,   59,   55,   54,
 /*  2530 */    58,   57,   56,   53,   52,   51,   49,   50,  815,  815,
 /*  2540 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  2550 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2560 */   482,  278,  815,  815,  815,  815,  815,  815,   86,   85,
 /*  2570 */   815,  208,  815,  815,   83,   84,   90,   91,  314,   98,
 /*  2580 */   815,   47,  815,  815,  815,  815,  815,  143,  815,  481,
 /*  2590 */   480,  479,  478,  815,  815,  815,  815,  815,  515,  815,
 /*  2600 */   815,  512,  517,  516,  514,  513,  815,  815,  815,  815,
 /*  2610 */   815,  511,  510,  509,  508,  507,  506,  505,  504,  503,
 /*  2620 */   502,  501,  500,  499,  498,  497,  496,  295,  294,  293,
 /*  2630 */   282,  482,  278,  815,  378,  245,  376,  375,  374,  373,
 /*  2640 */   815,  815,  815,  815,  815,  815,  178,  238,  815,  237,
 /*  2650 */   815,  236,  235,  233,  231,  229,  815,  227,  225,  815,
 /*  2660 */   481,  480,  479,  478,   97,  405,  815,  205,  298,  815,
 /*  2670 */   815,  815,  407,  815,  815,  815,  410,  815,  815,  411,
 /*  2680 */   409,  408,  406,  404,  403,  815,  815,  249,  815,  815,
 /*  2690 */   511,  510,  509,  508,  507,  506,  505,  504,  503,  502,
 /*  2700 */   501,  500,  499,  498,  497,  496,  295,  294,  293,  282,
 /*  2710 */   405,  278,  815,  815,  815,  815,  815,  407,  815,  815,
 /*  2720 */   815,  410,  815,  815,  411,  409,  408,  406,  404,  403,
 /*  2730 */   815,  815,  815,  815,  815,  511,  510,  509,  508,  507,
 /*  2740 */   506,  505,  504,  503,  502,  501,  500,  499,  498,  497,
 /*  2750 */   496,  295,  294,  293,  282,   30,  278,  815,  815,  111,
 /*  2760 */   102,  815,  815,  815,  815,  815,  815,  815,  815,  121,
 /*  2770 */   815,   94,  815,  399,  815,  815,  815,  252,   96,  815,
 /*  2780 */   815,  815,  815,  815,  168,  815,  815,  515,  392,  387,
 /*  2790 */   512,  517,  516,  514,  513,  815,  515,  815,  815,  512,
 /*  2800 */   517,  516,  514,  513,  815,  815,  815,  476,  150,  815,
 /*  2810 */   815,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  2820 */   271,  269,  267,  189,  462,  815,  398,  397,  396,  395,
 /*  2830 */   394,  815,  815,  815,  815,  815,  815,  815,  393,  815,
 /*  2840 */   815,  815,  511,  510,  509,  508,  507,  506,  505,  504,
 /*  2850 */   503,  502,  501,  500,  499,  498,  497,  496,  295,  294,
 /*  2860 */   293,  282,  815,  278,  815,  815,  398,  397,  396,  395,
 /*  2870 */   394,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  2880 */   815,  815,  511,  510,  509,  508,  507,  506,  505,  504,
 /*  2890 */   503,  502,  501,  500,  499,  498,  497,  496,  295,  294,
 /*  2900 */   293,  282,  315,  278,  207,  304,  206,  302,  815,  815,
 /*  2910 */   815,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  2920 */   815,  815,  815,  815,  815,  511,  510,  509,  508,  507,
 /*  2930 */   506,  505,  504,  503,  502,  501,  500,  499,  498,  497,
 /*  2940 */   496,  295,  294,  293,  282,  815,  278,  207,  304,  206,
 /*  2950 */   302,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  2960 */   815,  815,  815,  815,  815,  815,  815,  815,  511,  510,
 /*  2970 */   509,  508,  507,  506,  505,  504,  503,  502,  501,  500,
 /*  2980 */   499,  498,  497,  496,  295,  294,  293,  282,  142,  278,
 /*  2990 */   815,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  3000 */   208,  815,  815,  815,  815,  211,  210,  301,   98,  815,
 /*  3010 */   815,  815,  424,  815,  815,  515,  815,  425,  512,  517,
 /*  3020 */   516,  514,  513,  815,  815,  815,  815,  515,  815,  142,
 /*  3030 */   512,  517,  516,  514,  513,  476,  150,  815,  815,  450,
 /*  3040 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  3050 */   267,  189,  462,  254,  815,  815,  515,  182,  815,  512,
 /*  3060 */   517,  516,  514,  513,  815,  815,  815,  815,  815,  815,
 /*  3070 */   142,  815,  815,  815,  815,  815,  476,  150,  815,  815,
 /*  3080 */   450,  145,  467,  157,  199,  198,  149,  191,  272,  271,
 /*  3090 */   269,  267,  189,  462,  254,  815,  815,  515,  183,  815,
 /*  3100 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  3110 */   815,  142,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  3120 */   815,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  3130 */   271,  269,  267,  189,  462,  254,  815,  815,  515,  427,
 /*  3140 */   815,  512,  517,  516,  514,  513,  815,  815,  815,  815,
 /*  3150 */   815,  815,  127,  815,  815,  815,  815,  815,  476,  150,
 /*  3160 */   815,  815,  450,  145,  467,  157,  199,  198,  149,  191,
 /*  3170 */   272,  271,  269,  267,  189,  462,  201,  815,  815,  515,
 /*  3180 */   815,  815,  512,  517,  516,  514,  513,  815,  815,  815,
 /*  3190 */   815,  815,  815,  142,  815,  815,  815,  815,  815,  476,
 /*  3200 */   150,  815,  815,  450,  145,  467,  157,  199,  198,  149,
 /*  3210 */   191,  272,  271,  269,  267,  189,  462,  200,  815,  815,
 /*  3220 */   515,  815,  815,  512,  517,  516,  514,  513,  815,  815,
 /*  3230 */   815,  815,  815,  815,  142,  815,  815,  815,  815,  815,
 /*  3240 */   476,  150,  815,  815,  450,  145,  467,  157,  199,  198,
 /*  3250 */   149,  191,  272,  271,  269,  267,  189,  462,  201,  815,
 /*  3260 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  3270 */   815,  815,  815,  815,  815,  142,  815,  815,  815,  815,
 /*  3280 */   815,  476,  150,  815,  815,  450,  145,  467,  157,  199,
 /*  3290 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  175,
 /*  3300 */   815,  815,  515,  815,  815,  512,  517,  516,  514,  513,
 /*  3310 */   815,  815,  815,  815,  815,  815,  142,  815,  815,  815,
 /*  3320 */   815,  815,  476,  150,  815,  815,  450,  145,  467,  157,
 /*  3330 */   199,  198,  149,  191,  272,  271,  269,  267,  189,  462,
 /*  3340 */   174,  815,  815,  515,  815,  815,  512,  517,  516,  514,
 /*  3350 */   513,  815,  815,  815,  815,  815,  815,  142,  815,  815,
 /*  3360 */   815,  815,  815,  476,  150,  815,  815,  450,  145,  467,
 /*  3370 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  3380 */   462,  173,  815,  815,  515,  815,  815,  512,  517,  516,
 /*  3390 */   514,  513,  815,  815,  815,  815,  815,  815,  142,  815,
 /*  3400 */   815,  815,  815,  815,  476,  150,  815,  815,  450,  145,
 /*  3410 */   467,  157,  199,  198,  149,  191,  272,  271,  269,  267,
 /*  3420 */   189,  462,  172,  815,  815,  515,  815,  815,  512,  517,
 /*  3430 */   516,  514,  513,  815,  815,  815,  815,  815,  815,  142,
 /*  3440 */   815,  815,  815,  815,  815,  476,  150,  815,  815,  450,
 /*  3450 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  3460 */   267,  189,  462,  171,  815,  815,  515,  815,  815,  512,
 /*  3470 */   517,  516,  514,  513,  815,  815,  815,  815,  815,  815,
 /*  3480 */   142,  815,  815,  815,  815,  815,  476,  150,  815,  815,
 /*  3490 */   450,  145,  467,  157,  199,  198,  149,  191,  272,  271,
 /*  3500 */   269,  267,  189,  462,  202,  815,  815,  515,  815,  815,
 /*  3510 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  3520 */   142,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  3530 */   815,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  3540 */   271,  269,  267,  189,  462,  815,  815,  515,  815,  815,
 /*  3550 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  3560 */   815,  142,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  3570 */   187,  441,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  3580 */   271,  269,  267,  189,  462,  169,  815,  815,  515,  815,
 /*  3590 */   815,  512,  517,  516,  514,  513,  815,  815,  815,  815,
 /*  3600 */   815,  815,  142,  815,  815,  815,  815,  815,  476,  150,
 /*  3610 */   815,  815,  450,  145,  467,  157,  199,  198,  149,  191,
 /*  3620 */   272,  271,  269,  267,  189,  462,  167,  815,  815,  515,
 /*  3630 */   815,  815,  512,  517,  516,  514,  513,  815,  815,  815,
 /*  3640 */   815,  815,  815,  142,  815,  815,  815,  815,  815,  476,
 /*  3650 */   150,  815,  815,  450,  145,  467,  157,  199,  198,  149,
 /*  3660 */   191,  272,  271,  269,  267,  189,  462,  166,  815,  815,
 /*  3670 */   515,  815,  815,  512,  517,  516,  514,  513,  815,  815,
 /*  3680 */   815,  815,  815,  815,  142,  815,  815,  815,  815,  815,
 /*  3690 */   476,  150,  815,  815,  450,  145,  467,  157,  199,  198,
 /*  3700 */   149,  191,  272,  271,  269,  267,  189,  462,  165,  815,
 /*  3710 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  3720 */   815,  815,  815,  815,  815,  142,  815,  815,  815,  815,
 /*  3730 */   815,  476,  150,  815,  815,  450,  145,  467,  157,  199,
 /*  3740 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  163,
 /*  3750 */   815,  815,  515,  815,  815,  512,  517,  516,  514,  513,
 /*  3760 */   815,  815,  815,  815,  815,  815,  142,  815,  815,  815,
 /*  3770 */   815,  815,  476,  150,  815,  815,  450,  145,  467,  157,
 /*  3780 */   199,  198,  149,  191,  272,  271,  269,  267,  189,  462,
 /*  3790 */   164,  815,  815,  515,  815,  815,  512,  517,  516,  514,
 /*  3800 */   513,  815,  815,  815,  815,  815,  815,  142,  815,  815,
 /*  3810 */   815,  815,  815,  476,  150,  815,  815,  450,  145,  467,
 /*  3820 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  3830 */   462,  162,  815,  815,  515,  815,  815,  512,  517,  516,
 /*  3840 */   514,  513,  815,  815,  815,  815,  815,  815,  142,  815,
 /*  3850 */   815,  815,  815,  815,  476,  150,  815,  815,  450,  145,
 /*  3860 */   467,  157,  199,  198,  149,  191,  272,  271,  269,  267,
 /*  3870 */   189,  462,  161,  815,  815,  515,  815,  815,  512,  517,
 /*  3880 */   516,  514,  513,  815,  815,  815,  815,  815,  815,  142,
 /*  3890 */   815,  815,  815,  815,  815,  476,  150,  815,  815,  450,
 /*  3900 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  3910 */   267,  189,  462,  160,  815,  815,  515,  815,  815,  512,
 /*  3920 */   517,  516,  514,  513,  815,  815,  815,  815,  815,  815,
 /*  3930 */   142,  815,  815,  815,  815,  815,  476,  150,  815,  815,
 /*  3940 */   450,  145,  467,  157,  199,  198,  149,  191,  272,  271,
 /*  3950 */   269,  267,  189,  462,  159,  815,  815,  515,  815,  815,
 /*  3960 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  3970 */   142,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  3980 */   815,  450,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  3990 */   271,  269,  267,  189,  462,  815,  815,  515,  815,  815,
 /*  4000 */   512,  517,  516,  514,  513,  815,  815,  815,  142,  815,
 /*  4010 */   815,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  4020 */   815,  440,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  4030 */   271,  269,  267,  189,  462,  515,  815,  815,  512,  517,
 /*  4040 */   516,  514,  513,  815,  815,  815,  142,  815,  815,  815,
 /*  4050 */   815,  815,  815,  815,  815,  476,  150,  815,  815,  461,
 /*  4060 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  4070 */   267,  189,  462,  515,  815,  815,  512,  517,  516,  514,
 /*  4080 */   513,  815,  815,  815,  142,  815,  815,  815,  815,  815,
 /*  4090 */   815,  815,  815,  476,  150,  815,  815,  460,  145,  467,
 /*  4100 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  4110 */   462,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  4120 */   815,  815,  142,  815,  815,  815,  815,  815,  815,  815,
 /*  4130 */   815,  476,  150,  815,  815,  459,  145,  467,  157,  199,
 /*  4140 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  515,
 /*  4150 */   815,  815,  512,  517,  516,  514,  513,  815,  815,  815,
 /*  4160 */   142,  815,  815,  815,  815,  815,  815,  815,  815,  476,
 /*  4170 */   150,  815,  815,  458,  145,  467,  157,  199,  198,  149,
 /*  4180 */   191,  272,  271,  269,  267,  189,  462,  515,  815,  815,
 /*  4190 */   512,  517,  516,  514,  513,  815,  815,  815,  142,  815,
 /*  4200 */   815,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  4210 */   815,  457,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  4220 */   271,  269,  267,  189,  462,  515,  815,  815,  512,  517,
 /*  4230 */   516,  514,  513,  815,  815,  815,  142,  815,  815,  815,
 /*  4240 */   815,  815,  815,  815,  815,  476,  150,  815,  815,  456,
 /*  4250 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  4260 */   267,  189,  462,  515,  815,  815,  512,  517,  516,  514,
 /*  4270 */   513,  815,  815,  815,  142,  815,  815,  815,  815,  815,
 /*  4280 */   815,  815,  815,  476,  150,  815,  815,  455,  145,  467,
 /*  4290 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  4300 */   462,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  4310 */   815,  815,  142,  815,  815,  815,  815,  815,  815,  815,
 /*  4320 */   815,  476,  150,  815,  815,  454,  145,  467,  157,  199,
 /*  4330 */   198,  149,  191,  272,  271,  269,  267,  189,  462,  515,
 /*  4340 */   815,  815,  512,  517,  516,  514,  513,  815,  815,  815,
 /*  4350 */   142,  815,  815,  815,  815,  815,  815,  815,  815,  476,
 /*  4360 */   150,  815,  815,  453,  145,  467,  157,  199,  198,  149,
 /*  4370 */   191,  272,  271,  269,  267,  189,  462,  515,  815,  815,
 /*  4380 */   512,  517,  516,  514,  513,  815,  815,  815,  142,  815,
 /*  4390 */   815,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  4400 */   815,  452,  145,  467,  157,  199,  198,  149,  191,  272,
 /*  4410 */   271,  269,  267,  189,  462,  515,  815,  815,  512,  517,
 /*  4420 */   516,  514,  513,  815,  815,  815,  142,  815,  815,  815,
 /*  4430 */   815,  815,  815,  815,  815,  476,  150,  815,  815,  451,
 /*  4440 */   145,  467,  157,  199,  198,  149,  191,  272,  271,  269,
 /*  4450 */   267,  189,  462,  515,  815,  815,  512,  517,  516,  514,
 /*  4460 */   513,  815,  815,  815,  815,  815,   99,  815,  815,  815,
 /*  4470 */   815,  815,  815,  476,  150,  815,  815,  442,  145,  467,
 /*  4480 */   157,  199,  198,  149,  191,  272,  271,  269,  267,  189,
 /*  4490 */   462,  511,  510,  509,  508,  507,  506,  505,  504,  503,
 /*  4500 */   502,  501,  500,  499,  498,  497,  496,  295,  294,  293,
 /*  4510 */   282,  815,  278,  815,  205,  815,  815,  815,  815,  815,
 /*  4520 */   815,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  4530 */   142,  815,  815,  815,  815,  815,  815,  511,  510,  509,
 /*  4540 */   508,  507,  506,  505,  504,  503,  502,  501,  500,  499,
 /*  4550 */   498,  497,  496,  295,  294,  293,  282,  515,  278,  815,
 /*  4560 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  4570 */   815,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  4580 */   815,  815,  468,  467,  157,  199,  198,  149,  191,  272,
 /*  4590 */   271,  269,  267,  189,  463,  815,  815,  815,  815,  815,
 /*  4600 */   815,  511,  510,  509,  508,  507,  506,  505,  504,  503,
 /*  4610 */   502,  501,  500,  499,  498,  497,  496,  295,  294,  293,
 /*  4620 */   282,  142,  278,  815,  814,   66,  297,  523,  128,  815,
 /*  4630 */   319,  108,  110,  815,   95,  129,  815,  815,  815,  815,
 /*  4640 */   815,  815,  815,   94,  815,  399,  815,  815,  515,  252,
 /*  4650 */   815,  512,  517,  516,  514,  513,  815,  815,  815,  515,
 /*  4660 */   815,  815,  512,  517,  516,  514,  513,  815,  476,  150,
 /*  4670 */   142,  815,  815,  468,  467,  157,  199,  198,  149,  191,
 /*  4680 */   272,  271,  269,  267,  188,  815,  815,  815,  815,  815,
 /*  4690 */   815,  815,  815,  815,  815,  815,  815,  515,  815,  815,
 /*  4700 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  4710 */   142,  815,  815,  815,  815,  815,  815,  476,  150,  815,
 /*  4720 */   116,  815,  468,  467,  157,  199,  198,  149,  191,  272,
 /*  4730 */   271,  269,  265,  412,  815,  815,  815,  515,  815,  815,
 /*  4740 */   512,  517,  516,  514,  513,  142,  815,  515,  815,  815,
 /*  4750 */   512,  517,  516,  514,  513,  815,  815,  476,  150,  815,
 /*  4760 */   815,  815,  468,  467,  157,  199,  198,  149,  191,  272,
 /*  4770 */   271,  266,  515,  815,  815,  512,  517,  516,  514,  513,
 /*  4780 */   815,  142,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  4790 */   815,  815,  476,  150,  815,  815,  815,  468,  467,  157,
 /*  4800 */   199,  198,  149,  191,  272,  268,  815,  815,  515,  815,
 /*  4810 */   815,  512,  517,  516,  514,  513,  815,  815,  815,  815,
 /*  4820 */   815,  815,  815,  815,  815,  815,  815,  815,  476,  150,
 /*  4830 */   815,  815,  815,  468,  467,  157,  199,  198,  149,  191,
 /*  4840 */   270,  815,  815,  815,  524,  523,  128,  815,  319,  108,
 /*  4850 */   110,  815,   95,  129,  815,  815,  815,  815,  142,  815,
 /*  4860 */   815,   94,  815,  399,  815,  815,  815,  252,  815,  815,
 /*  4870 */   142,  815,  815,  815,  815,  815,  815,  515,  815,  815,
 /*  4880 */   512,  517,  516,  514,  513,  515,  815,  815,  512,  517,
 /*  4890 */   516,  514,  513,  815,  815,  815,  815,  515,  815,  815,
 /*  4900 */   512,  517,  516,  514,  513,  476,  150,  815,  142,  815,
 /*  4910 */   468,  467,  157,  199,  198,  149,  190,  476,  150,  815,
 /*  4920 */   815,  815,  468,  467,  157,  199,  198,  148,  815,  815,
 /*  4930 */   815,  815,  815,  815,  815,  515,  815,  815,  512,  517,
 /*  4940 */   516,  514,  513,  815,  142,  815,  815,  815,  815,  815,
 /*  4950 */   815,  815,  815,  815,  815,  476,  150,  815,  815,  815,
 /*  4960 */   468,  467,  157,  199,  198,  147,  815,  815,  815,  815,
 /*  4970 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  142,
 /*  4980 */   815,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  4990 */   815,  476,  150,  815,  815,  815,  468,  467,  157,  199,
 /*  5000 */   195,  815,  815,  815,  815,  815,  515,  815,  815,  512,
 /*  5010 */   517,  516,  514,  513,  142,  815,  815,  815,  815,  815,
 /*  5020 */   815,  815,  815,  815,  142,  815,  476,  150,  815,  815,
 /*  5030 */   815,  468,  467,  157,  199,  194,  815,  815,  815,  815,
 /*  5040 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  5050 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  142,
 /*  5060 */   815,  476,  150,  815,  815,  815,  468,  467,  157,  199,
 /*  5070 */   193,  476,  150,  815,  815,  815,  468,  467,  157,  199,
 /*  5080 */   192,  815,  815,  815,  815,  815,  515,  815,  142,  512,
 /*  5090 */   517,  516,  514,  513,  815,  815,  815,  142,  815,  815,
 /*  5100 */   815,  815,  815,  815,  815,  815,  476,  150,  815,  815,
 /*  5110 */   815,  468,  467,  157,  197,  515,  815,  815,  512,  517,
 /*  5120 */   516,  514,  513,  815,  515,  142,  815,  512,  517,  516,
 /*  5130 */   514,  513,  815,  815,  815,  476,  150,  815,  815,  815,
 /*  5140 */   468,  467,  157,  196,  476,  150,  815,  815,  815,  468,
 /*  5150 */   467,  156,  515,  815,  815,  512,  517,  516,  514,  513,
 /*  5160 */   208,  815,  815,  815,  815,  203,  210,  301,   98,  815,
 /*  5170 */   815,  815,  476,  150,  142,  815,  815,  468,  467,  155,
 /*  5180 */   815,  815,  815,  815,  142,  815,  815,  515,  815,  815,
 /*  5190 */   512,  517,  516,  514,  513,  815,  815,  815,  815,  815,
 /*  5200 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  5210 */   815,  515,  142,  815,  512,  517,  516,  514,  513,  815,
 /*  5220 */   815,  476,  150,  142,  815,  815,  468,  472,  815,  815,
 /*  5230 */   815,  476,  150,  815,  815,  815,  468,  471,  815,  515,
 /*  5240 */   815,  815,  512,  517,  516,  514,  513,  142,  815,  815,
 /*  5250 */   515,  815,  815,  512,  517,  516,  514,  513,  142,  476,
 /*  5260 */   150,  815,  815,  815,  468,  470,  815,  815,  815,  815,
 /*  5270 */   476,  150,  815,  815,  515,  468,  469,  512,  517,  516,
 /*  5280 */   514,  513,  142,  815,  815,  515,  815,  815,  512,  517,
 /*  5290 */   516,  514,  513,  142,  476,  150,  815,  815,  815,  468,
 /*  5300 */   466,  815,  815,  815,  815,  476,  150,  815,  815,  515,
 /*  5310 */   468,  465,  512,  517,  516,  514,  513,  142,  815,  815,
 /*  5320 */   515,  815,  815,  512,  517,  516,  514,  513,  815,  476,
 /*  5330 */   150,  815,  815,  815,  468,  464,  815,  815,  815,  815,
 /*  5340 */   476,  150,  815,  815,  515,  468,  445,  512,  517,  516,
 /*  5350 */   514,  513,  815,  815,  142,  815,  815,  815,  815,  815,
 /*  5360 */   815,  815,  815,  815,  476,  150,  815,  815,  815,  449,
 /*  5370 */   815,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  5380 */   815,  515,  815,  815,  512,  517,  516,  514,  513,  815,
 /*  5390 */   815,  815,  815,  815,  815,  815,  815,  815,  815,  815,
 /*  5400 */   815,  476,  150,  815,  815,  815,  473,
};
static const YYCODETYPE yy_lookahead[] = {
 /*     0 */    27,   28,   35,   36,   37,   38,   33,   34,   35,   36,
 /*    10 */    38,   38,   40,   40,    2,  142,   43,   44,   45,   46,
 /*    20 */   147,  148,   29,   30,   31,   52,   66,  154,  155,   56,
 /*    30 */    27,   28,   59,   60,   61,   62,   63,   64,   65,   66,
 /*    40 */    67,   83,    1,   70,   71,   72,   73,   74,   75,   76,
 /*    50 */    77,   78,   79,   80,   81,   82,   83,   84,   85,   86,
 /*    60 */    87,   88,   89,   90,   91,   53,   54,   55,   56,   57,
 /*    70 */    58,   98,   99,  100,  101,  102,  103,  117,  118,  106,
 /*    80 */    39,  108,   21,   22,   23,   24,   38,  114,   83,  133,
 /*    90 */    27,   28,  119,  120,  121,  122,   33,   34,   35,   36,
 /*   100 */   144,   38,  146,   40,    2,  142,   43,   44,   45,   46,
 /*   110 */   108,  148,   19,   20,  148,   52,  114,  154,  155,   56,
 /*   120 */   154,  155,   59,   60,   61,   62,   63,   64,   65,   66,
 /*   130 */    67,   25,   26,   70,   71,   72,   73,   74,   75,   76,
 /*   140 */    77,   78,   79,   80,   81,   82,   83,   84,   85,   86,
 /*   150 */    87,   88,   89,   90,   91,   21,   54,   55,   56,   57,
 /*   160 */    58,   98,   99,  100,  101,  102,  103,  101,  102,  106,
 /*   170 */   136,  108,  106,  101,  102,  141,  142,  114,  106,  133,
 /*   180 */    27,   28,  119,  120,  121,  122,   33,   34,   35,   36,
 /*   190 */   144,   38,  146,   40,  142,  148,   43,   44,   45,   46,
 /*   200 */   148,  154,  155,   23,  148,   52,  154,  155,  133,   56,
 /*   210 */   154,  155,   59,   60,   61,   62,   63,   64,   65,  144,
 /*   220 */    67,  146,    1,   70,   71,   72,   73,   74,   75,   76,
 /*   230 */    77,   78,   79,   80,   81,   82,   83,   84,   85,   86,
 /*   240 */    87,   88,   89,   90,   91,   14,   46,  157,  158,  159,
 /*   250 */    23,   98,   99,  100,  101,  102,  103,  148,    1,  106,
 /*   260 */   132,  108,   41,  154,  155,   13,   14,  114,   68,   69,
 /*   270 */    27,   28,  119,  120,  121,  122,   33,   34,   35,   36,
 /*   280 */   152,   37,    1,   40,   53,   41,   43,  159,   45,   46,
 /*   290 */   162,  163,  164,  165,  166,   52,  101,  102,  133,   56,
 /*   300 */    43,  106,   59,   60,   61,   62,   63,   64,    1,  144,
 /*   310 */    67,  146,    1,   70,   71,   72,   73,   74,   75,   76,
 /*   320 */    77,   78,   79,   80,   81,   82,   83,   84,   85,   86,
 /*   330 */    87,   88,   89,   90,   91,   68,   69,  127,  136,  129,
 /*   340 */   130,  131,  132,  141,  142,    1,  181,   66,   41,    1,
 /*   350 */    39,   39,   40,  143,  108,  145,   23,   23,   43,  149,
 /*   360 */   114,   46,  119,  120,  121,  122,  156,    1,    1,  159,
 /*   370 */    40,   41,  162,  163,  164,  165,  166,  167,  168,    1,
 /*   380 */   170,  171,  172,  173,    1,  175,  176,   43,    1,  179,
 /*   390 */   180,   43,    1,  183,  184,  185,  186,  187,  188,  189,
 /*   400 */   190,  191,  192,  193,  194,  195,  196,  127,   41,  129,
 /*   410 */   130,  131,  132,   79,   80,   81,   82,   83,   84,   85,
 /*   420 */    86,   87,    1,  143,  133,  145,   43,   43,   41,  149,
 /*   430 */    23,   53,   66,   23,   43,   40,  156,  146,    1,  159,
 /*   440 */     1,   46,  162,  163,  164,  165,  166,  167,  168,   65,
 /*   450 */   170,  171,  172,  173,   43,  175,  176,   46,    2,  179,
 /*   460 */   180,    2,   41,  183,  184,  185,  186,  187,  188,  189,
 /*   470 */   190,  191,  192,  193,  194,  195,  196,   21,   41,   83,
 /*   480 */    21,  127,   43,  129,  130,  131,  132,   79,   80,   81,
 /*   490 */    82,   83,   84,   85,   86,   87,  136,  143,   23,  145,
 /*   500 */   174,   23,  142,  149,  178,  117,  118,  147,  148,   53,
 /*   510 */   156,   83,   53,  159,  154,  155,  162,  163,  164,  165,
 /*   520 */   166,  167,  168,   23,  170,  171,  172,  173,    1,  175,
 /*   530 */   176,    1,  132,  179,  180,  135,   21,  183,  184,  185,
 /*   540 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*   550 */   196,  127,  133,  129,  130,  131,  132,    1,    1,  159,
 /*   560 */     1,    1,  162,  163,  164,  165,  166,  143,   23,  145,
 /*   570 */    43,   41,  153,  149,   79,   80,   81,   82,   83,   84,
 /*   580 */   156,   86,   87,  159,    1,   21,  162,  163,  164,  165,
 /*   590 */   166,    1,  168,    1,  170,  171,  172,  173,   41,  175,
 /*   600 */   176,   41,   43,  179,  180,  132,    1,  183,  184,  185,
 /*   610 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*   620 */   196,    2,  141,  142,   90,  127,   43,  129,  130,  131,
 /*   630 */   132,   41,  159,   41,    1,  162,  163,  164,  165,  166,
 /*   640 */     1,  143,   90,  145,  141,  142,    1,  149,   43,  158,
 /*   650 */   159,   90,   23,   18,  156,   17,   16,  159,   15,   46,
 /*   660 */   162,  163,  164,  165,  166,   40,  168,   46,  170,  171,
 /*   670 */   172,  173,   53,  175,  176,  132,   46,  179,  180,   69,
 /*   680 */    41,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*   690 */   192,  193,  194,  195,  196,  127,   41,  129,  130,  131,
 /*   700 */   132,   40,  159,   46,   41,  162,  163,  164,  165,  166,
 /*   710 */     1,  143,   68,  145,   43,   65,   46,  149,   46,   46,
 /*   720 */    43,   43,   43,   40,  156,   41,   90,  159,   39,   41,
 /*   730 */   162,  163,  164,  165,  166,   39,  168,   90,  170,  171,
 /*   740 */   172,  173,   39,  175,  176,   39,   39,  179,  180,  132,
 /*   750 */    39,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*   760 */   192,  193,  194,  195,  196,   39,  108,  108,   39,  127,
 /*   770 */   108,  129,  130,  131,  132,   39,  159,  108,   39,  162,
 /*   780 */   163,  164,  165,  166,   39,  143,  101,  145,  114,  114,
 /*   790 */    40,  149,  102,   40,   43,   40,   42,   40,  156,   65,
 /*   800 */    53,  159,   46,   40,  162,  163,  164,  165,  166,   41,
 /*   810 */   168,    1,  170,  171,  172,  173,   46,  175,  176,   46,
 /*   820 */    51,  179,  180,   49,   46,  183,  184,  185,  186,  187,
 /*   830 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  127,
 /*   840 */    40,  129,  130,  131,  132,   41,  133,   46,  181,   40,
 /*   850 */   133,   53,  155,  154,    2,  143,   53,  145,  142,  136,
 /*   860 */    53,  149,  181,  177,  129,   40,  136,   65,  156,  142,
 /*   870 */   142,  159,  129,  142,  162,  163,  164,  165,  166,  142,
 /*   880 */   168,  159,  170,  171,  172,  173,  178,  175,  176,  169,
 /*   890 */    53,  179,  180,   46,  136,  183,  184,  185,  186,  187,
 /*   900 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  197,
 /*   910 */   197,  159,  197,  127,  197,  129,  130,  131,  132,  197,
 /*   920 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  143,
 /*   930 */   197,  145,  197,  197,  197,  149,  197,  197,  197,  197,
 /*   940 */   197,  197,  156,  197,  197,  159,  197,  197,  162,  163,
 /*   950 */   164,  165,  166,  197,  168,  197,  170,  171,  172,  173,
 /*   960 */   197,  175,  176,  197,  197,  179,  180,  197,  197,  183,
 /*   970 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*   980 */   194,  195,  196,  127,  197,  129,  130,  131,  132,  197,
 /*   990 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  143,
 /*  1000 */   197,  145,  197,  197,  197,  149,  197,  197,  197,  197,
 /*  1010 */   197,  197,  156,  197,  197,  159,  197,  197,  162,  163,
 /*  1020 */   164,  165,  166,  197,  168,  197,  170,  171,  172,  173,
 /*  1030 */   197,  175,  176,  197,  197,  179,  180,  197,  197,  183,
 /*  1040 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  1050 */   194,  195,  196,  197,  197,  197,  197,  127,  197,  129,
 /*  1060 */   130,  131,  132,  197,  197,  197,  197,  197,  197,  197,
 /*  1070 */   197,  197,  197,  143,  197,  145,  197,  197,  197,  149,
 /*  1080 */   197,  197,  197,  197,  197,  197,  156,  197,  197,  159,
 /*  1090 */   197,  197,  162,  163,  164,  165,  166,  197,  168,  197,
 /*  1100 */   170,  171,  172,  173,  197,  175,  176,  197,  197,  179,
 /*  1110 */   180,  197,  197,  183,  184,  185,  186,  187,  188,  189,
 /*  1120 */   190,  191,  192,  193,  194,  195,  196,  127,  197,  129,
 /*  1130 */   130,  131,  132,  197,  197,  197,  197,  197,  197,  197,
 /*  1140 */   197,  197,  197,  143,  197,  145,  197,  197,  197,  149,
 /*  1150 */   197,  197,  197,  197,  197,  197,  156,  197,  197,  159,
 /*  1160 */   197,  197,  162,  163,  164,  165,  166,  197,  168,  197,
 /*  1170 */   170,  171,  172,  173,  197,  175,  176,  197,  197,  179,
 /*  1180 */   180,  197,  197,  183,  184,  185,  186,  187,  188,  189,
 /*  1190 */   190,  191,  192,  193,  194,  195,  196,  197,  197,  197,
 /*  1200 */   197,  127,  197,  129,  130,  131,  132,  197,  197,  197,
 /*  1210 */   197,  197,  197,  197,  197,  197,  197,  143,  197,  145,
 /*  1220 */   197,  197,  197,  149,  197,  197,  197,  197,  197,  197,
 /*  1230 */   156,  197,  197,  159,  197,  197,  162,  163,  164,  165,
 /*  1240 */   166,  197,  168,  197,  170,  171,  172,  173,  197,  175,
 /*  1250 */   176,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  1260 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  1270 */   196,  127,  197,  129,  130,  131,  132,  197,  197,  197,
 /*  1280 */   197,  197,  197,  197,  197,  197,  197,  143,  197,  145,
 /*  1290 */   197,  197,  197,  149,  197,  197,  197,  197,  197,  197,
 /*  1300 */   156,  197,  197,  159,  197,  197,  162,  163,  164,  165,
 /*  1310 */   166,  197,  168,  197,  170,  171,  172,  173,  197,  175,
 /*  1320 */   176,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  1330 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  1340 */   196,  197,  197,  197,  197,  127,  197,  129,  130,  131,
 /*  1350 */   132,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  1360 */   197,  143,  197,  145,  197,  197,  197,  149,  197,  197,
 /*  1370 */   197,  197,  197,  197,  156,  197,  197,  159,  197,  197,
 /*  1380 */   162,  163,  164,  165,  166,  197,  168,  197,  170,  171,
 /*  1390 */   172,  173,  197,  175,  176,  197,  197,  179,  180,  197,
 /*  1400 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  1410 */   192,  193,  194,  195,  196,  127,  197,  129,  130,  131,
 /*  1420 */   132,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  1430 */   197,  143,  197,  145,  197,  197,  197,  149,  197,  197,
 /*  1440 */   197,  197,  197,  197,  156,  197,  197,  159,  197,  197,
 /*  1450 */   162,  163,  164,  165,  166,  197,  168,  197,  170,  171,
 /*  1460 */   172,  173,  197,  175,  176,  197,  197,  179,  180,  197,
 /*  1470 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  1480 */   192,  193,  194,  195,  196,  197,  197,  197,  197,  127,
 /*  1490 */   197,  129,  130,  131,  132,  197,  197,  197,  197,  197,
 /*  1500 */   197,  197,  197,  197,  197,  143,  197,  145,  197,  197,
 /*  1510 */   197,  149,  197,  197,  197,  197,  197,  197,  156,  197,
 /*  1520 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  1530 */   168,  197,  170,  171,  172,  173,  197,  175,  176,  197,
 /*  1540 */   197,  179,  180,  197,  197,  183,  184,  185,  186,  187,
 /*  1550 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  127,
 /*  1560 */   197,  129,  130,  131,  132,  197,  197,  197,  197,  197,
 /*  1570 */   197,  197,  197,  197,  197,  143,  197,  145,  197,  197,
 /*  1580 */   197,  149,  197,  197,  197,  197,  197,  197,  156,  197,
 /*  1590 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  1600 */   168,  197,  170,  171,  172,  173,  197,  175,  176,  197,
 /*  1610 */   197,  179,  180,  197,  197,  183,  184,  185,  186,  187,
 /*  1620 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  197,
 /*  1630 */   197,  197,  197,  127,  197,  129,  130,  131,  132,  197,
 /*  1640 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  143,
 /*  1650 */   197,  145,  197,  197,  197,  149,  197,  197,  197,  197,
 /*  1660 */   197,  197,  156,  197,  197,  159,  197,  197,  162,  163,
 /*  1670 */   164,  165,  166,  197,  168,  197,  170,  171,  172,  173,
 /*  1680 */   197,  175,  176,  197,  197,  179,  180,  197,  197,  183,
 /*  1690 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  1700 */   194,  195,  196,  127,  197,  129,  130,  131,  132,  197,
 /*  1710 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  143,
 /*  1720 */   197,  145,  197,  197,  197,  149,  197,  197,  197,  197,
 /*  1730 */   197,  197,  156,    0,  197,  159,  197,  197,  162,  163,
 /*  1740 */   164,  165,  166,  197,  168,  197,  170,  171,  172,  173,
 /*  1750 */   197,  175,  176,  197,  197,  179,  180,  197,  197,  183,
 /*  1760 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  1770 */   194,  195,  196,  197,  197,  197,  197,   44,   45,  197,
 /*  1780 */    47,   48,  197,  197,  197,   52,  197,  197,  197,   56,
 /*  1790 */   197,  197,   59,   60,   61,   62,   63,   64,  197,  197,
 /*  1800 */    67,  197,  197,   70,   71,   72,   73,   74,   75,   76,
 /*  1810 */    77,   78,   79,   80,   81,   82,   83,   84,   85,   86,
 /*  1820 */    87,   88,   89,  197,   91,   27,   28,  197,  197,  197,
 /*  1830 */   197,   33,   34,   35,   36,  197,  197,   39,   40,  197,
 /*  1840 */   197,  197,  197,  197,   46,  197,  197,   27,   28,  197,
 /*  1850 */   197,  197,  197,   33,   34,   35,   36,  197,  197,  197,
 /*  1860 */    40,   41,  197,  197,  197,  197,   46,  197,   70,   71,
 /*  1870 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  1880 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  1890 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  1900 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  1910 */    90,   91,  197,  197,  197,   27,   28,  119,  120,  121,
 /*  1920 */   122,   33,   34,   35,   36,  197,  197,  197,   40,  197,
 /*  1930 */   197,   43,  197,  197,   46,  197,  197,   27,   28,  119,
 /*  1940 */   120,  121,  122,   33,   34,   35,   36,  197,  197,  197,
 /*  1950 */    40,  197,  197,   43,  197,  197,   46,  197,   70,   71,
 /*  1960 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  1970 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  1980 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  1990 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2000 */    90,   91,  197,  197,  197,   27,   28,  119,  120,  121,
 /*  2010 */   122,   33,   34,   35,   36,  197,  197,  197,   40,   41,
 /*  2020 */   197,  197,  197,  197,   46,  197,  197,   27,   28,  119,
 /*  2030 */   120,  121,  122,   33,   34,   35,   36,  197,  197,  197,
 /*  2040 */    40,   41,  197,  197,  197,  197,   46,  197,   70,   71,
 /*  2050 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  2060 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  2070 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  2080 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2090 */    90,   91,  197,  197,  197,   27,   28,  119,  120,  121,
 /*  2100 */   122,   33,   34,   35,   36,  197,  197,  197,   40,  197,
 /*  2110 */   197,   43,  197,  197,   46,  197,  197,   27,   28,  119,
 /*  2120 */   120,  121,  122,   33,   34,   35,   36,  197,  197,  197,
 /*  2130 */    40,   41,  197,  197,  197,  197,   46,  197,   70,   71,
 /*  2140 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  2150 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  2160 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  2170 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2180 */    90,   91,  197,  197,  197,   27,   28,  119,  120,  121,
 /*  2190 */   122,   33,   34,   35,   36,  197,  197,  197,   40,   41,
 /*  2200 */   197,  197,  197,  197,   46,  197,  197,   27,   28,  119,
 /*  2210 */   120,  121,  122,   33,   34,   35,   36,  197,  197,  197,
 /*  2220 */    40,  197,  197,   43,  197,  197,   46,  197,   70,   71,
 /*  2230 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  2240 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  2250 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  2260 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2270 */    90,   91,  197,  197,  197,   27,   28,  119,  120,  121,
 /*  2280 */   122,   33,   34,   35,   36,  197,  197,  197,   40,   41,
 /*  2290 */   197,  197,  197,  197,   46,  197,  197,   27,   28,  119,
 /*  2300 */   120,  121,  122,   33,   34,   35,   36,  197,  197,  197,
 /*  2310 */    40,   41,  197,  197,  197,  197,   46,  197,   70,   71,
 /*  2320 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  2330 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  2340 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  2350 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2360 */    90,   91,  197,  197,  197,   27,   28,  119,  120,  121,
 /*  2370 */   122,   33,   34,   35,   36,  197,  197,  197,   40,  197,
 /*  2380 */   197,  197,  197,  197,   46,  197,  197,  197,  197,  119,
 /*  2390 */   120,  121,  122,  197,  197,  197,  197,  197,  197,  197,
 /*  2400 */   197,  197,  197,   65,  197,  197,  197,  197,   70,   71,
 /*  2410 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  2420 */    82,   83,   84,   85,   86,   87,   88,   89,   90,   91,
 /*  2430 */   197,   27,   28,  197,  132,  197,  197,   33,   34,   35,
 /*  2440 */    36,  197,  197,  141,   40,  197,  197,  197,  197,  197,
 /*  2450 */    46,  197,  150,  151,  152,  197,  197,  119,  120,  121,
 /*  2460 */   122,  159,  197,  197,  162,  163,  164,  165,  166,   65,
 /*  2470 */   197,  197,  197,  197,   70,   71,   72,   73,   74,   75,
 /*  2480 */    76,   77,   78,   79,   80,   81,   82,   83,   84,   85,
 /*  2490 */    86,   87,   88,   89,   90,   91,  197,   27,   28,  197,
 /*  2500 */   197,  197,  197,   33,   34,   35,   36,  197,  197,  197,
 /*  2510 */    40,  197,  197,  197,  197,  197,   46,  197,  197,  197,
 /*  2520 */   197,  197,  197,  119,  120,  121,  122,    2,    3,    4,
 /*  2530 */     5,    6,    7,    8,    9,   10,   11,   12,  197,  197,
 /*  2540 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  2550 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2560 */    90,   91,  197,  197,  197,  197,  197,  197,   27,   28,
 /*  2570 */   197,  132,  197,  197,   33,   34,   35,   36,  139,  140,
 /*  2580 */   197,   40,  197,  197,  197,  197,  197,   46,  197,  119,
 /*  2590 */   120,  121,  122,  197,  197,  197,  197,  197,  159,  197,
 /*  2600 */   197,  162,  163,  164,  165,  166,  197,  197,  197,  197,
 /*  2610 */   197,   70,   71,   72,   73,   74,   75,   76,   77,   78,
 /*  2620 */    79,   80,   81,   82,   83,   84,   85,   86,   87,   88,
 /*  2630 */    89,   90,   91,  197,   92,   93,   94,   95,   96,   97,
 /*  2640 */   197,  197,  197,  197,  197,  197,  104,  105,  197,  107,
 /*  2650 */   197,  109,  110,  111,  112,  113,  197,  115,  116,  197,
 /*  2660 */   119,  120,  121,  122,   44,   45,  197,   47,   48,  197,
 /*  2670 */   197,  197,   52,  197,  197,  197,   56,  197,  197,   59,
 /*  2680 */    60,   61,   62,   63,   64,  197,  197,   67,  197,  197,
 /*  2690 */    70,   71,   72,   73,   74,   75,   76,   77,   78,   79,
 /*  2700 */    80,   81,   82,   83,   84,   85,   86,   87,   88,   89,
 /*  2710 */    45,   91,  197,  197,  197,  197,  197,   52,  197,  197,
 /*  2720 */   197,   56,  197,  197,   59,   60,   61,   62,   63,   64,
 /*  2730 */   197,  197,  197,  197,  197,   70,   71,   72,   73,   74,
 /*  2740 */    75,   76,   77,   78,   79,   80,   81,   82,   83,   84,
 /*  2750 */    85,   86,   87,   88,   89,  127,   91,  197,  197,  131,
 /*  2760 */   132,  197,  197,  197,  197,  197,  197,  197,  197,  132,
 /*  2770 */   197,  143,  197,  145,  197,  197,  197,  149,  141,  197,
 /*  2780 */   197,  197,  197,  197,  156,  197,  197,  159,  151,  152,
 /*  2790 */   162,  163,  164,  165,  166,  197,  159,  197,  197,  162,
 /*  2800 */   163,  164,  165,  166,  197,  197,  197,  179,  180,  197,
 /*  2810 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  2820 */   192,  193,  194,  195,  196,  197,   54,   55,   56,   57,
 /*  2830 */    58,  197,  197,  197,  197,  197,  197,  197,   66,  197,
 /*  2840 */   197,  197,   70,   71,   72,   73,   74,   75,   76,   77,
 /*  2850 */    78,   79,   80,   81,   82,   83,   84,   85,   86,   87,
 /*  2860 */    88,   89,  197,   91,  197,  197,   54,   55,   56,   57,
 /*  2870 */    58,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  2880 */   197,  197,   70,   71,   72,   73,   74,   75,   76,   77,
 /*  2890 */    78,   79,   80,   81,   82,   83,   84,   85,   86,   87,
 /*  2900 */    88,   89,   47,   91,   49,   50,   51,   52,  197,  197,
 /*  2910 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  2920 */   197,  197,  197,  197,  197,   70,   71,   72,   73,   74,
 /*  2930 */    75,   76,   77,   78,   79,   80,   81,   82,   83,   84,
 /*  2940 */    85,   86,   87,   88,   89,  197,   91,   49,   50,   51,
 /*  2950 */    52,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  2960 */   197,  197,  197,  197,  197,  197,  197,  197,   70,   71,
 /*  2970 */    72,   73,   74,   75,   76,   77,   78,   79,   80,   81,
 /*  2980 */    82,   83,   84,   85,   86,   87,   88,   89,  132,   91,
 /*  2990 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  3000 */   132,  197,  197,  197,  197,  137,  138,  139,  140,  197,
 /*  3010 */   197,  197,  156,  197,  197,  159,  197,  161,  162,  163,
 /*  3020 */   164,  165,  166,  197,  197,  197,  197,  159,  197,  132,
 /*  3030 */   162,  163,  164,  165,  166,  179,  180,  197,  197,  183,
 /*  3040 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  3050 */   194,  195,  196,  156,  197,  197,  159,  160,  197,  162,
 /*  3060 */   163,  164,  165,  166,  197,  197,  197,  197,  197,  197,
 /*  3070 */   132,  197,  197,  197,  197,  197,  179,  180,  197,  197,
 /*  3080 */   183,  184,  185,  186,  187,  188,  189,  190,  191,  192,
 /*  3090 */   193,  194,  195,  196,  156,  197,  197,  159,  160,  197,
 /*  3100 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  3110 */   197,  132,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  3120 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  3130 */   192,  193,  194,  195,  196,  156,  197,  197,  159,  160,
 /*  3140 */   197,  162,  163,  164,  165,  166,  197,  197,  197,  197,
 /*  3150 */   197,  197,  132,  197,  197,  197,  197,  197,  179,  180,
 /*  3160 */   197,  197,  183,  184,  185,  186,  187,  188,  189,  190,
 /*  3170 */   191,  192,  193,  194,  195,  196,  156,  197,  197,  159,
 /*  3180 */   197,  197,  162,  163,  164,  165,  166,  197,  197,  197,
 /*  3190 */   197,  197,  197,  132,  197,  197,  197,  197,  197,  179,
 /*  3200 */   180,  197,  197,  183,  184,  185,  186,  187,  188,  189,
 /*  3210 */   190,  191,  192,  193,  194,  195,  196,  156,  197,  197,
 /*  3220 */   159,  197,  197,  162,  163,  164,  165,  166,  197,  197,
 /*  3230 */   197,  197,  197,  197,  132,  197,  197,  197,  197,  197,
 /*  3240 */   179,  180,  197,  197,  183,  184,  185,  186,  187,  188,
 /*  3250 */   189,  190,  191,  192,  193,  194,  195,  196,  156,  197,
 /*  3260 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  3270 */   197,  197,  197,  197,  197,  132,  197,  197,  197,  197,
 /*  3280 */   197,  179,  180,  197,  197,  183,  184,  185,  186,  187,
 /*  3290 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  156,
 /*  3300 */   197,  197,  159,  197,  197,  162,  163,  164,  165,  166,
 /*  3310 */   197,  197,  197,  197,  197,  197,  132,  197,  197,  197,
 /*  3320 */   197,  197,  179,  180,  197,  197,  183,  184,  185,  186,
 /*  3330 */   187,  188,  189,  190,  191,  192,  193,  194,  195,  196,
 /*  3340 */   156,  197,  197,  159,  197,  197,  162,  163,  164,  165,
 /*  3350 */   166,  197,  197,  197,  197,  197,  197,  132,  197,  197,
 /*  3360 */   197,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  3370 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  3380 */   196,  156,  197,  197,  159,  197,  197,  162,  163,  164,
 /*  3390 */   165,  166,  197,  197,  197,  197,  197,  197,  132,  197,
 /*  3400 */   197,  197,  197,  197,  179,  180,  197,  197,  183,  184,
 /*  3410 */   185,  186,  187,  188,  189,  190,  191,  192,  193,  194,
 /*  3420 */   195,  196,  156,  197,  197,  159,  197,  197,  162,  163,
 /*  3430 */   164,  165,  166,  197,  197,  197,  197,  197,  197,  132,
 /*  3440 */   197,  197,  197,  197,  197,  179,  180,  197,  197,  183,
 /*  3450 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  3460 */   194,  195,  196,  156,  197,  197,  159,  197,  197,  162,
 /*  3470 */   163,  164,  165,  166,  197,  197,  197,  197,  197,  197,
 /*  3480 */   132,  197,  197,  197,  197,  197,  179,  180,  197,  197,
 /*  3490 */   183,  184,  185,  186,  187,  188,  189,  190,  191,  192,
 /*  3500 */   193,  194,  195,  196,  156,  197,  197,  159,  197,  197,
 /*  3510 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  3520 */   132,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  3530 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  3540 */   192,  193,  194,  195,  196,  197,  197,  159,  197,  197,
 /*  3550 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  3560 */   197,  132,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  3570 */   182,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  3580 */   192,  193,  194,  195,  196,  156,  197,  197,  159,  197,
 /*  3590 */   197,  162,  163,  164,  165,  166,  197,  197,  197,  197,
 /*  3600 */   197,  197,  132,  197,  197,  197,  197,  197,  179,  180,
 /*  3610 */   197,  197,  183,  184,  185,  186,  187,  188,  189,  190,
 /*  3620 */   191,  192,  193,  194,  195,  196,  156,  197,  197,  159,
 /*  3630 */   197,  197,  162,  163,  164,  165,  166,  197,  197,  197,
 /*  3640 */   197,  197,  197,  132,  197,  197,  197,  197,  197,  179,
 /*  3650 */   180,  197,  197,  183,  184,  185,  186,  187,  188,  189,
 /*  3660 */   190,  191,  192,  193,  194,  195,  196,  156,  197,  197,
 /*  3670 */   159,  197,  197,  162,  163,  164,  165,  166,  197,  197,
 /*  3680 */   197,  197,  197,  197,  132,  197,  197,  197,  197,  197,
 /*  3690 */   179,  180,  197,  197,  183,  184,  185,  186,  187,  188,
 /*  3700 */   189,  190,  191,  192,  193,  194,  195,  196,  156,  197,
 /*  3710 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  3720 */   197,  197,  197,  197,  197,  132,  197,  197,  197,  197,
 /*  3730 */   197,  179,  180,  197,  197,  183,  184,  185,  186,  187,
 /*  3740 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  156,
 /*  3750 */   197,  197,  159,  197,  197,  162,  163,  164,  165,  166,
 /*  3760 */   197,  197,  197,  197,  197,  197,  132,  197,  197,  197,
 /*  3770 */   197,  197,  179,  180,  197,  197,  183,  184,  185,  186,
 /*  3780 */   187,  188,  189,  190,  191,  192,  193,  194,  195,  196,
 /*  3790 */   156,  197,  197,  159,  197,  197,  162,  163,  164,  165,
 /*  3800 */   166,  197,  197,  197,  197,  197,  197,  132,  197,  197,
 /*  3810 */   197,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  3820 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  3830 */   196,  156,  197,  197,  159,  197,  197,  162,  163,  164,
 /*  3840 */   165,  166,  197,  197,  197,  197,  197,  197,  132,  197,
 /*  3850 */   197,  197,  197,  197,  179,  180,  197,  197,  183,  184,
 /*  3860 */   185,  186,  187,  188,  189,  190,  191,  192,  193,  194,
 /*  3870 */   195,  196,  156,  197,  197,  159,  197,  197,  162,  163,
 /*  3880 */   164,  165,  166,  197,  197,  197,  197,  197,  197,  132,
 /*  3890 */   197,  197,  197,  197,  197,  179,  180,  197,  197,  183,
 /*  3900 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  3910 */   194,  195,  196,  156,  197,  197,  159,  197,  197,  162,
 /*  3920 */   163,  164,  165,  166,  197,  197,  197,  197,  197,  197,
 /*  3930 */   132,  197,  197,  197,  197,  197,  179,  180,  197,  197,
 /*  3940 */   183,  184,  185,  186,  187,  188,  189,  190,  191,  192,
 /*  3950 */   193,  194,  195,  196,  156,  197,  197,  159,  197,  197,
 /*  3960 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  3970 */   132,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  3980 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  3990 */   192,  193,  194,  195,  196,  197,  197,  159,  197,  197,
 /*  4000 */   162,  163,  164,  165,  166,  197,  197,  197,  132,  197,
 /*  4010 */   197,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  4020 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  4030 */   192,  193,  194,  195,  196,  159,  197,  197,  162,  163,
 /*  4040 */   164,  165,  166,  197,  197,  197,  132,  197,  197,  197,
 /*  4050 */   197,  197,  197,  197,  197,  179,  180,  197,  197,  183,
 /*  4060 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  4070 */   194,  195,  196,  159,  197,  197,  162,  163,  164,  165,
 /*  4080 */   166,  197,  197,  197,  132,  197,  197,  197,  197,  197,
 /*  4090 */   197,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  4100 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  4110 */   196,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  4120 */   197,  197,  132,  197,  197,  197,  197,  197,  197,  197,
 /*  4130 */   197,  179,  180,  197,  197,  183,  184,  185,  186,  187,
 /*  4140 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  159,
 /*  4150 */   197,  197,  162,  163,  164,  165,  166,  197,  197,  197,
 /*  4160 */   132,  197,  197,  197,  197,  197,  197,  197,  197,  179,
 /*  4170 */   180,  197,  197,  183,  184,  185,  186,  187,  188,  189,
 /*  4180 */   190,  191,  192,  193,  194,  195,  196,  159,  197,  197,
 /*  4190 */   162,  163,  164,  165,  166,  197,  197,  197,  132,  197,
 /*  4200 */   197,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  4210 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  4220 */   192,  193,  194,  195,  196,  159,  197,  197,  162,  163,
 /*  4230 */   164,  165,  166,  197,  197,  197,  132,  197,  197,  197,
 /*  4240 */   197,  197,  197,  197,  197,  179,  180,  197,  197,  183,
 /*  4250 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  4260 */   194,  195,  196,  159,  197,  197,  162,  163,  164,  165,
 /*  4270 */   166,  197,  197,  197,  132,  197,  197,  197,  197,  197,
 /*  4280 */   197,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  4290 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  4300 */   196,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  4310 */   197,  197,  132,  197,  197,  197,  197,  197,  197,  197,
 /*  4320 */   197,  179,  180,  197,  197,  183,  184,  185,  186,  187,
 /*  4330 */   188,  189,  190,  191,  192,  193,  194,  195,  196,  159,
 /*  4340 */   197,  197,  162,  163,  164,  165,  166,  197,  197,  197,
 /*  4350 */   132,  197,  197,  197,  197,  197,  197,  197,  197,  179,
 /*  4360 */   180,  197,  197,  183,  184,  185,  186,  187,  188,  189,
 /*  4370 */   190,  191,  192,  193,  194,  195,  196,  159,  197,  197,
 /*  4380 */   162,  163,  164,  165,  166,  197,  197,  197,  132,  197,
 /*  4390 */   197,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  4400 */   197,  183,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  4410 */   192,  193,  194,  195,  196,  159,  197,  197,  162,  163,
 /*  4420 */   164,  165,  166,  197,  197,  197,  132,  197,  197,  197,
 /*  4430 */   197,  197,  197,  197,  197,  179,  180,  197,  197,  183,
 /*  4440 */   184,  185,  186,  187,  188,  189,  190,  191,  192,  193,
 /*  4450 */   194,  195,  196,  159,  197,  197,  162,  163,  164,  165,
 /*  4460 */   166,  197,  197,  197,  197,  197,   45,  197,  197,  197,
 /*  4470 */   197,  197,  197,  179,  180,  197,  197,  183,  184,  185,
 /*  4480 */   186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
 /*  4490 */   196,   70,   71,   72,   73,   74,   75,   76,   77,   78,
 /*  4500 */    79,   80,   81,   82,   83,   84,   85,   86,   87,   88,
 /*  4510 */    89,  197,   91,  197,   47,  197,  197,  197,  197,  197,
 /*  4520 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  4530 */   132,  197,  197,  197,  197,  197,  197,   70,   71,   72,
 /*  4540 */    73,   74,   75,   76,   77,   78,   79,   80,   81,   82,
 /*  4550 */    83,   84,   85,   86,   87,   88,   89,  159,   91,  197,
 /*  4560 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  4570 */   197,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  4580 */   197,  197,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  4590 */   192,  193,  194,  195,  196,  197,  197,  197,  197,  197,
 /*  4600 */   197,   70,   71,   72,   73,   74,   75,   76,   77,   78,
 /*  4610 */    79,   80,   81,   82,   83,   84,   85,   86,   87,   88,
 /*  4620 */    89,  132,   91,  197,  124,  125,  126,  127,  128,  197,
 /*  4630 */   130,  131,  132,  197,  134,  135,  197,  197,  197,  197,
 /*  4640 */   197,  197,  197,  143,  197,  145,  197,  197,  159,  149,
 /*  4650 */   197,  162,  163,  164,  165,  166,  197,  197,  197,  159,
 /*  4660 */   197,  197,  162,  163,  164,  165,  166,  197,  179,  180,
 /*  4670 */   132,  197,  197,  184,  185,  186,  187,  188,  189,  190,
 /*  4680 */   191,  192,  193,  194,  195,  197,  197,  197,  197,  197,
 /*  4690 */   197,  197,  197,  197,  197,  197,  197,  159,  197,  197,
 /*  4700 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  4710 */   132,  197,  197,  197,  197,  197,  197,  179,  180,  197,
 /*  4720 */   132,  197,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  4730 */   192,  193,  194,  145,  197,  197,  197,  159,  197,  197,
 /*  4740 */   162,  163,  164,  165,  166,  132,  197,  159,  197,  197,
 /*  4750 */   162,  163,  164,  165,  166,  197,  197,  179,  180,  197,
 /*  4760 */   197,  197,  184,  185,  186,  187,  188,  189,  190,  191,
 /*  4770 */   192,  193,  159,  197,  197,  162,  163,  164,  165,  166,
 /*  4780 */   197,  132,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  4790 */   197,  197,  179,  180,  197,  197,  197,  184,  185,  186,
 /*  4800 */   187,  188,  189,  190,  191,  192,  197,  197,  159,  197,
 /*  4810 */   197,  162,  163,  164,  165,  166,  197,  197,  197,  197,
 /*  4820 */   197,  197,  197,  197,  197,  197,  197,  197,  179,  180,
 /*  4830 */   197,  197,  197,  184,  185,  186,  187,  188,  189,  190,
 /*  4840 */   191,  197,  197,  197,  126,  127,  128,  197,  130,  131,
 /*  4850 */   132,  197,  134,  135,  197,  197,  197,  197,  132,  197,
 /*  4860 */   197,  143,  197,  145,  197,  197,  197,  149,  197,  197,
 /*  4870 */   132,  197,  197,  197,  197,  197,  197,  159,  197,  197,
 /*  4880 */   162,  163,  164,  165,  166,  159,  197,  197,  162,  163,
 /*  4890 */   164,  165,  166,  197,  197,  197,  197,  159,  197,  197,
 /*  4900 */   162,  163,  164,  165,  166,  179,  180,  197,  132,  197,
 /*  4910 */   184,  185,  186,  187,  188,  189,  190,  179,  180,  197,
 /*  4920 */   197,  197,  184,  185,  186,  187,  188,  189,  197,  197,
 /*  4930 */   197,  197,  197,  197,  197,  159,  197,  197,  162,  163,
 /*  4940 */   164,  165,  166,  197,  132,  197,  197,  197,  197,  197,
 /*  4950 */   197,  197,  197,  197,  197,  179,  180,  197,  197,  197,
 /*  4960 */   184,  185,  186,  187,  188,  189,  197,  197,  197,  197,
 /*  4970 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  132,
 /*  4980 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  4990 */   197,  179,  180,  197,  197,  197,  184,  185,  186,  187,
 /*  5000 */   188,  197,  197,  197,  197,  197,  159,  197,  197,  162,
 /*  5010 */   163,  164,  165,  166,  132,  197,  197,  197,  197,  197,
 /*  5020 */   197,  197,  197,  197,  132,  197,  179,  180,  197,  197,
 /*  5030 */   197,  184,  185,  186,  187,  188,  197,  197,  197,  197,
 /*  5040 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  5050 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  132,
 /*  5060 */   197,  179,  180,  197,  197,  197,  184,  185,  186,  187,
 /*  5070 */   188,  179,  180,  197,  197,  197,  184,  185,  186,  187,
 /*  5080 */   188,  197,  197,  197,  197,  197,  159,  197,  132,  162,
 /*  5090 */   163,  164,  165,  166,  197,  197,  197,  132,  197,  197,
 /*  5100 */   197,  197,  197,  197,  197,  197,  179,  180,  197,  197,
 /*  5110 */   197,  184,  185,  186,  187,  159,  197,  197,  162,  163,
 /*  5120 */   164,  165,  166,  197,  159,  132,  197,  162,  163,  164,
 /*  5130 */   165,  166,  197,  197,  197,  179,  180,  197,  197,  197,
 /*  5140 */   184,  185,  186,  187,  179,  180,  197,  197,  197,  184,
 /*  5150 */   185,  186,  159,  197,  197,  162,  163,  164,  165,  166,
 /*  5160 */   132,  197,  197,  197,  197,  137,  138,  139,  140,  197,
 /*  5170 */   197,  197,  179,  180,  132,  197,  197,  184,  185,  186,
 /*  5180 */   197,  197,  197,  197,  132,  197,  197,  159,  197,  197,
 /*  5190 */   162,  163,  164,  165,  166,  197,  197,  197,  197,  197,
 /*  5200 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  5210 */   197,  159,  132,  197,  162,  163,  164,  165,  166,  197,
 /*  5220 */   197,  179,  180,  132,  197,  197,  184,  185,  197,  197,
 /*  5230 */   197,  179,  180,  197,  197,  197,  184,  185,  197,  159,
 /*  5240 */   197,  197,  162,  163,  164,  165,  166,  132,  197,  197,
 /*  5250 */   159,  197,  197,  162,  163,  164,  165,  166,  132,  179,
 /*  5260 */   180,  197,  197,  197,  184,  185,  197,  197,  197,  197,
 /*  5270 */   179,  180,  197,  197,  159,  184,  185,  162,  163,  164,
 /*  5280 */   165,  166,  132,  197,  197,  159,  197,  197,  162,  163,
 /*  5290 */   164,  165,  166,  132,  179,  180,  197,  197,  197,  184,
 /*  5300 */   185,  197,  197,  197,  197,  179,  180,  197,  197,  159,
 /*  5310 */   184,  185,  162,  163,  164,  165,  166,  132,  197,  197,
 /*  5320 */   159,  197,  197,  162,  163,  164,  165,  166,  197,  179,
 /*  5330 */   180,  197,  197,  197,  184,  185,  197,  197,  197,  197,
 /*  5340 */   179,  180,  197,  197,  159,  184,  185,  162,  163,  164,
 /*  5350 */   165,  166,  197,  197,  132,  197,  197,  197,  197,  197,
 /*  5360 */   197,  197,  197,  197,  179,  180,  197,  197,  197,  184,
 /*  5370 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  5380 */   197,  159,  197,  197,  162,  163,  164,  165,  166,  197,
 /*  5390 */   197,  197,  197,  197,  197,  197,  197,  197,  197,  197,
 /*  5400 */   197,  179,  180,  197,  197,  197,  184,
};
#define YY_SHIFT_USE_DFLT (-43)
#define YY_SHIFT_COUNT (296)
#define YY_SHIFT_MIN   (-42)
#define YY_SHIFT_MAX   (4531)
static const short yy_shift_ofst[] = {
 /*     0 */  2620,   63,  153,  153,  -27,  153,  153,  153,  153,  153,
 /*    10 */   153,  153,  153,  153,  153,  153,  153,  153,  153,  153,
 /*    20 */   153,  153,  153,  243, 2404, 2404, 2404, 2338, 2270, 2248,
 /*    30 */  2180, 2158, 2090, 2068, 2000, 1978, 1910, 1888, 1820, 1798,
 /*    40 */  2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541,
 /*    50 */  2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541,
 /*    60 */  2541, 2541, 2541, 2541, 2541, 2541, 1733, 2541, 2541, 2541,
 /*    70 */  2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541, 2541,
 /*    80 */  2541, 2541, 2541, 2541, 2541, 2541, 2541, 2855, 2855, 2812,
 /*    90 */  2470, 2470, 2772, 2898, 2665, 4467, 4531, 4421, 4531, 4531,
 /*   100 */   459,  456,  395,  619,  619,  408,   12,   12,  411,  315,
 /*   110 */   847,  801,  837,  837,  837,  837,  801,  334,  102,  102,
 /*   120 */   388,  801,  801, 2542,  408,  408,  -40,  330,  384,  807,
 /*   130 */   852,  852,  852,  852,  807,  825,  802,  807,  852,  803,
 /*   140 */   798,  801,  809,  809,  801, 2525,  495,   61,   61,   61,
 /*   150 */   -33,  195,   72,   66,  200,   -7,   -7,   -7,  -28,  639,
 /*   160 */   592,  605,  590,  559,  560,  557,  530,  583,  527,  439,
 /*   170 */   391,  378,  437,  421,  387,  367,  246,    2,  312,  383,
 /*   180 */   348,  344,  366,  281,  267,  244,  257,  307,  231,  252,
 /*   190 */    93,   93,  106,  106,  106,  106,    3,    3,  106,    3,
 /*   200 */   311,  221,   41,  804,  800,  778,  774,  769,  773,  770,
 /*   210 */   810,  768,  763,  756,  747,  734,  757,  754,  755,  753,
 /*   220 */   751,  750,  690,  685,  675,  745,  674,  739,  669,  736,
 /*   230 */   662,  729,  659,  726,  658,  711,  707,  706,  703,  696,
 /*   240 */   688,  647,  689,  684,  636,  683,  679,  678,  677,  673,
 /*   250 */   672,  670,  650,  671,  709,  644,  610,  663,  657,  661,
 /*   260 */   655,  630,  621,  625,  613,  643,  640,  643,  638,  640,
 /*   270 */   635,  638,  635,  629,  561,  645,  552,  633,  564,  545,
 /*   280 */   534,  556,  515,  500,  428,  478,  396,  475,  410,  407,
 /*   290 */   333,  227,  180,  134,    5,  -42,   48,
};
#define YY_REDUCE_USE_DFLT (-128)
#define YY_REDUCE_COUNT (144)
#define YY_REDUCE_MIN   (-127)
#define YY_REDUCE_MAX   (5222)
static const short yy_reduce_ofst[] = {
 /*     0 */  4500,  354,  280,  210,  712,  712, 1576, 1506, 1432, 1362,
 /*    10 */  1288, 1218, 1144, 1074, 1000,  930,  856,  786,  712,  642,
 /*    20 */   568,  498,  424, 2628, 2979, 2938, 2897, 2856, 3798, 3757,
 /*    30 */  3716, 3675, 3634, 3593, 3552, 3511, 3470, 3429, 3388, 3348,
 /*    40 */  3307, 3266, 3225, 3184, 3143, 3102, 3061, 3020, 4294, 4256,
 /*    50 */  4218, 4180, 4142, 4104, 4066, 4028, 3990, 3952, 3914, 3876,
 /*    60 */  3838, 4398, 4489, 4538, 4578, 4613, 4718, 4649, 4726, 4776,
 /*    70 */  4738, 4892, 4882, 4847, 4812, 4956, 4927, 4993, 4965, 5161,
 /*    80 */  5150, 5126, 5115, 5091, 5080, 5052, 5042, 5028, 2868, 2302,
 /*    90 */  5222, 5185, 2637, 2439, 4588,  400,  128,  617,  543,  473,
 /*   100 */   360, -127,  165,   52,  -37,   90,  202,   34,   46,   46,
 /*   110 */    75,   46,  109,   56,   47,  -34,  -44,  491,  503,  481,
 /*   120 */   326,  419,  291,  720,  752,  722,  708,  681,  743,  758,
 /*   130 */   737,  731,  728,  727,  730,  686,  735,  723,  716,  699,
 /*   140 */   697,  717,  681,  667,  713,
};
static const YYACTIONTYPE yy_default[] = {
 /*     0 */   813,  813,  741,  739,  813,  740,  813,  813,  813,  813,
 /*    10 */   813,  813,  813,  813,  813,  813,  813,  813,  738,  813,
 /*    20 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*    30 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*    40 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*    50 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*    60 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*    70 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*    80 */   813,  813,  813,  813,  813,  813,  813,  544,  544,  813,
 /*    90 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   100 */   606,  598,  813,  602,  594,  813,  562,  554,  813,  813,
 /*   110 */   813,  813,  604,  600,  596,  592,  813,  813,  558,  550,
 /*   120 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  538,
 /*   130 */   560,  556,  552,  548,  536,  813,  813,  614,  813,  620,
 /*   140 */   619,  813,  813,  742,  813,  767,  813,  786,  785,  784,
 /*   150 */   760,  813,  813,  813,  813,  775,  774,  773,  623,  813,
 /*   160 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   170 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   180 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  797,
 /*   190 */   788,  787,  783,  782,  781,  780,  778,  777,  779,  776,
 /*   200 */   813,  813,  813,  813,  813,  813,  565,  563,  813,  813,
 /*   210 */   543,  813,  813,  813,  813,  813,  813,  689,  813,  813,
 /*   220 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   230 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   240 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   250 */   813,  813,  813,  813,  631,  813,  813,  813,  813,  813,
 /*   260 */   813,  813,  813,  813,  813,  796,  794,  795,  792,  793,
 /*   270 */   790,  791,  789,  813,  813,  813,  813,  813,  813,  813,
 /*   280 */   813,  813,  813,  813,  813,  813,  813,  813,  813,  813,
 /*   290 */   813,  813,  813,  813,  813,  813,  623,  526,  541,  537,
 /*   300 */   540,  545,  568,  567,  564,  566,  561,  559,  557,  555,
 /*   310 */   553,  551,  549,  547,  546,  542,  539,  535,  532,  531,
 /*   320 */   736,  718,  717,  719,  716,  712,  711,  710,  709,  714,
 /*   330 */   713,  715,  735,  730,  723,  708,  707,  703,  702,  698,
 /*   340 */   697,  696,  695,  694,  693,  692,  673,  737,  691,  690,
 /*   350 */   688,  687,  685,  734,  733,  729,  728,  727,  726,  731,
 /*   360 */   725,  732,  724,  722,  706,  701,  721,  705,  700,  720,
 /*   370 */   704,  699,  684,  680,  679,  678,  677,  676,  675,  683,
 /*   380 */   682,  681,  674,  672,  671,  608,  609,  612,  615,  616,
 /*   390 */   613,  611,  610,  607,  574,  573,  572,  571,  570,  578,
 /*   400 */   686,  577,  576,  588,  587,  586,  585,  584,  583,  582,
 /*   410 */   581,  580,  579,  589,  605,  603,  601,  599,  628,  630,
 /*   420 */   629,  627,  597,  595,  636,  635,  634,  633,  632,  593,
 /*   430 */   618,  617,  591,  626,  625,  624,  569,  590,  575,  534,
 /*   440 */   812,  758,  759,  757,  756,  768,  755,  754,  753,  761,
 /*   450 */   811,  810,  809,  808,  807,  806,  805,  804,  803,  802,
 /*   460 */   801,  800,  799,  798,  772,  771,  770,  769,  767,  766,
 /*   470 */   765,  764,  763,  762,  752,  750,  749,  748,  747,  746,
 /*   480 */   745,  744,  743,  751,  670,  669,  668,  667,  666,  665,
 /*   490 */   664,  663,  662,  661,  660,  659,  658,  657,  656,  655,
 /*   500 */   654,  653,  652,  651,  650,  649,  648,  647,  646,  645,
 /*   510 */   644,  643,  642,  641,  640,  639,  638,  637,  622,  621,
 /*   520 */   533,  530,  529,  528,  527,
};

/* The next table maps tokens into fallback tokens.  If a construct
** like the following:
** 
**      %fallback ID X Y Z.
**
** appears in the grammar, then ID becomes a fallback token for X, Y,
** and Z.  Whenever one of the tokens X, Y, or Z is input to the parser
** but it does not parse, the type of the token is changed to ID and
** the parse is retried before an error is thrown.
*/
#ifdef YYFALLBACK
static const YYCODETYPE yyFallback[] = {
};
#endif /* YYFALLBACK */

/* The following structure represents a single element of the
** parser's stack.  Information stored includes:
**
**   +  The state number for the parser at this level of the stack.
**
**   +  The value of the token stored at this level of the stack.
**      (In other words, the "major" token.)
**
**   +  The semantic value stored at this level of the stack.  This is
**      the information used by the action routines in the grammar.
**      It is sometimes called the "minor" token.
*/
struct yyStackEntry {
  YYACTIONTYPE stateno;  /* The state-number */
  YYCODETYPE major;      /* The major token value.  This is the code
                         ** number for the token at this stack level */
  YYMINORTYPE minor;     /* The user-supplied minor token value.  This
                         ** is the value of the token  */
};
typedef struct yyStackEntry yyStackEntry;

/* The state of the parser is completely contained in an instance of
** the following structure */
struct yyParser {
  int yyidx;                    /* Index of top element in stack */
#ifdef YYTRACKMAXSTACKDEPTH
  int yyidxMax;                 /* Maximum value of yyidx */
#endif
  int yyerrcnt;                 /* Shifts left before out of the error */
  ParseHLSLARG_SDECL                /* A place to hold %extra_argument */
#if YYSTACKDEPTH<=0
  int yystksz;                  /* Current side of the stack */
  yyStackEntry *yystack;        /* The parser's stack */
#else
  yyStackEntry yystack[YYSTACKDEPTH];  /* The parser's stack */
#endif
};
typedef struct yyParser yyParser;

#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
#include <stdio.h>
static FILE *yyTraceFILE = 0;
static char *yyTracePrompt = 0;
#endif /* NDEBUG */

#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
/*
** Turn parser tracing on by giving a stream to which to write the trace
** and a prompt to preface each trace message.  Tracing is turned off
** by making either argument NULL 
**
** Inputs:
** <ul>
** <li> A FILE* to which trace output should be written.
**      If NULL, then tracing is turned off.
** <li> A prefix string written at the beginning of every
**      line of trace output.  If NULL, then tracing is
**      turned off.
** </ul>
**
** Outputs:
** None.
*/
#if __MOJOSHADER__
static
#endif
void ParseHLSLTrace(FILE *TraceFILE, char *zTracePrompt){
  yyTraceFILE = TraceFILE;
  yyTracePrompt = zTracePrompt;
  if( yyTraceFILE==0 ) yyTracePrompt = 0;
  else if( yyTracePrompt==0 ) yyTraceFILE = 0;
}
#endif /* NDEBUG */

#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
/* For tracing shifts, the names of all terminals and nonterminals
** are required.  The following table supplies these names */
static const char *const yyTokenName[] = { 
  "$",             "COMMA",         "ASSIGN",        "ADDASSIGN",   
  "SUBASSIGN",     "MULASSIGN",     "DIVASSIGN",     "MODASSIGN",   
  "LSHIFTASSIGN",  "RSHIFTASSIGN",  "ANDASSIGN",     "ORASSIGN",    
  "XORASSIGN",     "QUESTION",      "OROR",          "ANDAND",      
  "OR",            "XOR",           "AND",           "EQL",         
  "NEQ",           "LT",            "LEQ",           "GT",          
  "GEQ",           "LSHIFT",        "RSHIFT",        "PLUS",        
  "MINUS",         "STAR",          "SLASH",         "PERCENT",     
  "TYPECAST",      "EXCLAMATION",   "COMPLEMENT",    "MINUSMINUS",  
  "PLUSPLUS",      "DOT",           "LBRACKET",      "RBRACKET",    
  "LPAREN",        "RPAREN",        "ELSE",          "SEMICOLON",   
  "TYPEDEF",       "CONST",         "IDENTIFIER",    "VOID",        
  "INLINE",        "IN",            "INOUT",         "OUT",         
  "UNIFORM",       "COLON",         "LINEAR",        "CENTROID",    
  "NOINTERPOLATION",  "NOPERSPECTIVE",  "SAMPLE",        "EXTERN",      
  "SHARED",        "STATIC",        "VOLATILE",      "ROWMAJOR",    
  "COLUMNMAJOR",   "LBRACE",        "RBRACE",        "STRUCT",      
  "PACKOFFSET",    "REGISTER",      "USERTYPE",      "SAMPLER",     
  "SAMPLER1D",     "SAMPLER2D",     "SAMPLER3D",     "SAMPLERCUBE", 
  "SAMPLER_STATE",  "SAMPLERSTATE",  "SAMPLERCOMPARISONSTATE",  "BOOL",        
  "INT",           "UINT",          "HALF",          "FLOAT",       
  "DOUBLE",        "STRING",        "SNORM",         "UNORM",       
  "BUFFER",        "VECTOR",        "INT_CONSTANT",  "MATRIX",      
  "ISOLATE",       "MAXINSTRUCTIONCOUNT",  "NOEXPRESSIONOPTIMIZATIONS",  "REMOVEUNUSEDINPUTS",
  "UNUSED",        "XPS",           "BREAK",         "CONTINUE",    
  "DISCARD",       "DO",            "WHILE",         "RETURN",      
  "UNROLL",        "LOOP",          "FOR",           "BRANCH",      
  "IF",            "FLATTEN",       "IFALL",         "IFANY",       
  "PREDICATE",     "PREDICATEBLOCK",  "SWITCH",        "FORCECASE",   
  "CALL",          "CASE",          "DEFAULT",       "FLOAT_CONSTANT",
  "STRING_LITERAL",  "TRUE",          "FALSE",         "error",       
  "shader",        "compilation_units",  "compilation_unit",  "variable_declaration",
  "function_signature",  "statement_block",  "typedef",       "struct_declaration",
  "datatype",      "scalar_or_array",  "function_storageclass",  "function_details",
  "semantic",      "function_parameters",  "function_parameter_list",  "function_parameter",
  "input_modifier",  "interpolation_mod",  "initializer",   "variable_attribute_list",
  "variable_declaration_details_list",  "variable_attribute",  "variable_declaration_details",  "annotations", 
  "variable_lowlevel",  "struct_intro",  "struct_member_list",  "struct_member",
  "struct_member_details",  "struct_member_item_list",  "packoffset",    "register",    
  "expression",    "annotation_list",  "annotation",    "datatype_scalar",
  "initializer_block_list",  "initializer_block",  "intrinsic_datatype",  "datatype_vector",
  "datatype_matrix",  "datatype_sampler",  "datatype_buffer",  "statement_list",
  "statement",     "statement_attribute",  "do_intro",      "while_intro", 
  "if_intro",      "switch_intro",  "switch_case_list",  "for_statement",
  "for_intro",     "for_details",   "switch_case",   "primary_expr",
  "postfix_expr",  "arguments",     "argument_list",  "assignment_expr",
  "unary_expr",    "cast_expr",     "multiplicative_expr",  "additive_expr",
  "shift_expr",    "relational_expr",  "equality_expr",  "and_expr",    
  "exclusive_or_expr",  "inclusive_or_expr",  "logical_and_expr",  "logical_or_expr",
  "conditional_expr",
};
#endif /* NDEBUG */

#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
/* For tracing reduce actions, the names of all rules are required.
*/
static const char *const yyRuleName[] = {
 /*   0 */ "shader ::= compilation_units",
 /*   1 */ "compilation_units ::= compilation_unit",
 /*   2 */ "compilation_units ::= compilation_units compilation_unit",
 /*   3 */ "compilation_unit ::= variable_declaration",
 /*   4 */ "compilation_unit ::= function_signature SEMICOLON",
 /*   5 */ "compilation_unit ::= function_signature statement_block",
 /*   6 */ "compilation_unit ::= typedef",
 /*   7 */ "compilation_unit ::= struct_declaration SEMICOLON",
 /*   8 */ "typedef ::= TYPEDEF CONST datatype scalar_or_array",
 /*   9 */ "typedef ::= TYPEDEF datatype scalar_or_array",
 /*  10 */ "function_signature ::= function_storageclass function_details semantic",
 /*  11 */ "function_signature ::= function_storageclass function_details",
 /*  12 */ "function_signature ::= function_details semantic",
 /*  13 */ "function_signature ::= function_details",
 /*  14 */ "function_details ::= datatype IDENTIFIER LPAREN function_parameters RPAREN",
 /*  15 */ "function_details ::= VOID IDENTIFIER LPAREN function_parameters RPAREN",
 /*  16 */ "function_storageclass ::= INLINE",
 /*  17 */ "function_parameters ::= VOID",
 /*  18 */ "function_parameters ::= function_parameter_list",
 /*  19 */ "function_parameters ::=",
 /*  20 */ "function_parameter_list ::= function_parameter",
 /*  21 */ "function_parameter_list ::= function_parameter_list COMMA function_parameter",
 /*  22 */ "function_parameter ::= input_modifier datatype IDENTIFIER semantic interpolation_mod initializer",
 /*  23 */ "function_parameter ::= input_modifier datatype IDENTIFIER semantic interpolation_mod",
 /*  24 */ "function_parameter ::= input_modifier datatype IDENTIFIER semantic initializer",
 /*  25 */ "function_parameter ::= input_modifier datatype IDENTIFIER semantic",
 /*  26 */ "function_parameter ::= input_modifier datatype IDENTIFIER interpolation_mod initializer",
 /*  27 */ "function_parameter ::= input_modifier datatype IDENTIFIER interpolation_mod",
 /*  28 */ "function_parameter ::= input_modifier datatype IDENTIFIER initializer",
 /*  29 */ "function_parameter ::= input_modifier datatype IDENTIFIER",
 /*  30 */ "function_parameter ::= datatype IDENTIFIER semantic interpolation_mod initializer",
 /*  31 */ "function_parameter ::= datatype IDENTIFIER semantic interpolation_mod",
 /*  32 */ "function_parameter ::= datatype IDENTIFIER semantic initializer",
 /*  33 */ "function_parameter ::= datatype IDENTIFIER semantic",
 /*  34 */ "function_parameter ::= datatype IDENTIFIER interpolation_mod initializer",
 /*  35 */ "function_parameter ::= datatype IDENTIFIER interpolation_mod",
 /*  36 */ "function_parameter ::= datatype IDENTIFIER initializer",
 /*  37 */ "function_parameter ::= datatype IDENTIFIER",
 /*  38 */ "input_modifier ::= IN",
 /*  39 */ "input_modifier ::= INOUT",
 /*  40 */ "input_modifier ::= OUT",
 /*  41 */ "input_modifier ::= IN OUT",
 /*  42 */ "input_modifier ::= OUT IN",
 /*  43 */ "input_modifier ::= UNIFORM",
 /*  44 */ "semantic ::= COLON IDENTIFIER",
 /*  45 */ "interpolation_mod ::= LINEAR",
 /*  46 */ "interpolation_mod ::= CENTROID",
 /*  47 */ "interpolation_mod ::= NOINTERPOLATION",
 /*  48 */ "interpolation_mod ::= NOPERSPECTIVE",
 /*  49 */ "interpolation_mod ::= SAMPLE",
 /*  50 */ "variable_declaration ::= variable_attribute_list datatype variable_declaration_details_list SEMICOLON",
 /*  51 */ "variable_declaration ::= datatype variable_declaration_details_list SEMICOLON",
 /*  52 */ "variable_declaration ::= struct_declaration variable_declaration_details_list SEMICOLON",
 /*  53 */ "variable_attribute_list ::= variable_attribute",
 /*  54 */ "variable_attribute_list ::= variable_attribute_list variable_attribute",
 /*  55 */ "variable_attribute ::= EXTERN",
 /*  56 */ "variable_attribute ::= NOINTERPOLATION",
 /*  57 */ "variable_attribute ::= SHARED",
 /*  58 */ "variable_attribute ::= STATIC",
 /*  59 */ "variable_attribute ::= UNIFORM",
 /*  60 */ "variable_attribute ::= VOLATILE",
 /*  61 */ "variable_attribute ::= CONST",
 /*  62 */ "variable_attribute ::= ROWMAJOR",
 /*  63 */ "variable_attribute ::= COLUMNMAJOR",
 /*  64 */ "variable_declaration_details_list ::= variable_declaration_details",
 /*  65 */ "variable_declaration_details_list ::= variable_declaration_details_list COMMA variable_declaration_details",
 /*  66 */ "variable_declaration_details ::= scalar_or_array semantic annotations initializer variable_lowlevel",
 /*  67 */ "variable_declaration_details ::= scalar_or_array semantic annotations initializer",
 /*  68 */ "variable_declaration_details ::= scalar_or_array semantic annotations variable_lowlevel",
 /*  69 */ "variable_declaration_details ::= scalar_or_array semantic annotations",
 /*  70 */ "variable_declaration_details ::= scalar_or_array semantic initializer variable_lowlevel",
 /*  71 */ "variable_declaration_details ::= scalar_or_array semantic initializer",
 /*  72 */ "variable_declaration_details ::= scalar_or_array semantic variable_lowlevel",
 /*  73 */ "variable_declaration_details ::= scalar_or_array semantic",
 /*  74 */ "variable_declaration_details ::= scalar_or_array annotations initializer variable_lowlevel",
 /*  75 */ "variable_declaration_details ::= scalar_or_array annotations initializer",
 /*  76 */ "variable_declaration_details ::= scalar_or_array annotations variable_lowlevel",
 /*  77 */ "variable_declaration_details ::= scalar_or_array annotations",
 /*  78 */ "variable_declaration_details ::= scalar_or_array initializer variable_lowlevel",
 /*  79 */ "variable_declaration_details ::= scalar_or_array initializer",
 /*  80 */ "variable_declaration_details ::= scalar_or_array variable_lowlevel",
 /*  81 */ "variable_declaration_details ::= scalar_or_array",
 /*  82 */ "struct_declaration ::= struct_intro LBRACE struct_member_list RBRACE",
 /*  83 */ "struct_intro ::= STRUCT IDENTIFIER",
 /*  84 */ "struct_member_list ::= struct_member",
 /*  85 */ "struct_member_list ::= struct_member_list struct_member",
 /*  86 */ "struct_member ::= interpolation_mod struct_member_details",
 /*  87 */ "struct_member ::= struct_member_details",
 /*  88 */ "struct_member_details ::= datatype struct_member_item_list SEMICOLON",
 /*  89 */ "struct_member_item_list ::= scalar_or_array",
 /*  90 */ "struct_member_item_list ::= scalar_or_array semantic",
 /*  91 */ "struct_member_item_list ::= struct_member_item_list COMMA IDENTIFIER",
 /*  92 */ "variable_lowlevel ::= packoffset register",
 /*  93 */ "variable_lowlevel ::= register packoffset",
 /*  94 */ "variable_lowlevel ::= packoffset",
 /*  95 */ "variable_lowlevel ::= register",
 /*  96 */ "scalar_or_array ::= IDENTIFIER LBRACKET RBRACKET",
 /*  97 */ "scalar_or_array ::= IDENTIFIER LBRACKET expression RBRACKET",
 /*  98 */ "scalar_or_array ::= IDENTIFIER",
 /*  99 */ "packoffset ::= COLON PACKOFFSET LPAREN IDENTIFIER DOT IDENTIFIER RPAREN",
 /* 100 */ "packoffset ::= COLON PACKOFFSET LPAREN IDENTIFIER RPAREN",
 /* 101 */ "register ::= COLON REGISTER LPAREN IDENTIFIER RPAREN",
 /* 102 */ "annotations ::= LT annotation_list GT",
 /* 103 */ "annotation_list ::= annotation",
 /* 104 */ "annotation_list ::= annotation_list annotation",
 /* 105 */ "annotation ::= datatype_scalar initializer SEMICOLON",
 /* 106 */ "initializer_block_list ::= expression",
 /* 107 */ "initializer_block_list ::= LBRACE initializer_block_list RBRACE",
 /* 108 */ "initializer_block_list ::= initializer_block_list COMMA initializer_block_list",
 /* 109 */ "initializer_block ::= LBRACE initializer_block_list RBRACE",
 /* 110 */ "initializer ::= ASSIGN initializer_block",
 /* 111 */ "initializer ::= ASSIGN expression",
 /* 112 */ "intrinsic_datatype ::= datatype_vector",
 /* 113 */ "intrinsic_datatype ::= datatype_matrix",
 /* 114 */ "intrinsic_datatype ::= datatype_scalar",
 /* 115 */ "intrinsic_datatype ::= datatype_sampler",
 /* 116 */ "intrinsic_datatype ::= datatype_buffer",
 /* 117 */ "datatype ::= intrinsic_datatype",
 /* 118 */ "datatype ::= USERTYPE",
 /* 119 */ "datatype_sampler ::= SAMPLER",
 /* 120 */ "datatype_sampler ::= SAMPLER1D",
 /* 121 */ "datatype_sampler ::= SAMPLER2D",
 /* 122 */ "datatype_sampler ::= SAMPLER3D",
 /* 123 */ "datatype_sampler ::= SAMPLERCUBE",
 /* 124 */ "datatype_sampler ::= SAMPLER_STATE",
 /* 125 */ "datatype_sampler ::= SAMPLERSTATE",
 /* 126 */ "datatype_sampler ::= SAMPLERCOMPARISONSTATE",
 /* 127 */ "datatype_scalar ::= BOOL",
 /* 128 */ "datatype_scalar ::= INT",
 /* 129 */ "datatype_scalar ::= UINT",
 /* 130 */ "datatype_scalar ::= HALF",
 /* 131 */ "datatype_scalar ::= FLOAT",
 /* 132 */ "datatype_scalar ::= DOUBLE",
 /* 133 */ "datatype_scalar ::= STRING",
 /* 134 */ "datatype_scalar ::= SNORM FLOAT",
 /* 135 */ "datatype_scalar ::= UNORM FLOAT",
 /* 136 */ "datatype_buffer ::= BUFFER LT BOOL GT",
 /* 137 */ "datatype_buffer ::= BUFFER LT INT GT",
 /* 138 */ "datatype_buffer ::= BUFFER LT UINT GT",
 /* 139 */ "datatype_buffer ::= BUFFER LT HALF GT",
 /* 140 */ "datatype_buffer ::= BUFFER LT FLOAT GT",
 /* 141 */ "datatype_buffer ::= BUFFER LT DOUBLE GT",
 /* 142 */ "datatype_buffer ::= BUFFER LT SNORM FLOAT GT",
 /* 143 */ "datatype_buffer ::= BUFFER LT UNORM FLOAT GT",
 /* 144 */ "datatype_vector ::= VECTOR LT datatype_scalar COMMA INT_CONSTANT GT",
 /* 145 */ "datatype_matrix ::= MATRIX LT datatype_scalar COMMA INT_CONSTANT COMMA INT_CONSTANT GT",
 /* 146 */ "statement_block ::= LBRACE RBRACE",
 /* 147 */ "statement_block ::= LBRACE statement_list RBRACE",
 /* 148 */ "statement_list ::= statement",
 /* 149 */ "statement_list ::= statement_list statement",
 /* 150 */ "statement_attribute ::= ISOLATE",
 /* 151 */ "statement_attribute ::= MAXINSTRUCTIONCOUNT LPAREN INT_CONSTANT RPAREN",
 /* 152 */ "statement_attribute ::= NOEXPRESSIONOPTIMIZATIONS",
 /* 153 */ "statement_attribute ::= REMOVEUNUSEDINPUTS",
 /* 154 */ "statement_attribute ::= UNUSED",
 /* 155 */ "statement_attribute ::= XPS",
 /* 156 */ "statement ::= BREAK SEMICOLON",
 /* 157 */ "statement ::= CONTINUE SEMICOLON",
 /* 158 */ "statement ::= DISCARD SEMICOLON",
 /* 159 */ "statement ::= LBRACKET statement_attribute RBRACKET statement_block",
 /* 160 */ "statement ::= variable_declaration",
 /* 161 */ "statement ::= struct_declaration SEMICOLON",
 /* 162 */ "statement ::= do_intro DO statement WHILE LPAREN expression RPAREN SEMICOLON",
 /* 163 */ "statement ::= while_intro LPAREN expression RPAREN statement",
 /* 164 */ "statement ::= if_intro LPAREN expression RPAREN statement",
 /* 165 */ "statement ::= if_intro LPAREN expression RPAREN statement ELSE statement",
 /* 166 */ "statement ::= switch_intro LPAREN expression RPAREN LBRACE switch_case_list RBRACE",
 /* 167 */ "statement ::= typedef",
 /* 168 */ "statement ::= SEMICOLON",
 /* 169 */ "statement ::= expression SEMICOLON",
 /* 170 */ "statement ::= RETURN SEMICOLON",
 /* 171 */ "statement ::= RETURN expression SEMICOLON",
 /* 172 */ "statement ::= statement_block",
 /* 173 */ "statement ::= for_statement",
 /* 174 */ "while_intro ::= LBRACKET UNROLL LPAREN INT_CONSTANT RPAREN RBRACKET WHILE",
 /* 175 */ "while_intro ::= LBRACKET UNROLL RBRACKET WHILE",
 /* 176 */ "while_intro ::= LBRACKET LOOP RBRACKET WHILE",
 /* 177 */ "while_intro ::= WHILE",
 /* 178 */ "for_statement ::= for_intro for_details",
 /* 179 */ "for_intro ::= LBRACKET UNROLL LPAREN INT_CONSTANT RPAREN RBRACKET FOR",
 /* 180 */ "for_intro ::= LBRACKET UNROLL RBRACKET FOR",
 /* 181 */ "for_intro ::= LBRACKET LOOP RBRACKET FOR",
 /* 182 */ "for_intro ::= FOR",
 /* 183 */ "for_details ::= LPAREN expression SEMICOLON expression SEMICOLON expression RPAREN statement",
 /* 184 */ "for_details ::= LPAREN SEMICOLON SEMICOLON RPAREN statement",
 /* 185 */ "for_details ::= LPAREN SEMICOLON SEMICOLON expression RPAREN statement",
 /* 186 */ "for_details ::= LPAREN SEMICOLON expression SEMICOLON RPAREN statement",
 /* 187 */ "for_details ::= LPAREN SEMICOLON expression SEMICOLON expression RPAREN statement",
 /* 188 */ "for_details ::= LPAREN expression SEMICOLON SEMICOLON RPAREN statement",
 /* 189 */ "for_details ::= LPAREN expression SEMICOLON SEMICOLON expression RPAREN statement",
 /* 190 */ "for_details ::= LPAREN expression SEMICOLON expression SEMICOLON RPAREN statement",
 /* 191 */ "for_details ::= LPAREN variable_declaration expression SEMICOLON expression RPAREN statement",
 /* 192 */ "for_details ::= LPAREN variable_declaration SEMICOLON RPAREN statement",
 /* 193 */ "for_details ::= LPAREN variable_declaration SEMICOLON expression RPAREN statement",
 /* 194 */ "for_details ::= LPAREN variable_declaration expression SEMICOLON RPAREN statement",
 /* 195 */ "do_intro ::= LBRACKET UNROLL LPAREN INT_CONSTANT RPAREN RBRACKET DO",
 /* 196 */ "do_intro ::= LBRACKET UNROLL RBRACKET DO",
 /* 197 */ "do_intro ::= LBRACKET LOOP RBRACKET DO",
 /* 198 */ "do_intro ::= DO",
 /* 199 */ "if_intro ::= LBRACKET BRANCH RBRACKET IF",
 /* 200 */ "if_intro ::= LBRACKET FLATTEN RBRACKET IF",
 /* 201 */ "if_intro ::= LBRACKET IFALL RBRACKET IF",
 /* 202 */ "if_intro ::= LBRACKET IFANY RBRACKET IF",
 /* 203 */ "if_intro ::= LBRACKET PREDICATE RBRACKET IF",
 /* 204 */ "if_intro ::= LBRACKET PREDICATEBLOCK RBRACKET IF",
 /* 205 */ "if_intro ::= IF",
 /* 206 */ "switch_intro ::= LBRACKET FLATTEN RBRACKET SWITCH",
 /* 207 */ "switch_intro ::= LBRACKET BRANCH RBRACKET SWITCH",
 /* 208 */ "switch_intro ::= LBRACKET FORCECASE RBRACKET SWITCH",
 /* 209 */ "switch_intro ::= LBRACKET CALL RBRACKET SWITCH",
 /* 210 */ "switch_intro ::= SWITCH",
 /* 211 */ "switch_case_list ::= switch_case",
 /* 212 */ "switch_case_list ::= switch_case_list switch_case",
 /* 213 */ "switch_case ::= CASE expression COLON statement_list",
 /* 214 */ "switch_case ::= CASE expression COLON",
 /* 215 */ "switch_case ::= DEFAULT COLON statement_list",
 /* 216 */ "switch_case ::= DEFAULT COLON",
 /* 217 */ "primary_expr ::= IDENTIFIER",
 /* 218 */ "primary_expr ::= INT_CONSTANT",
 /* 219 */ "primary_expr ::= FLOAT_CONSTANT",
 /* 220 */ "primary_expr ::= STRING_LITERAL",
 /* 221 */ "primary_expr ::= TRUE",
 /* 222 */ "primary_expr ::= FALSE",
 /* 223 */ "primary_expr ::= LPAREN expression RPAREN",
 /* 224 */ "postfix_expr ::= primary_expr",
 /* 225 */ "postfix_expr ::= postfix_expr LBRACKET expression RBRACKET",
 /* 226 */ "postfix_expr ::= IDENTIFIER arguments",
 /* 227 */ "postfix_expr ::= datatype arguments",
 /* 228 */ "postfix_expr ::= postfix_expr DOT IDENTIFIER",
 /* 229 */ "postfix_expr ::= postfix_expr PLUSPLUS",
 /* 230 */ "postfix_expr ::= postfix_expr MINUSMINUS",
 /* 231 */ "arguments ::= LPAREN RPAREN",
 /* 232 */ "arguments ::= LPAREN argument_list RPAREN",
 /* 233 */ "argument_list ::= assignment_expr",
 /* 234 */ "argument_list ::= argument_list COMMA assignment_expr",
 /* 235 */ "unary_expr ::= postfix_expr",
 /* 236 */ "unary_expr ::= PLUSPLUS unary_expr",
 /* 237 */ "unary_expr ::= MINUSMINUS unary_expr",
 /* 238 */ "unary_expr ::= PLUS cast_expr",
 /* 239 */ "unary_expr ::= MINUS cast_expr",
 /* 240 */ "unary_expr ::= COMPLEMENT cast_expr",
 /* 241 */ "unary_expr ::= EXCLAMATION cast_expr",
 /* 242 */ "cast_expr ::= unary_expr",
 /* 243 */ "cast_expr ::= LPAREN datatype RPAREN cast_expr",
 /* 244 */ "multiplicative_expr ::= cast_expr",
 /* 245 */ "multiplicative_expr ::= multiplicative_expr STAR cast_expr",
 /* 246 */ "multiplicative_expr ::= multiplicative_expr SLASH cast_expr",
 /* 247 */ "multiplicative_expr ::= multiplicative_expr PERCENT cast_expr",
 /* 248 */ "additive_expr ::= multiplicative_expr",
 /* 249 */ "additive_expr ::= additive_expr PLUS multiplicative_expr",
 /* 250 */ "additive_expr ::= additive_expr MINUS multiplicative_expr",
 /* 251 */ "shift_expr ::= additive_expr",
 /* 252 */ "shift_expr ::= shift_expr LSHIFT additive_expr",
 /* 253 */ "shift_expr ::= shift_expr RSHIFT additive_expr",
 /* 254 */ "relational_expr ::= shift_expr",
 /* 255 */ "relational_expr ::= relational_expr LT shift_expr",
 /* 256 */ "relational_expr ::= relational_expr GT shift_expr",
 /* 257 */ "relational_expr ::= relational_expr LEQ shift_expr",
 /* 258 */ "relational_expr ::= relational_expr GEQ shift_expr",
 /* 259 */ "equality_expr ::= relational_expr",
 /* 260 */ "equality_expr ::= equality_expr EQL relational_expr",
 /* 261 */ "equality_expr ::= equality_expr NEQ relational_expr",
 /* 262 */ "and_expr ::= equality_expr",
 /* 263 */ "and_expr ::= and_expr AND equality_expr",
 /* 264 */ "exclusive_or_expr ::= and_expr",
 /* 265 */ "exclusive_or_expr ::= exclusive_or_expr XOR and_expr",
 /* 266 */ "inclusive_or_expr ::= exclusive_or_expr",
 /* 267 */ "inclusive_or_expr ::= inclusive_or_expr OR exclusive_or_expr",
 /* 268 */ "logical_and_expr ::= inclusive_or_expr",
 /* 269 */ "logical_and_expr ::= logical_and_expr ANDAND inclusive_or_expr",
 /* 270 */ "logical_or_expr ::= logical_and_expr",
 /* 271 */ "logical_or_expr ::= logical_or_expr OROR logical_and_expr",
 /* 272 */ "conditional_expr ::= logical_or_expr",
 /* 273 */ "conditional_expr ::= logical_or_expr QUESTION logical_or_expr COLON conditional_expr",
 /* 274 */ "assignment_expr ::= conditional_expr",
 /* 275 */ "assignment_expr ::= unary_expr ASSIGN assignment_expr",
 /* 276 */ "assignment_expr ::= unary_expr MULASSIGN assignment_expr",
 /* 277 */ "assignment_expr ::= unary_expr DIVASSIGN assignment_expr",
 /* 278 */ "assignment_expr ::= unary_expr MODASSIGN assignment_expr",
 /* 279 */ "assignment_expr ::= unary_expr ADDASSIGN assignment_expr",
 /* 280 */ "assignment_expr ::= unary_expr SUBASSIGN assignment_expr",
 /* 281 */ "assignment_expr ::= unary_expr LSHIFTASSIGN assignment_expr",
 /* 282 */ "assignment_expr ::= unary_expr RSHIFTASSIGN assignment_expr",
 /* 283 */ "assignment_expr ::= unary_expr ANDASSIGN assignment_expr",
 /* 284 */ "assignment_expr ::= unary_expr XORASSIGN assignment_expr",
 /* 285 */ "assignment_expr ::= unary_expr ORASSIGN assignment_expr",
 /* 286 */ "expression ::= assignment_expr",
 /* 287 */ "expression ::= expression COMMA assignment_expr",
};
#endif /* NDEBUG */


#if YYSTACKDEPTH<=0
/*
** Try to increase the size of the parser stack.
*/
static void yyGrowStack(yyParser *p){
  int newSize;
  yyStackEntry *pNew;

  newSize = p->yystksz*2 + 100;
  pNew = realloc(p->yystack, newSize*sizeof(pNew[0]));
  if( pNew ){
    p->yystack = pNew;
    p->yystksz = newSize;
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
    if( yyTraceFILE ){
      fprintf(yyTraceFILE,"%sStack grows to %d entries!\n",
              yyTracePrompt, p->yystksz);
    }
#endif
  }
}
#endif

/* 
** This function allocates a new parser.
** The only argument is a pointer to a function which works like
** malloc.
**
** Inputs:
** A pointer to the function used to allocate memory.
**
** Outputs:
** A pointer to a parser.  This pointer is used in subsequent calls
** to ParseHLSL and ParseHLSLFree.
*/
#if __MOJOSHADER__
static void *ParseHLSLAlloc(void *(*mallocProc)(int,void *), void *malloc_data){
  yyParser *pParser;
  pParser = (yyParser*)(*mallocProc)( (int)sizeof(yyParser), malloc_data );
#else
void *ParseHLSLAlloc(void *(*mallocProc)(size_t)){
  yyParser *pParser;
  pParser = (yyParser*)(*mallocProc)( (size_t)sizeof(yyParser) );
#endif
  if( pParser ){
    pParser->yyidx = -1;
#ifdef YYTRACKMAXSTACKDEPTH
    pParser->yyidxMax = 0;
#endif
#if YYSTACKDEPTH<=0
    pParser->yystack = NULL;
    pParser->yystksz = 0;
    yyGrowStack(pParser);
#endif
  }
  return pParser;
}

/* The following function deletes the value associated with a
** symbol.  The symbol can be either a terminal or nonterminal.
** "yymajor" is the symbol code, and "yypminor" is a pointer to
** the value.
*/
static void yy_destructor(
  yyParser *yypParser,    /* The parser */
  YYCODETYPE yymajor,     /* Type code for object to destroy */
  YYMINORTYPE *yypminor   /* The object to be destroyed */
){
  ParseHLSLARG_FETCH;
  switch( yymajor ){
    /* Here is inserted the actions which take place when a
    ** terminal or non-terminal is destroyed.  This can happen
    ** when the symbol is popped from the stack during a
    ** reduce or during error processing or when a parser is 
    ** being destroyed before it is finished parsing.
    **
    ** Note: during a reduce, the only symbols destroyed are those
    ** which appear on the RHS of the rule, but which are not used
    ** inside the C code.
    */
    case 125: /* compilation_units */
    case 126: /* compilation_unit */
{
#line 81 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_compilation_unit(ctx, (yypminor->yy139)); 
#line 2033 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 127: /* variable_declaration */
    case 144: /* variable_declaration_details_list */
    case 146: /* variable_declaration_details */
{
#line 175 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_variable_declaration(ctx, (yypminor->yy24)); 
#line 2042 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 128: /* function_signature */
    case 135: /* function_details */
{
#line 102 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_function_signature(ctx, (yypminor->yy364)); 
#line 2050 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 129: /* statement_block */
    case 167: /* statement_list */
    case 168: /* statement */
    case 175: /* for_statement */
    case 177: /* for_details */
{
#line 354 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_statement(ctx, (yypminor->yy233)); 
#line 2061 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 130: /* typedef */
{
#line 96 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_typedef(ctx, (yypminor->yy71)); 
#line 2068 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 131: /* struct_declaration */
{
#line 224 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_struct_declaration(ctx, (yypminor->yy249)); 
#line 2075 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 133: /* scalar_or_array */
{
#line 260 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_scalar_or_array(ctx, (yypminor->yy380)); 
#line 2082 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 137: /* function_parameters */
    case 138: /* function_parameter_list */
    case 139: /* function_parameter */
{
#line 124 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_function_params(ctx, (yypminor->yy307)); 
#line 2091 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 142: /* initializer */
    case 156: /* expression */
    case 160: /* initializer_block_list */
    case 161: /* initializer_block */
    case 179: /* primary_expr */
    case 180: /* postfix_expr */
    case 183: /* assignment_expr */
    case 184: /* unary_expr */
    case 185: /* cast_expr */
    case 186: /* multiplicative_expr */
    case 187: /* additive_expr */
    case 188: /* shift_expr */
    case 189: /* relational_expr */
    case 190: /* equality_expr */
    case 191: /* and_expr */
    case 192: /* exclusive_or_expr */
    case 193: /* inclusive_or_expr */
    case 194: /* logical_and_expr */
    case 195: /* logical_or_expr */
    case 196: /* conditional_expr */
{
#line 301 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_expr(ctx, (yypminor->yy322)); 
#line 2117 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 147: /* annotations */
    case 157: /* annotation_list */
    case 158: /* annotation */
{
#line 277 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_annotation(ctx, (yypminor->yy268)); 
#line 2126 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 148: /* variable_lowlevel */
{
#line 252 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_variable_lowlevel(ctx, (yypminor->yy82)); 
#line 2133 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 150: /* struct_member_list */
    case 151: /* struct_member */
    case 152: /* struct_member_details */
    case 153: /* struct_member_item_list */
{
#line 232 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_struct_member(ctx, (yypminor->yy346)); 
#line 2143 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 154: /* packoffset */
{
#line 266 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_pack_offset(ctx, (yypminor->yy8)); 
#line 2150 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 174: /* switch_case_list */
    case 178: /* switch_case */
{
#line 450 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_switch_case(ctx, (yypminor->yy165)); 
#line 2158 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    case 181: /* arguments */
    case 182: /* argument_list */
{
#line 485 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
 delete_arguments(ctx, (yypminor->yy26)); 
#line 2166 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
}
      break;
    default:  break;   /* If no destructor action specified: do nothing */
  }
}

/*
** Pop the parser's stack once.
**
** If there is a destructor routine associated with the token which
** is popped from the stack, then call it.
**
** Return the major token number for the symbol popped.
*/
static int yy_pop_parser_stack(yyParser *pParser){
  YYCODETYPE yymajor;
  yyStackEntry *yytos = &pParser->yystack[pParser->yyidx];

  if( pParser->yyidx<0 ) return 0;
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
  if( yyTraceFILE && pParser->yyidx>=0 ){
    fprintf(yyTraceFILE,"%sPopping %s\n",
      yyTracePrompt,
      yyTokenName[yytos->major]);
  }
#endif
  yymajor = yytos->major;
  yy_destructor(pParser, yymajor, &yytos->minor);
  pParser->yyidx--;
  return yymajor;
}

/* 
** Deallocate and destroy a parser.  Destructors are all called for
** all stack elements before shutting the parser down.
**
** Inputs:
** <ul>
** <li>  A pointer to the parser.  This should be a pointer
**       obtained from ParseHLSLAlloc.
** <li>  A pointer to a function used to reclaim memory obtained
**       from malloc.
** </ul>
*/
#if __MOJOSHADER__
static
#endif
void ParseHLSLFree(
  void *p,                    /* The parser to be deleted */
#if __MOJOSHADER__
  void (*freeProc)(void*,void*),     /* Function used to reclaim memory */
  void *malloc_data
#else
  void (*freeProc)(void*)     /* Function used to reclaim memory */
#endif
){
  yyParser *pParser = (yyParser*)p;
  if( pParser==0 ) return;
  while( pParser->yyidx>=0 ) yy_pop_parser_stack(pParser);
#if YYSTACKDEPTH<=0
  free(pParser->yystack);
#endif
#if __MOJOSHADER__
  (*freeProc)((void*)pParser, malloc_data);
#else
  (*freeProc)((void*)pParser);
#endif
}

/*
** Return the peak depth of the stack for a parser.
*/
#ifdef YYTRACKMAXSTACKDEPTH
static int ParseHLSLStackPeak(void *p){
  yyParser *pParser = (yyParser*)p;
  return pParser->yyidxMax;
}
#endif

/*
** Find the appropriate action for a parser given the terminal
** look-ahead token iLookAhead.
**
** If the look-ahead token is YYNOCODE, then check to see if the action is
** independent of the look-ahead.  If it is, return the action, otherwise
** return YY_NO_ACTION.
*/
static int yy_find_shift_action(
  yyParser *pParser,        /* The parser */
  YYCODETYPE iLookAhead     /* The look-ahead token */
){
  int i;
  int stateno = pParser->yystack[pParser->yyidx].stateno;
 
  if( stateno>YY_SHIFT_COUNT
   || (i = yy_shift_ofst[stateno])==YY_SHIFT_USE_DFLT ){
    return yy_default[stateno];
  }
  assert( iLookAhead!=YYNOCODE );
  i += iLookAhead;
  if( i<0 || i>=YY_ACTTAB_COUNT || yy_lookahead[i]!=iLookAhead ){
    if( iLookAhead>0 ){
#ifdef YYFALLBACK
      YYCODETYPE iFallback;            /* Fallback token */
      if( iLookAhead<sizeof(yyFallback)/sizeof(yyFallback[0])
             && (iFallback = yyFallback[iLookAhead])!=0 ){
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
        if( yyTraceFILE ){
          fprintf(yyTraceFILE, "%sFALLBACK %s => %s\n",
             yyTracePrompt, yyTokenName[iLookAhead], yyTokenName[iFallback]);
        }
#endif
        return yy_find_shift_action(pParser, iFallback);
      }
#endif
#ifdef YYWILDCARD
      {
        int j = i - iLookAhead + YYWILDCARD;
        if( 
#if YY_SHIFT_MIN+YYWILDCARD<0
          j>=0 &&
#endif
#if YY_SHIFT_MAX+YYWILDCARD>=YY_ACTTAB_COUNT
          j<YY_ACTTAB_COUNT &&
#endif
          yy_lookahead[j]==YYWILDCARD
        ){
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
          if( yyTraceFILE ){
            fprintf(yyTraceFILE, "%sWILDCARD %s => %s\n",
               yyTracePrompt, yyTokenName[iLookAhead], yyTokenName[YYWILDCARD]);
          }
#endif /* NDEBUG */
          return yy_action[j];
        }
      }
#endif /* YYWILDCARD */
    }
    return yy_default[stateno];
  }else{
    return yy_action[i];
  }
}

/*
** Find the appropriate action for a parser given the non-terminal
** look-ahead token iLookAhead.
**
** If the look-ahead token is YYNOCODE, then check to see if the action is
** independent of the look-ahead.  If it is, return the action, otherwise
** return YY_NO_ACTION.
*/
static int yy_find_reduce_action(
  int stateno,              /* Current state number */
  YYCODETYPE iLookAhead     /* The look-ahead token */
){
  int i;
#ifdef YYERRORSYMBOL
  if( stateno>YY_REDUCE_COUNT ){
    return yy_default[stateno];
  }
#else
  assert( stateno<=YY_REDUCE_COUNT );
#endif
  i = yy_reduce_ofst[stateno];
  assert( i!=YY_REDUCE_USE_DFLT );
  assert( iLookAhead!=YYNOCODE );
  i += iLookAhead;
#ifdef YYERRORSYMBOL
  if( i<0 || i>=YY_ACTTAB_COUNT || yy_lookahead[i]!=iLookAhead ){
    return yy_default[stateno];
  }
#else
  assert( i>=0 && i<YY_ACTTAB_COUNT );
  assert( yy_lookahead[i]==iLookAhead );
#endif
  return yy_action[i];
}

/*
** The following routine is called if the stack overflows.
*/
static void yyStackOverflow(yyParser *yypParser, YYMINORTYPE *yypMinor){
   ParseHLSLARG_FETCH;
   yypParser->yyidx--;
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
   if( yyTraceFILE ){
     fprintf(yyTraceFILE,"%sStack Overflow!\n",yyTracePrompt);
   }
#endif
   while( yypParser->yyidx>=0 ) yy_pop_parser_stack(yypParser);
   /* Here code is inserted which will execute if the parser
   ** stack every overflows */
#line 47 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"

    // !!! FIXME: make this a proper fail() function.
    fail(ctx, "Giving up. Parser stack overflow");
#line 2364 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
   ParseHLSLARG_STORE; /* Suppress warning about unused %extra_argument var */
}

/*
** Perform a shift action.
*/
static void yy_shift(
  yyParser *yypParser,          /* The parser to be shifted */
  int yyNewState,               /* The new state to shift in */
  int yyMajor,                  /* The major token to shift in */
  YYMINORTYPE *yypMinor         /* Pointer to the minor token to shift in */
){
  yyStackEntry *yytos;
  yypParser->yyidx++;
#ifdef YYTRACKMAXSTACKDEPTH
  if( yypParser->yyidx>yypParser->yyidxMax ){
    yypParser->yyidxMax = yypParser->yyidx;
  }
#endif
#if YYSTACKDEPTH>0 
  if( yypParser->yyidx>=YYSTACKDEPTH ){
    yyStackOverflow(yypParser, yypMinor);
    return;
  }
#else
  if( yypParser->yyidx>=yypParser->yystksz ){
    yyGrowStack(yypParser);
    if( yypParser->yyidx>=yypParser->yystksz ){
      yyStackOverflow(yypParser, yypMinor);
      return;
    }
  }
#endif
  yytos = &yypParser->yystack[yypParser->yyidx];
  yytos->stateno = (YYACTIONTYPE)yyNewState;
  yytos->major = (YYCODETYPE)yyMajor;
  yytos->minor = *yypMinor;
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
  if( yyTraceFILE && yypParser->yyidx>0 ){
    int i;
    fprintf(yyTraceFILE,"%sShift %d\n",yyTracePrompt,yyNewState);
    fprintf(yyTraceFILE,"%sStack:",yyTracePrompt);
    for(i=1; i<=yypParser->yyidx; i++)
      fprintf(yyTraceFILE," %s",yyTokenName[yypParser->yystack[i].major]);
    fprintf(yyTraceFILE,"\n");
  }
#endif
}

/* The following table contains information about every rule that
** is used during the reduce.
*/
static const struct {
  YYCODETYPE lhs;         /* Symbol on the left-hand side of the rule */
  unsigned char nrhs;     /* Number of right-hand side symbols in the rule */
} yyRuleInfo[] = {
  { 124, 1 },
  { 125, 1 },
  { 125, 2 },
  { 126, 1 },
  { 126, 2 },
  { 126, 2 },
  { 126, 1 },
  { 126, 2 },
  { 130, 4 },
  { 130, 3 },
  { 128, 3 },
  { 128, 2 },
  { 128, 2 },
  { 128, 1 },
  { 135, 5 },
  { 135, 5 },
  { 134, 1 },
  { 137, 1 },
  { 137, 1 },
  { 137, 0 },
  { 138, 1 },
  { 138, 3 },
  { 139, 6 },
  { 139, 5 },
  { 139, 5 },
  { 139, 4 },
  { 139, 5 },
  { 139, 4 },
  { 139, 4 },
  { 139, 3 },
  { 139, 5 },
  { 139, 4 },
  { 139, 4 },
  { 139, 3 },
  { 139, 4 },
  { 139, 3 },
  { 139, 3 },
  { 139, 2 },
  { 140, 1 },
  { 140, 1 },
  { 140, 1 },
  { 140, 2 },
  { 140, 2 },
  { 140, 1 },
  { 136, 2 },
  { 141, 1 },
  { 141, 1 },
  { 141, 1 },
  { 141, 1 },
  { 141, 1 },
  { 127, 4 },
  { 127, 3 },
  { 127, 3 },
  { 143, 1 },
  { 143, 2 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 145, 1 },
  { 144, 1 },
  { 144, 3 },
  { 146, 5 },
  { 146, 4 },
  { 146, 4 },
  { 146, 3 },
  { 146, 4 },
  { 146, 3 },
  { 146, 3 },
  { 146, 2 },
  { 146, 4 },
  { 146, 3 },
  { 146, 3 },
  { 146, 2 },
  { 146, 3 },
  { 146, 2 },
  { 146, 2 },
  { 146, 1 },
  { 131, 4 },
  { 149, 2 },
  { 150, 1 },
  { 150, 2 },
  { 151, 2 },
  { 151, 1 },
  { 152, 3 },
  { 153, 1 },
  { 153, 2 },
  { 153, 3 },
  { 148, 2 },
  { 148, 2 },
  { 148, 1 },
  { 148, 1 },
  { 133, 3 },
  { 133, 4 },
  { 133, 1 },
  { 154, 7 },
  { 154, 5 },
  { 155, 5 },
  { 147, 3 },
  { 157, 1 },
  { 157, 2 },
  { 158, 3 },
  { 160, 1 },
  { 160, 3 },
  { 160, 3 },
  { 161, 3 },
  { 142, 2 },
  { 142, 2 },
  { 162, 1 },
  { 162, 1 },
  { 162, 1 },
  { 162, 1 },
  { 162, 1 },
  { 132, 1 },
  { 132, 1 },
  { 165, 1 },
  { 165, 1 },
  { 165, 1 },
  { 165, 1 },
  { 165, 1 },
  { 165, 1 },
  { 165, 1 },
  { 165, 1 },
  { 159, 1 },
  { 159, 1 },
  { 159, 1 },
  { 159, 1 },
  { 159, 1 },
  { 159, 1 },
  { 159, 1 },
  { 159, 2 },
  { 159, 2 },
  { 166, 4 },
  { 166, 4 },
  { 166, 4 },
  { 166, 4 },
  { 166, 4 },
  { 166, 4 },
  { 166, 5 },
  { 166, 5 },
  { 163, 6 },
  { 164, 8 },
  { 129, 2 },
  { 129, 3 },
  { 167, 1 },
  { 167, 2 },
  { 169, 1 },
  { 169, 4 },
  { 169, 1 },
  { 169, 1 },
  { 169, 1 },
  { 169, 1 },
  { 168, 2 },
  { 168, 2 },
  { 168, 2 },
  { 168, 4 },
  { 168, 1 },
  { 168, 2 },
  { 168, 8 },
  { 168, 5 },
  { 168, 5 },
  { 168, 7 },
  { 168, 7 },
  { 168, 1 },
  { 168, 1 },
  { 168, 2 },
  { 168, 2 },
  { 168, 3 },
  { 168, 1 },
  { 168, 1 },
  { 171, 7 },
  { 171, 4 },
  { 171, 4 },
  { 171, 1 },
  { 175, 2 },
  { 176, 7 },
  { 176, 4 },
  { 176, 4 },
  { 176, 1 },
  { 177, 8 },
  { 177, 5 },
  { 177, 6 },
  { 177, 6 },
  { 177, 7 },
  { 177, 6 },
  { 177, 7 },
  { 177, 7 },
  { 177, 7 },
  { 177, 5 },
  { 177, 6 },
  { 177, 6 },
  { 170, 7 },
  { 170, 4 },
  { 170, 4 },
  { 170, 1 },
  { 172, 4 },
  { 172, 4 },
  { 172, 4 },
  { 172, 4 },
  { 172, 4 },
  { 172, 4 },
  { 172, 1 },
  { 173, 4 },
  { 173, 4 },
  { 173, 4 },
  { 173, 4 },
  { 173, 1 },
  { 174, 1 },
  { 174, 2 },
  { 178, 4 },
  { 178, 3 },
  { 178, 3 },
  { 178, 2 },
  { 179, 1 },
  { 179, 1 },
  { 179, 1 },
  { 179, 1 },
  { 179, 1 },
  { 179, 1 },
  { 179, 3 },
  { 180, 1 },
  { 180, 4 },
  { 180, 2 },
  { 180, 2 },
  { 180, 3 },
  { 180, 2 },
  { 180, 2 },
  { 181, 2 },
  { 181, 3 },
  { 182, 1 },
  { 182, 3 },
  { 184, 1 },
  { 184, 2 },
  { 184, 2 },
  { 184, 2 },
  { 184, 2 },
  { 184, 2 },
  { 184, 2 },
  { 185, 1 },
  { 185, 4 },
  { 186, 1 },
  { 186, 3 },
  { 186, 3 },
  { 186, 3 },
  { 187, 1 },
  { 187, 3 },
  { 187, 3 },
  { 188, 1 },
  { 188, 3 },
  { 188, 3 },
  { 189, 1 },
  { 189, 3 },
  { 189, 3 },
  { 189, 3 },
  { 189, 3 },
  { 190, 1 },
  { 190, 3 },
  { 190, 3 },
  { 191, 1 },
  { 191, 3 },
  { 192, 1 },
  { 192, 3 },
  { 193, 1 },
  { 193, 3 },
  { 194, 1 },
  { 194, 3 },
  { 195, 1 },
  { 195, 3 },
  { 196, 1 },
  { 196, 5 },
  { 183, 1 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 183, 3 },
  { 156, 1 },
  { 156, 3 },
};

static void yy_accept(yyParser*);  /* Forward Declaration */

/*
** Perform a reduce action and the shift that must immediately
** follow the reduce.
*/
static void yy_reduce(
  yyParser *yypParser,         /* The parser */
  int yyruleno                 /* Number of the rule by which to reduce */
){
  int yygoto;                     /* The next state */
  int yyact;                      /* The next action */
  YYMINORTYPE yygotominor;        /* The LHS of the rule reduced */
  yyStackEntry *yymsp;            /* The top of the parser's stack */
  int yysize;                     /* Amount to pop the stack */
  ParseHLSLARG_FETCH;
  yymsp = &yypParser->yystack[yypParser->yyidx];
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
  if( yyTraceFILE && yyruleno>=0
        && yyruleno<(int)(sizeof(yyRuleName)/sizeof(yyRuleName[0])) ){
    fprintf(yyTraceFILE, "%sReduce [%s].\n", yyTracePrompt,
      yyRuleName[yyruleno]);
  }
#endif /* NDEBUG */

  /* Silence complaints from purify about yygotominor being uninitialized
  ** in some cases when it is copied into the stack after the following
  ** switch.  yygotominor is uninitialized when a rule reduces that does
  ** not set the value of its left-hand side nonterminal.  Leaving the
  ** value of the nonterminal uninitialized is utterly harmless as long
  ** as the value is never used.  So really the only thing this code
  ** accomplishes is to quieten purify.  
  **
  ** 2007-01-16:  The wireshark project (www.wireshark.org) reports that
  ** without this code, their parser segfaults.  I'm not sure what there
  ** parser is doing to make this happen.  This is the second bug report
  ** from wireshark this week.  Clearly they are stressing Lemon in ways
  ** that it has not been previously stressed...  (SQLite ticket #2172)
  */
  /*memset(&yygotominor, 0, sizeof(yygotominor));*/
  yygotominor = yyzerominor;


  switch( yyruleno ){
  /* Beginning here are the reduction cases.  A typical example
  ** follows:
  **   case 0:
  **  #line <lineno> <grammarfile>
  **     { ... }           // User supplied code
  **  #line <lineno> <thisfile>
  **     break;
  */
      case 0: /* shader ::= compilation_units */
#line 78 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ assert(ctx->ast == NULL); REVERSE_LINKED_LIST(MOJOSHADER_astCompilationUnit, yymsp[0].minor.yy139); ctx->ast = (MOJOSHADER_astNode *) yymsp[0].minor.yy139; }
#line 2766 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 1: /* compilation_units ::= compilation_unit */
#line 82 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy139 = yymsp[0].minor.yy139; }
#line 2771 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 2: /* compilation_units ::= compilation_units compilation_unit */
#line 83 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ if (yymsp[0].minor.yy139) { yymsp[0].minor.yy139->next = yymsp[-1].minor.yy139; yygotominor.yy139 = yymsp[0].minor.yy139; } }
#line 2776 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 3: /* compilation_unit ::= variable_declaration */
#line 88 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy139 = new_global_variable(ctx, yymsp[0].minor.yy24); }
#line 2781 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 4: /* compilation_unit ::= function_signature SEMICOLON */
#line 89 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy139 = new_function(ctx, yymsp[-1].minor.yy364, NULL); }
#line 2786 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 5: /* compilation_unit ::= function_signature statement_block */
#line 90 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy139 = new_function(ctx, yymsp[-1].minor.yy364, yymsp[0].minor.yy233); }
#line 2791 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 6: /* compilation_unit ::= typedef */
#line 91 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy139 = new_global_typedef(ctx, yymsp[0].minor.yy71); }
#line 2796 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 7: /* compilation_unit ::= struct_declaration SEMICOLON */
#line 92 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy139 = new_global_struct(ctx, yymsp[-1].minor.yy249); }
#line 2801 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 8: /* typedef ::= TYPEDEF CONST datatype scalar_or_array */
#line 98 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy71 = new_typedef(ctx, 1, yymsp[-1].minor.yy37, yymsp[0].minor.yy380); push_usertype(ctx, yymsp[0].minor.yy380->identifier, yygotominor.yy71->datatype); }
#line 2806 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 9: /* typedef ::= TYPEDEF datatype scalar_or_array */
#line 99 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy71 = new_typedef(ctx, 0, yymsp[-1].minor.yy37, yymsp[0].minor.yy380); push_usertype(ctx, yymsp[0].minor.yy380->identifier, yygotominor.yy71->datatype); }
#line 2811 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 10: /* function_signature ::= function_storageclass function_details semantic */
#line 103 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy364 = yymsp[-1].minor.yy364; yygotominor.yy364->storage_class = yymsp[-2].minor.yy175; yygotominor.yy364->semantic = yymsp[0].minor.yy306; }
#line 2816 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 11: /* function_signature ::= function_storageclass function_details */
#line 104 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy364 = yymsp[0].minor.yy364; yygotominor.yy364->storage_class = yymsp[-1].minor.yy175; }
#line 2821 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 12: /* function_signature ::= function_details semantic */
#line 105 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy364 = yymsp[-1].minor.yy364; yygotominor.yy364->semantic = yymsp[0].minor.yy306; }
#line 2826 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 13: /* function_signature ::= function_details */
#line 106 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy364 = yymsp[0].minor.yy364; }
#line 2831 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 14: /* function_details ::= datatype IDENTIFIER LPAREN function_parameters RPAREN */
#line 110 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy364 = new_function_signature(ctx, yymsp[-4].minor.yy37, yymsp[-3].minor.yy0.string, yymsp[-1].minor.yy307); }
#line 2836 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 15: /* function_details ::= VOID IDENTIFIER LPAREN function_parameters RPAREN */
#line 111 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy364 = new_function_signature(ctx, NULL, yymsp[-3].minor.yy0.string, yymsp[-1].minor.yy307); }
#line 2841 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 16: /* function_storageclass ::= INLINE */
#line 121 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy175 = MOJOSHADER_AST_FNSTORECLS_INLINE; }
#line 2846 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 17: /* function_parameters ::= VOID */
      case 19: /* function_parameters ::= */ yytestcase(yyruleno==19);
#line 125 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = NULL; }
#line 2852 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 18: /* function_parameters ::= function_parameter_list */
#line 126 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astFunctionParameters, yymsp[0].minor.yy307); yygotominor.yy307 = yymsp[0].minor.yy307; }
#line 2857 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 20: /* function_parameter_list ::= function_parameter */
#line 131 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = yymsp[0].minor.yy307; }
#line 2862 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 21: /* function_parameter_list ::= function_parameter_list COMMA function_parameter */
#line 132 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yymsp[0].minor.yy307->next = yymsp[-2].minor.yy307; yygotominor.yy307 = yymsp[0].minor.yy307; }
#line 2867 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 22: /* function_parameter ::= input_modifier datatype IDENTIFIER semantic interpolation_mod initializer */
#line 138 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-5].minor.yy75, yymsp[-4].minor.yy37, yymsp[-3].minor.yy0.string, yymsp[-2].minor.yy306, yymsp[-1].minor.yy111, yymsp[0].minor.yy322); }
#line 2872 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 23: /* function_parameter ::= input_modifier datatype IDENTIFIER semantic interpolation_mod */
#line 139 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-4].minor.yy75, yymsp[-3].minor.yy37, yymsp[-2].minor.yy0.string, yymsp[-1].minor.yy306, yymsp[0].minor.yy111, NULL); }
#line 2877 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 24: /* function_parameter ::= input_modifier datatype IDENTIFIER semantic initializer */
#line 140 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-4].minor.yy75, yymsp[-3].minor.yy37, yymsp[-2].minor.yy0.string, yymsp[-1].minor.yy306, MOJOSHADER_AST_INTERPMOD_NONE, yymsp[0].minor.yy322); }
#line 2882 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 25: /* function_parameter ::= input_modifier datatype IDENTIFIER semantic */
#line 141 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-3].minor.yy75, yymsp[-2].minor.yy37, yymsp[-1].minor.yy0.string, yymsp[0].minor.yy306, MOJOSHADER_AST_INTERPMOD_NONE, NULL); }
#line 2887 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 26: /* function_parameter ::= input_modifier datatype IDENTIFIER interpolation_mod initializer */
#line 142 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-4].minor.yy75, yymsp[-3].minor.yy37, yymsp[-2].minor.yy0.string, NULL, yymsp[-1].minor.yy111, yymsp[0].minor.yy322); }
#line 2892 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 27: /* function_parameter ::= input_modifier datatype IDENTIFIER interpolation_mod */
#line 143 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-3].minor.yy75, yymsp[-2].minor.yy37, yymsp[-1].minor.yy0.string, NULL, yymsp[0].minor.yy111, NULL); }
#line 2897 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 28: /* function_parameter ::= input_modifier datatype IDENTIFIER initializer */
#line 144 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-3].minor.yy75, yymsp[-2].minor.yy37, yymsp[-1].minor.yy0.string, NULL, MOJOSHADER_AST_INTERPMOD_NONE, yymsp[0].minor.yy322); }
#line 2902 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 29: /* function_parameter ::= input_modifier datatype IDENTIFIER */
#line 145 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, yymsp[-2].minor.yy75, yymsp[-1].minor.yy37, yymsp[0].minor.yy0.string, NULL, MOJOSHADER_AST_INTERPMOD_NONE, NULL); }
#line 2907 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 30: /* function_parameter ::= datatype IDENTIFIER semantic interpolation_mod initializer */
#line 146 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-4].minor.yy37, yymsp[-3].minor.yy0.string, yymsp[-2].minor.yy306, yymsp[-1].minor.yy111, yymsp[0].minor.yy322); }
#line 2912 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 31: /* function_parameter ::= datatype IDENTIFIER semantic interpolation_mod */
#line 147 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-3].minor.yy37, yymsp[-2].minor.yy0.string, yymsp[-1].minor.yy306, yymsp[0].minor.yy111, NULL); }
#line 2917 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 32: /* function_parameter ::= datatype IDENTIFIER semantic initializer */
#line 148 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-3].minor.yy37, yymsp[-2].minor.yy0.string, yymsp[-1].minor.yy306, MOJOSHADER_AST_INTERPMOD_NONE, yymsp[0].minor.yy322); }
#line 2922 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 33: /* function_parameter ::= datatype IDENTIFIER semantic */
#line 149 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-2].minor.yy37, yymsp[-1].minor.yy0.string, yymsp[0].minor.yy306, MOJOSHADER_AST_INTERPMOD_NONE, NULL); }
#line 2927 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 34: /* function_parameter ::= datatype IDENTIFIER interpolation_mod initializer */
#line 150 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-3].minor.yy37, yymsp[-2].minor.yy0.string, NULL, yymsp[-1].minor.yy111, yymsp[0].minor.yy322); }
#line 2932 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 35: /* function_parameter ::= datatype IDENTIFIER interpolation_mod */
#line 151 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-2].minor.yy37, yymsp[-1].minor.yy0.string, NULL, yymsp[0].minor.yy111, NULL); }
#line 2937 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 36: /* function_parameter ::= datatype IDENTIFIER initializer */
#line 152 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-2].minor.yy37, yymsp[-1].minor.yy0.string, NULL, MOJOSHADER_AST_INTERPMOD_NONE, yymsp[0].minor.yy322); }
#line 2942 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 37: /* function_parameter ::= datatype IDENTIFIER */
#line 153 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy307 = new_function_param(ctx, MOJOSHADER_AST_INPUTMOD_NONE, yymsp[-1].minor.yy37, yymsp[0].minor.yy0.string, NULL, MOJOSHADER_AST_INTERPMOD_NONE, NULL); }
#line 2947 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 38: /* input_modifier ::= IN */
#line 156 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy75 = MOJOSHADER_AST_INPUTMOD_IN; }
#line 2952 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 39: /* input_modifier ::= INOUT */
      case 41: /* input_modifier ::= IN OUT */ yytestcase(yyruleno==41);
      case 42: /* input_modifier ::= OUT IN */ yytestcase(yyruleno==42);
#line 157 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy75 = MOJOSHADER_AST_INPUTMOD_INOUT; }
#line 2959 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 40: /* input_modifier ::= OUT */
#line 158 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy75 = MOJOSHADER_AST_INPUTMOD_OUT; }
#line 2964 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 43: /* input_modifier ::= UNIFORM */
#line 161 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy75 = MOJOSHADER_AST_INPUTMOD_UNIFORM; }
#line 2969 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 44: /* semantic ::= COLON IDENTIFIER */
#line 164 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy306 = yymsp[0].minor.yy0.string; }
#line 2974 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 45: /* interpolation_mod ::= LINEAR */
#line 168 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy111 = MOJOSHADER_AST_INTERPMOD_LINEAR; }
#line 2979 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 46: /* interpolation_mod ::= CENTROID */
#line 169 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy111 = MOJOSHADER_AST_INTERPMOD_CENTROID; }
#line 2984 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 47: /* interpolation_mod ::= NOINTERPOLATION */
#line 170 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy111 = MOJOSHADER_AST_INTERPMOD_NOINTERPOLATION; }
#line 2989 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 48: /* interpolation_mod ::= NOPERSPECTIVE */
#line 171 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy111 = MOJOSHADER_AST_INTERPMOD_NOPERSPECTIVE; }
#line 2994 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 49: /* interpolation_mod ::= SAMPLE */
#line 172 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy111 = MOJOSHADER_AST_INTERPMOD_SAMPLE; }
#line 2999 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 50: /* variable_declaration ::= variable_attribute_list datatype variable_declaration_details_list SEMICOLON */
#line 176 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astVariableDeclaration, yymsp[-1].minor.yy24); yygotominor.yy24 = yymsp[-1].minor.yy24; yygotominor.yy24->attributes = yymsp[-3].minor.yy270; yygotominor.yy24->datatype = yymsp[-2].minor.yy37; }
#line 3004 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 51: /* variable_declaration ::= datatype variable_declaration_details_list SEMICOLON */
#line 177 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astVariableDeclaration, yymsp[-1].minor.yy24); yygotominor.yy24 = yymsp[-1].minor.yy24; yygotominor.yy24->datatype = yymsp[-2].minor.yy37; }
#line 3009 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 52: /* variable_declaration ::= struct_declaration variable_declaration_details_list SEMICOLON */
#line 179 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astVariableDeclaration, yymsp[-1].minor.yy24); yygotominor.yy24 = yymsp[-1].minor.yy24; yygotominor.yy24->anonymous_datatype = yymsp[-2].minor.yy249; }
#line 3014 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 53: /* variable_attribute_list ::= variable_attribute */
#line 182 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = yymsp[0].minor.yy270; }
#line 3019 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 54: /* variable_attribute_list ::= variable_attribute_list variable_attribute */
#line 183 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = yymsp[-1].minor.yy270 | yymsp[0].minor.yy270; }
#line 3024 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 55: /* variable_attribute ::= EXTERN */
#line 186 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_EXTERN; }
#line 3029 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 56: /* variable_attribute ::= NOINTERPOLATION */
#line 187 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_NOINTERPOLATION; }
#line 3034 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 57: /* variable_attribute ::= SHARED */
#line 188 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_SHARED; }
#line 3039 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 58: /* variable_attribute ::= STATIC */
#line 189 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_STATIC; }
#line 3044 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 59: /* variable_attribute ::= UNIFORM */
#line 190 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_UNIFORM; }
#line 3049 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 60: /* variable_attribute ::= VOLATILE */
#line 191 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_VOLATILE; }
#line 3054 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 61: /* variable_attribute ::= CONST */
#line 192 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_CONST; }
#line 3059 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 62: /* variable_attribute ::= ROWMAJOR */
#line 193 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_ROWMAJOR; }
#line 3064 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 63: /* variable_attribute ::= COLUMNMAJOR */
#line 194 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_VARATTR_COLUMNMAJOR; }
#line 3069 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 64: /* variable_declaration_details_list ::= variable_declaration_details */
#line 198 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = yymsp[0].minor.yy24; }
#line 3074 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 65: /* variable_declaration_details_list ::= variable_declaration_details_list COMMA variable_declaration_details */
#line 199 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = yymsp[0].minor.yy24; yygotominor.yy24->next = yymsp[-2].minor.yy24; }
#line 3079 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 66: /* variable_declaration_details ::= scalar_or_array semantic annotations initializer variable_lowlevel */
#line 203 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-4].minor.yy380, yymsp[-3].minor.yy306, yymsp[-2].minor.yy268, yymsp[-1].minor.yy322, yymsp[0].minor.yy82); }
#line 3084 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 67: /* variable_declaration_details ::= scalar_or_array semantic annotations initializer */
#line 204 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-3].minor.yy380, yymsp[-2].minor.yy306, yymsp[-1].minor.yy268, yymsp[0].minor.yy322, NULL); }
#line 3089 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 68: /* variable_declaration_details ::= scalar_or_array semantic annotations variable_lowlevel */
#line 205 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-3].minor.yy380, yymsp[-2].minor.yy306, yymsp[-1].minor.yy268, NULL, yymsp[0].minor.yy82); }
#line 3094 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 69: /* variable_declaration_details ::= scalar_or_array semantic annotations */
#line 206 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-2].minor.yy380, yymsp[-1].minor.yy306, yymsp[0].minor.yy268, NULL, NULL); }
#line 3099 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 70: /* variable_declaration_details ::= scalar_or_array semantic initializer variable_lowlevel */
#line 207 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-3].minor.yy380, yymsp[-2].minor.yy306, NULL, yymsp[-1].minor.yy322, yymsp[0].minor.yy82); }
#line 3104 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 71: /* variable_declaration_details ::= scalar_or_array semantic initializer */
#line 208 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-2].minor.yy380, yymsp[-1].minor.yy306, NULL, yymsp[0].minor.yy322, NULL); }
#line 3109 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 72: /* variable_declaration_details ::= scalar_or_array semantic variable_lowlevel */
#line 209 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-2].minor.yy380, yymsp[-1].minor.yy306, NULL, NULL, yymsp[0].minor.yy82); }
#line 3114 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 73: /* variable_declaration_details ::= scalar_or_array semantic */
#line 210 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-1].minor.yy380, yymsp[0].minor.yy306, NULL, NULL, NULL); }
#line 3119 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 74: /* variable_declaration_details ::= scalar_or_array annotations initializer variable_lowlevel */
#line 211 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-3].minor.yy380, NULL, yymsp[-2].minor.yy268, yymsp[-1].minor.yy322, yymsp[0].minor.yy82); }
#line 3124 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 75: /* variable_declaration_details ::= scalar_or_array annotations initializer */
#line 212 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-2].minor.yy380, NULL, yymsp[-1].minor.yy268, yymsp[0].minor.yy322, NULL); }
#line 3129 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 76: /* variable_declaration_details ::= scalar_or_array annotations variable_lowlevel */
#line 213 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-2].minor.yy380, NULL, yymsp[-1].minor.yy268, NULL, yymsp[0].minor.yy82); }
#line 3134 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 77: /* variable_declaration_details ::= scalar_or_array annotations */
#line 214 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-1].minor.yy380, NULL, yymsp[0].minor.yy268, NULL, NULL); }
#line 3139 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 78: /* variable_declaration_details ::= scalar_or_array initializer variable_lowlevel */
#line 215 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-2].minor.yy380, NULL, NULL, yymsp[-1].minor.yy322, yymsp[0].minor.yy82); }
#line 3144 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 79: /* variable_declaration_details ::= scalar_or_array initializer */
#line 216 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-1].minor.yy380, NULL, NULL, yymsp[0].minor.yy322, NULL); }
#line 3149 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 80: /* variable_declaration_details ::= scalar_or_array variable_lowlevel */
#line 217 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[-1].minor.yy380, NULL, NULL, NULL, yymsp[0].minor.yy82); }
#line 3154 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 81: /* variable_declaration_details ::= scalar_or_array */
#line 218 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy24 = new_variable_declaration(ctx, yymsp[0].minor.yy380, NULL, NULL, NULL, NULL); }
#line 3159 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 82: /* struct_declaration ::= struct_intro LBRACE struct_member_list RBRACE */
#line 225 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astStructMembers, yymsp[-1].minor.yy346); yygotominor.yy249 = new_struct_declaration(ctx, yymsp[-3].minor.yy306, yymsp[-1].minor.yy346); }
#line 3164 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 83: /* struct_intro ::= STRUCT IDENTIFIER */
#line 229 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy306 = yymsp[0].minor.yy0.string; push_usertype(ctx, yygotominor.yy306, &ctx->dt_none); }
#line 3169 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 84: /* struct_member_list ::= struct_member */
      case 87: /* struct_member ::= struct_member_details */ yytestcase(yyruleno==87);
#line 233 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy346 = yymsp[0].minor.yy346; }
#line 3175 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 85: /* struct_member_list ::= struct_member_list struct_member */
#line 234 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy346 = yymsp[0].minor.yy346; MOJOSHADER_astStructMembers *i = yygotominor.yy346; while (i->next) { i = i->next; } i->next = yymsp[-1].minor.yy346; }
#line 3180 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 86: /* struct_member ::= interpolation_mod struct_member_details */
#line 238 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ MOJOSHADER_astStructMembers *i = yymsp[0].minor.yy346; yygotominor.yy346 = yymsp[0].minor.yy346; while (i) { i->interpolation_mod = yymsp[-1].minor.yy111; i = i->next; } }
#line 3185 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 88: /* struct_member_details ::= datatype struct_member_item_list SEMICOLON */
#line 243 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ MOJOSHADER_astStructMembers *i = yymsp[-1].minor.yy346; yygotominor.yy346 = yymsp[-1].minor.yy346; while (i) { i->datatype = yymsp[-2].minor.yy37; i = i->next; } }
#line 3190 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 89: /* struct_member_item_list ::= scalar_or_array */
#line 247 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy346 = new_struct_member(ctx, yymsp[0].minor.yy380, NULL); }
#line 3195 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 90: /* struct_member_item_list ::= scalar_or_array semantic */
#line 248 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy346 = new_struct_member(ctx, yymsp[-1].minor.yy380, yymsp[0].minor.yy306); }
#line 3200 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 91: /* struct_member_item_list ::= struct_member_item_list COMMA IDENTIFIER */
#line 249 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy346 = new_struct_member(ctx, new_scalar_or_array(ctx, yymsp[0].minor.yy0.string, 0, NULL), NULL); yygotominor.yy346->next = yymsp[-2].minor.yy346; yygotominor.yy346->semantic = yymsp[-2].minor.yy346->semantic; }
#line 3205 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 92: /* variable_lowlevel ::= packoffset register */
#line 253 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy82 = new_variable_lowlevel(ctx, yymsp[-1].minor.yy8, yymsp[0].minor.yy306); }
#line 3210 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 93: /* variable_lowlevel ::= register packoffset */
#line 254 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy82 = new_variable_lowlevel(ctx, yymsp[0].minor.yy8, yymsp[-1].minor.yy306); }
#line 3215 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 94: /* variable_lowlevel ::= packoffset */
#line 255 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy82 = new_variable_lowlevel(ctx, yymsp[0].minor.yy8, NULL); }
#line 3220 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 95: /* variable_lowlevel ::= register */
#line 256 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy82 = new_variable_lowlevel(ctx, NULL, yymsp[0].minor.yy306); }
#line 3225 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 96: /* scalar_or_array ::= IDENTIFIER LBRACKET RBRACKET */
#line 261 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy380 = new_scalar_or_array(ctx, yymsp[-2].minor.yy0.string, 1, NULL); }
#line 3230 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 97: /* scalar_or_array ::= IDENTIFIER LBRACKET expression RBRACKET */
#line 262 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy380 = new_scalar_or_array(ctx, yymsp[-3].minor.yy0.string, 1, yymsp[-1].minor.yy322); }
#line 3235 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 98: /* scalar_or_array ::= IDENTIFIER */
#line 263 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy380 = new_scalar_or_array(ctx, yymsp[0].minor.yy0.string, 0, NULL); }
#line 3240 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 99: /* packoffset ::= COLON PACKOFFSET LPAREN IDENTIFIER DOT IDENTIFIER RPAREN */
#line 267 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy8 = new_pack_offset(ctx, yymsp[-3].minor.yy0.string, yymsp[-1].minor.yy0.string); }
#line 3245 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 100: /* packoffset ::= COLON PACKOFFSET LPAREN IDENTIFIER RPAREN */
#line 268 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy8 = new_pack_offset(ctx, yymsp[-1].minor.yy0.string, NULL); }
#line 3250 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 101: /* register ::= COLON REGISTER LPAREN IDENTIFIER RPAREN */
#line 274 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy306 = yymsp[-1].minor.yy0.string; }
#line 3255 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 102: /* annotations ::= LT annotation_list GT */
#line 278 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astAnnotations, yymsp[-1].minor.yy268); yygotominor.yy268 = yymsp[-1].minor.yy268; }
#line 3260 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 103: /* annotation_list ::= annotation */
#line 282 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy268 = yymsp[0].minor.yy268; }
#line 3265 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 104: /* annotation_list ::= annotation_list annotation */
#line 283 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy268 = yymsp[0].minor.yy268; yygotominor.yy268->next = yymsp[-1].minor.yy268; }
#line 3270 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 105: /* annotation ::= datatype_scalar initializer SEMICOLON */
#line 288 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy268 = new_annotation(ctx, yymsp[-2].minor.yy37, yymsp[-1].minor.yy322); }
#line 3275 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 106: /* initializer_block_list ::= expression */
      case 110: /* initializer ::= ASSIGN initializer_block */ yytestcase(yyruleno==110);
      case 111: /* initializer ::= ASSIGN expression */ yytestcase(yyruleno==111);
      case 224: /* postfix_expr ::= primary_expr */ yytestcase(yyruleno==224);
      case 235: /* unary_expr ::= postfix_expr */ yytestcase(yyruleno==235);
      case 238: /* unary_expr ::= PLUS cast_expr */ yytestcase(yyruleno==238);
      case 242: /* cast_expr ::= unary_expr */ yytestcase(yyruleno==242);
      case 244: /* multiplicative_expr ::= cast_expr */ yytestcase(yyruleno==244);
      case 248: /* additive_expr ::= multiplicative_expr */ yytestcase(yyruleno==248);
      case 251: /* shift_expr ::= additive_expr */ yytestcase(yyruleno==251);
      case 254: /* relational_expr ::= shift_expr */ yytestcase(yyruleno==254);
      case 259: /* equality_expr ::= relational_expr */ yytestcase(yyruleno==259);
      case 262: /* and_expr ::= equality_expr */ yytestcase(yyruleno==262);
      case 264: /* exclusive_or_expr ::= and_expr */ yytestcase(yyruleno==264);
      case 266: /* inclusive_or_expr ::= exclusive_or_expr */ yytestcase(yyruleno==266);
      case 268: /* logical_and_expr ::= inclusive_or_expr */ yytestcase(yyruleno==268);
      case 270: /* logical_or_expr ::= logical_and_expr */ yytestcase(yyruleno==270);
      case 272: /* conditional_expr ::= logical_or_expr */ yytestcase(yyruleno==272);
      case 274: /* assignment_expr ::= conditional_expr */ yytestcase(yyruleno==274);
      case 286: /* expression ::= assignment_expr */ yytestcase(yyruleno==286);
#line 292 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = yymsp[0].minor.yy322; }
#line 3299 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 107: /* initializer_block_list ::= LBRACE initializer_block_list RBRACE */
      case 109: /* initializer_block ::= LBRACE initializer_block_list RBRACE */ yytestcase(yyruleno==109);
      case 223: /* primary_expr ::= LPAREN expression RPAREN */ yytestcase(yyruleno==223);
#line 293 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = yymsp[-1].minor.yy322; }
#line 3306 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 108: /* initializer_block_list ::= initializer_block_list COMMA initializer_block_list */
      case 287: /* expression ::= expression COMMA assignment_expr */ yytestcase(yyruleno==287);
#line 294 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_COMMA, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3312 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 112: /* intrinsic_datatype ::= datatype_vector */
      case 113: /* intrinsic_datatype ::= datatype_matrix */ yytestcase(yyruleno==113);
      case 114: /* intrinsic_datatype ::= datatype_scalar */ yytestcase(yyruleno==114);
      case 115: /* intrinsic_datatype ::= datatype_sampler */ yytestcase(yyruleno==115);
      case 116: /* intrinsic_datatype ::= datatype_buffer */ yytestcase(yyruleno==116);
      case 117: /* datatype ::= intrinsic_datatype */ yytestcase(yyruleno==117);
#line 306 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = yymsp[0].minor.yy37; }
#line 3322 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 118: /* datatype ::= USERTYPE */
#line 314 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = yymsp[0].minor.yy0.datatype; }
#line 3327 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 119: /* datatype_sampler ::= SAMPLER */
      case 121: /* datatype_sampler ::= SAMPLER2D */ yytestcase(yyruleno==121);
#line 317 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_sampler2d; }
#line 3333 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 120: /* datatype_sampler ::= SAMPLER1D */
#line 318 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_sampler1d; }
#line 3338 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 122: /* datatype_sampler ::= SAMPLER3D */
#line 320 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_sampler3d; }
#line 3343 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 123: /* datatype_sampler ::= SAMPLERCUBE */
#line 321 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_samplercube; }
#line 3348 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 124: /* datatype_sampler ::= SAMPLER_STATE */
      case 125: /* datatype_sampler ::= SAMPLERSTATE */ yytestcase(yyruleno==125);
#line 322 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_samplerstate; }
#line 3354 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 126: /* datatype_sampler ::= SAMPLERCOMPARISONSTATE */
#line 324 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_samplercompstate; }
#line 3359 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 127: /* datatype_scalar ::= BOOL */
#line 327 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_bool; }
#line 3364 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 128: /* datatype_scalar ::= INT */
#line 328 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_int; }
#line 3369 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 129: /* datatype_scalar ::= UINT */
#line 329 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_uint; }
#line 3374 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 130: /* datatype_scalar ::= HALF */
#line 330 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_half; }
#line 3379 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 131: /* datatype_scalar ::= FLOAT */
#line 331 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_float; }
#line 3384 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 132: /* datatype_scalar ::= DOUBLE */
#line 332 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_double; }
#line 3389 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 133: /* datatype_scalar ::= STRING */
#line 333 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_string; }
#line 3394 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 134: /* datatype_scalar ::= SNORM FLOAT */
#line 334 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_float_snorm; }
#line 3399 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 135: /* datatype_scalar ::= UNORM FLOAT */
#line 335 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_float_unorm; }
#line 3404 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 136: /* datatype_buffer ::= BUFFER LT BOOL GT */
#line 338 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_bool; }
#line 3409 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 137: /* datatype_buffer ::= BUFFER LT INT GT */
#line 339 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_int; }
#line 3414 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 138: /* datatype_buffer ::= BUFFER LT UINT GT */
#line 340 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_uint; }
#line 3419 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 139: /* datatype_buffer ::= BUFFER LT HALF GT */
#line 341 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_half; }
#line 3424 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 140: /* datatype_buffer ::= BUFFER LT FLOAT GT */
#line 342 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_float; }
#line 3429 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 141: /* datatype_buffer ::= BUFFER LT DOUBLE GT */
#line 343 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_double; }
#line 3434 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 142: /* datatype_buffer ::= BUFFER LT SNORM FLOAT GT */
#line 344 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_float_snorm; }
#line 3439 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 143: /* datatype_buffer ::= BUFFER LT UNORM FLOAT GT */
#line 345 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = &ctx->dt_buf_float_unorm; }
#line 3444 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 144: /* datatype_vector ::= VECTOR LT datatype_scalar COMMA INT_CONSTANT GT */
#line 348 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = new_datatype_vector(ctx, yymsp[-3].minor.yy37, (int) yymsp[-1].minor.yy0.i64); }
#line 3449 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 145: /* datatype_matrix ::= MATRIX LT datatype_scalar COMMA INT_CONSTANT COMMA INT_CONSTANT GT */
#line 351 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy37 = new_datatype_matrix(ctx, yymsp[-5].minor.yy37, (int) yymsp[-3].minor.yy0.i64, (int) yymsp[-1].minor.yy0.i64); }
#line 3454 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 146: /* statement_block ::= LBRACE RBRACE */
#line 355 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_block_statement(ctx, NULL); }
#line 3459 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 147: /* statement_block ::= LBRACE statement_list RBRACE */
#line 356 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astStatement, yymsp[-1].minor.yy233); yygotominor.yy233 = new_block_statement(ctx, yymsp[-1].minor.yy233); }
#line 3464 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 148: /* statement_list ::= statement */
      case 172: /* statement ::= statement_block */ yytestcase(yyruleno==172);
      case 173: /* statement ::= for_statement */ yytestcase(yyruleno==173);
#line 360 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = yymsp[0].minor.yy233; }
#line 3471 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 149: /* statement_list ::= statement_list statement */
#line 361 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = yymsp[0].minor.yy233; yygotominor.yy233->next = yymsp[-1].minor.yy233; }
#line 3476 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 150: /* statement_attribute ::= ISOLATE */
      case 151: /* statement_attribute ::= MAXINSTRUCTIONCOUNT LPAREN INT_CONSTANT RPAREN */ yytestcase(yyruleno==151);
      case 152: /* statement_attribute ::= NOEXPRESSIONOPTIMIZATIONS */ yytestcase(yyruleno==152);
      case 153: /* statement_attribute ::= REMOVEUNUSEDINPUTS */ yytestcase(yyruleno==153);
      case 154: /* statement_attribute ::= UNUSED */ yytestcase(yyruleno==154);
      case 155: /* statement_attribute ::= XPS */ yytestcase(yyruleno==155);
      case 176: /* while_intro ::= LBRACKET LOOP RBRACKET WHILE */ yytestcase(yyruleno==176);
      case 181: /* for_intro ::= LBRACKET LOOP RBRACKET FOR */ yytestcase(yyruleno==181);
      case 197: /* do_intro ::= LBRACKET LOOP RBRACKET DO */ yytestcase(yyruleno==197);
#line 367 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = 0; }
#line 3489 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 156: /* statement ::= BREAK SEMICOLON */
#line 376 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_break_statement(ctx); }
#line 3494 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 157: /* statement ::= CONTINUE SEMICOLON */
#line 377 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_continue_statement(ctx); }
#line 3499 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 158: /* statement ::= DISCARD SEMICOLON */
#line 378 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_discard_statement(ctx); }
#line 3504 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 159: /* statement ::= LBRACKET statement_attribute RBRACKET statement_block */
#line 379 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = yymsp[0].minor.yy233; /* !!! FIXME: yygotominor.yy233->attributes = yymsp[-2].minor.yy270;*/ yymsp[-2].minor.yy270 = 0; }
#line 3509 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 160: /* statement ::= variable_declaration */
#line 380 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_vardecl_statement(ctx, yymsp[0].minor.yy24); }
#line 3514 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 161: /* statement ::= struct_declaration SEMICOLON */
#line 381 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_struct_statement(ctx, yymsp[-1].minor.yy249); }
#line 3519 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 162: /* statement ::= do_intro DO statement WHILE LPAREN expression RPAREN SEMICOLON */
#line 382 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_do_statement(ctx, yymsp[-7].minor.yy270, yymsp[-5].minor.yy233, yymsp[-2].minor.yy322); }
#line 3524 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 163: /* statement ::= while_intro LPAREN expression RPAREN statement */
#line 383 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_while_statement(ctx, yymsp[-4].minor.yy270, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3529 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 164: /* statement ::= if_intro LPAREN expression RPAREN statement */
#line 384 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_if_statement(ctx, yymsp[-4].minor.yy270, yymsp[-2].minor.yy322, yymsp[0].minor.yy233, NULL); }
#line 3534 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 165: /* statement ::= if_intro LPAREN expression RPAREN statement ELSE statement */
#line 385 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_if_statement(ctx, yymsp[-6].minor.yy270, yymsp[-4].minor.yy322, yymsp[-2].minor.yy233, yymsp[0].minor.yy233); }
#line 3539 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 166: /* statement ::= switch_intro LPAREN expression RPAREN LBRACE switch_case_list RBRACE */
#line 386 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astSwitchCases, yymsp[-1].minor.yy165); yygotominor.yy233 = new_switch_statement(ctx, yymsp[-6].minor.yy270, yymsp[-4].minor.yy322, yymsp[-1].minor.yy165); }
#line 3544 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 167: /* statement ::= typedef */
#line 387 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_typedef_statement(ctx, yymsp[0].minor.yy71); }
#line 3549 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 168: /* statement ::= SEMICOLON */
#line 388 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_empty_statement(ctx); }
#line 3554 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 169: /* statement ::= expression SEMICOLON */
#line 389 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_expr_statement(ctx, yymsp[-1].minor.yy322); }
#line 3559 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 170: /* statement ::= RETURN SEMICOLON */
#line 390 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_return_statement(ctx, NULL); }
#line 3564 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 171: /* statement ::= RETURN expression SEMICOLON */
#line 391 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_return_statement(ctx, yymsp[-1].minor.yy322); }
#line 3569 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 174: /* while_intro ::= LBRACKET UNROLL LPAREN INT_CONSTANT RPAREN RBRACKET WHILE */
      case 179: /* for_intro ::= LBRACKET UNROLL LPAREN INT_CONSTANT RPAREN RBRACKET FOR */ yytestcase(yyruleno==179);
#line 397 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = (yymsp[-3].minor.yy0.i64 < 0) ? 0 : yymsp[-3].minor.yy0.i64; }
#line 3575 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 175: /* while_intro ::= LBRACKET UNROLL RBRACKET WHILE */
      case 180: /* for_intro ::= LBRACKET UNROLL RBRACKET FOR */ yytestcase(yyruleno==180);
      case 196: /* do_intro ::= LBRACKET UNROLL RBRACKET DO */ yytestcase(yyruleno==196);
#line 398 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = -1; }
#line 3582 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 177: /* while_intro ::= WHILE */
      case 182: /* for_intro ::= FOR */ yytestcase(yyruleno==182);
      case 198: /* do_intro ::= DO */ yytestcase(yyruleno==198);
#line 400 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = -2; }
#line 3589 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 178: /* for_statement ::= for_intro for_details */
#line 404 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = yymsp[0].minor.yy233; ((MOJOSHADER_astForStatement *) yygotominor.yy233)->unroll = yymsp[-1].minor.yy270; }
#line 3594 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 183: /* for_details ::= LPAREN expression SEMICOLON expression SEMICOLON expression RPAREN statement */
#line 414 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, yymsp[-6].minor.yy322, yymsp[-4].minor.yy322, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3599 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 184: /* for_details ::= LPAREN SEMICOLON SEMICOLON RPAREN statement */
#line 415 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, NULL, NULL, NULL, yymsp[0].minor.yy233); }
#line 3604 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 185: /* for_details ::= LPAREN SEMICOLON SEMICOLON expression RPAREN statement */
#line 416 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, NULL, NULL, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3609 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 186: /* for_details ::= LPAREN SEMICOLON expression SEMICOLON RPAREN statement */
#line 417 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, NULL, yymsp[-3].minor.yy322, NULL, yymsp[0].minor.yy233); }
#line 3614 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 187: /* for_details ::= LPAREN SEMICOLON expression SEMICOLON expression RPAREN statement */
#line 418 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, NULL, yymsp[-4].minor.yy322, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3619 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 188: /* for_details ::= LPAREN expression SEMICOLON SEMICOLON RPAREN statement */
#line 419 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, yymsp[-4].minor.yy322, NULL, NULL, yymsp[0].minor.yy233); }
#line 3624 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 189: /* for_details ::= LPAREN expression SEMICOLON SEMICOLON expression RPAREN statement */
#line 420 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, yymsp[-5].minor.yy322, NULL, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3629 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 190: /* for_details ::= LPAREN expression SEMICOLON expression SEMICOLON RPAREN statement */
#line 421 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, NULL, yymsp[-5].minor.yy322, yymsp[-3].minor.yy322, NULL, yymsp[0].minor.yy233); }
#line 3634 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 191: /* for_details ::= LPAREN variable_declaration expression SEMICOLON expression RPAREN statement */
#line 422 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, yymsp[-5].minor.yy24, NULL, yymsp[-4].minor.yy322, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3639 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 192: /* for_details ::= LPAREN variable_declaration SEMICOLON RPAREN statement */
#line 423 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, yymsp[-3].minor.yy24, NULL, NULL, NULL, yymsp[0].minor.yy233); }
#line 3644 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 193: /* for_details ::= LPAREN variable_declaration SEMICOLON expression RPAREN statement */
#line 424 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, yymsp[-4].minor.yy24, NULL, yymsp[-2].minor.yy322, NULL, yymsp[0].minor.yy233); }
#line 3649 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 194: /* for_details ::= LPAREN variable_declaration expression SEMICOLON RPAREN statement */
#line 425 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy233 = new_for_statement(ctx, yymsp[-4].minor.yy24, NULL, yymsp[-3].minor.yy322, NULL, yymsp[0].minor.yy233); }
#line 3654 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 195: /* do_intro ::= LBRACKET UNROLL LPAREN INT_CONSTANT RPAREN RBRACKET DO */
#line 428 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = (yymsp[-3].minor.yy0.i64 < 0) ? 0 : (int) yymsp[-3].minor.yy0.i64; }
#line 3659 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 199: /* if_intro ::= LBRACKET BRANCH RBRACKET IF */
#line 434 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_BRANCH; }
#line 3664 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 200: /* if_intro ::= LBRACKET FLATTEN RBRACKET IF */
#line 435 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_FLATTEN; }
#line 3669 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 201: /* if_intro ::= LBRACKET IFALL RBRACKET IF */
#line 436 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_IFALL; }
#line 3674 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 202: /* if_intro ::= LBRACKET IFANY RBRACKET IF */
#line 437 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_IFANY; }
#line 3679 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 203: /* if_intro ::= LBRACKET PREDICATE RBRACKET IF */
#line 438 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_PREDICATE; }
#line 3684 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 204: /* if_intro ::= LBRACKET PREDICATEBLOCK RBRACKET IF */
#line 439 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_PREDICATEBLOCK; }
#line 3689 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 205: /* if_intro ::= IF */
#line 440 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_IFATTR_NONE; }
#line 3694 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 206: /* switch_intro ::= LBRACKET FLATTEN RBRACKET SWITCH */
#line 443 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_SWITCHATTR_FLATTEN; }
#line 3699 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 207: /* switch_intro ::= LBRACKET BRANCH RBRACKET SWITCH */
#line 444 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_SWITCHATTR_BRANCH; }
#line 3704 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 208: /* switch_intro ::= LBRACKET FORCECASE RBRACKET SWITCH */
#line 445 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_SWITCHATTR_FORCECASE; }
#line 3709 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 209: /* switch_intro ::= LBRACKET CALL RBRACKET SWITCH */
#line 446 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_SWITCHATTR_CALL; }
#line 3714 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 210: /* switch_intro ::= SWITCH */
#line 447 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy270 = MOJOSHADER_AST_SWITCHATTR_NONE; }
#line 3719 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 211: /* switch_case_list ::= switch_case */
#line 451 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy165 = yymsp[0].minor.yy165; }
#line 3724 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 212: /* switch_case_list ::= switch_case_list switch_case */
#line 452 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy165 = yymsp[0].minor.yy165; yygotominor.yy165->next = yymsp[-1].minor.yy165; }
#line 3729 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 213: /* switch_case ::= CASE expression COLON statement_list */
#line 458 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astStatement, yymsp[0].minor.yy233); yygotominor.yy165 = new_switch_case(ctx, yymsp[-2].minor.yy322, yymsp[0].minor.yy233); }
#line 3734 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 214: /* switch_case ::= CASE expression COLON */
#line 459 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy165 = new_switch_case(ctx, yymsp[-1].minor.yy322, NULL); }
#line 3739 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 215: /* switch_case ::= DEFAULT COLON statement_list */
#line 460 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astStatement, yymsp[0].minor.yy233); yygotominor.yy165 = new_switch_case(ctx, NULL, yymsp[0].minor.yy233); }
#line 3744 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 216: /* switch_case ::= DEFAULT COLON */
#line 461 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy165 = new_switch_case(ctx, NULL, NULL); }
#line 3749 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 217: /* primary_expr ::= IDENTIFIER */
#line 466 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_identifier_expr(ctx, yymsp[0].minor.yy0.string); }
#line 3754 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 218: /* primary_expr ::= INT_CONSTANT */
#line 467 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_literal_int_expr(ctx, yymsp[0].minor.yy0.i64); }
#line 3759 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 219: /* primary_expr ::= FLOAT_CONSTANT */
#line 468 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_literal_float_expr(ctx, yymsp[0].minor.yy0.dbl); }
#line 3764 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 220: /* primary_expr ::= STRING_LITERAL */
#line 469 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_literal_string_expr(ctx, yymsp[0].minor.yy0.string); }
#line 3769 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 221: /* primary_expr ::= TRUE */
#line 470 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_literal_boolean_expr(ctx, 1); }
#line 3774 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 222: /* primary_expr ::= FALSE */
#line 471 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_literal_boolean_expr(ctx, 0); }
#line 3779 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 225: /* postfix_expr ::= postfix_expr LBRACKET expression RBRACKET */
#line 477 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_DEREF_ARRAY, yymsp[-3].minor.yy322, yymsp[-1].minor.yy322); }
#line 3784 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 226: /* postfix_expr ::= IDENTIFIER arguments */
#line 478 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_callfunc_expr(ctx, yymsp[-1].minor.yy0.string, yymsp[0].minor.yy26); }
#line 3789 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 227: /* postfix_expr ::= datatype arguments */
#line 479 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_constructor_expr(ctx, yymsp[-1].minor.yy37, yymsp[0].minor.yy26); }
#line 3794 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 228: /* postfix_expr ::= postfix_expr DOT IDENTIFIER */
#line 480 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_deref_struct_expr(ctx, yymsp[-2].minor.yy322, yymsp[0].minor.yy0.string); }
#line 3799 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 229: /* postfix_expr ::= postfix_expr PLUSPLUS */
#line 481 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_POSTINCREMENT, yymsp[-1].minor.yy322); }
#line 3804 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 230: /* postfix_expr ::= postfix_expr MINUSMINUS */
#line 482 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_POSTDECREMENT, yymsp[-1].minor.yy322); }
#line 3809 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 231: /* arguments ::= LPAREN RPAREN */
#line 486 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy26 = NULL; }
#line 3814 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 232: /* arguments ::= LPAREN argument_list RPAREN */
#line 487 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ REVERSE_LINKED_LIST(MOJOSHADER_astArguments, yymsp[-1].minor.yy26); yygotominor.yy26 = yymsp[-1].minor.yy26; }
#line 3819 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 233: /* argument_list ::= assignment_expr */
#line 491 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy26 = new_argument(ctx, yymsp[0].minor.yy322); }
#line 3824 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 234: /* argument_list ::= argument_list COMMA assignment_expr */
#line 492 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy26 = new_argument(ctx, yymsp[0].minor.yy322); yygotominor.yy26->next = yymsp[-2].minor.yy26; }
#line 3829 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 236: /* unary_expr ::= PLUSPLUS unary_expr */
#line 497 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_PREINCREMENT, yymsp[0].minor.yy322); }
#line 3834 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 237: /* unary_expr ::= MINUSMINUS unary_expr */
#line 498 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_PREDECREMENT, yymsp[0].minor.yy322); }
#line 3839 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 239: /* unary_expr ::= MINUS cast_expr */
#line 500 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_NEGATE, yymsp[0].minor.yy322); }
#line 3844 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 240: /* unary_expr ::= COMPLEMENT cast_expr */
#line 501 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_COMPLEMENT, yymsp[0].minor.yy322); }
#line 3849 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 241: /* unary_expr ::= EXCLAMATION cast_expr */
#line 502 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_unary_expr(ctx, MOJOSHADER_AST_OP_NOT, yymsp[0].minor.yy322); }
#line 3854 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 243: /* cast_expr ::= LPAREN datatype RPAREN cast_expr */
#line 507 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_cast_expr(ctx, yymsp[-2].minor.yy37, yymsp[0].minor.yy322); }
#line 3859 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 245: /* multiplicative_expr ::= multiplicative_expr STAR cast_expr */
#line 512 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_MULTIPLY, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3864 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 246: /* multiplicative_expr ::= multiplicative_expr SLASH cast_expr */
#line 513 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_DIVIDE, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3869 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 247: /* multiplicative_expr ::= multiplicative_expr PERCENT cast_expr */
#line 514 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_MODULO, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3874 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 249: /* additive_expr ::= additive_expr PLUS multiplicative_expr */
#line 519 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_ADD, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3879 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 250: /* additive_expr ::= additive_expr MINUS multiplicative_expr */
#line 520 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_SUBTRACT, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3884 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 252: /* shift_expr ::= shift_expr LSHIFT additive_expr */
#line 525 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_LSHIFT, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3889 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 253: /* shift_expr ::= shift_expr RSHIFT additive_expr */
#line 526 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_RSHIFT, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3894 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 255: /* relational_expr ::= relational_expr LT shift_expr */
#line 531 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_LESSTHAN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3899 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 256: /* relational_expr ::= relational_expr GT shift_expr */
#line 532 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_GREATERTHAN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3904 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 257: /* relational_expr ::= relational_expr LEQ shift_expr */
#line 533 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_LESSTHANOREQUAL, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3909 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 258: /* relational_expr ::= relational_expr GEQ shift_expr */
#line 534 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_GREATERTHANOREQUAL, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3914 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 260: /* equality_expr ::= equality_expr EQL relational_expr */
#line 539 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_EQUAL, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3919 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 261: /* equality_expr ::= equality_expr NEQ relational_expr */
#line 540 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_NOTEQUAL, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3924 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 263: /* and_expr ::= and_expr AND equality_expr */
#line 545 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_BINARYAND, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3929 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 265: /* exclusive_or_expr ::= exclusive_or_expr XOR and_expr */
#line 550 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_BINARYXOR, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3934 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 267: /* inclusive_or_expr ::= inclusive_or_expr OR exclusive_or_expr */
#line 555 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_BINARYOR, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3939 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 269: /* logical_and_expr ::= logical_and_expr ANDAND inclusive_or_expr */
#line 560 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_LOGICALAND, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3944 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 271: /* logical_or_expr ::= logical_or_expr OROR logical_and_expr */
#line 565 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_LOGICALOR, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3949 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 273: /* conditional_expr ::= logical_or_expr QUESTION logical_or_expr COLON conditional_expr */
#line 570 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_ternary_expr(ctx, MOJOSHADER_AST_OP_CONDITIONAL, yymsp[-4].minor.yy322, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3954 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 275: /* assignment_expr ::= unary_expr ASSIGN assignment_expr */
#line 575 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_ASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3959 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 276: /* assignment_expr ::= unary_expr MULASSIGN assignment_expr */
#line 576 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_MULASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3964 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 277: /* assignment_expr ::= unary_expr DIVASSIGN assignment_expr */
#line 577 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_DIVASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3969 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 278: /* assignment_expr ::= unary_expr MODASSIGN assignment_expr */
#line 578 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_MODASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3974 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 279: /* assignment_expr ::= unary_expr ADDASSIGN assignment_expr */
#line 579 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_ADDASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3979 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 280: /* assignment_expr ::= unary_expr SUBASSIGN assignment_expr */
#line 580 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_SUBASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3984 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 281: /* assignment_expr ::= unary_expr LSHIFTASSIGN assignment_expr */
#line 581 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_LSHIFTASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3989 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 282: /* assignment_expr ::= unary_expr RSHIFTASSIGN assignment_expr */
#line 582 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_RSHIFTASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3994 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 283: /* assignment_expr ::= unary_expr ANDASSIGN assignment_expr */
#line 583 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_ANDASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 3999 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 284: /* assignment_expr ::= unary_expr XORASSIGN assignment_expr */
#line 584 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_XORASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 4004 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      case 285: /* assignment_expr ::= unary_expr ORASSIGN assignment_expr */
#line 585 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"
{ yygotominor.yy322 = new_binary_expr(ctx, MOJOSHADER_AST_OP_ORASSIGN, yymsp[-2].minor.yy322, yymsp[0].minor.yy322); }
#line 4009 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
        break;
      default:
        break;
  };
  yygoto = yyRuleInfo[yyruleno].lhs;
  yysize = yyRuleInfo[yyruleno].nrhs;
  yypParser->yyidx -= yysize;
  yyact = yy_find_reduce_action(yymsp[-yysize].stateno,(YYCODETYPE)yygoto);
  if( yyact < YYNSTATE ){
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
    /* If we are not debugging and the reduce action popped at least
    ** one element off the stack, then we can push the new element back
    ** onto the stack here, and skip the stack overflow test in yy_shift().
    ** That gives a significant speed improvement. */
    if( yysize ){
      yypParser->yyidx++;
      yymsp -= yysize-1;
      yymsp->stateno = (YYACTIONTYPE)yyact;
      yymsp->major = (YYCODETYPE)yygoto;
      yymsp->minor = yygotominor;
    }else
#endif
    {
      yy_shift(yypParser,yyact,yygoto,&yygotominor);
    }
  }else{
    assert( yyact == YYNSTATE + YYNRULE + 1 );
    yy_accept(yypParser);
  }
}

/*
** The following code executes when the parse fails
*/
#ifndef YYNOERRORRECOVERY
static void yy_parse_failed(
  yyParser *yypParser           /* The parser */
){
  ParseHLSLARG_FETCH;
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
  if( yyTraceFILE ){
    fprintf(yyTraceFILE,"%sFail!\n",yyTracePrompt);
  }
#endif
  while( yypParser->yyidx>=0 ) yy_pop_parser_stack(yypParser);
  /* Here code is inserted which will be executed whenever the
  ** parser fails */
#line 42 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"

    // !!! FIXME: make this a proper fail() function.
    fail(ctx, "Giving up. Parser is hopelessly lost...");
#line 4061 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
  ParseHLSLARG_STORE; /* Suppress warning about unused %extra_argument variable */
}
#endif /* YYNOERRORRECOVERY */

/*
** The following code executes when a syntax error first occurs.
*/
static void yy_syntax_error(
  yyParser *yypParser,           /* The parser */
  int yymajor,                   /* The major type of the error token */
  YYMINORTYPE yyminor            /* The minor type of the error token */
){
  ParseHLSLARG_FETCH;
#define TOKEN (yyminor.yy0)
#line 37 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.lemon"

    // !!! FIXME: make this a proper fail() function.
    fail(ctx, "Syntax error");
#line 4080 "/Volumes/Data/Compiling/terraria/mojoshader/mojoshader_parser_hlsl.h"
  ParseHLSLARG_STORE; /* Suppress warning about unused %extra_argument variable */
}

/*
** The following is executed when the parser accepts
*/
static void yy_accept(
  yyParser *yypParser           /* The parser */
){
  ParseHLSLARG_FETCH;
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
  if( yyTraceFILE ){
    fprintf(yyTraceFILE,"%sAccept!\n",yyTracePrompt);
  }
#endif
  while( yypParser->yyidx>=0 ) yy_pop_parser_stack(yypParser);
  /* Here code is inserted which will be executed whenever the
  ** parser accepts */
  ParseHLSLARG_STORE; /* Suppress warning about unused %extra_argument variable */
}

/* The main parser program.
** The first argument is a pointer to a structure obtained from
** "ParseHLSLAlloc" which describes the current state of the parser.
** The second argument is the major token number.  The third is
** the minor token.  The fourth optional argument is whatever the
** user wants (and specified in the grammar) and is available for
** use by the action routines.
**
** Inputs:
** <ul>
** <li> A pointer to the parser (an opaque structure.)
** <li> The major token number.
** <li> The minor token number.
** <li> An option argument of a grammar-specified type.
** </ul>
**
** Outputs:
** None.
*/
#if __MOJOSHADER__
static
#endif
void ParseHLSL(
  void *yyp,                   /* The parser */
  int yymajor,                 /* The major token code number */
  ParseHLSLTOKENTYPE yyminor       /* The value for the token */
  ParseHLSLARG_PDECL               /* Optional %extra_argument parameter */
){
  YYMINORTYPE yyminorunion;
  int yyact;            /* The parser action. */
  int yyendofinput;     /* True if we are at the end of input */
#ifdef YYERRORSYMBOL
  int yyerrorhit = 0;   /* True if yymajor has invoked an error */
#endif
  yyParser *yypParser;  /* The parser */

  /* (re)initialize the parser, if necessary */
  yypParser = (yyParser*)yyp;
  if( yypParser->yyidx<0 ){
#if YYSTACKDEPTH<=0
    if( yypParser->yystksz <=0 ){
      /*memset(&yyminorunion, 0, sizeof(yyminorunion));*/
      yyminorunion = yyzerominor;
      yyStackOverflow(yypParser, &yyminorunion);
      return;
    }
#endif
    yypParser->yyidx = 0;
    yypParser->yyerrcnt = -1;
    yypParser->yystack[0].stateno = 0;
    yypParser->yystack[0].major = 0;
  }
  yyminorunion.yy0 = yyminor;
  yyendofinput = (yymajor==0);
  ParseHLSLARG_STORE;

#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
  if( yyTraceFILE ){
    fprintf(yyTraceFILE,"%sInput %s\n",yyTracePrompt,yyTokenName[yymajor]);
  }
#endif

  do{
    yyact = yy_find_shift_action(yypParser,(YYCODETYPE)yymajor);
    if( yyact<YYNSTATE ){
      assert( !yyendofinput );  /* Impossible to shift the $ token */
      yy_shift(yypParser,yyact,yymajor,&yyminorunion);
      yypParser->yyerrcnt--;
      yymajor = YYNOCODE;
    }else if( yyact < YYNSTATE + YYNRULE ){
      yy_reduce(yypParser,yyact-YYNSTATE);
    }else{
      assert( yyact == YY_ERROR_ACTION );
#ifdef YYERRORSYMBOL
      int yymx;
#endif
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
      if( yyTraceFILE ){
        fprintf(yyTraceFILE,"%sSyntax Error!\n",yyTracePrompt);
      }
#endif
#ifdef YYERRORSYMBOL
      /* A syntax error has occurred.
      ** The response to an error depends upon whether or not the
      ** grammar defines an error token "ERROR".  
      **
      ** This is what we do if the grammar does define ERROR:
      **
      **  * Call the %syntax_error function.
      **
      **  * Begin popping the stack until we enter a state where
      **    it is legal to shift the error symbol, then shift
      **    the error symbol.
      **
      **  * Set the error count to three.
      **
      **  * Begin accepting and shifting new tokens.  No new error
      **    processing will occur until three tokens have been
      **    shifted successfully.
      **
      */
      if( yypParser->yyerrcnt<0 ){
        yy_syntax_error(yypParser,yymajor,yyminorunion);
      }
      yymx = yypParser->yystack[yypParser->yyidx].major;
      if( yymx==YYERRORSYMBOL || yyerrorhit ){
#if LEMON_SUPPORT_TRACING   /* __MOJOSHADER__ */
        if( yyTraceFILE ){
          fprintf(yyTraceFILE,"%sDiscard input token %s\n",
             yyTracePrompt,yyTokenName[yymajor]);
        }
#endif
        yy_destructor(yypParser, (YYCODETYPE)yymajor,&yyminorunion);
        yymajor = YYNOCODE;
      }else{
         while(
          yypParser->yyidx >= 0 &&
          yymx != YYERRORSYMBOL &&
          (yyact = yy_find_reduce_action(
                        yypParser->yystack[yypParser->yyidx].stateno,
                        YYERRORSYMBOL)) >= YYNSTATE
        ){
          yy_pop_parser_stack(yypParser);
        }
        if( yypParser->yyidx < 0 || yymajor==0 ){
          yy_destructor(yypParser,(YYCODETYPE)yymajor,&yyminorunion);
          yy_parse_failed(yypParser);
          yymajor = YYNOCODE;
        }else if( yymx!=YYERRORSYMBOL ){
          YYMINORTYPE u2;
          u2.YYERRSYMDT = 0;
          yy_shift(yypParser,yyact,YYERRORSYMBOL,&u2);
        }
      }
      yypParser->yyerrcnt = 3;
      yyerrorhit = 1;
#elif defined(YYNOERRORRECOVERY)
      /* If the YYNOERRORRECOVERY macro is defined, then do not attempt to
      ** do any kind of error recovery.  Instead, simply invoke the syntax
      ** error routine and continue going as if nothing had happened.
      **
      ** Applications can set this macro (for example inside %include) if
      ** they intend to abandon the parse upon the first syntax error seen.
      */
      yy_syntax_error(yypParser,yymajor,yyminorunion);
      yy_destructor(yypParser,(YYCODETYPE)yymajor,&yyminorunion);
      yymajor = YYNOCODE;
      
#else  /* YYERRORSYMBOL is not defined */
      /* This is what we do if the grammar does not define ERROR:
      **
      **  * Report an error message, and throw away the input token.
      **
      **  * If the input token is $, then fail the parse.
      **
      ** As before, subsequent error messages are suppressed until
      ** three input tokens have been successfully shifted.
      */
      if( yypParser->yyerrcnt<=0 ){
        yy_syntax_error(yypParser,yymajor,yyminorunion);
      }
      yypParser->yyerrcnt = 3;
      yy_destructor(yypParser,(YYCODETYPE)yymajor,&yyminorunion);
      if( yyendofinput ){
        yy_parse_failed(yypParser);
      }
      yymajor = YYNOCODE;
#endif
    }
  }while( yymajor!=YYNOCODE && yypParser->yyidx>=0 );
  return;
}
