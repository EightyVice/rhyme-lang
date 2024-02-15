﻿using Rhyme.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhyme.Scanner;
using Rhyme.Parser;
using Rhyme.Resolver;

namespace Rhyme.TypeChecker
{
    
    internal class TypeChecker : Node.IVisitor<RhymeType>, ICompilerPass
    {
        

        private readonly IReadOnlySymbolTable _symbolTable;

        public bool HadError { get; private set; }

        List<PassError> _errors = new List<PassError>();

        public IReadOnlyCollection<PassError> Errors { get; private set; }

        internal enum Operator
        {
            Assignment,
            Arithmetic,
            Bitwise
        }

        public TypeChecker(IReadOnlySymbolTable symbolTable)
        {
            _symbolTable = symbolTable;
            Errors = _errors;   
        }

        public bool Check(Node.CompilationUnit program)
        {
            check(program);
            return true;
        }

        RhymeType check(Node node)
        {
            return node.Accept(this);
        }
        public RhymeType Visit(Node.Literal literalExpr)
        {
            if (literalExpr.ValueToken.Value is int)
                return RhymeType.U32;

            if (literalExpr.ValueToken.Value is string)
                return RhymeType.Str;

            
            return RhymeType.NoneType;
        }

        public RhymeType Visit(Node.Binary binaryExpr)
        {
            var lhs = check(binaryExpr.Left);
            var rhs = check(binaryExpr.Right);

            var eval_result = TypeEvaluate(lhs, binaryExpr.Op.Type, rhs);

            if (eval_result.valid)
                return eval_result.result;
            else
                Error(binaryExpr.Op, $"Can't apply operator '{binaryExpr.Op.Lexeme}' on types '{lhs}' and '{rhs}'");

            return RhymeType.NoneType;
        }

        public RhymeType Visit(Node.Unary unaryExpr)
        {
            throw new NotImplementedException();
        }

        public RhymeType Visit(Node.Block blockExpr)
        {
            _symbolTable.OpenScope();

            foreach(var exprstmt in blockExpr.ExpressionsStatements)
                check(exprstmt);

            _symbolTable.CloseScope();

            return new RhymeType.Function(RhymeType.Void);
        }

        public RhymeType Visit(Node.BindingDeclaration bindingDecl)
        {
            var rhs_type = check(bindingDecl.expression);

            if (rhs_type == RhymeType.NoneType)
                return RhymeType.NoneType;

            var decl = bindingDecl.Declaration;
            var decl_type = _symbolTable[decl.Identifier];
            if (!decl_type.Equals(rhs_type)){
                Error(decl.Identifier.Line, decl.Identifier.Start, decl.Identifier.Lexeme.Length,
                    $"Can not implicitly convert type '{rhs_type}' to a binding of type '{decl_type}'");
            }
            
            return RhymeType.NoneType;
        }

        void Error(Token error_token, string message)
        {
            Error(error_token.Line, error_token.Start, error_token.Lexeme.Length, message);
        }
        void Error(int line, int start, int length, string message)
        {
            HadError = true;
            _errors.Add(new PassError(line, start, length, message));
        }
        public RhymeType Visit(Node.If ifStmt)
        {
            throw new NotImplementedException();
        }

        public RhymeType Visit(Node.Assignment assignment)
        {
            var rhs = check(assignment.Expression);

            if (assignment.Assignee is not Node.Binding)
                throw new Exception("Unassignable target.");

            var lhs = _symbolTable[((Node.Binding)assignment.Assignee).Identifier];
            var eval_result = TypeEvaluate(rhs, TokenType.Equal, lhs);

            if (eval_result.valid)
            {
                return eval_result.result;
            }
            else
            {
                Error(((Node.Binding)assignment.Assignee).Identifier,
                    $"Can't implicitly assign value of type '{rhs}' to a binding of type '{lhs}'");
                return RhymeType.NoneType;
            }
        }

        
        (bool valid, RhymeType result) TypeEvaluate(RhymeType lhs, TokenType operatorToken, RhymeType rhs)
        {
            switch (operatorToken)
            {
                // Arithmetic
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Asterisk:
                case TokenType.Slash:
                case TokenType.Percent:
                    if (lhs is not RhymeType.Numeric || rhs is not RhymeType.Numeric)
                        return (false, RhymeType.NoneType);

                    if (lhs is RhymeType.Numeric && rhs is RhymeType.Numeric)
                        return (true, RhymeType.Numeric.Max((RhymeType.Numeric)lhs, (RhymeType.Numeric)rhs));

                    break;

                // Assignment
                case TokenType.Equal:
                    if (rhs == lhs)
                        return (true, rhs);

                    if (lhs is RhymeType.Primitive && rhs is RhymeType.Primitive)
                    {
                        if (lhs is RhymeType.Numeric && rhs is RhymeType.Numeric)
                        {
                            // Only assignment from lower numeric types to higher ones is allowed (e.g. u16 to f64)

                            bool valid = (RhymeType.Numeric)lhs > (RhymeType.Numeric)rhs;

                            if (valid)
                                return (valid, RhymeType.Numeric.Max((RhymeType.Numeric)lhs, (RhymeType.Numeric)rhs));
                            else
                                return (false, RhymeType.NoneType);
                        }
                    }
                    break;
                default:
                    return (false, RhymeType.NoneType);
            }

            return (false, RhymeType.NoneType);
        }

        public RhymeType Visit(Node.Binding binding)
        {
            if (_symbolTable.Contains(binding.Identifier))
                return _symbolTable[binding.Identifier];
            else
                return RhymeType.NoneType;
        }

        public RhymeType Visit(Node.Grouping grouping)
        {
            throw new NotImplementedException();
        }

        public RhymeType Visit(Node.CompilationUnit compilationUnit)
        {
            foreach(var unit in compilationUnit.Units)
            {
                check(unit);
            }
            return RhymeType.NoneType;
        }

        public RhymeType Visit(Node.FunctionCall callExpr)
        {
            var type = check(callExpr.Callee);

            if(type is RhymeType.Function)
            {
                var func_type = (RhymeType.Function)type;

                if (func_type.Parameters.Length == callExpr.Args.Length)
                {
                    for (int i = 0; i < func_type.Parameters.Length; i++)
                    {
                        var arg_type = check(callExpr.Args[i]);

                        if (arg_type != func_type.Parameters[i])
                        {
                            throw new Exception($"Wrong argument type from {arg_type} to {func_type.Parameters[i]}");
                        }
                    }
                    return func_type.ReturnType;
                }
                throw new Exception($"{callExpr.Args.Length} arguments passed to the function and expects {func_type.Parameters.Length}");
            }
            throw new Exception("Invalid call");
        }
    }
}
