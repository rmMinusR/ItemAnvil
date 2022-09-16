using System.Collections.Generic;

public interface Inventory
{
    public void AddItem(Item itemType, int quantity);
    public void AddItem(ItemStack newStack);

    public bool TryRemove(Item typeToRemove, int totalToRemove);
    public int RemoveAll(Item typeToRemove);
    
    public int Count(Item itemType);

    public IEnumerator<ReadOnlyItemStack> GetContents();

    public List<ItemStack> CloneContents();
}
