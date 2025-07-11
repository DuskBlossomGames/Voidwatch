using Menus.Util;
using UnityEngine.EventSystems;

namespace Extensions
{
    public static class EventSystemExtension
    {
        public static PointerEventData GetPointerData(this EventSystem eventSystem)
        {
            return ((CustomInputModule)EventSystem.current.currentInputModule).GetPointerData();
        }
    }
}