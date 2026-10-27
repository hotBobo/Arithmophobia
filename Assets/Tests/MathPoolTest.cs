using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

public class MathPoolTest
{
    System.Random rand = new System.Random(67);

    [Test]
    public void OneEntryReturned()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2));
        
        Token result = mathPool.DrawKey();

        Assert.AreEqual(result.Symbol, Token.CreateNumber(2).Symbol);
    }

    [Test]
    public void WeightZeroNotReturned()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(4));
        mathPool.AddKey(Token.CreateNumber(4));
        mathPool.AddKey(Token.CreateNumber(3));
        mathPool.AddKey(Token.CreateNumber(3), -1);
        
        Token result = mathPool.DrawKey();

        Assert.AreNotEqual(result.Symbol, Token.CreateNumber(3).Symbol);
        Assert.AreEqual(0, mathPool.GetWeight(Token.CreateNumber(3)));
    }

    [Test]
    public void WeightIsRaised()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(4));
        mathPool.AddKey(Token.CreateNumber(4));
        

        Assert.AreEqual(2, mathPool.GetWeight(Token.CreateNumber(4)));
        Assert.AreEqual(1, mathPool.GetWeight(Token.CreateNumber(2)));
    }

    [Test]
    public void WeightsAreWeightedCorrectly()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(3));

        int count = 0;
        for(int i = 0; i < 10000; ++i)
        {
            Token result = mathPool.DrawKey();
            if (result.Symbol == Token.CreateNumber(2).Symbol) ++count;
        }

        Assert.That(count, Is.InRange(8700, 9300));
    }

    [Test]
    public void EmptyPoolThrowsException()
    {
        var mathPool = new MathPool(rand);

        Assert.Throws<System.InvalidOperationException>(() => mathPool.DrawKey());
    }
};