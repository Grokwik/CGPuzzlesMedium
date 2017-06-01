using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/*
south = 0
east = 1
north = 2
west = 3
*/

struct Pos
{
    public int x;
    public int y;
}

class Solution
{
    #region Properties
    static string Dir;                  //  Bender's direction
    static int L, C, CurrentDir;
    static List<List<int>> HistoGrid;   //  keep the invert/drunk/direction states for each cell
    static List<string> Grid;           //  Map
    static List<char> Blockers;
    static String[] strDirs;
    static Pos[] PosInc;
    static Pos P, Teleport1, Teleport2;
    static bool Drunk, Invert, Finished;
    static Queue<string> StairwayToHell;
    #endregion

    static void WhereAmI()
    {
        switch (Grid[P.y][P.x])
        {
            case '$': Finished = true;
                      Console.Error.WriteLine("WhereAmI: Cheguei !");
                      break;
            case 'N': CurrentDir = 2;
                      Console.Error.WriteLine("WhereAmI: Let's head North");
                      break;
            case 'S': CurrentDir = 0;
                      Console.Error.WriteLine("WhereAmI: Let's head South");
                      break;
            case 'W': CurrentDir = 3;
                      Console.Error.WriteLine("WhereAmI: Let's head West");
                      break;
            case 'E': CurrentDir = 1;
                      Console.Error.WriteLine("WhereAmI: Let's head East");
                      break;
            case 'B': Drunk = !Drunk;
                      UpdateBlockers();
                      Console.Error.WriteLine("WhereAmI: Yeeah, beer time !");
                      break;
            case 'I': Invert = !Invert;
                      Console.Error.WriteLine("WhereAmI: White rabbit mode");
                      break;
            case 'T': if (P.x == Teleport1.x && P.y == Teleport1.y)
                      {
                          P.x = Teleport2.x;
                          P.y = Teleport2.y;
                      }
                      else
                      {
                          P.x = Teleport1.x;
                          P.y = Teleport1.y;
                      }
                      Console.Error.WriteLine("WhereAmI: Scotty !! Teleport !!");
                      break;
            case 'X': Console.Error.WriteLine("WhereAmI: Mode passe muraille");
                      Console.Error.WriteLine(Grid[P.y]);
                      var newLine = Grid[P.y].Substring(0, P.x);
                      newLine += " ";
                      newLine += Grid[P.y].Substring(P.x+1);
                      Grid[P.y] = newLine;
                      Console.Error.WriteLine(Grid[P.y]);
                      ReinitHistoGrid();
                      break;
        }
    }

    static void WhereDoIGo()
    {
        char[] cells = new char[4];
        cells[0] = Grid[P.y + 1][P.x]; // south
        cells[1] = Grid[P.y][P.x + 1]; // east
        cells[2] = Grid[P.y - 1][P.x]; // north
        cells[3] = Grid[P.y][P.x - 1]; // west

        if (!Blockers.Contains(cells[CurrentDir]))
        {
            Dir = strDirs[CurrentDir];
            StairwayToHell.Enqueue(Dir);
            P.x += PosInc[CurrentDir].x;
            P.y += PosInc[CurrentDir].y;
            return;
        }
        
        int scannedDir;
        scannedDir = (Invert) ? 3 : 0;
        while (scannedDir<4 && scannedDir>=0)
        {
            if (!Blockers.Contains(cells[scannedDir]))
            {
                Dir = strDirs[scannedDir];
                StairwayToHell.Enqueue(Dir);
                CurrentDir = scannedDir;
                P.x += PosInc[scannedDir].x;
                P.y += PosInc[scannedDir].y;
                break;
            }
            if (Invert)
                scannedDir--;
            else
                scannedDir++;
        }
    }

    static void UpdateBlockers()
    {
        Blockers = new List<char>() { '#' };
        if (!Drunk)
        {
            Blockers.Add('X');
        }
    }

    static void Main(string[] args)
    {
        #region Init
        Grid = new List<string>();
        StairwayToHell = new Queue<string>();
        var inputs = Console.ReadLine().Split(' ');
        L = int.Parse(inputs[0]);
        C = int.Parse(inputs[1]);
        UpdateBlockers();
        PosInc = new Pos[4];
        PosInc[0].y = 1;  // south
        PosInc[1].x = 1;  // east
        PosInc[2].y = -1; // north
        PosInc[3].x = -1; // west

        strDirs = new String[4];
        strDirs[0] = "SOUTH";
        strDirs[1] = "EAST";
        strDirs[2] = "NORTH";
        strDirs[3] = "WEST";

        Drunk = false;
        Invert = false;
        Dir = "SOUTH";
        Finished = false;

        for (int l = 0; l < L; l++)
        {
            string row = Console.ReadLine();
            Grid.Add(row);

            //  Look for the '@'
            var aropos = row.IndexOf("@");
            if (aropos != -1)
            {
                P.x = aropos;
                P.y = l;
            }

            //  Look for the teleporters
            var Tidx = row.IndexOf("T");
            if (Tidx != -1)
            {
                if (Teleport1.x == 0)
                {
                    Teleport1.x = Tidx;
                    Teleport1.y = l;
                }
                else
                {
                    Teleport2.x = Tidx;
                    Teleport2.y = l;
                }
            }
        }
        CurrentDir = 0; // 0 = SOUTH

        ReinitHistoGrid();
        #endregion

        DisplayGrid();

        while (!Finished)
        {
            WhereDoIGo();
            WhereAmI();
            if (!RegisterHisto())
            {
                Console.WriteLine("LOOP");
                Finished = true;
                StairwayToHell.Clear();
            }
        }
        while (0 != StairwayToHell.Count)
        {
            Dir = StairwayToHell.Dequeue();
            Console.WriteLine(Dir);
        }
    }

    static void DisplayGrid()
    {
        Console.Error.WriteLine("Initial pos=" + P.x + "," + P.y);
        for (int l = 0; l < L; l++)
        {
            Console.Error.WriteLine(Grid[l]);
        }
    }

    static void ReinitHistoGrid()
    {
        HistoGrid = new List<List<int>>();
        for (var l = 0; l < L; l++)
        {
            var line = new List<int>();
            for (var i = 0; i < C; i++)
                line.Add(0);
            HistoGrid.Add(line);
        }
    }

    static bool RegisterHisto()
    {
        int state = 0;
        var filter = 0;
        if (Dir == "WEST")
        {
            filter = 7;             //  000 000 000 111
            state += 1;             //  000 000 000 001
            if (Invert) state += 2; //  000 000 000 010
            if (Drunk) state += 4;  //  000 000 000 100
        }
        else if (Dir == "EAST")
        {
            filter = 56;            //  000 000 111 000
            state += 8;             //  000 000 001 000
            if (Invert) state += 16;//  000 000 010 000
            if (Drunk) state += 32; //  000 000 100 000
        }
        else if (Dir == "SOUTH")
        {
            filter = 448;            //  000 111 000 000
            state += 64;             //  000 001 000 000
            if (Invert) state += 128;//  000 010 000 000
            if (Drunk) state += 256; //  000 100 000 000
        }
        else if (Dir == "NORTH")
        {
            filter = 3584;              //  111 000 000 000
            state += 512;               //  001 000 000 000
            if (Invert) state += 1024;  //  010 000 000 000
            if (Drunk) state += 2048;   //  100 000 000 000
        }
        if ((HistoGrid[P.y][P.x] & filter) == state)
            return false;

        HistoGrid[P.y][P.x] |= state;
        return true;
    }
}
