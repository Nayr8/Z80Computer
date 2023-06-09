using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
