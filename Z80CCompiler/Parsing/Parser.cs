using Z80CCompiler.Parsing.ASTNodes;
using Z80CCompiler.Parsing.Tokens;

namespace Z80CCompiler.Parsing;

public class Parser
{
    public List<IToken> _tokens { get; } = new();
    private int _cursor;

    public Parser(List<IToken> tokens)
    {
        _tokens = tokens;
    }

    private IToken? Peek()
    {
        return _cursor < _tokens.Count ? _tokens[_cursor] : null;
    }

    private IToken? Next()
    {
        IToken? token = _cursor < _tokens.Count ? _tokens[_cursor] : null;
        ++_cursor;
        return token;
    }

    private void NextAssert<T>() where T : IToken
    {
        if (Next() is not T)
        {
            throw new Exception();
        }
    }

    private T NextAssertAndReturn<T>() where T : IToken
    {
        if (Next() is T token)
        {
            return token;
        }
        throw new Exception();
    }

    public AbstractSyntaxTree Parse()
    {
        return new AbstractSyntaxTree(ParseFunction());
    }

    private Function ParseFunction()
    {
        NextAssert<IntKeywordToken>();
        string id = NextAssertAndReturn<IdentifierToken>().Identifier;
        NextAssert<LBracket>();
        NextAssert<RBracket>();
        NextAssert<LBrace>();
        Statement statement = ParseStatement();
        NextAssert<RBrace>();
        return new Function(id, statement);
    }

    private Statement ParseStatement()
    {
        NextAssert<ReturnKeywordToken>();
        Expression expression = ParseExpression();
        NextAssert<SemicolonToken>();
        return new Statement(expression);
    }

    private Expression ParseExpression()
    {
        Term firstTerm = ParseTerm();
        Expression expression = new(firstTerm);
        while (Peek() is AdditionToken or NegationToken)
        {
            AddSubOp op = Next() switch
            {
                AdditionToken => AddSubOp.Add,
                NegationToken => AddSubOp.Sub,
                _ => throw new ArgumentOutOfRangeException()
            };
            Term term = ParseTerm();
            expression.AddSubTerms.Add((op, term));
        }
        return expression;
    }

    private Term ParseTerm()
    {
        object firstFactor = ParseFactor();
        Term term = new(firstFactor);
        while (Peek() is MultiplicationToken or DivisionToken)
        {
            MulDivOp op = Next() switch
            {
                MultiplicationToken => MulDivOp.Mul,
                DivisionToken => MulDivOp.Div,
                _ => throw new ArgumentOutOfRangeException()
            };
            object factor = ParseFactor();
            term.MulDivFactors.Add((op, factor));
        }
        return term;
    }

    private object ParseFactor()
    {
        switch (Next())
        {
            case LBracket:
                Expression expression = ParseExpression();
                NextAssert<RBracket>();
                return expression;
            case LogicalNegationToken:
                return new UnaryFactor(UnaryOp.LogicalNegate, ParseFactor());
            case BitwiseComplementToken:
                return new UnaryFactor(UnaryOp.BitwiseComplement, ParseFactor());
            case NegationToken:
                return new UnaryFactor(UnaryOp.Negate, ParseFactor());
            case IntegerLiteralToken token:
                return new IntFactor(token.Integer);
            default: throw new NotImplementedException();
        }
    }
}