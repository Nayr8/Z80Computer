namespace Z80Assembler.Ast;



/* Combinations
 bc, nn
 (bc), a
 bc
 b
 b, n
 af, af'

*/
public enum SingleOperandType
{
    Register8,
    Register16,
    
}