using Z80CCompiler.Parsing.ASTNodes;

namespace Z80CCompiler.Assembling;

public class CodeGenerator
{
    private readonly AbstractSyntaxTree _ast;
    private readonly GeneratedCode _generatedCode = new();
    
    public CodeGenerator(AbstractSyntaxTree ast)
    {
        _ast = ast;
    }

    public GeneratedCode Assemble()
    {
        AssembleFunction(_ast.Function);
        return _generatedCode;
    }

    private void AssembleFunction(Function function)
    {
        _generatedCode.AddLabel(function.Identifier);
        AssembleStatement(function.Statement);
    }

    private void AssembleStatement(Statement statement)
    {
        AssembleExpression(statement.Expression);
        
        // ret
        _generatedCode.AddByte(0xC9);
    }

    private void AssembleExpression(Expression expression)
    {
        
        AssembleLogicalAndExpression(expression.LogicalAndExpression);
        foreach (LogicalAndExpression logicalAndExpression in expression.OrEqualityExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            AssembleLogicalAndExpression(logicalAndExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);
            // TODO
        }
    }

    private void AssembleLogicalAndExpression(LogicalAndExpression expression)
    {
        
        AssembleEqualityExpression(expression.EqualityExpression);
        foreach (EqualityExpression equalityExpression in expression.AndEqualityExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            AssembleEqualityExpression(equalityExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);
            // TODO
        }
    }

    private void AssembleEqualityExpression(EqualityExpression expression)
    {
        AssembleRelationalExpression(expression.RelationalExpression);
        foreach ((EqualityOp op, RelationalExpression relationalExpression) in expression.EqualityRelationalExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            AssembleRelationalExpression(relationalExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);
            switch (op)
            {
                case EqualityOp.Equal:
                    // cp b
                    _generatedCode.AddByte(0xB8);
                    // jr z, 5 ; true
                    _generatedCode.AddByte(0x28);
                    _generatedCode.AddByte(0x05);
                    // false: xor a
                    _generatedCode.AddByte(0xAF);
                    // jr 4 ; end
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0x04);
                    // true: ld a, 1
                    _generatedCode.AddByte(0x3E);
                    _generatedCode.AddByte(0x01);
                    // end:
                    break;
                case EqualityOp.NotEqual:
                    // cp b
                    _generatedCode.AddByte(0xB8);
                    // jr nz, 5 ; true
                    _generatedCode.AddByte(0x20);
                    _generatedCode.AddByte(0x05);
                    // false: xor a
                    _generatedCode.AddByte(0xAF);
                    // jr 4 ; end
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0x04);
                    // true: ld a, 1
                    _generatedCode.AddByte(0x3E);
                    _generatedCode.AddByte(0x01);
                    // end:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void AssembleRelationalExpression(RelationalExpression expression)
    {
        AssembleAdditiveExpression(expression.AdditiveExpression);
        foreach ((RelationalOp op, AdditiveExpression additiveExpression) in expression.RelationalAdditiveExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            AssembleAdditiveExpression(additiveExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);
            switch (op)
            {
                case RelationalOp.GreaterThan:
                    // cp b
                    _generatedCode.AddByte(0xB8);
                    // jr c, 8 ; zero
                    _generatedCode.AddByte(0x38);
                    _generatedCode.AddByte(0x08);
                    // jr z, 6 ; zero
                    _generatedCode.AddByte(0x28);
                    _generatedCode.AddByte(0x06);
                    // ld a, 1
                    _generatedCode.AddByte(0x3E);
                    _generatedCode.AddByte(0x01);
                    // jr 3 ; end
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0x03);
                    // zero: xor a
                    _generatedCode.AddByte(0xAF);
                    // end:
                    break;
                case RelationalOp.GreaterThanEqual:
                    // cp b
                    _generatedCode.AddByte(0xB8);
                    // jr c, 6 ; false
                    _generatedCode.AddByte(0x38);
                    _generatedCode.AddByte(0x06);
                    // true: ld a, 1
                    _generatedCode.AddByte(0x3E);
                    _generatedCode.AddByte(0x01);
                    // jr 3 ; end
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0x03);
                    // false: xor a
                    _generatedCode.AddByte(0xAF);
                    // end:
                    break;
                case RelationalOp.LessThan:
                    // cp b
                    _generatedCode.AddByte(0xB8);
                    // jr c, 5 ; true
                    _generatedCode.AddByte(0x38);
                    _generatedCode.AddByte(0x05);
                    // false: xor a
                    _generatedCode.AddByte(0xAF);
                    // jr 4 ; end
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0x04);
                    // true: ld a, 1
                    _generatedCode.AddByte(0x3E);
                    _generatedCode.AddByte(0x01);
                    // end:
                    break;
                case RelationalOp.LessThanEqual:
                    // cp b
                    _generatedCode.AddByte(0xB8);
                    // jr c, 7 ; true
                    _generatedCode.AddByte(0x38);
                    _generatedCode.AddByte(0x07);
                    // jr z, 5 ; true
                    _generatedCode.AddByte(0x28);
                    _generatedCode.AddByte(0x05);
                    // false: xor a
                    _generatedCode.AddByte(0xAF);
                    // jr 4 ; end
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0x04);
                    // true: ld a, 1
                    _generatedCode.AddByte(0x3E);
                    _generatedCode.AddByte(0x01);
                    // end:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }

