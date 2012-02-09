/**
 * MojoShader; generate shader programs from bytecode of compiled
 *  Direct3D shaders.
 *
 * Please see the file LICENSE.txt in the source's root directory.
 *
 *  This file written by Ryan C. Gordon.
 */

// This was originally based on examples/pp-c.re from re2c: http://re2c.org/
//   re2c is public domain code.
//
// You build mojoshader_lexer_preprocessor.c from the .re file with re2c...
// re2c -is -o mojoshader_lexer_preprocessor.c mojoshader_lexer_preprocessor.re
//
// Changes to the lexer are done to the .re file, not the C code!
//
// Please note that this isn't a perfect C lexer, since it is used for both
//  HLSL and shader assembly language, and follows the quirks of Microsoft's
//  tools.

#define __MOJOSHADER_INTERNAL__ 1
#include "mojoshader_internal.h"

typedef unsigned char uchar;

/*!max:re2c */
#define RET(t) return update_state(s, eoi, cursor, token, (Token) t)
#define YYCTYPE uchar
#define YYCURSOR cursor
#define YYLIMIT limit
#define YYMARKER s->lexer_marker
#define YYFILL(n) { if ((n) == 1) { cursor = sentinel; limit = cursor + YYMAXFILL; eoi = 1; } }

static uchar sentinel[YYMAXFILL];

static Token update_state(IncludeState *s, int eoi, const uchar *cur,
                          const uchar *tok, const Token val)
{
    if (eoi)
    {
        s->bytes_left = 0;
        s->source = (const char *) s->source_base + s->orig_length;
        if ( (tok >= sentinel) && (tok < (sentinel+YYMAXFILL)) )
            s->token = s->source;
        else
            s->token = (const char *) tok;
    } // if
    else
    {
        s->bytes_left -= (unsigned int) (cur - ((const uchar *) s->source));
        s->source = (const char *) cur;
        s->token = (const char *) tok;
    } // else
    s->tokenlen = (unsigned int) (s->source - s->token);
    s->tokenval = val;
    return val;
} // update_state

