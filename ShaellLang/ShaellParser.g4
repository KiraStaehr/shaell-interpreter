parser grammar ShaellParser;

options {
    tokenVocab = 'ShaellLexer';
}

prog: stmts | programArgs stmts;
stmts: stmt*;
stmt: ifStmt | forLoop | whileLoop | returnStatement | functionDefinition | foreach | foreachKeyValue | expr;
boolean: 
    TRUE # TrueBoolean 
    | FALSE # FalseBoolean
    ;
expr: DQUOTE strcontent* END_STRING # StringLiteralExpr
    | LET IDENTIFIER # LetExpr
    | NUMBER # NumberExpr
    | NULL # NullExpr
	| boolean # BooleanExpr
	| IDENTIFIER # IdentifierExpr
	| LPAREN expr RPAREN # Parenthesis
	| LCURL (objfields ASSIGN expr (COMMA objfields ASSIGN expr)*)? RCURL #ObjectLiteral
	|<assoc=right> DEREF expr # DerefExpr
	|<assoc=right> LNOT expr # LnotExpr
	|<assoc=right> BNOT expr # BnotExpr
	|<assoc=right> MINUS expr # NegExpr
	|<assoc=right> PLUS expr # PosExpr
	| expr COLON IDENTIFIER # IdentifierIndexExpr
	| expr LSQUACKET expr RSQUACKET # SubScriptExpr
	| expr LPAREN innerArgList RPAREN # FunctionCallExpr
	| expr POW expr # PowExpr
	| expr MOD expr # ModExpr
	| expr DIV expr # DivExpr
	| expr MULT expr # MultExpr
    | expr PLUS expr # AddExpr
    | expr MINUS expr # MinusExpr
    | expr LT expr # LTExpr
    | expr LEQ expr # LEQExpr
    | expr GT expr # GTExpr
    | expr GEQ expr # GEQExpr
    | expr EQ expr # EQExpr
    | expr NEQ expr # NEQExpr
    | expr LAND expr # LANDExpr
    | expr LOR expr # LORExpr
    | expr PIPE expr # PIPEExpr
	|<assoc=right> expr ASSIGN expr # AssignExpr
	|<assoc=right> expr PLUSEQ expr  # PlusEqExpr
	|<assoc=right> expr MINUSEQ expr # MinusEqExpr
	|<assoc=right> expr MULTEQ expr # MultEqExpr
	|<assoc=right> expr DIVEQ expr # DivEqExpr
    |<assoc=right> expr MODEQ expr # ModEqExpr
    |<assoc=right> expr POWEQ expr # PowEqExpr
    |anonFunctionDefinition # AnonFnDefinition
	;
strcontent:
    NEWLINE # NewLine
    | INTERPOLATION expr STRINGCLOSEBRACE # Interpolation
    | TEXT # StringLiteral
    ;
objfields:
    IDENTIFIER # FieldIdentifier
    | LSQUACKET expr RSQUACKET #FieldExpr
    ;
innerArgList: (expr (COMMA expr)*)?;
innerFormalArgList: (IDENTIFIER (COMMA IDENTIFIER)*)?;
programArgs: ARGS LPAREN innerFormalArgList RPAREN;
ifStmt: IF expr THEN stmts (ELSE stmts)? END;
forLoop: FOR expr COMMA expr COMMA expr DO stmts END;
foreach: FOREACH IDENTIFIER IN expr DO stmts END;
foreachKeyValue: FOREACH IDENTIFIER COMMA IDENTIFIER IN expr DO stmts END;
whileLoop: WHILE expr DO stmts END;
functionDefinition: FUNCTION IDENTIFIER LPAREN innerFormalArgList RPAREN stmts END;
anonFunctionDefinition: FUNCTION LPAREN innerFormalArgList RPAREN stmts END;
returnStatement: RETURN expr;