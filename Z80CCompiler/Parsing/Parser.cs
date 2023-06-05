using Z80CCompiler.Parsing.ASTNodes;
using Z80CCompiler.Parsing.ASTNodes.Expression;
using Z80CCompiler.Parsing.ASTNodes.Factor;
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
        LogicalOrExpression expression = ParseExpression();
        NextAssert<SemicolonToken>();
        return new Statement(expression);
    }

    private LogicalOrExpression ParseExpression()
    {
        LogicalOrExpression expression = new(ParseLogicalAndExpression());
        while (Peek() is OrToken)
        {
            Next();
            LogicalAndExpression logicalAndExpression = ParseLogicalAndExpression();
            expression.OrEqualityExpressions.Add(logicalAndExpression);
        }
        return expression;
    }

    private LogicalAndExpression ParseLogicalAndExpression()
    {
        LogicalAndExpression logicalAndExpression = new(ParseBitwiseOrExpression());
        while (Peek() is AndToken)
        {
            Next();
            BitwiseOrExpression bitwiseOrExpression = ParseBitwiseOrExpression();
            logicalAndExpression.BitwiseOrAndExpressions.Add(bitwiseOrExpression);
        }
        return logicalAndExpression;
    }

    private BitwiseOrExpression ParseBitwiseOrExpression()
    {
        BitwiseOrExpression bitwiseOrExpression = new(ParseBitwiseXorExpression());
        while (Peek() is AndToken)
        {
            Next();
            BitwiseXorExpression bitwiseXorExpression = ParseBitwiseXorExpression();
            bitwiseOrExpression.BitwiseOrBitwiseXorExpressions.Add(bitwiseXorExpression);
        }
        return bitwiseOrExpression;
    }

    private BitwiseXorExpression ParseBitwiseXorExpression()
    {
        BitwiseXorExpression bitwiseXorExpression = new(ParseBitwiseAndExpression());
        while (Peek() is AndToken)
        {
            Next();
            BitwiseAndExpression bitwiseAndExpression = ParseBitwiseAndExpression();
            bitwiseXorExpression.BitwiseXorBitwiseAndExpressions.Add(bitwiseAndExpression);
        }
        return bitwiseXorExpression;
    }

    private BitwiseAndExpression ParseBitwiseAndExpression()
    {
        BitwiseAndExpression bitwiseAndExpression = new(ParseEqualityExpression());
        while (Peek() is AndToken)
        {
            Next();
            EqualityExpression relationalExpression = ParseEqualityExpression();
            bitwiseAndExpression.BitwiseAndEqualityExpressions.Add(relationalExpression);
        }
        return bitwiseAndExpression;
    }

    private EqualityExpression ParseEqualityExpression()
    {
        EqualityExpression equalityExpression = new(ParseRelationalExpression());
        while (Peek() is EqualToken or NotEqualToken)
        {
            EqualityOp op = Next() switch
            {
                EqualToken => EqualityOp.Equal,
                NotEqualToken => EqualityOp.NotEqual,
                _ => throw new ArgumentOutOfRangeException()
            };
            RelationalExpression relationalExpression = ParseRelationalExpression();
            equalityExpression.EqualityRelationalExpressions.Add((op, relationalExpression));
        }
        return equalityExpression;
    }

    private RelationalExpression ParseRelationalExpression()
    {
        RelationalExpression relationalExpression = new(ParseAdditiveExpression());
        while (Peek() is LessThanToken or LessThanEqualToken or GreaterThanToken or GreaterThanEqualToken)
        {
            RelationalOp op = Next() switch
            {
                LessThanToken => RelationalOp.LessThan,
                LessThanEqualToken => RelationalOp.LessThanEqual,
                GreaterThanToken => RelationalOp.GreaterThan,
                GreaterThanEqualToken => RelationalOp.GreaterThanEqual,
                _ => throw new ArgumentOutOfRangeException()
            };
            AdditiveExpression additiveExpression = ParseAdditiveExpression();
            relationalExpression.RelationalAdditiveExpressions.Add((op, additiveExpression));
        }
        return relationalExpression;
    }

    private AdditiveExpression ParseAdditiveExpression()
    {
        AdditiveExpression additiveExpression = new(ParseTerm());
        while (Peek() is AdditionToken or NegationToken)
        {
            AddSubOp op = Next() switch
            {
                AdditionToken => AddSubOp.Add,
                NegationToken => AddSubOp.Sub,
                _ => throw new ArgumentOutOfRangeException()
            };
            Term term = ParseTerm();
            additiveExpression.AddSubTerms.Add((op, term));
        }
        return additiveExpression;
    }

    private Term ParseTerm()
    {
        Term term = new(ParseFactor());
        while (Peek() is MultiplicationToken or DivisionToken)
        {
            MulDivOp op = Next() switch
            {
                MultiplicationToken => MulDivOp.Mul,
                DivisionToken => MulDivOp.Div,
                _ => throw new ArgumentOutOfRangeException()
            };
            IFactor factor = ParseFactor();
            term.MulDivFactors.Add((op, factor));
        }
        return term;
    }

    private IFactor ParseFactor()
    {
        switch (Next())
        {
            case LBracket:
                LogicalOrExpression expression = ParseExpression();
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