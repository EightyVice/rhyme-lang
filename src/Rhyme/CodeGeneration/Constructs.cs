using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Rhyme.CodeGeneration
{
    internal static class Constructs
    {
        internal class String : Vector
        {
            public String(string text) 
                : base(LLVMTypeRef.Int8, 
                      Encoding.UTF8.GetBytes(text).Select(b => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, b)).ToArray()
                )
            {

            }
        }

        internal class Vector
        {
            public static readonly LLVMTypeRef StructTypeRef = LLVMTypeRef.CreateStruct(
                [
                    LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0), // Pointer
                    LLVMTypeRef.Int32, // Capacity
                    LLVMTypeRef.Int32, // Length
                ],
                false
            );

            public LLVMValueRef VectorValueRef { get; private set; }
            public LLVMTypeRef  TypeRef { get; private set; }
            public LLVMValueRef[] ElementsRefs { get; private set; }
            public Vector(LLVMTypeRef typeRef, LLVMValueRef[] valueRefs)
            {
                TypeRef = typeRef;
                ElementsRefs = valueRefs;
            }

            public LLVMValueRef DeclareOnStack(LLVMBuilderRef builder)
            {
                var vecStruct = builder.BuildAlloca(TypeRef, "_vect_");

                int len = ElementsRefs.Length > 16 ? ElementsRefs.Length : 16;
                var malloc = builder.BuildArrayMalloc(TypeRef, Utils.LLVMInt(len), "vecm");

                builder.BuildStore(LLVMValueRef.CreateConstArray(TypeRef, ElementsRefs), malloc);

                builder.BuildStore(malloc, builder.BuildStructGEP2(StructTypeRef, vecStruct, 0, "VECP"));
                builder.BuildStore(Utils.LLVMInt(len), builder.BuildStructGEP2(StructTypeRef, vecStruct, 1, "VECC"));
                builder.BuildStore(Utils.LLVMInt(len), builder.BuildStructGEP2(StructTypeRef, vecStruct, 2, "VECL"));

                VectorValueRef = vecStruct;
                return vecStruct;
            }
        }
    }
}
