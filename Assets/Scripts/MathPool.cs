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
    public MathPool()
    {
        weightedKeys = new List<WeightedEntry>();
    }
    public MathPool(System.Random random)
    {
        weightedKeys = new List<WeightedEntry>();
        rng = random;
    }
    // Add num Keys to our pool. If alr exists, then increment its weight by num
    public void AddKey(Token token, int num=1)
    {
        for (int i = 0; i < weightedKeys.Count; ++i)
        {
            if (weightedKeys[i].EntryToken.Symbol == token.Symbol)
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
            if (weightedKeys[i].EntryToken.Symbol == token.Symbol)
            {
                return weightedKeys[i].Weight;
            }
        }
        return 0;
    }
    private List<WeightedEntry> weightedKeys;
    private int totalWeight = 0;
    System.Random rng = new System.Random();
}