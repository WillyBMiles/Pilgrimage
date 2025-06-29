using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[BoxGroup]
public abstract class Effect 
{
    /*Possible effects: 
     * For each adjacent tent (of a particular type)
     *      gain X resource
     * Do something to each adjacent tent
     * Raw gain resources
     */

    public abstract ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary);

    public virtual ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        return GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
    }
}

public class GainResources : Effect
{
    public ResourceChange resourceChange = new();
    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        return resourceChange;
    }
}

public class MultiEffect : Effect
{
    public List<Effect> effects = new();

    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        ResourceChange rc = new();
        foreach (Effect e in effects)
        {
            rc += e.GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
        }

        return rc;
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        ResourceChange rc = new();
        foreach (Effect e in effects)
        {
            rc += e.ApplyEffect(myTent, origin, allTents, adjacencyDictionary);
        }

        return rc;
    }
}

public class ForEachAdjacent : Effect
{
    public Effect effect;

    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        ResourceChange rc = new();
        foreach (Tent t in adjacencyDictionary[myTent])
        {
            rc += effect.GetResourceChange(t, origin, allTents, adjacencyDictionary);
        }
        return rc;
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        ResourceChange rc = new();
        foreach (Tent t in adjacencyDictionary[myTent])
        {
            rc += effect.ApplyEffect(t, origin, allTents, adjacencyDictionary);
        }
        return rc;
    }
}

public class Destroy : Effect
{
    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (myTent.isFirstTent)
            return new();
        return new() { changes = new() { { ResourceManager.Resource.Tents, -1 } } };
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (myTent.isFirstTent)
            return new();
        GameController.Instance.currentTents.Remove(GameController.Instance.currentTents.First(t => t.Name == myTent.Name));
        return new();
    }
}

public class Duplicate :Effect
{
    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        return myTent.effect.GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        return myTent.effect.ApplyEffect(myTent, origin, allTents, adjacencyDictionary);
    }
}

public class IfGod : Effect {
    public Effect effect;
    public Tent.God god;

    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (myTent.god != god)
            return new();
        return effect.GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (myTent.god != god)
            return new();
        return effect.ApplyEffect(myTent, origin, allTents, adjacencyDictionary);
    }
}

public class IfNotChief : Effect
{
    public Effect effect;

    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (myTent.isFirstTent)
            return new();
        return effect.GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (myTent.isFirstTent)
            return new();
        return effect.ApplyEffect(myTent, origin, allTents, adjacencyDictionary);
    }
}

public class IfAdjacentMatchingGod : Effect
{
    public Effect effect;
    public Tent.God god;
    public int min = 1;

    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (!adjacencyDictionary.ContainsKey(myTent))
            return new();

        int c = adjacencyDictionary[myTent].Where(t => t.god == god).Count();

        if (c < min)
            return new();
        return effect.GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (!adjacencyDictionary.ContainsKey(myTent))
            return new();

        int c = adjacencyDictionary[myTent].Where(t => t.god == god).Count();

        if (c < min)
            return new();
        return effect.ApplyEffect(myTent, origin, allTents, adjacencyDictionary);
    }
}

public class AddAdjacents : Effect {
    public int multiplier = 1;
    
    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        if (!adjacencyDictionary.ContainsKey(myTent))
            return new();

        for (int i =1; i < multiplier; i++)
        {
            adjacencyDictionary[myTent] = adjacencyDictionary[myTent].Concat(adjacencyDictionary[myTent]);
        }
        
        return new();
    }
}

public class Count : Effect
{
    public Effect effect;

    public Number number1;
    public Comparison comparison;
    public Number number2;

    public enum Comparison
    {
        EqualTo,
        LessThan
    }

    public override ResourceChange GetResourceChange(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        bool shouldContinue = false;
        int n1 = number1.Get(myTent, origin, allTents, adjacencyDictionary);
        int n2 = number2.Get(myTent, origin, allTents, adjacencyDictionary);
        switch (comparison)
        {
            case Comparison.EqualTo:
                shouldContinue = n1 == n2;
                break;
            case Comparison.LessThan:
                shouldContinue = n1 < n2;
                break;
        }
        if (!shouldContinue)
        {
            return new();
        }

        return effect.GetResourceChange(myTent, origin, allTents, adjacencyDictionary);
    }

    public override ResourceChange ApplyEffect(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        bool shouldContinue = false;
        int n1 = number1.Get(myTent, origin, allTents, adjacencyDictionary);
        int n2 = number2.Get(myTent, origin, allTents, adjacencyDictionary);
        switch (comparison)
        {
            case Comparison.EqualTo:
                shouldContinue = n1 == n2;
                break;
            case Comparison.LessThan:
                shouldContinue = n1 < n2;
                break;
        }
        if (!shouldContinue)
        {
            return new();
        }

        return effect.ApplyEffect(myTent, origin, allTents, adjacencyDictionary);
    }
}

public abstract class Number
{
    public abstract int Get(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary);
}

public class SimpleNumber : Number
{
    public int Number;
    public override int Get(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        return Number;
    }
}

public class Adjacent : Number
{
    public override int Get(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {
        return adjacencyDictionary.ContainsKey(myTent) ? adjacencyDictionary[myTent].Count() : 0;
    }
}

public class AdjacentUniqueGods : Number
{
    public override int Get(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {

        return adjacencyDictionary.ContainsKey(myTent) ? adjacencyDictionary[myTent].Select(t => t.god).Distinct().Count() : 0;
    }
}

public class AdjacentCountGods : Number
{
    public Tent.God god;
    public override int Get(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {

        return adjacencyDictionary.ContainsKey(myTent) ? adjacencyDictionary[myTent].Where(t => t.god == god).Count() : 0;
    }
}

public class AdjacentChiefs : Number
{
    public override int Get(Tent myTent, Tent origin, IEnumerable<Tent> allTents, Dictionary<Tent, IEnumerable<Tent>> adjacencyDictionary)
    {

        return adjacencyDictionary.ContainsKey(myTent) ? adjacencyDictionary[myTent].Where(t => t.isFirstTent).Count() : 0;
    }
}