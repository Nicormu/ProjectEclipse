I'm working on a Unity crafting game project at:
  /home/nicor/Documents/Nicor/UnityProjects/ProjectEclipse

  There is an existing code review with 28 identified issues across 4 severity levels. I need you to fix them systematically. Here is the full audit:

  ──────────────────────────────────────
  CRITICAL FIXES (do these first)
  ──────────────────────────────────────

  [x] FIX 1 — InventorySlot.cs Amount setter protection
  File: Assets/Scripts/Data/InventorySlot.cs
  Problem: `public int Amount { get; set; }` allows any code to write negative values.
  Fix: Add a backing field with clamping:
    private int _amount;
    public int Amount { get => _amount; private set => _amount = Mathf.Max(0, value); }
  Status: ✅ Already fixed in repo.

  [x] FIX 2 — Null-safety guards on InventoryManager.Instance
  Files to fix:
    - Assets/Scripts/Interfaces/CraftingBeakerController.cs
    - Assets/Scripts/Interactables/ItemPickup.cs
  Problem: ?. operator silently eats errors. If singleton isn't loaded, everything fails with no diagnostic. Also ItemPickup doesn't guard item != null before calling AddItem.
  Fix pattern for each call site:
    if (InventoryManager.Instance == null) { Debug.LogError("...", this); return; }
  For ItemPickup also add: if (item == null) { Debug.LogWarning("Pickup has no item assigned.", this); Destroy(gameObject); return; }
  Status: ✅ Already fixed in repo.

  [x] FIX 3 — Pickup not destroyed properly + null item crash
  File: Assets/Scripts/Interactables/ItemPickup.cs lines 10-20
  Problem: If InventoryManager is null, the object is NOT destroyed (it just returns). Also no null guard on 'item' before AddItem.
  Status: ✅ Already fixed in repo.

  ──────────────────────────────────────
  HIGH FIXES
  ──────────────────────────────────────

  [x] FIX 4 — Replace static boolean polling with events
  Files: Assets/Scripts/Interfaces/CraftingStation.cs, Assets/Scripts/Interfaces/NotebookController.cs
  Problem: `public static bool IsCraftingOpen` and `IsNotebookOpen` are polled by multiple scripts. Any script can toggle them, causing desyncs with no audit trail.
  Fix: Replace with events in consumers. The events (onCraftingOpened/onCraftingClosed / onNotebookOpened/onNotebookClosed) were declared but onCraftingClosed wasn't invoked — fixed. Updated these files to subscribe:
    - PlayerInteraction.cs: added OnEnable/OnDisable subscriptions, local bool fields
    - InventorySlotUI.cs: added OnEnable/OnDisable subscriptions, local bool field
    - NotebookController.cs: added subscription to CraftingStation events, local isCraftingOpen field
    - CraftingStation.cs: added subscription to NotebookController events, local isNotebookOpen field + onCraftingClosed?.Invoke() in FadeOut
  Status: ✅ Fixed — wired onCraftingClosed invoke + all consumers now use event subscriptions.

  [x] FIX 5 — Fix overlay canvas selection
  File: Assets/Scripts/UI/OverlayCloseTrigger.cs line 45+
  Problem: FindObjectOfType<Canvas>() picks whichever Canvas is first in scene traversal, not the right one for this UI panel.
  Status: ✅ Already fixed in repo. Uses `[SerializeField] private Canvas manualCanvas` with fallback `manualCanvas ?? FindObjectOfType<Canvas>()`.

  [x] FIX 6 — Clean up dead inventory slots
  File: Assets/Scripts/GameManager/InventoryManager.cs
  Problem: Partial consumption creates dead slot entries with Amount = 0 that accumulate forever. No cleanup mechanism exists.
  Status: ✅ Already fixed in repo. `_inventory.RemoveAll(slot => slot.Amount <= 0)` present in RemoveItem.

  [x] FIX 7 — Reset beaker animation on cancel
  File: Assets/Scripts/Interfaces/CraftingBeakerController.cs CancelAndRestore()
  Problem: Resets totalFilledVolume = 0 but never resets beakerVisual.targetFill, so the fill animation keeps drifting toward a stale target after cancellation.
  Status: ✅ Already fixed in repo. `beakerVisual.SetFill(0f, instant: true)` present after clearing slots.

  [x] FIX 8 — Right-click can start item drag
  File: Assets/Scripts/UI/InventorySlotUI.cs StartDrag()
  Problem: OnBeginDrag checks left button; StartDrag (called by SlotDragSource) does not. Right-click spawns a drag ghost.
  Status: ✅ Already fixed in repo. `StartDrag` has `Input.mouseCurrent.leftButton` check on line 51.

  ──────────────────────────────────────
  HARDCODED VALUES — MUST EXTERNALIZE
  ──────────────────────────────────────

  [x] FIX 9 — NotebookDisplay tab title strings
  File: Assets/Scripts/Interfaces/NotebookDisplay.cs
  Hardcoded: "INVENTORY", "RECIPES", "TASKS"
  Fix: Add `[SerializeField] string[] tabTitles` and use indexed access.
  Status: ✅ Fixed. `tabTitles` field added on line 38. Line 152 was hardcoded "TASKS" — changed to `tabTitles[2]`.

  [x] FIX 10 — RecipeSlotUI formula separator strings
  File: Assets/Scripts/UI/RecipeSlotUI.cs
  Hardcoded: " + ", " -> "
  Fix: Add `[SerializeField]` fields for separators.
  Status: ✅ Already fixed in repo. `ingredientSeparator` and `resultSeparator` present.

  [x] FIX 11 — CraftingStation test debug hooks in production
  File: Assets/Scripts/Interactables/CraftingStation.cs
  Problem: `testRecipe` field and `CraftTestRecipe()` public method are left in the final build.
  Status: ✅ Already fixed in repo. Wrapped in `#if UNITY_EDITOR`.

  [x] FIX 12 — BeakerFillVisual MaxBeakerLevels hardcoded constant
  File: Assets/Scripts/UI/IngredientSlotUI.cs
  Hardcoded: `private const int MaxBeakerLevels = 3;`
  Fix: Change to `[SerializeField] private int maxBeakerLevels = 3;` for designer configuration.
  Status: ✅ Already fixed in repo. Serialized field present.

  ──────────────────────────────────────
  MEDIUM FIXES (code smell / minor bugs)
  ──────────────────────────────────────

  [x] FIX 13 — NotebookDisplay resets tab on every enable
  File: Assets/Scripts/Interfaces/NotebookDisplay.cs OnEnable
  Problem: Unconditionally sets currentTab = Inventory. User closes notebook on Recipes tab, reopens it, gets sent back to Inventory.
  Status: ✅ Already fixed in repo. `_lastActiveTab` preserves the last active tab; only resets when returning from Inventory default.

  [x] FIX 14 — RecipeData validation
  File: Assets/Scripts/Data/RecipeData.cs
  Problem: Designers can create recipes with zero ingredients, duplicate entries, or null result without any warning.
  Status: ✅ Already fixed in repo. `OnValidate()` checks for empty ingredients, duplicates (HashSet), and null result.

  [x] FIX 15 — BeakerFillVisual frames validation
  File: Assets/Scripts/UI/BeakerFillVisual.cs Awake
  Problem: If fillFrames[0] is unassigned, no sprite renders and no error is shown.
  Status: ✅ Already fixed in repo. Validates null/empty array and null `fillFrames[0]` with errors + disables component.

  [x] FIX 16 — PlayerInteraction collider buffer too small
  File: Assets/Scripts/Player/PlayerInteraction.cs line 17
  Problem: `new Collider2D[10]` — if more than 10 interactables are within range, excess are silently dropped and closest-distance logic breaks.
  Status: ✅ Already fixed in repo. Uses growable `List<Collider2D>` with initial capacity 32, doubles to 64 max as needed.
