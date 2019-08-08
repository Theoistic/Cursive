grammar Cursive;
prog:
	useNamespace* declareClass*;
stmt:
	expr+ '!';
expr:
	  'the' ID=Ident ('as'|'is') Expr=expr										# Declare
	| Left=expr ('as'|'is') Right=expr										         	# Assign
	| Pre=('halla' | 'clusterfuck') (Expr=expr '.')? ID=Ident ('with' expr+)?	# Call
	| Target=expr '.' ID=Ident													# GetField
	| ('bitching' | 'bitchin') Expr=expr										# Print
	| Value=Ident																# Ident
	| Value=Number																# Number
	| '"' (~'"')* '"'															# String
	| Left=expr (Operator expr)+										# Operation
	| 'jizz' Expr=expr?												    		# Return
	| Left=expr 'thundercunt' Right=expr										# HandleEvent
	| 'quim' Expr=expr															# Await
	;
conditionalElse:
	Pre=('else'|'else if') expr? 'then' scopeBlock 'then jizzslurp';
conditional:
	'if' expr 'then' scopeBlock 'then jizzslurp' conditionalElse*;
scopeBlock:
	(stmt|block|conditional)*;
block:
	'exploited cunt known as' Expr=expr 'do' scopeBlock 'until jizzed upon'				# Using
	| 'fuckstrangle' ID=Ident 'then' scopeBlock 'until jizzed upon'						# Lock
	;
declareFunction:
	ID=Ident 'is a' Modifier* 'cum stain' attribute* ('with' (Ident Ident)*)? scopeBlock;
classProperty:
    'fucking' Modifier* Name=Ident attribute*;
declareClass:
	'cum drop' ID=Ident ('as a' Ident)* attribute* (classProperty|declareFunction)*;
useNamespace:
	(('exploited cumdumpster'|'exploited cumdumpsters') ('called'|'known as') (Ident)+)+;
attribute:
	'with' ID=Ident 'dick cheese' ('with' expr+ '!')?;
Modifier:
	'static'|'public'|'readonly'|'protected'|'override'|'virtual'|'async'|'private';
Operator:
	'plus'|'minus'|('multipliy'|'multiplied')|('divide'|'divided')|'equals'|('not equals'|'not equal')|'bigger than'|'bigger or equal to'|'less than'|'less than or equal to';
Number:
	[0-9]+;
Ident:
	IdentPart ('.' IdentPart)*;
IdentPart:
	IdentChar(IdentChar|Number)*;
IdentChar:
	[a-zA-Z]|'<'|'>';
WS:
	(' '|'\r'|'\n'|'\t') -> channel(HIDDEN);