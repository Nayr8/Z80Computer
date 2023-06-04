using Z80CCompiler.Parsing.ASTNodes;

namespace Z80CCompiler.Assembling;

public class Assembler
{
    private AbstractSyntaxTree _ast;
    private AssembledCode _assembledCode = new();
    
    public Assembler(AbstractSyntaxTree ast)
    {
        _ast = ast;
    }

    public AssembledCode Assemble()
    {
        AssembleFunction(_ast.Function);
        return _assembledCode;
    }

    private void AssembleFunction(Function function)
    {
        _assembledCode.AddLabel(function.Identifier);
        AssembleStatement(function.Statement);
    }

    private void AssembleStatement(Statement statement)
    {
        AssembleExpression(statement.Expression);
        
        // ret
        _assembledCode.AddByte(0xC9);
    }

    private void AssembleExpression(Expression expression)
    {
        AssembleTerm(expression.Term);
        foreach ((AddSubOp op, Term term) in expression.AddSubTerms)
        {
            // push af
            _assembledCode.AddByte(0xF5);
            AssembleTerm(term);
            // pop bc
            _assembledCode.AddByte(0xC1);
            switch (op)
            {
                case AddSubOp.Add:
                    // add a, b
                    _assembledCode.AddByte(0x80);
                    break;
                case AddSubOp.Sub:
                    // sub b
                    _assembledCode.AddByte(0x90);
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
            _assembledCode.AddByte(0xF5);
            AssembleFactor(factor);
            // pop bc
            _assembledCode.AddByte(0xC1);
            switch (op)
            {
                case MulDivOp.Mul: // a * b
                    // add a, a
                    _assembledCode.AddByte(0x87);
                    // djnz -1
                    _assembledCode.AddByte(0x10);
                    _assembledCode.AddByte(0xFF);
                    break;
                case MulDivOp.Div: // a / b
                    // zero c
                    // minus b from a until 1 before overflow
                    // ld c, 0
                    _assembledCode.AddByte(0x0E);
                    _assembledCode.AddByte(0x00);
                    // loop: sub b
                    _assembledCode.AddByte(0x90);
                    // jr c, 5 ; end
                    _assembledCode.AddByte(0x38);
                    _assembledCode.AddByte(0x05);
                    // inc c
                    _assembledCode.AddByte(0x0C);
                    // jr -4 ; loop
                    _assembledCode.AddByte(0x18);
                    _assembledCode.AddByte(0xFC);
                    // end: ld a, c
                    _assembledCode.AddByte(0x79);
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
        _assembledCode.AddByte(0x3E);
        _assembledCode.AddByte((byte)integer);
    }

    private void AssembleUnaryFactor(UnaryOp unaryOp)
    {
        switch (unaryOp)
        {
            case UnaryOp.Negate:
                // neg
                _assembledCode.AddByte(0xED);
                _assembledCode.AddByte(0x44);
                break;
            case UnaryOp.BitwiseComplement:
                // cpl
                _assembledCode.AddByte(0x2F);
                break;
            case UnaryOp.LogicalNegate:
                // or a ; set zero flag is a is zero
                _assembledCode.AddByte(0xB7);
                // jr z, 5
                _assembledCode.AddByte(0x28);
                _assembledCode.AddByte(0x05);
                // xor a ; set a to zero
                _assembledCode.AddByte(0xAF);
                // jr 4
                _assembledCode.AddByte(0x18);
                _assembledCode.AddByte(0x04);
                // ld a, 1 ; set a to one
                _assembledCode.AddByte(0x3E);
                _assembledCode.AddByte(0x01);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}