﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : Menu {




    public void OpenLocationMenu()
    {
        MenuManager.Instance.LocationMenu.Show(true);

    }
	
}
