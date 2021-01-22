using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Tooltip;

public class BeerSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int slotNumber;
    
    public Sprite glassFull;
    public Sprite glassEmpty;
    public Sprite fill;

    public Tooltip tooltip;
    
    private ItemBeerHolder itemBeerHolder;

    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(WaitUntilBeerHolderInitialized());
    }
    
    public void UpdateSlot()
    {
        // Debug.Log("BeerSlot UI Updating");
        if (itemBeerHolder.GetItemBeerFromSlot(slotNumber) != null)
        {
            // Debug.Log("Slot Filled:");
            ItemBeer item = itemBeerHolder.GetItemBeerFromSlot(slotNumber);
            gameObject.GetComponent<Image>().enabled = true;
            gameObject.GetComponent<Image>().color = item.GetColorModifier();
            gameObject.transform.Find("Glass Image").GetComponent<Image>().sprite = glassFull;
        }
        else
        {
            gameObject.GetComponent<Image>().enabled = false;
            gameObject.transform.Find("Glass Image").GetComponent<Image>().sprite = glassEmpty;
        }
    }

    public void EmptySlot()
    {
        itemBeerHolder.Remove(slotNumber);
    }

    private IEnumerator WaitUntilBeerHolderInitialized()
    {
        yield return new WaitUntil(() => ItemBeerHolder.GetInstance());
        yield return new WaitUntil(ItemBeerHolder.IsInitialized);
        
        if (!itemBeerHolder)
        {
            itemBeerHolder = ItemBeerHolder.GetInstance();
            UpdateSlot();
        }

        GameManager.GetEventHandler().onItemBeerHolderChanged.AddListener(UpdateSlot);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemBeer itemBeer = GetItemBeerOnThisSlot();
        var stringBuilder = new StringBuilder();

        var attributeA = ItemBeer.AttributeToString(itemBeer.GetAttributes()[0]);
        var attributeB = ItemBeer.AttributeToString(itemBeer.GetAttributes()[1]);

        if (!string.IsNullOrWhiteSpace(attributeA))
        {
            stringBuilder.Append($"+ Attribut: {attributeA}\n");
        }
        
        if (!string.IsNullOrWhiteSpace(attributeB))
        {
            stringBuilder.Append($"+ Attribut: {attributeB}\n");
        }

        stringBuilder.Append($"+ Kombination: {itemBeer.GetAttributeCombinationDenominator()}\n");
        
        Tooltip.ShowTooltip_Static(tooltip, stringBuilder.ToString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.HideTooltip_Static(tooltip);
    }

    public ItemBeer GetItemBeerOnThisSlot()
    {
        return itemBeerHolder.GetItemBeerFromSlot(slotNumber);
    }
}
