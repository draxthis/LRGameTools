using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Encounter {

    // This dictonary will keep the character name look up to character class
    // for cross-reference purposes.
    public Dictionary<string, Character> NameCharacter { get; protected set; }

    public int rounds { get; set; }

    public Encounter (int rounds)
    {
        // Upon creation set the round counter to the round number
        // to start at.  Initially, this will default to zero in the
        // Game controller but in the future, this can be used to start
        // tracking at some other point in the combat.
        this.rounds = rounds;
        NameCharacter = new Dictionary<string, Character>();
    }

    public void addCharacter(String cn, Character c)
    {
        cn = cn.Trim();
        if (cn.Length != 0)
        {
            NameCharacter.Add(cn, c);
        }
    }

    public Character checkForCharacter (string cn)
    {
        if (NameCharacter.ContainsKey(cn) == true)
        {
            return NameCharacter[cn];
        }
        else
        {
            return null;
        }
    }
}