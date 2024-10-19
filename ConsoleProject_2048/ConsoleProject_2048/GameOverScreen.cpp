#include "GameOverScreen.h"


void GameOverScreen::PrintGameover()
{
    SetColor(ConsoleColor::Aqua);
    SetConsoleCursorPosition(3, 3);
    cout << ":::   :::  ::::::::  :::    :::  :::         ::::::::   ::::::::  :::::::::: ";
    SetConsoleCursorPosition(3, 4);
    cout << ":+:   :+: :+:    :+: :+:    :+:  :+:        :+:    :+: :+:    :+: :+:   ";
    SetConsoleCursorPosition(3, 5);
    cout << " +:+ +:+  +:+    +:+ +:+    +:+  +:+        +:+    +:+ +:+        +:+ ";
    SetConsoleCursorPosition(3, 6);
    cout << "  +#++:   +#+    +:+ +#+    +:+  +#+        +#+    +:+ +#++:++#++ +#++:++#  ";
    SetConsoleCursorPosition(3, 7);
    cout << "   +#+    +#+    +#+ +#+    +#+  +#+        +#+    +#+        +#+ +#+        ";
    SetConsoleCursorPosition(3, 8);
    cout << "   #+#    #+#    #+# #+#    #+#  #+#        #+#    #+# #+#    #+# #+#       ";
    SetConsoleCursorPosition(3, 9);
    cout << "   ###     ########   ########   ##########  ########   ########  ########## ";
    DefaultColor();
}

int GameOverScreen::ContinueSelect(int topscore)
{
    int input = 0;
    bool SelectTrigger = true;
    int currentY = 16;
    while (SelectTrigger)
    {
        SetConsoleCursorPosition(31, 14);
        cout << "Top Score : " << topscore;
        SetConsoleCursorPosition(36, 16);
        cout << "Continue";
        SetConsoleCursorPosition(36, 18);
        cout << "QUIT";


        SetConsoleCursorPosition(36, currentY);
        switch(currentY)
        {
        case 16:
            ColoredPrint("Continue", ConsoleColor::LightGreen);
            break;
        case 18:
            ColoredPrint("QUIT", ConsoleColor::LightRed);
            break;
        }

        input = _getch();
        if (input == 224)
        {
            input = _getch();

            if (input == 72)
            {
                if (currentY == 16) 
                {
                    currentY = 18;
                }
                else 
                {
                    currentY -= 2;
                }
            }
            else if (input == 80)
            {
                if (currentY == 18) 
                {
                    currentY = 16;
                }
                else 
                {
                    currentY += 2;
                }
            }
        }
        else if (input == 32)
        {
            SelectTrigger = false;
            return currentY;
        }

    }
}


