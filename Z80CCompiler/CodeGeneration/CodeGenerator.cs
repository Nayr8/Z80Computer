using Z80CCompiler.Parsing.ASTNodes;
using Z80CCompiler.Parsing.ASTNodes.Expression;

namespace Z80CCompiler.Assembling;

public class CodeGenerator
{
    private readonly AbstractSyntaxTree _ast;
    private readonly GeneratedCode _generatedCode = new();
    
    public CodeGenerator(AbstractSyntaxTree ast)
    {
        _ast = ast;
    }

    public GeneratedCode Generate()
    {
        GenerateFunction(_ast.Function);
        return _generatedCode;
    }

    private void GenerateFunction(Function function)
    {
        _generatedCode.AddLabel(function.Identifier);
        GenerateStatement(function.Statement);
    }

    private void GenerateStatement(Statement statement)
    {
        GenerateExpression(statement.Expression);
        
        // ret
        _generatedCode.AddByte(0xC9);
    }

    private void GenerateExpression(LogicalOrExpression expression)
    {
        
        GenerateLogicalAndExpression(expression.LogicalAndExpression);
        foreach (LogicalAndExpression logicalAndExpression in expression.OrEqualityExpressions)
        {
            string endLabel = _generatedCode.GenerateTempLabel();

            // cp 0
            _generatedCode.AddByte(0xFE);
            _generatedCode.AddByte(0x00);
            // jr nz, 5 ; second
            _generatedCode.AddByte(0x28);
            _generatedCode.AddByte(0x05);
            // jp end ; unknown distance safer to use absolute TODO never mind change to jr as the position is not garanteed or make the label not tempory
            _generatedCode.AddByte(0xC3);
            _generatedCode.AddTempLabelPointer(endLabel);
            // second:

            GenerateLogicalAndExpression(logicalAndExpression);

            // cp 0
            _generatedCode.AddByte(0xFE);
            _generatedCode.AddByte(0x00);
            // ld a, 0 ; doesn't effect flags
            _generatedCode.AddByte(0x3E);
            _generatedCode.AddByte(0x00);
            // jr nz, 4 ; end
            _generatedCode.AddByte(0x20);
            _generatedCode.AddByte(0x04);
            // ld a, 1
            _generatedCode.AddByte(0x3E);
            _generatedCode.AddByte(0x01);
            // end:
            _generatedCode.AddTempLabel(endLabel);
        }
    }

    private void GenerateLogicalAndExpression(LogicalAndExpression expression)
    {

        GenerateBitwiseOrExpression(expression.BitwiseOrExpression);
        foreach (BitwiseOrExpression bitwiseOrExpression in expression.BitwiseOrAndExpressions)
        {
            string endLabel = _generatedCode.GenerateTempLabel();

            // cp 0
            _generatedCode.AddByte(0xFE);
            _generatedCode.AddByte(0x00);
            // jr z, 7 ; second
            _generatedCode.AddByte(0x20);
            _generatedCode.AddByte(0x07);
            // ld a, 1
            _generatedCode.AddByte(0x3E);
            _generatedCode.AddByte(0x01);
            // jp end ; unknown distance safer to use absolute
            _generatedCode.AddByte(0xC3);
            _generatedCode.AddTempLabelPointer(endLabel);
            // second:

            GenerateBitwiseOrExpression(bitwiseOrExpression);

            // cp 0
            _generatedCode.AddByte(0xFE);
            _generatedCode.AddByte(0x00);
            // ld a, 0 ; doesn't effect flags
            _generatedCode.AddByte(0x3E);
            _generatedCode.AddByte(0x00);
            // jr nz, 4 ; end
            _generatedCode.AddByte(0x20);
            _generatedCode.AddByte(0x04);
            // ld a, 1
            _generatedCode.AddByte(0x3E);
            _generatedCode.AddByte(0x01);
            // end:
            _generatedCode.AddTempLabel(endLabel);
        }
    }

    private void GenerateBitwiseOrExpression(BitwiseOrExpression expression)
    {
        GenerateBitwiseXorExpression(expression.BitwiseXorExpression);
        foreach (BitwiseXorExpression bitwiseXorExpression in expression.BitwiseOrBitwiseXorExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateBitwiseXorExpression(bitwiseXorExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);

            // or b
            _generatedCode.AddByte(0xB0);
        }
    }

    private void GenerateBitwiseXorExpression(BitwiseXorExpression expression)
    {
        GenerateBitwiseAndExpression(expression.BitwiseAndExpression);
        foreach (BitwiseAndExpression bitwiseAndExpression in expression.BitwiseXorBitwiseAndExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateBitwiseAndExpression(bitwiseAndExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);

            // xor b
            _generatedCode.AddByte(0xA8);
        }
    }

    private void GenerateBitwiseAndExpression(BitwiseAndExpression expression)
    {
        GenerateEqualityExpression(expression.EqualityExpression);
        foreach (EqualityExpression equalityExpression in expression.BitwiseAndEqualityExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateEqualityExpression(equalityExpression);
            // pop bc
            _generatedCode.AddByte(0xC1);

            // and b
            _generatedCode.AddByte(0xA0);
        }
    }

    private void GenerateEqualityExpression(EqualityExpression expression)
    {
        GenerateRelationalExpression(expression.RelationalExpression);
        foreach ((EqualityOp op, RelationalExpression relationalExpression) in expression.EqualityRelationalExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateRelationalExpression(relationalExpression);
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

    private void GenerateRelationalExpression(RelationalExpression expression)
    {
        GenerateAdditiveExpression(expression.AdditiveExpression);
        foreach ((RelationalOp op, AdditiveExpression additiveExpression) in expression.RelationalAdditiveExpressions)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateAdditiveExpression(additiveExpression);
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

    private void GenerateAdditiveExpression(AdditiveExpression expression)
    {
        GenerateTerm(expression.Term);
        foreach ((AddSubOp op, Term term) in expression.AddSubTerms)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateTerm(term);
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

    private void GenerateTerm(Term term)
    {
        GenerateFactor(term.Factor);
        foreach ((MulDivOp op, object factor) in term.MulDivFactors)
        {
            // push af
            _generatedCode.AddByte(0xF5);
            GenerateFactor(factor);
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
                case MulDivOp.Modulo: // a / b
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
                    // end: add a, b
                    _generatedCode.AddByte(0x80);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void GenerateFactor(object factor)
    {
        switch (factor)
        {
            case IntFactor integer:
                GenerateIntegerFactor(integer.Integer); return;
            case UnaryFactor unaryFactor:
                GenerateUnaryFactor(unaryFactor.UnaryOp);
                GenerateFactor(unaryFactor.Factor);
                return;
            case LogicalOrExpression expression:
                GenerateExpression(expression); return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void GenerateIntegerFactor(int integer)
    {
        // ld a, n
        _generatedCode.AddByte(0x3E);
        _generatedCode.AddByte((byte)integer);
    }

    private void GenerateUnaryFactor(UnaryOp unaryOp)
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