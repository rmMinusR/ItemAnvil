
# Item Anvil: modular RPG-style items

[![Tests](https://github.com/rmMinusR/ItemAnvil/actions/workflows/unit-test.yml/badge.svg)](https://github.com/rmMinusR/ItemAnvil/actions/workflows/unit-test.yml)

**Item Anvil** is a flexible and highly customizable RPG-style item ecosystem. Designer-friendly, with a drag-and-drop workflow built on ScriptableObjects. Easy-to-understand expressive scripting syntax.

| In player | In editor |
| --------- | --------- |
| ![Tea Shop example](https://raw.githubusercontent.com/rmMinusR/ItemAnvil/assets/screenshots/tea-demo-1.png) | ![Metallurgy editors](https://raw.githubusercontent.com/rmMinusR/ItemAnvil/assets/screenshots/metallurgy-editor-4.png) |

Just like GameObjects have component scripts, items gain functionality by adding properties. These can affect all items of that type (like tuning the damage of a sword) or only one item (such as enchantments).

## Getting started
### Installing
 - Option 1: Use the Package Manager to [add by git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html): `https://github.com/rmMinusR/ItemAnvil.git` Recommended for most users.
 - Option 2: Git clone to your Packages folder. Recommended for contributors.
 - ~~Option 3: [Download from Unity Asset Store](https://u3d.as/33kM).~~ Currently awaiting review, not available yet. Will likely be slightly out of date due to the review process.
 - Option 4: Download and place in your Assets folder. Not recommended, as the samples folder will be invisible.

### Examples
 - **Tea Shop** ([try it!](https://rmMinusR.github.io/ItemAnvil/demos/TeaShop/)): Shows a basic setup for shops, similar to an NPC or player. Similar to traders in *Divinity: Original Sin 2*.
 - **Metallurgy** ([try it!](https://rmMinusR.github.io/ItemAnvil/demos/Metallurgy/)): A crafting minigame! Buy ores => smelt => alloy => sell for profit.

## Features
 - [x] Pre-built UI
	 - [x] Tooltips
	 - [x] Sort button
	 - [x] Model-View-Controller architecture
 - [x] Crafting recipes
 - [x] Items use assigned icon in Project browser and Asset Picker
 - [x] Control over max stack size
 - [x] Generic inventories:
	 - [x] Auto-expanding
	 - [x] Fixed-slot
 - [x] Fully serializable using Unity's built-in tools
 - [x] Per-instance properties (enchantments, temperature, aesthetic customizations, etc)
 - [x] Fuzzy filters and advanced querying
 - [x] Unit testing - actively improving

### Roadmap
 - [ ] Loot tables - Planned for 0.6-0.7
 - [ ] Better player-facing inventory
	 - [ ] Drag-and-drop rearranging - Planned for 0.6
	 - [ ] Drag-and-drop moving between inventories - Planned for 0.6
	 - [ ] Searching/highlighting - Partially supported through Filters, needs proper UI
	 - [ ] Gamepad support
 - [ ] Event hooks
 - [ ] Combat example
 - [ ] Dynamic weaponsmithing example
 - [ ] Random stats ("+ 10 to 20 Armor")
 - [ ] Inventory properties
 - [ ] Fixed-volume inventory

Have a suggestion? [File a feature request!](https://github.com/rmMinusR/ItemAnvil/issues/new)

## Usage

Creating content is largely done through the Create Assets menu, filling out the relevant fields, and dragging and dropping.

For scripting, the in-code documentation should be your first source, but here's a general overview:
 - **Item**: The most fundamental building block, a concept: jug of water, sword, shield.
 - **ItemProperty**: Adds functionality to an item: a sword is now a flaming sword; a shield blocks 75% damage.
 - **ItemStack**: An instance of an item, made real. Can stack, if the item definition allows it.
 - **ItemInstanceProperty**: A unique characteristic of *this* instance of the item, applied to an ItemStack.
 - **Inventory**: An abstract class describing the most generic inventory possible. To attach one to a GameObject, consider **InventoryHolder**.
	 - **CondensingInventory**: Expands and contracts as needed to fit all necessary items.
	 - **FixedSlotInventory**: Has a fixed number of slots. Attempting to add items while full will fail and raise an error.
 - **CraftingRecipe**: Turn one pile of items into another pile of items, within the same inventory.
 - **Transaction**: Represents a trade, or moving items between inventories.

### Code example

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