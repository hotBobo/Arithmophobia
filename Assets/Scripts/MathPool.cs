// List of tokens with weight assosiacted to them to fill in the keys on a calc.
using System.Collections.Generic;

class WeightedEntry{
    public readonly Token EntryToken;
    public int Weight {get; private set;} // Apperantly this essentially makes it private for setting, just public for getting
    public int IncrementWeight(int i)
    {
        int newWeight = Weight + i;
        if (newWeight < 0) i -= newWeight;
        Weight +=i;
        return i;
    }
    public WeightedEntry(Token token, int weight=1)
    {
        EntryToken = token;
        if (weight < 0) weight = 0;
        Weight = weight;
    }
}
public class MathPool
{
    public MathPool(System.Random random = null)
    {
        weightedKeys = new List<WeightedEntry>();
        rng = random ?? new System.Random();
    }
    // Add num Keys to our pool. If alr exists, then increment its weight by num
    public void AddKey(Token token, int num=1)
    {
        for (int i = 0; i < weightedKeys.Count; ++i)
        {
            if (weightedKeys[i].EntryToken == token)
            {
                totalWeight += weightedKeys[i].IncrementWeight(num);
                return;
            }
        }
        weightedKeys.Add(new WeightedEntry(token, num));
        if (num > 0) totalWeight += num;
    }
    // Draw a random Key from our List
    public Token DrawKey()
    {
        if (totalWeight == 0)
        {
            throw new System.InvalidOperationException("Cannot draw from empty MathPool");
        }
        int key = rng.Next(totalWeight);
        int i = 0;
        key -= weightedKeys[i].Weight;
        while (key >= 0)
        {
            ++i;
            key -= weightedKeys[i].Weight;
        }
        return weightedKeys[i].EntryToken;
    }
    public int GetWeight(Token token)
    {
        for(int i = 0; i < weightedKeys.Count; ++i)
        {
            if (weightedKeys[i].EntryToken == token)
            {
                return weightedKeys[i].Weight;
            }
        }
        return 0;
    }
    public static MathPool CreateDefaultOperands(System.Random rand = null)
    {
        MathPool temp = new MathPool(rand);
        for(int i = 0; i < 10; ++i)
            temp.AddKey(Token.CreateNumber(i));
        return temp;
    }
    public static MathPool CreateDefaultOperators(System.Random rand = null)
    {
        MathPool temp = new MathPool(rand);
        temp.AddKey(Token.CreateOperator("+"), 2);
        temp.AddKey(Token.CreateOperator("-"), 2);
        temp.AddKey(Token.CreateOperator("*"));
        temp.AddKey(Token.CreateOperator("/"));
        return temp;
    }
    private List<WeightedEntry> weightedKeys;
    private int totalWeight = 0;
    System.Random rng;
}