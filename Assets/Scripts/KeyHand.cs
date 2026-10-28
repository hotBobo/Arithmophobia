using System.Collections.Generic;

public class KeyHand
{
    public KeyHand()
    {
        operatorPool = new MathPool();
        operandPool = new MathPool();
        initHand();
    }
    public KeyHand(System.Random rng)
    {
        operatorPool = new MathPool(rng);
        operandPool = new MathPool(rng);
        initHand();
    }

    public Token Getoperator(int slot)
    {
        return operatorHand[slot];
    }
    public Token Getoperand(int slot)
    {
        return operandHand[slot];
    }

    // Only used in Constructor. Assumes Pools were set up.
    private void initHand()
    {
        operatorHand = new List<Token>(operatorHandSize);
        operandHand = new List<Token>(operandHandSize);
        
        for(int i = 0; i < operatorHandSize; ++i)
            operatorHand[i] = operatorPool.DrawKey();
        for(int i = 0; i < operandHandSize; ++i)
            operandHand[i] = operandPool.DrawKey();
    }
    private List<Token> operatorHand;
    private List<Token> operandHand;
    private MathPool operatorPool;
    private MathPool operandPool;
    private int operatorHandSize = 2;
    private int operandHandSize = 6;
}
