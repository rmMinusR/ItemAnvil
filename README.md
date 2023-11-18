
# Item Anvil: modular RPG-style items

[![Tests](https://github.com/rmMinusR/ItemAnvil/actions/workflows/unit-test.yml/badge.svg)](https://github.com/rmMinusR/ItemAnvil/actions/workflows/unit-test.yml)

**Item Anvil** is an intuitive, highly customizable, rigorously tested RPG-style item ecosystem. It is designer-friendly, with a drag-and-drop workflow built on ScriptableObjects and adding properties that change how components function. Scripting new behavior is equally easy and expressive.

| In player | In editor |
| --------- | --------- |
| ![Tea Shop example](https://raw.githubusercontent.com/rmMinusR/ItemAnvil/assets/screenshots/tea-demo-1.png) | ![Editor view of the Metallurgy example](https://raw.githubusercontent.com/rmMinusR/ItemAnvil/assets/screenshots/metallurgy-editor-4.png) |

Items gain functionality by adding properties, much like how GameObjects have component scripts. These can affect all items of that type (like tuning the damage of a sword) or only one item (such as enchantments or durability). Inventories and their slots can also have properties, such as expanding automatically, or only allowing certain items inside (armor/gear slots).

## Getting started
### Installing
Option 1: Use the Package Manager to [add this repo's git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html): `https://github.com/rmMinusR/ItemAnvil.git` Recommended for most users.
Option 2: Clone to your Packages folder. Recommended if you intend to contribute.
Option 3: [Download from Unity Asset Store](https://u3d.as/33kM). Might slightly out of date due to the review process.

### Dependencies
 - InputSystem
 - TextMeshPro

### Examples
 - **Tea Shop** ([try it!](https://rmMinusR.github.io/ItemAnvil/demos/TeaShop/)): Shows a basic setup for shops, similar to an NPC or player. Similar to traders in *Divinity: Original Sin 2*.
 - **Metallurgy** ([try it!](https://rmMinusR.github.io/ItemAnvil/demos/Metallurgy/)): A crafting minigame! Buy ores => smelt => alloy => sell for profit.

## Features
 - [x] Pre-built ingame UI
	 - [x] Drag-and-drop
	 - [x] Gamepad support
	 - [x] Tooltips
	 - [x] Sort button
	 - [x] Model-View-Controller architecture
 - [x] Crafting recipes and transactions
 - [x] Items use assigned icon in Project browser and Asset Picker
 - [x] Properties attachable to items, item instances, inventories, and inventory slots. Built-in properties include:
	 - [x] Control over item's max stack size
	 - [x] Making inventory automatically expand/condense
	 - [x] Slots that only allow certain items to be placed in them
	 - [ ] Anything else you can dream up! (enchantments, temperature, quality level, aesthetic customizations)
 - [x] Fully serializable using Unity's built-in tools
 - [x] Fuzzy filters and advanced querying
 - [x] Event hooks (when item added/removed, overflow handling, when sorted, etc)
 - [x] Custom inventory implementation support
 - [x] Unit tested (>80% coverage on core features as of v0.6)

### Roadmap
 - [ ] Loot tables - Planned for 0.7
	 - [ ] Probability analysis tools
 - [ ] Better player-facing inventory
	 - [ ] Searching/highlighting - supported in code through Filters, needs proper UI
 - [ ] Combat example
 - [ ] Dynamic weaponsmithing example
 - [ ] Default instance properties that items automatically are spawned with
	 - [ ] Random stats ("+ 10 to 20 Armor")
 - [ ] Fixed-volume inventory property

Have a suggestion? [File a feature request!](https://github.com/rmMinusR/ItemAnvil/issues/new)

## Usage

Creating content is largely done through the Create Assets menu, filling out the relevant fields, and dragging and dropping.

For scripting, the in-code documentation should be your first source, but here's a general overview:
 - **Item**: The most fundamental building block, a concept: jug of water, sword, shield. **ItemProperties** can be attached to add functionality: a sword is now a flaming sword; a shield blocks 75% damage.
 - **ItemStack**: An instance of an item, made real. Can stack, if the item definition allows it. **ItemInstanceProperties** represent a unique characteristic of *this* instance of the item, like durability or color.
 - **Inventory**: A generic inventory description, which can be attached to a GameObject using **InventoryHolder**. By default, the built-in **StandardInventory** is fixed in size, but can be customized through **InventoryProperties** such as AutoExpand.
 - **InventorySlot**: A slot in an inventory, which can be customized through **SlotProperties** such as FilterSlotContents.
 - **ItemFilter**: A serializable filter used for selecting and removing items, sorting, and crafting recipes.
 - **CraftingRecipe**: Turn a set of input items into another set of items, within the same inventory. The Simple variant works on just item types, while the Fuzzy version uses the more advanced Filter system, allowing for recipe requirements like "any gemstone" or "anything with the Flaming enchantment".
 - **Transaction**: Represents a trade, or moving items between inventories. Note that depending on your game, shops might be considered crafting stations instead.

### Code examples

```csharp
public class Temperature : ItemInstanceProperty
{
    public const float ROOM_TEMP = 70;
    
    [Min(0)] public float temperature = ROOM_TEMP;
    [Min(0)] public float heatLoss = 1;

    public override void Tick()
    {
        temperature = Mathf.MoveTowards(temperature, ROOM_TEMP, heatLoss);
    }
}
```

```csharp
public class CurseItemsInInventory : InventoryProperty
{
    protected override void InstallHooks(Inventory inventory)
    {
        inventory.Hooks.TryAdd.InsertHook(CurseOnAdd, 0);
    }

    protected override void UninstallHooks(Inventory inventory)
    {
        inventory.Hooks.TryAdd.RemoveHook(CurseOnAdd);
    }

    private QueryEventResult CurseOnAdd(ItemStack finalToAdd, ReadOnlyItemStack original, object cause)
    {
        if (!finalToAdd.instanceProperties.Contains<Cursed>()) finalToAdd.instanceProperties.Add<Cursed>();
        return QueryEventResult.Allow;
    }
}
```