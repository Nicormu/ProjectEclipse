using UnityEngine;

public interface INotebookTabContent
{
    GameObject SlotPrefab { get; }
    int ItemCount { get; }
    void Refresh();
    /// <returns>False if this index has nothing to show (slotInstance will be destroyed).</returns>
    bool PopulateSlot(int index, GameObject slotInstance);
}
