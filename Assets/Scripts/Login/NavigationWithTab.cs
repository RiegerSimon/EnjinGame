using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
public class NavigationWithTab : MonoBehaviour
{
    EventSystem system;
 
    void Start()
    {
        system = EventSystem.current;// EventSystemManager.currentSystem;
     
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
         
            if (next != null)
            {
             
                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret
             
                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            } 
            //Here is the navigating back to the 1 Object:
            else
            {
                next = Selectable.allSelectables[0];
                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
            
         
        }
    }
}