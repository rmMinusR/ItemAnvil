using System;

[Serializable]
public class ItemStackFilter
{
    public int quantity;
    public ItemFilter typeFilter;

    public ItemStackFilter Clone()
    {
        return new ItemStackFilter()
        {
            quantity = quantity,
            typeFilter = typeFilter
        };
    }
}
