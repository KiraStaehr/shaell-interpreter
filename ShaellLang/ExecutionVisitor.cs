﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ShaellLang;

public class ExecutionVisitor : ShaellBaseVisitor<IValue>
{
    private ScopeManager _scopeManager;
    private ScopeContext _globalScope;
    private bool _shouldReturn;
    private string[] _args;
    public ExecutionVisitor(string[] args)
    {
        _globalScope = new ScopeContext();
        _scopeManager = new ScopeManager();
        _scopeManager.PushScope(_globalScope);
        _args = args;
        _shouldReturn = false;
    }
    
    public ExecutionVisitor()
    {
        _globalScope = new ScopeContext();
        _scopeManager = new ScopeManager();
        _scopeManager.PushScope(_globalScope);
        _shouldReturn = false;
    }
    
    public ExecutionVisitor(ScopeContext globalScope, ScopeManager scopeManager)
    {
        _globalScope = globalScope;
        _scopeManager = scopeManager;
        _shouldReturn = false;
    }

    private IValue SafeVisit(ParserRuleContext context)
    {
        try
        {
            return Visit(context);
        }
        catch (SemanticError ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SemanticError(ex.Message, context.start, context.stop);
        }
    }
    
    public void SetGlobal(string key, IValue val)
    {
        _globalScope.SetValue(key, val);
    }
    
    public override IValue VisitProg(ShaellParser.ProgContext context)
    {
        if (context.children.Count == 2)
            VisitProgramArgs(context.programArgs());
        VisitStmts(context.stmts(), false);
        return null;
    }

    private IValue VisitStmts(ShaellParser.StmtsContext context, bool scoper)
    {
        if (scoper)
            _scopeManager.PushScope(new ScopeContext());
        foreach (var stmt in context.stmt())
        {
            var rv = SafeVisit(stmt);
            if (_shouldReturn)
            {
                _shouldReturn = false;
                return rv;
            }
        }
        if (scoper)
            _scopeManager.PopScope();
        return null;
    }
    public override IValue VisitStmts(ShaellParser.StmtsContext context) => VisitStmts(context, true);

    public override IValue VisitStmt(ShaellParser.StmtContext context)
    {
        if (context.children.Count == 1)
        {
            var child = Visit(context.children[0]);
            if (child is SProcess proc)
            {
                var jo = proc.Execute().ToJobObject();
                return jo;
            }

            return child;
        }
        throw new Exception("No no no");
    }

    public override IValue VisitIfStmt(ShaellParser.IfStmtContext context)
    {
        var stmts = context.stmts();
        
        if (Visit(context.expr()).ToBool())
            return SafeVisit(stmts[0]);
        if (stmts.Length > 1)
            return SafeVisit(stmts[1]);

        return null;
    }

    public override IValue VisitForLoop(ShaellParser.ForLoopContext context)
    {
        SafeVisit(context.expr()[0]);
        while (SafeVisit(context.expr()[1]).ToBool())
        {
            var rv = SafeVisit(context.stmts());
            if (_shouldReturn)
                return rv;
            SafeVisit(context.expr()[2]);
        }
        return null;
    }

    public override IValue VisitWhileLoop(ShaellParser.WhileLoopContext context)
    {
        while (SafeVisit(context.expr()).ToBool())
        {
            var rv = SafeVisit(context.stmts());
            if (_shouldReturn)
                return rv;
        }
        return null;
    }

    public override IValue VisitReturnStatement(ShaellParser.ReturnStatementContext context)
    {
        _shouldReturn = true;
        //TODO: Kan returnere som reference
        return SafeVisit(context.expr());
    }

    public override IValue VisitFunctionDefinition(ShaellParser.FunctionDefinitionContext context)
    {
        var formalArgIdentifiers = new List<string>();
        foreach (var formalArg in context.innerFormalArgList().VARIDENTFIER())
        {
            formalArgIdentifiers.Add(formalArg.GetText());
        }
        
        _scopeManager.SetValue(
            context.VARIDENTFIER().GetText(),
            new UserFunc(
                _globalScope, 
                context.stmts(), 
                _scopeManager.CopyScopes(), 
                formalArgIdentifiers
                )
        );
        
        return null;
    }

