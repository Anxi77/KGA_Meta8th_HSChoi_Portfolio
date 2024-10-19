#include "LobbyScreen.h"

void GridSelect()
{
    ConsoleUtils::SetConsoleCursorPosition(36, 12);
    cout << "START 4 X 4";
    ConsoleUtils::SetConsoleCursorPosition(36, 14);
    cout << "START 5 X 5";
    ConsoleUtils::SetConsoleCursorPosition(40, 16);
    cout << "QUIT";
}

int LobbyScreen::StageSelect()
{
    int input = 0;
    bool SelectTrigger = true;
    int currentY = 12;
    while (SelectTrigger)
    {
        GridSelect();
        SetConsoleCursorPosition(33, currentY);
        switch(currentY)
        {
        case 12:
            SetConsoleCursorPosition(36, 12);
            ColoredPrint("START 4 X 4", ConsoleColor::LightGreen);
            break;
        case 14:
            SetConsoleCursorPosition(36, 14);
            ColoredPrint("START 5 X 5", ConsoleColor::LightGreen);
            break;
        case 16:
            SetConsoleCursorPosition(40, 16);
            ColoredPrint("QUIT", ConsoleColor::LightGreen);
            break;

        }

        input = _getch();
        if (input == 224)
        {
            input = _getch();

            if (input == 72)
            {
                if (currentY == 12)
                    currentY = 16;
                else
                    currentY -= 2;
            }
            else if (input == 80)
            {
                if (currentY == 16)
                    currentY = 12;
                else
                    currentY += 2;
            }
        }
        else if (input == 32)
        {
            SelectTrigger = false;
            return currentY;
        }

    }
}

void LobbyScreen::PrintLogo()
{
    SetColor(ConsoleColor::Yellow);
    SetConsoleCursorPosition(13, 3);
    cout << "       ::::::::       :::::::         :::       :::::::: ";
    SetConsoleCursorPosition(13, 4);
    cout << "     :+:    :+:     :+:   :+:       :+:       :+:    :+: ";
    SetConsoleCursorPosition(13, 5);
    cout << "          +:+      +:+   +:+      +:+ +:+    +:+    +:+ ";
    SetConsoleCursorPosition(13, 6);
    cout << "       +#+        +#+   +:+     +#+  +:+     +#++:++#  ";
    SetConsoleCursorPosition(13, 7);
    cout << "    +#+          +#+   +#+    +#+#+#+#+#+  +#+    +#+  ";
    SetConsoleCursorPosition(13, 8);
    cout << "  #+#           #+#   #+#          #+#    #+#    #+# ";
    SetConsoleCursorPosition(13, 9);
    cout << "##########      #######           ###     ########    ";
    DefaultColor();
}
