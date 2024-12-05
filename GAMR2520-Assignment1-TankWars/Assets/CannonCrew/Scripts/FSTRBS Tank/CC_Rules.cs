using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules
{
    public void addRule(Rule rule)
    {
        GetRules.Add(rule);
    }

    public List<Rule> GetRules { get; } = new List<Rule>();
}