    public override IValue VisitAnonFunctionDefinition(ShaellParser.AnonFunctionDefinitionContext context)
    {
        var formalArgIdentifiers = new List<string>();
        foreach (var formalArg in context.innerFormalArgList().VARIDENTFIER())
        {
            formalArgIdentifiers.Add(formalArg.GetText());
        }

        return new UserFunc(
            _globalScope,
            context.stmts(),
            _scopeManager.CopyScopes(),
            formalArgIdentifiers
        );
    }

    public override IValue VisitExpr(ShaellParser.ExprContext context)
    {
        throw new Exception("nejnejnej");
    }

    public override IValue VisitAssignExpr(ShaellParser.AssignExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        var value = lhs as RefValue;
        if (value == null)
        {
            throw new SyntaxErrorException("Syntax Error: Tried to assign to non ref");
        }

        RefValue refLhs = value;

        var rhs = SafeVisit(context.expr(1));
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        refLhs.Set(rhs);

        return refLhs.Get();
    }

    #region ARITHMETIC_EXPRESSIONS
    public override IValue VisitAddExpr(ShaellParser.AddExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        if (lhs.Unpack() is SString || rhs.Unpack() is SString)
            return lhs.ToSString() + rhs.ToSString();

        return lhs.ToNumber() + rhs.ToNumber();
    }

    public override IValue VisitMinusExpr(ShaellParser.MinusExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return lhs.ToNumber() - rhs.ToNumber();
    }

    //Visit DivExpr and evaluate both sides and return the two values divided
    public override IValue VisitDivExpr(ShaellParser.DivExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return lhs.ToNumber() / rhs.ToNumber();
    }

    public override IValue VisitMultExpr(ShaellParser.MultExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        if (lhs.Unpack() is SString)
        {
            return lhs.ToSString() * rhs.ToNumber();
        }

        if (rhs.Unpack() is SString)
        {
            return rhs.ToSString() * lhs.ToNumber();
        }
        
        return lhs.ToNumber() * rhs.ToNumber();
    }

    public override IValue VisitModExpr(ShaellParser.ModExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return lhs.ToNumber() % rhs.ToNumber();
    }

    public override IValue VisitPowExpr(ShaellParser.PowExprContext context)
    {
        var basenum = SafeVisit(context.expr(0));
        var exponent = SafeVisit(context.expr(1));

        return Number.Power(basenum.ToNumber(), exponent.ToNumber());
    }

    public override IValue VisitPlusEqExpr(ShaellParser.PlusEqExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        if (lhs is not RefValue)
        {
            throw new Exception("Tried to assign to non ref");
        }
    
        var refLhs = lhs as RefValue;
        
        var rhs = SafeVisit(context.expr(1));
        
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        if (lhs.Unpack() is SString || rhs.Unpack() is SString)
            refLhs.Set(lhs.ToSString() + rhs.ToSString());
        else
            refLhs.Set(lhs.ToNumber() + rhs.ToNumber());

        return refLhs.Get();
    }
    
    public override IValue VisitMinusEqExpr(ShaellParser.MinusEqExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        if (lhs is not RefValue)
        {
            throw new Exception("Tried to assign to non ref");
        }
    
        var refLhs = lhs as RefValue;
        
        var rhs = SafeVisit(context.expr(1));
        
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        var rhsResult = lhs.ToNumber() - rhs.ToNumber();
        
        refLhs.Set(rhsResult);

        return refLhs.Get();
    }
    
    public override IValue VisitMultEqExpr(ShaellParser.MultEqExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        if (lhs is not RefValue)
        {
            throw new Exception("Tried to assign to non ref");
        }
    
        var refLhs = lhs as RefValue;
        
        var rhs = SafeVisit(context.expr(1));
        
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        var rhsResult = lhs.ToNumber() * rhs.ToNumber();
        
        refLhs.Set(rhsResult);

        return refLhs.Get();
    }
    
    public override IValue VisitDivEqExpr(ShaellParser.DivEqExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        if (lhs is not RefValue)
        {
            throw new Exception("Tried to assign to non ref");
        }
    
        var refLhs = lhs as RefValue;
        
        var rhs = SafeVisit(context.expr(1));
        
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        var rhsResult = lhs.ToNumber() / rhs.ToNumber();
        
        refLhs.Set(rhsResult);

        return refLhs.Get();
    }
    
