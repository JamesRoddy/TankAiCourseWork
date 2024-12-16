using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Rule
{
    public string antecentA;
    public string antecentB;
    public Type consequentState;
    public BaseST debugType;
    public Predicate compare;

    public enum Predicate
    {
        And, Or, nAnd
    }

    public Rule(string antecentA, string antecentB, Type consequentState,BaseST debugType, Predicate compare)
    {
        this.antecentA = antecentA;
        this.antecentB = antecentB;
        this.consequentState = consequentState;
        this.compare = compare;
        this.debugType = debugType;
    }

    public Type CheckRule(Dictionary<string, bool> stats)
    {
        
     
        bool antecentABool = stats[antecentA];
        bool antecentBBool = stats[antecentB];

        switch (compare)
        {
            case Predicate.And:
                if (antecentABool && antecentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            case Predicate.Or:
                if (antecentABool || antecentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            case Predicate.nAnd:
                if (!antecentABool && !antecentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            default:
                return null;
        }

    }
}
