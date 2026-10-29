using System.Collections.Generic;

public class KeySlot
{
    public KeySlot(Token token)
    {
        KeyToken = token;
    }
    public void Spend()
    {
        IsSpent = true;
    }
    public void Replace(Token token)
    {
        IsSpent = false;
        KeyToken = token;
    }
    public Token KeyToken {get; private set;}
    public bool IsSpent {get; private set;} = false;
}

public class KeyHand
{
    public KeyHand(System.Random rng = null)
    {
        operatorPool = MathPool.CreateDefaultOperators(rng);
        operandPool = MathPool.CreateDefaultOperands(rng);
        operatorHand = new List<KeySlot>(operatorHandSize);
        operandHand = new List<KeySlot>(operandHandSize);
        
        for(int i = 0; i < operatorHandSize; ++i)
            operatorHand.Add(new KeySlot(operatorPool.DrawKey()));
        for(int i = 0; i < operandHandSize; ++i)
            operandHand.Add(new KeySlot(operandPool.DrawKey()));
    }
    public Token GetOperator(int slot)
    {
        return operatorHand[slot].KeyToken;
    }
    public Token GetOperand(int slot)
    {
        return operandHand[slot].KeyToken;
    }
    public void SpendOperator(int slot)
    {
        operatorHand[slot].Spend();
    }
    public void SpendOperand(int slot)
    {
        operandHand[slot].Spend();
    }
    public bool IsOperatorSpent(int slot)
    {
        return operatorHand[slot].IsSpent;
    }
    public bool IsOperandSpent(int slot)
    {
        return operandHand[slot].IsSpent;
    }    
    public void Refresh()
    {
        foreach(KeySlot key in operatorHand)
            if (key.IsSpent) key.Replace(operatorPool.DrawKey());
        foreach(KeySlot key in operandHand)
            if (key.IsSpent) key.Replace(operandPool.DrawKey());
    }
    private List<KeySlot> operatorHand;
    private List<KeySlot> operandHand;
    private MathPool operatorPool;
    private MathPool operandPool;
    private int operatorHandSize = 2;
    private int operandHandSize = 6;
}
