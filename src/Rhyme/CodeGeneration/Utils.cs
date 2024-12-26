using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhyme.CodeGeneration
{
    internal class Utils
    {
        public static LLVMValueRef LLVMInt(int value) => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)value);
    }
}
