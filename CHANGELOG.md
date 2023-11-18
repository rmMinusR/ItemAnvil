# 0.6.0

 * Overhauled UI
	 + Added drag-and-drop for ingame UI
	 + Added controller support
 * Reworked item storage in inventories
	 + Added InventorySlot
	 + Added SlotProperty
	 * Inventories will automatically update to use slots instead of stacks
 + Added StandardInventory
	 + Added InventoryProperty
	 + Added AutoExpand
	 - Deprecated CondensingInventory. Use AutoExpand instead
	 - Deprecated FixedSlotInventory. Use the default behavior of StandardInventory instead
 + Added hooks
	 + Added HookPoint to help facilitate hooks
 + Added a number of convenience function overloads to Inventory
 + Added a lot of tests
 + Added this changelog. **Note that changelog for previous versions may be incomplete or inaccurate.**
 
 # 0.5.0

 + Added prebuilt inventory UI prefabs
 + Added sorting
 + Added Filter system
 * Crafting recipe abstraction; added simple/fuzzy crafting recipes
 * Namespaced all types
 + Added tests

# 0.4.2

 + Added TypeSwitcher attribute
 + Added tests

# 0.4.1

 * Moved tooltips to be part of Item Anvil instead of a separate package
 + Added tests

# 0.4.0

 + Added Tea Shop example
 + Started writing tests
 + Added unit tests, CI/CD

# 0.3.1

 * Inventory abstraction
	 + Added FixedSlotInventory
	 + Added CondensingInventory
 + Added readme

# 0.3.0

 + Added Metallurgy sample

# 0.2.2

 + Added crafting recipe
 + Items now display as their set icon in Project browser

# 0.2.1

 + Added Max Stack Size
 * Broke existing ItemStack instances

# 0.2.0

 * Reworked item functionality: now defined through component-like properties instead of inheritance hierarchy
