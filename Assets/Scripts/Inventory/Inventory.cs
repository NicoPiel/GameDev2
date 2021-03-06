﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace Inventory
{
    public class Inventory
    {
        private ConcurrentDictionary<Item, int> _contents;
        private int _capacityPerSlot;

        public Inventory(int capacityPerSlot)
        {
            _capacityPerSlot = capacityPerSlot;
        }
        
        public Inventory(int capacityPerSlot, Dictionary<Item, int> contents)
        {
            _capacityPerSlot = capacityPerSlot;
            _contents = new ConcurrentDictionary<Item, int>();
        }
        
        /**
         * <summary>Adds a specified amount of an Item to the inventory</summary>
         * <returns>The amount of items that couldn't be added due to capacity restrictions (e.g. 0 if all items were added)</returns>
         */
        public int AddItem(Item type, int amount)
        {
            //Calculate remaining capacity for this type of item
            int capacityOnStack = _capacityPerSlot;
            int storedAmount = GetAmountOf(type);
            capacityOnStack -= storedAmount;
            
            
            //Calculate how many items of amount can't fit into the slot and update amount accordingly
            int overflow = 0;
            if (amount > capacityOnStack)
            {
                overflow = amount - capacityOnStack;
                amount = capacityOnStack - storedAmount;
            }
            
            //If any items can be added, update the content dictionary
            if(amount > 0)
                _contents.AddOrUpdate(type, amount, (oldType, oldAmount) => oldAmount + amount);
            
            return overflow;
        }
        
        /**
         * <summary>Removes the specified amount of an item from this inventory</summary>
         * <returns>The amount of items that were removed. (min = 0 (Item not in inventory), max = amount)</returns>
         */
        public int RemoveItem(Item type, int amount)
        {
            //Check how many items of type exist and return if the Inventory doesn't contain any.
            int currentAmount = GetAmountOf(type);
            if (currentAmount <= 0) return 0;
            
            //If more items than the inventory currently holds are to be removed, decrease the amount of items to remove to the currently stored amount
            if (amount > currentAmount) amount = currentAmount;
            
            //If not all items of a stack are removed, update the stack size, otherwise delete the entire stack
            if (_contents.ContainsKey(type) && amount > 0 && amount < currentAmount)
            {
                _contents.AddOrUpdate(type, currentAmount - amount, (tem, i) => i - amount);
            }else if (amount > 0 && amount >= currentAmount)
            {
                _contents.TryRemove(type, out _);
            }
            
            //Return the amount of items that could be removed
            return amount;
        }

        public int GetAmountOf(Item type)
        {
            _contents.TryGetValue(type, out var amount);
            return amount;
        }
        
    }
}
