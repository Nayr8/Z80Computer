using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z80Assembler.Tokens;

namespace Z80Assembler.Ast;
public enum FlagCheckType
{
    Nz,
    Nc,
    Po,
    P,
    Z,
    C,
    Pe,
    M,
}

public static class FlagCheckTypeExtensions
{
    public static FlagCheckType? ToFlagCheck(this TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Nz => FlagCheckType.Nz,
            TokenType.Nc => FlagCheckType.Nc,
            TokenType.Po => FlagCheckType.Po,
            TokenType.P => FlagCheckType.P,
            TokenType.Z => FlagCheckType.Z,
            TokenType.C => FlagCheckType.C,
            TokenType.Pe => FlagCheckType.Pe,
            TokenType.M => FlagCheckType.M,
            _ => null
        };
    }
}