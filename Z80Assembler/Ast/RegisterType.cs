using Z80Assembler.Tokens;

namespace Z80Assembler.Ast;

public enum RegisterType
{
    A,
    B,
    C,
    D,
    E,
    H,
    L,
    Af,
    AfShadow,
    Bc,
    De,
    Hl,
    Ix,
    Iy,
    Ixh,
    Ixl,
    Iyh,
    Iyl,
}

public static class RegisterExtensions
{
    public static RegisterType? ToRegisterType(this TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.A => RegisterType.A,
            TokenType.B => RegisterType.B,
            TokenType.C => RegisterType.C,
            TokenType.D => RegisterType.D,
            TokenType.E => RegisterType.E,
            TokenType.H => RegisterType.H,
            TokenType.L => RegisterType.L,
            TokenType.Af => RegisterType.Af,
            TokenType.AfShadow => RegisterType.AfShadow,
            TokenType.Bc => RegisterType.Bc,
            TokenType.De => RegisterType.De,
            TokenType.Hl => RegisterType.Hl,
            TokenType.Ix => RegisterType.Ix,
            TokenType.Iy => RegisterType.Iy,
            TokenType.Ixh => RegisterType.Ixh,
            TokenType.Ixl => RegisterType.Ixl,
            TokenType.Iyh => RegisterType.Iyh,
            TokenType.Iyl => RegisterType.Iyl,
            _ => null
        };
    }
}