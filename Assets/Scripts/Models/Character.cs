using UnityEngine;
using System.Collections;
using System;

// Type of character that may hold initiative
public enum CharacterType { PC };

public class Character {

    // We need to know the context in which we exist. Probably. Maybe.
    public Encounter encounter { get; protected set; }

    // This is the internal CharacterType of the instantiated Character
    public CharacterType type { get; protected set; }
    public string CharacterName { get; protected set; }

    // Internal Initiative of the character
    private int _initiative = 0;

    // The function we callback any time our tile's data changes
    // Action<Character> cbInitiativeChanged;

    // This is the public access of Initiative that initiates a callback
    public int initiative
    {
        get { return _initiative; }
        set
        {
            //int oldType = _initiative;
            _initiative = value;

            //Call the callback and let things know we've changed.
            //if(cbInitiativeChanged !=null && oldType != _initiative)
            //{
            //    cbInitiativeChanged(this);
            //}
        }
    }

    public Character(Encounter encounter, CharacterType type, string CharacterName)
    {
        this.encounter = encounter;
        this.type = type;
        this.CharacterName = CharacterName;
    }
}