Token preprocessor_lexer(IncludeState *s)
{
    const uchar *cursor = (const uchar *) s->source;
    const uchar *token = cursor;
    const uchar *matchptr;
    const uchar *limit = cursor + s->bytes_left;
    int eoi = 0;
    int saw_newline = 0;

/*!re2c
    ANY = [\000-\377];
    ANYLEGAL = [a-zA-Z0-9_/'*=+%^&|!#<>()[{}.,~^:;? \t\v\f\r\n\-\]\\];
    O = [0-7];
    D = [0-9];
    L = [a-zA-Z_];
    H = [a-fA-F0-9];
    E = [Ee] [+-]? D+;
    FS = [fFhH];
    IS = [uUlL]*;
    ESC = [\\] ([abfnrtv?'"\\] | "x" H+ | O+);
    PP = "#" [ \t]*;
    NEWLINE = ("\r\n" | "\r" | "\n");
    WHITESPACE = [ \t\v\f]+;
*/

    // preprocessor directives are only valid at start of line.
    if (s->tokenval == ((Token) '\n'))
        goto ppdirective;  // may jump back to scanner_loop.

scanner_loop:
    if (YYLIMIT == YYCURSOR) YYFILL(1);
    token = cursor;

/*!re2c
    "\\" [ \t\v\f]* NEWLINE  { s->line++; goto scanner_loop; }

    "/*"            { goto multilinecomment; }
    "//"            { goto singlelinecomment; }

    L (L|D)*        { RET(TOKEN_IDENTIFIER); }
    
    ("0" [xX] H+ IS?) | ("0" D+ IS?) | (D+ IS?) |
    (['] (ESC|ANY\[\r\n\\'])* ['])
                    { RET(TOKEN_INT_LITERAL); }
    
    (D+ E FS?) | (D* "." D+ E? FS?) | (D+ "." D* E? FS?)
                    { RET(TOKEN_FLOAT_LITERAL); }
    
    (["] (ESC|ANY\[\r\n\\"])* ["])
                    { RET(TOKEN_STRING_LITERAL); }

    ">>="           { RET(TOKEN_RSHIFTASSIGN); }
    "<<="           { RET(TOKEN_LSHIFTASSIGN); }
    "+="            { RET(TOKEN_ADDASSIGN); }
    "-="            { RET(TOKEN_SUBASSIGN); }
    "*="            { RET(TOKEN_MULTASSIGN); }
    "/="            { RET(TOKEN_DIVASSIGN); }
    "%="            { RET(TOKEN_MODASSIGN); }
    "^="            { RET(TOKEN_XORASSIGN); }
    "&="            { RET(TOKEN_ANDASSIGN); }
    "|="            { RET(TOKEN_ORASSIGN); }
    "++"            { RET(TOKEN_INCREMENT); }
    "--"            { RET(TOKEN_DECREMENT); }
    ">>"            { RET(TOKEN_RSHIFT); }
    "<<"            { RET(TOKEN_LSHIFT); }
    "&&"            { RET(TOKEN_ANDAND); }
    "||"            { RET(TOKEN_OROR); }
    "<="            { RET(TOKEN_LEQ); }
    ">="            { RET(TOKEN_GEQ); }
    "=="            { RET(TOKEN_EQL); }
    "!="            { RET(TOKEN_NEQ); }
    "#"             { RET(TOKEN_HASH); }
    "##"            { RET(TOKEN_HASHHASH); }
    "("             { RET('('); }
    ")"             { RET(')'); }
    "["             { RET('['); }
    "]"             { RET(']'); }
    "."             { RET('.'); }
    ","             { RET(','); }
    "&"             { RET('&'); }
    "!"             { RET('!'); }
    "~"             { RET('~'); }
    "-"             { RET('-'); }
    "+"             { RET('+'); }
    "*"             { RET('*'); }
    "/"             { RET('/'); }
    "%"             { RET('%'); }
    "<"             { RET('<'); }
    ">"             { RET('>'); }
    "^"             { RET('^'); }
    "|"             { RET('|'); }
    ":"             { RET(':'); }
    "{"             { RET('{'); }
    "}"             { RET('}'); }
    "="             { RET('='); }
    "?"             { RET('?'); }

    ";"             { if (s->asm_comments) goto singlelinecomment; RET(';'); }

    "\000"          { if (eoi) { RET(TOKEN_EOI); } goto bad_chars; }

    WHITESPACE      { if (s->report_whitespace) RET(' '); goto scanner_loop; }
    NEWLINE         { s->line++; RET('\n'); }
    ANY             { goto bad_chars; }
*/

multilinecomment:
    if (YYLIMIT == YYCURSOR) YYFILL(1);
    matchptr = cursor;
// The "*\/" is just to avoid screwing up text editor syntax highlighting.
/*!re2c
    "*\/"           {
                        if (saw_newline)
                            RET('\n');
                        else if (s->report_whitespace)
                            RET(' ');
                        goto scanner_loop;
                    }
    NEWLINE         {
                        s->line++;
                        token = matchptr;
                        saw_newline = 1;
                        goto multilinecomment;
                    }
    "\000"          {
                        if (eoi)
                            RET(TOKEN_INCOMPLETE_COMMENT);
                        goto multilinecomment;
                    }
    ANY             { goto multilinecomment; }
*/

singlelinecomment:
    if (YYLIMIT == YYCURSOR) YYFILL(1);
    matchptr = cursor;
/*!re2c
    NEWLINE         { s->line++; token = matchptr; RET('\n'); }
    "\000"          { if (eoi) { RET(TOKEN_EOI); } goto singlelinecomment; }
    ANY             { goto singlelinecomment; }
*/

ppdirective:
    if (YYLIMIT == YYCURSOR) YYFILL(1);
/*!re2c
        PP "include"    { RET(TOKEN_PP_INCLUDE); }
        PP "line"       { RET(TOKEN_PP_LINE); }
        PP "define"     { RET(TOKEN_PP_DEFINE); }
        PP "undef"      { RET(TOKEN_PP_UNDEF); }
        PP "if"         { RET(TOKEN_PP_IF); }
        PP "ifdef"      { RET(TOKEN_PP_IFDEF); }
        PP "ifndef"     { RET(TOKEN_PP_IFNDEF); }
        PP "else"       { RET(TOKEN_PP_ELSE); }
        PP "elif"       { RET(TOKEN_PP_ELIF); }
        PP "endif"      { RET(TOKEN_PP_ENDIF); }
        PP "error"      { RET(TOKEN_PP_ERROR); }
        PP "pragma"     { RET(TOKEN_PP_PRAGMA); }
        WHITESPACE      { goto ppdirective; }

        ANY             {
                            token = cursor = (const uchar *) s->source;
                            limit = cursor + s->bytes_left;
                            goto scanner_loop;
                        }
*/

bad_chars:
    if (YYLIMIT == YYCURSOR) YYFILL(1);
/*!re2c
    ANYLEGAL        { cursor--; RET(TOKEN_BAD_CHARS); }
    "\000"          {
                        if (eoi)
                        {
                            assert( !((token >= sentinel) &&
                                     (token < sentinel+YYMAXFILL)) );
                            eoi = 0;
                            cursor = (uchar *) s->source_base + s->orig_length;
                            RET(TOKEN_BAD_CHARS);  // next call will be EOI.
                        }
                        goto bad_chars;
                    }

    ANY             { goto bad_chars; }
*/

    assert(0 && "Shouldn't hit this code");
    RET(TOKEN_UNKNOWN);
} // preprocessor_lexer

// end of mojoshader_lexer_preprocessor.re (or .c) ...

