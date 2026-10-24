using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CalcTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void Addition_ReturnsSum()
    {
        var tokens = new List<Token>
        {
            Token.CreateNumber(2),
            Token.CreateOperator("+"),
            Token.CreateNumber(3)
        };

        EvaluationResult result = EquationEvaluator.EvaluateTokens(tokens, currentAns: 0);

        Assert.AreEqual(EvaluationStatus.Success, result.Status);
        Assert.AreEqual(5, result.Value);
    }

    [Test]
    public void Subtraction_ReturnsDifference()
    {
        var tokens = new List<Token>
        {
            Token.CreateNumber(5),
            Token.CreateOperator("-"),
            Token.CreateNumber(3)
        };

        EvaluationResult result = EquationEvaluator.EvaluateTokens(tokens, currentAns: 0);

        Assert.AreEqual(EvaluationStatus.Success, result.Status);
        Assert.AreEqual(2, result.Value);
    }
    
    [Test]
    public void Division_ByZero_ReturnsError()
    {
        var tokens = new List<Token>
        {
            Token.CreateNumber(5),
            Token.CreateOperator("/"),
            Token.CreateNumber(0)
        };

        EvaluationResult result = EquationEvaluator.EvaluateTokens(tokens, currentAns: 0);

        Assert.AreEqual(EvaluationStatus.DivideByZero, result.Status);
    }

    [Test]
    public void InvalidExpression_ReturnsSyntaxError()
    {
        var tokens = new List<Token>
        {
            Token.CreateNumber(5),
            Token.CreateOperator("+"),
            Token.CreateOperator("*"), // Invalid sequence
            Token.CreateNumber(3)
        };

        EvaluationResult result = EquationEvaluator.EvaluateTokens(tokens, currentAns: 0);

        Assert.AreEqual(EvaluationStatus.SyntaxError, result.Status);
    }

    [Test]
    public void EquationWithAns_ReturnsCorrectValue()
    {
        var tokens = new List<Token>
        {
            Token.CreateAns(), // Assume previous answer was 5
            Token.CreateOperator("+"),
            Token.CreateNumber(10)
        };

        double currentAns = 5; // Assume previous answer was 5
        EvaluationResult result = EquationEvaluator.EvaluateTokens(tokens, currentAns);

        Assert.AreEqual(EvaluationStatus.Success, result.Status);
        Assert.AreEqual(15, result.Value);
    }

    [Test]
    public void ComplexExpression_ReturnsCorrectValue()
    {
        var tokens = new List<Token>
        {
            Token.CreateNumber(2),
            Token.CreateOperator("+"),
            Token.CreateNumber(3),
            Token.CreateOperator("*"),
            Token.CreateLeftParen(),
            Token.CreateNumber(4),
            Token.CreateOperator("-"),
            Token.CreateNumber(1),
            Token.CreateRightParen()
        };

        EvaluationResult result = EquationEvaluator.EvaluateTokens(tokens, currentAns: 0);

        Assert.AreEqual(EvaluationStatus.Success, result.Status);
        Assert.AreEqual(11, result.Value); // 2 + 3 * (4 - 1) = 2 + 3 * 3 = 2 + 9 = 11
    }


    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator CalcTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
