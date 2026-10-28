using NUnit.Framework;

public class MathPoolTest
{
    System.Random rand;

    [SetUp]
    public void Init()
    {
        rand = new System.Random(67);
    }

    [Test]
    public void OneEntryReturned()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2));
        
        Token result = mathPool.DrawKey();

        Assert.AreEqual(Token.CreateNumber(2).Symbol, result.Symbol);
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
        
        for(int i = 0; i < 100; ++i)
        {
            Token result = mathPool.DrawKey();
            Assert.AreNotEqual(Token.CreateNumber(3).Symbol, result.Symbol); 
        }

        Assert.AreEqual(0, mathPool.GetWeight(Token.CreateNumber(3)));
    }

    [Test]
    public void WeightIsRaised()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2));
        mathPool.AddKey(Token.CreateNumber(4));
        mathPool.AddKey(Token.CreateNumber(4));
        mathPool.AddKey(Token.CreateNumber(6), 7);
        
        Assert.AreEqual(1, mathPool.GetWeight(Token.CreateNumber(2)));
        Assert.AreEqual(2, mathPool.GetWeight(Token.CreateNumber(4)));
        Assert.AreEqual(7, mathPool.GetWeight(Token.CreateNumber(6)));
        
    }

    [Test]
    public void WeightsAreWeightedCorrectly()
    {
        var mathPool = new MathPool(rand);
        mathPool.AddKey(Token.CreateNumber(2), 9);
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

        mathPool.AddKey(Token.CreateNumber(3), 0);
        
        Assert.Throws<System.InvalidOperationException>(() => mathPool.DrawKey());
    }
};