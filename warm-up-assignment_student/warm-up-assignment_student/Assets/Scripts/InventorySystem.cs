using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class InventorySystem : MonoBehaviour
{
    public Dictionary<string, Item> inventory = new Dictionary<string, Item>();
    public enum Rarities { Common, Legendary }
    [SerializeField] private string itemName;
    [SerializeField] private Rarities rarity;
    public struct Item
    {
        public Rarities Rarity;
        public int Amount;
        public string Name;

        // Proper Constructor
        public Item(string name, Rarities rarity = Rarities.Common, int amount = 1)
        {
            Rarity = rarity;
            Amount = amount;
            Name = name;
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void AddItem()
    {
        Debug.Log("-----------------");

        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("No item name was given");
            return;
        }

        string name = rarity + itemName;

        if (inventory.ContainsKey(name))
        {
            Item item = inventory[name];

            if (item.Rarity != Rarities.Legendary)
            {
                item.Amount++; // Modify struct copy
                inventory[name] = item; // Reassign modified copy
                DisplayItems();
            }

            else
            {
                Debug.LogWarning("Only 1 legendary item allowed!");
            }
        }

        else
        {
            Item newItem = new Item(itemName, rarity, 1);
            inventory.Add(name, newItem);
            DisplayItems();
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void RemoveItem()
    {
        Debug.Log("-----------------");

        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("No item name was given");
            return;
        }

        string name = rarity + itemName;

        if (inventory.ContainsKey(name))
        {
            Item item = inventory[name];
            item.Amount--;

            if (item.Amount <= 0)
            {
                inventory.Remove(name);
            }
            else
            {
                inventory[name] = item;
            }

            DisplayItems();
        }

        else
        {
            Debug.LogWarning("No Item was found in the inventory with this name!");
        }
    }
    private void DisplayItems()
    {
        if (inventory.Count == 0)
        {
            Debug.Log("No items in inventory!");
            return;
        }

        foreach (var entry in inventory)
        {
            Debug.Log($"{entry.Value.Amount} {entry.Value.Rarity} {entry.Value.Name}");
        }
    }
}