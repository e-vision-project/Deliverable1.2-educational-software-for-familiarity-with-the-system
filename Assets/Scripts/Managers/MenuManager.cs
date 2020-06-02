using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager
{
    #region Singleton

    private static MenuManager s_Instance;

    public static MenuManager Instance
    {
        get
        {
            if (s_Instance == null) s_Instance = new MenuManager();
            return s_Instance;
        }
    }

    #endregion


    public enum MenuState
    {
        MainMenu,
        LocationMenu,
        Count
    }

    private MenuState m_CurrentMenuState = MenuState.Count;


    public MainMenu MainMenu;
    public LocationMenu LocationMenu;
    public Dictionary<string, Menu> MenuDictionary = new Dictionary<string, Menu>();

    //-----------------------------------------------------------------


    //Requires all menus to be present and instantiated
    private MenuManager()
    {
        Menu[] menus = Object.FindObjectsOfType<Menu>();
        for (int i = 0; i < menus.Length; i++)
        {
            MenuDictionary.Add(menus[i].name, menus[i]); 
            menus[i].Show(false);
            Debug.Log(menus[i].name);
        }

    }

    public void Init()
    {
        MainMenu = (MainMenu)MenuDictionary[MenuState.MainMenu.ToString()];

        SetState(MenuState.MainMenu);
    }

    public void Update() { 

    }

    public void SetState(MenuState state)
    {
        m_CurrentMenuState = state;
        Show(state.ToString()); //NOTE: To use this like this it requires the enum to have the same name as in the dictionary
    }

    private void Show(string menuKey)
    {
        if (MenuDictionary.ContainsKey(menuKey))
        {
            Menu menu = MenuDictionary[menuKey];
            Show(menu);
        }
        else
        {
            Debug.LogError("Key: " + menuKey + " doesn't exist");
        }
    }

    private void Show(Menu menu)
    {
        Menu.CurrentMenu.Show(false);
        Menu.CurrentMenu = menu;
        Menu.CurrentMenu.Show(true);
    }
}
