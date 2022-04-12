using System;

namespace ShaellLang
{
    public class PrettyVisitor : ShaellParserBaseVisitor<object>
    {
        private int indentLevel = 0;
        private string indent => new string('\t', indentLevel);
        public override object VisitProg(ShaellParser.ProgContext context)
        {
            VisitStmts(context.stmts());
            return null;
        }

        public override object VisitStmts(ShaellParser.StmtsContext context)
        {
            foreach (var stmt in context.stmt())
            {
                VisitStmt(stmt);
            }
            return null;
        }

        public override object VisitStmt(ShaellParser.StmtContext context)
        {
            foreach (var child in context.children)
            {
                Console.Write(indent);
                Visit(child);
                Console.WriteLine();
            }   
            return null;
        }

        public override object VisitIfStmt(ShaellParser.IfStmtContext context)
        {
            Console.Write(indent + "if ");
            Visit(context.expr());
            Console.WriteLine(" then");
            indentLevel += 1;
            Visit(context.stmts(0));
            indentLevel -= 1;
            Console.WriteLine(indent + "end");
            return null;
        }

        public override object VisitStringLiteralExpr(ShaellParser.StringLiteralExprContext literal)
        {
            //Console.Write($"{literal.STRINGLITERAL()}");
            return null;
        }

        public override object VisitNumberExpr(ShaellParser.NumberExprContext number)
        {
            Console.Write(number.NUMBER());
            return null;
        }

        public override object VisitBooleanExpr(ShaellParser.BooleanExprContext booleanexpr)
        {
            Visit(booleanexpr.boolean());
            return null;
        }
        
        public override object VisitParenthesis(ShaellParser.ParenthesisContext parenthesisContext)
        {
            Console.Write("(");
            Visit(parenthesisContext.expr());
            Console.Write(")");
            return null;
        }

        public override object VisitDerefExpr(ShaellParser.DerefExprContext derefExpr)
        {
            Console.Write("(@");
            Visit(derefExpr.expr());
            Console.Write(")");
            return null;
        }

        public override object VisitLnotExpr(ShaellParser.LnotExprContext lnotExpr)
        {
            Console.Write("(!");
            Visit(lnotExpr.expr());
            Console.Write(")");
            return null;
        }

        public override object VisitNegExpr(ShaellParser.NegExprContext negExprContext)
        {
            Console.Write("(-");
            Visit(negExprContext.expr());
            Console.Write(")");
            return null;
        }

        public override object VisitPosExpr(ShaellParser.PosExprContext posExprContext)
        {
            Console.Write("(+");
            Visit(posExprContext.expr());
            Console.Write(")");
            return null;
        }

        public override object VisitIdentifierIndexExpr(ShaellParser.IdentifierIndexExprContext identifierIndexExpr)
        {
            Console.Write("(");
            Visit(identifierIndexExpr.expr());
            Console.Write(":");
            Visit(identifierIndexExpr.IDENTIFIER());
            Console.Write(")");
            return null;
        }

        public override object VisitSubScriptExpr(ShaellParser.SubScriptExprContext subScriptExprContext)
        {
            Visit(subScriptExprContext.expr(0));
            Console.Write("[");
            Visit(subScriptExprContext.expr((1)));
            Console.Write("]");
            return null;
        }

        public override object VisitFunctionCallExpr(ShaellParser.FunctionCallExprContext functionCallExprContext)
        {
            Console.Write("(");
            Visit(functionCallExprContext.expr());
            Console.Write(")");
            Console.Write("(");
            bool first = true;
            foreach (var arg in functionCallExprContext.innerArgList().expr())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.Write(", ");
                }
                Visit(arg);
            }
            Console.Write(")");
            return null;
        }

        public override object VisitAddExpr(ShaellParser.AddExprContext addExprContext)
        {
            Console.Write("(");
            Visit(addExprContext.expr(0));
            Console.Write(" + ");
            Visit(addExprContext.expr(1));
            Console.Write(")");
            return null;
        }

        public override object VisitLTExpr(ShaellParser.LTExprContext ltExprContext)
        {
            Console.Write("(");
            Visit(ltExprContext.expr(0));
            Console.Write(" < ");
            Visit(ltExprContext.expr(1));
            Console.Write(")");
            return null;
        }

        public override object VisitEQExpr(ShaellParser.EQExprContext context)
        {
            Console.Write("(");
            Visit(context.expr(0));
            Console.Write(" == ");
            Visit(context.expr(1));
            Console.Write(")");
            return null;
        }

        public override object VisitLORExpr(ShaellParser.LORExprContext context)
        {
            Console.Write("(");
            Visit(context.expr(0));
            Console.Write(" || ");
            Visit(context.expr(1));
            Console.Write(")");
            return null;
        }

        public override object VisitFunctionDefinition(ShaellParser.FunctionDefinitionContext context)
        {
            Console.Write(indent + "fn ");
            Console.Write(context.IDENTIFIER());
            Console.Write("(");
            bool first = true;
            foreach (var arg in context.innerFormalArgList().IDENTIFIER())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.Write(", ");
                }
                Visit(arg);
            }
            Console.WriteLine(")");
            indentLevel += 1;
            Visit(context.stmts());
            indentLevel -= 1;
            Console.WriteLine(indent + "end");
            return null;
        }

        public override object VisitPIPEExpr(ShaellParser.PIPEExprContext context)
        {
            Console.Write("(");
            Visit(context.expr(0));
            Console.Write("->");
            Visit(context.expr(1));
            Console.Write(")");
            return 0;
        }

        public override object VisitAssignExpr(ShaellParser.AssignExprContext context)
        {
            Console.Write("(");
            Visit(context.expr(0));
            Console.Write(" = ");
            Visit(context.expr(1));
            Console.Write(")");
            return null;
        }
    }
}