    private void AssembleAdditiveExpression(AdditiveExpression expression)
    {
        AssembleTerm(expression.Term);
        foreach ((AddSubOp op, Term term) in expression.AddSubTerms)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            AssembleTerm(term);
            // pop bc
            _generatedCode.AddByte(0xC1);
            switch (op)
            {
                case AddSubOp.Add:
                    // add a, b
                    _generatedCode.AddByte(0x80);
                    break;
                case AddSubOp.Sub:
                    // sub b
                    _generatedCode.AddByte(0x90);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void AssembleTerm(Term term)
    {
        AssembleFactor(term.Factor);
        foreach ((MulDivOp op, object factor) in term.MulDivFactors)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            AssembleFactor(factor);
            // pop bc
            _generatedCode.AddByte(0xC1);
            switch (op)
            {
                case MulDivOp.Mul: // a * b
                    // add a, a
                    _generatedCode.AddByte(0x87);
                    // djnz -1
                    _generatedCode.AddByte(0x10);
                    _generatedCode.AddByte(0xFF);
                    break;
                case MulDivOp.Div: // a / b
                    // zero c
                    // minus b from a until 1 before overflow
                    // ld c, 0
                    _generatedCode.AddByte(0x0E);
                    _generatedCode.AddByte(0x00);
                    // loop: sub b
                    _generatedCode.AddByte(0x90);
                    // jr c, 5 ; end
                    _generatedCode.AddByte(0x38);
                    _generatedCode.AddByte(0x05);
                    // inc c
                    _generatedCode.AddByte(0x0C);
                    // jr -4 ; loop
                    _generatedCode.AddByte(0x18);
                    _generatedCode.AddByte(0xFC);
                    // end: ld a, c
                    _generatedCode.AddByte(0x79);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void AssembleFactor(object factor)
    {
        switch (factor)
        {
            case IntFactor integer:
                AssembleIntegerFactor(integer.Integer); return;
            case UnaryFactor unaryFactor:
                AssembleUnaryFactor(unaryFactor.UnaryOp);
                AssembleFactor(unaryFactor.Factor);
                return;
            case Expression expression:
                AssembleExpression(expression); return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AssembleIntegerFactor(int integer)
    {
        // ld a, n
        _generatedCode.AddByte(0x3E);
        _generatedCode.AddByte((byte)integer);
    }

    private void AssembleUnaryFactor(UnaryOp unaryOp)
    {
        switch (unaryOp)
        {
            case UnaryOp.Negate:
                // neg
                _generatedCode.AddByte(0xED);
                _generatedCode.AddByte(0x44);
                break;
            case UnaryOp.BitwiseComplement:
                // cpl
                _generatedCode.AddByte(0x2F);
                break;
            case UnaryOp.LogicalNegate:
                // or a ; set zero flag is a is zero
                _generatedCode.AddByte(0xB7);
                // jr z, 5
                _generatedCode.AddByte(0x28);
                _generatedCode.AddByte(0x05);
                // xor a ; set a to zero
                _generatedCode.AddByte(0xAF);
                // jr 4
                _generatedCode.AddByte(0x18);
                _generatedCode.AddByte(0x04);
                // ld a, 1 ; set a to one
                _generatedCode.AddByte(0x3E);
                _generatedCode.AddByte(0x01);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}