using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour {

    public InputField ifRound;

    // We will need a reference to the encounter
    Encounter encounter;
    Canvas canvas;
    Dictionary<string, GameObject> InitiativeSlot;
    Dictionary<GameObject, Character> goCharacterSlot;

    private int currHighlight = 0;

    
	// Use this for initialization
	void Start () {

        // Instantiate the encounter
        encounter = new Encounter(0);
        canvas = FindObjectOfType<Canvas>();
        InitiativeSlot = new Dictionary<string, GameObject>();
        goCharacterSlot = new Dictionary<GameObject, Character> ();

        if(canvas == null)
        {
            Debug.LogError("What happened to the canvas!!!!");
        }
    }

    // Update is called once per frame
    void Update () {
	
	}

    public void addInventorySlot()
    {
        // FIXME: this is temporary as I will add a scroll view in a later release
        // however, for now, the screen is sized for only seven initiative slots.
        // Also, add a vertical layout group and vertical resizer component to make it
        // easy to resize regardless of the number of characters.
        if (InitiativeSlot.Count == 7)
        {
            return;
        }

        GameObject go_c = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/grpCharacterInitiative"));
        go_c.transform.SetParent(canvas.transform, false);
        if (InitiativeSlot == null)
        {
            go_c.name = "InitiativeSlot_1";
        }
        else
        {
            go_c.name = "InitiativeSlot_" + (InitiativeSlot.Count + 1);
        }
        InputField[] ifCharInit = go_c.GetComponentsInChildren<InputField>();

        if (ifCharInit.Length == 0)
        {
            Debug.LogError("There should absolutely be input field children in here");
        }
        else
        {
            for (int x = 0; x < ifCharInit.Length; x++)
            {
                InputField i = ifCharInit[x];

                if (ifCharInit[x].name == "Character")
                {
                    ifCharInit[x].onEndEdit.AddListener(delegate { characterChanged(go_c, i); });
                }
                else
                {
                    ifCharInit[x].onEndEdit.AddListener(delegate { initiativeChanged(go_c, i); });
                }
            }
        }

        InitiativeSlot.Add(go_c.name, go_c);

        go_c.transform.localPosition = new Vector3(0, 130 - ((InitiativeSlot.Count - 1) * 50), 0);
    }        

    public void characterChanged (GameObject go, InputField i)
    {
        // Check to see if the character exists already.
        Character c;
        if (encounter.checkForCharacter(i.text) == null)
        {
            c = new Character(encounter, CharacterType.PC, i.text);
            encounter.addCharacter(c.CharacterName, c);
        }
        else
        {
            c = encounter.checkForCharacter(i.text);
        }

        if(goCharacterSlot.ContainsValue(c) == true)
        {
            GameObject old_go = goCharacterSlot.FirstOrDefault(x => x.Value == c).Key;
            if (old_go != go)
            {
                goCharacterSlot[old_go] = null;

                InputField[] ifCharInit = old_go.GetComponentsInChildren<InputField>();

                if (ifCharInit.Length == 0)
                {
                    Debug.LogError("There should absolutely be input field children in here");
                }
                else
                {
                    for (int x = 0; x < ifCharInit.Length; x++)
                    {
                        ifCharInit[0].text = null;
                    }
                }
            }
        }

        // If this is the first time we are seeing any data added to this GameObject
        // then add the GameObject and Character to our dictioinary.  Else update
        // the existing value in the dictionary with the character.
        if (i.name == "Character" && goCharacterSlot.ContainsKey(go) == false)
        {
            goCharacterSlot.Add(go, c);
        }
        else
        {
            goCharacterSlot[go] = c;
        }
    }

    public void initiativeChanged(GameObject go, InputField i)
    {

        int initiative;

        // TODO: I have some debug code to put in here in case the text is blank or not a number
        if (i.text != null && int.TryParse(i.text, out initiative))
        {
            Character c = goCharacterSlot[go];
            Debug.Log(c.initiative);
            c.initiative = initiative;
            Debug.Log(c.initiative);
        }
    }

    // This function is the button click on next initiative
    public void nextInitiative()
    {
        int currRound;

        if (ifRound.text != null && int.TryParse(ifRound.text, out currRound))
        {
            if(currRound == 0)
            {
                if (checkForInitiative() == true)
                {
                    removeUnusedPrefab();
                    updateRoundCounter(currRound);
                    sortByInitiative();
                    highlightNext();
                }
                else
                {
                    Debug.Log("At least one character is missing an initiative score and this cannot be sorted.");
                }
            }
            else if (currHighlight == InitiativeSlot.Count)
            {
                removeUnusedPrefab();
                updateRoundCounter(currRound);
                sortByInitiative();
                highlightNext();
            }
            else
            {
                highlightNext();
            }
        }
        else
        {
            Debug.LogError("updateRoundCounter: " + ifRound.name + " is NULL or does not contain an integer");
        }
    }

    private void updateRoundCounter (int currRound)
    {
        encounter.rounds += 1;
        currRound += 1;
        ifRound.text = currRound.ToString();
    }

    private void sortByInitiative ()
    {
        SortedDictionary<int, Character> sort = new SortedDictionary<int, Character>();
        IEnumerable<KeyValuePair<int, Character >> result;
        int counter;

        foreach (KeyValuePair<string, GameObject> p in InitiativeSlot)
        {
            Character c = goCharacterSlot[p.Value];
            sort.Add(c.initiative, c);
        }

        result = sort.OrderByDescending(i => i.Key);

        counter = 1;
        foreach (KeyValuePair<int, Character> kvp in result)
        {
            GameObject go = InitiativeSlot["InitiativeSlot_" + counter];
            updateGoCharSlot(go, kvp.Value);
            counter += 1;
        }
    }

    private void updateGoCharSlot (GameObject go, Character c)
    {
        InputField[] ifNew;

        ifNew = go.GetComponentsInChildren<InputField>();
        if (ifNew.Length == 0)
        {
            Debug.LogError("There should absolutely be input field children in here");
        }
        else
        {
            for (int x = 0; x < ifNew.Length; x++)
            {
                if (ifNew[x].name == "Character")
                {
                    ifNew[0].text = c.CharacterName;
                }
                else
                {
                    ifNew[1].text = c.initiative.ToString();
                }
            }
        }
        goCharacterSlot[go] = c;
    }

    private void removeUnusedPrefab ()
    {
        int numCs = encounter.NameCharacter.Count;
        Debug.Log(numCs.ToString());

        for (int x = numCs + 1; x <= InitiativeSlot.Count; x++)
        {
            GameObject go = InitiativeSlot["InitiativeSlot_" + x];
            InitiativeSlot.Remove("InitiativeSlot_" + x);
            goCharacterSlot.Remove(go);
            Destroy(go);
            //Debug.Log("I have unused slots");
        }
    }

    private bool checkForInitiative()
    {
        // Dictionary<GameObject, Character> goCharacterSlot
        foreach (KeyValuePair<GameObject, Character> kvp in goCharacterSlot)
        {
            if(kvp.Value.initiative == 0)
            {
                return false;
            }
        }
        return true;
    }

    private void highlightNext ()
    {
        GameObject old_go;
        GameObject go;
        
        if(currHighlight == 0)
        {
            currHighlight = 1;
            old_go = InitiativeSlot["InitiativeSlot_" + currHighlight];
        }
        else if (currHighlight == InitiativeSlot.Count)
        {
            old_go = InitiativeSlot["InitiativeSlot_" + currHighlight];
            currHighlight = 1;
        }
        else
        {
            old_go = InitiativeSlot["InitiativeSlot_" + currHighlight];
            currHighlight += 1;
        }
        go = InitiativeSlot["InitiativeSlot_" + currHighlight];

        InputField[] ifOldInit = old_go.GetComponentsInChildren<InputField>();
        InputField[] ifCharInit = go.GetComponentsInChildren<InputField>();

        if (ifOldInit.Length == 0)
        {
            Debug.LogError("There should absolutely be input field children in here");
        }
        else
        {
            for (int x = 0; x < ifOldInit.Length; x++)
            {
                ColorBlock cb = ifOldInit[x].colors;
                cb.normalColor = Color.white;
                ifOldInit[x].colors = cb;
            }
        }

        if (ifCharInit.Length == 0)
        {
            Debug.LogError("There should absolutely be input field children in here");
        }
        else
        {
            for (int x = 0; x < ifCharInit.Length; x++)
            {
                ColorBlock cb = ifCharInit[x].colors;
                cb.normalColor = Color.red;
                ifCharInit[x].colors = cb;
            }
        }
    }
}
 