    public override IValue VisitModEqExpr(ShaellParser.ModEqExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        if (lhs is not RefValue)
        {
            throw new Exception("Tried to assign to non ref");
        }
    
        var refLhs = lhs as RefValue;
        
        var rhs = SafeVisit(context.expr(1));
        
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        var rhsResult = lhs.ToNumber() % rhs.ToNumber();
        
        refLhs.Set(rhsResult);

        return refLhs.Get();
    }

    public override IValue VisitPowEqExpr(ShaellParser.PowEqExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));

        if (lhs is not RefValue)
        {
            throw new Exception("Tried to assign to non ref");
        }
    
        var refLhs = lhs as RefValue;
        
        var rhs = SafeVisit(context.expr(1));
        
        if (rhs is RefValue)
        {
            rhs = (rhs as RefValue).Get();
        }
        
        var rhsResult = Number.Power(lhs.ToNumber(), rhs.ToNumber());
        
        refLhs.Set(rhsResult);

        return refLhs.Get();
    }
    #endregion

    #region LOGICAL_EXPRESSIONS
    
    //Implement less than operator
    public override IValue VisitLTExpr(ShaellParser.LTExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return new SBool(lhs.ToNumber() < rhs.ToNumber());
    }

    public override IValue VisitGTExpr(ShaellParser.GTExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return new SBool(lhs.ToNumber() > rhs.ToNumber());
    }

    public override IValue VisitLEQExpr(ShaellParser.LEQExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return new SBool(lhs.ToNumber() <= rhs.ToNumber());
    }

    public override IValue VisitGEQExpr(ShaellParser.GEQExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return new SBool(lhs.ToNumber() >= rhs.ToNumber());
    }

    public override IValue VisitEQExpr(ShaellParser.EQExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));
        if (lhs is RefValue lhsRef)
        {
            lhs = lhsRef.Unpack();
        }  
        if (rhs is RefValue rhsRef)
        {
            rhs = rhsRef.Unpack();
        }
        return new SBool(lhs.IsEqual(rhs));
    }

    public override IValue VisitNEQExpr(ShaellParser.NEQExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        return new SBool(!lhs.Equals(rhs.Unpack()));
    }

    public override IValue VisitLnotExpr(ShaellParser.LnotExprContext context)
    {
        var lhs = SafeVisit(context.expr());

        return new SBool(!lhs.ToBool());
    }
    
    #endregion

    public override IValue VisitVarIdentifier(ShaellParser.VarIdentifierContext context)
    {
        var val = _scopeManager.GetValue(context.VARIDENTFIER().GetText());
        if (val == null)
        {
            return _scopeManager.SetValue(context.VARIDENTFIER().GetText(), new SNull());
        }
        return val;
    }

    public override IValue VisitNumberExpr(ShaellParser.NumberExprContext context)
    {
        var num = context.NUMBER().GetText();

        if (num.Contains(".")) 
            return new Number(double.Parse(num, CultureInfo.InvariantCulture));

        return new Number(long.Parse(num));
    }

    public override IValue VisitFunctionCallExpr(ShaellParser.FunctionCallExprContext context)
    {
        var lhs = SafeVisit(context.expr()).ToFunction();
        
        var args = new List<IValue>();
        foreach (var expr in context.innerArgList().expr())
        {
            var val = SafeVisit(expr);
            if (val is RefValue refVal)
            {
                val = refVal.Get();
            }

            args.Add(val);
        }

        if (lhs is SProcess proc)
        {
            proc.AddArguments(args);
            return proc;
        }

        return lhs.Call(args);
    }

    public override IValue VisitPIPEExpr(ShaellParser.PIPEExprContext context)
    {
        var lhs = SafeVisit(context.expr(0)).ToSProcess();
        var rhs = SafeVisit(context.expr(1)).ToSProcess();

        rhs.LeftProcess = lhs;
        
        return rhs;
    }

    public override IValue VisitStringLiteralExpr(ShaellParser.StringLiteralExprContext context)
    {
        return new SString(context.STRINGLITERAL().GetText()[1..^1]);
    }
    
    
    //Visit PosExpr and return the value with toNumber
    public override IValue VisitPosExpr(ShaellParser.PosExprContext context)
    {
        var lhs = SafeVisit(context.expr());
        return lhs.ToNumber();
    }
    
    //Visit NegExpr and return the value with negative toNumber
    public override IValue VisitNegExpr(ShaellParser.NegExprContext context)
    {
        var lhs = SafeVisit(context.expr());
        return -lhs.ToNumber();
    }
    
    //Visit the LORExpr and return the value of the left or right side with short circuiting
    public override IValue VisitLORExpr(ShaellParser.LORExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        if (lhs.ToBool())
            return new SBool(true);
        
        var rhs = SafeVisit(context.expr(1));
        return new SBool(rhs.ToBool());
    }
    
    //Visit the IdentifierIndexExpr and use the right value to index the left as a table
    public override IValue VisitIdentifierIndexExpr(ShaellParser.IdentifierIndexExprContext context)
    {
        var lhs = SafeVisit(context.expr());
        var rhs = context.identifier().GetText(); //TODO: Views numbers as empty strings
        return lhs.ToTable().GetValue(new SString(rhs));
    }
    
    //Visit the TrueBoolean and return the value of true
    public override IValue VisitTrueBoolean(ShaellParser.TrueBooleanContext context) => new SBool(true);
    
    //Visit the FalseBoolean and return the value of false
    public override IValue VisitFalseBoolean(ShaellParser.FalseBooleanContext context) => new SBool(false);
    
    //Vist the SubScriptExpr and return the value of the left side with the right side as index
    public override IValue VisitSubScriptExpr(ShaellParser.SubScriptExprContext context)
    {
        var lhs = SafeVisit(context.expr(0));
        var rhs = SafeVisit(context.expr(1));

        if (rhs is RefValue refValue)
        {
            rhs = refValue.Get();
        }

        if (rhs is IKeyable rhsKeyable)
        {
            return lhs.ToTable().GetValue(rhsKeyable);
        }
        
        throw new Exception("Cannot index with a non-keyable value");
    }
    
    
    //Implement DerefExpr
    public override IValue VisitObjectLiteral(ShaellParser.ObjectLiteralContext context)
    {
        UserTable @out = new UserTable();
        for (int i = 0; i < context.expr().Length; i++)
        {
            IValue key = SafeVisit(context.objfields()[i]);
            RefValue value = @out.GetValue(key as IKeyable);
            value.Set(SafeVisit(context.expr()[i]));
        }

        return @out;
    }

    public override IValue VisitProgramArgs(ShaellParser.ProgramArgsContext context)
    {
        int i = 0;
        foreach (var formal in context.innerFormalArgList().VARIDENTFIER())
        {
            if (i < _args.Length)
            {
                _scopeManager.SetValue(formal.GetText(), new SString(_args[i]));
            }
            else
            {
                _scopeManager.SetValue(formal.GetText(), new SNull());
            }
            i++;
        }
        return null;
    }

    public override IValue VisitFieldExpr(ShaellParser.FieldExprContext context) => SafeVisit(context.expr());

    public override IValue VisitFieldIdentifier(ShaellParser.FieldIdentifierContext context) => new SString(context.GetText());
    public override IValue VisitDerefExpr(ShaellParser.DerefExprContext context) => new SFile(SafeVisit(context.expr()).ToSString().Val);
    public override IValue VisitFileIdentifier(ShaellParser.FileIdentifierContext context) => new SFile(context.GetText());
    
    
    public override IValue VisitNullExpr(ShaellParser.NullExprContext context) => new SNull();
    
    public override IValue VisitParenthesis(ShaellParser.ParenthesisContext context) => 
        SafeVisit(context.expr());

    public override IValue VisitLANDExpr(ShaellParser.LANDExprContext context)
    {
        var lhs = SafeVisit(context.expr(0)).ToBool();
        if (!lhs)
            return new SBool(false);
        
        var rhs = SafeVisit(context.expr(1)).ToBool();
        return new SBool(lhs && rhs);

    }
    
    public override IValue VisitBnotExpr(ShaellParser.BnotExprContext context) => 
        throw new NotImplementedException();
}