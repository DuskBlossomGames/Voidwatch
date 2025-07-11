using UnityEngine.EventSystems;

namespace Menus.Util
{
    public class CustomInputModule : StandaloneInputModule
    {
        public PointerEventData GetPointerData() => GetLastPointerEventData(-1); // lmb
    }
}