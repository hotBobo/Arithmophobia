using System.Collections.Generic;

public enum TokenType
{
    Number,
    Operator,
    Ans,
    LeftParenthesis,
    RightParenthesis
}

public struct Token : System.IEquatable<Token>
{
    // Type of token (Num, Op, Ans, etc)
    public readonly TokenType Type;
    // Value of token (val for num, else 0)
    public readonly double Value;
    // Symbol that will be shown from token (unique for all diff tokens)
    public readonly string Symbol;

    public bool Equals(Token other)
    {
        return Type == other.Type && Symbol == other.Symbol;
        
    }
    public override bool Equals(object obj)
    {
        return obj is Token other && Equals(other);
    }
    public override int GetHashCode()
    {
        return System.HashCode.Combine(Type,Symbol);
    }
    public static bool operator ==(Token a, Token b)
    {
        return a.Equals(b);
    }
    public static bool operator !=(Token a, Token b)
    {
        return !(a==b);
    }

    public Token(TokenType type, double value=0, string symbol="")
    {
        Type = type;
        Value = value;
        Symbol = symbol;
    }

    public static Token CreateNumber(double val) =>
        new Token(TokenType.Number, val, val.ToString());

    public static Token CreateOperator(string opSymbol) =>
        new Token(TokenType.Operator, 0, opSymbol);

    public static Token CreateAns() =>
        new Token(TokenType.Ans); // Must fix to make it declared here

    public static Token CreateLeftParen() =>
        new Token(TokenType.LeftParenthesis, 0, "(");
    
    public static Token CreateRightParen() =>
        new Token(TokenType.RightParenthesis, 0, ")");
}


public enum EvaluationStatus
{
    Success,
    SyntaxError,
    DivideByZero
}

public struct EvaluationResult
{
    public EvaluationStatus Status;
    public double Value;
    public string ErrorMessage;

    public static EvaluationResult SuccessResult(double val) =>
        new EvaluationResult { Status = EvaluationStatus.Success, Value = val };

    public static EvaluationResult Error(EvaluationStatus status, string message) =>
        new EvaluationResult { Status = status, Value = 0, ErrorMessage = message };
}

public class EquationEvaluator
{
    // Core evaluation method
    public static EvaluationResult EvaluateTokens(List<Token> tokens, double currentAns)
    {
        if (tokens == null || tokens.Count == 0)
            return EvaluationResult.Error(EvaluationStatus.SyntaxError, "Empty Equation");

        // Convert Token List -> RPN Queue directly
        List<Token> rpnTokens;
        try
        {
            rpnTokens = ConvertToRPN(tokens);
        }
        catch
        {
            return EvaluationResult.Error(EvaluationStatus.SyntaxError, "Mismatched Parentheses or Invalid Syntax");
        }

        // Compute final result
        return CalculateRPN(rpnTokens, currentAns);
    }

    private static List<Token> ConvertToRPN(List<Token> tokens)
    {
        List<Token> output = new List<Token>();
        Stack<Token> opStack = new Stack<Token>();

        foreach (Token token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                case TokenType.Ans:
                    output.Add(token);
                    break;

                case TokenType.LeftParenthesis:
                    opStack.Push(token);
                    break;

                case TokenType.RightParenthesis:
                    while (opStack.Count > 0 && opStack.Peek().Type != TokenType.LeftParenthesis)
                    {
                        output.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0) throw new System.Exception("Mismatched parentheses");
                    opStack.Pop(); // Remove '('
                    break;

                case TokenType.Operator:
                    while (opStack.Count > 0 && opStack.Peek().Type == TokenType.Operator)
                    {
                        Token top = opStack.Peek();
                        int tokenPrec = GetPrecedence(token.Symbol);
                        int topPrec = GetPrecedence(top.Symbol);

                        if ((!IsRightAssociative(token.Symbol) && tokenPrec <= topPrec) ||
                            (IsRightAssociative(token.Symbol) && tokenPrec < topPrec))
                        {
                            output.Add(opStack.Pop());
                        }
                        else
                        {
                            break;
                        }
                    }
                    opStack.Push(token);
                    break;
            }
        }

        while (opStack.Count > 0)
        {
            Token top = opStack.Pop();
            if (top.Type == TokenType.LeftParenthesis) throw new System.Exception("Mismatched parentheses");
            output.Add(top);
        }

        return output;
    }

    // Calculate based on RPN
    private static EvaluationResult CalculateRPN(List<Token> rpnTokens, double currentAns)
    {
        Stack<double> stack = new Stack<double>();

        foreach (Token token in rpnTokens)
        {
            if (token.Type == TokenType.Number)
            {
                stack.Push(token.Value);
            }
            else if (token.Type == TokenType.Ans)
            {
                stack.Push(currentAns);
            }
            else if (token.Type == TokenType.Operator)
            {
                if (stack.Count < 2)
                    return EvaluationResult.Error(EvaluationStatus.SyntaxError, "Missing Operands");

                double b = stack.Pop();
                double a = stack.Pop();

                switch (token.Symbol)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/":
                        if (System.Math.Abs(b) < 0.0000001)
                            return EvaluationResult.Error(EvaluationStatus.DivideByZero, "Cannot divide by zero");
                        stack.Push(a / b);
                        break;
                    case "^": stack.Push(System.Math.Pow(a, b)); break;
                }
            }
        }

        if (stack.Count != 1)
            return EvaluationResult.Error(EvaluationStatus.SyntaxError, "Invalid Equation");

        return EvaluationResult.SuccessResult(stack.Pop());
    }

    // Returns precedence of operator, higher happens first
    private static int GetPrecedence(string op)
    {
        switch (op)
        {
            case "+":
            case "-":
                return 1;

            case "*":
            case "/":
                return 2;

            case "^":
                return 3;

            default:
                // Error
                return 0;

        }
    }

    // Returns true if Right Associative, false otherwise
    private static bool IsRightAssociative(string op)
    {
        switch (op)
        {
            case "^":
                return true;
            case "+":
            case "-":
            case "*":
            case "/":
                return false;
            default:
                // Error
                return false;
        }
    }

}


