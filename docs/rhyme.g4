/*
    Rhyme ANTLR4 grammar
*/
grammar rhyme;


// Syntactical Rules
compilationUnit
    : moduleDecl importStmt* topLevelDeclaration+
    ;

moduleDecl
    : MODULE IDENTIFIER SEMI
    ;

importStmt
    : IMPORT IDENTIFIER SEMI
    ;

topLevelDeclaration
    : bindingDeclaration
    | functionDeclaration
    | externDeclaration
    ;


bindingDeclaration
    : type declarator (',' declarator)* ';'
    ;

functionDeclaration
    : type IDENTIFIER OPEN_PAREN parameters? CLOSE_PAREN
    ;

externDeclaration
    : EXTERN functionDeclaration
    ; 
    
type
    : IDENTIFIER
    | array_type
    | FN func_type 
    ;

array_type
    : type OPEN_BRACKET CLOSE_BRACKET
    ;

func_type
    : type OPEN_PAREN (paramDecl (',' paramDecl)*)? CLOSE_PAREN
    ;

parameters
    : paramDecl (',' paramDecl)*
    ;

paramDecl
    : type IDENTIFIER
    ;

declarator
    : IDENTIFIER ('=' expression)?
    ;

expression
    : IDENTIFIER
    | arrayExpression
    ;

arrayExpression
    :
    |   OPEN_BRACKET (expression (COMMA expression )*)? CLOSE_BRACKET
    ;

expressionList
    :
    |   expression
    ;

// Lexical Rules
IDENTIFIER: [_a-zA-Z][_a-zA-Z0-9]*;

EXPORT: 'export';
FN:     'fn';
GLOBAL: 'global';
IMPORT: 'import';
MODULE: 'module';
EXTERN: 'extern';

OPEN_PAREN: '(';
CLOSE_PAREN: ')';
OPEN_BRACKET: '[';
CLOSE_BRACKET: ']';
COMMA: ',';
SEMI: ';';
